namespace WinSetup;

public static class Commands
{
    public static int Status()
    {
        Console.WriteLine($"win-setup {Update.DisplayVersion}");
        var host = Checks.Collect();
        if (host is null)
        {
            Console.WriteLine("host checks unavailable (status reads live state on Windows)");
        }
        else
        {
            Console.WriteLine($"os         {host.Os} (build {host.Build})");
            Console.WriteLine($"elevated   {host.Admin}");
            Console.WriteLine($"secureboot {Describe(host.SecureBoot)}");
            Console.WriteLine($"tpm        {DescribeTpm(host)}");
            Console.WriteLine($"bitlocker  {host.BitLockerStatus} / protection {host.BitLockerProtection}");
            Console.WriteLine($"winget     {host.Winget}");
            Console.WriteLine($"chezmoi    {host.Chezmoi}");
            Console.WriteLine($"sshd       {host.Sshd}");
        }

        var missing = 0;
        if (OperatingSystem.IsWindows())
        {
            foreach (var package in Packages.All)
            {
                var result = Packages.Classify(Runner.Run(Paths.Winget, package.DetectArgs).ExitCode);
                var present = Packages.IsPresent(result);
                if (!present)
                {
                    missing++;
                }

                Console.WriteLine($"{(present ? "ok  " : "miss")} {package.Name} ({package.Id})");
            }
        }

        var drifted = 0;
        foreach (var setting in Setting.All)
        {
            var state = setting.Check();
            if (state != SettingState.Matches)
            {
                drifted++;
            }

            Console.WriteLine($"{state,-8} {setting.Name}: {setting.Why}");
        }

        foreach (var item in PowerItem.All)
        {
            var ac = item.ReadAc();
            if (ac is not null && ac != item.Ac)
            {
                drifted++;
            }

            var label = ac is null ? "unknown" : ac == item.Ac ? "ok" : "drift";
            Console.WriteLine($"{label,-8} {item.Why}");
        }

        return missing + drifted == 0 ? 0 : 1;
    }

    public static int Apply()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("apply must run on Windows");
            return 2;
        }

        var host = Checks.Collect();
        if (host is null)
        {
            Console.WriteLine("failed to collect host state");
            return 2;
        }

        if (!host.Admin)
        {
            Console.WriteLine("apply requires an elevated PowerShell");
            return 2;
        }

        var failures = new List<string>();
        var taskbarChanged = false;
        var colorStoreChanged = false;
        var rebootRequired = false;

        Console.WriteLine("== settings ==");
        foreach (var setting in Setting.All)
        {
            try
            {
                if (setting.Check() == SettingState.Matches)
                {
                    Console.WriteLine($"ok    {setting.Why}");
                    continue;
                }

                setting.Write();
                if (setting.Check() == SettingState.Matches)
                {
                    Console.WriteLine($"set   {setting.Why}");
                    if (setting.Key.Contains("StuckRects3", StringComparison.OrdinalIgnoreCase)
                        || setting.Name.Contains("Taskbar", StringComparison.OrdinalIgnoreCase)
                        || setting.Name.Equals("SearchboxTaskbarMode", StringComparison.Ordinal))
                    {
                        taskbarChanged = true;
                    }

                    if (setting.Key.Contains("CloudStore", StringComparison.OrdinalIgnoreCase))
                    {
                        colorStoreChanged = true;
                    }
                }
                else
                {
                    failures.Add($"setting: {setting.Why}");
                    Console.WriteLine($"FAIL  {setting.Why}");
                }
            }
            catch (Exception exception)
            {
                failures.Add($"setting: {setting.Why} ({exception.Message})");
                Console.WriteLine($"FAIL  {setting.Why} ({exception.Message})");
            }
        }

        if (colorStoreChanged)
        {
            Notify.SettingsChanged();
        }

        Console.WriteLine("== power ==");
        foreach (var item in PowerItem.All)
        {
            try
            {
                if (item.ReadAc() == item.Ac)
                {
                    Console.WriteLine($"ok    {item.Why}");
                    continue;
                }

                var result = item.Write();
                if (!result.Ok)
                {
                    failures.Add($"power: {item.Why} ({result.Hex})");
                    Console.WriteLine($"FAIL  {item.Why} ({result.Hex})");
                    continue;
                }

                // ponytail: hidden power settings are absent from powercfg /query; the set is idempotent, so no re-read is not a failure.
                var after = item.ReadAc();
                if (after == item.Ac || after is null)
                {
                    Console.WriteLine($"set   {item.Why}");
                }
                else
                {
                    failures.Add($"power: {item.Why}");
                    Console.WriteLine($"FAIL  {item.Why}");
                }
            }
            catch (Exception exception)
            {
                failures.Add($"power: {item.Why} ({exception.Message})");
                Console.WriteLine($"FAIL  {item.Why} ({exception.Message})");
            }
        }

        Console.WriteLine("== packages ==");
        foreach (var package in Packages.All)
        {
            try
            {
                if (Packages.IsPresent(Packages.Classify(Runner.Run(Paths.Winget, package.DetectArgs).ExitCode)))
                {
                    Console.WriteLine($"ok    {package.Name}");
                    continue;
                }

                var install = Runner.Run(Paths.Winget, package.InstallArgs());
                var result = Packages.Classify(install.ExitCode);
                if (Packages.IsPresent(result))
                {
                    Console.WriteLine($"inst  {package.Name}");
                    if (result == WingetResult.RebootRequired)
                    {
                        rebootRequired = true;
                        Console.WriteLine($"      reboot needed to finish {package.Name}");
                    }
                }
                else
                {
                    failures.Add($"package: {package.Name} ({install.Hex})");
                    Console.WriteLine($"FAIL  {package.Name} ({install.Hex})");
                    if (!string.IsNullOrWhiteSpace(install.StdErr))
                    {
                        Console.WriteLine($"      {install.StdErr.Trim()}");
                    }
                }
            }
            catch (Exception exception)
            {
                failures.Add($"package: {package.Name} ({exception.Message})");
                Console.WriteLine($"FAIL  {package.Name} ({exception.Message})");
            }
        }

        Console.WriteLine("== bitlocker ==");
        if (!host.TpmReady)
        {
            failures.Add("bitlocker: TPM not ready");
            Console.WriteLine("FAIL  TPM not ready");
        }
        else if (host.BitLockerProtected && File.Exists(BitLocker.KeyFile))
        {
            Console.WriteLine("ok    protected, recovery key file present");
        }
        else
        {
            var result = BitLocker.Enable();
            var parts = result.StdOut.Trim().Split('|');
            var volume = parts.Length > 0 ? parts[0].Trim() : string.Empty;
            var protection = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            var protecting = protection.Equals("On", StringComparison.OrdinalIgnoreCase)
                || volume.Equals("EncryptionInProgress", StringComparison.OrdinalIgnoreCase);
            if (result.Ok && protecting)
            {
                var state = protection.Equals("On", StringComparison.OrdinalIgnoreCase)
                    ? $"protection {protection}"
                    : $"{volume}, protection {protection}";
                Console.WriteLine($"set   {state}");
            }
            else
            {
                failures.Add($"bitlocker ({result.Hex})");
                Console.WriteLine($"FAIL  bitlocker ({result.Hex}) {result.StdErr.Trim()}");
            }
        }

        var keyWritten = File.Exists(BitLocker.KeyFile);

        Console.WriteLine("== chezmoi ==");
        Paths.RefreshPath();
        if (Chezmoi.IsInitialized())
        {
            Console.WriteLine("applying chezmoi configuration");
            if (Chezmoi.Apply() != 0)
            {
                failures.Add("chezmoi apply");
            }
        }
        else
        {
            Console.WriteLine($"todo  run: {Chezmoi.InitHint}");
        }

        if (File.Exists(Setting.WallpaperPath))
        {
            Notify.SetWallpaper(Setting.WallpaperPath);
        }

        Console.WriteLine();
        Console.WriteLine(failures.Count == 0 ? "summary: all steps ok" : $"summary: {failures.Count} failed");
        foreach (var failure in failures)
        {
            Console.WriteLine($"  {failure}");
        }

        if (keyWritten)
        {
            Console.WriteLine();
            Console.WriteLine($"BitLocker recovery key saved to: {BitLocker.KeyFile}");
            Console.WriteLine("Store it in 1Password from another device now.");
        }

        if (taskbarChanged)
        {
            Console.WriteLine();
            Console.WriteLine("Restart Explorer or reboot for the taskbar changes to appear.");
        }

        if (rebootRequired)
        {
            Console.WriteLine();
            Console.WriteLine("A reboot is required to finish one or more package installs.");
        }

        return failures.Count == 0 ? 0 : 1;
    }

    private static string Describe(bool? value) => value switch
    {
        true => "enabled",
        false => "disabled",
        null => "unknown",
    };

    private static string DescribeTpm(HostStatus host) =>
        !host.TpmPresent ? "absent" : host.TpmReady ? "ready" : "present, not ready";
}
