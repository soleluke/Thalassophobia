using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Utils;

namespace Thalassophobia.Items.Tier1
{
    public class CritDamageUp : ItemBase<CritDamageUp>
    {
        public override string ItemName => "Military Training";

        public override string ItemLangTokenName => "CRIT_DAMAGE_UP";

        public override string ItemPickupDesc => "Increases crit chance and crit damage.";

        public override string ItemFullDescription => "Increases chance to 'critically strike' by <style=cIsDamage>5%</style> <style=cStack>(+5% per stack)</style>, and increases 'critical strike' damage by  <style=cIsDamage>5%</style> <style=cStack>(+5% per stack)</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("MilTrainingModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("MilitaryTrainingIcon.png");

        // Item stats
        private float damageUp;
        private float critUp;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Damage };

            damageUp = config.Bind<float>("Item: " + ItemName, "CritDamageUp", 0.05f, "Percent increase to crit damage where 1.0 is 100%.").Value;
            critUp = config.Bind<float>("Item: " + ItemName, "CritChanceIncrease", 5f, "Percent crit chance increase.").Value;
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
            int critDamageUpCount = GetCount(sender);
            if (critDamageUpCount > 0)
            {
                args.critAdd += critUp * critDamageUpCount;
                args.critDamageMultAdd += damageUp * critDamageUpCount;
            }
        }
    }
}