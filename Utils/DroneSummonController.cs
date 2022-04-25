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
        public float cooldown = 0f;
        public int damage = 0;
        public int maxDrones = 0;
        public int items = 0;
        public int summonItemCount = 0;
        private float time = 0f;
        private int decay = 40;
        private int hp = 10;
        private List<CharacterMaster> allDrones;

        private void Start()
        {
            allDrones = new List<CharacterMaster>();
            SummonDrone();
        }

        private void FixedUpdate()
        {
            this.time += Time.fixedDeltaTime;
            if (time >= cooldown)
            {
                if (Plugin.DEBUG)
                {
                    Log.LogInfo("Drone spawn time: " + time);
                }
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
                    Log.LogInfo($"Attempting spawn drone at {owner.transform.position.x}, {owner.transform.position.y}, {owner.transform.position.z}");
                }
            }
            CharacterMaster characterMaster;
            int decayTemp = decay;
            float roll = UnityEngine.Random.Range(0.0f, 100.0f);
            string masterName = "DroneBackupMaster";
            if (roll <= 33)
            {
                masterName = "FlameDroneMaster";
            }
            else if (roll <= 66)
            {
                masterName = "DroneMissileMaster";
            }
            else
            {
                masterName = "DroneBackupMaster";
            }
            if (Util.CheckRoll(1.5f, owner))
            {
                masterName = "MegaDroneMaster";
                decayTemp = decayTemp * 2;
            }
            NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Air);
            List<NodeGraph.NodeIndex> list = nodeGraph.FindNodesInRangeWithFlagConditions(
                    owner.GetBody().transform.position,
                    3,
                    15,
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
                characterMaster = new MasterSummon
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
                    characterMaster.inventory.GiveItem(inventoryFiltered.itemAcquisitionOrder[itemIndexToGive]);
                    inventoryFiltered.RemoveItem(inventoryFiltered.itemAcquisitionOrder[itemIndexToGive], 1);
                }
                if (Plugin.DEBUG)
                {
                    string s = "";
                    foreach (ItemIndex itemIndex in characterMaster.inventory.itemAcquisitionOrder)
                    {
                        s += ItemCatalog.GetItemDef(itemIndex).name + " x" + characterMaster.inventory.GetItemCount(itemIndex) + " || ";
                    }
                    Log.LogInfo($"Spawning {masterName}\n" +
                        $"Spawned with items: {s}");
                }
                characterMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, decayTemp);
                characterMaster.inventory.GiveItem(RoR2Content.Items.BoostHp, hp);
                characterMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, damage);
            }
        }

        public void EndSummons()
        {
            Destroy(this);
        }
    }
}
