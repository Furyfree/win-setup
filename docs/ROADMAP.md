# Roadmap

The roadmap intentionally keeps the project smaller than Nimbus. Each milestone
should produce a useful Windows machine without requiring later milestones.

## v0.1 - Foundation

Goal: establish the small C# utility and documentation without implementing a
configuration-management framework.

- [x] Initialize .NET 10 solution and test project.
- [x] Split active project documentation by responsibility.
- [ ] Add a minimal `win-setup status` command.
- [ ] Report Windows edition/build and basic host information.
- [ ] Report elevation state.
- [ ] Add basic CI/build/test validation.
- [ ] Add the public bootstrap script and document the final one-line entry
      point.

## v0.2 - Secure Windows

Goal: make the fresh installation safe to keep real data on.

- [ ] Detect Secure Boot state.
- [ ] Detect TPM availability/readiness.
- [ ] Detect BitLocker state on the Windows system volume.
- [ ] Enable BitLocker when selected and not already active.
- [ ] Ensure TPM-backed normal unlock.
- [ ] Ensure a recovery protector exists without leaking it into logs or Git.
- [ ] Verify Windows Defender, Firewall, Windows Update, and App Installer remain
      usable.
- [ ] Document firmware/update workflow when BitLocker suspension is required.

## v0.3 - Gaming-ready baseline

Goal: make the machine immediately useful for Windows gaming.

- [ ] Research and select the small set of Windows settings that matter.
- [ ] Verify/install current GPU driver through the appropriate vendor route.
- [ ] Verify/install Visual C++ x64/x86 redistributables.
- [ ] Install/verify Steam.
- [ ] Install/verify Battle.net.
- [ ] Install/verify Prism Launcher.
- [ ] Install/verify WowUp-CF.
- [ ] Keep unnecessary launchers out of startup.
- [ ] Test a representative game, including fullscreen/window-management
      behavior and anti-cheat compatibility where relevant.

## v0.4 - Personal desktop configuration

Goal: make Windows comfortable without duplicating Chezmoi.

- [ ] Install/verify Chezmoi.
- [ ] Define the Windows machine/profile data needed by the existing dotfiles.
- [ ] Apply Windows Terminal configuration through Chezmoi.
- [ ] Apply PowerShell profile through Chezmoi.
- [ ] Apply Git configuration through Chezmoi.
- [ ] Apply Zed configuration through Chezmoi.
- [ ] Apply GlazeWM/Zebar configuration through Chezmoi.
- [ ] Install and configure the selected desktop utilities from PACKAGES.md.
- [ ] Verify rerunning `win-setup apply` leaves an already-correct machine
      unchanged.

## Later

Only add these once the gaming/desktop setup is stable and there is a real need:

- WSL.
- Fedora inside WSL.
- Docker/development containers.
- Linux development runtimes and tooling.
- Zed WSL remoting.
- Additional native Windows development tooling.
- Broader diagnostics or media tooling.
- More advanced setup/resume/update behavior.
