using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2Mod.RoR2ModPlugin;
using static RoR2Mod.ItemManager;
using R2API.Utils;

namespace RoR2Mod.Items.Tier3
{
    public class DroneSummon : ItemBase<DroneSummon>
    {
        public override string ItemName => "Overclocked Control Unit";

        public override string ItemLangTokenName => "DRONE_SUMMON";

        public override string ItemPickupDesc => "Summon powerful drones that copy your items.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float cooldown;
        private int numItems;
        private int maxDrones;
        private float damage;


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

            cooldown = config.Bind<float>("Item: " + ItemName, "Cooldown", 30f, "Time between spawning drones.").Value;
            numItems = config.Bind<int>("Item: " + ItemName, "NumberOfItems", 10, "Number of items the drones copy.").Value;
            maxDrones = config.Bind<int>("Item: " + ItemName, "MaxDrones", 3, "Max number of drones.").Value;
            damage = config.Bind<float>("Item: " + ItemName, "Damage", 1.5f, "Percent damage the drones deal.").Value;
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