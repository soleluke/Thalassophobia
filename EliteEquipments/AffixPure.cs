using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Thalassophobia.EliteEquipments
{
    class AffixPure : EliteEquipmentBase<AffixPure>
    {
        public override string EliteEquipmentName => "Purifying Light";

        public override string EliteAffixToken => "AFFIX_PURE";

        public override string EliteEquipmentPickupDesc => "Become an aspect of purity.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Nullifying";

        public override GameObject EliteEquipmentModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite EliteEquipmentIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public override Sprite EliteBuffIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public override Color32 EliteColor => new Color32(50, 50, 50, 255);

        public override int EliteRampIndex => 2;

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
        }

        private bool SetAvailability(SpawnCard.EliteRules arg)
        {
            return arg == SpawnCard.EliteRules.Default;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            // Removes debuffs from self
            On.RoR2.CharacterBody.UpdateBuffs += (orig, self, deltaTime) =>
            {
                if (self.HasBuff(EliteBuffDef))
                {
                    Util.CleanseBody(self, true, false, false, true, true, false);
                }
                orig(self, deltaTime);
            };

            // Removes buffs from hit enemies
            On.RoR2.GlobalEventManager.OnHitAll += (orig, self, DamageInfo, hitObject) =>
            {
                if (DamageInfo.attacker && hitObject.GetComponent<CharacterBody>())
                {
                    if (DamageInfo.attacker.GetComponent<CharacterBody>().HasBuff(EliteBuffDef))
                    {
                        Util.CleanseBody(hitObject.GetComponent<CharacterBody>(), false, true, false, false, false, false);
                    }
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
