using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using static R2API.SoundAPI;

namespace RoR2Mod
{
    public static class AssetManager
    {
        public enum ItemPrefabIndex
        {
            LunarDice = 0,
            AffixPurePickup = 1
        }

        public enum MaterialIndex
        {
            AffixPureOverlay = 0,
            AffixUnstableOverlay = 1,
            AffixWitheringOverlay = 2,
            AffixArmoredOverlay = 3
        }

        public enum SpriteIndex
        {
            AffixPureIcon = 0
        }

        public enum ProjectileIndex
        {
            Bean = 0
        }

        public enum EffectPrefabIndex
        {
            Bean = 0
        }

        // Location of the plugin
        private static string pluginfolder;

        // Asset bundle to load
        private static string assetBundleName = "myassets";
        private static AssetBundle assets;

        // Prefab location inside asset bundle
        private static string lunarDice = "Assets/Prefabs/LunarDice.prefab";
        private static string affixPurePickup = "Assets/Prefabs/PickupAffixPure.prefab";
        private static string bean = "Assets/Prefabs/Projectiles/BeanProjectile.prefab";

        // Effect
        private static string beanBurst = "Assets/Prefabs/Effects/BeanBurst.prefab";

        // Mat location inside bundle
        private static string affixPureOverlay = "Assets/Materials/matAffixPureOverlay.mat";
        private static string affixWitheringOverlay = "Assets/Materials/matAffixWitheringOverlay.mat";
        private static string affixUnstableOverlay = "Assets/Materials/matAffixUnstableOverlay.mat";
        private static string affixArmoredOverlay = "Assets/Materials/matAffixArmoredOverlay.mat";

        // Item prefabs
        private static Dictionary<ItemPrefabIndex, GameObject> itemPrefabs = new Dictionary<ItemPrefabIndex, GameObject>();

        // Mats
        private static Dictionary<MaterialIndex, Material> mats = new Dictionary<MaterialIndex, Material>();

        // Sprites
        private static Dictionary<MaterialIndex, Material> sprites = new Dictionary<MaterialIndex, Material>();

        // Assets
        private static Dictionary<ProjectileIndex, GameObject> projectiles = new Dictionary<ProjectileIndex, GameObject>();
        private static Dictionary<EffectPrefabIndex, GameObject> effects = new Dictionary<EffectPrefabIndex, GameObject>();

        /// <summary>
        /// Loads all assets
        /// </summary>
        /// <param name="plugin"></param>
        public static void Init(BaseUnityPlugin plugin)
        {
            // Load soundbank
            using (Stream bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RoR2Mod.ThalassophobiaSounds.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundBanks.Add(bytes);
            }


            pluginfolder = System.IO.Path.GetDirectoryName(plugin.GetType().Assembly.Location);

            // load assetbundle
            assets = AssetBundle.LoadFromFile($"{pluginfolder}/{assetBundleName}");

            ApplyShaders();

            GameObject lunarDicePrefab = assets.LoadAsset<GameObject>(lunarDice);
            itemPrefabs.Add(ItemPrefabIndex.LunarDice, lunarDicePrefab);
            
            GameObject affixPurePickupPrefab = assets.LoadAsset<GameObject>(affixPurePickup);
            itemPrefabs.Add(ItemPrefabIndex.AffixPurePickup, affixPurePickupPrefab);

            Material affixPureMat = assets.LoadAsset<Material>(affixPureOverlay);
            mats.Add(MaterialIndex.AffixPureOverlay, affixPureMat);

            Material affixUnstableMat = assets.LoadAsset<Material>(affixUnstableOverlay);
            mats.Add(MaterialIndex.AffixUnstableOverlay, affixUnstableMat);

            Material affixArmoredMat = assets.LoadAsset<Material>(affixArmoredOverlay);
            mats.Add(MaterialIndex.AffixArmoredOverlay, affixArmoredMat);

            Material affixWitheringMat = assets.LoadAsset<Material>(affixWitheringOverlay);
            mats.Add(MaterialIndex.AffixWitheringOverlay, affixWitheringMat);

            GameObject beanEffect = assets.LoadAsset<GameObject>(beanBurst);
            EffectAPI.AddEffect(beanEffect);
            effects.Add(EffectPrefabIndex.Bean, beanEffect);

            GameObject beanPrefab = assets.LoadAsset<GameObject>(bean);
            projectiles.Add(ProjectileIndex.Bean, beanPrefab);
            
        }

        /// <summary>
        /// Gets an item definition
        /// </summary>
        /// <param name="index">The index to get</param>
        public static GameObject GetPrefab(ItemPrefabIndex index)
        {
            GameObject prefab;
            itemPrefabs.TryGetValue(index, out prefab);
            if (prefab == null)
            {
                Log.LogError("Null Prefab");
            }
            return prefab;
        }

        /// <summary>
        /// Gets an item definition
        /// </summary>
        /// <param name="index">The index to get</param>
        public static GameObject GetEffect(EffectPrefabIndex index)
        {
            GameObject prefab;
            effects.TryGetValue(index, out prefab);
            if (prefab == null)
            {
                Log.LogError("Null Prefab");
            }
            return prefab;
        }


        /// <summary>
        /// Gets an item definition
        /// </summary>
        /// <param name="index">The index to get</param>
        public static GameObject GetProjectile(ProjectileIndex index)
        {
            GameObject prefab;
            projectiles.TryGetValue(index, out prefab);
            if (prefab == null)
            {
                Log.LogError("Null Prefab");
            }
            return prefab;
        }

        /// <summary>
        /// Gets an item definition
        /// </summary>
        /// <param name="index">The index to get</param>
        public static Material GetMaterial(MaterialIndex index)
        {
            Material mat;
            mats.TryGetValue(index, out mat);
            if (mat == null)
            {
                Log.LogError("Null Material");
            }
            return mat;
        }

        public static void ApplyShaders()
        {
            var materials = assets.LoadAllAssets<Material>();
            foreach (Material material in materials)
                if (material.shader.name.StartsWith("StubbedShader"))
                    material.shader = Resources.Load<Shader>("shaders" + material.shader.name.Substring(13));
        }
    }
}
