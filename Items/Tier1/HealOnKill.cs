using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier1
{
    public class HealOnKill : ItemBase<HealOnKill>
    {
        public override string ItemName => "Predatory Fungus";

        public override string ItemLangTokenName => "HEAL_ON_KILL";

        public override string ItemPickupDesc => "Regen on kill.";

        public override string ItemFullDescription => "On killing an enemy you gain regeneration for 3 (+1 per stack) seconds.";
        
        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float time;
        BuffDef regen;

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

            time = config.Bind<float>("Item: " + ItemName, "Time", 3f, "").Value;

            regen = ScriptableObject.CreateInstance<BuffDef>();
            regen.name = "Fungus Regeneration";
            regen.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            regen.canStack = true;
            regen.isDebuff = false;
            ContentAddition.AddBuffDef(regen);
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
                int count = self.GetBuffCount(regen);
                if (count > 0)
                {
                    Reflection.SetPropertyValue<float>(self, "regen", self.regen + 0.6f * count);
                }
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) => {
                orig(self, damageReport);
                if (damageReport.attacker)
                {
                    var itemCount = GetCount(damageReport.attackerBody);
                    if (itemCount > 0)
                    {
                        damageReport.attackerBody.AddTimedBuff(regen, 3 + (1 * itemCount));
                    }
                }
            };
        }
    }
}