using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2Mod.RoR2ModPlugin;
using static RoR2Mod.ItemManager;
using R2API.Utils;
using static On.RoR2.CharacterBody;

namespace RoR2Mod.Items.Tier2
{
    public class DebuffTimeDown : ItemBase<DebuffTimeDown>
    {
        public override string ItemName => "Diving Gear";

        public override string ItemLangTokenName => "DEBUFF_TIME_DOWN";

        public override string ItemPickupDesc => "Reduces debuff time on yourself.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float debuffReduction;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Utility };

            debuffReduction = config.Bind<float>("Item: " + ItemName, "DebuffTimeReduction", 0.15f, "Percent that debuff time is reduced by.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            AddTimedBuff_BuffDef_float += (orig, self, buffDef, duration) => {
                var itemCount = GetCount(self);
                if (itemCount > 0) {
                    duration -= duration * (Mathf.Pow(itemCount, debuffReduction)-(1-debuffReduction));
                }
                orig(self, buffDef, duration);
            };
        }
    }
}