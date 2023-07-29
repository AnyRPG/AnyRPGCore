using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Gain Item Command", menuName = "AnyRPG/Chat Commands/Gain Item Command")]
    public class GainItemCommand : ChatCommand {

        [Header("Gain Item Command")]

        [Tooltip("If true, all parameters will be ignored, and the item provided will be the item listed below")]
        [SerializeField]
        private bool fixedItem = false;

        [Tooltip("Only applies if fixedItem is true")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        private string itemName = string.Empty;

        // game manager references
        SystemItemManager systemItemManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemItemManager = systemGameManager.SystemItemManager;
        }

        public override void ExecuteCommand(string commandParameters) {
            //Debug.Log("GainItemCommand.ExecuteCommand() Executing command " + DisplayName + " with parameters (" + commandParameters + ")");

            // add a fixed item
            if (fixedItem == true) {
                AddItem(itemName);
                return;
            }

            // the item comes from parameters, but none were given
            if (commandParameters == string.Empty) {
                return;
            }

            // add an item from parameters
            AddItem(commandParameters);
        }

        private void AddItem(string itemName) {
            Item tmpItem = systemItemManager.GetNewResource(itemName);
            if (tmpItem != null) {
                playerManager.UnitController.CharacterInventoryManager.AddItem(tmpItem, false);
            }
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (fixedItem == true && itemName != null && itemName != string.Empty) {
                Item tmpItem = systemDataFactory.GetResource<Item>(itemName);
                if (tmpItem == null) {
                    Debug.LogError("GainItemCommand.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}