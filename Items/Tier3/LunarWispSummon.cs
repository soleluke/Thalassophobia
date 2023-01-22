using BepInEx.Configuration;
using R2API;
using RoR2;
using Thalassophobia.Items.Lunar;
using Thalassophobia.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using static On.RoR2.GlobalEventManager;
using static RoR2.DotController;

namespace Thalassophobia.Items.Tier3
{
    public class LunarWispSummon : ItemBase<LunarWispSummon>
    {
        public override string ItemName => "Chimeric Mask";

        public override string ItemLangTokenName => "LUNAR_WISP_SUMMON";

        public override string ItemPickupDesc => "Spawn an allied Lunar Chimera.";

        public override string ItemFullDescription => "Spawns a <style=cIsUtility>Lunar Chimera Wisp</style> to fight for you. The wisp has <style=cIsDamage>200%</style> damage and <style=cIsHealing>200%</style> health. When the wisp dies this item <style=cIsUtility>breaks</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("Assets/Assembly/MyAssets/Models/CMaskModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/CMaskIcon.png");

        private DeployableSlot slot;
        private CharacterSpawnCard spawnCard;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();

            slot = R2API.DeployableAPI.RegisterDeployableSlot(delegate (CharacterMaster self, int mult) { return self.inventory.GetItemCount(this.ItemDef.itemIndex); });
            spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarWisp/cscLunarWisp.asset").WaitForCompletion();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Damage, ItemTag.CannotCopy };
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
                if (self.inventory.GetItemCount(this.ItemDef.itemIndex) > 0)
                {
                    int itemCount = self.inventory.GetItemCount(this.ItemDef.itemIndex);
                    int numSummoned = self.master.GetDeployableCount(slot);
                    if (itemCount > numSummoned)
                    {
                        SummonWisp(self.master);
                    }
                }
            }
        }

        private void SummonWisp(CharacterMaster owner)
        {
            int tier1Broken = owner.GetBody().inventory.GetItemCount(Tier1.WispSummonBroken.instance.ItemDef);
            int tier2Broken = owner.GetBody().inventory.GetItemCount(Tier2.GreaterWispSummonBroken.instance.ItemDef);
            int tier3Broken = owner.GetBody().inventory.GetItemCount(Tier3.LunarWispSummonBroken.instance.ItemDef);
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
                    CharacterMaster wisp = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
                    wisp.inventory.GiveItem(RoR2Content.Items.BoostHp, 10 + (2 * tier1Broken));
                    wisp.inventory.GiveItem(RoR2Content.Items.BoostDamage, 10 + (3 * tier2Broken));
                    wisp.inventory.GiveItem(RoR2Content.Items.BoostAttackSpeed, 5 + (3 * tier2Broken));
                    wisp.inventory.GiveItem(Tier1.FriendlyWispHelper.instance.ItemDef, 1 + tier1Broken + tier2Broken * 3 + tier3Broken * 10); ;
                    wisp.inventory.GiveItem(RoR2Content.Items.TeamSizeDamageBonus, tier3Broken);
                    wisp.inventory.GiveItem(RoR2Content.Items.ShinyPearl, tier3Broken * 5);
                    wisp.inventory.GiveItem(RoR2Content.Items.MinionLeash);

                    if (tier3Broken > 0)
                    {
                        int count = EliteCatalog.eliteDefs.Length;
                        int eliteIndex = UnityEngine.Random.Range(0, count - 1);
                        if (EliteCatalog.eliteDefs[eliteIndex].eliteEquipmentDef.nameToken == "EQUIPMENT_AFFIXGOLD_NAME")
                        {
                            eliteIndex -= 1;
                        }
                        Log.LogInfo(EliteCatalog.eliteDefs[eliteIndex].eliteEquipmentDef.nameToken);
                        wisp.inventory.SetEquipmentIndex(EliteCatalog.eliteDefs[eliteIndex].eliteEquipmentDef.equipmentIndex);
                    }
                    wisp.onBodyDeath = (wisp.onBodyDeath ?? new UnityEvent());
                    wisp.onBodyDeath.AddListener(() =>
                    {
                        EffectData effect = new EffectData
                        {
                            origin = owner.GetBody().transform.position
                        };
                        owner.inventory.RemoveItem(this.ItemDef);
                        owner.inventory.GiveItem(Tier3.LunarWispSummonBroken.instance.ItemDef);
                        CharacterMasterNotificationQueue.PushItemTransformNotification(owner,
                           this.ItemDef.itemIndex, Tier3.LunarWispSummonBroken.instance.ItemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                        effect.SetNetworkedObjectReference(owner.gameObject);
                        EffectManager.SpawnEffect(HealthComponent.AssetReferences.fragileDamageBonusBreakEffectPrefab, effect, true);
                    });
                    Deployable deployable = spawnResult.spawnedInstance.AddComponent<Deployable>();
                    owner.AddDeployable(deployable, slot);
                    deployable.onUndeploy = new UnityEvent();
                }
            });
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        }
    }
}