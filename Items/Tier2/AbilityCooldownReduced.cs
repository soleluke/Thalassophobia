using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier2
{
    public class AbilityCooldownReduced : ItemBase<AbilityCooldownReduced>
    {
        public override string ItemName => "Neuron Implant";

        public override string ItemLangTokenName => "ABILITY_COOLDOWN_REDUCED";

        public override string ItemPickupDesc => "Primary and secondary skills are faster.";

        public override string ItemFullDescription => "+10% (+10) attack speed and 0.3 (+0.3 per stack) seconds less cool down on primary and secondary skills. +10% (+10% per stack) attack speed";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float cooldownReduction;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Utility };

            cooldownReduction = config.Bind<float>("Item: " + ItemName, "CooldownReduction", 0.3f, "Amount of time ability cooldowns and reduced by.").Value;
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
                var itemCount = GetCount(self);
                if (itemCount > 0)
                {
                    Reflection.SetPropertyValue<float>(self, "attackSpeed", self.attackSpeed + (0.1f * itemCount));
                    if (self.skillLocator.primary)
                    {
                        self.skillLocator.primary.flatCooldownReduction += cooldownReduction * itemCount;
                    }
                    if (self.skillLocator.secondary)
                    {
                        self.skillLocator.secondary.flatCooldownReduction += cooldownReduction * itemCount;
                    }
                }
            };
        }
    }
}