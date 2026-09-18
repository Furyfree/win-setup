# win-setup

Small Windows 11 install utility. `win-setup status` reports machine state;
`win-setup apply` installs selected apps, applies selected settings, enables
BitLocker, and hands off to Chezmoi.

This must stay small. No state database, no plan/apply engine, no profiles, no
resource graph, no DI container, no separate Core/Infrastructure assemblies.
It is not a Windows Nimbus.

## Layout

- `src/WinSetup/` - one console app.
- `tests/WinSetup.Tests/` - pure-logic tests, runnable on Linux.
- `docs/` - specification, packages, configs, installation history.
- `bootstrap.ps1` - fresh-install entry point.

## Commands

- `dotnet build` / `dotnet test` - validation, works on Fedora.
- `dotnet publish src/WinSetup -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` - release exe.
- `win-setup status` -> `win-setup.exe status` on Windows.
- `win-setup snapshot` writes a read-only machine-state bundle for review.

## Rules

- Packages and settings live as C# records in `Packages.cs` and `Settings.cs`.
  Do not add JSON/YAML/TOML config files unless editing on the Windows box
  without a rebuild becomes a real need.
- `apply` is idempotent: detect, act, re-detect. The machine is the state.
- Exact WinGet IDs only. Never fuzzy match. Never uninstall.
- Registry changes go through `Microsoft.Win32.Registry`. PowerShell is only
  used where cmdlets are the supported interface (BitLocker, TPM, Secure Boot)
  and never with interpolated data in the command text.
- Never write secrets or recovery keys to console or logs. The BitLocker
  recovery key is written to a file and the path is printed in the summary.
- Tests cover pure logic only: exit-code mapping, JSON parsing, idempotency
  checks. No live Windows needed in CI.
- Keep `docs/` honest: if a behavior changes, update the doc that claims it.

## Researching settings and packages

Use these as reference sources when figuring out how to implement a setting or
what a package is called:

- WinUtil (Chris Titus Tech): https://github.com/ChrisTitusTech/winutil
- Winhance: https://github.com/memstechtips/Winhance

Cross-check the registry hive/key/name/value against those projects. Verify a
package ID with `winget search --id <id> --exact` on the real machine before
adding it. Presence in `docs/PACKAGES.md` does not mean it is implemented.

## Security

- Never commit recovery keys, tokens, passwords, SSH keys, or session data.
- BitLocker: a recovery key file on the encrypted volume is useless if the
  machine will not boot; the summary tells the user to move it into 1Password.
- `docs/SPEC.md` is the authoritative behavior contract.
