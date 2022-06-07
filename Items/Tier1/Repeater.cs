using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Utils;
using Thalassophobia.Utils;

namespace Thalassophobia.Items.Tier1
{
    public class Repeater : ItemBase<Repeater>
    {
        public override string ItemName => "Repeater";

        public override string ItemLangTokenName => "REPEATER";

        public override string ItemPickupDesc => "Every 5th attack does more damage.";

        public override string ItemFullDescription => "Every 5th attack deals <style=cIsDamage>20%</style> <style=cStack>(+20% per stack)</style> more damage.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("RepeaterModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("RepeaterIcon.png");

        DamageColorIndex customColor;

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

            customColor = CustomDamageColor.AddColor(new Color(1f, 0.549f, 0f));
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker)
            {
                if (damageInfo.attacker.GetComponent<CharacterBody>())
                {
                    CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (GetCount(body) > 0)
                    {
                        if (body.gameObject.GetComponent<RepeaterController>())
                        {
                            RepeaterController controller = body.gameObject.GetComponent<RepeaterController>();
                            controller.hits++;
                            if (controller.hits >= 5)
                            {
                                damageInfo.damage *= 1 + (GetCount(body) * 0.2f);
                                damageInfo.damageColorIndex = customColor;
                                controller.hits = 0;
                            }
                        }
                        else
                        {
                            RepeaterController controller = body.gameObject.AddComponent<RepeaterController>();
                            controller.hits++;
                        }
                    }
                }
            }
            orig(self, damageInfo);
        }
    }

    class RepeaterController : MonoBehaviour
    {
        public int hits = 0;
    }
}