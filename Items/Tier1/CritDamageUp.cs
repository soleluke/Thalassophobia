using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Utils;

namespace Thalassophobia.Items.Tier1
{
    public class CritDamageUp : ItemBase<CritDamageUp>
    {
        public override string ItemName => "Military Training";

        public override string ItemLangTokenName => "CRIT_DAMAGE_UP";

        public override string ItemPickupDesc => "Increases crit chance and crit damage.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float damageUp;
        private float critUp;

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

            damageUp = config.Bind<float>("Item: " + ItemName, "CritDamageUp", 0.05f, "Percent increase to crit damage where 1.0 is 100%.").Value;
            critUp = config.Bind<float>("Item: " + ItemName, "CritChanceIncrease", 5f, "Percent crit chance increase.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                orig(self);
                int critDamageUpCount = GetCount(self);
                if (critDamageUpCount > 0)
                {
                    Reflection.SetPropertyValue<float>(self, "crit", self.crit + (critUp * critDamageUpCount));
                }
            };

            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
            {
                if (damageInfo.attacker)
                {
                    var critDamageUpCount = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                    if (damageInfo.crit && critDamageUpCount > 0)
                    {
                        damageInfo.damage += (damageInfo.damage * damageUp) * critDamageUpCount;
                    }
                }
                orig(self, damageInfo);
            };
        }
    }
}