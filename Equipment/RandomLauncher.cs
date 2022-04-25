using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Equipment
{
    public class RandomLauncher : EquipmentBase
    {
        public override string EquipmentName => "Unstable Machine Parts";

        public override string EquipmentLangTokenName => "RANDOM_LAUNCHER";

        public override string EquipmentPickupDesc => "launches various machine parts at enemies.";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => "";

        public override GameObject EquipmentModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite EquipmentIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        private float duration;
        private float cooldown;
        private float damage;
        private float range;
        private float fireRate;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            Hooks();
        }

        protected override void CreateConfig(ConfigFile config)
        {
            duration = config.Bind<float>("Equipment: " + EquipmentName, "Duration", 10f, "How long the effect lasts.").Value;
            cooldown = config.Bind<float>("Equipment: " + EquipmentName, "Cooldown", 50f, "The recharge time for the equipment.").Value;
            damage = config.Bind<float>("Equipment: " + EquipmentName, "Damage", 1f, "Damage of the projectiles.").Value;
            range = config.Bind<float>("Equipment: " + EquipmentName, "Range", 20f, "How far the projectiles reach.").Value;
            fireRate = config.Bind<float>("Equipment: " + EquipmentName, "FireRate", 0.5f, "How fast the equipment shoots.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }
}
