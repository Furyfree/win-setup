namespace WinSetup;

public record Package(string Name, string Id, string Source = "winget", string[]? Args = null)
{
    public string[] DetectArgs =>
        Source == "msstore"
            ? ["list", "--id", Id, "--exact", "--source", "msstore", "--accept-source-agreements", "--disable-interactivity"]
            : ["list", "--id", Id, "--exact", "--accept-source-agreements", "--disable-interactivity"];

    public string[] InstallArgs()
    {
        var args = new List<string>
        {
            "install", "--id", Id, "--exact", "--silent", "--no-upgrade",
            "--accept-package-agreements", "--accept-source-agreements", "--disable-interactivity",
        };
        if (Source == "msstore")
        {
            args.Add("--source");
            args.Add("msstore");
        }

        if (Args is not null)
        {
            args.AddRange(Args);
        }

        return [.. args];
    }
}

public enum WingetResult { Installed, Missing, AlreadyInstalled, RebootRequired, Failed }

public static class Packages
{
    public static readonly Package[] All =
    [
        new("Steam", "Valve.Steam"),
        new("Battle.net", "Blizzard.BattleNet", Args: ["--location", @"C:\Program Files (x86)\Battle.net"]),
        new("Prism Launcher", "PrismLauncher.PrismLauncher"),
        new("WowUp-CF", "WowUp.CF"),
        new("Visual C++ x64", "Microsoft.VCRedist.2015+.x64"),
        new("Visual C++ x86", "Microsoft.VCRedist.2015+.x86"),
        new("NVIDIA App", "XP8CLZL93F5Z4P", Source: "msstore"),
        new("PowerShell 7", "Microsoft.PowerShell"),
        new("Windows Terminal", "Microsoft.WindowsTerminal"),
        new("VSCodium", "VSCodium.VSCodium"),
        new("Zed", "ZedIndustries.Zed"),
        new("Brave Origin Nightly", "Brave.BraveOrigin.Nightly"),
        new("Git", "Git.Git"),
        new("GitHub CLI", "GitHub.cli"),
        new("chezmoi", "twpayne.chezmoi"),
        new("1Password", "AgileBits.1Password"),
        new("1Password CLI", "AgileBits.1Password.CLI"),
        new("Tailscale", "Tailscale.Tailscale"),
        new("Signal", "OpenWhisperSystems.Signal"),
        new("7-Zip", "7zip.7zip"),
        new("Everything", "voidtools.Everything"),
        new(".NET Desktop Runtime 8", "Microsoft.DotNet.DesktopRuntime.8"),
        new("EverythingToolbar", "srwi.EverythingToolbar.Launcher"),
        new("PowerToys", "Microsoft.PowerToys"),
        // new("Windhawk", "RamenSoftware.Windhawk"),
        // new("GlazeWM", "glzr-io.glazewm"),
        // new("Zebar", "glzr-io.zebar"),
    ];

    public static WingetResult Classify(int exitCode) => exitCode switch
    {
        0 => WingetResult.Installed,
        unchecked((int)0x8A150014) => WingetResult.Missing,
        unchecked((int)0x8A150061) => WingetResult.AlreadyInstalled,
        unchecked((int)0x8A15010D) => WingetResult.AlreadyInstalled,
        unchecked((int)0x8A150109) => WingetResult.RebootRequired,
        unchecked((int)0x8A15010A) => WingetResult.RebootRequired,
        unchecked((int)0x8A15010B) => WingetResult.RebootRequired,
        _ => WingetResult.Failed,
    };

    public static bool IsPresent(WingetResult result) =>
        result is WingetResult.Installed or WingetResult.AlreadyInstalled or WingetResult.RebootRequired;
}
