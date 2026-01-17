using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace PrisciluOrigins;

/// <summary>
/// Service to check and unlock trader based on player level.
/// Uses reflection to safely access profile data across SPT versions.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class TraderUnlockService : IOnLoad
{
    private readonly ISptLogger<TraderUnlockService> _logger;
    private readonly SaveServer _saveServer;
    
    private const string PrisciluTraderId = "6748adca5c70634464b214a8";
    
    // Static config set by main mod
    public static int MinLevelRequired { get; set; } = 1;
    public static bool EnableLevelLock { get; set; } = false;
    
    public TraderUnlockService(
        ISptLogger<TraderUnlockService> logger,
        SaveServer saveServer)
    {
        _logger = logger;
        _saveServer = saveServer;
    }
    
    public Task OnLoad()
    {
        if (EnableLevelLock)
        {
            _logger.Info($"[PrisciluOrigins] TraderUnlockService active - checking profiles on load");
            CheckAllProfiles();
        }
        return Task.CompletedTask;
    }
    
    public void CheckAllProfiles()
    {
        if (!EnableLevelLock)
        {
            return;
        }
        
        try
        {
            var profiles = _saveServer.GetProfiles();
            foreach (var (sessionId, profile) in profiles)
            {
                CheckAndUnlockTrader(sessionId, profile);
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"[PrisciluOrigins] Error checking profiles: {ex.Message}");
        }
    }
    
    public void CheckAndUnlockTrader(string sessionId, object profile)
    {
        if (!EnableLevelLock || profile == null)
        {
            return;
        }
        
        try
        {
            // Access PMC character using reflection
            var charactersProperty = profile.GetType().GetProperty("Characters");
            if (charactersProperty == null) return;
            
            var characters = charactersProperty.GetValue(profile);
            if (characters == null) return;
            
            var pmcProperty = characters.GetType().GetProperty("Pmc");
            if (pmcProperty == null) return;
            
            var pmcProfile = pmcProperty.GetValue(characters);
            if (pmcProfile == null) return;
            
            // Get player level
            var infoProperty = pmcProfile.GetType().GetProperty("Info");
            if (infoProperty == null) return;
            
            var info = infoProperty.GetValue(pmcProfile);
            if (info == null) return;
            
            var levelProperty = info.GetType().GetProperty("Level");
            if (levelProperty == null) return;
            
            var levelValue = levelProperty.GetValue(info);
            int playerLevel = levelValue != null ? Convert.ToInt32(levelValue) : 0;
            
            // Get TradersInfo
            var tradersInfoProperty = pmcProfile.GetType().GetProperty("TradersInfo");
            if (tradersInfoProperty == null) return;
            
            var tradersInfo = tradersInfoProperty.GetValue(pmcProfile);
            if (tradersInfo == null) return;
            
            // Try to get the trader info from dictionary
            if (tradersInfo is System.Collections.IDictionary tradersDict)
            {
                if (tradersDict.Contains(PrisciluTraderId))
                {
                    var traderInfo = tradersDict[PrisciluTraderId];
                    if (traderInfo != null)
                    {
                        var unlockedProperty = traderInfo.GetType().GetProperty("Unlocked");
                        if (unlockedProperty != null)
                        {
                            var unlockedValue = unlockedProperty.GetValue(traderInfo);
                            bool isUnlocked = unlockedValue != null && (bool)unlockedValue;
                            
                            if (playerLevel >= MinLevelRequired && !isUnlocked)
                            {
                                unlockedProperty.SetValue(traderInfo, true);
                                _logger.Info($"[PrisciluOrigins] Trader UNLOCKED for session {sessionId} - Player Level: {playerLevel} >= Required: {MinLevelRequired}");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"[PrisciluOrigins] Error checking trader unlock for {sessionId}: {ex.Message}");
        }
    }
}
