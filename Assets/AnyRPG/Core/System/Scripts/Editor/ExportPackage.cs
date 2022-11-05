// ExportPackage.cs
using UnityEngine;
using UnityEditor;

namespace AnyRPG.Editor {

    public class ExportPackage {
        [MenuItem("Tools/AnyRPG/Export/FullExport")]
        static void export() {
            AssetDatabase.ExportPackage(AssetDatabase.GetAllAssetPaths(), PlayerSettings.productName + ".unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets);
        }
    }

}


