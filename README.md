# win-setup

Personal Windows 11 Pro setup utility. It installs apps through WinGet,
changes Windows settings, enables BitLocker and provisions Fedora WSL.
Chezmoi manages application configuration in each system's user home.

## Install

After Windows setup and driver installation, run in PowerShell:

```powershell
irm https://raw.githubusercontent.com/Furyfree/win-setup/main/bootstrap.ps1 | iex
```

Bootstrap downloads the latest release to
`%LOCALAPPDATA%\Programs\win-setup`, adds it to your PATH, runs `status`,
then requests elevation for `apply`. Open a new terminal to use the commands.
See [Installation](INSTALLATION.md) for preparing the Windows media.

## Use

```powershell
win-setup status    # inspect settings and packages; 0 clean, 1 drift/error
win-setup apply     # elevated terminal; check for updates, then apply setup
win-setup snapshot  # export machine state to ~/win-setup-snapshot
win-setup version
```

`apply` preserves installed packages and checks completed installations.
After a requested reboot, rerun it to finish setup. Logs are in
`%LOCALAPPDATA%\win-setup\apply.log`. Snapshot exports may contain personal
paths and machine details; review them before sharing.

Move `%USERPROFILE%\bitlocker-recovery-<host>.txt` into 1Password from
another device. A recovery key stored only on the encrypted PC cannot help
when that PC fails to boot.

See [Packages.cs](src/WinSetup/Packages.cs) for installed apps and
[Settings.cs](src/WinSetup/Settings.cs) for Windows preferences.

## Chezmoi and WSL

Initialize Windows dotfiles once in a normal terminal with Git and GitHub
authentication available:

```powershell
chezmoi init --apply https://github.com/Furyfree/dotfiles.git
```

Later runs of `apply` invoke `chezmoi apply`. Chezmoi also works independently.

WSL setup may need a reboot. After Fedora installs, launch `FedoraLinux-44`,
finish creating its non-root default user, then rerun `win-setup apply`.
Setup enables systemd, selects Fedora as the default distribution, installs
chezmoi/git/zsh/fastfetch and applies the development dotfiles. Existing
Chezmoi configuration is preserved; an uninitialized local source needs
manual review. Dotfile scripts are skipped during provisioning: install
mise and run `chezmoi apply` inside Fedora to finish the Linux tools.

## Build and check

Use the .NET SDK selected by `global.json`:

```sh
dotnet build
dotnet test
dotnet format --verify-no-changes
markdownlint README.md AGENTS.md INSTALLATION.md TASKS.md
dotnet publish src/WinSetup -c Release -r win-x64 \
    --self-contained -p:PublishSingleFile=true
```

The shell fixtures run on Linux. Real Windows checks remain in
[TASKS.md](TASKS.md).
