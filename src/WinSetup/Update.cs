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
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("win-setup");
            var json = client.GetStringAsync($"https://api.github.com/repos/{Repo}/releases/latest").GetAwaiter().GetResult();

            using var release = JsonDocument.Parse(json);
            var tag = release.RootElement.GetProperty("tag_name").GetString() ?? string.Empty;
            if (!IsNewer(CurrentVersion, tag))
            {
                return null;
            }

            var url = release.RootElement.GetProperty("assets").EnumerateArray()
                .Where(asset => asset.GetProperty("name").GetString() == "win-setup.exe")
                .Select(asset => asset.GetProperty("browser_download_url").GetString())
                .FirstOrDefault(link => !string.IsNullOrEmpty(link));
            if (url is null)
            {
                return null;
            }

            var target = Path.Combine(Path.GetTempPath(), $"win-setup-{tag}.exe");
            using (var download = client.GetStreamAsync(url).GetAwaiter().GetResult())
            using (var file = File.Create(target))
            {
                download.CopyTo(file);
            }

            Console.WriteLine($"Updating win-setup to {tag}...");
            var executable = ReplaceInstalled(target);
            return Runner.RunInteractive(executable, args.Length > 0 ? args : ["apply"]);
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

    private static string ReplaceInstalled(string downloaded)
    {
        if (!OperatingSystem.IsWindows())
        {
            return downloaded;
        }

        try
        {
            var old = InstalledExe + ".old";
            File.Move(InstalledExe, old, overwrite: true);
            File.Copy(downloaded, InstalledExe, overwrite: true);
            try
            {
                File.Delete(old);
            }
            catch (IOException)
            {
            }

            return InstalledExe;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return downloaded;
        }
    }
}
