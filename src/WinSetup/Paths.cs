using Microsoft.Win32;

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

    public static void RefreshPath()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var machine = ReadPathVariable(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");
        var user = ReadPathVariable(RegistryHive.CurrentUser, "Environment");
        var current = Environment.GetEnvironmentVariable("Path");
        var merged = new[] { machine, user, current }
            .Where(part => !string.IsNullOrWhiteSpace(part));
        Environment.SetEnvironmentVariable("Path", string.Join(';', merged), EnvironmentVariableTarget.Process);
    }

    private static string? ReadPathVariable(RegistryHive hive, string key)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
        using var subKey = baseKey.OpenSubKey(key);
        return subKey?.GetValue("Path") as string;
    }
}
