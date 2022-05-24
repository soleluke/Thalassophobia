using BepInEx.Configuration;
using R2API;
using RoR2;
using Thalassophobia.Items.Lunar;
using Thalassophobia.Utils;
using UnityEngine;
using static On.RoR2.GlobalEventManager;
using static RoR2.DotController;

namespace Thalassophobia.Items.Tier1
{
    public class WispSummonBroken : ItemBase<WispSummonBroken>
    {
        public override string ItemName => "Fragile Mask (Broken)";

        public override string ItemLangTokenName => "WISP_SUMMON_BROKEN";

        public override string ItemPickupDesc => "Alled wisp HP increased.";

        public override string ItemFullDescription => "HP of allied wisps is increased by 20% (+20% per stack).";

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

            WispSummonController.tier1BrokenDef = this.ItemDef;
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] {};
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
        }
    }
}