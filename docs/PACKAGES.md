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
| NVIDIA App | Microsoft Store | `XP8CLZL93F5Z4P` | Install through WinGet using the Microsoft Store source |

Additional game launchers such as EA app, Epic Games Launcher, and Ubisoft
Connect are opt-in later. They should not be part of the first fresh baseline
unless a game actually needs them.

## Core Windows utilities

| Application | Source | Identifier / route | Intended owner |
| --- | --- | --- | --- |
| PowerShell 7 | WinGet | `Microsoft.PowerShell` | Package: win-setup, config: Chezmoi |
| Windows Terminal | WinGet / Windows | `Microsoft.WindowsTerminal` | Package: win-setup, config: Chezmoi |
| VSCodium | WinGet | `VSCodium.VSCodium` | Package: win-setup, config/extensions: Chezmoi |
| Zed | WinGet | `ZedIndustries.Zed` | Package: win-setup, config: Chezmoi |
| Brave Origin Nightly | WinGet | `Brave.BraveOrigin.Nightly` | Only winget channel available for Origin |
| Signal | WinGet | `OpenWhisperSystems.Signal` | win-setup |
| Git for Windows | WinGet | `Git.Git` | Package: win-setup, config: Chezmoi |
| GitHub CLI | WinGet | `GitHub.cli` | win-setup |
| Chezmoi | WinGet | `twpayne.chezmoi` | win-setup bootstrap |
| 1Password | WinGet | `AgileBits.1Password` | win-setup |
| 1Password CLI | WinGet | `AgileBits.1Password.CLI` | win-setup |
| Tailscale | WinGet | `Tailscale.Tailscale` | win-setup |
| 7-Zip | WinGet | `7zip.7zip` | win-setup |
| Everything | WinGet | `voidtools.Everything` | win-setup |
| EverythingToolbar | WinGet | `srwi.EverythingToolbar.Launcher` | Requires full Everything running |
| .NET Desktop Runtime 8 | WinGet | `Microsoft.DotNet.DesktopRuntime.8` | Required by EverythingToolbar |
| PowerToys | WinGet | `Microsoft.PowerToys` | Package: win-setup; selected config may live in Chezmoi |
| GlazeWM | WinGet | `glzr-io.glazewm` | Package: win-setup, config: Chezmoi |
| Zebar | WinGet | `glzr-io.zebar` | Package: win-setup, config: Chezmoi |
| Windhawk | WinGet | `RamenSoftware.Windhawk` | Deferred; commented out in `Packages.cs` |

UniGetUI may be useful as a graphical package-review interface, but it is not
required for the automated baseline.

GlazeWM and Zebar are deferred until their Chezmoi configuration exists; the
package rows are commented out in `Packages.cs` for now.

## Desktop and communication candidates

These were selected in the earlier planning work but are secondary to the
gaming baseline.

| Application | Source | Identifier / route |
| --- | --- | --- |
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
| Claude | WinGet | `Anthropic.Claude` |
| ChatGPT | Microsoft Store | `9PLM9XGG6VKS` |

Brave Origin has no stable winget package; the Nightly channel is used.
Microsoft 365 requires its official distribution/licensing route.

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
