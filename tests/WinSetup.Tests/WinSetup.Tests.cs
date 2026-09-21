using WinSetup;

namespace WinSetup.Tests;

public class WingetTests
{
    [Theory]
    [InlineData(0, WingetResult.Installed)]
    [InlineData(unchecked((int)0x8A150014), WingetResult.Missing)]
    [InlineData(unchecked((int)0x8A150061), WingetResult.AlreadyInstalled)]
    [InlineData(unchecked((int)0x8A15010D), WingetResult.AlreadyInstalled)]
    [InlineData(unchecked((int)0x8A150109), WingetResult.RebootRequired)]
    [InlineData(unchecked((int)0x8A15010A), WingetResult.RebootBeforeInstall)]
    [InlineData(unchecked((int)0x8A15010B), WingetResult.RebootRequired)]
    [InlineData(unchecked((int)0x8A15002B), WingetResult.Failed)]
    public void Classifies_winget_exit_codes(int code, WingetResult expected)
        => Assert.Equal(expected, Packages.Classify(code));

    [Fact]
    public void Detection_errors_and_pending_reboots_are_not_installed_packages()
    {
        Assert.True(Packages.DetectResult(new(0, "", "")));
        Assert.False(Packages.DetectResult(new(unchecked((int)0x8A150014), "", "")));
        Assert.Throws<InvalidOperationException>(() => Packages.DetectResult(new(-1, "", "unavailable")));
        Assert.False(Packages.IsPresent(Packages.Classify(unchecked((int)0x8A15010A))));
        Assert.False(Packages.IsPresent(Packages.Classify(unchecked((int)0x8A150109))));
    }

    [Fact]
    public void Install_args_are_exact_and_do_not_upgrade()
    {
        var args = new Package("Steam", "Valve.Steam").InstallArgs();
        Assert.Contains("--exact", args);
        Assert.Contains("--no-upgrade", args);
        Assert.DoesNotContain("--force", args);
    }

    [Fact]
    public void Msstore_packages_use_the_store_source()
    {
        var args = new Package("NVIDIA App", "XP8CLZL93F5Z4P", "msstore").InstallArgs();
        Assert.Contains("msstore", args);
    }

    [Fact]
    public void Package_ids_are_unique_and_exact()
    {
        var ids = Packages.All.Select(package => package.Id).ToArray();
        Assert.Equal(ids.Length, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(ids, id => Assert.DoesNotContain(" ", id));
    }
}

public class ChecksTests
{
    [Fact]
    public void Parses_host_status_json()
    {
        const string json = """
            {"os":"Microsoft Windows 11 Pro","build":"26100","admin":true,"secureBoot":true,
             "tpmPresent":true,"tpmReady":true,"bitlockerStatus":"FullyEncrypted",
             "bitlockerProtection":"On","winget":true,"chezmoi":false,"sshd":"Running"}
            """;
        var status = Checks.Parse(json);
        Assert.NotNull(status);
        Assert.True(status.Admin);
        Assert.True(status.SecureBootEnabled);
        Assert.True(status.BitLockerProtected);
        Assert.False(status.Chezmoi);
        Assert.Equal("Running", status.Sshd);
    }

    [Fact]
    public void Rejects_garbage_json() => Assert.Null(Checks.Parse("not json"));
}

public class SettingsTests
{
    [Fact]
    public void Dword_settings_compare_numerically()
    {
        var setting = new Setting("why", "HKCU", @"Software\Test", "Value", 0, Microsoft.Win32.RegistryValueKind.DWord);
        Assert.True(setting.Matches(0));
        Assert.True(setting.Matches("0"));
        Assert.False(setting.Matches(1));
        Assert.False(setting.Matches(null));
    }

    [Fact]
    public void String_settings_compare_ordinally()
    {
        var setting = new Setting("why", "HKCU", @"Control Panel\Mouse", "MouseSpeed", "0", Microsoft.Win32.RegistryValueKind.String);
        Assert.True(setting.Matches("0"));
        Assert.False(setting.Matches("1"));
    }

    [Fact]
    public void Binary_settings_compare_the_target_bit()
    {
        var setting = new Setting("why", "HKCU", @"Software\Test", "Settings", (byte)0x01, Microsoft.Win32.RegistryValueKind.Binary, ByteIndex: 8);
        Assert.True(setting.Matches(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0 }));
        Assert.True(setting.Matches(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0x7B, 0, 0, 0 }));
        Assert.False(setting.Matches(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0 }));
        Assert.False(setting.Matches(new byte[] { 1 }));
        Assert.False(setting.Matches(null));
    }

    [Fact]
    public void Binary_full_value_settings_compare_exactly()
    {
        var setting = new Setting("why", "HKCU", @"Software\Test", "Data", new byte[] { 1, 2, 3 }, Microsoft.Win32.RegistryValueKind.Binary);
        Assert.True(setting.Matches(new byte[] { 1, 2, 3 }));
        Assert.False(setting.Matches(new byte[] { 1, 2 }));
        Assert.False(setting.Matches("System.Byte[]"));
    }

    [Fact]
    public void Night_light_blobs_match_ignoring_timestamps()
    {
        var schedule = new Setting("why", "HKCU", @"Software\Test", "Data", Setting.BuildNightLightSchedule(1700000000), Microsoft.Win32.RegistryValueKind.Binary, IgnoreTimestamps: true);
        Assert.True(schedule.Matches(Setting.BuildNightLightSchedule(1800000000)));
        Assert.False(schedule.Matches(Setting.BuildNightLightState(1800000000, 134000000000000000)));

        var state = new Setting("why", "HKCU", @"Software\Test", "Data", Setting.BuildNightLightState(1700000000, 133000000000000000), Microsoft.Win32.RegistryValueKind.Binary, IgnoreTimestamps: true);
        Assert.True(state.Matches(Setting.BuildNightLightState(1800000000, 134000000000000000)));
    }

    [Fact]
    public void Settings_are_unique_and_labeled()
    {
        var keys = Setting.All.Select(setting => $"{setting.Hive}\\{setting.Key}\\{setting.Name}").ToArray();
        Assert.Equal(keys.Length, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(Setting.All, setting => Assert.False(string.IsNullOrWhiteSpace(setting.Why)));
    }
}

public class UpdateTests
{
    [Theory]
    [InlineData("0.1.6", "v0.1.7", true)]
    [InlineData("0.1.7", "v0.1.6", false)]
    [InlineData("0.1.6", "0.1.6", false)]
    [InlineData("0.1.6+abc123", "v0.1.7", true)]
    [InlineData("nonsense", "v0.1.7", false)]
    public void Compares_release_versions(string current, string latest, bool newer)
        => Assert.Equal(newer, Update.IsNewer(current, latest));
}

public class PowerTests
{
    [Fact]
    public void Parses_the_ac_index_from_powercfg_output()
    {
        const string output = """
              Power Setting GUID: 29f6c1db-86da-48c5-9fdb-f2b67b1f44da  (Sleep after)
                Current AC Power Setting Index: 0x00000384
                Current DC Power Setting Index: 0x00000258
            """;
        Assert.Equal(900u, Power.ParseAc(output));
    }

    [Fact]
    public void Returns_null_when_the_setting_is_hidden() => Assert.Null(Power.ParseAc("  GUID Alias: SCHEME_BALANCED"));

    [Fact]
    public void Power_items_are_unique()
    {
        var keys = PowerItem.All.Select(item => $"{item.Subgroup}\\{item.Setting}").ToArray();
        Assert.Equal(keys.Length, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}

[CollectionDefinition("Process state", DisableParallelization = true)]
public class ProcessStateCollection;

[Collection("Process state")]
public class SetupRegressionTests
{
    [Fact]
    public void An_apply_io_error_does_not_repeat_the_operation()
    {
        using var scratch = new Scratch();
        var calls = 0;
        var output = Console.Out;
        Assert.Throws<IOException>(() => Program.WithLog(_ =>
        {
            calls++;
            throw new IOException("operation failed");
        }, [], Path.Combine(scratch.Path, "apply.log")));
        Assert.Equal(1, calls);
        Assert.Same(output, Console.Out);
    }

    [Fact]
    public void An_unavailable_log_still_runs_the_operation_once()
    {
        using var scratch = new Scratch();
        var blocker = Path.Combine(scratch.Path, "file");
        File.WriteAllText(blocker, "keep");
        var calls = 0;
        Assert.Equal(7, Program.WithLog(_ => { calls++; return 7; }, [], Path.Combine(blocker, "apply.log")));
        Assert.Equal(1, calls);
        Assert.Equal("keep", File.ReadAllText(blocker));
    }

    [LinuxFact]
    public void Apply_on_linux_stops_before_updating_or_logging()
    {
        Assert.Equal(2, Program.Main(["apply"]));
    }

    [Theory]
    [InlineData("Ubuntu\nFedora-Old\n", "Fedora-Test", false)]
    [InlineData("Ubuntu\r\nFedora-Test\r\n", "Fedora-Test", true)]
    [InlineData("Fedora-Test-Backup\n", "Fedora-Test", false)]
    [InlineData("F\0e\0d\0o\0r\0a\0\r\0\n\0", "Fedora", true)]
    public void Wsl_matches_the_whole_distribution_name(string output, string target, bool expected)
        => Assert.Equal(expected, Wsl.HasDistro(new(0, output, ""), target));

    [Fact]
    public void Wsl_query_failure_is_not_an_absent_distribution()
        => Assert.Throws<InvalidOperationException>(() => Wsl.HasDistro(new(1, "", "failed"), "Fedora"));

    [LinuxFact]
    public void Wsl_configures_only_the_selected_distribution_and_propagates_failure()
    {
        using var scratch = new Scratch();
        var events = Path.Combine(scratch.Path, "events");
        scratch.Command("wsl.exe", """
            case "$*" in
              '--list --quiet') printf 'Ubuntu\n%s\n' "$TEST_DISTRO" ;;
              *) printf '%s\n' "$*" >> "$TEST_EVENTS"
                 case "$*" in *Ubuntu*) exit 99;; esac
                 exit 23 ;;
            esac
            """);
        var oldPath = Environment.GetEnvironmentVariable("PATH");
        var oldEvents = Environment.GetEnvironmentVariable("TEST_EVENTS");
        var oldDistro = Environment.GetEnvironmentVariable("TEST_DISTRO");
        try
        {
            Environment.SetEnvironmentVariable("PATH", scratch.Path + ":" + oldPath);
            Environment.SetEnvironmentVariable("TEST_EVENTS", events);
            Environment.SetEnvironmentVariable("TEST_DISTRO", Wsl.DefaultDistro);
            Assert.Equal(23, Wsl.ConfigureDistro().ExitCode);
            var commands = File.ReadAllText(events);
            Assert.Contains(Wsl.DefaultDistro, commands);
            Assert.DoesNotContain("Ubuntu", commands);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", oldPath);
            Environment.SetEnvironmentVariable("TEST_EVENTS", oldEvents);
            Environment.SetEnvironmentVariable("TEST_DISTRO", oldDistro);
        }
    }

    [Theory]
    [InlineData("complete")]
    [InlineData("short")]
    [InlineData("interrupted")]
    public void Wsl_download_replaces_only_a_complete_image(string mode)
    {
        using var scratch = new Scratch();
        var target = Path.Combine(scratch.Path, "image.wsl");
        File.WriteAllText(target, "previous image");
        HttpContent content = mode == "interrupted"
            ? new StreamContent(new InterruptedDownload())
            : new ByteArrayContent("download"u8.ToArray());
        content.Headers.ContentLength = mode == "short" ? 16 : 8;
        using var client = new HttpClient(new DownloadHandler(content));
        if (mode == "complete")
        {
            Wsl.DownloadImage(client, "https://example.invalid/image", target);
            Assert.Equal("download", File.ReadAllText(target));
        }
        else
        {
            Assert.Throws<IOException>(() => Wsl.DownloadImage(client, "https://example.invalid/image", target));
            Assert.Equal("previous image", File.ReadAllText(target));
        }

        Assert.Single(Directory.GetFiles(scratch.Path));
    }

    [LinuxFact]
    public void Provisioning_preserves_existing_sources_configs_and_unrelated_files()
    {
        using var scratch = new Scratch();
        var userHome = Path.Combine(scratch.Path, "user");
        var source = Path.Combine(userHome, ".local/share/chezmoi");
        var config = Path.Combine(userHome, ".config/chezmoi/chezmoi.toml");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(Path.GetDirectoryName(config)!);
        File.WriteAllText(Path.Combine(source, "keep"), "local source");
        File.WriteAllText(Path.Combine(userHome, ".config/unrelated"), "personal settings");
        scratch.Command("getent", "printf 'tester:x:2001:2001::%s:/bin/sh\\n' \"$TEST_USER_HOME\"");
        scratch.Command("dnf", "exit 0");
        scratch.Command("runuser", "[ \"$1\" = -u ] && [ \"$2\" = tester ] && [ \"$3\" = -- ] || exit 1\nshift 3\nexec \"$@\"");
        scratch.Command("chezmoi", "exit 0");
        scratch.Command("git", "exit 98");
        scratch.Command("chown", "exit 97");
        var environment = new Dictionary<string, string>
        {
            ["PATH"] = scratch.Path + ":" + Environment.GetEnvironmentVariable("PATH"),
            ["TEST_USER_HOME"] = userHome,
        };
        var args = new[] { "-c", Wsl.ProvisionScript, "win-setup", "tester", "test-machine" };
        Assert.False(Runner.Run("/bin/sh", args, environment).Ok);
        Assert.Equal("local source", File.ReadAllText(Path.Combine(source, "keep")));
        Assert.False(File.Exists(config));

        File.WriteAllText(config, "existing config");
        Assert.True(Runner.Run("/bin/sh", args, environment).Ok);
        Assert.True(Runner.Run("/bin/sh", args, environment).Ok);
        Assert.Equal("existing config", File.ReadAllText(config));
        Assert.Equal("personal settings", File.ReadAllText(Path.Combine(userHome, ".config/unrelated")));
        Assert.True(File.Exists(Path.Combine(userHome, ".cache/win-setup-wsl-provisioned")));

        scratch.Command("getent", "printf 'tester:x:0:0::%s:/bin/sh\\n' \"$TEST_USER_HOME\"");
        var rejected = Runner.Run("/bin/sh", args, environment);
        Assert.False(rejected.Ok);
        Assert.Contains("non-root", rejected.StdErr);
        Assert.Equal("existing config", File.ReadAllText(config));
    }

    [LinuxFact]
    public void Enabling_systemd_preserves_other_settings_and_does_not_require_repeated_restarts()
    {
        using var scratch = new Scratch();
        var file = Path.Combine(scratch.Path, "wsl.conf");
        const string original = "[automount]\nenabled=false\n[boot]\nsystemd=false\ncommand=echo ready\n[user]\ndefault=tester\n";
        File.WriteAllText(file, original);
        var args = new[] { "-c", Wsl.SystemdScript, "win-setup", file };
        var changed = Runner.Run("/bin/sh", args);
        Assert.True(changed.Ok, changed.StdErr);
        Assert.Equal("changed", changed.StdOut.Trim());
        Assert.Equal(original.Replace("systemd=false", "systemd=true"), File.ReadAllText(file));
        var repeated = Runner.Run("/bin/sh", args);
        Assert.True(repeated.Ok, repeated.StdErr);
        Assert.Empty(repeated.StdOut);

        File.Delete(file);
        Assert.True(Runner.Run("/bin/sh", args).Ok);
        Assert.Equal("[boot]\nsystemd=true\n", File.ReadAllText(file));
        Assert.Single(Directory.GetFiles(scratch.Path));
    }

    private sealed class DownloadHandler(HttpContent content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content });
    }

    private sealed class InterruptedDownload() : MemoryStream("download"u8.ToArray())
    {
        public override void CopyTo(Stream destination, int bufferSize)
        {
            destination.WriteByte(1);
            throw new IOException("connection lost");
        }
    }

    private sealed class Scratch : IDisposable
    {
        public string Path { get; } = Directory.CreateTempSubdirectory("win-setup-test-").FullName;

        public void Command(string name, string script)
        {
            var file = System.IO.Path.Combine(Path, name);
            File.WriteAllText(file, "#!/bin/sh\nset -eu\n" + script + "\n");
            File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}

public sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux())
        {
            Skip = "Uses Linux shell fixtures; Windows integration is checked separately.";
        }
    }
}
