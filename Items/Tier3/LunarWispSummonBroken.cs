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

        public override string ItemFullDescription => "Increase all stats of <style=cIsUtility>allied wisps</style> by <style=cIsDamage>50%</style> <style=cStack>(+50% per stack)</style>. <style=cIsUtility>Allied wisps</style> spawn as <style=cIsDamage>elites</style> and become <style=cIsUtility>stronger the more allies you have</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.NoTier;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("Assets/Assembly/MyAssets/Models/");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/CMaskBrokenIcon.png");

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