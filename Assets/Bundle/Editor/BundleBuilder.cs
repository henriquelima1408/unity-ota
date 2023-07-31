#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace OTA.Editor
{
    public static class BundleBuilder
    {
        const string fileName = "BundleManifest.txt";
        const string fileFolderName = "BundleData";

        [MenuItem("Window/Build Bundles")]
        public static void BuildBundles()
        {
            var bundleManifestPath = Path.Combine(Application.streamingAssetsPath, fileFolderName, $"{fileName}");
            BundleManifest bundleManifest;
            var bundlesMetadataHash = new HashSet<BundleMetadata>();

            if (!File.Exists(bundleManifestPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(bundleManifestPath));
                File.CreateText(bundleManifestPath);
            }

            var assetBundleManifest = BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

            foreach (var assetBundleName in assetBundleManifest.GetAllAssetBundles())
            {
                var dependencies = AssetDatabase.GetAssetBundleDependencies(assetBundleName, true);
                var assetNames = ConvertPathsToAssetName(AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName));
                var assetBundlePath = Path.Combine(Application.streamingAssetsPath, assetBundleName);
                var file = File.ReadAllBytes(assetBundlePath);

                bundlesMetadataHash.Add(new BundleMetadata(assetBundleName, dependencies, assetNames, 1, ByteToSize(file), false));
                Debug.Log(ByteToSize(file));
            }

            //TODO: Get the previous version automatically and increment it during build
            bundleManifest = new BundleManifest(Application.version, bundlesMetadataHash.ToArray());

            using (StreamWriter streamWriter = new StreamWriter(bundleManifestPath))
            {
                streamWriter.Write(JsonConvert.SerializeObject(bundleManifest));
            }

        }

        static string[] ConvertPathsToAssetName(string[] assetPaths)
        {
            var result = new string[assetPaths.Length];

            for (int i = 0; i < assetPaths.Length; i++)
            {
                var splitElements = assetPaths[i].Split('/');
                var fileName = splitElements[splitElements.Length - 1];

                result[i] = Path.GetFileNameWithoutExtension(fileName);
            }

            return result;
        }

        static double ByteToSize(byte[] bytes)
        {
            var length = bytes.Length;
            long KB = 1024, MB = KB * 1024;
            double size = length;

            if (length >= MB)
            {
                size = Math.Round((double)length / MB, 2);
            }
            else if (length >= KB)
            {
                size = Math.Round((double)length / KB, 2) / 1000;
            }

            return size;
        }
    }
}
#endif
