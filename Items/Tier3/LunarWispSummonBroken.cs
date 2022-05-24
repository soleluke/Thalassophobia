using BepInEx.Configuration;
using R2API;
using RoR2;
using Thalassophobia.Items.Lunar;
using Thalassophobia.Utils;
using UnityEngine;
using static On.RoR2.GlobalEventManager;
using static RoR2.DotController;

namespace Thalassophobia.Items.Tier3
{
    public class LunarWispSummonBroken : ItemBase<LunarWispSummonBroken>
    {
        public override string ItemName => "Chimeric Mask (Broken)";

        public override string ItemLangTokenName => "LUNAR_WISP_SUMMON_BROKEN";

        public override string ItemPickupDesc => "Your wisps become incredibly powerful.";

        public override string ItemFullDescription => "All allied wisps gain 50% (+50% per stack) increased stats, and spawn as elites. Wisps become stronger the more allies you have.";

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

            WispSummonController.tier3BrokenDef = this.ItemDef;
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