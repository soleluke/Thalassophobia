using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2Mod.RoR2ModPlugin;
using static RoR2Mod.ItemManager;
using R2API.Utils;

namespace RoR2Mod.Items.Lunar
{
    public class LunarClock : ItemBase<LunarClock>
    {
        public override string ItemName => "Borrwed Time";

        public override string ItemLangTokenName => "LUNAR_CLOCK";

        public override string ItemPickupDesc => "Amplifies the damage you take, but is applied over time.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item Stats
        private float damageAmp;
        private float time;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Healing };

            damageAmp = config.Bind<float>("Item: " + ItemName, "DamageMultiplier", 0.25f, "How much extra damage you take.").Value;
            time = config.Bind<float>("Item: " + ItemName, "Time", 5f, "How long the damage is dealt to you.").Value;
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