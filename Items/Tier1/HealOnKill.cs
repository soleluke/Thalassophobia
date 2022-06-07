using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier1
{
    public class HealOnKill : ItemBase<HealOnKill>
    {
        public override string ItemName => "Predatory Fungus";

        public override string ItemLangTokenName => "HEAL_ON_KILL";

        public override string ItemPickupDesc => "Gain a burst of regeneration on killing an enemy.";

        public override string ItemFullDescription => "Gain <style=cIsHealing>regeneration</style> for <style=cIsHealing>3</style> <style=cStack>(+1 per stack)</style> seconds on killing an enemy.";
        
        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("PFunfusModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("PredatoryFungusIcon.png");

        // Item stats
        private float time;
        BuffDef regen;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Healing, ItemTag.OnKillEffect };

            time = config.Bind<float>("Item: " + ItemName, "Time", 3f, "").Value;

            regen = ScriptableObject.CreateInstance<BuffDef>();
            regen.name = "Fungus Regeneration";
            regen.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            regen.canStack = true;
            regen.isDebuff = false;
            regen.isHidden = true;
            ContentAddition.AddBuffDef(regen);

            bool check = TempVisualEffectAPI.AddTemporaryVisualEffect(Plugin.assetBundle.LoadAsset<GameObject>("Assets/Assembly/MyAssets/Effects/RegenEmitter.prefab"), condition => {
                return condition.HasBuff(regen);
            }, true);

            Log.LogInfo("Temp effect check: " + check);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) => {
                orig(self, damageReport);
                if (damageReport.attacker)
                {
                    var itemCount = GetCount(damageReport.attackerBody);
                    if (itemCount > 0)
                    {
                        damageReport.attackerBody.AddTimedBuff(regen, 3 + (1 * itemCount));
                    }
                }
            };
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int count = sender.GetBuffCount(regen);
            if (count > 0)
            {
                args.baseRegenAdd += 0.25f * sender.level;
                args.regenMultAdd += 1.5f;
            }
        }
    }
}