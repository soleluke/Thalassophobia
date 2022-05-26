using RoR2;
using RoR2.Navigation;
using Thalassophobia.Items.Tier3;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Thalassophobia.Utils
{
    class WispSummonController : MonoBehaviour
    {
        public CharacterMaster owner;
        private List<CharacterMaster> wisps = new List<CharacterMaster>();
        public static ItemDef tier1Def;
        public static ItemDef tier2Def;
        public static ItemDef tier3Def;
        public static ItemDef tier1BrokenDef;
        public static ItemDef tier2BrokenDef;
        public static ItemDef tier3BrokenDef;
        public static ItemDef helperDef;

        private void Start()
        {
        }

        private void FixedUpdate()
        {
            for (int i = wisps.Count - 1; i >= 0; i--)
            {
                CharacterMaster master = wisps[i];
                if (master.IsDeadAndOutOfLivesServer())
                {
                    onBodyDestroyed(master.GetBody());
                }
            }
        }

        // WispMaster GreaterWispMaster LunarWispMaster
        public void SummonWisp(string masterName)
        {
            if (Plugin.DEBUG)
            {
                if (!owner)
                {
                    Log.LogInfo("Owner is null");
                }
                else
                {
                    Log.LogInfo($"Attempting spawn {masterName} at {owner.GetBody().transform.position.x}, {owner.GetBody().transform.position.y}, {owner.GetBody().transform.position.z}");
                }
            }
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
            int tier1 = owner.GetBody().inventory.GetItemCount(tier1Def);
            int tier2 = owner.GetBody().inventory.GetItemCount(tier2Def);
            int tier3 = owner.GetBody().inventory.GetItemCount(tier3Def);

            if (Plugin.DEBUG)
            {
                Log.LogInfo(wisps.Count + " <= " + (tier1 + tier2 + tier3));
            }

            if (list.Count > 0 && wisps.Count < tier1 + tier2 + tier3)
            {
                int index = UnityEngine.Random.Range(0, list.Count - 1);
                NodeGraph.NodeIndex nodeIndex = list[index];
                Vector3 pos;
                nodeGraph.GetNodePosition(nodeIndex, out pos);

                CharacterMaster wisp = new MasterSummon
                {
                    masterPrefab = MasterCatalog.FindMasterPrefab(masterName),
                    position = pos,
                    rotation = owner.GetBody().transform.rotation,
                    summonerBodyObject = owner.GetBodyObject(),
                    ignoreTeamMemberLimit = true,
                    teamIndexOverride = owner.teamIndex
                }.Perform();
                if (Plugin.DEBUG)
                {
                    Log.LogInfo($"Spawning {masterName}\n");
                }

                int tier1Broken = owner.GetBody().inventory.GetItemCount(tier1BrokenDef);
                int tier2Broken = owner.GetBody().inventory.GetItemCount(tier2BrokenDef);
                int tier3Broken = owner.GetBody().inventory.GetItemCount(tier3BrokenDef);

                wisp.inventory.GiveItem(RoR2Content.Items.BoostHp, 10 + (2 * tier1Broken));
                wisp.inventory.GiveItem(RoR2Content.Items.BoostDamage, 10 + (3 * tier2Broken));
                wisp.inventory.GiveItem(RoR2Content.Items.BoostAttackSpeed, 5 + (3 * tier2Broken));
                wisp.inventory.GiveItem(helperDef, 1 + tier1Broken + tier2Broken * 3 + tier3Broken * 10);
                wisp.inventory.GiveItem(RoR2Content.Items.TeamSizeDamageBonus, tier3Broken);
                wisp.inventory.GiveItem(RoR2Content.Items.ShinyPearl, tier3Broken * 5);
                wisp.inventory.GiveItem(RoR2Content.Items.MinionLeash);

                if (tier3Broken > 0)
                {
                    int count = EliteCatalog.eliteDefs.Length;
                    int eliteIndex = UnityEngine.Random.Range(0, count - 1);
                    Log.LogInfo(EliteCatalog.eliteDefs[eliteIndex].eliteEquipmentDef.nameToken);
                    wisp.inventory.SetEquipmentIndex(EliteCatalog.eliteDefs[eliteIndex].eliteEquipmentDef.equipmentIndex);
                }

                Log.LogInfo("test");

                wisps.Add(wisp);
            }
        }

        public void UpdateWispPower(ItemDef def)
        {
            foreach (CharacterMaster master in wisps)
            {
                Inventory inventory = master.inventory;
                if (def == tier1BrokenDef)
                {
                    inventory.GiveItem(RoR2Content.Items.BoostHp, 2);
                }
                else if (def == tier2BrokenDef)
                {
                    inventory.GiveItem(RoR2Content.Items.BoostDamage, 3);
                    inventory.GiveItem(RoR2Content.Items.BoostAttackSpeed, 3);
                }
                else if (tier3BrokenDef)
                {
                    inventory.GiveItem(RoR2Content.Items.TeamSizeDamageBonus, 1);
                    inventory.GiveItem(RoR2Content.Items.ShinyPearl, 5);
                    if (owner.inventory.GetItemCount(tier3BrokenDef) == 1)
                    {
                        int count = EliteCatalog.eliteDefs.Length;
                        int eliteIndex = UnityEngine.Random.Range(0, count - 1);
                        Log.LogInfo(EliteCatalog.eliteDefs[eliteIndex].eliteEquipmentDef.nameToken);
                        inventory.SetEquipmentIndex(EliteCatalog.eliteDefs[eliteIndex].eliteEquipmentDef.equipmentIndex);
                    }
                }
            }
        }

        private void onBodyDestroyed(CharacterBody obj)
        {
            if (Plugin.DEBUG)
            {
                Log.LogInfo(obj.baseNameToken);
            }
            wisps.Remove(obj.master);
            EffectData effect = new EffectData
            {
                origin = owner.GetBody().transform.position
            };
            switch (obj.baseNameToken)
            {
                case "WISP_BODY_NAME":
                    owner.inventory.RemoveItem(tier1Def);
                    owner.inventory.GiveItem(tier1BrokenDef);
                    CharacterMasterNotificationQueue.PushItemTransformNotification(owner,
                        tier1Def.itemIndex, tier1BrokenDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    UpdateWispPower(tier1BrokenDef);
                    effect.SetNetworkedObjectReference(base.gameObject);
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.fragileDamageBonusBreakEffectPrefab, effect, true);
                    break;
                case "GREATERWISP_BODY_NAME":
                    owner.inventory.RemoveItem(tier2Def);
                    owner.inventory.GiveItem(tier2BrokenDef);
                    CharacterMasterNotificationQueue.PushItemTransformNotification(owner,
                       tier2Def.itemIndex, tier2BrokenDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    UpdateWispPower(tier2BrokenDef);
                    effect.SetNetworkedObjectReference(base.gameObject);
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.fragileDamageBonusBreakEffectPrefab, effect, true);
                    break;
                case "LUNARWISP_BODY_NAME":
                    owner.inventory.RemoveItem(tier3Def);
                    owner.inventory.GiveItem(tier3BrokenDef);
                    CharacterMasterNotificationQueue.PushItemTransformNotification(owner,
                       tier3Def.itemIndex, tier3BrokenDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    UpdateWispPower(tier3BrokenDef);
                    effect.SetNetworkedObjectReference(base.gameObject);
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.fragileDamageBonusBreakEffectPrefab, effect, true);
                    break;
            }
        }
    }
}
