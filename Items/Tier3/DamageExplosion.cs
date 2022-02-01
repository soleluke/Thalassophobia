using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2Mod.RoR2ModPlugin;
using static RoR2Mod.ItemManager;
using R2API.Utils;

namespace RoR2Mod.Items.Tier3
{
    public class DamageExplosion : ItemBase<DamageExplosion>
    {
        public override string ItemName => "Omen Of Suffering";

        public override string ItemLangTokenName => "DAMAGE_EXPLOSION";

        public override string ItemPickupDesc => "Detonate enemies.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Custom buffs for tracking downtime and damage
        private CustomBuff downtimeDebuff;
        private CustomBuff omenDebuff;


        // Item stats
        private float cooldown;
        private float duration;
        private float radius;
        private float damage;
        private float damageStack;
        private float killedBonus;

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

            cooldown = config.Bind<float>("Item: " + ItemName, "Cooldown", 10f, "Cooldown between marking enemies").Value;
            duration = config.Bind<float>("Item: " + ItemName, "Duration", 5f, "How long the enemy is marked for.").Value;
            radius = config.Bind<float>("Item: " + ItemName, "Radius", 15f, "Radius of the explosion").Value;
            damage = config.Bind<float>("Item: " + ItemName, "Damage", 1f, "Damage multiplier for the explosion.").Value;
            damageStack = config.Bind<float>("Item: " + ItemName, "DamageStack", 0.5f, "Extra damage per item stack.").Value;
            killedBonus = config.Bind<float>("Item: " + ItemName, "KilledBonus", 0.5f, "Bonus damage percent if the enemy is killed early.").Value;

            downtimeDebuff = new CustomBuff("Charging Omen", Resources.Load<Sprite>("textures/bufficons/texbuffbanditskullicon"), Color.green, false, true);
            BuffAPI.Add(downtimeDebuff);

            omenDebuff = new CustomBuff("Suffering", Resources.Load<Sprite>("textures/bufficons/texbuffbanditskullicon"), Color.blue, false, true);
            BuffAPI.Add(omenDebuff);
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