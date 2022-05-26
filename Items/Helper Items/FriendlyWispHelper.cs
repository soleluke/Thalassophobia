using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Utils;
using Thalassophobia.Utils;

namespace Thalassophobia.Items.Tier1
{
    public class FriendlyWispHelper : ItemBase<FriendlyWispHelper>
    {
        public override string ItemName => "WISP_HELPER";

        public override string ItemLangTokenName => "WISP_HELPER";

        public override string ItemPickupDesc => "";

        public override string ItemFullDescription => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.NoTier;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();

            WispSummonController.helperDef = this.ItemDef;
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] {
                ItemTag.AIBlacklist,
                ItemTag.BrotherBlacklist,
                ItemTag.CannotCopy,
                ItemTag.CannotDuplicate,
                ItemTag.CannotSteal
            };
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
            int count = GetCount(sender);
            if (count > 0)
            {
                args.baseRegenAdd += sender.level * (0.02f * count);
                args.moveSpeedMultAdd += 0.25f;
                args.cooldownMultAdd -= 0.2f + 0.02f * count;
            }
        }
    }
}