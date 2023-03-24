using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertEquipmentWizard : ScriptableWizard {

        public List<Equipment> equipmentList = new List<Equipment>();

        [MenuItem("Tools/AnyRPG/Wizard/Convert/Convert Equipment to 0.16")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ConvertEquipmentWizard>("New Convert Equipment Wizard", "Convert");
        }

        void OnEnable() {
            /*
            systemConfigurationManager = GameObject.FindObjectOfType<SystemConfigurationManager>();
            if (systemConfigurationManager != null) {
                gameName = systemConfigurationManager.GameName;
            }
            */
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("Convert Equipment Wizard", "Beginning Conversion...", 0f);

            int i = 0;
            foreach (Equipment equipment in equipmentList) {
                i++;
                EditorUtility.DisplayProgressBar("Convert Equipment Wizard", "Beginning Conversion...", (float)i / (float)equipmentList.Count);

                // copy uma recipe profile
                equipment.SharedEquipmentModels = equipment.DeprecatedUmaRecipeProfileName;

                // copy uma recipe profile properties
                if (equipment.DeprecatedUMARecipeProfileProperties.UMARecipes.Count > 0 || equipment.DeprecatedUMARecipeProfileProperties.SharedColors.Count > 0) {
                    UMAEquipmentModel umaEquipmentModel = new UMAEquipmentModel();
                    umaEquipmentModel.Properties = equipment.DeprecatedUMARecipeProfileProperties;
                    equipment.InlineEquipmentModels.EquipmentModels.Add(umaEquipmentModel);
                }

                // copy prefab equipment models
                if (equipment.DeprecatedHoldableObjectList.Count > 0) {
                    PrefabEquipmentModel prefabEquipmentModel = new PrefabEquipmentModel();
                    prefabEquipmentModel.Properties.HoldableObjectList = equipment.DeprecatedHoldableObjectList;
                    equipment.InlineEquipmentModels.EquipmentModels.Add(prefabEquipmentModel);
                }

                EditorUtility.SetDirty(equipment);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert Equipment Wizard", "Conversion Complete!", "OK");

        }

        void OnWizardUpdate() {
            helpString = "Converts older equipment to 0.16 compatible";

            errorString = Validate();
            isValid = (errorString == null || errorString == "");
        }

        string Validate() {

            /*
            // check for empty game name
            if (gameName == null || gameName.Trim() == "") {
                return "Game name must not be empty";
            }
            */

            return null;
        }

        private void ShowError(string message) {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }
       
    }

}
