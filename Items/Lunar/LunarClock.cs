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
        public override string ItemName => "Borrowed Time";

        public override string ItemLangTokenName => "LUNAR_CLOCK";

        public override string ItemPickupDesc => "Damage taken is applied over time... <style=cDeath>BUT increases the damage taken.</style>";

        public override string ItemFullDescription => "<style=cIsHealing>All damage is applied to you over 4 seconds</style> <style=cStack>(+2 per stack)</style>, but You take <style=cDeath>30% more damage</style> <style=cStack>(+30% per stack)</style> every time you take damage.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("ClockModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("ClockIcon.png");

        // Item Stats
        private float damageMultiplier;
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
            ItemTags = new ItemTag[] { ItemTag.Cleansable };

            damageMultiplier = config.Bind<float>("Item: " + ItemName, "Damage Multiplier", 0.30f, "How much extra damage you take.").Value;
            timePeriod = config.Bind<float>("Item: " + ItemName, "Time", 4f, "How long the damage is dealt to you.").Value;
            peridScaling = config.Bind<float>("Item: " + ItemName, "Time Scaling", 2f, "").Value;

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
                float times = timePeriod + (peridScaling * (count - 1));
                Stack<float> damageStack = new Stack<float>();
                for (int i = 0; i < times; i++)
                {
                    damageStack.Push(damageInfo.damage / times);
                }

                if (self.body.gameObject.GetComponent<ClockController>())
                {
                    ClockController controller = self.body.gameObject.GetComponent<ClockController>();
                    controller.pendingDamage.Add(damageStack);
                    controller.multiplier += damageMultiplier * count * damageInfo.procCoefficient;
                }
                else
                {
                    ClockController controller = self.body.gameObject.AddComponent<ClockController>();
                    controller.owner = self.body;
                    controller.damageType = lunarClockDamage;
                    controller.pendingDamage.Add(damageStack);
                    controller.multiplier += damageMultiplier * count * damageInfo.procCoefficient;
                }
            }
            else
            {
                orig(self, damageInfo);
            }
        }
    }
}