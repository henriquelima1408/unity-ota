#if UNITY_EDITOR
using Newtonsoft.Json;
using OTA.BundleProvider;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OTA.BundleProvider.Editor
{
    public class EditorBundleProvider : IBundleProvider
    {
        const string fileName = "BundleManifest.txt";
        const string fileFolderName = "BundleData";

        readonly Dictionary<string, BundleData> bundles = new Dictionary<string, BundleData>();
        public IReadOnlyDictionary<string, BundleData> Bundles => throw new NotImplementedException();

        public static T GetAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            var assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);

            foreach (var assetPath in assetPaths)
            {
                var name = Path.GetFileNameWithoutExtension(assetPath);
                if (name == assetName)
                {
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                }
            }

            Debug.LogError($"Could not find asset: {assetName} in bundle: {bundleName}");

            return null;
        }

        public BundleManifest LoadManifestFromLocalCache()
        {
            var bundleManifestPath = Path.Combine(Application.streamingAssetsPath, fileFolderName, $"{fileName}");

            Debug.Assert(File.Exists(bundleManifestPath), $"Bundle manifest doesnt exist in path: {bundleManifestPath}");

            BundleManifest bundleManifest = null;
            using (StreamReader streamReader = new StreamReader(bundleManifestPath))
            {
                var manifestJson = streamReader.ReadToEnd();
                bundleManifest = BundleManifest.FromJson(manifestJson);
            }

            foreach (var bundleMetadata in bundleManifest.BundleMetadatas)
            {
                bundles.Add(bundleMetadata.BundleName, new BundleData(bundleMetadata));
            }

            return bundleManifest;
        }

        public void RequestBundle(string bundleName, Action<string> onBundleReady)
        {
            onBundleReady?.Invoke(bundleName);
        }

        public void RequestManifest(Action<BundleManifest> onManifestReady)
        {
            onManifestReady?.Invoke(LoadManifestFromLocalCache());
        }
    }
}
#endif