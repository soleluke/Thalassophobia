using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2Mod.RoR2ModPlugin;
using static RoR2Mod.ItemManager;
using R2API.Utils;

namespace RoR2Mod.Items.Tier2
{
    public class DotDamageUp : ItemBase<DotDamageUp>
    {
        public override string ItemName => "Compound LETH-41";

        public override string ItemLangTokenName => "DOT_DAMAGE_UP";

        public override string ItemPickupDesc => "Damage over time effects do more damage.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float damageUp;

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

            damageUp = config.Bind<float>("Item: " + ItemName, "DamageIncrease", 0.075f, "The percent increase to DoT damage where 1.0 is 100%.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.DotController.AddDot += (orig, self, attackerObject, duration, dotIndex, damageMultiplier) =>
            {
                var itemCount = GetCount(attackerObject.GetComponent<CharacterBody>());
                if (itemCount > 0)
                {
                    damageMultiplier *= 1 + (damageUp * itemCount);
                }
                orig(self, attackerObject, duration, dotIndex, damageMultiplier);
            };
        }
    }
}