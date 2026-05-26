using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class CopyItemPickupPropertyWizard : ScriptableWizard {

        public List<Item> sourceItems = new List<Item>();
        public List<Item> targetItems = new List<Item>();

        private Dictionary<string, Item> sourceItemDictionary = new Dictionary<string, Item>();
        private Dictionary<string, Item> targetItemDictionary = new Dictionary<string, Item>();

        //[MenuItem("Tools/AnyRPG/Wizard/Convert/Copy Item Pickup Property")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<CopyItemPickupPropertyWizard>("New Copy Item Pickup Property Wizard", "Populate");
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

            EditorUtility.DisplayProgressBar("Copy Item Pickup Property Wizard", "Beginning Copy...", 0f);

            // populate lookup dictionaries
            foreach (Item item in sourceItems) {
                if (item != null && !sourceItemDictionary.ContainsKey(item.ResourceName)) {
                    sourceItemDictionary.Add(item.ResourceName, item);
                }
            }
            foreach (Item item in targetItems) {
                if (item != null && !targetItemDictionary.ContainsKey(item.ResourceName)) {
                    targetItemDictionary.Add(item.ResourceName, item);
                }
            }

            int i = 0;
            foreach (Item item in targetItems) {
                i++;
                EditorUtility.DisplayProgressBar("Copy Item Pickup Property Wizard", "Beginning Copy...", (float)i / (float)targetItems.Count);
                if (item == null) {
                    continue;
                }
                if (item.ItemPickupPrefabProfileName != string.Empty) {
                    continue;
                }
                if (sourceItemDictionary.TryGetValue(item.ResourceName, out Item sourceItem)) {
                    item.ItemPickupPrefabProfileName = sourceItem.ItemPickupPrefabProfileName;
                    Debug.Log($"Updated item {item.ResourceName} with item pickup prefab profile {item.ItemPickupPrefabProfileName}");
                }

                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert Equipment Wizard", "Conversion Complete!", "OK");

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
