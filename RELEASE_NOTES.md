# Release Notes - v6.4.0

## 🚀 Changes

### Configuration Update
- **New Default Settings**: Updated the bundled configuration to match recommended defaults.
  - `MinLevel`: 5
  - `TraderRefreshMin`: 1800 (30 min)
  - `TraderRefreshMax`: 3600 (60 min)
  - `DebugLogging`: false
  - `UnlimitedStock`: true

### Improvements (Cumulative from v6.3.x)
- **Code Optimization**: Removed unused development tools (`Inspector`).
- **Build Cleanup**: Release artifacts no longer contain unnecessary `.pdb`, `.deps.json`, or `.cs` source files.
- **Trader Logic**: Improved stock management and "Live" unlock logic.

## 📦 Installation
1. Download the release.
2. Copy the `Priscilu_Origins_v2` folder into your `<SPT-Server>/user/mods/` directory.
3. Start the server.

---
*Targeting SPT 4.0.11+*
