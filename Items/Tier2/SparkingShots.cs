using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Utils;

namespace Thalassophobia.Items.Tier2
{
    public class SparkingShots : ItemBase<SparkingShots>
    {
        public override string ItemName => "Red Phosphorous";

        public override string ItemLangTokenName => "SPARKING_SHOTS";

        public override string ItemPickupDesc => "Make enemies spark on hit.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Custom buff for sparking
        private BuffDef sparkingDebuff;

        // Item stats
        private float damageThreshold;
        private float duration;
        private float range;
        private float damageCoefficient;
        private float procCoefficient;
        private int sparks;
        private int sparksPerStack;

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

            damageThreshold = config.Bind<float>("Item: " + ItemName, "DamageThreshold", 300f, "Percent damage that needs to be done to trigger the effect.").Value;
            duration = config.Bind<float>("Item: " + ItemName, "Duration", 3.5f, "Time that the debuff lasts.").Value;
            range = config.Bind<float>("Item: " + ItemName, "Range", 10f, "Range of the sparks").Value;
            damageCoefficient = config.Bind<float>("Item: " + ItemName, "DamageCoefficient", 0.2f, "Percent damage that the sparks deal.").Value;
            procCoefficient = config.Bind<float>("Item: " + ItemName, "ProcCoefficent", 0.05f, "Proc coefficicent of the sparks.").Value;
            sparks = config.Bind<int>("Item: " + ItemName, "AmountOfSparks", 3, "Amount of sparks produced when striking on enemy.").Value;
            sparksPerStack = config.Bind<int>("Item: " + ItemName, "SparksPerStack", 1, "Amount of extra sparks per item stack.").Value;
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