using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static On.RoR2.CharacterBody;

namespace Thalassophobia.Items.Tier2
{
    public class EliteKillCritUp : ItemBase<EliteKillCritUp>
    {
        public override string ItemName => "Conductive Sap";

        public override string ItemLangTokenName => "ELITE_KILL_CRIT_UP";

        public override string ItemPickupDesc => "Killing an elite monster grants critical strike chance";

        public override string ItemFullDescription => "Killing an elite gives you <style=cIsDamage>+10%</style> <style=cStack>(+10% per stack)</style> critical strike chance for <style=cIsUtility>7</style> seconds.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        static BuffDef critUp;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Utility, ItemTag.OnKillEffect };

            critUp = ScriptableObject.CreateInstance<BuffDef>();
            critUp.name = "Elite Kill Critical Strike";
            critUp.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            critUp.canStack = true;
            critUp.isDebuff = false;
            critUp.isCooldown = false;
            critUp.buffColor = Color.red;
            ContentAddition.AddBuffDef(critUp);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(critUp))
            {
                args.critAdd += 10 * GetCount(sender);
            }
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            CharacterBody body = damageReport.attackerBody;
            int count = GetCount(body);
            if (damageReport.victimIsElite && count > 0)
            {
                body.AddTimedBuff(critUp, 7);
            }
        }
    }
}