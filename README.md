# win-setup

Personal Windows 11 setup for a secure, minimal, gaming-ready system.

The project is intentionally small. `win-setup` owns Windows system setup,
security, selected settings, and package installation. Chezmoi owns personal
application and dotfile configuration.

## Bootstrap

During development the repository is private:

```powershell
gh repo clone Furyfree/win-setup
```

Once the repository is public and the bootstrap script is implemented, the
intended fresh-install entry point is:

```powershell
irm https://raw.githubusercontent.com/Furyfree/win-setup/main/bootstrap.ps1 | iex
```

## Documentation

- [Specification](docs/SPEC.md)
- [Roadmap](docs/ROADMAP.md)
- [Tasks](docs/TASKS.md)
- [Packages](docs/PACKAGES.md)
- [Configuration](docs/CONFIGS.md)
- [Installation](docs/INSTALLATION.md)
- [Post-install snapshot (CTT)](docs/SNAPSHOT-2026-09-18.md)

The original Windows/WSL planning document is retained as
[historical planning](docs/Windows_Setup_Spec_2026-09-11.md) while useful
details are migrated into the active documentation.
