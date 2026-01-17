# Project Context: Priscilu_Origins_v2

## Overview
This project is a server-side mod for **Single Player Tarkov (SPT)** version 4.0.11+.
It adds a custom trader named **Priscilu** with a large assortment of items and a custom avatar.

## Key Components
- **PrisciluOrigins.cs**: Main plugin class. Implements `IOnLoad` to register the trader at server startup.
- **AddCustomTraderHelper.cs**: Helper class for interacting with SPT's `DatabaseService` and `LocaleService` to inject trader data.
- **Data/**:
    - `base.json`: Base trader configuration (ID, name, location).
    - `assort.json`: Trader inventory/assortment.
    - `Priscilu_Origins.jpg`: Trader avatar.

## Development Workflow
- **Language**: C# (.NET 9.0)
- **Build**: `dotnet build -c Release`
- **Output**: `bin/Release/Priscilu_Origins/`
- **Installation**: Copy output contents to `<SPT_ROOT>/user/mods/Priscilu_Origins/`.

## Agent Instructions
- Use the `SPT Server Mod Development` skill for reference on patterns.
- Ensure `assort.json` matches item template IDs exactly.
- When modifying the trader, ensure `AddCustomTraderHelper` methods are updated if the logic changes.
