using HG;
using R2API;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using Thalassophobia.Items.Tier1;


namespace Thalassophobia.Items.Lunar
{
    public class LunarDice : ItemBase<LunarDice>
    {
        public override string ItemName => "Eulogy of a Gambler";

        public override string ItemLangTokenName => "LUNAR_DICE";

        public override string ItemPickupDesc => "You feel lucky, but you have no skill.";

        public override string ItemFullDescription => $"All random on hit effects are rolled <style=cIsUtility>+1</style> <style=cStack>(+1 per stack)</style> " +
                "times regardless of outcome allowing items to <style=cIsUtility>proc multiple times</style>. All attacks gain a " +
                $"<style=cDeath>20% chance to miss</style> <style=cStack>(+20% per stack)</style>.\n" +
                "<style=cArtifact>Skill Issue</style>";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        private float missChance;
        private int rolls;
        private ConfigFile config;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Cleansable };

            this.config = config;
            missChance = config.Bind<float>("Item: " + ItemName, "MissChance", 20f, "Percent chance per item obtained that you miss.").Value;
            rolls = config.Bind<int>("Item: " + ItemName, "Rolls", 1, "Number of chances to proc an item multiple times.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            // Miss chance
            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
            {
                if (damageInfo.attacker)
                {
                    float rerollCount = rolls * GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                    Plugin.BUILDER.Append("REROLL COUNT: " + rerollCount + "\n");
                    for (int i = 0; i < rerollCount; i++)
                    {
                        if (Util.CheckRoll(missChance, 0f, null))
                        {
                            EffectData effectData = new EffectData
                            {
                                origin = damageInfo.position,
                                rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
                            };
                            EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearEffectPrefab, effectData, true);
                            Plugin.BUILDER.Append("Missed");
                            damageInfo.rejected = true;
                            break;
                        }
                    }
                }
                orig(self, damageInfo);
            };

            // Extra rolls
            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);
                if (damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>())
                {
                    var count = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                    float numRolls = count * rolls;

                    if (count > 0)
                    {
                        CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
                        CharacterMaster master = body.master;

                        // Starting string Plugin.BUILDER for logging
                        Plugin.BUILDER.Clear();
                        Plugin.BUILDER.Append("STARTING ROLLS\n");

                        // Checks for initial validity
                        if (!IsValid(damageInfo))
                        {
                            Plugin.BUILDER.Append("END OF ROLLS - INVALID");
                            if (Plugin.DEBUG)
                            {
                                Log.LogInfo(Plugin.BUILDER.ToString());
                            }
                            return;
                        }

                        // Initial info
                        Plugin.BUILDER.Append(
                            "Attacker: " + damageInfo.attacker.name + "\n" +
                            "Damage Type: " + damageInfo.damageType + "\n" +
                            "ProcChainMask: \n");
                        damageInfo.procChainMask.AppendToStringBuilder(Plugin.BUILDER);
                        Plugin.BUILDER.Append("\n");

                        // Rolls
                        for (float i = 0; i < numRolls; i++)
                        {
                        }

                        // Reached end of rolls
                        Plugin.BUILDER.Append("END OF ROLLS");
                        if (Plugin.DEBUG)
                        {
                            Log.LogInfo(Plugin.BUILDER.ToString());
                        }
                    }
                }
            };
        }

        private void DecreaseProcCoefficient(DamageInfo damageInfo, float currentRole, float maxRolls)
        {
            float procDecrease = (damageInfo.procCoefficient * ((currentRole + 1f) / (maxRolls + 4f)));
            damageInfo.procCoefficient = damageInfo.procCoefficient - (procDecrease);
            if (damageInfo.procCoefficient < 0.05f)
            {
                damageInfo.procCoefficient = 0.0f;
            }
        }

        private bool IsValid(DamageInfo damageInfo)
        {
            if (damageInfo.procCoefficient <= 0)
            {
                return false;
            }

            if (damageInfo.rejected)
            {
                return false;
            }

            return true;
        }
    }
}