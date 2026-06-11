using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Collections; // Added for IDictionary support
using Path = System.IO.Path;
using Tony.config;

namespace Tony;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.amightytank.yetanothertradermod";
    public override string Name { get; init; } = "YetAnotherTraderMod";
    public override string Author { get; init; } = "AMightyTank | Based on PrisciluOrigins by Reis/Anigx";
    public override List<string>? Contributors { get; init; } = ["Reis", "Anigx"];
    public override SemanticVersioning.Version Version { get; init; } = new("0.0.1");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.11");
    public override List<string>? Incompatibilities { get; init; } = [];
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = null;
    public override string? Url { get; init; } = null;
    public override bool? IsBundleMod { get; init; } = false;
    public override string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class TonyMod(
    ModHelper modHelper,
    ImageRouter imageRouter,
    ConfigServer configServer,
    DatabaseServer databaseServer,
    AddCustomTraderHelper addCustomTraderHelper,
    TraderUnlockService traderUnlockService)
    : IOnLoad
{
    private readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();
    private readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();

    public Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        
        // [DEBUG LOG] Initialize Logger
        TonyLogger.Init(pathToMod);
        TonyLogger.Log("Mod OnLoad started.");

        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "data/base.json");
        var assort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "data/assort.json");
        var traderImagePath = Path.Combine(pathToMod, "data/Tony.jpg");

        // [NEW] Load Configuration
        var config = new TonyConfig(pathToMod, databaseServer);
        config.LoadOrGenerate(traderBase, assort);

        // [LOG] Set Debug Flag
        TonyLogger.IsDebugEnabled = config.Settings.DebugLogging;
        if (TonyLogger.IsDebugEnabled)
        {
             TonyLogger.LogDebug($"Debug Mode Enabled. Config Loaded.");
             TonyLogger.LogDebug($"  MinLevel: {config.Settings.MinLevel}");
             TonyLogger.LogDebug($"  UnlockedByDefault: {config.Settings.UnlockedByDefault}");
             TonyLogger.LogDebug($"  UnlimitedStock: {config.Settings.UnlimitedStock}");
             TonyLogger.LogDebug($"  RandomizeStock: {config.Settings.RandomizeStockAvailable} (Chance: {config.Settings.OutOfStockChance}%)");
             TonyLogger.LogDebug($"  PriceMultiplier: {config.Settings.PriceMultiplier}");
        }

        // [NEW] Apply Settings (Level & Unlock)
        traderBase.UnlockedByDefault = config.Settings.UnlockedByDefault;
        
        if (traderBase.LoyaltyLevels.Count > 0)
        {
            traderBase.LoyaltyLevels[0].MinLevel = config.Settings.MinLevel;
        }

        // [NEW] Apply Configurable Services
        // [FIX] Apply Insurance Coefficient to Loyalty Levels (Correct Place!)
        if (traderBase.LoyaltyLevels != null)
        {
            foreach (var level in traderBase.LoyaltyLevels)
            {
                try 
                {
                    // InsurancePriceCoefficient might be int? or double?
                    // We use reflection to set it safely
                    var prop = level.GetType().GetProperty("InsurancePriceCoefficient");
                    if (prop != null && prop.CanWrite)
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        object val = Convert.ChangeType(config.Settings.InsurancePriceCoef, targetType);
                        prop.SetValue(level, val);
                        TonyLogger.LogDebug($"[Insurance] Set Level {level.MinLevel} Coef to: {val}");
                    }
                    else
                    {
                         // Fallback to ExtensionData if property missing?
                         // But Inspector confirmed property exists.
                         TonyLogger.LogDebug($"[Insurance] Warning: InsurancePriceCoefficient property not found on LoyaltyLevel.");
                    }
                }
                catch (Exception ex)
                {
                    TonyLogger.Log($"[Insurance] Error settingcoef for level: {ex.Message}");
                }
            }
        }
        
        // [OLD] Global Insurance setting (likely ineffective but kept for safety/legacy if core uses it)
        if (traderBase.Insurance != null)
        {
             // Try valid ExtensionData injection just in case
             if (traderBase.Insurance.ExtensionData == null) traderBase.Insurance.ExtensionData = new Dictionary<string, object>();
             traderBase.Insurance.ExtensionData["insurance_price_coef"] = config.Settings.InsurancePriceCoef;
        }
        
        if (traderBase.Repair != null)
        {
            // Repair.Quality is double?, so we assign it directly without ToString()
            traderBase.Repair.Quality = config.Settings.RepairQuality;
        }

        if (!config.Settings.UnlockedByDefault)
        {
            TraderUnlockService.EnableLevelLock = true;
            TraderUnlockService.MinLevelRequired = config.Settings.MinLevel;
            traderUnlockService.OnLoad();
            TonyLogger.Log($"Level-based unlock enabled. Required level: {config.Settings.MinLevel}");
        }
        else
        {
            TraderUnlockService.EnableLevelLock = false;
            TraderUnlockService.ForceUnlock = true; // [FIX] Force unlock for existing profiles
            TonyLogger.Log("Trader unlocked by default (ForceUnlock active).");
        }

        // [FIX] Ensure ID consistency
        if (string.IsNullOrEmpty(traderBase.Id))
        {
             TonyLogger.Log("CRITICAL ERROR: traderBase.Id is null or empty! Hardcoding ID to ensure stability.");
             traderBase.Id = "66a0f6b2c4d8e90123456789"; 
        }

        // Ensure non-null collections
        if (traderBase.ItemsBuy == null) traderBase.ItemsBuy = new() { Category = [], IdList = [] };
        if (traderBase.ItemsBuyProhibited == null) traderBase.ItemsBuyProhibited = new() { Category = [], IdList = [] };
        if (traderBase.ItemsSell == null) traderBase.ItemsSell = [];

        // Apply Price Overrides
        foreach (var priceConfig in config.Prices)
        {
            foreach (var item in assort.Items)
            {
               var tpl = TonyConfig.GetTemplateId(item);
               if (!string.IsNullOrEmpty(tpl) && tpl == priceConfig.TplId && item.ParentId == "hideout")
               {
                   if (assort.BarterScheme.ContainsKey(item.Id))
                   {
                        var scheme = assort.BarterScheme[item.Id][0][0];
                        scheme.Count = priceConfig.Price;

                        // [NEW] Apply Currency
                        var currencyTpl = priceConfig.Currency switch
                        {
                            "USD" => "5696686a4bdc2da3298b456a",
                            "EUR" => "569668774bdc2da2298b4568",
                            "RUB" => "5449016a4bdc2d6f028b456f",
                            _ => null // Keep original if unknown/OTHER
                        };

                        if (currencyTpl != null)
                        {
                            scheme.Template = currencyTpl;
                        }
                   }
               }
            }
        }

        var avatarRoute = traderBase.Avatar ?? string.Empty;
        avatarRoute = avatarRoute.Replace(".png", "").Replace(".jpg", "").Replace(".jpeg", "");
        imageRouter.AddRoute(avatarRoute, traderImagePath);

        // [LOGIC] Flea Market Visibility
        if (config.Settings.AddTraderToFleaMarket)
        {
            _ragfairConfig.Traders.TryAdd(traderBase.Id, true);
        }
        else
        {
             _ragfairConfig.Traders.Remove(traderBase.Id);
        }

        // [LOGIC] Stock Manipulation (Randomization & Unlimited)
        if (config.Settings.RandomizeStockAvailable || config.Settings.UnlimitedStock)
        {
            TonyLogger.LogDebug("Starting Stock Manipulation...");
            var itemsToRemove = new List<string>();
            var itemsToRemoveNames = new List<string>(); // [NEW] Track names
            var random = new Random();
            int modifiedCount = 0;
            
            // [NEW] Get Locales for Name Resolution
            var locales = databaseServer.GetTables().Locales.Global["en"];

            foreach (var item in assort.Items)
            {
                 if (item.ParentId == "hideout")
                 {
                     // Randomize Availability
                     if (config.Settings.RandomizeStockAvailable)
                     {
                         if (random.Next(0, 100) < config.Settings.OutOfStockChance)
                         {
                             // Mark for removal
                             itemsToRemove.Add(item.Id);
                             
                             // [NEW] Resolve Name
                             string itemName = item.Id;
                             var tpl = TonyConfig.GetTemplateId(item);
                             if (!string.IsNullOrEmpty(tpl) && locales.Value != null && locales.Value.TryGetValue($"{tpl} Name", out var nameVal))
                             {
                                 itemName = nameVal.ToString();
                             }
                             itemsToRemoveNames.Add($"{itemName} ({item.Id})");

                             TonyLogger.LogDebug($"[Random Stock] removing: {itemName} ({item.Id})");
                             continue;
                         }
                     }

                     // Unlimited Stock Override
                     if (item.Upd != null)
                     {
                         if (config.Settings.UnlimitedStock)
                         {
                             item.Upd.UnlimitedCount = true;
                             item.Upd.StackObjectsCount = 999999;
                             if (item.Upd.BuyRestrictionMax > 0) 
                             {
                                item.Upd.BuyRestrictionMax = 9999;
                                item.Upd.BuyRestrictionCurrent = 0;
                             }
                             modifiedCount++;
                         }
                         else
                         {
                             item.Upd.UnlimitedCount = false;
                             item.Upd.StackObjectsCount = 100; 
                             modifiedCount++;
                         }
                     }
                 }
            }

            TonyLogger.LogDebug($"Total items modified for Stock setting: {modifiedCount}");

            // Perform Removals
            if (itemsToRemove.Count > 0)
            {
                assort.Items.RemoveAll(x => itemsToRemove.Contains(x.Id) || itemsToRemove.Contains(x.ParentId)); 
                foreach (var id in itemsToRemove)
                {
                    assort.BarterScheme.Remove(id);
                    assort.LoyalLevelItems.Remove(id);
                }
                TonyLogger.Log($"[Stock] Removed {itemsToRemove.Count} offers due to randomization.");
                TonyLogger.LogDebug($"Removed Items:\n  {string.Join("\n  ", itemsToRemoveNames)}");
            }
            else 
            {
                 TonyLogger.LogDebug("No items removed by randomization this turn.");
            }
        }

        // [LOGIC] Price Multiplier
        if (Math.Abs(config.Settings.PriceMultiplier - 1.0) > 0.001)
        {
             TonyLogger.LogDebug($"Applying Price Multiplier {config.Settings.PriceMultiplier}...");
             int changedCount = 0;
             
             // [NEW] Dictionary for quick item lookup to resolve names
             var itemMap = assort.Items.ToDictionary(x => x.Id, x => x);
             // Ensure locales are fetched (might be fetched above, but fetch here to be safe/local)
             var localesForPrice = databaseServer.GetTables().Locales.Global["en"];

             foreach (var itemSchemePair in assort.BarterScheme)
             {
                 var itemId = itemSchemePair.Key;
                 var schemeList = itemSchemePair.Value;

                 foreach (var schemeSubList in schemeList)
                 {
                     foreach (var component in schemeSubList)
                     {
                         if (component.Count.HasValue)
                         {
                             var oldPrice = component.Count.Value;
                             component.Count = (double)Math.Round(component.Count.Value * config.Settings.PriceMultiplier);
                             
                             // [NEW] Resolve Name for logging
                             string itemName = itemId;
                             if (itemMap.TryGetValue(itemId, out var item))
                             {
                                 var tpl = TonyConfig.GetTemplateId(item);
                                 if (!string.IsNullOrEmpty(tpl) && localesForPrice.Value != null && localesForPrice.Value.TryGetValue($"{tpl} Name", out var nameVal))
                                 {
                                     itemName = nameVal.ToString();
                                 }
                             }

                             TonyLogger.LogDebug($"  Price adjust: {oldPrice} -> {component.Count} | {itemName} ({itemId})");
                             changedCount++;
                         }
                     }
                 }
             }
             TonyLogger.Log($"[Pricing] Applied Global Price Multiplier: {config.Settings.PriceMultiplier} to {changedCount} items.");
        }

        // [TIMER] Configurable Refresh Time
        var timerRandom = new Random();
        int restockTime = timerRandom.Next(config.Settings.TraderRefreshMin, config.Settings.TraderRefreshMax);
        
        TonyLogger.Log($"Setting trader restock timer to {restockTime} seconds.");
        addCustomTraderHelper.SetTraderUpdateTime(
            _traderConfig,
            traderBase,
            restockTime,
            restockTime);

        traderBase.NextResupply = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + restockTime);

        addCustomTraderHelper.AddTraderToDb(traderBase, assort);
        
        if (config.Settings.DebugLogging)
        {
             TonyLogger.Log($"Trader initialized. Debug Enabled.");
        }

        var localeFirstName = traderBase.Nickname ?? traderBase.Name ?? "Tony";
        var localeDescription = "An ex-BEAR operator and former enforcer for Russian organized crime. After Tarkov collapsed, Volkov turned old connections into a quiet business, supplying weapons, armor, and contraband to smugglers, mercenaries, and criminals. He respects usefulness, hates weakness, and only opens doors for those who earn his trust.";
        addCustomTraderHelper.AddTraderToLocales(traderBase, localeFirstName, localeDescription);

        return Task.CompletedTask;
    }
}
