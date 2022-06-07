using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static On.RoR2.CharacterBody;

namespace Thalassophobia.Items.Tier2
{
    public class EliteKillRemovesDebuff : ItemBase<EliteKillRemovesDebuff>
    {
        public override string ItemName => "Diving Gear";

        public override string ItemLangTokenName => "DEBUFF_TIME_DOWN";

        public override string ItemPickupDesc => "Killing an elite removes a debuff";

        public override string ItemFullDescription => "Killing an elite monster removes <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> debuff. Can only be triggered every <style=cIsUtility>5</style> seconds.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => Plugin.assetBundle.LoadAsset<GameObject>("Assets/Assembly/MyAssets/Models/DiveModel.prefab");

        public override Sprite ItemIcon => Plugin.assetBundle.LoadAsset<Sprite>("Assets/Assembly/MyAssets/Icons/DiveIcon.png");

        static BuffDef cooldown;
        static BuffDef ready;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Utility, ItemTag.OnKillEffect };

            cooldown = ScriptableObject.CreateInstance<BuffDef>();
            cooldown.name = "Elite Cleansing Cooldown";
            cooldown.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            cooldown.canStack = false;
            cooldown.isDebuff = false;
            cooldown.isCooldown = true;
            cooldown.buffColor = Color.yellow;
            ContentAddition.AddBuffDef(cooldown);

            ready = ScriptableObject.CreateInstance<BuffDef>();
            ready.name = "Elite Cleansing Ready";
            ready.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            ready.canStack = false;
            ready.isDebuff = false;
            ready.isCooldown = false;
            ready.buffColor = Color.red;
            ContentAddition.AddBuffDef(ready);
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
                if (self && self.GetBody())
                {
                    self.GetBody().AddItemBehavior<CleanseOnKill>(GetCount(self.GetBody()));
                }
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (damageReport.victimIsElite)
            {
                CharacterBody body = damageReport.attackerBody;

                if (body.HasBuff(ready))
                {
                    for (int i = 0; i < GetCount(body);)
                    {
                        BuffIndex buffIndex = (BuffIndex)0;
                        BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
                        while (buffIndex < buffCount)
                        {
                            BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
                            if ((buffDef.isDebuff) || buffDef.isCooldown)
                            {
                                body.ClearTimedBuffs(buffIndex);
                                i++;
                                body.RemoveBuff(ready);
                                body.AddTimedBuff(cooldown, 5);
                            }
                            buffIndex++;
                        }
                    }
                }
            }
        }
        public class CleanseOnKill : CharacterBody.ItemBehavior
        {
            private void Start()
            {
                if (!(this.body.HasBuff(cooldown) || this.body.HasBuff(cooldown)))
                {
                    this.body.AddTimedBuff(cooldown, 2.0f);
                }
            }

            private void OnDisable()
            {
                if (this.body)
                {
                    if (this.body.HasBuff(ready))
                    {
                        this.body.RemoveBuff(ready);
                    }
                    if (this.body.HasBuff(cooldown))
                    {
                        this.body.RemoveBuff(cooldown);
                    }
                }
            }
            private void FixedUpdate()
            {
                bool flag = this.body.HasBuff(cooldown);
                bool flag2 = this.body.HasBuff(ready);
                if (!flag && !flag2)
                {
                    if (Plugin.DEBUG)
                    {
                        //Log.LogInfo("Cleanse Ready");
                    }
                    this.body.AddBuff(ready);
                }
                if (flag2 && flag)
                {
                    this.body.RemoveBuff(ready);
                }
            }
        }
    }
}