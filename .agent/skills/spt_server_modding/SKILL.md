---
name: SPT Server Mod Development
description: A guide for developing server-side mods for SPT (Single Player Tarkov) using C# and the BepInEx/SPT server API.
---

# SPT Server Mod Development (C#)

This skill covers the creation and maintenance of server-side mods for SPT.

## Project Structure
A standard SPT server mod consists of:
- **.csproj**: Targets .NET 9.0 (for SPT 4.x). Includes references to `SPTarkov.Server.Core.dll` and other SPT dependencies.
- **Mod Class**: A class inheriting from `IOnLoad` (or `IPreAkiLoad` / `IPostAkiLoad`) and decorated with `[Injectable]`.
- **Metadata**: A record inheriting `AbstractModMetadata`.
- **Data/**: JSON files for trader base, assorts, quests, etc.

## Dependencies
Ensure your project references the necessary SPT server DLLs found in `SPT_Data/Server/`:
- `SPTarkov.Server.Core`
- `SPTarkov.Common`

## Basic Mod Template

```csharp
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace MyModName;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.author.modname";
    public override string Name { get; init; } = "My Mod Name";
    public override string Author { get; init; } = "Author";
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class MyMod : IOnLoad
{
    private readonly ConfigServer _configServer;
    
    public MyMod(ConfigServer configServer)
    {
        _configServer = configServer;
    }

    public Task OnLoad()
    {
        // Mod logic here
        return Task.CompletedTask;
    }
}
```

## Creating Custom Traders

### 1. Data Files
- `base.json`: Trader details (ID, nickname, avatar, etc.).
- `assort.json`: Items the trader sells.

### 2. Helper Pattern
Use a helper service to interact with the database.

Common operations:
- **Add to Database**: `databaseService.GetTables().Traders.TryAdd(traderId, traderData)`
- **Add Locales**: access `databaseService.GetTables().Locales.Global` and add transformers.
- **Set Update Time**: Update `TraderConfig`.
- **Load Images**: Use `ImageRouter.AddRoute`.

### 3. Example Helper Usage (from Priscilu_Origins)
```csharp
public void AddTraderToLocales(TraderBase baseJson, string firstName, string description)
{
    var locales = databaseService.GetTables().Locales.Global;
    foreach (var (_, localeKvP) in locales)
    {
        localeKvP.AddTransformer(data =>
        {
            data.TryAdd($"{baseJson.Id} FullName", baseJson.Name);
            data.TryAdd($"{baseJson.Id} FirstName", firstName);
            return data;
        });
    }
}
```

## Build & Deploy
1. **Build**: `dotnet build -c Release`
2. **Deploy**: Copy the output from `bin/Release/ModName/` to `<SPT>/user/mods/ModName/`.
   - Ensure `mod.json` (if used) or the DLL structure is correct.
   - For C# mods, the DLL is usually loaded by the BepInEx/SPT loader.

## Best Practices
- Use Dependency Injection (`[Injectable]`).
- Keep logic separated into helpers/services.
- Use `ModHelper` to load local JSON files.
- logging: Inject `ISptLogger<MyMod>`.
