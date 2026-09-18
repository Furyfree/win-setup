# Configuration

Chezmoi is the configuration owner for `win-setup`.

The goal is to reuse the existing cross-platform dotfiles where that makes
sense while keeping Windows machine setup in C#.

## Boundary

### win-setup

`win-setup` is responsible for:

- installing Chezmoi when it is missing;
- detecting whether Chezmoi is initialized;
- supplying/choosing the intended machine context when needed;
- invoking the normal Chezmoi workflow;
- reporting whether configuration is clean/applied.

It should not parse and rewrite every managed application config itself.

### Chezmoi

Chezmoi is responsible for user configuration files, templates, and
cross-platform differences.

Likely Windows-managed configs include:

- PowerShell profile.
- Windows Terminal settings.
- Git config.
- Zed settings/keymap.
- GlazeWM configuration.
- Zebar configuration.
- Starship configuration if used on Windows.
- other application files that are stable, text-based, and appropriate to keep
  in dotfiles.

Some settings may remain shared with Linux/macOS through templates. Others will
be Windows-only.

## What does not belong in Chezmoi

Chezmoi should not own machine-level security or system provisioning such as:

- enabling BitLocker;
- Secure Boot or TPM state;
- installing Windows packages;
- Windows Update;
- Defender/Firewall state;
- drivers;
- broad Windows registry/system settings whose lifecycle belongs to
  `win-setup`;
- secrets, recovery keys, authentication caches, or private key material.

A user-level registry preference can be Chezmoi-managed only when that is
clearly the simplest and most maintainable representation. Otherwise selected
Windows settings should stay with `win-setup`.

## Cross-platform approach

Prefer one logical config with platform conditionals when an application is
genuinely shared.

Examples:

- Git identity and aliases can share a template while machine/work context stays
  conditional.
- Zed can share most editor preferences with path/platform-specific differences
  templated.
- Starship can generally remain shared.
- PowerShell is Windows-specific even if shell concepts overlap with Linux
  configuration.
- GlazeWM/Zebar are Windows-specific.

Do not copy Linux desktop/session configuration into Windows or later WSL
environments simply because Chezmoi can technically deploy it.

## Machine/profile data

The exact Chezmoi data model should be decided in the dotfiles repository after
its current templates are inspected. `win-setup` should not invent a second
profile model.

At minimum the Windows setup may need to distinguish:

- operating system/platform;
- machine role/name where relevant;
- personal vs work context if applicable.

## Apply flow

The intended high-level flow is:

```text
win-setup
  -> ensure chezmoi exists
  -> ensure source/configuration is initialized
  -> show/check configuration state
  -> apply chezmoi
  -> verify/report result
```

Chezmoi remains independently usable. A user must still be able to run normal
commands such as:

```text
chezmoi status
chezmoi diff
chezmoi apply
```

without going through `win-setup`.

## Secrets

Never commit:

- BitLocker recovery passwords.
- API tokens.
- account passwords.
- session cookies/caches.
- private SSH key material.
- machine-bound authentication state.

Existing secret-manager integrations can be used through Chezmoi where
appropriate, but `win-setup` should not create a new secret store.
