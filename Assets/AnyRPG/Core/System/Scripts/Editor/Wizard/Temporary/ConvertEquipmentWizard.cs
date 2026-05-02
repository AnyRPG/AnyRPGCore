using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertEquipmentWizard : ScriptableWizard {

        public List<Equipment> equipmentList = new List<Equipment>();

        //[MenuItem("Tools/AnyRPG/Wizard/Convert/Convert equipment for dropped items")]
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
                if (equipment == null) {
                    continue;
                }
                if (equipment.ItemPickupPrefabProfileName != string.Empty) {
                    continue;
                }
                UpdateEquipment(equipment);

                EditorUtility.SetDirty(equipment);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert Equipment Wizard", "Conversion Complete!", "OK");

        }

        private void UpdateEquipment(Equipment equipment) {
            // search inline equipment models for the first model with a valid model prefab and assign it to the dropped item pickup model prefab
            if (equipment.InlineEquipmentModels != null) {
                foreach (EquipmentModel equipmentModel in equipment.InlineEquipmentModels.EquipmentModels) {
                    if (equipmentModel is PrefabEquipmentModel) {
                        PrefabEquipmentModel prefabEquipmentModel = equipmentModel as PrefabEquipmentModel;
                        if (prefabEquipmentModel.Properties.HoldableObjectList != null && prefabEquipmentModel.Properties.HoldableObjectList.Count > 0) {
                            foreach (HoldableObjectAttachment attachment in prefabEquipmentModel.Properties.HoldableObjectList) {
                                if (attachment.AttachmentNodes != null && attachment.AttachmentNodes.Count > 0) {
                                    foreach (AttachmentNode node in attachment.AttachmentNodes) {
                                        if (node.HoldableObjectName != string.Empty) {
                                            equipment.ItemPickupPrefabProfileName = node.HoldableObjectName;
                                            Debug.Log($"Updated equipment {equipment.ResourceName} with item pickup prefab profile {equipment.ItemPickupPrefabProfileName}");
                                            return;
                                        }
                                    }
                                }
                            }
                        }

                    }

                }
            }
        }

        void OnWizardUpdate() {
            helpString = "Converts equipment to have dropped items if the equipment has a prefab model associated with it";

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
