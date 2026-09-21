# win-setup

Personal Windows 11 Pro utility: one C# console app and one test project.
Keep the current structure; the machine supplies state.

## Map

- `src/WinSetup/Program.cs`: command dispatch and apply logging.
- `src/WinSetup/Commands.cs`: status and setup order, summaries and rechecks.
- `src/WinSetup/Packages.cs`, `Settings.cs`, `Power.cs`: selected apps and
  settings, detection and writes. Verify new WinGet IDs on Windows.
- `src/WinSetup/Wsl.cs`: Fedora installation and Linux user provisioning.
- `src/WinSetup/Update.cs`, `bootstrap.ps1`: self-update and first install.
- `src/WinSetup/BitLocker.cs`, `Checks.cs`: encryption and host checks.
- `src/WinSetup/Chezmoi.cs`: Windows dotfile handoff.
- `src/WinSetup/Runner.cs`, `PowerShell.cs`, `Paths.cs`: process boundaries.
- `src/WinSetup/Snapshot.cs`, `Notify.cs`: state exports and desktop refresh.
- `tests/WinSetup.Tests/WinSetup.Tests.cs`: logic and isolated regressions.
- `docs/INSTALLATION.md`: media preparation; `docs/TASKS.md`: remaining work.
- `docs/local/`: ignored local reference material; never publish it.

## What must not break

Use exact WinGet IDs. Preserve installed apps and existing user files.
Detect, apply, then recheck; report errors instead of assuming absence.
Preserve Secure Boot, TPM, Defender, Firewall, UAC, Windows Update and Store.
Use native registry APIs and supported PowerShell cmdlets. Pass user data as
arguments or environment variables, never interpolated PowerShell code.
Keep recovery passwords out of output and logs; save them to the key file
and remind the user to store it independently of the encrypted PC.

Cross-check new Windows settings against
[WinUtil](https://github.com/ChrisTitusTech/winutil) and
[Winhance](https://github.com/memstechtips/Winhance).

## Verify

Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes` and
`markdownlint README.md AGENTS.md docs/*.md`. `CLAUDE.md` is only an include.
Tests use temporary files and fake external commands; they cannot establish
Windows integration. Never run bootstrap or live `apply` as a routine test.
