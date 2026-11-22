using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using UnhollowerRuntimeLib;
using HarmonyLib;

namespace MyFirstPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin {
    internal static new ManualLogSource Log;

    public override void Load() {

        // Log
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        // State Manager
        AddComponent<State>();

        // Mods
        AddComponent<ModPowerBypass>();
        AddComponent<ModResetBypass>();
        AddComponent<ModReuseLastShot>();
        AddComponent<ModHoleInOneBruteforcer>();

        // Patches
        Harmony.CreateAndPatchAll(typeof(CheckForBallHitPatch));
        
    }
}