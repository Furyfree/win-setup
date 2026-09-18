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
    [InlineData(unchecked((int)0x8A15010A), WingetResult.RebootRequired)]
    [InlineData(unchecked((int)0x8A15002B), WingetResult.Failed)]
    public void Classifies_winget_exit_codes(int code, WingetResult expected)
        => Assert.Equal(expected, Packages.Classify(code));

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
    public void Settings_are_unique_and_labeled()
    {
        var keys = Setting.All.Select(setting => $"{setting.Hive}\\{setting.Key}\\{setting.Name}").ToArray();
        Assert.Equal(keys.Length, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(Setting.All, setting => Assert.False(string.IsNullOrWhiteSpace(setting.Why)));
    }
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
