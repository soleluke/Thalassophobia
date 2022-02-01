using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2Mod.RoR2ModPlugin;
using static RoR2Mod.ItemManager;
using R2API.Utils;

namespace RoR2Mod.Items.Tier2
{
    public class DamageUp : ItemBase<DamageUp>
    {
        public override string ItemName => "Steroids";

        public override string ItemLangTokenName => "DAMAGE_UP";

        public override string ItemPickupDesc => "Damage up.";

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
        private float damageUpStack;
        private float attackSpeedUp;
        private float attackSpeedUpStack;

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

            damageUp = config.Bind<float>("Item: " + ItemName, "DamageIncrease", 0.15f, "The percent increase to base damage where 1.0 is 100%.").Value;
            damageUpStack = config.Bind<float>("Item: " + ItemName, "DamageIncreasePerStack", 0.10f, "The percent increase to base damage for every stack of the item over 1 stack where 1.0 is 100%.").Value;
            attackSpeedUp = config.Bind<float>("Item: " + ItemName, "AttackSpeedIncrease", 0.15f, "The percent increase to attack speed where 1.0 is 100%.").Value;
            attackSpeedUpStack = config.Bind<float>("Item: " + ItemName, "AttackSpeedIncreasePerStack", 0.10f, "The percent increase to attack speed for every stack of the item over 1 stack where 1.0 is 100%.").Value;
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
                var damageUpCount = GetCount(self);
                if (damageUpCount > 0)
                {
                    Reflection.SetPropertyValue<float>(self, "damage", self.damage + (self.damage * (damageUp + (damageUpStack * (damageUpCount - 1)))));
                    Reflection.SetPropertyValue<float>(self, "attackSpeed", self.attackSpeed + (self.attackSpeed * (attackSpeedUp + (attackSpeedUpStack * (damageUpCount - 1)))));
                }
            };
        }
    }
}