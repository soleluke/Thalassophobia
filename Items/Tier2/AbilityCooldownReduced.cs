using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier2
{
    public class AbilityCooldownReduced : ItemBase<AbilityCooldownReduced>
    {
        public override string ItemName => "Neuron Implant";

        public override string ItemLangTokenName => "ABILITY_COOLDOWN_REDUCED";

        public override string ItemPickupDesc => "Ability cooldowns reduced.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

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
                    if (self.skillLocator.primary)
                    {
                        self.skillLocator.primary.flatCooldownReduction += cooldownReduction * itemCount;
                    }
                    if (self.skillLocator.secondary)
                    {
                        self.skillLocator.secondary.flatCooldownReduction += cooldownReduction * itemCount;
                    }
                    if (self.skillLocator.utility)
                    {
                        self.skillLocator.utility.flatCooldownReduction += cooldownReduction * itemCount;
                    }
                    if (self.skillLocator.special)
                    {
                        self.skillLocator.special.flatCooldownReduction += cooldownReduction * itemCount;
                    }
                }
            };
        }
    }
}