using BepInEx;
using R2API;
using R2API.Networking;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Thalassophobia.Items;
using Thalassophobia.Utils;
using UnityEngine;
using static On.RoR2.CharacterBody;
using static R2API.SoundAPI;
using static RoR2.DotController;
using static Thalassophobia.Utils.WispSummonController;

namespace Thalassophobia
{
    // Meta data and dependencies
    [BepInDependency("com.bepis.r2api")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(DotAPI), nameof(EliteAPI), nameof(ContentAddition), nameof(DamageAPI), nameof(OrbAPI), nameof(RecalculateStatsAPI), nameof(LegacyResourcesAPI), nameof(NetworkingAPI), nameof(TempVisualEffectAPI))]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        // Mod info
        public const string MODNAME = "Thalassophobia";
        public const string AUTHOR = "JTPuff";
        public const string GUID = "com." + AUTHOR + "." + MODNAME;
        public const string VERSION = "0.1.0";

        // Asset bundle
        public static AssetBundle assetBundle;
        public static string[] allAssets;

        // String builder
        public static StringBuilder BUILDER = new StringBuilder();

        // Helpers
        private ItemHelper itemHelper;
        private EliteHelper eliteHelper;

        public static Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"stubbed hopoo games/deferred/standard", "shaders/deferred/hgstandard"},
            {"stubbed hopoo games/fx/cloud remap", "shaders/fx/hgcloudremap" },
            {"stubbed hopoo games/fx/cloud intersection remap", "shaders/fx/hgintersectioncloudremap" },
        };

        // DEBUG
        public static bool DEBUG = true;
        private static List<Material> SwappedMaterials = new List<Material>();

        private void Awake()
        {

            // Logger
            Log.Init(Logger);

            // Bundle
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Thalassophobia.myassets"))
            {
                assetBundle = AssetBundle.LoadFromStream(stream);
            }
            allAssets = assetBundle.GetAllAssetNames();

            // Shader Conversion
            ShaderConversion(assetBundle);
            if (DEBUG)
            {
                Log.LogInfo(SwappedMaterials.Count);
            }

            // Load items
            itemHelper = new ItemHelper(Config);
            itemHelper.LoadAll();

            // Load elites
            eliteHelper = new EliteHelper(Config);
            eliteHelper.LoadAll();

            // Register network thing
            NetworkingAPI.RegisterMessageType<SyncCheckAlive>();

            CustomDamageColor.Init();

            // Hooks
            Hooks();

            // Log that everything finished loading
            Log.LogInfo(nameof(Awake) + " Penis");
            Log.LogInfo(allAssets.Length);
            if (DEBUG)
            {
                foreach (string s in allAssets)
                {
                    Log.LogInfo(s);
                }
            }
        }

        void Hooks()
        {
            // Fixing void pairings and item tiers that weren't working
            On.RoR2.Items.ContagiousItemManager.Init += ItemBase.RegisterVoidPairings;
            On.RoR2.ItemTierCatalog.Init += ItemBase.RegisterItemTier;
        }

        // Borrowed this from Aetherium
        public static void ShaderConversion(AssetBundle assets)
        {
            var materialAssets = assets.LoadAllAssets<Material>().Where(material => material.shader.name.StartsWith("Stubbed"));
            if (DEBUG)
            {
                Log.LogInfo(materialAssets.ToList().Count);
            }
            foreach (Material material in materialAssets)
            {
                var replacementShader = LegacyResourcesAPI.Load<Shader>(ShaderLookup[material.shader.name.ToLowerInvariant()]);
                if (replacementShader)
                {
                    material.shader = replacementShader;
                    SwappedMaterials.Add(material);
                    if (DEBUG)
                    {
                        Log.LogInfo("Swapped Shader");
                        Log.LogInfo(material.name);
                    }
                }
            }
        }
    }
}
