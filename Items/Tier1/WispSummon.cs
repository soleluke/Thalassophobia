using BepInEx.Configuration;
using R2API;
using RoR2;
using Thalassophobia.Items.Lunar;
using Thalassophobia.Utils;
using UnityEngine;
using static On.RoR2.GlobalEventManager;
using static RoR2.DotController;

namespace Thalassophobia.Items.Tier1
{
    public class WispSummon : ItemBase<WispSummon>
    {
        public override string ItemName => "Fragile Mask";

        public override string ItemLangTokenName => "WISP_SUMMON";

        public override string ItemPickupDesc => "Spawn an allied wisp.";

        public override string ItemFullDescription => "Spawns a <style=cIsUtility>Lesser Wisp</style> to fight for you. The wisp has <style=cIsDamage>200%</style> damage and <style=cIsHealing>200%</style> health. When the wisp dies this item <style=cIsUtility>breaks</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();

            WispSummonController.tier1Def = this.ItemDef;
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Damage };
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnItemAddedClient += CharacterMaster_OnItemAddedClient;
        }

        private void CharacterMaster_OnItemAddedClient(On.RoR2.CharacterMaster.orig_OnItemAddedClient orig, CharacterMaster self, ItemIndex itemIndex)
        {
            orig(self, itemIndex);
            if (itemIndex == ItemDef.itemIndex)
            {
                if (self.GetBodyObject().GetComponent<WispSummonController>())
                {
                    WispSummonController wispController = self.GetBodyObject().GetComponent<WispSummonController>();
                    wispController.SummonWisp("WispMaster");
                }
                else
                {
                    WispSummonController wispController = self.GetBodyObject().AddComponent<WispSummonController>();
                    wispController.owner = self;
                    wispController.SummonWisp("WispMaster");
                }
            }
        }
    }
}