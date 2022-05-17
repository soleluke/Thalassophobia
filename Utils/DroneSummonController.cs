using RoR2;
using RoR2.Navigation;
using Thalassophobia.Items.Tier3;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Thalassophobia.Utils
{
    class DroneSummonController : MonoBehaviour
    {
        public CharacterMaster owner;
        public float cooldown = 15f;
        public int damage = 0;
        public int items = 0;
        public int summonItemCount = 0;
        private float time = 0f;
        private int hp = 10;
        private bool isActive = false;
        private CharacterMaster drone;

        private void Start()
        {
            SummonDrone();
        }

        private void FixedUpdate()
        {
            this.time += Time.fixedDeltaTime;
            if (this.time >= cooldown && !isActive)
            {
                time = 0;
                SummonDrone();
            }
        }

        private void SummonDrone()
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
            string masterName = "MegaDroneMaster";
            NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Air);
            List<NodeGraph.NodeIndex> list = nodeGraph.FindNodesInRangeWithFlagConditions(
                    owner.GetBody().transform.position,
                    3,
                   30,
                    (HullMask)(1 << (int)HullClassification.Human),
                    NodeFlags.None,
                    NodeFlags.NoCharacterSpawn,
                    true
                );
            if (Plugin.DEBUG)
            {
                Log.LogInfo(list.Count);
            }
            if (list.Count > 0)
            {
                Inventory inventoryFiltered = new Inventory();
                inventoryFiltered.CopyItemsFrom(owner.inventory, (ItemIndex index) =>
                {
                    return !ItemCatalog.GetItemDef(index).ContainsTag(ItemTag.CannotCopy);
                });
                int index = UnityEngine.Random.Range(0, list.Count - 1);
                NodeGraph.NodeIndex nodeIndex = list[index];
                Vector3 pos;
                nodeGraph.GetNodePosition(nodeIndex, out pos);
                drone = new MasterSummon
                {
                    masterPrefab = MasterCatalog.FindMasterPrefab(masterName),
                    position = pos,
                    rotation = owner.GetBody().transform.rotation,
                    summonerBodyObject = owner.GetBodyObject(),
                    ignoreTeamMemberLimit = true,
                    teamIndexOverride = owner.teamIndex
                }.Perform();
                for (int num = 0; num < 10 * summonItemCount; num++)
                {
                    if (inventoryFiltered.itemAcquisitionOrder.Count <= 0)
                    {
                        break;
                    }
                    int itemIndexToGive = UnityEngine.Random.Range(0, inventoryFiltered.itemAcquisitionOrder.Count);
                    drone.inventory.GiveItem(inventoryFiltered.itemAcquisitionOrder[itemIndexToGive]);
                    inventoryFiltered.RemoveItem(inventoryFiltered.itemAcquisitionOrder[itemIndexToGive], 1);
                }
                if (Plugin.DEBUG)
                {
                    string s = "";
                    foreach (ItemIndex itemIndex in drone.inventory.itemAcquisitionOrder)
                    {
                        s += ItemCatalog.GetItemDef(itemIndex).name + " x" + drone.inventory.GetItemCount(itemIndex) + " || ";
                    }
                    Log.LogInfo($"Spawning {masterName}\n" +
                        $"Spawned with items: {s}");
                }
                drone.inventory.GiveItem(RoR2Content.Items.BoostHp, hp);
                drone.inventory.GiveItem(RoR2Content.Items.BoostDamage, damage);
                isActive = true;
                drone.onBodyDestroyed += Drone_onBodyDestroyed;
            }
        }

        private void Drone_onBodyDestroyed(CharacterBody body)
        {
            isActive = false;
        }

        public void EndSummons()
        {
            Destroy(this);
        }
    }
}
