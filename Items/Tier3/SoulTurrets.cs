using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2Mod.RoR2ModPlugin;
using static RoR2Mod.ItemManager;
using R2API.Utils;

namespace RoR2Mod.Items.Tier3
{
    public class SoulTurrets : ItemBase<SoulTurrets>
    {
        public override string ItemName => "Soul Converter";

        public override string ItemLangTokenName => "SOUL_TURRETS";

        public override string ItemPickupDesc => "Convert slain enemies into orbital turrets.";

        public override string ItemFullDescription => "";


        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float conversionChance;
        private float damage;
        private float duration;
        private float maxTurrets;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Damage };

            conversionChance = config.Bind<float>("Item: " + ItemName, "ConversionChance", 25f, "Chance to convert slain enemies.").Value;
            damage = config.Bind<float>("Item: " + ItemName, "Damage", 400f, "Percent damage that the turrets do.").Value;
            duration = config.Bind<float>("Item: " + ItemName, "Duration", 10f, "How long the turrets last.").Value;
            maxTurrets = config.Bind<float>("Item: " + ItemName, "MaxTurrets", 1f, "Max number of turrets.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
        }
    }
}