using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class UMANewGameWizard : NewGameWizardBase {

        private const string pathToUMAGLIBPrefab = "/UMA/Getting Started/UMA_GLIB.prefab";
        private const string pathToPlayerUnitsTemplate = "/AnyRPG/UMA/Content/TemplatePackages/UnitProfile/Player/UMAHumanPlayerUnitsTemplatePackage.asset";

        public override string PathToPlayerUnitsTemplate { get => pathToPlayerUnitsTemplate; }

        [MenuItem("Tools/AnyRPG/Wizard/UMA/New Game Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<UMANewGameWizard>("New Game Wizard", "Create");
        }

        protected override bool CheckFilesExist() {

            // check for presence of the UMA GLIB prefab
            if (WizardUtilities.CheckFileExists(pathToUMAGLIBPrefab, "UMA GLIB Prefab") == false) {
                return false;
            }

            return base.CheckFilesExist();
        }

        protected override void MakeOptionalContent(string fileSystemGameName, string prefabPath) {
            base.MakeOptionalContent(fileSystemGameName, prefabPath);

            // create a variant of the UMA GLIB prefab
            MakeUMAPrefabVariant(prefabPath + "/GameManager/UMA_GLIB.prefab", fileSystemGameName);
        }

        private GameObject MakeUMAPrefabVariant(string newPath, string gameName) {

            GameObject umaGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToUMAGLIBPrefab);

            // instantiate original
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(umaGameObject);
            instantiatedGO.name = gameName + instantiatedGO.name;

            // make variant on disk
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instantiatedGO, newPath);

            // remove original from scene
            GameObject.DestroyImmediate(instantiatedGO);

            // instantiate new variant in scene
            //PrefabUtility.InstantiatePrefab(variant);
            GameObject sceneVariant = (GameObject)PrefabUtility.InstantiatePrefab(variant);
            sceneVariant.name = umaGameObject.name;

            return variant;
        }

        protected override void SetDefaultPlayerUnitProfileName(SystemConfigurationManager systemConfigurationManager) {
            base.SetDefaultPlayerUnitProfileName(systemConfigurationManager);

            systemConfigurationManager.DefaultPlayerUnitProfileName = "UMA Human Male";
        }

    }

}
