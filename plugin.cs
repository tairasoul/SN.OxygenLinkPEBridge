using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace OxygenLinkPEBridge;

[BepInPlugin("tairasoul.subnautica.oxygenlinkpebridge", "OxygenLinkPEBridge", "1.0.1")]
[BepInDependency("0xKate.OxygenLink")]
[BepInDependency("PickupableStorageEnhanced")]
class Plugin : BaseUnityPlugin 
{
  internal static ManualLogSource Log;
  void Awake()
  {
    Log = Logger;
    Harmony harmony = new("tairasoul.subnautica.oxygenlinkpebridge");
    harmony.PatchAll();
    Logger.LogInfo("Patched OxygenLink methods");
  }
}