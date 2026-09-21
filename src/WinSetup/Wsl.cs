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

    public static bool DistroInstalled() => HasDistro(Runner.Run(Exe, ["--list", "--quiet"]), DefaultDistro);

    public static bool HasDistro(RunResult result, string name) =>
        Clean(result).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(name, StringComparer.OrdinalIgnoreCase);

    private static string DefaultUser()
    {
        var user = Clean(Runner.Run(Exe, ["-d", DefaultDistro, "--", "id", "-un"])).Trim();
        if (user.Length == 0 || user == "root")
        {
            throw new InvalidOperationException($"Launch {DefaultDistro} and finish non-root user setup, then rerun apply.");
        }

        return user;
    }

    public static bool Provisioned() =>
        Runner.Run(Exe, ["-d", DefaultDistro, "-u", DefaultUser(), "--", "sh", "-c",
            "test -f \"$HOME/.cache/win-setup-wsl-provisioned\""]).Ok;

    public static bool ShellIsZsh() =>
        Runner.Run(Exe, ["-d", DefaultDistro, "-u", DefaultUser(), "--", "sh", "-c",
            "test \"$(getent passwd \"$(id -u)\" | cut -d: -f7)\" = /bin/zsh"]).Ok;

    public static RunResult SetZshShell() =>
        Runner.Run(Exe, ["-d", DefaultDistro, "-u", "root", "--", "chsh", "-s", "/bin/zsh", DefaultUser()]);

    public const string ProvisionScript = """
        set -eu
        user=$1
        machine=$2
        entry=$(getent passwd "$user")
        uid=$(printf '%s' "$entry" | cut -d: -f3)
        user_home=$(printf '%s' "$entry" | cut -d: -f6)
        case "$uid" in ''|*[!0-9]*|0) echo 'A non-root Linux user is required.' >&2; exit 1;; esac
        case "$user_home" in /|/*/../*|'') echo 'Invalid Linux home.' >&2; exit 1;; /*) ;; *) exit 1;; esac
        [ -d "$user_home" ] || { echo 'Linux home does not exist.' >&2; exit 1; }
        dnf install -y chezmoi fastfetch git zsh
        runuser -u "$user" -- sh -s -- "$user_home" "$machine" <<'USER_SCRIPT'
        set -eu
        user_home=$1
        machine=$2
        src="$user_home/.local/share/chezmoi"
        cfg="$user_home/.config/chezmoi/chezmoi.toml"
        log="$user_home/.cache/win-setup-wsl.log"
        mkdir -p "$user_home/.cache" "$user_home/.config/chezmoi"
        if [ ! -e "$cfg" ]; then
          if [ -e "$src" ] && [ ! -d "$src/.git" ]; then
            echo "Existing Chezmoi source preserved at $src; initialize it manually before rerunning." >&2
            exit 1
          fi
          if [ ! -d "$src" ]; then
            git clone --depth 1 https://github.com/Furyfree/dotfiles.git "$src"
          fi
          # Noclobber protects an existing config, including a dangling symlink.
          (set -C; cat > "$cfg" <<CONFIG
        [diff]
        exclude = ["scripts"]

        [data]
        Machine = "$machine"
        fastmailUsername = ""
        ManagedByNimbus = false
        onePasswordSsh = false
        Profiles = ["common", "development"]
        profiles = ["common", "unix", "linux", "development"]
        CONFIG
          )
        fi
        if ! chezmoi --no-tty apply --exclude=scripts </dev/null >"$log" 2>&1; then
          echo "chezmoi apply failed; see $log" >&2
          exit 1
        fi
        touch "$user_home/.cache/win-setup-wsl-provisioned"
        echo "note: config files applied; run scripts skipped. Install mise and run 'chezmoi apply' in WSL for tools."
        USER_SCRIPT
        """;

    public static RunResult Provision() =>
        Runner.RunStreaming(Exe, ["-d", DefaultDistro, "-u", "root", "--", "sh", "-c", ProvisionScript,
            "win-setup", DefaultUser(), Environment.MachineName]);

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
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("win-setup");
        // Old caches may contain interrupted downloads from previous versions.
        DownloadImage(client, ImageUrl, ImageFile);
        var install = Runner.Run(Exe, ["--install", "--from-file", ImageFile, "--no-launch"]);
        return install.Ok ? ConfigureDistro() : install;
    }

    public static void DownloadImage(HttpClient client, string url, string destination)
    {
        var temporary = destination + "." + Guid.NewGuid().ToString("N") + ".part";
        try
        {
            using var response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            using (var download = response.Content.ReadAsStream())
            using (var file = new FileStream(temporary, FileMode.CreateNew))
            {
                download.CopyTo(file);
                if (file.Length == 0 || (response.Content.Headers.ContentLength is long expected && file.Length != expected))
                {
                    throw new IOException("Incomplete WSL image download.");
                }
            }

            File.Move(temporary, destination, overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
        }
    }

    public static RunResult ConfigureDistro()
    {
        if (!DistroInstalled())
        {
            return new RunResult(1, string.Empty, $"{DefaultDistro} is not installed.");
        }

        var result = Runner.Run(Exe, ["--set-default", DefaultDistro]);
        if (!result.Ok)
        {
            return result;
        }

        // Preserve other WSL settings and only restart when systemd needs enabling.
        result = Runner.Run(Exe, ["-d", DefaultDistro, "-u", "root", "--", "sh", "-c",
            SystemdScript, "win-setup", "/etc/wsl.conf"]);
        return result.Ok && result.StdOut.Trim() == "changed"
            ? Runner.Run(Exe, ["--terminate", DefaultDistro])
            : result;
    }

    public const string SystemdScript = """
            set -eu
            file=$1
            if [ -f "$file" ] && awk '
              /^\[/ { boot = ($0 ~ /^\[boot\][[:space:]]*$/) }
              boot && /^[[:space:]]*systemd[[:space:]]*=[[:space:]]*true[[:space:]]*$/ { found=1 }
              END { exit !found }
            ' "$file"; then exit 0; fi
            tmp=$(mktemp "$file.XXXXXX")
            trap 'rm -f "$tmp"' EXIT
            touch "$file"
            awk '
              /^\[boot\][[:space:]]*$/ { boot=1; seen=1; print; print "systemd=true"; next }
              /^\[/ { boot=0 }
              boot && /^[[:space:]]*systemd[[:space:]]*=/ { next }
              { print }
              END { if (!seen) print "[boot]\nsystemd=true" }
            ' "$file" > "$tmp"
            cat "$tmp" > "$file"
            echo changed
            """;

    private static string Clean(RunResult result)
    {
        if (!result.Ok)
        {
            throw new InvalidOperationException($"WSL command failed ({result.Hex}): {result.StdErr.Trim()}");
        }

        // wsl.exe writes UTF-16; a UTF-8 pipe decoder leaves NUL bytes between ASCII characters.
        return result.StdOut.Replace("\0", string.Empty);
    }

    private static bool FeatureEnabled(string name)
    {
        var result = Runner.Run(Dism, ["/online", "/English", "/Get-FeatureInfo", $"/FeatureName:{name}"]);
        return Clean(result).Contains("State : Enabled", StringComparison.OrdinalIgnoreCase);
    }
}
