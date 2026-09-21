# Windows installation

Use Windows 11 Pro x64 with UEFI, Secure Boot and TPM 2.0. This workstation
uses a dedicated 1 TB Samsung 990 PRO; verify the target disk before writing.

## Prepare the media

Create the custom Windows media with Win11 Creator in Chris Titus Tech's
[Windows Toolbox](https://cttstore.com/products/windows-toolbox).
Win11 Creator is also available in
[WinUtil](https://github.com/ChrisTitusTech/winutil).

For WoeUSB on Fedora, use the
[furyfree/woeusb COPR](https://copr.fedorainfracloud.org/coprs/furyfree/woeusb/).
See [WoeUSB on GitHub](https://github.com/WoeUSB/WoeUSB) for usage.

```sh
sudo woeusb --device Win11_Custom.iso /dev/sdX
```

Replace `/dev/sdX` with the USB device to erase and write.

## Install and boot

1. Boot the installer in UEFI mode with Secure Boot enabled.
2. Select the dedicated Windows disk; preserve the other disks.
3. Complete Windows setup. Win11 Creator may reboot again after first login;
   let provisioning finish.
4. Check Windows Update, Defender, Firewall and Store/App Installer, then
   install the hardware drivers.
5. Follow the [bootstrap instructions](README.md#install).
6. Store the BitLocker recovery key outside the PC and check a game.

Do not clear the TPM, replace firmware keys or change storage-controller
mode to work around an installer problem.
