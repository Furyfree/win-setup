# Remaining Windows checks

- Run bootstrap on a fresh Windows install; interrupt a download and verify
  that the existing executable survives. Check the elevation prompt.
- Run `status` and `apply`, reboot when requested, then repeat `apply`.
  Verify package detection failures and reboot-before-install reporting.
- Test Fedora setup with another WSL distribution already installed. Verify
  that its configuration survives, Fedora becomes default, systemd works,
  and a non-root user can finish provisioning with existing Chezmoi files.
- Check self-update while the installed executable is running.
- Verify BitLocker protection and the recovery key stored in 1Password,
  Windows security updates, the dual-boot clock and screen locking.
