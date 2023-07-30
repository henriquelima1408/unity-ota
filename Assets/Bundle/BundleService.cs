using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using App.System.Bundles.Editor;

namespace App.Game.Services
{
    public class BundleService : IBundleService
    {
        const string fileName = "BundleManifest.txt";
        const string fileFolderName = "BundleData";

        readonly string bundleManifestPath;

        BundleManifest bundleManifest;
        Dictionary<string, BundleData> bundles = new Dictionary<string, BundleData>();

        public BundleService()
        {
            //TODO: Request manifest for server and create the manifest with response

            bundleManifestPath = Path.Combine(Application.streamingAssetsPath, fileFolderName, $"{fileName}");

            Debug.Assert(File.Exists(bundleManifestPath), $"Bundle manifest doesnt exist in path: {bundleManifestPath}");

            using (StreamReader streamReader = new StreamReader(bundleManifestPath))
            {
                var manifestJson = streamReader.ReadToEnd();
                bundleManifest = JsonConvert.DeserializeObject<BundleManifest>(manifestJson);
            }

            foreach (var bundleMetadata in bundleManifest.BundleMetadatas)
            {
                bundles.Add(bundleMetadata.BundleName, new BundleData(bundleMetadata));
            }
        }

        public bool IsBundleLoaded(string bundleName)
        {
            return bundles[bundleName].IsLoaded;
        }

        public string[] GetAssetNames(string bundleName)
        {
            return bundles[bundleName].GetAssetNames();
        }

        public void Dispose()
        {
            foreach (var bundleName in bundles.Keys)
            {
                bundles[bundleName].UnloadBundle();
            }

            bundles = null;
            bundleManifest = null;
        }

        public void LoadAssetAsync<T>(string bundleName, string assetName, Action<T> onAssetLoaded) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            onAssetLoaded?.Invoke(BundleAssetProvider.GetAsset<T>(bundleName, assetName));
#endif


            var bundle = bundles[bundleName];

            LoadBundle(bundleName, (bn) =>
            {
                onAssetLoaded?.Invoke(bundle.AssetBundle.LoadAsset<T>(assetName));
            });
        }

        public void LoadBundle(string bundleName, Action<string> onBundleReady)
        {
            var bundle = bundles[bundleName];

            if (bundle.IsRemote)
            {
                if (bundle.IsLoaded)
                {
                    onBundleReady?.Invoke(bundle.BundleName);
                }
                else
                {
                    //TODO: Request bundle
                }
            }
            else
            {
                if (!bundle.IsLoaded)
                    bundle.LoadBundle();

                onBundleReady?.Invoke(bundle.BundleName);
            }
        }

        [Serializable]
        class BundleData
        {
            readonly string bundleName;
            readonly string bundlePath;
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

            public BundleData(BundleMetadata bundleMetadata)
            {
                this.bundleMetadata = bundleMetadata;
                this.bundleName = bundleMetadata.BundleName;
                this.isRemote = bundleMetadata.IsRemote;
                this.bundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);
            }

            public void LoadBundle()
            {
                if (!IsLoaded)
                    assetBundle = AssetBundle.LoadFromFile(bundlePath);
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
}
