using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Thalassophobia.EliteEquipments
{
    class AffixWithering : EliteEquipmentBase<AffixWithering>
    {
        public override string EliteEquipmentName => "Only Dust May Remain";

        public override string EliteAffixToken => "AFFIX_WITHERING";

        public override string EliteEquipmentPickupDesc => "Become an aspect of decay.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Zircon";

        public override GameObject EliteEquipmentModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite EliteEquipmentIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public override Sprite EliteBuffIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public override float HealthMultiplier => 18f;

        public override float DamageMultiplier => 6f;

        public override float CostMultiplierOfElite => 2f;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
        }

        private void CreateEliteTiers()
        {
            CanAppearInEliteTiers = new CombatDirector.EliteTierDef[]
            {
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier * CostMultiplierOfElite,
                    eliteTypes = Array.Empty<EliteDef>(),
                    isAvailable = SetAvailability
                }
            };
        }

        private bool SetAvailability(SpawnCard.EliteRules arg)
        {
            return Run.instance.loopClearCount > 0 && arg == SpawnCard.EliteRules.Default;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);
                if (self.HasBuff(EliteBuffDef))
                {
                    if (!NetworkServer.active)
                    {
                        return;
                    }
                    bool flag = false;
                    if (self.inventory)
                    {
                        flag = self.HasBuff(EliteBuffDef);
                    }
                    /*
                    if (self.GetComponent<Utils.DecayManager>() != flag)
                    {
                        if (flag)
                        {
                            self.gameObject.AddComponent<Utils.DecayManager>();
                            return;
                        }
                        UnityEngine.Object.Destroy(self.GetComponent<Utils.DecayManager>().gameObject);
                    }
                                     */
                }
            };
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }
}
