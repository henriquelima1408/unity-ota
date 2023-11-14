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
            DeleteDirectoryContents(Application.streamingAssetsPath);

            var manifest = BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
            var bundleManifestPath = Path.Combine(Application.streamingAssetsPath, fileFolderName, $"{fileName}");

            BundleManifest bundleManifest;
            var bundlesMetadataHash = new HashSet<BundleMetadata>();

            foreach (var assetBundleName in manifest.GetAllAssetBundles())
            {
                var dependencies = AssetDatabase.GetAssetBundleDependencies(assetBundleName, true);
                var assetNames = ConvertPathsToAssetName(AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName));
                var assetBundlePath = Path.Combine(Application.streamingAssetsPath, assetBundleName);
                var file = File.ReadAllBytes(assetBundlePath);
                var bundleMetadata = new BundleMetadata(assetBundleName, dependencies, assetNames, GetAssetVersion(assetBundleName), ByteToSize(file), IsAssetRemote(assetBundleName));

                bundlesMetadataHash.Add(bundleMetadata);
                Debug.Log(bundleMetadata.ToString());
            }

            //TODO: Get the previous version automatically and increment it during build
            bundleManifest = new BundleManifest(Application.version, bundlesMetadataHash.ToArray());

            Directory.CreateDirectory(Path.GetDirectoryName(bundleManifestPath));
            using (StreamWriter streamWriter = new StreamWriter(bundleManifestPath))
            {
                streamWriter.Write(JsonConvert.SerializeObject(bundleManifest));
            }
        }

        static int GetAssetVersion(string assetBundleName)
        {
            return 1;
        }

        static bool IsAssetRemote(string assetBundleName)
        {
            return false;
        }

        static void DeleteDirectoryContents(string directoryPath)
        {
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                File.Delete(file);
            }

            foreach (var subdirectory in Directory.GetDirectories(directoryPath))
            {
                DeleteDirectory(subdirectory);
            }

            static void DeleteDirectory(string targetDirectory)
            {
                DeleteDirectoryContents(targetDirectory);
                Directory.Delete(targetDirectory);
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
