# Specification

`win-setup` is a small Windows 11 setup utility. It takes a fresh install from
this project's custom media to a secure, sane, gaming-ready system with as
little machinery as possible. It is not a Windows Nimbus.

## Target

- Windows 11 Pro x64, UEFI, Secure Boot, TPM 2.0.
- Compatible with games, launchers, anti-cheat, Store/App Installer, Windows
  Update, Defender, and vendor drivers.
- No broad debloat pass: the installation media already provides that baseline.

## Implementation

C# on .NET 10, one executable and one test project. No state database, plan
engine, profiles, resource graph, DI container, or extra assemblies.

Commands: `status` (read-only drift check), `apply` (idempotent setup),
`snapshot` (read-only state bundle).

## Ownership

- **win-setup**: security checks, BitLocker, selected Windows settings, package
  installation, Chezmoi bootstrap and invocation.
- **Chezmoi**: personal dotfiles and application configuration. See
  [CONFIGS.md](CONFIGS.md).
- **WinGet / Store / vendors**: package installation. Exact verified IDs only;
  never fuzzy match; never uninstall.

## Principles

1. Implement only what this machine needs.
2. Detect before mutating, then recheck.
3. One intended owner per setting, package, or config.
4. Prefer supported mechanisms (native APIs, WinGet, documented commands,
   narrow PowerShell adapters) over registry hacks.
5. PowerShell is an adapter, not the architecture.
6. No secrets in Git or logs.

## Security

Preserve UEFI, Secure Boot, TPM 2.0, Defender, Firewall, UAC, Windows Update,
and Store/App Installer.

BitLocker: enable only when the TPM is ready; use a TPM-backed protector;
ensure a recovery protector exists; never write the recovery password to
console or logs (it goes to a file named in the summary); verify protection
after enabling. Firmware or boot-chain changes may require suspending
BitLocker, and the recovery key must live independently of the PC.

## Settings

Settings are individually selected C# records in `Settings.cs`, detected by
reading current state and written only on mismatch. Reboot and Explorer-restart
needs are stated in each setting's description. Power scheme values use
`powercfg` where Windows does not act on direct registry writes.

## Packages

See [PACKAGES.md](PACKAGES.md). Existing installs are detected and preserved.

## Non-goals

WSL and Linux provisioning, Docker, development runtimes and databases, a
desired-state engine, disk repartitioning, game installation, backup
orchestration, and chasing a process count.
