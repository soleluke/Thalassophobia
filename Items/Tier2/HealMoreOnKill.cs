using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Utils;

namespace Thalassophobia.Items.Tier2
{
    public class HealMoreOnKill : ItemBase<HealMoreOnKill>
    {
        public override string ItemName => "Red Stone";

        public override string ItemLangTokenName => "HEAL_MORE_ON_KILL";

        public override string ItemPickupDesc => "Increases the amount of healing you receive after slaying an enemy.";

        public override string ItemFullDescription => "On killing an enemy, gain a buff that gives 7% increased healing and 5 armor that can stack 3 (+1 per stack) times. (hehe jojo reference)";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Custom buff for healing
        private BuffDef redStoneBoost;

        // Item Stats
        private float healUp;
        private int baseMaxBuffs;
        private int maxBuffsStack;
        private float duration;

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

            healUp = config.Bind<float>("Item: " + ItemName, "HealingIncrease", 0.07f, "Percent of extra healing from the buff gained after killing an enemy.").Value;
            baseMaxBuffs = config.Bind<int>("Item: " + ItemName, "MaxBuffStacks", 3, "The amount of times you can stack the buff.").Value;
            maxBuffsStack = config.Bind<int>("Item: " + ItemName, "MaxBuffStacksPerItem", 1, "How many extra stacks you can have per stack of the item.").Value;
            duration = config.Bind<float>("Item: " + ItemName, "Duration", 5f, "How long the buff lasts.").Value;

            redStoneBoost = ScriptableObject.CreateInstance<BuffDef>();
            redStoneBoost.name = "Red Stone Boost";
            redStoneBoost.iconSprite = Resources.Load<Sprite>("textures/bufficons/texbuffregenboosticon");
            redStoneBoost.isDebuff = false;
            redStoneBoost.canStack = true;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.Heal += (orig, self, amount, procChainMask, nonRegen) =>
            {
                var itemCountHealKill = GetCount(self.body);
                if (itemCountHealKill > 0)
                {
                    int stacks = self.body.GetBuffCount(redStoneBoost);
                    amount *= 1 + (healUp * stacks);
                }

                return orig(self, amount, procChainMask, nonRegen);
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                orig(self, damageReport);
                if (damageReport.attacker)
                {
                    var itemCountHealKill = GetCount(damageReport.attackerBody);
                    if (itemCountHealKill > 0)
                    {
                        CharacterBody body = damageReport.attackerBody;
                        body.AddTimedBuff(redStoneBoost, duration, (int)(baseMaxBuffs + (maxBuffsStack * (itemCountHealKill - 1))));
                    }
                }
            };
        }
    }
}