using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Utils;
using RoR2.Projectile;
using static On.RoR2.GlobalEventManager;
using Thalassophobia.Items.Lunar;

namespace Thalassophobia.Items.Tier2
{
    public class Beans : ItemBase<Beans>
    {
        public override string ItemName => "Can of Beans";

        public override string ItemLangTokenName => "BEANS";

        public override string ItemPickupDesc => "You've spilt beans.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "";
        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item Stats
        private float procChance;
        private float maxBeans;
        private float minBeans;
        private float radius;
        private float radiusIncrease;
        private float damageCoefficient;
        private float procCoefficient;


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

            procChance = config.Bind<float>("Item: " + ItemName, "ProcChance", 10f, "Chance to proc the item on hit.").Value;
            maxBeans = config.Bind<float>("Item: " + ItemName, "MaxBeans", 4f, "Maximum number of beans launched.").Value;
            minBeans = config.Bind<float>("Item: " + ItemName, "MinBeans", 2f, "Minimum number of beans launched.").Value;
            radius = config.Bind<float>("Item: " + ItemName, "ExplosionRadius", 7.5f, "The radius of the explosion.").Value;
            radiusIncrease = config.Bind<float>("Item: " + ItemName, "ExplosionRadiusIncrease", 2.5f, "How much the explosion radius is increased per stack.").Value;
            damageCoefficient = config.Bind<float>("Item: " + ItemName, "DamageCoefficient", 0.1f, "Percent damage that the explosion deals where 1.0 is 100%.").Value;
            procCoefficient = config.Bind<float>("Item: " + ItemName, "ProcCoefficent", 0.4f, "Proc coefficient of the explosion.").Value;

            /*
            GameObject projectile = AssetManager.GetProjectile(AssetManager.ProjectileIndex.Bean);
            projectile.GetComponent<ProjectileImpactExplosion>().blastProcCoefficient = procCoefficient;
            projectile.GetComponent<ProjectileImpactExplosion>().explosionEffect = AssetManager.GetEffect(AssetManager.EffectPrefabIndex.BeanPop);
            ProjectileAPI.Add(projectile);
            */
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);
                OnHitEffect(damageInfo, victim);
            };
            LunarDice.hook_DiceReroll += OnHitEffect;
        }

        public void OnHitEffect(global::RoR2.DamageInfo damageInfo, GameObject victim)
        {
            /*
if (damageInfo.attacker)
{
    var beanCount = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
    if (beanCount > 0 && !damageInfo.procChainMask.HasProc((ProcType)(ProcType.Count + 1)))
    {
        if (Util.CheckRoll(procChance * damageInfo.procCoefficient, damageInfo.attacker.GetComponent<CharacterMaster>()) && !damageInfo.rejected)
        {
            GameObject projectilePrefab = AssetManager.GetProjectile(AssetManager.ProjectileIndex.Bean);
            ProcChainMask procMask = damageInfo.procChainMask;
            procMask.AddProc((ProcType)(ProcType.Count + 1));
            projectilePrefab.GetComponent<ProjectileImpactExplosion>().blastRadius = radius + (radiusIncrease * (beanCount - 1));
            projectilePrefab.GetComponent<ProjectileImpactExplosion>().explosionEffect = AssetManager.GetEffect(AssetManager.EffectPrefabIndex.BeanPop);
            FireProjectileInfo info = new FireProjectileInfo();
            info.procChainMask = procMask;
            info.projectilePrefab = projectilePrefab;
            info.position = damageInfo.position;
            info.owner = damageInfo.attacker;
            info.damageTypeOverride = DamageType.Generic;
            info.damage = damageInfo.attacker.GetComponent<CharacterBody>().damage * damageCoefficient;
            info.crit = false;
            int num = Mathf.RoundToInt(Random.Range(minBeans, maxBeans));
            for (; num > 0; num--)
            {
                info.rotation = GetRandomLaunch();
                ProjectileManager.instance.FireProjectile(info);
            }
        }
    }
}
*/
        }


        public Quaternion GetRandomLaunch()
        {
            Quaternion quat;
            quat = Quaternion.Euler(-75.0f, Random.Range(0.0f, 360.0f), 0.0f);
            return quat;
        }
    }
}