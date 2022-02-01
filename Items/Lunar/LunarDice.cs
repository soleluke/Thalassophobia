using BepInEx;
using HG;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.DotController;
using static RoR2Mod.RoR2ModPlugin;
using static RoR2Mod.ItemManager;
using BepInEx.Configuration;
using RoR2Mod.Items.Tier1;
using RoR2Mod.Items.Tier2;

namespace RoR2Mod.Items.Lunar
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

        public override GameObject ItemModel => AssetManager.GetPrefab(AssetManager.ItemPrefabIndex.LunarDice);

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
                    BUILDER.Append("REROLL COUNT: " + rerollCount + "\n");
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
                            BUILDER.Append("Missed");
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

                        // Starting string BUILDER for logging
                        BUILDER.Clear();
                        BUILDER.Append("STARTING ROLLS\n");

                        // Checks for initial validity
                        if (!IsValid(damageInfo))
                        {
                            BUILDER.Append("END OF ROLLS - INVALID");
                            if (RoR2ModPlugin.DEBUG)
                            {
                                Log.LogInfo(BUILDER.ToString());
                            }
                            return;
                        }

                        // Initial info
                        BUILDER.Append(
                            "Attacker: " + damageInfo.attacker.name + "\n" +
                            "Damage Type: " + damageInfo.damageType + "\n" +
                            "ProcChainMask: \n");
                        damageInfo.procChainMask.AppendToStringBuilder(BUILDER);
                        BUILDER.Append("\n");

                        // Rolls
                        for (float i = 0; i < numRolls; i++)
                        {
                            // Decreasing proc coefficient and logging data
                            DecreaseProcCoefficient(damageInfo, i, numRolls);
                            if (!IsValid(damageInfo))
                            {
                                BUILDER.Append("ROLL DISCARDED\n");
                                break;
                            }
                            BUILDER.Append(
                                "ROLL " + (i + 1) + "\n" +
                                "Proc Coefficient: " + damageInfo.procCoefficient + "\n");

                            // Proc
                            // Dont change procChainMask it works like an int so when you assign it to a new variable your passing a copy

                            // Vanilla proc
                            ProcChainMask procChainMask = damageInfo.procChainMask;
                            float procCoefficient = damageInfo.procCoefficient;
                            Inventory inventory = body.inventory;
                            CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                            TeamComponent teamComponent = body.GetComponent<TeamComponent>();
                            TeamIndex teamIndex = teamComponent ? teamComponent.teamIndex : TeamIndex.Neutral;
                            Vector3 aimOrigin = body.aimOrigin;

                            // Bleed Dagger
                            if (!procChainMask.HasProc(ProcType.BleedOnHit))
                            {
                                int itemCount2 = inventory.GetItemCount(RoR2Content.Items.BleedOnHit);
                                bool flag = (damageInfo.damageType & DamageType.BleedOnHit) > DamageType.Generic;
                                if ((itemCount2 > 0 || flag))
                                {
                                    BUILDER.Append($"Checking bleed - proc:{10f * (float)itemCount2 * procCoefficient}\n");
                                    if ((flag || Util.CheckRoll(10f * (float)itemCount2 * procCoefficient, master)))
                                    {
                                        ProcChainMask procChainMask2 = damageInfo.procChainMask;
                                        procChainMask2.AddProc(ProcType.BleedOnHit);
                                        DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed, 3f * damageInfo.procCoefficient, 1f);
                                        BUILDER.Append($"--Bleed Proc--\n");
                                    }
                                }
                            }

                            // MISSLE
                            int missileCount = inventory.GetItemCount(RoR2Content.Items.Missile);
                            if (missileCount > 0)
                            {
                                BUILDER.Append($"Checking missile - proc:{10f * procCoefficient}\n");
                                if (!damageInfo.procChainMask.HasProc(ProcType.Missile))
                                {
                                    GlobalEventManager.instance.ProcMissile(missileCount, body, master, teamIndex, procChainMask, victim, damageInfo);
                                }
                            }

                            // UKELEL
                            int itemCount5 = inventory.GetItemCount(RoR2Content.Items.ChainLightning);
                            float num2 = 15f;
                            if (itemCount5 > 0)
                            {
                                BUILDER.Append($"Checking ukelel - proc:{num2 * damageInfo.procCoefficient}\n");
                                if (!damageInfo.procChainMask.HasProc(ProcType.ChainLightning) && Util.CheckRoll(num2 * damageInfo.procCoefficient, master))
                                {
                                    BUILDER.Append($"--Ukelel Proc--\n");
                                    float damageCoefficient2 = 0.8f;
                                    float damageValue2 = Util.OnHitProcDamage(damageInfo.damage, body.damage, damageCoefficient2);
                                    LightningOrb lightningOrb2 = new LightningOrb();
                                    lightningOrb2.origin = damageInfo.position;
                                    lightningOrb2.damageValue = damageValue2;
                                    lightningOrb2.isCrit = damageInfo.crit;
                                    lightningOrb2.bouncesRemaining = 2 * itemCount5;
                                    lightningOrb2.teamIndex = teamIndex;
                                    lightningOrb2.attacker = damageInfo.attacker;
                                    lightningOrb2.bouncedObjects = new List<HealthComponent>
                            {
                                victim.GetComponent<HealthComponent>()
                            };
                                    lightningOrb2.procChainMask = damageInfo.procChainMask;
                                    lightningOrb2.procChainMask.AddProc(ProcType.ChainLightning);
                                    lightningOrb2.procCoefficient = 0.2f * procCoefficient;
                                    lightningOrb2.lightningType = LightningOrb.LightningType.Ukulele;
                                    lightningOrb2.damageColorIndex = DamageColorIndex.Item;
                                    lightningOrb2.range += (float)(2 * itemCount5);
                                    HurtBox hurtBox2 = lightningOrb2.PickNextTarget(damageInfo.position);
                                    if (hurtBox2)
                                    {
                                        lightningOrb2.target = hurtBox2;
                                        OrbManager.instance.AddOrb(lightningOrb2);
                                    }
                                }
                            }

                            // Meat hook
                            int itemCount6 = inventory.GetItemCount(RoR2Content.Items.BounceNearby);
                            if (itemCount6 > 0)
                            {
                                float num3 = (1f - 100f / (100f + 20f * (float)itemCount6)) * 100f;
                                BUILDER.Append($"Checking meathook - proc:{num3 * damageInfo.procCoefficient}\n");
                                if (!damageInfo.procChainMask.HasProc(ProcType.BounceNearby) && Util.CheckRoll(num3 * damageInfo.procCoefficient, master))
                                {
                                    BUILDER.Append($"--Meathook Proc--\n");
                                    List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                                    int maxTargets = 5 + itemCount6 * 5;
                                    BullseyeSearch search = new BullseyeSearch();
                                    List<HealthComponent> list2 = CollectionPool<HealthComponent, List<HealthComponent>>.RentCollection();
                                    if (body && body.healthComponent)
                                    {
                                        list2.Add(body.healthComponent);
                                    }
                                    if (victim.GetComponent<CharacterBody>() && victim.GetComponent<CharacterBody>().healthComponent)
                                    {
                                        list2.Add(victim.GetComponent<CharacterBody>().healthComponent);
                                    }
                                    BounceOrb.SearchForTargets(search, teamIndex, damageInfo.position, 30f, maxTargets, list, list2);
                                    CollectionPool<HealthComponent, List<HealthComponent>>.ReturnCollection(list2);
                                    List<HealthComponent> bouncedObjects = new List<HealthComponent>
                                {
                                    victim.GetComponent<HealthComponent>()
                                };
                                    float damageCoefficient3 = 1f;
                                    float damageValue3 = Util.OnHitProcDamage(damageInfo.damage, body.damage, damageCoefficient3);
                                    int i2 = 0;
                                    int count2 = list.Count;
                                    while (i2 < count2)
                                    {
                                        HurtBox hurtBox3 = list[i2];
                                        if (hurtBox3)
                                        {
                                            BounceOrb bounceOrb = new BounceOrb();
                                            bounceOrb.origin = damageInfo.position;
                                            bounceOrb.damageValue = damageValue3;
                                            bounceOrb.isCrit = damageInfo.crit;
                                            bounceOrb.teamIndex = teamIndex;
                                            bounceOrb.attacker = damageInfo.attacker;
                                            bounceOrb.procChainMask = damageInfo.procChainMask;
                                            bounceOrb.procChainMask.AddProc(ProcType.BounceNearby);
                                            bounceOrb.procCoefficient = 0.33f * procCoefficient;
                                            bounceOrb.damageColorIndex = DamageColorIndex.Item;
                                            bounceOrb.bouncedObjects = bouncedObjects;
                                            bounceOrb.target = hurtBox3;
                                            OrbManager.instance.AddOrb(bounceOrb);
                                        }
                                        i2++;
                                    }
                                    CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
                                }
                            }

                            // Sticky Bomb
                            int itemCount7 = inventory.GetItemCount(RoR2Content.Items.StickyBomb);
                            if (itemCount7 > 0)
                            {
                                BUILDER.Append($"Checking sticky bomb - proc:{5f * (float)itemCount7 * damageInfo.procCoefficient}\n");
                                if (Util.CheckRoll(5f * (float)itemCount7 * damageInfo.procCoefficient, master) && victim.GetComponent<CharacterBody>())
                                {
                                    BUILDER.Append($"--Sticky Bomb Proc--\n");
                                    bool alive = victim.GetComponent<CharacterBody>().healthComponent.alive;
                                    float num4 = 5f;
                                    Vector3 position = damageInfo.position;
                                    Vector3 forward = victim.GetComponent<CharacterBody>().corePosition - position;
                                    float magnitude = forward.magnitude;
                                    Quaternion rotation = (magnitude != 0f) ? Util.QuaternionSafeLookRotation(forward) : UnityEngine.Random.rotationUniform;
                                    float damageCoefficient4 = 1.8f;
                                    float damage = Util.OnHitProcDamage(damageInfo.damage, body.damage, damageCoefficient4);
                                    ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/StickyBomb"), position, rotation, damageInfo.attacker, damage, 100f, damageInfo.crit, DamageColorIndex.Item, null, alive ? (magnitude * num4) : -1f);
                                }
                            }

                            // IDK saw thing why
                            if (damageInfo != null && damageInfo.inflictor != null && damageInfo.inflictor.GetComponent<BoomerangProjectile>() != null && !damageInfo.procChainMask.HasProc(ProcType.BleedOnHit))
                            {
                                int num7 = 0;
                                if (inventory.GetEquipmentIndex() == RoR2Content.Equipment.Saw.equipmentIndex)
                                {
                                    num7 = 1;
                                }
                                bool flag5 = (damageInfo.damageType & DamageType.BleedOnHit) > DamageType.Generic;
                                if ((num7 > 0 || flag5) && (flag5 || Util.CheckRoll(100f, master)))
                                {
                                    ProcChainMask procChainMask5 = damageInfo.procChainMask;
                                    procChainMask5.AddProc(ProcType.BleedOnHit);
                                    DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed, 4f * damageInfo.procCoefficient, 1f);
                                }
                            }

                            // Bleed G O D
                            if (inventory.GetItemCount(RoR2Content.Items.BleedOnHitAndExplode) > 0)
                            {
                                BUILDER.Append($"Checking Shatterspleen\n");
                                if (!damageInfo.procChainMask.HasProc(ProcType.BleedOnHit) && damageInfo.crit)
                                {
                                    BUILDER.Append($"--Shatterspleen Proc--\n");
                                    ProcChainMask procChainMask6 = damageInfo.procChainMask;
                                    procChainMask6.AddProc(ProcType.BleedOnHit);
                                    DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed, 3f * damageInfo.procCoefficient, 1f);
                                }
                            }

                            // Molten preferator
                            int itemCount11 = inventory.GetItemCount(RoR2Content.Items.FireballsOnHit);
                            if (itemCount11 > 0)
                            {
                                BUILDER.Append($"Checking Moleten Perferator - proc:{10f * procCoefficient}\n");
                                if (!damageInfo.procChainMask.HasProc(ProcType.Meatball))
                                {
                                    BUILDER.Append($"--Molten Perferator Proc--\n");
                                    InputBankTest component4 = body.GetComponent<InputBankTest>();
                                    Vector3 vector2 = victimBody.characterMotor ? (victim.transform.position + Vector3.up * (victimBody.characterMotor.capsuleHeight * 0.5f + 2f)) : (victim.transform.position + Vector3.up * 2f);
                                    Vector3 forward2 = component4 ? component4.aimDirection : victim.transform.forward;
                                    forward2 = Vector3.up;
                                    float num8 = 20f;
                                    if (Util.CheckRoll(10f * procCoefficient, master))
                                    {
                                        EffectData effectData = new EffectData
                                        {
                                            scale = 1f,
                                            origin = vector2
                                        };
                                        EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleflashFireMeatBall"), effectData, true);
                                        int num9 = 3;
                                        float damageCoefficient7 = 3f * (float)itemCount11;
                                        float damage4 = Util.OnHitProcDamage(damageInfo.damage, body.damage, damageCoefficient7);
                                        float min = 15f;
                                        float max = 30f;
                                        ProcChainMask procChainMask7 = damageInfo.procChainMask;
                                        procChainMask7.AddProc(ProcType.Meatball);
                                        float speedOverride2 = UnityEngine.Random.Range(min, max);
                                        float num10 = (float)(360 / num9);
                                        float num11 = num10 / 360f;
                                        float num12 = 1f;
                                        float num13 = num10;
                                        for (int k = 0; k < num9; k++)
                                        {
                                            float num14 = (float)k * 3.1415927f * 2f / (float)num9;
                                            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                                            {
                                                projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/FireMeatBall"),
                                                position = vector2 + new Vector3(num12 * Mathf.Sin(num14), 0f, num12 * Mathf.Cos(num14)),
                                                rotation = Util.QuaternionSafeLookRotation(forward2),
                                                procChainMask = procChainMask7,
                                                target = victim,
                                                owner = body.gameObject,
                                                damage = damage4,
                                                crit = damageInfo.crit,
                                                force = 200f,
                                                damageColorIndex = DamageColorIndex.Item,
                                                speedOverride = speedOverride2,
                                                useSpeedOverride = true
                                            };
                                            num13 += num10;
                                            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                                            forward2.x += Mathf.Sin(num14 + UnityEngine.Random.Range(-num8, num8));
                                            forward2.z += Mathf.Cos(num14 + UnityEngine.Random.Range(-num8, num8));
                                        }
                                    }
                                }
                            }

                            // Charged perforator
                            int itemCount12 = inventory.GetItemCount(RoR2Content.Items.LightningStrikeOnHit);
                            if (itemCount12 > 0)
                            {
                                BUILDER.Append($"Checking Charged Perferator - proc:{10f * procCoefficient}\n");
                                if (!damageInfo.procChainMask.HasProc(ProcType.LightningStrikeOnHit) && Util.CheckRoll(10f * procCoefficient, master))
                                {
                                    BUILDER.Append($"--Charged Perferator Proc--\n");
                                    float damageValue4 = Util.OnHitProcDamage(damageInfo.damage, body.damage, 5f * (float)itemCount12);
                                    ProcChainMask procChainMask8 = damageInfo.procChainMask;
                                    procChainMask8.AddProc(ProcType.LightningStrikeOnHit);
                                    HurtBox target = victimBody.mainHurtBox;
                                    if (victimBody.hurtBoxGroup)
                                    {
                                        target = victimBody.hurtBoxGroup.hurtBoxes[UnityEngine.Random.Range(0, victimBody.hurtBoxGroup.hurtBoxes.Length)];
                                    }
                                    OrbManager.instance.AddOrb(new SimpleLightningStrikeOrb
                                    {
                                        attacker = body.gameObject,
                                        damageColorIndex = DamageColorIndex.Item,
                                        damageValue = damageValue4,
                                        isCrit = Util.CheckRoll(body.crit, master),
                                        procChainMask = procChainMask8,
                                        procCoefficient = procCoefficient,
                                        target = target
                                    });
                                }
                            }

                            // My Proc

                            // Acid Rounds
                            ConfigEntry<float> out1;
                            config.TryGetEntry("Item: Acidic Rounds", "ProcChance", out out1);
                            ConfigEntry<float> out2;
                            config.TryGetEntry("Item: Acidic Rounds", "Duration", out out2);
                            float chance = out1.Value;
                            float duration = out2.Value;
                            var acidCount = AcidOnHit.instance.GetCount(body);
                            if (acidCount > 0)
                            {
                                if (Util.CheckRoll(chance * damageInfo.procCoefficient, damageInfo.attacker.GetComponent<CharacterMaster>()) && !damageInfo.rejected)
                                {
                                    DotController.InflictDot(victim, damageInfo.attacker, AcidOnHit.instance.acidDoTIndex, duration, 1f);
                                }
                            }

                            // Bean
                            var beanCount = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                            if (beanCount > 0 && !damageInfo.procChainMask.HasProc((ProcType)(ProcType.Count + 1)))
                            {
                                ConfigEntry<float> out3;
                                config.TryGetEntry("Item: Can Of Beans", "ProcChance", out out3);
                                ConfigEntry<float> out4;
                                config.TryGetEntry("Item: Can Of Beans", "DamageCoefficient", out out4);
                                ConfigEntry<float> out5;
                                config.TryGetEntry("Item: Can Of Beans", "ExplosionRadius", out out5);
                                ConfigEntry<float> out6;
                                config.TryGetEntry("Item: Can Of Beans", "ExplosionRadiusIncrease", out out6);
                                ConfigEntry<float> out7;
                                config.TryGetEntry("Item: Can Of Beans", "MaxBeans", out out7);
                                ConfigEntry<float> out8;
                                config.TryGetEntry("Item: Can Of Beans", "MinBeans", out out8);
                                if (Util.CheckRoll(out3.Value * damageInfo.procCoefficient, damageInfo.attacker.GetComponent<CharacterMaster>()) && !damageInfo.rejected)
                                {
                                    GameObject projectilePrefab = AssetManager.GetProjectile(AssetManager.ProjectileIndex.Bean);
                                    ProcChainMask procMask = damageInfo.procChainMask;
                                    procMask.AddProc((ProcType)(ProcType.Count + 1));
                                    FireProjectileInfo info = new FireProjectileInfo();
                                    info.procChainMask = procMask;
                                    info.projectilePrefab = projectilePrefab;
                                    info.position = damageInfo.position;
                                    info.owner = damageInfo.attacker;
                                    info.damage = damageInfo.attacker.GetComponent<CharacterBody>().damage * out4.Value;
                                    info.crit = false;
                                    projectilePrefab.GetComponent<ProjectileImpactExplosion>().blastRadius = out5.Value + (out6.Value * (beanCount - 1));
                                    int num = Mathf.RoundToInt(Random.Range(out8.Value, out7.Value));
                                    for (; num > 0; num--)
                                    {
                                        Quaternion quat;
                                        quat = Quaternion.Euler(-75.0f, Random.Range(0.0f, 360.0f), 0.0f);
                                        info.rotation = quat;
                                        ProjectileManager.instance.FireProjectile(info);
                                    }
                                }
                            }

                            // End of roll
                            BUILDER.Append("----------------End of Roll----------------\n");
                        }

                        // Reached end of rolls
                        BUILDER.Append("END OF ROLLS");
                        if (RoR2ModPlugin.DEBUG)
                        {
                            Log.LogInfo(BUILDER.ToString());
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