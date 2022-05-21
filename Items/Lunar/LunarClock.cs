using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using Thalassophobia.Utils;
using UnityEngine;
using static R2API.DamageAPI;

namespace Thalassophobia.Items.Lunar
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

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item Stats
        private float damageMultiplier;
        private float multiplerScaling;
        private float timePeriod;
        private float peridScaling;

        ModdedDamageType lunarClockDamage;

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

            damageMultiplier = config.Bind<float>("Item: " + ItemName, "Damage Multiplier", 2f, "How much extra damage you take.").Value;
            multiplerScaling = config.Bind<float>("Item: " + ItemName, "Damage Scaling", 0.5f, "").Value;
            timePeriod = config.Bind<float>("Item: " + ItemName, "Time", 4f, "How long the damage is dealt to you.").Value;
            peridScaling = config.Bind<float>("Item: " + ItemName, "Time Scaling", 1.5f, "").Value;

            lunarClockDamage = ReserveDamageType();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self.body && GetCount(self.body) > 0 && !DamageAPI.HasModdedDamageType(damageInfo, lunarClockDamage))
            {
                int count = GetCount(self.body);
                float totalDamage = damageInfo.damage * (damageMultiplier + (multiplerScaling * (count - 1)));
                float times = timePeriod + (peridScaling * (count - 1));
                Stack<float> damageStack = new Stack<float>();
                for (int i = 0; i < times; i++)
                {
                    damageStack.Push(totalDamage / times);
                }

                if (self.body.gameObject.GetComponent<ClockController>())
                {
                    ClockController controller = self.body.gameObject.GetComponent<ClockController>();
                    controller.pendingDamage.Add(damageStack);
                }
                else
                {
                    ClockController controller = self.body.gameObject.AddComponent<ClockController>();
                    controller.owner = self.body;
                    controller.damageType = lunarClockDamage;
                    controller.pendingDamage.Add(damageStack);
                }
            }
            else
            {
                orig(self, damageInfo);
            }
        }
    }
}