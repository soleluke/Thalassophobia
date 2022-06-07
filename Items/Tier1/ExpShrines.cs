using BepInEx.Configuration;
using R2API;
using RoR2;
using Thalassophobia.Items.Lunar;
using UnityEngine;
using static On.RoR2.GlobalEventManager;
using static RoR2.DotController;

namespace Thalassophobia.Items.Tier1
{
    public class ExpShrines : ItemBase<ExpShrines>
    {
        public override string ItemName => "Cracked Orb";

        public override string ItemLangTokenName => "EXP_SHRINES";

        public override string ItemPickupDesc => "Gain experience when using a shrine.";

        public override string ItemFullDescription => "Gain <style=cIsUtility>15</style> <style=cStack>(+15 per stack)</style> experience when using a shrine.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("CrackedOrbModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("CrackedOrbIcon.png");



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
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            CharacterBody body = activator.GetComponent<CharacterBody>();
            int count = GetCount(body);
            if (self.CanBeAffordedByInteractor(activator) && count > 0 && self.isShrine)
            {
                if (Plugin.DEBUG)
                {
                    Log.LogInfo("Orb");
                }
                body.master.GiveExperience((ulong)(15 * count));
            }
            orig(self, activator);
        }
    }
}