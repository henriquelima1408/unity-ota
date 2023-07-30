using System;

namespace App.Game.Services
{
    public interface IBundleService
    {
        void LoadAssetAsync<T>(string bundleName, string assetName, Action<T> onAssetLoaded) where T : UnityEngine.Object;
        void LoadBundle(string bundleName, Action<string> onBundleLoaded);
        bool IsBundleLoaded(string bundleName);
        string[] GetAssetNames(string bundleName);
    }
}
