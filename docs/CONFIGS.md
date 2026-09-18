# Configuration

Chezmoi owns user configuration; win-setup owns the machine. The goal is to
reuse the existing cross-platform dotfiles without duplicating them.

## Boundary

win-setup:

- installs Chezmoi when it is missing;
- detects whether Chezmoi is initialized;
- runs `chezmoi apply` when it is;
- otherwise prints the one-time `chezmoi init --apply` command;
- reports configuration state.

Chezmoi owns the PowerShell profile, Windows Terminal, Git, Zed, GlazeWM,
Zebar, Starship, and other stable text-based per-user configuration, shared
with Linux/macOS through templates where that makes sense.

Chezmoi must not own BitLocker, Secure Boot, TPM state, package installation,
Windows Update, Defender/Firewall state, drivers, or secrets and recovery keys.

## Flow

```text
win-setup apply
  -> install chezmoi if missing
  -> chezmoi apply if initialized
  -> print the init command if not
```

Chezmoi stays independently usable (`chezmoi status`, `chezmoi diff`,
`chezmoi apply`). Machine and profile data belong to the dotfiles repository;
win-setup does not invent a second profile model.

WSL is a separate Linux home. Run `chezmoi init --apply` inside Fedora when
it is set up; win-setup does not configure the WSL home.

## Secrets

Never commit recovery keys, tokens, passwords, session data, SSH keys, or
machine authentication state. Secret-manager integration belongs to Chezmoi.
