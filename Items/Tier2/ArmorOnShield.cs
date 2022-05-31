using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier2
{
    public class ArmorOnShield : ItemBase<ArmorOnShield>
    {
        public override string ItemName => "Hardlight Shields";

        public override string ItemLangTokenName => "ARMOR_ON_SHIELD";

        public override string ItemPickupDesc => "Shields increase your armor.";

        public override string ItemFullDescription => "Having active shields increases your armor by <style=cIsUtility>50</style> <style=cStack>(+25 per stack)</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

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
            if (count > 0 )
            {
                if (sender.healthComponent.shield > 0)
                {
                    args.armorAdd = (50 + (25 * (count - 1))) * (sender.healthComponent.shield / sender.healthComponent.fullShield);
                }
                args.baseShieldAdd += sender.healthComponent.fullHealth / 10.0f;
            }
        }
    }
}