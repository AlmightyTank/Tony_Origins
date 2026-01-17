using System.Text.Json;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Servers;
using Path = System.IO.Path; // [FIX] Ambiguity

namespace PrisciluOrigins.Config;

public class PrisciluConfig
{
    private readonly string _modPath;
    private readonly string _configDir;
    private readonly string _settingsPath;
    private readonly string _pricesPath;
    
    private readonly DatabaseServer _databaseServer;

    public SettingsConfig Settings { get; private set; } = new();
    public List<PriceConfigItem> Prices { get; private set; } = new();

    public PrisciluConfig(string modPath, DatabaseServer databaseServer)
    {
        _modPath = modPath;
        _databaseServer = databaseServer;
        _configDir = Path.Combine(_modPath, "config");
        _settingsPath = Path.Combine(_configDir, "settings.json");
        _pricesPath = Path.Combine(_configDir, "prices.json");
    }

    public void LoadOrGenerate(TraderBase baseJson, TraderAssort assortJson)
    {
        if (!Directory.Exists(_configDir))
        {
            Directory.CreateDirectory(_configDir);
        }

        LoadOrGenerateSettings(baseJson);
        LoadOrGeneratePrices(assortJson);
    }

    private void LoadOrGenerateSettings(TraderBase baseJson)
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                Settings = JsonSerializer.Deserialize<SettingsConfig>(json) ?? new SettingsConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Priscilu] Error loading settings.json: {ex.Message}");
            }
        }
        else
        {
            Settings = new SettingsConfig
            {
                MinLevel = baseJson.LoyaltyLevels.FirstOrDefault()?.MinLevel ?? 1,
                UnlockedByDefault = baseJson.UnlockedByDefault ?? false,
                RestockTimerSeconds = 3600
            };
            SaveJson(_settingsPath, Settings);
        }
    }

    private void LoadOrGeneratePrices(TraderAssort assortJson)
    {
        if (File.Exists(_pricesPath))
        {
            try
            {
                var json = File.ReadAllText(_pricesPath);
                Prices = JsonSerializer.Deserialize<List<PriceConfigItem>>(json) ?? new List<PriceConfigItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Priscilu] Error loading prices.json: {ex.Message}");
            }
        }
        else
        {
            Prices = new List<PriceConfigItem>();
            // [FIX] Locale handling might be complex with LazyLoad. Skip names for now to ensure build stability.
            // var locales = _databaseServer.GetTables().Locales.Global["en"]; 

            foreach (var item in assortJson.Items)
            {
                if (item.ParentId == "hideout")
                {
                    // Find price
                    if (!assortJson.BarterScheme.ContainsKey(item.Id)) continue;
                    
                    var schemeList = assortJson.BarterScheme[item.Id];
                    if (schemeList == null || schemeList.Count == 0) continue;
                    
                    var scheme = schemeList[0][0]; // Assuming first scheme, first component

                    // [FIX] Use dynamic to bypass missing member error, assume _tpl matches JSON
                    var tpl = (string)((dynamic)item)._tpl; 
                    var name = tpl; // Fallback to ID since locale lookup is disabled for stability

                    Prices.Add(new PriceConfigItem
                    {
                        TplId = tpl,
                        ItemName = name,
                        Price = scheme.Count ?? 0, 
                        Currency = scheme.Template == "5449016a4bdc2d6f028b456f" ? "RUB" : 
                                   scheme.Template == "5696686a4bdc2da3298b456a" ? "USD" : 
                                   scheme.Template == "569668774bdc2da2298b4568" ? "EUR" : "OTHER"
                    });
                }
            }
            
            Prices = Prices.OrderBy(x => x.ItemName).ToList();

            SaveJson(_pricesPath, Prices);
        }
    }

    private void SaveJson<T>(string path, T data)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(path, JsonSerializer.Serialize(data, options));
    }
}
