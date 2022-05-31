﻿using BepInEx;
using R2API;
using R2API.Networking;
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
using static Thalassophobia.Utils.WispSummonController;

namespace Thalassophobia
{
    // Meta data and dependencies
    [BepInDependency("com.bepis.r2api")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(DotAPI), nameof(EliteAPI), nameof(ContentAddition), nameof(DamageAPI), nameof(OrbAPI), nameof(RecalculateStatsAPI), nameof(LegacyResourcesAPI), nameof(NetworkingAPI))]
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

        // DEBUG
        public static bool DEBUG = true;

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

            // Load items
            itemHelper = new ItemHelper(Config);
            itemHelper.LoadAll();

            // Load elites
            eliteHelper = new EliteHelper(Config);
            eliteHelper.LoadAll();

            NetworkingAPI.RegisterMessageType<SyncCheckAlive>();

            if (DEBUG)
            {
                Hooks();
            }

            // Log that everything finished loading
            Log.LogInfo(nameof(Awake) + " Penis");
            Log.LogInfo(allAssets.Length);
            foreach (string s in allAssets)
            {
                Log.LogInfo(s);
            }
        }

        void Hooks()
        {
            On.RoR2.Items.ContagiousItemManager.Init += ItemBase.RegisterVoidPairings;
            On.RoR2.ItemTierCatalog.Init += ItemBase.RegisterItemTier;
        }
    }
}
