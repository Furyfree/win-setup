namespace WinSetup;

public static class BitLocker
{
    public static string KeyFile => Path.Combine(Paths.UserProfile, $"bitlocker-recovery-{Environment.MachineName}.txt");

    private const string Script = """
        $ErrorActionPreference = 'Stop'
        $WarningPreference = 'SilentlyContinue'
        $drive = $env:SystemDrive
        $keyFile = $env:WINSETUP_BITLOCKER_FILE
        $keyWritten = 'no'

        $volume = Get-BitLockerVolume -MountPoint $drive
        $recovery = $volume.KeyProtector | Where-Object { $_.KeyProtectorType -eq 'RecoveryPassword' } | Select-Object -First 1
        if (-not $recovery) {
            $recovery = (Add-BitLockerKeyProtector -MountPoint $drive -RecoveryPasswordProtector).KeyProtector |
                Where-Object { $_.KeyProtectorType -eq 'RecoveryPassword' } |
                Select-Object -First 1
        }
        if ($recovery -and -not (Test-Path -LiteralPath $keyFile)) {
            Set-Content -LiteralPath $keyFile -Value $recovery.RecoveryPassword -NoNewline
            $keyWritten = 'yes'
        }

        if ((Get-BitLockerVolume -MountPoint $drive).VolumeStatus -eq 'FullyDecrypted') {
            Enable-BitLocker -MountPoint $drive -EncryptionMethod XtsAes256 -UsedSpaceOnly -TpmProtector -SkipHardwareTest | Out-Null
        }

        $volume = Get-BitLockerVolume -MountPoint $drive
        if (-not ($volume.KeyProtector | Where-Object { $_.KeyProtectorType -eq 'Tpm' })) {
            Add-BitLockerKeyProtector -MountPoint $drive -TpmProtector | Out-Null
        }

        $volume = Get-BitLockerVolume -MountPoint $drive
        if ($volume.ProtectionStatus -eq 'Off' -and $volume.VolumeStatus -eq 'FullyEncrypted') {
            Resume-BitLocker -MountPoint $drive | Out-Null
        }

        $volume = Get-BitLockerVolume -MountPoint $drive
        '{0}|{1}|{2}' -f $volume.VolumeStatus, $volume.ProtectionStatus, $keyWritten
        """;

    public static RunResult Enable()
    {
        var environment = new Dictionary<string, string> { ["WINSETUP_BITLOCKER_FILE"] = KeyFile };
        return Runner.Run(PowerShell.Exe, PowerShell.Args(Script), environment);
    }
}
