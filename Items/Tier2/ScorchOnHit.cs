using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using Thalassophobia.Items.Lunar;
using UnityEngine;
using UnityEngine.Networking;
using static On.RoR2.GlobalEventManager;
using static R2API.DamageAPI;
using static RoR2.DotController;

namespace Thalassophobia.Items.Tier2
{
    public class ScorchOnHit : ItemBase<ScorchOnHit>
    {
        public override string ItemName => "Broken Candle";

        public override string ItemLangTokenName => "FIRE_ON_HIT";

        public override string ItemPickupDesc => "Chance to scorch enemies on hit.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float chance;
        private float duration;
        private float durationScale;
        private float radius;
        private float radiusScale;
        private float damage;
        private float bonusDamage;

        // BuffDef
        BuffDef scorched;

        // Damage Type
        ModdedDamageType scorchedDamage;

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

            chance = config.Bind<float>("Item: " + ItemName, "Chance", 10.0f, "").Value;
            duration = config.Bind<float>("Item: " + ItemName, "Duration", 2.5f, "").Value;
            durationScale = config.Bind<float>("Item: " + ItemName, "Duration Scale", 1.0f, "").Value;
            radius = config.Bind<float>("Item: " + ItemName, "Radius", 8.0f, "").Value;
            radiusScale = config.Bind<float>("Item: " + ItemName, "Radius Scale", 1.5f, "").Value;
            damage = config.Bind<float>("Item: " + ItemName, "Damage", 0.30f, "").Value;
            bonusDamage = config.Bind<float>("Item: " + ItemName, "Damage Bonus", 1.30f, "").Value;

            scorched = ScriptableObject.CreateInstance<BuffDef>();
            scorched.name = "Scorched";
            scorched.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffDeathMarkIcon");
            scorched.buffColor = Color.black;
            scorched.isDebuff = true;
            scorched.canStack = false;
            ContentAddition.AddBuffDef(scorched);

            scorchedDamage = ReserveDamageType();
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

            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
            {
                if (damageInfo.attacker)
                {
                    if (self.body.HasBuff(scorched))
                    {
                        damageInfo.damage *= bonusDamage;
                    }
                }
                orig(self, damageInfo);
            };
        }
        public void OnHitEffect(global::RoR2.DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker)
            {
                var scorchCount = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                if (scorchCount > 0 && damageInfo.procCoefficient > 0)
                {
                    if (Util.CheckRoll(chance, damageInfo.attacker.GetComponent<CharacterMaster>()) && !damageInfo.rejected)
                    {
                        if (Plugin.DEBUG)
                        {
                            Log.LogInfo("Proc Candle");
                        }

                        Vector3 position = victim.transform.position;
                        float baseDamage = (damage * damageInfo.procCoefficient) + 5;
                        GameObject explode = UnityEngine.Object.Instantiate<GameObject>(GlobalEventManager.CommonAssets.explodeOnDeathPrefab, position, Quaternion.identity);
                        DelayBlast blast = explode.GetComponent<DelayBlast>();
                        if (blast)
                        {
                            blast.position = position;
                            blast.baseDamage = baseDamage;
                            blast.baseForce = 500f;
                            blast.bonusForce = Vector3.up * 250f;
                            blast.radius = radius + (radiusScale * (scorchCount - 1));
                            blast.attacker = damageInfo.attacker;
                            blast.inflictor = null;
                            blast.crit = Util.CheckRoll(damageInfo.attacker.GetComponent<CharacterBody>().crit, damageInfo.attacker.GetComponent<CharacterBody>().master);
                            blast.maxTimer = 0.0f;
                            blast.damageColorIndex = DamageColorIndex.Item;
                            blast.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                            blast.procCoefficient = 0;
                            var damageTypeComponent = blast.gameObject.AddComponent<ModdedDamageTypeHolderComponent>();
                            damageTypeComponent.Add(scorchedDamage);
                        }
                        TeamFilter component7 = explode.GetComponent<TeamFilter>();
                        if (component7)
                        {
                            component7.teamIndex = damageInfo.attacker.GetComponent<CharacterBody>().teamComponent.teamIndex;
                        }
                        NetworkServer.Spawn(explode);
                    }
                }

                if (DamageAPI.HasModdedDamageType(damageInfo, scorchedDamage))
                {
                    victim.GetComponent<CharacterBody>().AddTimedBuff(scorched, duration + (durationScale * (scorchCount - 1)));
                }
            }
        }
    }
}