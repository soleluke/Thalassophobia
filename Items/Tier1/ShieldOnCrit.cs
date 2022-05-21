using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Utils;

namespace Thalassophobia.Items.Tier1
{
    public class ShieldOnCrit : ItemBase<ShieldOnCrit>
    {
        public override string ItemName => "Electric Carapace";

        public override string ItemLangTokenName => "SHIELD_ON_CRIT";

        public override string ItemPickupDesc => "";

        public override string ItemFullDescription => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float shieldGain;
        private float chance;

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

            shieldGain = config.Bind<float>("Item: " + ItemName, "Shield Gained", 5f, "").Value;
            chance = config.Bind<float>("Item: " + ItemName, "Chance", 25f, "").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                orig(self);
                int count = GetCount(self);
                if (count > 0)
                {
                    Reflection.SetPropertyValue<float>(self, "crit", self.crit + 5f);
                }
            };

            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.attacker)
            {
                var count = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                if (damageInfo.crit && count > 0)
                {
                    if (Util.CheckRoll(chance, damageInfo.attacker.GetComponent<CharacterMaster>()) && !damageInfo.rejected)
                    {
                        Log.LogInfo("Adding shield - " + shieldGain);
                        damageInfo.attacker.GetComponent<CharacterBody>().healthComponent.AddBarrier(shieldGain * count * damageInfo.procCoefficient);
                    }
                }
            }
        }
    }
}