using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using Thalassophobia.Utils;
using UnityEngine;

namespace Thalassophobia.Items.Tier3
{
    public class DroneSummon : ItemBase<DroneSummon>
    {
        public override string ItemName => "Overclocked Control Unit";

        public override string ItemLangTokenName => "DRONE_SUMMON";

        public override string ItemPickupDesc => "Summon a powerful drone that copies your items.";

        public override string ItemFullDescription => "Summons a <style=cIsUtility>TC-280 Prototype</style> to fight for you. It copies <style=cIsUtility>10</style> <style=cStack>(+10 per stack)</style> of your <style=cIsUtility>items</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("Assets/Assembly/MyAssets/Models/ControlUnitModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/ControlUnitIcon.png");

        // Item stats
        private float cooldown;
        private int numItems;
        private float damage;

        private static List<DroneSummonController> controllers = new List<DroneSummonController>();


        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Damage, ItemTag.CannotCopy, ItemTag.Utility };

            cooldown = config.Bind<float>("Item: " + ItemName, "Cooldown", 5f, "Time between spawning drones.").Value;
            numItems = config.Bind<int>("Item: " + ItemName, "NumberOfItems", 10, "Number of items the drones copy.").Value;
            damage = config.Bind<float>("Item: " + ItemName, "Damage", 100f, "Percent damage the drones deal.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.Update += CharacterBody_Update;
        }

        private void CharacterBody_Update(On.RoR2.CharacterBody.orig_Update orig, CharacterBody self)
        {
            orig(self);
            if (GetCount(self) > 0)
            {
                if (self.GetComponent<DroneSummonController>())
                {
                    self.GetComponent<DroneSummonController>().summonItemCount = GetCount(self);
                }
                else
                {
                    Log.LogInfo("test");
                    DroneSummonController controller = self.gameObject.AddComponent<DroneSummonController>();
                    controller.owner = self.master;
                    controller.cooldown = cooldown;
                    controller.damage = (int)damage;
                    controller.items = numItems;
                    controller.summonItemCount = GetCount(self);
                    controllers.Add(controller);
                }
            }
        }
    }
}