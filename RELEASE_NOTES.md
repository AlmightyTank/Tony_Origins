# Release Notes - v6.3.2

## 🚀 Fixes & Improvements

### Build & Deployment
- **Fixed Build Process**: Added missing `package.json` to ensure mod compatibility and build success.
- **Optimized Release Artifacts**: Modified project configuration to exclude unnecessary files from the release folder:
  - Source code files (`.cs`) in `config/`
  - Debug symbols (`.pdb`)
  - Dependency manifests (`.deps.json`)
  - Web SDK assets (`staticwebassets`)
  - **Result**: A cleaner, smaller release package containing only essential files.

### Code Cleanup
- **Removed Inspector**: Deleted unused `Inspector/` tool from the project source.
- **Refactoring**: Renamed `TraderUnlockCallback.cs` to `TraderUnlockService.cs` to properly reflect its class name and purpose.

### Features (From previous 6.3.x updates)
- **Live Trader Unlock**: "Live" level checking service (every 10s) to unlock the trader immediately when reaching the required level, without needing a restart.
- **Stock Manipulation**: Randomization of stock availability and unlimited stock options.
- **Insurance Coefficient**: Configurable insurance price multipliers.

## 📦 Installation
1. Download the release.
2. Copy the `Priscilu_Origins` folder into your `<SPT-Server>/user/mods/` directory.
3. Start the server.

---
*Targeting SPT 4.0.11+*
