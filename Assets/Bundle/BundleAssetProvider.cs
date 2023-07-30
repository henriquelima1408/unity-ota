using System.IO;
using UnityEngine;

namespace App.System.Bundles.Editor
{
    public static class BundleAssetProvider
    {
        public static T GetAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            var assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);

            foreach (var assetPath in assetPaths)
            {
                var name = Path.GetFileNameWithoutExtension(assetPath);
                if (name == assetName)
                {
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                }
            }
    
            Debug.LogError($"Could not find asset: {assetName} in bundle: {bundleName}");
#endif
            return null;
        }

    }
}

