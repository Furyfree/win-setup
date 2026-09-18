# Tasks

This is the current work queue. Long-term sequencing belongs in
[ROADMAP.md](ROADMAP.md).

## Documentation

- [x] Preserve the original September Windows/WSL planning document.
- [x] Define the smaller C# scope in [SPEC.md](SPEC.md).
- [x] Move package decisions into [PACKAGES.md](PACKAGES.md).
- [x] Define the Chezmoi boundary in [CONFIGS.md](CONFIGS.md).
- [x] Record the actual installation procedure in
      [INSTALLATION.md](INSTALLATION.md).
- [x] Record the post-CTT registry/service/appx baseline (kept in the personal
      docs repository, not here).
- [ ] Continue replacing assumptions from the old planning document with facts
      from the new installation.

## First implementation

- [x] Decide the exact CLI parsing approach once `status` is implemented.
- [x] Implement `win-setup status`.
- [x] Detect Windows edition/build.
- [x] Detect administrator/elevation state.
- [x] Detect Secure Boot.
- [x] Detect TPM.
- [x] Detect BitLocker system-volume state.
- [x] Detect `winget`.
- [x] Detect `chezmoi`.
- [x] Add tests for logic that does not require a live Windows host.

## Security

- [ ] Research the cleanest Windows 11 Pro BitLocker implementation from C#.
- [ ] Define recovery-key handling before enabling BitLocker automatically.
- [ ] Enable and verify BitLocker.
- [ ] Record the recovery procedure in the docs.
- [ ] Verify Defender, Firewall, Windows Update, and App Installer after the
      debloated install.

## Windows settings

- [ ] Inventory the current settings produced by the Win11 Creator installation.
- [ ] Compare individual settings against WinUtil, Winhance, Sophia Script, and
      Win11Debloat as research sources.
- [ ] Select only settings we actually want.
- [ ] Document every selected setting with detection, apply, undo, and reboot
      behavior.
- [ ] Keep broad tweak/debloat presets out of the baseline.

## Gaming

- [ ] Verify NVIDIA driver state.
- [ ] Verify Visual C++ x64/x86 runtime state.
- [ ] Install/verify Steam.
- [ ] Install/verify Battle.net.
- [ ] Install/verify Prism Launcher.
- [ ] Install/verify WowUp-CF.
- [ ] Review startup applications after package installation.
- [ ] Verify one representative game.

## Chezmoi

- [ ] Inspect the existing dotfiles for Windows support before changing them.
- [ ] Define Windows machine/profile data.
- [ ] Decide which currently Linux-only configs should become cross-platform.
- [ ] Add Windows Terminal config.
- [ ] Add PowerShell profile.
- [ ] Add Git config.
- [ ] Add Zed config.
- [ ] Add GlazeWM/Zebar config if selected.
- [ ] Make `win-setup` bootstrap/invoke Chezmoi without duplicating its work.

## Bootstrap

- [ ] Add `bootstrap.ps1`.
- [ ] Keep it small: obtain the repository/tool, build or fetch the executable,
      and hand off to `win-setup`.
- [ ] Make the public one-liner in README functional before changing the
      repository visibility.
