using System.Text.Json;

namespace WinSetup;

public static class Snapshot
{
    public static string Folder => Path.Combine(Paths.UserProfile, "win-setup-snapshot");

    private const string Script = """
        $ErrorActionPreference = 'SilentlyContinue'
        $dest = $env:WINSETUP_SNAPSHOT_DIR
        New-Item -ItemType Directory -Force -Path $dest | Out-Null

        $keys = @(
          'HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced',
          'HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3',
          'HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects',
          'HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband',
          'HKCU\Software\Microsoft\Windows\CurrentVersion\Search',
          'HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize',
          'HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager',
          'HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo',
          'HKCU\Control Panel\Desktop',
          'HKCU\Control Panel\Mouse',
          'HKCU\Control Panel\Accessibility',
          'HKCU\System\GameConfigStore',
          'HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR',
          'HKCU\Software\Microsoft\GameBar',
          'HKCU\Software\Policies\Microsoft\Windows',
          'HKLM\SOFTWARE\Policies\Microsoft\Windows',
          'HKLM\SOFTWARE\Microsoft\PolicyManager\current\device',
          'HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies',
          'HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers'
        )
        foreach ($key in $keys) {
          $name = ($key -replace '[\\:]', '_') + '.reg'
          reg export "$key" (Join-Path $dest $name) /y 2>$null | Out-Null
        }

        winget export --output (Join-Path $dest 'winget.json') --include-versions --accept-source-agreements --disable-interactivity | Out-Null

        Get-AppxPackage | Sort-Object Name | Select-Object Name, Version |
          ConvertTo-Json -Compress | Out-File (Join-Path $dest 'appx.json') -Encoding utf8
        Get-CimInstance Win32_StartupCommand | Select-Object Name, Command, Location, User |
          ConvertTo-Json -Compress | Out-File (Join-Path $dest 'startup.json') -Encoding utf8
        Get-Service | Where-Object Status -eq 'Running' | Sort-Object Name |
          Select-Object -ExpandProperty Name | Out-File (Join-Path $dest 'services-running.txt') -Encoding utf8
        Get-ChildItem -Force $env:USERPROFILE | Select-Object Mode, Name |
          Format-Table -AutoSize | Out-String | Out-File (Join-Path $dest 'userprofile.txt') -Encoding utf8
        """;

    public static int Run()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("snapshot must run on Windows");
            return 2;
        }

        Directory.CreateDirectory(Folder);
        var environment = new Dictionary<string, string> { ["WINSETUP_SNAPSHOT_DIR"] = Folder };
        var result = Runner.Run(PowerShell.Exe, PowerShell.Args(Script), environment);
        if (!result.Ok)
        {
            Console.WriteLine($"snapshot failed ({result.Hex}) {result.StdErr.Trim()}");
            return 1;
        }

        var host = Checks.Collect();
        if (host is not null)
        {
            File.WriteAllText(
                Path.Combine(Folder, "host.json"),
                JsonSerializer.Serialize(host, new JsonSerializerOptions { WriteIndented = true }));
        }

        var settings = Setting.All.Select(setting => new
        {
            setting.Why,
            path = $"{setting.Hive}\\{setting.Key}",
            valueName = setting.Name,
            current = setting.Read(),
            state = setting.Check().ToString(),
        });
        File.WriteAllText(
            Path.Combine(Folder, "settings.json"),
            JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));

        Console.WriteLine($"snapshot written to {Folder}");
        return 0;
    }
}
