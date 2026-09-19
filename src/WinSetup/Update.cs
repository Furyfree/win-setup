using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace WinSetup;

public static class Update
{
    private const string Repo = "Furyfree/win-setup";

    public static string InstallDir => Path.Combine(Paths.LocalAppData, "Programs", "win-setup");

    public static string InstalledExe => Path.Combine(InstallDir, "win-setup.exe");

    public static string CurrentVersion =>
        typeof(Update).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(Update).Assembly.GetName().Version?.ToString()
        ?? "0.0.0";

    public static string DisplayVersion
    {
        get
        {
            var text = CurrentVersion;
            var cut = text.IndexOf('+');
            return cut >= 0 ? text[..cut] : text;
        }
    }

    public static int RunApply(string[] args)
    {
        EnsureInstalled();
        return Commands.Apply();
    }

    public static int? RestartIfNewer(string[] args)
    {
        try
        {
            EnsureInstalled();
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("win-setup");
            var json = client.GetStringAsync($"https://api.github.com/repos/{Repo}/releases/latest").GetAwaiter().GetResult();

            using var release = JsonDocument.Parse(json);
            var tag = release.RootElement.GetProperty("tag_name").GetString() ?? string.Empty;
            if (!IsNewer(CurrentVersion, tag))
            {
                return null;
            }

            var asset = release.RootElement.GetProperty("assets").EnumerateArray()
                .FirstOrDefault(item => item.GetProperty("name").GetString() == "win-setup.exe");
            var url = asset.GetProperty("browser_download_url").GetString();
            var expectedSize = asset.GetProperty("size").GetInt64();
            if (url is null)
            {
                return null;
            }

            var target = Path.Combine(Path.GetTempPath(), $"win-setup-{tag}.exe");
            using (var download = client.GetStreamAsync(url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult())
            using (var file = File.Create(target))
            {
                download.CopyTo(file);
            }

            if (new FileInfo(target).Length != expectedSize)
            {
                File.Delete(target);
                return null;
            }

            Console.WriteLine($"Updating win-setup to {tag}...");
            var exitCode = Runner.RunInteractive(target, args.Length > 0 ? args : ["apply"]);
            ScheduleReplace(target);
            return exitCode;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return null;
        }
    }

    public static bool IsNewer(string current, string latest)
    {
        return TryParse(current, out var currentVersion)
            && TryParse(latest, out var latestVersion)
            && latestVersion > currentVersion;
    }

    public static bool TryParse(string? value, out Version version)
    {
        var text = (value ?? string.Empty).Trim();
        if (text.StartsWith('v'))
        {
            text = text[1..];
        }

        var cut = text.IndexOfAny(['+', '-']);
        if (cut >= 0)
        {
            text = text[..cut];
        }

        return Version.TryParse(text, out version!);
    }

    public static void EnsureInstalled()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var current = Environment.ProcessPath;
        if (current is null || string.Equals(current, InstalledExe, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                Directory.CreateDirectory(InstallDir);
                File.Copy(current, InstalledExe, overwrite: true);
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(200);
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
        }
    }

    // ponytail: a detached cmd retries until this process releases the installed exe; renaming it breaks single-file assembly loading.
    private static void ScheduleReplace(string downloaded)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            var script = Path.Combine(Path.GetTempPath(), "win-setup-replace.cmd");
            File.WriteAllText(script,
                "@echo off\r\n" +
                "for /l %%i in (1,1,30) do (\r\n" +
                $"  copy /y \"{downloaded}\" \"{InstalledExe}\" >nul 2>&1 && exit /b\r\n" +
                "  timeout /t 1 /nobreak >nul\r\n" +
                ")\r\n");
            var startInfo = new ProcessStartInfo("cmd.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("/c");
            startInfo.ArgumentList.Add(script);
            Process.Start(startInfo);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
        }
    }
}
