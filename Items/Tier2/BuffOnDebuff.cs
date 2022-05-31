using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier2
{
    public class BuffOnDebuff : ItemBase<BuffOnDebuff>
    {
        public override string ItemName => "Analytical Scale";

        public override string ItemLangTokenName => "BUFF_ON_DEBUFF";

        public override string ItemPickupDesc => "Gives a buff when you are debuffed.";

        public override string ItemFullDescription => "Gives a random buff that lasts <style=cIsUtility>4</style> <style=cStack>(+2 per stack)</style> seconds when receiving a debuff. Can be <style=cIsDamage>+50%</style> damage, <style=cIsDamage>+50%</style> attack speed, <style=cIsDamage>+50%</style> crit chance and damage, <style=cIsUtility>+50%</style> movement speed, or <style=cIsHealing>+50%</style> regeneration. ";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float time;

        BuffDef damageBuff;
        BuffDef attackSpeedBuff;
        BuffDef critBuff;
        BuffDef movementBuff;
        BuffDef regenBuff;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Utility, ItemTag.Damage };

            time = config.Bind<float>("Item: " + ItemName, "Time", 4f, "").Value;

            damageBuff = ScriptableObject.CreateInstance<BuffDef>();
            damageBuff.name = "Analytical Damage";
            damageBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            damageBuff.canStack = false;
            damageBuff.isDebuff = false;
            ContentAddition.AddBuffDef(damageBuff);

            attackSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            attackSpeedBuff.name = "Analytical Attack Speed";
            attackSpeedBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            attackSpeedBuff.canStack = false;
            attackSpeedBuff.isDebuff = false;
            ContentAddition.AddBuffDef(attackSpeedBuff);

            critBuff = ScriptableObject.CreateInstance<BuffDef>();
            critBuff.name = "Analytical Critical Strikes";
            critBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            critBuff.canStack = false;
            critBuff.isDebuff = false;
            ContentAddition.AddBuffDef(critBuff);

            movementBuff = ScriptableObject.CreateInstance<BuffDef>();
            movementBuff.name = "Analytical Speed";
            movementBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            movementBuff.canStack = false;
            movementBuff.isDebuff = false;
            ContentAddition.AddBuffDef(movementBuff);

            regenBuff = ScriptableObject.CreateInstance<BuffDef>();
            regenBuff.name = "Analytical Regeneration";
            regenBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            regenBuff.canStack = false;
            regenBuff.isDebuff = false;
            ContentAddition.AddBuffDef(regenBuff);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            On.RoR2.CharacterBody.AddBuff_BuffIndex += CharacterBody_AddBuff_BuffIndex;

            On.RoR2.DotController.AddDot += DotController_AddDot;
        }

        private void CharacterBody_AddBuff_BuffIndex(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            orig(self, buffType);
            if (GetCount(self) > 0 && BuffCatalog.GetBuffDef(buffType).isDebuff && !BuffCatalog.GetBuffDef(buffType).isHidden)
            {
                GiveBuff(self);
            }
            if (Plugin.DEBUG)
            {
                Log.LogInfo("" + BuffCatalog.GetBuffDef(buffType).name);
            }
        }

        private void DotController_AddDot(On.RoR2.DotController.orig_AddDot orig, DotController self, GameObject attackerObject, float duration, DotController.DotIndex dotIndex, float damageMultiplier, uint? maxStacksFromAttacker, float? totalDamage, DotController.DotIndex? preUpgradeDotIndex)
        {
            orig(self, attackerObject, duration, dotIndex, damageMultiplier, maxStacksFromAttacker, totalDamage, preUpgradeDotIndex);
            if (attackerObject.GetComponent<CharacterBody>())
            {
                CharacterBody body = attackerObject.GetComponent<CharacterBody>();
                if (GetCount(body) > 0)
                {
                    GiveBuff(body);
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(damageBuff))
            {
                args.damageMultAdd += 0.50f;
            }

            if (sender.HasBuff(attackSpeedBuff))
            {
                args.attackSpeedMultAdd += 0.50f;
            }

            if (sender.HasBuff(critBuff))
            {
                args.critAdd += 50f;
                args.critDamageMultAdd += 0.5f;
            }

            if (sender.HasBuff(movementBuff))
            {
                args.moveSpeedMultAdd += 0.5f;
            }

            if (sender.HasBuff(regenBuff))
            {
                args.regenMultAdd += 0.5f;
            }
        }

        private void GiveBuff(CharacterBody body)
        {
            int buffIndex = Random.Range(0, 4);
            switch (buffIndex)
            {
                case 0:
                    body.AddTimedBuff(damageBuff, 4 + (2 * (GetCount(body) - 1)));
                    break;
                case 1:
                    body.AddTimedBuff(attackSpeedBuff, 4 + (2 * (GetCount(body) - 1)));
                    break;
                case 2:
                    body.AddTimedBuff(critBuff, 4 + (2 * (GetCount(body) - 1)));
                    break;
                case 3:
                    body.AddTimedBuff(movementBuff, 4 + (2 * (GetCount(body) - 1)));
                    break;
                case 4:
                    body.AddTimedBuff(regenBuff, 4 + (2 * (GetCount(body) - 1)));
                    break;
            }
        }
    }
}