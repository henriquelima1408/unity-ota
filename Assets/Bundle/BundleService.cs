using System;
using OTA.BundleProvider;

namespace OTA
{
    public class BundleService : IBundleService
    {
        readonly IBundleProvider bundleProvider;

        public BundleService(IBundleProvider bundleProvider)
        {
            this.bundleProvider = bundleProvider;
        }

        public bool IsBundleLoaded(string bundleName)
        {
            return bundleProvider.Bundles[bundleName].IsLoaded;
        }

        public string[] GetAssetNames(string bundleName)
        {
            return bundleProvider.Bundles[bundleName].GetAssetNames();
        }

        public void LoadAssetAsync<T>(string bundleName, string assetName, Action<T> onAssetLoaded) where T : UnityEngine.Object
        {
            var bundle = bundleProvider.Bundles[bundleName];

            LoadBundle(bundleName, (bn) =>
            {
#if UNITY_EDITOR
                onAssetLoaded?.Invoke(BundleProvider.Editor.EditorBundleProvider.GetAsset<T>(bundleName, assetName));
#else
onAssetLoaded?.Invoke(bundle.AssetBundle.LoadAsset<T>(assetName));
#endif
            });
        }

        public void LoadBundle(string bundleName, Action<string> onBundleReady)
        {
            var bundle = bundleProvider.Bundles[bundleName];

            if (bundle.IsRemote)
            {
                if (bundle.IsLoaded)
                {
                    onBundleReady?.Invoke(bundle.BundleName);
                }
                else
                {
                    bundleProvider.RequestBundle(bundle.BundleName, (bundleName) =>
                    {
                        if (!bundle.IsLoaded)
                            bundle.LoadBundle();

                        onBundleReady?.Invoke(bundle.BundleName);
                    });
                }
            }
            else
            {
                if (!bundle.IsLoaded)
                    bundle.LoadBundle();

                onBundleReady?.Invoke(bundle.BundleName);
            }
        }

    }
}
