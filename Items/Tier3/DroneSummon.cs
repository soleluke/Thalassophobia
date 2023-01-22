using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Navigation;
using System.Collections.Generic;
using Thalassophobia.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

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

        private DeployableSlot slot;
        private CharacterSpawnCard spawnCard;

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

            cooldown = config.Bind<float>("Item: " + ItemName, "Cooldown", 60f, "Time between spawning drones.").Value;
            numItems = config.Bind<int>("Item: " + ItemName, "NumberOfItems", 10, "Number of items the drones copy.").Value;
            damage = config.Bind<float>("Item: " + ItemName, "Damage", 100f, "Percent damage the drones deal.").Value;

            slot = R2API.DeployableAPI.RegisterDeployableSlot(delegate (CharacterMaster self, int mult) { return 1; });
            spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Drones/cscMegaDrone.asset").WaitForCompletion();
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
            if (self && self.inventory && self.master)
            {
                if (self.inventory.GetItemCount(this.ItemDef.itemIndex) > 0 && self.master.GetDeployableCount(slot) < 1)
                {
                    SummonDrone(self.master);
                }
            }
        }

        private void SummonDrone(CharacterMaster owner)
        {
            if (Plugin.DEBUG)
            {
                if (!owner)
                {
                    Log.LogInfo("Owner is null");
                }
                else
                {
                    Log.LogInfo($"Attempting spawn drone at {owner.GetBody().transform.position.x}, {owner.GetBody().transform.position.y}, {owner.GetBody().transform.position.z}");
                }
            }
            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                minDistance = 3f,
                maxDistance = 40f,
                spawnOnTarget = owner.GetBody().transform
            }, RoR2Application.rng);
            directorSpawnRequest.summonerBodyObject = owner.GetBodyObject();
            directorSpawnRequest.onSpawnedServer = new System.Action<SpawnCard.SpawnResult>(delegate (SpawnCard.SpawnResult spawnResult)
            {
                if (spawnResult.success && spawnResult.spawnedInstance)
                {
                    CharacterMaster drone = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
                    Deployable deployable = spawnResult.spawnedInstance.AddComponent<Deployable>();
                    owner.AddDeployable(deployable, slot);
                    deployable.onUndeploy = (deployable.onUndeploy ?? new UnityEvent());
                    deployable.onUndeploy.AddListener(new UnityAction(drone.TrueKill));

                    Inventory inventoryFiltered = new Inventory();
                    inventoryFiltered.CopyItemsFrom(owner.inventory, (ItemIndex index) =>
                    {
                        return !ItemCatalog.GetItemDef(index).ContainsTag(ItemTag.CannotCopy);
                    });
                    for (int num = 0; num < 10 * owner.inventory.GetItemCount(this.ItemDef.itemIndex); num++)
                    {
                        if (inventoryFiltered.itemAcquisitionOrder.Count <= 0)
                        {
                            break;
                        }
                        int itemIndexToGive = UnityEngine.Random.Range(0, inventoryFiltered.itemAcquisitionOrder.Count);
                        drone.inventory.GiveItem(inventoryFiltered.itemAcquisitionOrder[itemIndexToGive]);
                        inventoryFiltered.RemoveItem(inventoryFiltered.itemAcquisitionOrder[itemIndexToGive], 1);
                    }
                    drone.inventory.GiveItem(RoR2Content.Items.BoostHp, 10);
                }
            });
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        }
    }
}