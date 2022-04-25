using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
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
            duration = config.Bind<float>("Item: " + ItemName, "Duration", 3.0f, "").Value;
            durationScale = config.Bind<float>("Item: " + ItemName, "Duration Scale", 1.0f, "").Value;
            radius = config.Bind<float>("Item: " + ItemName, "Radius", 6.0f, "").Value;
            radiusScale = config.Bind<float>("Item: " + ItemName, "Radius Scale", 2.0f, "").Value;
            damage = config.Bind<float>("Item: " + ItemName, "Damage", 0.30f, "").Value;
            bonusDamage = config.Bind<float>("Item: " + ItemName, "Damage Bonus", 1.30f, "").Value;

            scorched = ScriptableObject.CreateInstance<BuffDef>();
            scorched.name = "Scorched";
            scorched.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffDeathMarkIcon");
            scorched.buffColor = Color.black;
            scorched.isDebuff = true;
            scorched.canStack = false;
            ContentAddition.AddBuffDef(scorched);
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
                if (damageInfo.attacker)
                {
                    var scorchCount = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                    if (scorchCount > 0)
                    {
                        if (Util.CheckRoll(chance, damageInfo.attacker.GetComponent<CharacterMaster>()) && !damageInfo.rejected)
                        {
                            if (Plugin.DEBUG)
                            {
                                Log.LogInfo("Proc Candle");
                            }
                            victim.GetComponent<CharacterBody>().AddTimedBuff(scorched, duration + (durationScale * scorchCount));

                            Vector3 position = victim.transform.position;
                            float baseDamage = damage * damageInfo.procCoefficient;
                            GameObject explode = UnityEngine.Object.Instantiate<GameObject>(GlobalEventManager.CommonAssets.explodeOnDeathPrefab, position, Quaternion.identity);
                            DelayBlast component6 = explode.GetComponent<DelayBlast>();
                            if (component6)
                            {
                                component6.position = position;
                                component6.baseDamage = baseDamage;
                                component6.baseForce = 2000f;
                                component6.bonusForce = Vector3.up * 1000f;
                                component6.radius = radius + radiusScale * scorchCount;
                                component6.attacker = damageInfo.attacker;
                                component6.inflictor = null;
                                component6.crit = Util.CheckRoll(damageInfo.attacker.GetComponent<CharacterBody>().crit, damageInfo.attacker.GetComponent<CharacterBody>().master);
                                component6.maxTimer = 0.2f;
                                component6.damageColorIndex = DamageColorIndex.Item;
                                component6.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                            }
                            TeamFilter component7 = explode.GetComponent<TeamFilter>();
                            if (component7)
                            {
                                component7.teamIndex = damageInfo.attacker.GetComponent<CharacterBody>().teamComponent.teamIndex;
                            }
                            NetworkServer.Spawn(explode);
                        }
                    }
                }
            };

            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
            {
                orig(self, damageInfo);
                if (damageInfo.attacker)
                {
                    if (self.body.HasBuff(scorched))
                    {
                        damageInfo.damage *= bonusDamage;
                    }
                }
            };
        }
    }
}