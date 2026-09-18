# Specification

## Purpose

`win-setup` is a small Windows 11 setup utility for this workstation. Its first
goal is to take a fresh Windows installation from the custom installation media
used by this project to a secure, sane, gaming-ready system with as little
machinery as possible.

It is not intended to become a Windows equivalent of Nimbus.

## Initial target

- Windows 11 Pro on x64.
- Bare-metal installation on the dedicated 1 TB Samsung 990 PRO NVMe.
- UEFI, Secure Boot, and TPM 2.0 remain enabled.
- Windows stays compatible with normal games, launchers, anti-cheat systems,
  Microsoft Store/App Installer, Windows Update, Defender, and vendor drivers.
- The installation remains deliberately debloated without applying additional
  broad "remove everything" tweak bundles after installation.

## Implementation

The utility is written in C# on .NET 10.

The initial implementation should remain one executable and one test project.
Do not introduce separate Core/Infrastructure/provider assemblies, a state
database, profile engine, resource graph, or dependency-injection framework
until an actual requirement justifies one.

The planned command surface starts small:

```text
win-setup status
win-setup apply
win-setup snapshot
```

`status` is read-only. `snapshot` is read-only and writes a machine-state
bundle for review. `apply` performs the setup steps owned by this project and
must be safe to rerun.

A dry-run or more specialized commands may be added later if they solve a
concrete problem.

## Ownership

### win-setup owns

- Windows security checks and machine-level security setup.
- BitLocker state for the Windows system volume.
- A deliberately small set of selected Windows settings.
- Installation and verification of Windows packages and gaming prerequisites.
- Bootstrapping Chezmoi and invoking it when appropriate.
- Verification that the resulting Windows system is usable for its intended
  purpose.

### Chezmoi owns

- Personal dotfiles and application configuration.
- Cross-platform configuration shared with Linux/macOS where appropriate.
- Windows Terminal configuration.
- PowerShell profile and shell configuration.
- Git configuration.
- Zed configuration.
- GlazeWM and Zebar configuration.
- Other stable per-user application configuration that is naturally represented
  as files or Chezmoi templates.

See [CONFIGS.md](CONFIGS.md).

### Package managers and vendors own

`win-setup` orchestrates package managers and vendor installers rather than
reimplementing them. WinGet should be the default Windows package source when a
verified package identity exists. Microsoft Store and official vendor installers
remain valid owners where WinGet is not appropriate.

## Design principles

1. **Small first.** Implement only the setup needed for the current machine.
2. **Observe before changing.** Detect current state before applying a mutation.
3. **Idempotent where practical.** Rerunning setup should not duplicate entries,
   reinstall already satisfied packages unnecessarily, or reset unrelated user
   state.
4. **Explicit ownership.** Each setting, package, or config has one intended
   owner.
5. **No blanket debloat pass.** The installation media already provides the
   chosen debloated baseline.
6. **Prefer supported Windows mechanisms.** Use native APIs, WinGet, documented
   commands, or narrowly scoped PowerShell adapters rather than registry hacks
   when a supported interface exists.
7. **PowerShell is an adapter, not the architecture.** C# is the main
   implementation. Small PowerShell 5.1 scripts are acceptable when Windows
   exposes functionality most reliably through those cmdlets.
8. **No secret state in Git.** Recovery keys, tokens, passwords, session data,
   private keys, and machine authentication caches are never committed.

## Security contract

The setup must preserve:

- UEFI boot.
- Secure Boot.
- TPM 2.0.
- Windows Defender unless a deliberate later decision changes that.
- Windows Firewall.
- UAC.
- Windows Update.
- Microsoft Store/App Installer functionality needed by selected software.

For BitLocker, the program should:

- inspect the current system-volume state;
- enable protection only when prerequisites are satisfied;
- use an appropriate TPM-backed protector for normal boot;
- ensure a recovery protector exists;
- never write the recovery password to normal logs or the repository;
- verify that protection is active after applying the change.

Firmware, Secure Boot, TPM, or boot-chain changes may require temporarily
suspending BitLocker. The recovery key must be stored independently of the PC.

## Windows settings contract

Only individually selected settings belong in the project. Every managed
setting should eventually document:

- desired state;
- current/default state where known;
- how it is detected;
- how it is applied;
- whether elevation is required;
- whether sign-out/restart/reboot is required;
- how the change is reversed.

The initial settings work should focus on practical Windows behavior and gaming,
not chasing a minimum process count.

Candidate areas include Explorer preferences, Windows suggestions/advertising,
startup behavior, gaming-related Windows settings, power behavior, and selected
privacy settings. Each item must be reviewed before implementation.

## Packages

Package decisions live in [PACKAGES.md](PACKAGES.md). Package installation must
use exact verified identifiers where possible and must not use fuzzy matching to
install similarly named software.

Existing installations should be detected and preserved. The initial project
does not automatically uninstall unrelated software.

## Configuration

Chezmoi is the configuration owner. `win-setup` may install Chezmoi, determine
whether it is initialized, and invoke the appropriate Chezmoi workflow. It
should not grow a second template system or understand the internal format of
every application config.

## Non-goals for the first usable version

The first usable version does not need to manage:

- WSL distributions;
- Fedora provisioning;
- Docker;
- development language runtimes;
- development databases;
- a generalized desired-state engine;
- automatic disk repartitioning or formatting;
- game installation itself;
- backup orchestration;
- every possible Windows optimization.

WSL and development tooling can be added later if Windows becomes more than the
gaming/secondary environment.

## Verification

A successful setup should eventually verify at minimum:

- Windows edition/build is expected;
- Secure Boot is enabled;
- TPM is ready;
- BitLocker protects the system volume;
- Windows Update, Defender, and App Installer remain functional;
- selected gaming prerequisites are present;
- selected launchers are installed;
- Chezmoi is installed and configuration can be applied;
- a representative game can run normally.
