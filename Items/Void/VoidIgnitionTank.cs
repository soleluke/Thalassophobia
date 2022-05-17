using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using Thalassophobia.Items.Lunar;
using Thalassophobia.Utils;
using UnityEngine;
using static R2API.DamageAPI;
using static RoR2.DotController;

namespace Thalassophobia.Items.Void
{
    class VoidIgnitionTank : ItemBase<VoidIgnitionTank>
    {
        public override string ItemName => "Infected Aerosol";

        public override string ItemLangTokenName => "VOID_IGNITION_TANK";

        public override string ItemPickupDesc => "Corrupts all Ignition Tanks.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override String CorruptsItem => DLC1Content.Items.StrengthenBurn.nameToken;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Buff
        public BuffDef voidFog;

        // Damage Type
        ModdedDamageType inflictVoid;

        // Custom DoT
        public DotIndex voidDotIndex;

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

            inflictVoid = ReserveDamageType();

            voidFog = ScriptableObject.CreateInstance<BuffDef>();
            voidFog.name = "Suffocating";
            voidFog.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            voidFog.canStack = true;
            voidFog.isDebuff = true;
            ContentAddition.AddBuffDef(voidFog);

            DotDef voidDot = new DotDef();
            voidDot.damageColorIndex = DamageColorIndex.WeakPoint;
            voidDot.associatedBuff = voidFog;
            voidDot.damageCoefficient = 0;
            voidDot.interval = 2f;
            voidDotIndex = DotAPI.RegisterDotDef(voidDot, (self, dotStack) => { });
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.StrengthenBurnUtils.CheckDotForUpgrade += StrengthenBurnUtils_CheckDotForUpgrade;

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
            };
        }

        private void StrengthenBurnUtils_CheckDotForUpgrade(On.RoR2.StrengthenBurnUtils.orig_CheckDotForUpgrade orig, Inventory inventory, ref InflictDotInfo dotInfo)
        {
            orig(inventory, ref dotInfo);
            if (dotInfo.dotIndex == DotController.DotIndex.Burn || dotInfo.dotIndex == DotController.DotIndex.Helfire)
            {
                int itemCount = GetCount(inventory.gameObject.GetComponent<CharacterBody>());
                if (itemCount > 0)
                {
                    dotInfo.dotIndex = voidDotIndex;
                }
            }
        }
    }
}
