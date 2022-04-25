using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Thalassophobia.Items;
using UnityEngine;
using static On.RoR2.CharacterBody;
using static R2API.SoundAPI;
using static RoR2.DotController;

namespace Thalassophobia
{
    // Meta data and dependencies
    [BepInDependency("com.bepis.r2api")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(DotAPI), nameof(EliteAPI), nameof(ContentAddition), nameof(DamageAPI), nameof(OrbAPI))]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        // Mod info
        public const string MODNAME = "Thalassophobia";
        public const string AUTHOR = "JTPuff";
        public const string GUID = "com." + AUTHOR + "." + MODNAME;
        public const string VERSION = "0.1.0";

        // String builder
        public static StringBuilder BUILDER = new StringBuilder();

        // Helpers
        private ItemHelper itemHelper;
        private EliteHelper eliteHelper;

        // DEBUG
        public static bool DEBUG = true;

        private void Awake()
        {
            // Logger
            Log.Init(Logger);

            // Load items
            itemHelper = new ItemHelper(Config);
            itemHelper.LoadAll();

            // Load elites
            eliteHelper = new EliteHelper(Config);
            eliteHelper.LoadAll();

            if (DEBUG) {
                Hooks();
            }

            // Log that everything finished loading
            Log.LogInfo(nameof(Awake) + " done.");
        }

        void Hooks() {
            On.RoR2.Items.ContagiousItemManager.Init += ItemBase.RegisterVoidPairings;


            On.RoR2.CharacterBody.OnBuffFirstStackGained += (orig, self, buffDef) =>
            {
                //Log.LogInfo(buffDef.name);
                //Log.LogInfo(System.Environment.StackTrace);
            };
        }
    }
}
