# Windows installation

Use Windows 11 Pro x64 with UEFI, Secure Boot and TPM 2.0. This workstation
uses a dedicated 1 TB Samsung 990 PRO; verify the target disk before writing.

## Prepare the media

The current baseline uses custom media from Chris Titus Tech's paid Win11
Creator. Keep the tool and generated ISO outside this repository.

On Fedora, write the ISO to a verified USB device with WoeUSB:

```sh
pkexec /usr/bin/woeusb --device Win11_Custom.iso /dev/sdX
```

Replace `/dev/sdX` with the USB device. This erases that device.

WoeUSB needs the i386-pc GRUB modules on Fedora. A missing
`/usr/lib/grub/i386-pc/modinfo.sh` means those modules are absent.
Stop the USB automounter temporarily if it mounts a partition while WoeUSB
is writing and causes a mounted-filesystem error.

## Install and boot

1. Boot the installer in UEFI mode with Secure Boot enabled.
2. Select the dedicated Windows disk; preserve the other disks.
3. Complete Windows setup. Win11 Creator may reboot again after first login;
   let provisioning finish.
4. Check Windows Update, Defender, Firewall and Store/App Installer, then
   install the hardware drivers.
5. Follow the [bootstrap instructions](../README.md#install).
6. Store the BitLocker recovery key outside the PC and check a game.

Do not clear the TPM, replace firmware keys or change storage-controller
mode to work around an installer problem.
