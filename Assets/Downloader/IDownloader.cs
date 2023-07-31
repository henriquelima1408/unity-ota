using System;

namespace OTA.Downloader
{
    public interface IDownloader
    {
        public void DownloadBundle(string bundlePath, Action<string> onBundleDownloaded);
        public void DownloadManifest(string manifestPath, Action<string> onManifestDownloaded);

    }

}