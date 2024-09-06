using BepInEx;
using HarmonyLib;
using IL.RoR2.ContentManagement;
using Newtonsoft.Json.Utilities;
using R2API;
using R2API.ContentManagement;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.SceneManagement;
using static RoR2.CombatDirector;
using Random = UnityEngine.Random;

//using On.RoR2;

namespace removeGilded
{
    // This is an example plugin that can be put in
    // BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    // It's a small plugin that adds a relatively simple item to the game,
    // and gives you that item whenever you press F2.

    // This attribute specifies that we have a dependency on a given BepInEx Plugin,
    // We need the R2API ItemAPI dependency because we are using for adding our item to the game.
    // You don't need this if you're not using R2API in your plugin,
    // it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(DirectorAPI.PluginGUID)]
    [BepInDependency(EliteAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    [R2API.Utils.NetworkCompatibility(R2API.Utils.CompatibilityLevel.NoNeedForSync,R2API.Utils.VersionStrictness.DifferentModVersionsAreOk)]
    public class RemoveGilded : BaseUnityPlugin
    {
        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore,
        // we will deprecate this mod.
        // Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "K0FFE1NE";
        public const string PluginName = "RemoveGilded";
        public const string PluginVersion = "1.0.0";
        // We need our item definition to persist through our functions, and therefore make it a class field.

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            Logger.LogInfo("Change the cost to spawn a gilded elite to t1 cost");
            On.RoR2.CombatDirector.Init += CombatDirector_Init;

            Logger.LogInfo("Hooking into CombatDirector_Spawn");
            On.RoR2.CombatDirector.Spawn += CombatDirector_Spawn;
        }

        private void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();

            EliteTierDef t1Tier = EliteAPI.VanillaEliteTiers[1];
            EliteTierDef gildedTier = EliteAPI.VanillaEliteTiers[3];
            gildedTier.costMultiplier = t1Tier.costMultiplier;

            Logger.LogDebug("t1 cost: " +  t1Tier.costMultiplier);
        }

        private bool CombatDirector_Spawn(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            if (eliteDef is not null && eliteDef.name == "edAurelionite")
            {
                Logger.LogDebug("Director attempted to spawn a gilded");

                EliteDef[] t1Elites = EliteAPI.VanillaEliteTiers[1].eliteTypes;

                Logger.LogDebug("Randomly choosing a tier 1 elite to spawn instead");
                int n = Random.RandomRangeInt(0, t1Elites.Length);
                eliteDef = t1Elites[n];

                Logger.LogDebug($"Replaced with:{eliteDef.name}");
            }
            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }
    }
}
