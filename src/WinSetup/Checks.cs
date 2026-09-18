using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinSetup;

public record HostStatus(
    string? Os,
    string? Build,
    bool Admin,
    bool? SecureBoot,
    bool TpmPresent,
    bool TpmReady,
    string? BitLockerStatus,
    string? BitLockerProtection,
    bool Winget,
    bool Chezmoi,
    string? Sshd)
{
    [JsonIgnore]
    public bool SecureBootEnabled => SecureBoot == true;

    [JsonIgnore]
    public bool BitLockerProtected => string.Equals(BitLockerProtection, "On", StringComparison.OrdinalIgnoreCase);
}

public static class Checks
{
    private const string Script = """
        $ErrorActionPreference = 'SilentlyContinue'
        $os = Get-CimInstance Win32_OperatingSystem
        $admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
        $secureBoot = try { Confirm-SecureBootUEFI } catch { $null }
        $tpm = Get-Tpm
        $bl = Get-BitLockerVolume -MountPoint $env:SystemDrive
        [pscustomobject]@{
          os = $os.Caption
          build = $os.BuildNumber
          admin = $admin
          secureBoot = $secureBoot
          tpmPresent = [bool]$tpm.TpmPresent
          tpmReady = [bool]$tpm.TpmReady
          bitlockerStatus = [string]$bl.VolumeStatus
          bitlockerProtection = [string]$bl.ProtectionStatus
          winget = [bool](Test-Path (Join-Path $env:LOCALAPPDATA 'Microsoft\WindowsApps\winget.exe'))
          chezmoi = [bool](Get-Command chezmoi -ErrorAction SilentlyContinue)
          sshd = [string](Get-Service sshd -ErrorAction SilentlyContinue).Status
        } | ConvertTo-Json -Compress
        """;

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static HostStatus? Collect()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        var result = Runner.Run(PowerShell.Exe, PowerShell.Args(Script));
        return result.Ok ? Parse(result.StdOut) : null;
    }

    public static HostStatus? Parse(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<HostStatus>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
