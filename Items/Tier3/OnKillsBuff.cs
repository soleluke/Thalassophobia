using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier3
{
    public class OnKillsBuff : ItemBase<OnKillsBuff>
    {
        public override string ItemName => "Volatile Concoction";

        public override string ItemLangTokenName => "ON_KILLS_BUFF";

        public override string ItemPickupDesc => "Trigger on kill effects on hit.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float damageThreshold;
        private float cooldown;
        private float damageBonus;

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

            damageThreshold = config.Bind<float>("Item: " + ItemName, "DamageThreshold", 450f, "Amount of damage needed to trigger the effect.").Value;
            cooldown = config.Bind<float>("Item: " + ItemName, "Cooldown", 2f, "Cooldown between triggering the effect.").Value;
            damageBonus = config.Bind<float>("Item: " + ItemName, "Damage", 0.25f, "Damage bonus to on kill effects.").Value;
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