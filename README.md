# win-setup

Small Windows 11 setup utility for this workstation. One executable, exact
WinGet IDs, no state database. Chezmoi owns dotfiles; win-setup owns the machine.

## Commands

| Command | Does |
| --- | --- |
| `win-setup status` | Read-only drift check. Exit 0 clean, 1 drift. |
| `win-setup apply` | Needs elevation. Applies settings, installs packages, enables BitLocker, runs Chezmoi. Safe to rerun. |
| `win-setup snapshot` | Read-only machine-state bundle for review. |

Every change is detect -> act -> recheck, so a rerun only writes what differs.

## What apply does

Before anything else, `apply` checks the latest release and updates itself if a
newer one exists (v0.1.6 and later).

1. **Settings** - taskbar, snapping, Explorer, ads/suggestions, gaming, mouse,
   power, dual-boot clock, feature-update pin. Already-correct values are
   skipped.
2. **Packages** - exact IDs from `Packages.cs` via WinGet. Never fuzzy matches,
   never uninstalls, never upgrades an existing install.
3. **BitLocker** - TPM protector plus a recovery password written to
   `%USERPROFILE%\bitlocker-recovery-<host>.txt`. Move that file into 1Password
   from another device: a key that only exists on the encrypted disk is useless
   if the machine will not boot.
4. **Chezmoi** - installs it if missing. If the dotfiles are already
   initialized it runs `chezmoi apply`; otherwise it prints the one-time
   bootstrap. Run this once in a normal terminal (Git and GitHub
   authentication must be available), answer its prompts, and every later
   `apply` keeps the dotfiles updated:

   ```powershell
   chezmoi init --apply https://github.com/Furyfree/dotfiles.git
   ```

   Chezmoi stays usable on its own: `chezmoi status`, `chezmoi diff`, and
   `chezmoi apply` work directly.

## Bootstrap

Fresh install entry point:

```powershell
irm https://raw.githubusercontent.com/Furyfree/win-setup/main/bootstrap.ps1 | iex
```

It downloads the latest release to `%LOCALAPPDATA%\Programs\win-setup`, adds
that directory to the user PATH, runs `status`, then requests elevation for
`apply`. Open a new terminal afterwards and `win-setup status|apply|snapshot`
work directly.

## Build and test

```bash
dotnet test
dotnet publish src/WinSetup -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Docs

- [Specification](docs/SPEC.md)
- [Packages](docs/PACKAGES.md)
- [Configuration](docs/CONFIGS.md)
- [Installation history](docs/INSTALLATION.md)
- [Tasks](docs/TASKS.md)
