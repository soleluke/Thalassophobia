using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2Mod.RoR2ModPlugin;
using static RoR2Mod.ItemManager;

namespace RoR2Mod.Items.Tier1
{
    public class HealOnKill : ItemBase<HealOnKill>
    {
        public override string ItemName => "Predatory Fungus";

        public override string ItemLangTokenName => "HEAL_ON_KILL";

        public override string ItemPickupDesc => "Heal on kill.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float healNum;
        private float healFraction;

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

            healNum = config.Bind<float>("Item: " + ItemName, "ConstantHealing", 5f, "Constant amount of health to heal on every kill.").Value;
            healFraction = config.Bind<float>("Item: " + ItemName, "PercentHealing", 0.0075f, "Percent of max health to heal where 1.0 is 100%.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) => {
                orig(self, damageReport);
                if (damageReport.attacker)
                {
                    var itemCountPF = GetCount(damageReport.attackerBody);
                    if (itemCountPF > 0)
                    {
                        RoR2.HealthComponent health = damageReport.attackerBody.healthComponent;

                        health.Heal(healNum * itemCountPF + (healFraction * itemCountPF * health.fullHealth), damageReport.damageInfo.procChainMask);
                    }
                }
            };
        }
    }
}