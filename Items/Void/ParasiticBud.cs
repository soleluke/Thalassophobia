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
    class ParasiticBud : ItemBase<ParasiticBud>
    {
        public override string ItemName => "Parasitic Bud";

        public override string ItemLangTokenName => "VOID_HEALING_SEED";

        public override string ItemPickupDesc => "<style=cIsVoid>Corrupts all Leeching Seeds.</style> Hitting an enemy leeches that enemy.";

        public override string ItemFullDescription => "Hitting an enemy leeches that enemy. Leeched enemies heal you for <style=cIsHealing>1 HP</style> <style=cStack>(+1 HP per stack)</style> repeatedly for 5 seconds. Only 1 stack of leech can be applied to an enemy. <style=cIsVoid>Corrupts all Leeching Seeds.</style>";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override String CorruptsItem => RoR2Content.Items.Seed.nameToken;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("Assets/Assembly/MyAssets/Models/BudModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/VoidBudIcon.png");

        // Buff
        public BuffDef voidLeech;

        // Custom DoT for the acid
        public DotIndex leechDotIndex;

        // Item stats
        private float healing;
        private float duration;

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

            duration = config.Bind<float>("Item: " + ItemName, "Duration", 5f, "").Value;
            healing = config.Bind<float>("Item: " + ItemName, "Healing", 1f, "").Value;

            voidLeech = ScriptableObject.CreateInstance<BuffDef>();
            voidLeech.name = "Inhabited";
            voidLeech.iconSprite = Plugin.assetBundle.LoadAsset<Sprite>("BudIcon.png");
            voidLeech.canStack = false;
            voidLeech.isDebuff = true;
            ContentAddition.AddBuffDef(voidLeech);

            DotController.DotDef leechDef = new DotController.DotDef();
            leechDef.associatedBuff = voidLeech;
            leechDef.damageCoefficient = 0.05f;
            leechDef.damageColorIndex = DamageColorIndex.Void;
            leechDef.interval = 0.5f;
            leechDef.resetTimerOnAdd = true;
            leechDotIndex = DotAPI.RegisterDotDef(leechDef, (self, dotStack) =>
            {

            });
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);
                OnHitEffect(damageInfo, victim);
            };
            LunarDice.hook_DiceReroll += OnHitEffect;

            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
            {
                orig(self, damageInfo);
                //Log.LogInfo(damageInfo.dotIndex == leechDotIndex);
                if (damageInfo.dotIndex == leechDotIndex)
                {
                    RoR2.Orbs.HealOrb orb = new RoR2.Orbs.HealOrb();
                    orb.origin = self.transform.position;
                    orb.target = damageInfo.attacker.GetComponent<CharacterBody>().mainHurtBox;
                    orb.healValue = 1 * GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                    orb.scaleOrb = true;
                    //Log.LogInfo(orb.origin.x + ", " + orb.origin.y);
                    RoR2.Orbs.OrbManager.instance.AddOrb(orb);
                }
            };
        }


        public void OnHitEffect(global::RoR2.DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker)
            {
                var leechCount = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                if (leechCount > 0)
                {
                    InflictDot(victim, damageInfo.attacker, leechDotIndex, 5, 1, 1);
                }
            }
        }
    }
}
