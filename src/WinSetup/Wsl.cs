namespace WinSetup;

using Microsoft.Win32;

public static class Wsl
{
    public const string DefaultDistro = "FedoraLinux-44";

    private const string ImageUrl =
        "https://download.fedoraproject.org/pub/fedora/linux/releases/44/Container/x86_64/images/Fedora-WSL-Base-44-1.7.x86_64.wsl";

    public static string Exe => Path.Combine(Environment.SystemDirectory, "wsl.exe");

    public static string Dism => Path.Combine(Environment.SystemDirectory, "dism.exe");

    public static string ImageFile => Path.Combine(Paths.LocalAppData, "win-setup", "Fedora-WSL-Base-44.wsl");

    public static bool FeaturesEnabled() =>
        FeatureEnabled("VirtualMachinePlatform") && FeatureEnabled("Microsoft-Windows-Subsystem-Linux");

    public static bool DistroInstalled() =>
        Clean(Runner.Run(Exe, ["--list", "--quiet"])).Contains("Fedora", StringComparison.OrdinalIgnoreCase);

    public static bool Provisioned() =>
        Runner.Run(Exe, ["-d", DefaultDistro, "--", "sh", "-c",
            "test -f \"$HOME/.config/chezmoi/chezmoi.toml\" && test -d \"$HOME/.local/share/chezmoi/.git\""]).Ok;

    public static RunResult Provision()
    {
        var data = $$"""
            {"Machine":"{{Environment.MachineName}}","ManagedByNimbus":false,"onePasswordSsh":false,"Profiles":["development"]}
            """;
        var script = $$"""
            set -e
            dnf install -y chezmoi git zsh
            user=$(getent passwd 1000 | cut -d: -f1)
            home=$(getent passwd 1000 | cut -d: -f6)
            log="$home/.cache/win-setup-wsl.log"
            if [ ! -f "$home/.config/chezmoi/chezmoi.toml" ]; then
              if [ -d "$home/.local/share/chezmoi" ] && [ ! -d "$home/.local/share/chezmoi/.git" ]; then
                rm -rf "$home/.local/share/chezmoi"
              fi
              mkdir -p "$home/.cache"
              chown -R "$user:" "$home/.cache"
              if ! runuser -u "$user" -- chezmoi init --apply --override-data '{{data}}' https://github.com/Furyfree/dotfiles.git </dev/null >"$log" 2>&1; then
                echo "chezmoi init failed; last log lines:"
                tail -30 "$log"
                exit 1
              fi
              tail -3 "$log"
            fi
            """;
        var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(script));
        return Runner.RunStreaming(Exe, ["-d", DefaultDistro, "-u", "root", "bash", "-c", $"echo {encoded} | base64 -d | bash"]);
    }

    public static bool RebootPending() =>
        Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending") is not null
        || Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired") is not null;

    public static RunResult EnableFeatures()
    {
        var platform = Normalize(Runner.Run(Dism, ["/online", "/enable-feature", "/featurename:VirtualMachinePlatform", "/all", "/norestart"]));
        if (!platform.Ok)
        {
            return platform;
        }

        return Normalize(Runner.Run(Dism, ["/online", "/enable-feature", "/featurename:Microsoft-Windows-Subsystem-Linux", "/all", "/norestart"]));
    }

    private static RunResult Normalize(RunResult result) =>
        result.ExitCode == 3010 ? result with { ExitCode = 0 } : result;

    public static RunResult EnsurePackage()
    {
        if (Runner.Run(Exe, ["--version"]).Ok)
        {
            return new RunResult(0, string.Empty, string.Empty);
        }

        return Runner.Run(Paths.Winget,
        [
            "install", "--id", "Microsoft.WSL", "--exact", "--silent", "--no-upgrade",
            "--accept-package-agreements", "--accept-source-agreements", "--disable-interactivity",
        ]);
    }

    public static RunResult InstallDistro()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ImageFile)!);
        if (!File.Exists(ImageFile))
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("win-setup");
            using var download = client.GetStreamAsync(ImageUrl).GetAwaiter().GetResult();
            using var file = File.Create(ImageFile);
            download.CopyTo(file);
        }

        var install = Runner.Run(Exe, ["--install", "--from-file", ImageFile, "--no-launch"]);
        if (!install.Ok)
        {
            return install;
        }

        var name = FirstDistro();
        if (name is not null)
        {
            Runner.Run(Exe, ["--set-default", name]);
            Runner.Run(Exe,
            [
                "-d", name, "-u", "root", "sh", "-c",
                "grep -q '^systemd=true' /etc/wsl.conf 2>/dev/null || printf '[boot]\\nsystemd=true\\n' >> /etc/wsl.conf",
            ]);
            Runner.Run(Exe, ["--terminate", name]);
        }

        return install;
    }

    private static string? FirstDistro() =>
        Clean(Runner.Run(Exe, ["--list", "--quiet"]))
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

    // ponytail: wsl.exe writes UTF-16; when the pipe decodes as UTF-8 every ASCII char has a NUL byte.
    private static string Clean(RunResult result) => result.StdOut.Replace("\0", string.Empty);

    private static bool FeatureEnabled(string name) =>
        Runner.Run(Dism, ["/online", "/Get-FeatureInfo", $"/FeatureName:{name}"])
            .StdOut.Contains("State : Enabled", StringComparison.OrdinalIgnoreCase);
}
