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
                try
                {
                    var present = Packages.Detect(package);
                    if (!present)
                    {
                        missing++;
                    }

                    Console.WriteLine($"{(present ? "ok  " : "miss")} {package.Name} ({package.Id})");
                }
                catch (InvalidOperationException exception)
                {
                    missing++;
                    Console.WriteLine($"FAIL {package.Name}: {exception.Message}");
                }
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
        var hibernateChanged = false;
        var rebootRequired = false;

        Console.WriteLine("== settings ==");
        var unchanged = 0;
        foreach (var setting in Setting.All)
        {
            try
            {
                if (setting.Check() == SettingState.Matches)
                {
                    unchanged++;
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

                    if (setting.Name.Equals("HibernateEnabled", StringComparison.Ordinal))
                    {
                        hibernateChanged = true;
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

        Console.WriteLine($"ok    {unchanged} settings already correct");

        if (colorStoreChanged)
        {
            Notify.SettingsChanged();
        }

        if (hibernateChanged)
        {
            Power.DisableHibernate();
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
        Console.WriteLine("      checking installed packages...");
        var present = new List<string>();
        var missing = new List<Package>();
        var checkedCount = 0;
        foreach (var package in Packages.All)
        {
            checkedCount++;
            if (!Console.IsErrorRedirected)
            {
                Console.Error.Write(("\r      " + $"{checkedCount}/{Packages.All.Length} {package.Name}").PadRight(79));
            }

            try
            {
                if (Packages.Detect(package))
                {
                    present.Add(package.Name);
                }
                else
                {
                    missing.Add(package);
                }
            }
            catch (Exception exception)
            {
                failures.Add($"package: {package.Name} ({exception.Message})");
                Console.WriteLine($"FAIL  {package.Name} ({exception.Message})");
            }
        }

        if (!Console.IsErrorRedirected)
        {
            Console.Error.Write("\r".PadRight(80) + "\r");
        }

        if (present.Count > 0)
        {
            Console.WriteLine($"ok    already installed: {string.Join(", ", present)}");
        }

        foreach (var package in missing)
        {
            try
            {
                Console.WriteLine($"inst  {package.Name}");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var exit = Runner.RunInteractive(Paths.Winget, package.InstallArgs());
                stopwatch.Stop();
                var result = Packages.Classify(exit);
                if (result is WingetResult.RebootRequired or WingetResult.RebootBeforeInstall)
                {
                    rebootRequired = true;
                    Console.WriteLine($"todo  reboot and rerun apply to {(result == WingetResult.RebootBeforeInstall ? "install" : "verify")} {package.Name}");
                    if (result == WingetResult.RebootBeforeInstall)
                    {
                        failures.Add($"package: {package.Name} awaits installation after reboot");
                    }
                }
                else if (Packages.IsPresent(result))
                {
                    if (Packages.Detect(package))
                    {
                        Console.WriteLine($"done  {package.Name} ({stopwatch.Elapsed.TotalSeconds:0}s)");
                    }
                    else
                    {
                        failures.Add($"package: {package.Name} is still missing after installation");
                        Console.WriteLine($"FAIL  {package.Name} is still missing after installation");
                    }
                }
                else
                {
                    failures.Add($"package: {package.Name} (0x{unchecked((uint)exit):X8})");
                    Console.WriteLine($"FAIL  {package.Name} (0x{unchecked((uint)exit):X8})");
                    Console.WriteLine($"      reproduce: winget install --id {package.Id} --exact");
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
            Console.WriteLine("      enabling BitLocker (can take a minute)");
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

        Console.WriteLine("== wsl ==");
        try
        {
            if (!Wsl.FeaturesEnabled())
            {
                var enable = Wsl.EnableFeatures();
                if (enable.Ok)
                {
                    Console.WriteLine("set   WSL features enabled (reboot required)");
                    rebootRequired = true;
                }
                else
                {
                    failures.Add($"wsl features ({enable.Hex})");
                    Console.WriteLine($"FAIL  wsl features ({enable.Hex}) {enable.StdErr.Trim()}");
                }
            }
            else if (Wsl.DistroInstalled())
            {
                var configure = Wsl.ConfigureDistro();
                if (!configure.Ok)
                {
                    throw new InvalidOperationException($"Fedora WSL configuration failed ({configure.Hex}): {configure.StdErr.Trim()}");
                }

                if (Wsl.Provisioned())
                {
                    Console.WriteLine("ok    Fedora WSL provisioned");
                }
                else
                {
                    Console.WriteLine("set   provisioning Fedora WSL (packages + Chezmoi)");
                    var provision = Wsl.Provision();
                    if (provision.Ok)
                    {
                        Console.WriteLine("ok    Fedora WSL provisioned");
                    }
                    else
                    {
                        failures.Add($"wsl provision ({provision.Hex})");
                        Console.WriteLine($"FAIL  wsl provision ({provision.Hex}) {provision.StdErr.Trim()}");
                    }
                }

                if (Wsl.ShellIsZsh())
                {
                    Console.WriteLine("ok    zsh is the WSL login shell");
                }
                else
                {
                    var shell = Wsl.SetZshShell();
                    if (shell.Ok)
                    {
                        Console.WriteLine("set   zsh is the WSL login shell");
                    }
                    else
                    {
                        failures.Add($"wsl shell ({shell.Hex})");
                        Console.WriteLine($"FAIL  wsl shell ({shell.Hex}) {shell.StdErr.Trim()}");
                    }
                }
            }
            else if (Wsl.RebootPending())
            {
                Console.WriteLine("todo  reboot to finish the WSL feature install");
                rebootRequired = true;
            }
            else
            {
                var package = Wsl.EnsurePackage();
                if (!package.Ok)
                {
                    failures.Add($"wsl package ({package.Hex})");
                    Console.WriteLine($"FAIL  wsl package ({package.Hex})");
                }
                else
                {
                    Console.WriteLine("installing Fedora WSL (large download)");
                    var install = Wsl.InstallDistro();
                    if (install.Ok)
                    {
                        Console.WriteLine("inst  Fedora WSL");
                        Console.WriteLine($"todo  launch {Wsl.DefaultDistro}, finish Linux user setup, then rerun apply");
                    }
                    else
                    {
                        failures.Add($"wsl distro ({install.Hex})");
                        Console.WriteLine($"FAIL  wsl distro ({install.Hex}) {install.StdErr.Trim()}");
                    }
                }
            }
        }
        catch (Exception exception)
        {
            failures.Add($"wsl ({exception.Message})");
            Console.WriteLine($"FAIL  wsl ({exception.Message})");
        }

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
        Console.WriteLine(failures.Count == 0
            ? rebootRequired ? "summary: reboot and rerun apply" : "summary: all steps ok"
            : $"summary: {failures.Count} failed");
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
            Console.WriteLine("A reboot is required; rerun apply afterwards.");
        }

        ResetConsole();
        return failures.Count == 0 ? 0 : 1;
    }

    // ponytail: PowerShell writes the prompt before PSReadLine reads; winget can leave SGR/cursor/OSC progress state that hides it.
    private static void ResetConsole()
    {
        if (Console.IsErrorRedirected)
        {
            return;
        }

        try
        {
            Console.CursorVisible = true;
        }
        catch (IOException)
        {
        }

        Console.Error.Write("\x1b[0m\x1b[?25h\x1b]9;4;0\x1b\\");
        Console.Error.Flush();
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
