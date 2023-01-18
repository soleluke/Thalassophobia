using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier2
{
    public class Snowglobe : ItemBase<Snowglobe>
    {
        public override string ItemName => "Snowglobe";

        public override string ItemLangTokenName => "SNOWGLOBE";

        public override string ItemPickupDesc => "Slowing an enemy decreases their attack speed.";

        public override string ItemFullDescription => "Slowing effects also decrease attack speed by <style=cIsUtility>50%</style> of the strength of the slowing effect. Increases length of these effects by <style=cIsUtility>25%</style> <style=cStack>(+10% per stack)</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("SnowglobeModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("SnowglobeIcon.png");

        BuffDef betterSlow50;
        BuffDef betterSlow60;
        BuffDef betterSlow80;
        BuffDef betterWeak;
        BuffDef betterTar;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Utility, ItemTag.Healing };

            betterSlow50 = ScriptableObject.CreateInstance<BuffDef>();
            betterSlow50.name = "50% Slow";
            betterSlow50.iconSprite = Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/betterSlow50Icon.png");
            betterSlow50.canStack = false;
            betterSlow50.isDebuff = true;
            betterSlow50.buffColor = Color.red;
            ContentAddition.AddBuffDef(betterSlow50);

            betterSlow60 = ScriptableObject.CreateInstance<BuffDef>();
            betterSlow60.name = "60% Slow";
            betterSlow60.iconSprite = Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/betterSlow60Icon.png");
            betterSlow60.canStack = false;
            betterSlow60.isDebuff = true;
            //betterSlow60.buffColor = Color.gray;
            ContentAddition.AddBuffDef(betterSlow60);

            betterSlow80 = ScriptableObject.CreateInstance<BuffDef>();
            betterSlow80.name = "80% Slow";
            betterSlow80.iconSprite = Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/betterSlow80Icon.png");
            betterSlow80.canStack = false;
            betterSlow80.isDebuff = true;
            betterSlow80.buffColor = Color.blue;
            ContentAddition.AddBuffDef(betterSlow80);

            betterWeak = ScriptableObject.CreateInstance<BuffDef>();
            betterWeak.name = "Weak";
            betterWeak.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            betterWeak.canStack = false;
            betterWeak.isDebuff = true;
            betterWeak.buffColor = Color.green;
            ContentAddition.AddBuffDef(betterWeak);

            betterTar = ScriptableObject.CreateInstance<BuffDef>();
            betterTar.name = "Tar";
            betterTar.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            betterTar.canStack = false;
            betterTar.isDebuff = true;
            betterTar.buffColor = Color.black;
            ContentAddition.AddBuffDef(betterTar);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;

            On.RoR2.CharacterBody.FixedUpdate += OverlayManager;
        }

        private void OverlayManager(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            /*
            if (self && self.modelLocator && self.modelLocator.modelTransform)
            {
                bool add = true;
                foreach (Material mat in self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>().currentOverlays) 
                {
                    if (mat == Plugin.assetBundle.LoadAsset<Material>("BetterSlowOverlay.mat")) {
                        add = false;
                    } 
                }
                if (add && self.HasBuff(betterSlow50) || self.HasBuff(betterSlow60) || self.HasBuff(betterSlow80))
                {
                    //var Meshes = Voidheart.ItemBodyModelPrefab.GetComponentsInChildren<MeshRenderer>();
                    float time = 0.1f;
                    RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                    overlay.duration = time;
                    overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    overlay.animateShaderAlpha = true;
                    overlay.destroyComponentOnEnd = true;
                    overlay.originalMaterial = Plugin.assetBundle.LoadAsset<Material>("BetterSlowOverlay.mat");
                    overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                }
            }
            */
            orig(self);
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>())
            {
                CharacterBody attacker = damageInfo.attacker.GetComponent<CharacterBody>();
                if (GetCount(attacker) > 0)
                {
                    //Log.LogInfo("test");
                    CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                    if (victimBody.HasBuff(BuffCatalog.FindBuffIndex("bdSlow50")))
                    {
                        //Log.LogInfo("test50");
                        float remainingTime = 0f;
                        foreach (CharacterBody.TimedBuff t in victimBody.timedBuffs)
                        {
                            if (t.buffIndex == BuffCatalog.FindBuffIndex("bdSlow50"))
                            {
                                remainingTime = t.timer;
                            }
                        }
                        victimBody.AddTimedBuff(betterSlow50, remainingTime * (1.25f + (0.1f * GetCount(attacker))));
                        victimBody.ClearTimedBuffs(BuffCatalog.FindBuffIndex("bdSlow50"));
                    }

                    if (victimBody.HasBuff(BuffCatalog.FindBuffIndex("bdSlow60")))
                    {
                        //Log.LogInfo("test60");
                        float remainingTime = 0f;
                        foreach (CharacterBody.TimedBuff t in victimBody.timedBuffs)
                        {
                            if (t.buffIndex == BuffCatalog.FindBuffIndex("bdSlow60"))
                            {
                                remainingTime = t.timer;
                            }
                        }
                        victimBody.AddTimedBuff(betterSlow60, remainingTime * (1.25f + (0.1f * GetCount(attacker))));
                        victimBody.ClearTimedBuffs(BuffCatalog.FindBuffIndex("bdSlow60"));
                    }

                    if (victimBody.HasBuff(BuffCatalog.FindBuffIndex("bdSlow80")))
                    {
                        //Log.LogInfo("test80");
                        float remainingTime = 0f;
                        foreach (CharacterBody.TimedBuff t in victimBody.timedBuffs)
                        {
                            if (t.buffIndex == BuffCatalog.FindBuffIndex("bdSlow80"))
                            {
                                remainingTime = t.timer;
                            }
                        }
                        victimBody.AddTimedBuff(betterSlow80, remainingTime * (1.25f + (0.1f * GetCount(attacker))));
                        victimBody.ClearTimedBuffs(BuffCatalog.FindBuffIndex("bdSlow80"));
                    }
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(betterSlow50))
            {
                args.moveSpeedReductionMultAdd += 0.6f;
                args.attackSpeedMultAdd -= 0.3f;
            }

            if (sender.HasBuff(betterSlow60))
            {
                args.moveSpeedReductionMultAdd += 0.7f;
                args.attackSpeedMultAdd -= 0.35f;
            }

            if (sender.HasBuff(betterSlow80))
            {
                args.moveSpeedReductionMultAdd += 0.9f;
                args.attackSpeedMultAdd -= 0.45f;
            }
        }
    }
}