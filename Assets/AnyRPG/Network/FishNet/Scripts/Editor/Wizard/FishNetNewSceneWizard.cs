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
    public class FishNetNewSceneWizard : NewSceneWizardBase, ICreateSceneRequestor {

        private const string pathToPhysicsSceneSync = "/AnyRPG/Network/FishNet/GameManager/FishNetPhysicsSceneSync.prefab";
        private const string portalTemplatePath = "/AnyRPG/Network/FishNet/Templates/Prefabs/Portal/FishNetStonePortal.prefab";

        public static string PortalTemplatePath => portalTemplatePath;

        [MenuItem("Tools/AnyRPG/Wizard/FishNet/New Scene Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<FishNetNewSceneWizard>("FishNet New Scene Wizard", "Create");
        }

        public override void ModifyScene() {
            base.ModifyScene();
            string sceneConfigPrefabAssetPath = "Assets" + pathToPhysicsSceneSync;
            GameObject sceneSyncGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(sceneConfigPrefabAssetPath);
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(sceneSyncGameObject);
            instantiatedGO.transform.SetAsFirstSibling();
        }

        public static bool CheckRequiredTemplatesExistStatic() {

            if (NewSceneWizardBase.CheckRequiredBaseTemplatesExist() == false) {
                return false;
            }

            // Check for presence of portal template
            if (WizardUtilities.CheckFileExists(portalTemplatePath, "portal template") == false) {
                return false;
            }

            return true;
        }

        public override string GetPortalTemplatePath() {
            return portalTemplatePath;
        }

    }


}
