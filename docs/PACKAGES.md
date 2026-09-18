# Packages

This file records package ownership and the intended Windows software baseline.
Exact identifiers and current availability must be revalidated when package
installation is implemented.

The first priority is gaming. Development/WSL packages are intentionally
deferred.

## Rules

- Prefer WinGet when there is a verified official or appropriate package.
- Use Microsoft Store when the selected app is distributed there.
- Use an official vendor installer when WinGet/Store is not appropriate.
- Detect an existing compatible installation before installing.
- Do not install by fuzzy package-name matching.
- Do not automatically uninstall unrelated existing software.
- Authentication, license activation, and account setup remain interactive
  unless there is a safe supported mechanism.
- A package being listed here does not imply it should run at Windows login.

## Gaming baseline

| Application | Source | Identifier / route | Notes |
| --- | --- | --- | --- |
| Steam | WinGet | `Valve.Steam` | Core gaming launcher |
| Battle.net | WinGet | `Blizzard.BattleNet` | Required for Blizzard games |
| Prism Launcher | WinGet | `PrismLauncher.PrismLauncher` | Minecraft launcher; let Prism select/download suitable Java per instance |
| WowUp-CF | WinGet | `WowUp.CF` | Configure after WoW paths exist |
| Visual C++ Redistributable x64 | WinGet | `Microsoft.VCRedist.2015+.x64` | Common game/application runtime |
| Visual C++ Redistributable x86 | WinGet | `Microsoft.VCRedist.2015+.x86` | Required by many 32-bit game components |
| NVIDIA driver | Vendor | NVIDIA-supported driver route | Do not replace with a third-party driver updater |

Additional game launchers such as EA app, Epic Games Launcher, and Ubisoft
Connect are opt-in later. They should not be part of the first fresh baseline
unless a game actually needs them.

## Core Windows utilities

| Application | Source | Identifier / route | Intended owner |
| --- | --- | --- | --- |
| PowerShell 7 | WinGet | `Microsoft.PowerShell` | Package: win-setup, config: Chezmoi |
| Windows Terminal | WinGet / Windows | `Microsoft.WindowsTerminal` | Package: win-setup, config: Chezmoi |
| Git for Windows | WinGet | `Git.Git` | Package: win-setup, config: Chezmoi |
| GitHub CLI | WinGet | `GitHub.cli` | win-setup |
| Chezmoi | WinGet | `twpayne.chezmoi` | win-setup bootstrap |
| 1Password | WinGet | `AgileBits.1Password` | win-setup |
| 1Password CLI | WinGet | `AgileBits.1Password.CLI` | win-setup |
| Tailscale | WinGet | `Tailscale.Tailscale` | win-setup |
| 7-Zip | WinGet | `7zip.7zip` | win-setup |
| Everything | WinGet | `voidtools.Everything` | win-setup |
| PowerToys | WinGet | `Microsoft.PowerToys` | Package: win-setup; selected config may live in Chezmoi |
| GlazeWM | WinGet | `glzr-io.glazewm` | Package: win-setup, config: Chezmoi |
| Zebar | WinGet | `glzr-io.zebar` | Package: win-setup, config: Chezmoi |
| Windhawk | WinGet | `RamenSoftware.Windhawk` | Install only; no default mod bundle initially |

UniGetUI may be useful as a graphical package-review interface, but it is not
required for the automated baseline.

## Desktop and communication candidates

These were selected in the earlier planning work but are secondary to the
gaming baseline.

| Application | Source | Identifier / route |
| --- | --- | --- |
| Signal | WinGet | `OpenWhisperSystems.Signal` |
| Vesktop | WinGet | `Vencord.Vesktop` |
| Obsidian | WinGet | `Obsidian.Obsidian` |
| Spotify | WinGet | `Spotify.Spotify` |
| SumatraPDF | WinGet | `SumatraPDF.SumatraPDF` |
| mpv | WinGet | `shinchiro.mpv` |
| Paint.NET | WinGet | `dotPDN.PaintDotNet` |
| OBS Studio | WinGet | `OBSProject.OBSStudio` |
| EarTrumpet | WinGet | `File-New-Project.EarTrumpet` |
| LocalSend | WinGet | `LocalSend.LocalSend` |
| NAPS2 | WinGet | `Cyanfish.NAPS2` |
| Zed | WinGet | `ZedIndustries.Zed` |
| Claude | WinGet | `Anthropic.Claude` |
| ChatGPT | Microsoft Store | `9PLM9XGG6VKS` |

Brave Origin stable and Microsoft 365 require their appropriate official
distribution/licensing routes rather than guessing a similarly named package.

## Startup policy

The baseline should avoid starting every installed application automatically.

Likely continuous desktop components, once selected and configured:

- GlazeWM.
- Zebar.
- selected PowerToys modules.
- Everything, if configured for continuous indexing.
- EarTrumpet.
- 1Password when needed for integration.
- Tailscale through its normal service behavior.

Keep launchers and heavyweight user applications such as Steam, Battle.net,
OBS, Spotify, UniGetUI, and AI desktop apps out of automatic login startup by
default unless explicitly chosen otherwise.

## Deferred development packages

The first gaming-ready version does not need to install Windows-native Node,
Python, Go, Java, Rust, .NET SDKs, CMake/Ninja, Visual Studio, WSL, Docker
Desktop, or a full Linux CLI collection.

Those can be added later when a concrete Windows development workflow requires
them.
