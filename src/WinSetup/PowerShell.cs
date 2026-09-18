namespace WinSetup;

public static class PowerShell
{
    public static string Exe
    {
        get
        {
            var path = Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");
            return File.Exists(path) ? path : "powershell.exe";
        }
    }

    public static string[] Args(string script) =>
        ["-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-Command", script];
}
