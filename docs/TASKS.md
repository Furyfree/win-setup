# Status and tasks

## Done

- C# utility with `status`, `apply`, `snapshot`, and pure-logic tests.
- Settings and package list built from the CTT install baseline and
  cross-checked against WinUtil and Winhance.
- BitLocker with TPM protector and recovery-key file.
- Power, update pin, dual-boot clock, and lock-on-screen-off settings.
- Chezmoi install and handoff; `bootstrap.ps1` for releases.

## Next

- [ ] First live run: `status`, then `apply`.
- [ ] Verify: packages installed, BitLocker protected with the key in
      1Password, clock stable after booting Fedora, screen locks when the
      display turns off, Windows Update still delivers security updates.
- [ ] Make the repository public and cut the first release so bootstrap works.

## Later

- GlazeWM and Zebar packages and Chezmoi configuration.
- Windows Terminal, PowerShell, and Zed configs in the dotfiles repository.
- WSL, Docker, and development tooling only if Windows becomes a dev machine.
