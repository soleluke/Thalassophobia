using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2Mod.EliteEquipments;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using static On.RoR2.CharacterBody;
using static R2API.SoundAPI;
using static RoR2.DotController;

namespace RoR2Mod
{
    // Meta data and dependencies
    [BepInDependency("com.bepis.r2api")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(ItemDropAPI), nameof(LanguageAPI), nameof(BuffAPI), nameof(DotAPI), nameof(EliteAPI), nameof(ProjectileAPI), nameof(EffectAPI), nameof(SoundAPI))]
    [BepInPlugin(GUID, MODNAME, VERSION)]

    public sealed class RoR2ModPlugin : BaseUnityPlugin
    {
        // Mod info
        public const string MODNAME = "Thalassophobia";
        public const string AUTHOR = "JTPuff";
        public const string GUID = "com." + AUTHOR + "." + MODNAME;
        public const string VERSION = "0.0.1";

        // String builder
        public static StringBuilder BUILDER = new StringBuilder();

        // Managers
        private ItemManager itemManager;
        private EliteManager eliteManager;

        // DEBUG
        public static bool DEBUG = true;
        public static bool GODMODE = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Awake is automatically called by Unity")]
        private void Awake() //Called when loaded by BepInEx.
        {
            // Logger
            Log.Init(Logger);

            // Loading assets
            AssetManager.Init(this);

            // Load items
            itemManager = new ItemManager(Config);
            itemManager.LoadAll();

            // Load elites
            eliteManager = new EliteManager(Config);
            eliteManager.LoadAll();

            // Log that everything finished loading
            Log.LogInfo(nameof(Awake) + " done.");

            // Godmode
            if (DEBUG)
            {
                On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
                {
                    if (GODMODE)
                    {
                        var charComponent = self.GetComponent<CharacterBody>();
                        if (charComponent != null && charComponent.isPlayerControlled)
                        {
                            return;
                        }
                        orig(self, damageInfo);
                    }
                    else
                    {
                        orig(self, damageInfo);
                    }
                };
            }
        }

        private void Update()
        {
            if (DEBUG)
            {
                // F2 Spawn thing
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(AffixPure.instance.EliteEquipmentDef.equipmentIndex), transform.position, transform.forward * 20f);
                }

                // F10 enables godmode
                if (Input.GetKeyDown(KeyCode.F10))
                {
                    GODMODE = !GODMODE;
                    if (GODMODE)
                    {
                        Log.LogInfo($"Godmode is enabled");
                    }
                    else if (!GODMODE)
                    {
                        Log.LogInfo($"Godmode is disabled");
                    }
                }
            }
        }
    }
}
