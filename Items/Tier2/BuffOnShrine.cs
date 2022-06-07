using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Thalassophobia.Items.Tier2
{
    public class BuffOnShrine : ItemBase<BuffOnShrine>
    {
        public override string ItemName => "Museum Brochure";

        public override string ItemLangTokenName => "BUFF_ON_SHRINE";

        public override string ItemPickupDesc => "Using a shrine increases your stats.";

        public override string ItemFullDescription => "Using a shrine gives you either <style=cIsUtility>+10%</style> <style=cStack>(+5% per stack)</style> <style=cIsDamage>attack speed</style>, <style=cIsUtility>armor</style>, <style=cIsHealing>health regeneration</style>, or <style=cIsDamage>critical strike chance chance</style> for the rest of the stage.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("Assets/Assembly/MyAssets/Models/PaperModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/PaperIcon.png");


        BuffDef shownBuff;
        BuffDef armorBuff;
        BuffDef attackSpeedBuff;
        BuffDef critBuff;
        BuffDef regenBuff;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Utility, ItemTag.Damage, ItemTag.InteractableRelated };

            shownBuff = ScriptableObject.CreateInstance<BuffDef>();
            shownBuff.name = "Musuem Brochure Buff";
            shownBuff.iconSprite = Plugin.assetBundle.LoadAsset<Sprite>("PaperBuffIcon.png");
            shownBuff.canStack = true;
            shownBuff.isDebuff = false;
            ContentAddition.AddBuffDef(shownBuff);

            armorBuff = ScriptableObject.CreateInstance<BuffDef>();
            armorBuff.name = "Armor";
            armorBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            armorBuff.canStack = true;
            armorBuff.isDebuff = false;
            armorBuff.isHidden = true;
            ContentAddition.AddBuffDef(armorBuff);

            attackSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            attackSpeedBuff.name = "Attack Speed";
            attackSpeedBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            attackSpeedBuff.canStack = true;
            attackSpeedBuff.isDebuff = false;
            attackSpeedBuff.isHidden = true;
            ContentAddition.AddBuffDef(attackSpeedBuff);

            critBuff = ScriptableObject.CreateInstance<BuffDef>();
            critBuff.name = "Critical Strikes";
            critBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            critBuff.canStack = true;
            critBuff.isDebuff = false;
            critBuff.isHidden = true;
            ContentAddition.AddBuffDef(critBuff);

            regenBuff = ScriptableObject.CreateInstance<BuffDef>();
            regenBuff.name = "Regeneration";
            regenBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            regenBuff.canStack = true;
            regenBuff.isDebuff = false;
            regenBuff.isHidden = true;
            ContentAddition.AddBuffDef(regenBuff);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            CharacterBody body = activator.GetComponent<CharacterBody>();
            int count = GetCount(body);
            if (self.CanBeAffordedByInteractor(activator) && count > 0 && self.isShrine)
            {
                GiveBuff(body);
            }
            orig(self, activator);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int count = GetCount(sender);
            if (sender.HasBuff(armorBuff))
            {
                args.armorAdd += 10f + (5 * (count - 1));
            }

            if (sender.HasBuff(attackSpeedBuff))
            {
                args.attackSpeedMultAdd += 0.1f + (0.05f * (count - 1));
            }

            if (sender.HasBuff(critBuff))
            {
                args.critAdd += 10f + (5 * (count - 1));
            }

            if (sender.HasBuff(regenBuff))
            {
                args.regenMultAdd += 0.1f + (0.05f * (count - 1));
            }
        }

        private void GiveBuff(CharacterBody body)
        {
            body.AddBuff(shownBuff);
            int buffIndex = Random.Range(0, 3);
            switch (buffIndex)
            {
                case 0:
                    if (Plugin.DEBUG)
                    {
                        Log.LogInfo("Giving attack speed");
                    }
                    body.AddBuff(attackSpeedBuff);
                    break;
                case 1:
                    if (Plugin.DEBUG)
                    {
                        Log.LogInfo("Giving crit");
                    }
                    body.AddBuff(critBuff);
                    break;
                case 2:
                    if (Plugin.DEBUG)
                    {
                        Log.LogInfo("Giving amor");
                    }
                    body.AddBuff(armorBuff);
                    break;
                case 3:
                    if (Plugin.DEBUG)
                    {
                        Log.LogInfo("Giving regen");
                    }
                    body.AddBuff(regenBuff);
                    break;
            }
        }
    }
}