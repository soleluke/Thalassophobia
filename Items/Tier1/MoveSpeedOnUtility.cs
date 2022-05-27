using BepInEx.Configuration;
using R2API;
using RoR2;
using Thalassophobia.Items.Lunar;
using UnityEngine;
using static On.RoR2.GlobalEventManager;
using static RoR2.DotController;

namespace Thalassophobia.Items.Tier1
{
    public class MoveSpeedOnUtility : ItemBase<MoveSpeedOnUtility>
    {
        public override string ItemName => "Booster Boots";

        public override string ItemLangTokenName => "MOVE_SPEED_ON_UTILITY";

        public override string ItemPickupDesc => "Move faster when using your utility skill.";

        public override string ItemFullDescription => "<style=cIsUtility>+25%</style> movement speed for <style=cIsUtility>2</style> <style=cStack>(+1 per stack)</style> seconds after using your <style=cIsUtility>utility skill</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        BuffDef speedUp;
        private float time;
        private float baseSpeedUp;
        private float timeScale;
        private float speedScale;


        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Utility };

            time = config.Bind<float>("Item: " + ItemName, "Time", 2f, "").Value;
            baseSpeedUp = config.Bind<float>("Item: " + ItemName, "Speed Up", 0.25f, "").Value;
            timeScale = config.Bind<float>("Item: " + ItemName, "Time Scale", 1f, "").Value;
            speedScale = config.Bind<float>("Item: " + ItemName, "Speed Scale", 0.10f, "").Value;

            speedUp = ScriptableObject.CreateInstance<BuffDef>();
            speedUp.name = "Booster Boots";
            speedUp.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            speedUp.canStack = false;
            speedUp.isDebuff = false;
            ContentAddition.AddBuffDef(speedUp);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(speedUp))
            {
                args.moveSpeedMultAdd += 0.25f + (speedScale*(GetCount(sender)-1));
            }
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig(self, skill);
            if (GetCount(self) > 0)
            {
                if (skill == self.skillLocator.utility)
                {
                    self.AddTimedBuff(speedUp, time + (timeScale*(GetCount(self)-1)));
                }
            }
        }
    }
}