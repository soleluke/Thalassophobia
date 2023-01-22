using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thalassophobia.Items.Lunar;
using Thalassophobia.Utils;
using UnityEngine;
using static On.RoR2.DotController;
using static RoR2.DotController;

namespace Thalassophobia.Items.Void
{
    class ResonatorCoil : ItemBase<ResonatorCoil>
    {
        public override string ItemName => "Resonator Coil";

        public override string ItemLangTokenName => "RESONATOR_COIL";

        public override string ItemPickupDesc => "<style=cIsVoid>Corrupts all Tesla Coils.</style> Strike nearby enemies with lightning every 10 seconds.";

        public override string ItemFullDescription => "Every <style=cIsDamage>10 seconds</style> unleash <style=cIsDamage>3</style> <style=cStack>(+2 per stack)</style> <style=cIsDamage>lightning strikes</style> on nearby enemies for <style=cIsDamage>1500%</style> base damage each. <style=cIsVoid>Corrupts all Tesla Coils.</style>";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.VoidTier3;

        public override String CorruptsItem => RoR2Content.Items.ShockNearby.nameToken;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("ResonatorModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("ResonatorIcon.png");

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
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnInventoryChanged += (orig, self) =>
            {
                orig(self);
                if (self && self.GetBody())
                {
                    self.GetBody().AddItemBehavior<ResonatorBehavior>(GetCount(self.GetBody()));
                }
            };
        }

        public class ResonatorBehavior : CharacterBody.ItemBehavior
        {
            private float activationTimer;
            private float delayTimer;
            private float baseDelay;
            private float delay;
            private BullseyeSearch search;
            private bool canStrike;
            private int remainingStrikes;
            private List<HealthComponent> struckObjects;

            private void Start()
            {
                activationTimer = 10.0f;
                baseDelay = 1.0f / 3.0f;
                delayTimer = baseDelay;
                delay = baseDelay;
                canStrike = false;
                struckObjects = new List<HealthComponent>();
            }

            private void OnDisable()
            {
            }

            private void FixedUpdate()
            {
                if (canStrike)
                {
                    delayTimer -= Time.fixedDeltaTime;
                    if (remainingStrikes > 0 && delayTimer <= 0)
                    {
                        if (Plugin.DEBUG)
                        {
                            Log.LogInfo("Trying strike: " + remainingStrikes + " remaining");
                        }
                        strike();
                        delayTimer = delay;
                        remainingStrikes--;
                        if (remainingStrikes <= 0) {
                            canStrike = false;
                            struckObjects.Clear();
                        }
                    }
                }
                else
                {
                    activationTimer -= Time.fixedDeltaTime;
                    if (this.activationTimer <= 0)
                    {
                        if (Plugin.DEBUG)
                        {
                            Log.LogInfo("Starting Strikes");
                        }
                        activationTimer = 10.0f;
                        canStrike = true;
                        remainingStrikes = 3 + ((this.stack-1) * 2);
                        delay = baseDelay / this.stack;
                        delayTimer = 0;
                    }
                }
            }

            public HurtBox PickNextTarget(Vector3 position)
            {
                if (search == null)
                {
                    search = new BullseyeSearch();
                }
                search.searchOrigin = position;
                search.searchDirection = Vector3.zero;
                search.teamMaskFilter = TeamMask.allButNeutral;
                search.teamMaskFilter.RemoveTeam(this.body.master.teamIndex);
                this.search.filterByLoS = false;
                this.search.sortMode = BullseyeSearch.SortMode.Distance;
                this.search.maxDistanceFilter = 45;
                this.search.RefreshCandidates();
                HurtBox hurtBox = (from v in search.GetResults()
                                   where !struckObjects.Contains(v.healthComponent)
                                   select v).FirstOrDefault<HurtBox>();
                return hurtBox;
            }

            private void strike()
            {
                LightningStrikeOrb orb = new LightningStrikeOrb
                {
                    damageValue = this.body.damage * 1500f,
                    teamIndex = this.body.teamComponent.teamIndex,
                    attacker = this.body.gameObject,
                    procCoefficient = 0.3f,
                    damageColorIndex = DamageColorIndex.Item,
                    isCrit = this.body.RollCrit(),
                };
                HurtBox hurtBox = PickNextTarget(this.body.transform.position);
                if (hurtBox)
                {
                    orb.target = hurtBox;
                    OrbManager.instance.AddOrb(orb);
                }
            }
        }
    }
}
