using HG;
using R2API;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using Thalassophobia.Items.Tier1;
using static On.RoR2.GlobalEventManager;
using Thalassophobia;
using System.Text;

namespace Thalassophobia.Items.Lunar
{
    public class LunarDice : ItemBase<LunarDice>
    {
        public override string ItemName => "Eulogy of a Gambler";

        public override string ItemLangTokenName => "LUNAR_DICE";

        public override string ItemPickupDesc => "You feel lucky, but you have no skill.";

        public override string ItemFullDescription => "All random on hit effects are rolled <style=cIsUtility>+1</style> <style=cStack>(+1 per stack)</style> " +
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

        private BuffDef diceTimer;

        public delegate void DiceReroll(global::RoR2.DamageInfo damageInfo, GameObject victim);
        public static DiceReroll hook_DiceReroll;

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
            missChance = config.Bind<float>("Item: " + ItemName, "MissChance", 10f, "Percent chance per item obtained that you miss.").Value;
            rolls = config.Bind<int>("Item: " + ItemName, "Rolls", 1, "Number of chances to proc an item multiple times.").Value;

            diceTimer = ScriptableObject.CreateInstance<BuffDef>();
            diceTimer.name = "Reroll";
            diceTimer.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            diceTimer.canStack = true;
            diceTimer.isDebuff = true;
            ContentAddition.AddBuffDef(diceTimer);
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

                    if (count > 0 && IsValid(damageInfo))
                    {
                        // Rolls
                        if (!victim.GetComponent<CharacterBody>().HasBuff(diceTimer) && !victim.GetComponent<DiceStack>())
                        {
                            DiceStack stack = victim.AddComponent<DiceStack>();
                            DamageInfo newInfo = new DamageInfo();
                            stack.damageInfo = damageInfo;
                            stack.currentRoll = 1;
                            stack.maxRolls = (int)numRolls;
                            for (float i = 0; i < numRolls; i++)
                            {
                                victim.GetComponent<CharacterBody>().AddTimedBuff(diceTimer, (1/numRolls) + (i * (1/numRolls)) + 0.2f, GetCount(damageInfo.attacker.GetComponent<CharacterBody>()));
                            }
                        }
                    }
                }
            };

            On.RoR2.CharacterBody.RemoveBuff_BuffIndex += (orig, self, buffIndex) =>
            {
                if (BuffCatalog.GetBuffDef(buffIndex) == diceTimer)
                {
                    DiceStack stack = self.gameObject.GetComponent<DiceStack>();
                    DamageInfo damageInfo = stack.damageInfo;
                    StringBuilder builder = new StringBuilder();
                    Log.LogInfo("Checking Rolls");
                    if (IsValid(damageInfo))
                    {
                        builder.Append("Starting roll: " + stack.currentRoll);
                        VanillaProc(damageInfo, self.gameObject, builder);
                        hook_DiceReroll(damageInfo, self.gameObject);
                        DecreaseProcCoefficient(damageInfo, stack.currentRoll, stack.maxRolls);

                        if (Plugin.DEBUG)
                        {
                            Log.LogInfo(builder.ToString());
                        }
                    }
                    if (stack.currentRoll == stack.maxRolls)
                    {
                        Object.Destroy(stack);
                    }
                    else
                    {
                        stack.currentRoll += 1;
                    }

                }
                orig(self, buffIndex);
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

        // Practically just copied code from the vanilla game with some minor modifications
        private void VanillaProc(global::RoR2.DamageInfo damageInfo, GameObject victim, StringBuilder builder)
        {
            if (damageInfo.attacker && damageInfo.procCoefficient > 0f)
            {
                // Max stacks i guess
                uint? maxStacksFromAttacker = null;
                if ((damageInfo != null) ? damageInfo.inflictor : null)
                {
                    ProjectileDamage component = damageInfo.inflictor.GetComponent<ProjectileDamage>();
                    if (component && component.useDotMaxStacksFromAttacker)
                    {
                        maxStacksFromAttacker = new uint?(component.dotMaxStacksFromAttacker);
                    }
                }

                // Setting up the variables and stuff
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    CharacterMaster master = attackerBody.master;
                    if (master)
                    {
                        Inventory inventory = master.inventory;
                        TeamComponent attackerTeamComponent = attackerBody.GetComponent<TeamComponent>();
                        TeamIndex attackerTeamIndex = attackerTeamComponent ? attackerTeamComponent.teamIndex : TeamIndex.Neutral;
                        Vector3 aimOrigin = attackerBody.aimOrigin;

                        if (damageInfo.crit)
                        {
                            GlobalEventManager.instance.OnCrit(attackerBody, damageInfo, master, damageInfo.procCoefficient, damageInfo.procChainMask);
                        }

                        // Leech Seed
                        if (!damageInfo.procChainMask.HasProc(ProcType.HealOnHit))
                        {
                            int itemCount = inventory.GetItemCount(RoR2Content.Items.Seed);
                            if (itemCount > 0)
                            {
                                HealthComponent component4 = attackerBody.GetComponent<HealthComponent>();
                                if (component4)
                                {
                                    builder.Append("    Leech Seed\n");
                                    ProcChainMask procChainMask = damageInfo.procChainMask;
                                    procChainMask.AddProc(ProcType.HealOnHit);
                                    component4.Heal((float)itemCount * damageInfo.procCoefficient, procChainMask, true);
                                }
                            }
                        }

                        // BLEED
                        if (!damageInfo.procChainMask.HasProc(ProcType.BleedOnHit))
                        {
                            bool flag = (damageInfo.damageType & DamageType.BleedOnHit) > DamageType.Generic || (inventory.GetItemCount(RoR2Content.Items.BleedOnHitAndExplode) > 0 && damageInfo.crit);
                            if ((attackerBody.bleedChance > 0f || flag) && (flag || Util.CheckRoll(damageInfo.procCoefficient * attackerBody.bleedChance, master)))
                            {
                                builder.Append("    Tri Tip Dagger\n");
                                ProcChainMask procChainMask2 = damageInfo.procChainMask;
                                procChainMask2.AddProc(ProcType.BleedOnHit);
                                DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed, 3f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
                                Log.LogInfo("TriTip");
                            }
                        }

                        // NEEDLE TICK
                        if (!damageInfo.procChainMask.HasProc(ProcType.FractureOnHit))
                        {
                            int num = inventory.GetItemCount(DLC1Content.Items.BleedOnHitVoid);
                            num += (attackerBody.HasBuff(DLC1Content.Buffs.EliteVoid) ? 10 : 0);
                            if (num > 0 && Util.CheckRoll(damageInfo.procCoefficient * (float)num * 10f, master))
                            {
                                builder.Append("    NeedleTick\n");
                                ProcChainMask procChainMask3 = damageInfo.procChainMask;
                                procChainMask3.AddProc(ProcType.FractureOnHit);
                                DotController.DotDef dotDef = DotController.GetDotDef(DotController.DotIndex.Fracture);
                                DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Fracture, dotDef.interval, 1f, maxStacksFromAttacker);
                            }
                        }

                        // WEAK ON HIT
                        if ((damageInfo.damageType & DamageType.WeakOnHit) > DamageType.Generic)
                        {
                            builder.Append("    Weak On Hit\n");
                            victimBody.AddTimedBuff(RoR2Content.Buffs.Weak, 6f * damageInfo.procCoefficient);
                        }

                        // BLIGHT
                        bool flag2 = (damageInfo.damageType & DamageType.BlightOnHit) > DamageType.Generic;
                        if (flag2 && flag2)
                        {
                            builder.Append("    Blight\n");
                            ProcChainMask procChainMask4 = damageInfo.procChainMask;
                            DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Blight, 5f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
                        }

                        // IGNITE ON HIT
                        if ((damageInfo.damageType & DamageType.IgniteOnHit) > DamageType.Generic || attackerBody.HasBuff(RoR2Content.Buffs.AffixRed))
                        {
                            builder.Append("    Ignite\n");
                            float num2 = 0.5f;
                            InflictDotInfo inflictDotInfo = new InflictDotInfo
                            {
                                attackerObject = damageInfo.attacker,
                                victimObject = victim,
                                totalDamage = new float?(damageInfo.damage * num2),
                                damageMultiplier = 1f,
                                dotIndex = DotController.DotIndex.Burn,
                                maxStacksFromAttacker = maxStacksFromAttacker
                            };
                            StrengthenBurnUtils.CheckDotForUpgrade(inventory, ref inflictDotInfo);
                            DotController.InflictDot(ref inflictDotInfo);
                        }

                        // SLOW FROM GLACIAL AND CELESTINE
                        int hasEliteSlow = attackerBody.HasBuff(RoR2Content.Buffs.AffixWhite) ? 1 : 0;
                        hasEliteSlow += (attackerBody.HasBuff(RoR2Content.Buffs.AffixHaunted) ? 2 : 0);
                        if (hasEliteSlow > 0 && victimBody)
                        {
                            builder.Append("    Slowed\n");
                            EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/AffixWhiteImpactEffect"), damageInfo.position, Vector3.up, true);
                            victimBody.AddTimedBuff(RoR2Content.Buffs.Slow80, 1.5f * damageInfo.procCoefficient * (float)hasEliteSlow);
                        }

                        // CHRONOBAUBLE
                        int chronoCount = master.inventory.GetItemCount(RoR2Content.Items.SlowOnHit);
                        if (chronoCount > 0 && victimBody)
                        {
                            builder.Append("    Chrono\n");
                            victimBody.AddTimedBuff(RoR2Content.Buffs.Slow60, 2f * (float)chronoCount);
                        }

                        // HENTAI
                        int tentacleCount = master.inventory.GetItemCount(DLC1Content.Items.SlowOnHitVoid);
                        if (tentacleCount > 0 && victimBody && Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(5f * (float)tentacleCount * damageInfo.procCoefficient), master))
                        {
                            builder.Append("    Tentacle\n");
                            victimBody.AddTimedBuff(RoR2Content.Buffs.Nullified, 1f * (float)tentacleCount);
                        }

                        // MALACHITE
                        if ((attackerBody.HasBuff(RoR2Content.Buffs.AffixPoison) ? 1 : 0) > 0 && victimBody)
                        {
                            builder.Append("    Malachite\n");
                            victimBody.AddTimedBuff(RoR2Content.Buffs.HealingDisabled, 8f * damageInfo.procCoefficient);
                        }

                        // CROWN
                        int crownCount = inventory.GetItemCount(RoR2Content.Items.GoldOnHit);
                        if (crownCount > 0 && Util.CheckRoll(30f * damageInfo.procCoefficient, master))
                        {
                            builder.Append("    Gold\n");
                            GoldOrb goldOrb = new GoldOrb();
                            goldOrb.origin = damageInfo.position;
                            goldOrb.target = attackerBody.mainHurtBox;
                            goldOrb.goldAmount = (uint)((float)crownCount * 2f * Run.instance.difficultyCoefficient);
                            OrbManager.instance.AddOrb(goldOrb);
                            EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CoinImpact"), damageInfo.position, Vector3.up, true);
                        }

                        // MISSILES (this ones long)
                        if (!damageInfo.procChainMask.HasProc(ProcType.Missile))
                        {
                            int missileCount = inventory.GetItemCount(RoR2Content.Items.Missile);
                            if (missileCount > 0)
                            {
                                if (Util.CheckRoll(10f * damageInfo.procCoefficient, master))
                                {
                                    builder.Append("    ATG\n");
                                    float damageCoefficient = 3f * (float)missileCount;
                                    float missileDamage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient);
                                    MissileUtils.FireMissile(attackerBody.corePosition, attackerBody, damageInfo.procChainMask, victim, missileDamage, damageInfo.crit, GlobalEventManager.CommonAssets.missilePrefab, DamageColorIndex.Item, true);
                                }
                            }
                            else
                            {
                                missileCount = inventory.GetItemCount(DLC1Content.Items.MissileVoid);
                                if (missileCount > 0 && attackerBody.healthComponent.shield > 0f)
                                {
                                    builder.Append("    Shrimp\n");
                                    int? moreMissileCount;
                                    if (attackerBody == null)
                                    {
                                        moreMissileCount = null;
                                    }
                                    else
                                    {
                                        Inventory inventory2 = attackerBody.inventory;
                                        moreMissileCount = ((inventory2 != null) ? new int?(inventory2.GetItemCount(DLC1Content.Items.MoreMissile)) : null);
                                    }
                                    int num5 = moreMissileCount ?? 0;
                                    float num6 = Mathf.Max(1f, 1f + 0.5f * (float)(num5 - 1));
                                    float damageCoefficient2 = 0.4f * (float)missileCount;
                                    float damageValue = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient2) * num6;
                                    int num7 = (num5 > 0) ? 3 : 1;
                                    for (int i = 0; i < num7; i++)
                                    {
                                        MissileVoidOrb missileVoidOrb = new MissileVoidOrb();
                                        missileVoidOrb.origin = aimOrigin;
                                        missileVoidOrb.damageValue = damageValue;
                                        missileVoidOrb.isCrit = damageInfo.crit;
                                        missileVoidOrb.teamIndex = attackerTeamIndex;
                                        missileVoidOrb.attacker = damageInfo.attacker;
                                        missileVoidOrb.procChainMask = damageInfo.procChainMask;
                                        missileVoidOrb.procChainMask.AddProc(ProcType.Missile);
                                        missileVoidOrb.procCoefficient = 0.2f;
                                        missileVoidOrb.damageColorIndex = DamageColorIndex.Void;
                                        HurtBox mainHurtBox = victimBody.mainHurtBox;
                                        if (mainHurtBox)
                                        {
                                            missileVoidOrb.target = mainHurtBox;
                                            OrbManager.instance.AddOrb(missileVoidOrb);
                                        }
                                    }
                                }
                            }
                        }

                        // Somthing about loader pylon idk
                        if (attackerBody.HasBuff(JunkContent.Buffs.LoaderPylonPowered) && !damageInfo.procChainMask.HasProc(ProcType.LoaderLightning))
                        {
                            builder.Append("    Pylon\n");
                            float damageCoefficient3 = 0.3f;
                            float damageValue2 = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient3);
                            LightningOrb lightningOrb = new LightningOrb();
                            lightningOrb.origin = damageInfo.position;
                            lightningOrb.damageValue = damageValue2;
                            lightningOrb.isCrit = damageInfo.crit;
                            lightningOrb.bouncesRemaining = 3;
                            lightningOrb.teamIndex = attackerTeamIndex;
                            lightningOrb.attacker = damageInfo.attacker;
                            lightningOrb.bouncedObjects = new List<HealthComponent>
                            {
                                victim.GetComponent<HealthComponent>()
                            };
                            lightningOrb.procChainMask = damageInfo.procChainMask;
                            lightningOrb.procChainMask.AddProc(ProcType.LoaderLightning);
                            lightningOrb.procCoefficient = 0f;
                            lightningOrb.lightningType = LightningOrb.LightningType.Loader;
                            lightningOrb.damageColorIndex = DamageColorIndex.Item;
                            lightningOrb.range = 20f;
                            HurtBox hurtBox = lightningOrb.PickNextTarget(damageInfo.position);
                            if (hurtBox)
                            {
                                lightningOrb.target = hurtBox;
                                OrbManager.instance.AddOrb(lightningOrb);
                            }
                        }

                        // UKULELE
                        int ukuleleCount = inventory.GetItemCount(RoR2Content.Items.ChainLightning);
                        float ukuleleChance = 25f;
                        if (ukuleleCount > 0 && !damageInfo.procChainMask.HasProc(ProcType.ChainLightning) && Util.CheckRoll(ukuleleChance * damageInfo.procCoefficient, master))
                        {
                            builder.Append(" Ukulele\n");
                            float damageCoefficient4 = 0.8f;
                            float damageValue3 = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient4);
                            LightningOrb lightningOrb2 = new LightningOrb();
                            lightningOrb2.origin = damageInfo.position;
                            lightningOrb2.damageValue = damageValue3;
                            lightningOrb2.isCrit = damageInfo.crit;
                            lightningOrb2.bouncesRemaining = 2 * ukuleleCount;
                            lightningOrb2.teamIndex = attackerTeamIndex;
                            lightningOrb2.attacker = damageInfo.attacker;
                            lightningOrb2.bouncedObjects = new List<HealthComponent>
                            {
                                victim.GetComponent<HealthComponent>()
                            };
                            lightningOrb2.procChainMask = damageInfo.procChainMask;
                            lightningOrb2.procChainMask.AddProc(ProcType.ChainLightning);
                            lightningOrb2.procCoefficient = 0.2f;
                            lightningOrb2.lightningType = LightningOrb.LightningType.Ukulele;
                            lightningOrb2.damageColorIndex = DamageColorIndex.Item;
                            lightningOrb2.range += (float)(2 * ukuleleCount);
                            HurtBox hurtBox2 = lightningOrb2.PickNextTarget(damageInfo.position);
                            if (hurtBox2)
                            {
                                lightningOrb2.target = hurtBox2;
                                OrbManager.instance.AddOrb(lightningOrb2);
                            }
                        }

                        // POLYLUTE
                        int luteCount = inventory.GetItemCount(DLC1Content.Items.ChainLightningVoid);
                        float luteChance = 25f;
                        if (luteCount > 0 && !damageInfo.procChainMask.HasProc(ProcType.ChainLightning) && Util.CheckRoll(luteChance * damageInfo.procCoefficient, master))
                        {
                            builder.Append("    Polylute\n");
                            float damageCoefficient5 = 0.6f;
                            float damageValue4 = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient5);
                            VoidLightningOrb voidLightningOrb = new VoidLightningOrb();
                            voidLightningOrb.origin = damageInfo.position;
                            voidLightningOrb.damageValue = damageValue4;
                            voidLightningOrb.isCrit = damageInfo.crit;
                            voidLightningOrb.totalStrikes = 3 * luteCount;
                            voidLightningOrb.teamIndex = attackerTeamIndex;
                            voidLightningOrb.attacker = damageInfo.attacker;
                            voidLightningOrb.procChainMask = damageInfo.procChainMask;
                            voidLightningOrb.procChainMask.AddProc(ProcType.ChainLightning);
                            voidLightningOrb.procCoefficient = 0.2f;
                            voidLightningOrb.damageColorIndex = DamageColorIndex.Void;
                            voidLightningOrb.secondsPerStrike = 0.1f;
                            HurtBox mainHurtBox2 = victimBody.mainHurtBox;
                            if (mainHurtBox2)
                            {
                                voidLightningOrb.target = mainHurtBox2;
                                OrbManager.instance.AddOrb(voidLightningOrb);
                            }
                        }

                        // MEAT HOOK
                        int meatHookCount = inventory.GetItemCount(RoR2Content.Items.BounceNearby);
                        if (meatHookCount > 0)
                        {
                            float meatHookChance = (1f - 100f / (100f + 20f * (float)meatHookCount)) * 100f;
                            if (!damageInfo.procChainMask.HasProc(ProcType.BounceNearby) && Util.CheckRoll(meatHookChance * damageInfo.procCoefficient, master))
                            {
                                builder.Append("    Meat Hook\n");
                                List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                                int maxTargets = 5 + meatHookCount * 5;
                                BullseyeSearch search = new BullseyeSearch();
                                List<HealthComponent> list2 = CollectionPool<HealthComponent, List<HealthComponent>>.RentCollection();
                                if (attackerBody && attackerBody.healthComponent)
                                {
                                    list2.Add(attackerBody.healthComponent);
                                }
                                if (victimBody && victimBody.healthComponent)
                                {
                                    list2.Add(victimBody.healthComponent);
                                }
                                BounceOrb.SearchForTargets(search, attackerTeamIndex, damageInfo.position, 30f, maxTargets, list, list2);
                                CollectionPool<HealthComponent, List<HealthComponent>>.ReturnCollection(list2);
                                List<HealthComponent> bouncedObjects = new List<HealthComponent>
                                {
                                    victim.GetComponent<HealthComponent>()
                                };
                                float damageCoefficient6 = 1f;
                                float damageValue5 = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient6);
                                int j = 0;
                                int count = list.Count;
                                while (j < count)
                                {
                                    HurtBox hurtBox3 = list[j];
                                    if (hurtBox3)
                                    {
                                        BounceOrb bounceOrb = new BounceOrb();
                                        bounceOrb.origin = damageInfo.position;
                                        bounceOrb.damageValue = damageValue5;
                                        bounceOrb.isCrit = damageInfo.crit;
                                        bounceOrb.teamIndex = attackerTeamIndex;
                                        bounceOrb.attacker = damageInfo.attacker;
                                        bounceOrb.procChainMask = damageInfo.procChainMask;
                                        bounceOrb.procChainMask.AddProc(ProcType.BounceNearby);
                                        bounceOrb.procCoefficient = 0.33f;
                                        bounceOrb.damageColorIndex = DamageColorIndex.Item;
                                        bounceOrb.bouncedObjects = bouncedObjects;
                                        bounceOrb.target = hurtBox3;
                                        OrbManager.instance.AddOrb(bounceOrb);
                                    }
                                    j++;
                                }
                                CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
                            }
                        }

                        // STICKY BOMB
                        int stickyCount = inventory.GetItemCount(RoR2Content.Items.StickyBomb);
                        if (stickyCount > 0 && Util.CheckRoll(5f * (float)stickyCount * damageInfo.procCoefficient, master) && victimBody)
                        {
                            builder.Append("    Sticky Bomb\n");
                            bool alive = victimBody.healthComponent.alive;
                            float num11 = 5f;
                            Vector3 position = damageInfo.position;
                            Vector3 forward = victimBody.corePosition - position;
                            float magnitude = forward.magnitude;
                            Quaternion rotation = (magnitude != 0f) ? Util.QuaternionSafeLookRotation(forward) : UnityEngine.Random.rotationUniform;
                            float damageCoefficient7 = 1.8f;
                            float damage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient7);
                            ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/StickyBomb"), position, rotation, damageInfo.attacker, damage, 100f, damageInfo.crit, DamageColorIndex.Item, null, alive ? (magnitude * num11) : -1f);
                        }

                        // DEATH MARK
                        int deathMarkCount = master.inventory.GetItemCount(RoR2Content.Items.DeathMark);
                        int num14 = 0;
                        if (deathMarkCount >= 1 && !victimBody.HasBuff(RoR2Content.Buffs.DeathMark))
                        {
                            foreach (BuffIndex buffType in BuffCatalog.debuffBuffIndices)
                            {
                                if (victimBody.HasBuff(buffType))
                                {
                                    num14++;
                                }
                            }
                            DotController dotController = DotController.FindDotController(victim.gameObject);
                            if (dotController)
                            {
                                for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
                                {
                                    if (dotController.HasDotActive(dotIndex))
                                    {
                                        num14++;
                                    }
                                }
                            }
                            if (num14 >= 4)
                            {
                                builder.Append("    Death Mark\n");
                                victimBody.AddTimedBuff(RoR2Content.Buffs.DeathMark, 7f * (float)deathMarkCount);
                            }
                        }

                        // SAW BLEED
                        if (damageInfo != null && damageInfo.inflictor != null && damageInfo.inflictor.GetComponent<BoomerangProjectile>() != null && !damageInfo.procChainMask.HasProc(ProcType.BleedOnHit))
                        {
                            int num15 = 0;
                            if (inventory.GetEquipmentIndex() == RoR2Content.Equipment.Saw.equipmentIndex)
                            {
                                num15 = 1;
                            }
                            bool flag3 = (damageInfo.damageType & DamageType.BleedOnHit) > DamageType.Generic;
                            if ((num15 > 0 || flag3) && (flag3 || Util.CheckRoll(100f, master)))
                            {
                                builder.Append("    SAW THING\n");
                                ProcChainMask procChainMask7 = damageInfo.procChainMask;
                                procChainMask7.AddProc(ProcType.BleedOnHit);
                                DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed, 4f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
                            }
                        }

                        // SUPER BLEED
                        if (damageInfo.crit && (damageInfo.damageType & DamageType.SuperBleedOnCrit) != DamageType.Generic)
                        {
                            builder.Append("    Super Bleed\n");
                            DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.SuperBleed, 15f * damageInfo.procCoefficient, 1f, maxStacksFromAttacker);
                        }

                        // LIFE STEAL
                        if (attackerBody.HasBuff(RoR2Content.Buffs.LifeSteal))
                        {
                            builder.Append("    Life Steal\n");
                            float amount = damageInfo.damage * 0.2f;
                            attackerBody.healthComponent.Heal(amount, damageInfo.procChainMask, true);
                        }

                        // PERFORATOR
                        int itemCount14 = inventory.GetItemCount(RoR2Content.Items.FireballsOnHit);
                        if (itemCount14 > 0 && !damageInfo.procChainMask.HasProc(ProcType.Meatball))
                        {
                            InputBankTest component5 = attackerBody.GetComponent<InputBankTest>();
                            Vector3 vector2 = victimBody.characterMotor ? (victim.transform.position + Vector3.up * (victimBody.characterMotor.capsuleHeight * 0.5f + 2f)) : (victim.transform.position + Vector3.up * 2f);
                            Vector3 forward2 = component5 ? component5.aimDirection : victim.transform.forward;
                            forward2 = Vector3.up;
                            float num16 = 20f;
                            if (Util.CheckRoll(10f * damageInfo.procCoefficient, master))
                            {
                                builder.Append("    Perforator\n");
                                EffectData effectData = new EffectData
                                {
                                    scale = 1f,
                                    origin = vector2
                                };
                                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleflashFireMeatBall"), effectData, true);
                                int num17 = 3;
                                float damageCoefficient11 = 3f * (float)itemCount14;
                                float damage5 = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient11);
                                float min = 15f;
                                float max = 30f;
                                ProcChainMask procChainMask8 = damageInfo.procChainMask;
                                procChainMask8.AddProc(ProcType.Meatball);
                                float speedOverride2 = UnityEngine.Random.Range(min, max);
                                float num18 = (float)(360 / num17);
                                float num19 = num18 / 360f;
                                float num20 = 1f;
                                float num21 = num18;
                                for (int l = 0; l < num17; l++)
                                {
                                    float num22 = (float)l * 3.1415927f * 2f / (float)num17;
                                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                                    {
                                        projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/FireMeatBall"),
                                        position = vector2 + new Vector3(num20 * Mathf.Sin(num22), 0f, num20 * Mathf.Cos(num22)),
                                        rotation = Util.QuaternionSafeLookRotation(forward2),
                                        procChainMask = procChainMask8,
                                        target = victim,
                                        owner = attackerBody.gameObject,
                                        damage = damage5,
                                        crit = damageInfo.crit,
                                        force = 200f,
                                        damageColorIndex = DamageColorIndex.Item,
                                        speedOverride = speedOverride2,
                                        useSpeedOverride = true
                                    };
                                    num21 += num18;
                                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                                    forward2.x += Mathf.Sin(num22 + UnityEngine.Random.Range(-num16, num16));
                                    forward2.z += Mathf.Cos(num22 + UnityEngine.Random.Range(-num16, num16));
                                }
                            }
                        }

                        // CHARGED PERFORATOR
                        int itemCount15 = inventory.GetItemCount(RoR2Content.Items.LightningStrikeOnHit);
                        if (itemCount15 > 0 && !damageInfo.procChainMask.HasProc(ProcType.LightningStrikeOnHit) && Util.CheckRoll(10f * damageInfo.procCoefficient, master))
                        {
                            builder.Append("    Charged Perforator\n");
                            float damageValue6 = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, 5f * (float)itemCount15);
                            ProcChainMask procChainMask9 = damageInfo.procChainMask;
                            procChainMask9.AddProc(ProcType.LightningStrikeOnHit);
                            HurtBox target = victimBody.mainHurtBox;
                            if (victimBody.hurtBoxGroup)
                            {
                                target = victimBody.hurtBoxGroup.hurtBoxes[UnityEngine.Random.Range(0, victimBody.hurtBoxGroup.hurtBoxes.Length)];
                            }
                            OrbManager.instance.AddOrb(new SimpleLightningStrikeOrb
                            {
                                attacker = attackerBody.gameObject,
                                damageColorIndex = DamageColorIndex.Item,
                                damageValue = damageValue6,
                                isCrit = Util.CheckRoll(attackerBody.crit, master),
                                procChainMask = procChainMask9,
                                procCoefficient = 1f,
                                target = target
                            });
                        }

                        // LUNAR SECONDARY THING
                        if ((damageInfo.damageType & DamageType.LunarSecondaryRootOnHit) != DamageType.Generic && victimBody)
                        {
                            builder.Append("    Lunar Secondary Thing\n");
                            int itemCount16 = master.inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement);
                            victimBody.AddTimedBuff(RoR2Content.Buffs.LunarSecondaryRoot, 3f * (float)itemCount16);
                        }

                        // maybe something with rex i dont really remember
                        if ((damageInfo.damageType & DamageType.FruitOnHit) != DamageType.Generic && victimBody)
                        {
                            builder.Append("    Fruit\n");
                            victimBody.AddTimedBuff(RoR2Content.Buffs.Fruiting, 10f);
                        }

                        // Apparently this goes here
                        if (inventory.GetItemCount(DLC1Content.Items.DroneWeaponsBoost) > 0)
                        {
                            DroneWeaponsBoostBehavior component6 = attackerBody.GetComponent<DroneWeaponsBoostBehavior>();
                            if (component6)
                            {
                                builder.Append("    Drone Thing\n");
                                component6.OnEnemyHit(damageInfo, victimBody);
                            }
                        }
                    }
                }
            }
        }

        private class DiceStack : MonoBehaviour
        {
            public DamageInfo damageInfo;
            public int currentRoll;
            public int maxRolls;
        }
    }
}