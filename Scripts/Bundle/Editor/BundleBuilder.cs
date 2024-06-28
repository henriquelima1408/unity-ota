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

        [MenuItem("Window/Build Bundles")]
        public static void BuildBundles()
        {
            DeleteDirectoryContents(Application.streamingAssetsPath);

            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            var manifest = BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
            var bundleManifestPath = Path.Combine(Application.streamingAssetsPath, $"{fileName}");

            BundleManifest bundleManifest;
            var bundlesMetadataHash = new HashSet<BundleMetadata>();

            foreach (var assetBundleName in manifest.GetAllAssetBundles())
            {
                uint crc = 0;
                var dependencies = AssetDatabase.GetAssetBundleDependencies(assetBundleName, true);
                var assetNames = ConvertPathsToAssetName(AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName));

                var assetBundlePath = Path.Combine(Application.streamingAssetsPath, assetBundleName);                
                BuildPipeline.GetCRCForAssetBundle(assetBundlePath, out crc);
                
                var file = File.ReadAllBytes(assetBundlePath);
                var bundleMetadata = new BundleMetadata(assetBundleName, dependencies, assetNames, GetAssetVersion(assetBundleName), crc.ToString(), IsAssetRemote(assetBundleName));

                bundlesMetadataHash.Add(bundleMetadata);
            }

            //TODO: Get the previous version automatically and increment it during build
            bundleManifest = new BundleManifest(Application.version, bundlesMetadataHash.ToArray());

            using (StreamWriter streamWriter = new StreamWriter(bundleManifestPath))
            {
                streamWriter.Write(JsonPrettify(JsonConvert.SerializeObject(bundleManifest)));
            }
        }

        static string JsonPrettify(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
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
            if (Directory.Exists(directoryPath))
            {
                foreach (var file in Directory.GetFiles(directoryPath))
                {
                    File.Delete(file);
                }

                foreach (var subdirectory in Directory.GetDirectories(directoryPath))
                {
                    DeleteDirectory(subdirectory);
                }
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
