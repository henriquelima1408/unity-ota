using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;

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
        readonly string crc;
        readonly double version;
            
        readonly bool isRemote;

        public BundleMetadata(string bundleName, string[] dependecies, string[] assets, double version, string crc, bool isRemote)
        {
            this.bundleName = bundleName;
            this.dependecies = dependecies;
            this.assets = assets;
            this.crc = crc;
            this.version = version;
            this.isRemote = isRemote;
        }

        public string BundleName => bundleName;

        public string[] Dependecies => dependecies;

        public string[] Assets => assets;

        public double Version => version;

        public string CRC => crc;

        public bool IsRemote => isRemote;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"BundleName: {BundleName}\n");

            stringBuilder.Append($"Dependecy: ");
            foreach (var depency in Dependecies)
            {
                stringBuilder.Append($"{depency}\n");
            }

            stringBuilder.Append($"Assets: ");
            foreach (var depency in Assets)
            {
                stringBuilder.Append($"{depency}\n");
            }

            stringBuilder.Append($"Version: {version}\n");
            stringBuilder.Append($"CRC: {crc}\n");
            stringBuilder.Append($"IsRemote: {isRemote}\n");

            return stringBuilder.ToString();
        }
    }
}
