namespace WinSetup;

public static class Paths
{
    public static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static string UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static string ProgramFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    public static string Winget => Runner.Find(
        Path.Combine(LocalAppData, "Microsoft", "WindowsApps", "winget.exe"),
        Path.Combine(LocalAppData, "Microsoft", "WinGet", "Links", "winget.exe"),
        Path.Combine(ProgramFiles, "WinGet", "Links", "winget.exe")) ?? "winget";

    public static string Chezmoi => Runner.Find(
        Path.Combine(LocalAppData, "Microsoft", "WinGet", "Links", "chezmoi.exe"),
        Path.Combine(ProgramFiles, "WinGet", "Links", "chezmoi.exe"),
        Path.Combine(UserProfile, ".local", "bin", "chezmoi.exe")) ?? "chezmoi";
}
