using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using Thalassophobia.Items.Lunar;
using Thalassophobia.Utils;
using UnityEngine;
using static On.RoR2.DotController;
using static RoR2.DotController;

namespace Thalassophobia.Items.Void
{
    class HuntersMark : ItemBase<HuntersMark>
    {
        public override string ItemName => "Hunters Mark";

        public override string ItemLangTokenName => "HUNTERS_MARK";

        public override string ItemPickupDesc => "<style=cIsVoid>Corrupts all Predatory Instincts.</style> Critical strikes increase your chance to activate other items.";

        public override string ItemFullDescription => "<style=cIsUtility>Critical strikes</style> increase your chance to activate other items by <style=cIsUtility>1%</style>. Maximum cap of <style=cIsUtility>10%</style> <style=cStack>(+5% per stack)</style> activation chance. <style=cIsVoid>Corrupts all Predatory Instincts.</style>";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override String CorruptsItem => RoR2Content.Items.Seed.nameToken;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("HuntersMarkModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("HuntersMarkIcon.png");

        // Buff
        public BuffDef buff;


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

            buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.name = "Lucky";
            buff.iconSprite = Plugin.assetBundle.LoadAsset<Sprite>("VoidDice.png");
            buff.canStack = true;
            buff.isDebuff = false;
            buff.isHidden = false;
            buff.canStack = true;
            ContentAddition.AddBuffDef(buff);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.Util.CheckRoll_float_float_CharacterMaster += Util_CheckRoll_float_float_CharacterMaster;

            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (GetCount(sender) > 0) {
                args.critAdd += 5f;
            }
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>()) 
            {
                CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
                if (damageInfo.crit && GetCount(body) > 0)
                {
                    body.AddTimedBuff(buff, 3.0f * damageInfo.procCoefficient, 10 + (GetCount(body) - 1) * 5);
                }
            }
        }

        private bool Util_CheckRoll_float_float_CharacterMaster(On.RoR2.Util.orig_CheckRoll_float_float_CharacterMaster orig, float percentChance, float luck, CharacterMaster effectOriginMaster)
        {
            if (effectOriginMaster) {
                GameObject bodyObject = effectOriginMaster.GetBodyObject();
                if (bodyObject)
                {
                    CharacterBody component = bodyObject.GetComponent<CharacterBody>();
                    if (component)
                    {
                        if (percentChance > 1f)
                        {
                            int count = component.GetBuffCount(buff);
                            percentChance += count;
                        }
                    }
                }
            }

            return orig(percentChance, luck, effectOriginMaster);
        }
    }
}
