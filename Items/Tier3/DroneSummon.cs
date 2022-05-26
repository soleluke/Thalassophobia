using BepInEx.Configuration;
using R2API;
using RoR2;
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

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float cooldown;
        private int numItems;
        private int maxDrones;
        private float damage;
        private float megaDroneChance;


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
            On.RoR2.CharacterMaster.OnInventoryChanged += (orig, self) =>
            {
                orig(self);
                if (GetCount(self.GetBody()) > 0)
                {
                    if (self.GetBodyObject().GetComponent<DroneSummonController>())
                    {
                        self.GetBodyObject().GetComponent<DroneSummonController>().summonItemCount = GetCount(self);
                    }
                    else
                    {
                        self.GetBodyObject().AddComponent<DroneSummonController>();
                        self.GetBodyObject().GetComponent<DroneSummonController>().owner = self;
                        self.GetBodyObject().GetComponent<DroneSummonController>().cooldown = cooldown;
                        self.GetBodyObject().GetComponent<DroneSummonController>().damage = (int)damage;
                        self.GetBodyObject().GetComponent<DroneSummonController>().items = numItems;
                        self.GetBodyObject().GetComponent<DroneSummonController>().summonItemCount = GetCount(self);
                    }
                }
            };
        }
    }
}