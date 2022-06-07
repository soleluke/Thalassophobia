using BepInEx.Configuration;
using R2API;
using RoR2;
using Thalassophobia.Items.Lunar;
using Thalassophobia.Utils;
using UnityEngine;
using static On.RoR2.GlobalEventManager;
using static RoR2.DotController;

namespace Thalassophobia.Items.Tier2
{
    public class GreaterWispSummonBroken : ItemBase<GreaterWispSummonBroken>
    {
        public override string ItemName => "Greater Mask (Broken)";

        public override string ItemLangTokenName => "GREATER_WISP_SUMMON_BROKEN";

        public override string ItemPickupDesc => "Allied wisp damage and attack speed up.";

        public override string ItemFullDescription => "<style=cIsDamage>Damage and attack speed</style> of <style=cIsUtility>allied wisps</style> is increased by <style=cIsDamage>20%</style> <style=cStack>(+20% per stack)</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.NoTier;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("Assets/Assembly/MyAssets/Models/");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/GMaskBrokenIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();

            WispSummonController.tier2BrokenDef = this.ItemDef;
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