using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Utils;

namespace Thalassophobia.Items.Tier2
{
    public class DotDamageUp : ItemBase<DotDamageUp>
    {
        public override string ItemName => "Compound LETH-41";

        public override string ItemLangTokenName => "DOT_DAMAGE_UP";

        public override string ItemPickupDesc => "Damage over time effects do more damage.";

        public override string ItemFullDescription => "DoT effects do <style=cIsDamage>+7.5%</style> <style=cStack>(+7.5% per stack)</style> more damage.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float damageUp;

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

            damageUp = config.Bind<float>("Item: " + ItemName, "DamageIncrease", 0.075f, "The percent increase to DoT damage where 1.0 is 100%.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.DotController.AddDot += DotController_AddDot;
        }

        private void DotController_AddDot(On.RoR2.DotController.orig_AddDot orig, DotController self, GameObject attackerObject, float duration, DotController.DotIndex dotIndex, float damageMultiplier, uint? maxStacksFromAttacker, float? totalDamage, DotController.DotIndex? preUpgradeDotIndex)
        {
            var itemCount = GetCount(attackerObject.GetComponent<CharacterBody>());
            if (itemCount > 0)
            {
                damageMultiplier *= 1 + (damageUp * itemCount);
            }
            orig(self, attackerObject, duration, dotIndex, damageMultiplier, maxStacksFromAttacker, totalDamage, preUpgradeDotIndex);
        }
    }
}