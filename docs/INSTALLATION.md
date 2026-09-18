# Installation

This records the bare-metal Windows installation used for this project. It is
both a reconstruction guide and a factual history of the current install.

## Hardware target

Windows is installed on the dedicated **1 TB Samsung 990 PRO NVMe**.

The drive presents roughly 930 GiB of usable capacity to Windows. Other system
drives must not be reformatted as part of this procedure.

## Installation media

The Windows installation media was created from **Windows 11 Pro** using the
paid **Win11 Creator by Chris Titus Tech**.

Win11 Creator is proprietary/paid software. Neither the tool nor the generated
Windows ISO belongs in this repository.

The resulting custom ISO is intentionally debloated while retaining normal
Windows application and gaming compatibility as the project baseline.

## Writing the USB from Fedora

The custom ISO was written from Fedora using **WoeUSB 5.2.4**.

The normal command shape is:

```bash
sudo woeusb --device Win11_Custom.iso /dev/sdX
```

The target device must be verified before running this command because
`--device` overwrites the selected disk.

For the successful installation WoeUSB created Windows-compatible media and
split the oversized `install.wim` into SWM parts for FAT32 as needed.

### WoeUSB notes from this install

Two Fedora-specific issues were encountered while preparing the USB:

1. WoeUSB's legacy GRUB stage needs the Fedora i386-pc GRUB modules. The missing
   dependency manifested as a missing
   `/usr/lib/grub/i386-pc/modinfo.sh`.
2. `udiskie` could immediately automount the new USB partition while WoeUSB was
   still preparing it. Temporarily stopping the automounter avoided the
   `contains a mounted filesystem` race.

These findings are also why the separate COPR packaging work includes the
required GRUB module dependency.

## Windows installation

1. Boot the WoeUSB-created installer in UEFI mode with Secure Boot still
   enabled.
2. Select the **1 TB Samsung 990 PRO** as the Windows target.
3. Install Windows 11 Pro normally.
4. Complete the initial Windows setup.

Do not disable Secure Boot, clear the TPM, replace firmware keys, or change
storage-controller mode merely to make the installer boot.

## First boot behavior

The custom Win11 Creator setup performs additional provisioning after the first
Windows boot.

Approximately 30 seconds after reaching the initial Windows environment, the
machine reboots again automatically. This reboot is expected and is part of the
Win11 Creator setup.

Allow it to reboot and finish before judging the final installation state.

## Observed initial result

After the custom setup completed, the fresh Windows installation was
approximately:

- **130-140 processes** shortly after boot.
- **888 GB free out of roughly 930 GB** on the Windows volume.

These values are observations, not hard requirements. Future Windows updates,
drivers, and selected applications will change them.

The project should optimize for a stable, secure, gaming-capable system rather
than enforcing a specific process count.

## Post-install sequence

The next setup work is:

1. Verify Windows Update, Defender, Firewall, Store/App Installer, and normal
   device functionality.
2. Install/update the appropriate GPU and hardware drivers.
3. Verify Secure Boot and TPM.
4. Enable BitLocker on the Windows system volume if it is not already enabled.
5. Apply the deliberately selected Windows settings.
6. Install the gaming prerequisites and launchers from
   [PACKAGES.md](PACKAGES.md).
7. Install/bootstrap Chezmoi and apply the selected configuration from
   [CONFIGS.md](CONFIGS.md).
8. Verify a representative game.

## BitLocker

The current fresh installation did not have BitLocker enabled automatically.

BitLocker is part of the planned post-install security baseline. Recovery-key
handling must be decided and documented before automatic enablement is
implemented.

See [SPEC.md](SPEC.md) and [TASKS.md](TASKS.md).
