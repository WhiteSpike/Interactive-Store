using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using InteractiveStore.Configuration;
using InteractiveStore.UI.Application;
using InteractiveTerminalAPI.UI;
using WeatherProbe.Misc;
namespace InteractiveStore
{
    [BepInPlugin(Metadata.GUID,Metadata.NAME,Metadata.VERSION)]
    [BepInDependency("WhiteSpike.InteractiveTerminalAPI")]
    public class Plugin : BaseUnityPlugin
    {
        internal static readonly Harmony harmony = new(Metadata.GUID);
        internal static ManualLogSource mls;
        internal static ModConfiguration config;
        void Awake()
        {
            config = new ModConfiguration(Config);
            InteractiveTerminalManager.RegisterApplication<StoreApplication>(config.ItemStoreCommandList.Value, caseSensitive: config.ItemStoreCommandCaseSensitive.Value);
            InteractiveTerminalManager.RegisterApplication<UnlockableStoreApplication>(config.DecorationStoreCommandList.Value, caseSensitive: config.DecorationStoreCommandCaseSensitive.Value);
			InteractiveTerminalManager.RegisterApplication<VehicleApplication>(config.VehicleStoreCommandList.Value, caseSensitive: config.VehicleStoreCommandCaseSensitive.Value);
			mls = Logger;
            mls.LogInfo($"{Metadata.NAME} {Metadata.VERSION} has been loaded successfully.");
        }
    }   
}
