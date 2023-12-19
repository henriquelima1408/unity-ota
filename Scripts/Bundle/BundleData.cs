using System;
using System.IO;
using UnityEngine;

namespace OTA
{
    [Serializable]
    public class BundleData
    {
        public static string BundlesFolderLocalPath = Path.Combine(Application.streamingAssetsPath, "Bundles");

        readonly string bundleName;
        readonly bool isRemote;
        readonly BundleMetadata bundleMetadata;

        AssetBundle assetBundle;

        public string BundleName { get => bundleName; }
        public bool IsRemote { get => isRemote; }
        public bool IsLoaded
        {
            get
            {

#if UNITY_EDITOR
                return true;
#else
                    return assetBundle != null;
#endif
            }
        }
        public AssetBundle AssetBundle { get => assetBundle; }

        public BundleMetadata BundleMetadata => bundleMetadata;

        public BundleData(BundleMetadata bundleMetadata)
        {
            this.bundleMetadata = bundleMetadata;
            this.bundleName = bundleMetadata.BundleName;
            this.isRemote = bundleMetadata.IsRemote;
        }

        public void LoadBundle()
        {
            if (!IsLoaded)
                assetBundle = AssetBundle.LoadFromFile(Path.Combine(BundlesFolderLocalPath, bundleName));
        }

        public void UnloadBundle()
        {
            if (IsLoaded)
                assetBundle = null;
        }

        public string[] GetDependencies()
        {
            return bundleMetadata.Dependecies;
        }

        public string[] GetAssetNames()
        {
            return bundleMetadata.Assets;
        }
    }
}
