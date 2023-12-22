using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using OTA.Downloader;
using OTA.Coroutine;

namespace OTA.BundleProvider
{
    public class BundleProvider : IBundleProvider
    {
        const string environment = "stag";

        readonly string appVersion;
        readonly string bundleManifestLocalFilePath;
        readonly ICoroutine coroutine;
        readonly IDownloader downloader;

        BundleManifest bundleManifest;
        Dictionary<string, BundleData> bundles = new Dictionary<string, BundleData>();

        public IReadOnlyDictionary<string, BundleData> Bundles => bundles;

        public BundleProvider(ICoroutine coroutine, IDownloader downloader, Action onManifestReady)
        {
            this.downloader = downloader;
            this.coroutine = coroutine;

            appVersion = Application.version;
            bundleManifestLocalFilePath = Path.Combine(Application.streamingAssetsPath, "BundleData", "BundleManifest.txt");

            coroutine.RunCoroutine(LoadManifestFromLocalCache(), onManifestReady);
        }

        IEnumerator LoadManifestFromLocalCache()
        {
            var unityWebRequest = UnityWebRequest.Get(bundleManifestLocalFilePath);
            yield return unityWebRequest.SendWebRequest();

            var fileBytes = unityWebRequest.downloadHandler.data;
            var fileContent = System.Text.Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length);

            UpdateManifest(BundleManifest.FromJson(fileContent));
        }

        IEnumerator LoadBundleFromLocalCache(string requestPath, Action<bool> onAssetBundleLoaded)
        {
            var unityWebRequest = UnityWebRequest.Get(requestPath);
            yield return unityWebRequest.SendWebRequest();

            var data = unityWebRequest.downloadHandler.data;
            var fileExist = data != null && data.Length > 0;

            onAssetBundleLoaded?.Invoke(fileExist);
        }

        public void RequestBundle(string bundleName, Action<string> onBundleReady)
        {
            var requestPath = Path.Combine(BundleData.BundlesFolderLocalPath, bundleName);

            coroutine.RunCoroutine(LoadBundleFromLocalCache(requestPath, (fileExist) =>
            {
                if (fileExist)
                {
                    onBundleReady?.Invoke(bundleName);
                }
                else
                {
                    Debug.LogError($"Could not load bundle {bundleName} from local cache. Requesting it from server...");
                    var fileVersion = bundles[bundleName].BundleMetadata.Version.ToString();
                    var filePath = Path.Combine(environment, "Bundles", bundleName, fileVersion);
                    downloader.DownloadBundle(filePath, onBundleReady);
                }
            }), null);
        }

        public void RequestManifest(Action<BundleManifest> onManifestReady)
        {
            var fileVersion = appVersion;
            var filePath = Path.Combine(environment, "Manifests", fileVersion);
            downloader.DownloadManifest(filePath, (content) =>
            {
                onManifestReady?.Invoke(BundleManifest.FromJson(content));
            });
        }

        void UpdateManifest(BundleManifest bundleManifest)
        {
            var newBundles = new Dictionary<string, BundleData>();
            foreach (var bundleMetadata in bundleManifest.BundleMetadatas)
            {
                newBundles.Add(bundleMetadata.BundleName, new BundleData(bundleMetadata));
            }

            if (this.bundleManifest != null)
            {
                var unloadAllBundles = this.bundleManifest.Version < bundleManifest.Version;
                //If there is another manifest loaded, unload and delete bundles delta
                if (unloadAllBundles)
                {
                    foreach (var bundle in bundles.Values)
                    {
                        var requestPath = Path.Combine(BundleData.BundlesFolderLocalPath, bundle.BundleName);
                        if (bundle.IsLoaded)
                        {
                            bundle.UnloadBundle();
                        }

                        // if bundle version is different or it doesn´t exist in the new bundleManifest, delete it
                        if (newBundles.ContainsKey(bundle.BundleName))
                        {
                            if (bundle.BundleMetadata.Version != newBundles[bundle.BundleName].BundleMetadata.Version)
                            {
                                UnityWebRequest.Delete(requestPath);
                            }
                        }
                        else
                        {
                            UnityWebRequest.Delete(requestPath);
                        }
                    }
                }
            }

            bundles = newBundles;
            this.bundleManifest = bundleManifest;
        }

        IEnumerable<string> GetBundleDependecies(string bundleName, bool recursive)
        {
            var bundle = bundles[bundleName];
            var dependenciesHash = new HashSet<string>();
            var dependecies = new HashSet<string>();

            foreach (var dependecy in bundle.GetDependencies())
            {
                dependecies.Add(dependecy);
            }

            if (recursive && dependecies.Count > 0)
            {
                while (dependecies.Count > 0)
                {
                    var dependecy = dependecies.First();

                    if (!dependenciesHash.Contains(dependecy))
                    {
                        var dBundle = bundles[dependecies.First()];

                        foreach (var d in dBundle.GetDependencies())
                        {
                            dependecies.Add(d);
                        }
                    }

                    dependecies.Remove(dependecy);
                }

                return dependenciesHash;
            }
            else
            {
                return dependecies;
            }
        }
    }
}