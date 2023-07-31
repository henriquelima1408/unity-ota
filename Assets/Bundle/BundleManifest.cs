﻿using Newtonsoft.Json;
using System;
using System.IO;

namespace OTA
{
    [Serializable]
    public class BundleManifest
    {
        BundleMetadata[] bundleMetadatas;
        readonly double version;

        public BundleManifest(string version, BundleMetadata[] bundleMetadatas)
        {
            this.bundleMetadatas = bundleMetadatas;
        }

        public BundleMetadata[] BundleMetadatas { get => bundleMetadatas; }

        public double Version => version;

        public static BundleManifest FromJson(string json)
        {
            return JsonConvert.DeserializeObject<BundleManifest>(json);
        }

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
