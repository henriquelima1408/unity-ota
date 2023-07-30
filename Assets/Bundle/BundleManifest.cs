using System;

namespace App.Game.Services
{
    [Serializable]
    public class BundleManifest
    {
        BundleMetadata[] bundleMetadatas;

        public BundleManifest(BundleMetadata[] bundleMetadatas)
        {
            this.bundleMetadatas = bundleMetadatas;
        }

        public BundleMetadata[] BundleMetadatas { get => bundleMetadatas; }
    }

    [Serializable]
    public class BundleMetadata
    {
        readonly string bundleName;
        readonly string[] dependecies;
        readonly string[] assets;
        readonly double version;
        readonly double size;
        readonly bool isRemote;

        public BundleMetadata(string bundleName, string[] dependecies, string[] assets, double version, double size, bool isRemote)
        {
            this.bundleName = bundleName;
            this.dependecies = dependecies;
            this.assets = assets;
            this.version = version;
            this.size = size;
            this.isRemote = isRemote;
        }

        public string BundleName => bundleName;

        public string[] Dependecies => dependecies;

        public string[] Assets => assets;

        public double Version => version;

        public double Size => size;

        public bool IsRemote => isRemote;
    }
}
