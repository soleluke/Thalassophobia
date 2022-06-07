using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier2
{
    public class CritOnBuffs : ItemBase<CritOnBuffs>
    {
        public override string ItemName => "Memory Chip";

        public override string ItemLangTokenName => "CRIT_ON_BUFFS";

        public override string ItemPickupDesc => "Gain critical strike chance for every buff you have.";

        public override string ItemFullDescription => "Gain <style=cIsDamage>+5%</style> <style=cStack>(+3% per stack)</style> critical strike chance for every <style=cIsUtility>unique buff</style> you have.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("Assets/Assembly/MyAssets/Models/MemoryChipModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/MemoryChipIcon.png");

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
                float critUp = 0;
                foreach (BuffIndex buffIndex in sender.activeBuffsList)
                {
                    if (!BuffCatalog.GetBuffDef(buffIndex).isDebuff && !BuffCatalog.GetBuffDef(buffIndex).isCooldown && !BuffCatalog.GetBuffDef(buffIndex).isHidden) 
                    {
                        critUp += 5 + (3 * (count - 1));
                    }
                }
                args.critAdd += critUp;
            }
        }
    }
}