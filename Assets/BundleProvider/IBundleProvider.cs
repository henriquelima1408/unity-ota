using System;
using System.Collections.Generic;

namespace OTA.BundleProvider
{
    public interface IBundleProvider
    {
        void RequestBundle(string bundleName, Action<string> onBundleReady);
        void RequestManifest(Action<BundleManifest> onManifestReady);
        IReadOnlyDictionary<string, BundleData> Bundles { get; }
    }
}