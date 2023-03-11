using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class SwappableMeshModelController : ModelAppearanceController {

        protected SwappableMeshModelOptions modelOptions = null;

        private Dictionary<string, SwappableMeshOptionGroup> optionGroups = new Dictionary<string, SwappableMeshOptionGroup>();
        
        // list of members of each group for the purpose of disabling members that are not currently chosen
        // after adding hide slot option, this is not necessary, since everything that is not chosen is disabled on the whole model, not at the group level
        //private Dictionary<string, Dictionary<string, string>> optionGroupMembers = new Dictionary<string, Dictionary<string, string>>();
        
        // defaults to set when nothing is chosen
        private Dictionary<string, string> optionGroupDefaults = new Dictionary<string, string>();

        // user choices
        private Dictionary<string, string> optionGroupChoices = new Dictionary<string, string>();

        // applied configuration takes group hiding by other groups into account
        private Dictionary<string, string> optionGroupAppliedConfiguration = new Dictionary<string, string>();


        public SwappableMeshModelController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager, SwappableMeshModelOptions modelOptions)
            : base(unitController, unitModelController, systemGameManager) {
            this.modelOptions = modelOptions;

            // populate the default dictionary
            foreach (SwappableMeshOptionDefaults optionDefault in ModelOptions.GroupDefaults) {
                if (optionGroupDefaults.ContainsKey(optionDefault.GroupName)) {
                    Debug.LogWarning(unitController.gameObject.name + ".SwappableMeshModelController(): Key '" + optionDefault.GroupName + "' already exists.  Ensure group names are unique in group defaults.");
                    continue;
                }
                optionGroupDefaults.Add(optionDefault.GroupName, optionDefault.MeshName);
            }

            // populate the option choice group dictionaries
            foreach (SwappableMeshOptionGroup modelGroup in ModelOptions.MeshGroups) {
                // option group dictionary
                optionGroups.Add(modelGroup.GroupName, modelGroup);

                // option group members dictionary
                /*
                Dictionary<string, string> groupMembers = new Dictionary<string, string>();
                foreach (SwappableMeshOptionChoice optionChoice in modelGroup.Meshes) {
                    groupMembers.Add(optionChoice.DisplayName, optionChoice.MeshName);
                }
                optionGroupMembers.Add(modelGroup.GroupName, groupMembers);
                */

                // set default choices
                if (optionGroupDefaults.ContainsKey(modelGroup.GroupName)) {
                    optionGroupChoices.Add(modelGroup.GroupName, optionGroupDefaults[modelGroup.GroupName]);
                } else {
                    optionGroupChoices.Add(modelGroup.GroupName, "");
                }
            }

            //SetupAppliedConfiguration();

            //ApplyConfiguration();
        }

        public SwappableMeshModelOptions ModelOptions { get => modelOptions; }
        public Dictionary<string, string> OptionGroupChoices { get => optionGroupChoices; }

        public override T GetModelAppearanceController<T>() {
            return this as T;
        }

        public override void SaveAppearanceSettings(AnyRPGSaveData saveData) {

            saveData.swappableMeshSaveData.Clear();
            foreach (string groupName in optionGroupChoices.Keys) {
                SwappableMeshSaveData swappableMeshSaveData = new SwappableMeshSaveData();
                swappableMeshSaveData.groupName = groupName;
                swappableMeshSaveData.meshName = optionGroupChoices[groupName];
                saveData.swappableMeshSaveData.Add(swappableMeshSaveData);
            }
        }

        private void SetupAppliedConfiguration() {
            //Debug.Log(unitController.gameObject.name + ".SwappableMeshModelController.SetupAppliedConfiguration()");

            // reset applied configuration and set it to the raw choices
            optionGroupAppliedConfiguration.Clear();
            foreach (string groupName in optionGroupChoices.Keys) {
                optionGroupAppliedConfiguration[groupName] = optionGroupChoices[groupName];
            }

            // disable any groups that should be hidden
            foreach (string groupName in optionGroupChoices.Keys) {
                if (optionGroupChoices[groupName] != "" && optionGroupAppliedConfiguration.ContainsKey(optionGroups[groupName].HidesGroup)) {
                    optionGroupAppliedConfiguration[optionGroups[groupName].HidesGroup] = "";
                }
            }

        }

        private bool LoadGroupChoice(string groupName, string optionChoice) {
            //Debug.Log(unitController.gameObject.name + ".SwappableMeshModelController.LoadGroupChoice(" + groupName + ", " + optionChoice + ")");

            if (optionGroups.ContainsKey(groupName) == false) {
                // option group did not exist
                return false;
            }

            if (unitModelController.UnitModel == null) {
                // could not find the model to search
                return false;
            }

            if (optionChoice == "" && optionGroupDefaults.ContainsKey(groupName) == true) {
                optionGroupChoices[groupName] = optionGroupDefaults[groupName];
            } else {
                optionGroupChoices[groupName] = optionChoice;
            }

            return true;
        }

        public void SetGroupChoice(string groupName, string optionChoice) {
            //Debug.Log(unitController.gameObject.name + ".SwappableMeshModelController.SetGroupChoice(" + groupName + ", " + optionChoice + ")");

            if (LoadGroupChoice(groupName, optionChoice) == false) {
                return;
            }

            SetupAppliedConfiguration();

            ApplyConfiguration();

            // enable correct mesh
            /*
            foreach (Transform child in unitModelController.UnitModel.transform) {
                if (child.name == optionChoice) {
                    // enable chosen mesh
                    child.gameObject.SetActive(true);
                } else if (optionGroupMembers[groupName].Contains(child.name)) {
                    // disable mesh that is in group but not chosen
                    child.gameObject.SetActive(false);
                }
            }
            */

        }

        private void ApplyConfiguration() {
            //Debug.Log(unitController.gameObject.name + ".SwappableMeshModelController.ApplyConfiguration()");

            // get a list of meshes to enable
            List<string> enabledMeshes = new List<string>();
            foreach (string groupName in optionGroupAppliedConfiguration.Keys) {
                //Debug.Log(groupName + " " + optionGroupAppliedConfiguration[groupName]);
                if (optionGroupAppliedConfiguration[groupName] != "") {
                    // extract the actual mesh name from the optionGroupMembers dictionary based on the chosen option for the group
                    //enabledMeshes.Add(optionGroupMembers[groupName][optionGroupAppliedConfiguration[groupName]]);
                    enabledMeshes.Add(optionGroupAppliedConfiguration[groupName]);
                }
            }

            // enable all chosen meshes and disable all others
            foreach (Transform child in unitModelController.UnitModel.transform) {
                if (enabledMeshes.Contains(child.name)) {
                    // enable chosen mesh
                    child.gameObject.SetActive(true);
                } else {
                    // disable meshes that were not chosen
                    child.gameObject.SetActive(false);
                }
            }

        }

        public override void SetInitialSavedAppearance(AnyRPGSaveData saveData) {
            //Debug.Log(unitController.gameObject.name + ".SwappableMeshModelController.SetInitialSavedAppearance()");

            foreach (SwappableMeshSaveData swappableMeshSaveData in saveData.swappableMeshSaveData) {
                LoadGroupChoice(swappableMeshSaveData.groupName, swappableMeshSaveData.meshName);
            }

            //SetupAppliedConfiguration();

            //ApplyConfiguration();
        }

        public override void BuildModelAppearance() {
            // nothing to do here for now
        }

        public override bool IsBuilding() {
            return false;
        }

        public override void ResetSettings() {
            // nothing to do here for now
        }

        public override void EquipItemModels(EquipmentSlotProfile equipmentSlotProfile, Equipment equipment) {
            base.EquipItemModels(equipmentSlotProfile, equipment);
        }

        protected override void UnequipItemModels(EquipmentSlotProfile equipmentSlot) {
            base.UnequipItemModels(equipmentSlot);
        }

        public override void DespawnModel() {
            // nothing to do here for now
        }

        public override void ConfigureUnitModel() {
            //Debug.Log(unitController.gameObject.name + ".SwappableMeshModelController.ConfigureUnitModel()");

            if (unitModelController.UnitModel == null) {
                return;
            }

            SetupAppliedConfiguration();

            ApplyConfiguration();

            unitModelController.SetModelReady();
        }

        public override bool KeepMonoBehaviorEnabled(MonoBehaviour monoBehaviour) {
            return false;
        }

        public override bool ShouldCalculateFloatHeight() {
            // modelReady is only false on first spawn, so this will only run once
            if (unitModelController.ModelCreated == false) {
                return true;
            }
            return false;
        }

    }

}