using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier2
{
    public class HealingShields : ItemBase<HealingShields>
    {
        public override string ItemName => "Charging Nanobots";

        public override string ItemLangTokenName => "HEALING_SHIELDS";

        public override string ItemPickupDesc => "Turn full shields into a large amount of regeneration. Shields recharge quicker.";

        public override string ItemFullDescription => "When you have full shields and are missing health, gain <style=cIsHealing>+5 hp/s</style> <style=cStack>(+5 hp/s per stack)</style> regeneration and drain your shields. The regeneration lasts until your shields are completely drained. Shields recharge <style=cIsUtility>15%</style> faster and take <style=cIsUtility>15%</style> less time to start recharging.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        BuffDef regen;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Utility, ItemTag.Healing };

            regen = ScriptableObject.CreateInstance<BuffDef>();
            regen.name = "Nanobots Regeneration";
            regen.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            regen.canStack = false;
            regen.isDebuff = false;
            regen.isHidden = true;
            regen.buffColor = Color.green;
            ContentAddition.AddBuffDef(regen);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if (GetCount(self) > 0)
            {
                if ((self.healthComponent.health < self.healthComponent.fullHealth)
                    && self.healthComponent.shield == self.healthComponent.fullShield)
                {
                    self.AddBuff(regen);
                }
            }
            if (self.HasBuff(regen))
            {
                self.healthComponent.shield -= 1;
                if ((self.healthComponent.health == self.healthComponent.fullHealth)
                    || self.healthComponent.shield <= 0)
                {
                    self.RemoveBuff(regen);
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var count = GetCount(sender);
            if (sender.GetBuffCount(regen) > 0)
            {
                args.regenMultAdd += 10f * count;
            }
            if (count > 0)
            {
                args.baseShieldAdd += sender.healthComponent.fullHealth / 10.0f;
            }
        }
    }
}