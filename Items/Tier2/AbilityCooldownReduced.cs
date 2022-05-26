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

        public override string ItemFullDescription => "<style=cIsDamage>+10%</style> <style=cStack>(+10% per stack)</style> attack speed and <style=cIsUtility>10%</style> <style=cStack>(+10% per stack)</style> less cool down on primary and secondary skills.";

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

            cooldownReduction = config.Bind<float>("Item: " + ItemName, "Cooldown Reduction Multiplier", 0.1f, "Amount of time ability cooldowns and reduced by.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var count = GetCount(sender);
            if (count > 0)
            {
                args.attackSpeedMultAdd += 0.1f * count;
                args.primaryCooldownMultAdd -= cooldownReduction * count;
                args.secondaryCooldownMultAdd -= cooldownReduction * count;
            }
        }
    }
}