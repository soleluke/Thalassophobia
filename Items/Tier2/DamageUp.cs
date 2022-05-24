using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using R2API.Utils;

namespace Thalassophobia.Items.Tier2
{
    public class DamageUp : ItemBase<DamageUp>
    {
        public override string ItemName => "Steroids";

        public override string ItemLangTokenName => "DAMAGE_UP";

        public override string ItemPickupDesc => "The more items you have, the more powerful you become.";

        public override string ItemFullDescription => "Gain +3% (+2% per stack) damage and +3% (+2% per stack) attack speed for every damage item you have.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float damageUp;
        private float damageUpStack;
        private float attackSpeedUp;
        private float attackSpeedUpStack;

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

            damageUp = config.Bind<float>("Item: " + ItemName, "DamageIncrease", 0.03f, "The percent increase to base damage where 1.0 is 100%.").Value;
            damageUpStack = config.Bind<float>("Item: " + ItemName, "DamageIncreasePerStack", 0.02f, "The percent increase to base damage for every stack of the item over 1 stack where 1.0 is 100%.").Value;
            attackSpeedUp = config.Bind<float>("Item: " + ItemName, "AttackSpeedIncrease", 0.03f, "The percent increase to attack speed where 1.0 is 100%.").Value;
            attackSpeedUpStack = config.Bind<float>("Item: " + ItemName, "AttackSpeedIncreasePerStack", 0.02f, "The percent increase to attack speed for every stack of the item over 1 stack where 1.0 is 100%.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                orig(self);
                var count = GetCount(self);
                if (count > 0)
                {
                    float damageIncrease = 0;
                    float attackSpeedIncrease = 0;
                    float damageItems = 0;
                    Inventory inventory = self.master.inventory;
                    foreach (ItemIndex index in inventory.itemAcquisitionOrder)
                    {
                        if (ItemCatalog.GetItemDef(index).ContainsTag(ItemTag.Damage))
                        {
                            damageItems += self.inventory.GetItemCount(index);
                        }
                    }
                    damageIncrease = (damageUp + (damageUpStack * (count - 1))) * damageItems;
                    attackSpeedIncrease = (attackSpeedUp + attackSpeedUpStack * (count - 1)) * damageItems;
                    Reflection.SetPropertyValue<float>(self, "damage", self.damage + (self.damage * damageIncrease));
                    Reflection.SetPropertyValue<float>(self, "attackSpeed", self.attackSpeed + (self.attackSpeed * attackSpeedIncrease));
                }
            };
        }
    }
}