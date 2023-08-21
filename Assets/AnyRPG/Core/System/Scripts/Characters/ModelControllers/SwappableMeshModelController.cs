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
        
        // all options in a single dictionary for the purpose of actual mesh name lookup
        private Dictionary<string, Dictionary<string, string>> allModelOptions = new Dictionary<string, Dictionary<string, string>>();
        
        // defaults to set when nothing is chosen (groupName, optionName)
        private Dictionary<string, string> optionGroupDefaults = new Dictionary<string, string>();

        // user choices (groupName, optionName)
        private Dictionary<string, string> optionGroupChoices = new Dictionary<string, string>();

        // applied configuration takes group hiding by other groups into account (groupName, optionName)
        private Dictionary<string, string> optionGroupAppliedConfiguration = new Dictionary<string, string>();

        // track mesh renderers
        private List<GameObject> meshRenderers = new List<GameObject>();


        public SwappableMeshModelController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager, SwappableMeshModelOptions modelOptions)
            : base(unitController, unitModelController, systemGameManager) {
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController()");

            this.modelOptions = modelOptions;

            // populate the default dictionary
            foreach (SwappableMeshOptionDefaults optionDefault in ModelOptions.GroupDefaults) {
                if (optionGroupDefaults.ContainsKey(optionDefault.GroupName)) {
                    Debug.LogWarning($"{unitController.gameObject.name}.SwappableMeshModelController(): Key '{optionDefault.GroupName}' already exists.  Ensure group names are unique in group defaults.");
                    continue;
                }
                //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController(): adding {optionDefault.GroupName} as {optionDefault.OptionName}");
                optionGroupDefaults.Add(optionDefault.GroupName, optionDefault.OptionName);
            }

            // populate the option choice group dictionaries
            foreach (SwappableMeshOptionGroup modelGroup in ModelOptions.MeshGroups) {
                // option group dictionary
                optionGroups.Add(modelGroup.GroupName, modelGroup);

                // option group members dictionary
                Dictionary<string, string> groupMembers = new Dictionary<string, string>();
                foreach (SwappableMeshOptionChoice optionChoice in modelGroup.Meshes) {
                    groupMembers.Add(optionChoice.DisplayName, optionChoice.MeshName);
                }
                allModelOptions.Add(modelGroup.GroupName, groupMembers);

                // set default choices
                if (optionGroupDefaults.ContainsKey(modelGroup.GroupName)) {
                    optionGroupChoices.Add(modelGroup.GroupName, optionGroupDefaults[modelGroup.GroupName]);
                } else {
                    optionGroupChoices.Add(modelGroup.GroupName, "");
                }
            }

        }

        public SwappableMeshModelOptions ModelOptions { get => modelOptions; }
        public Dictionary<string, string> OptionGroupChoices { get => optionGroupChoices; }

        public override T GetModelAppearanceController<T>() {
            return this as T;
        }

        public override void SaveAppearanceSettings(/*ISaveDataOwner saveDataOwner,*/ AnyRPGSaveData saveData) {
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.SaveAppearanceSettings()");

            saveData.swappableMeshSaveData.Clear();
            foreach (string groupName in optionGroupChoices.Keys) {
                SwappableMeshSaveData swappableMeshSaveData = new SwappableMeshSaveData();
                swappableMeshSaveData.groupName = groupName;
                swappableMeshSaveData.optionName = optionGroupChoices[groupName];
                saveData.swappableMeshSaveData.Add(swappableMeshSaveData);
            }
            //saveDataOwner.SetSaveData(saveData);
        }

        private void SetupAppliedConfiguration() {
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.SetupAppliedConfiguration()");

            // reset applied configuration and set it to the raw choices
            optionGroupAppliedConfiguration.Clear();
            foreach (string groupName in optionGroupChoices.Keys) {
                //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.SetupAppliedConfiguration() setting {groupName} to {optionGroupChoices[groupName]}");
                optionGroupAppliedConfiguration[groupName] = optionGroupChoices[groupName];
            }

            // disable any groups that should be hidden
            foreach (string groupName in optionGroupChoices.Keys) {
                if (optionGroupChoices[groupName] != "" && optionGroupAppliedConfiguration.ContainsKey(optionGroups[groupName].HidesGroup)) {
                    //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.SetupAppliedConfiguration() hiding {optionGroups[groupName].HidesGroup}");
                    optionGroupAppliedConfiguration[optionGroups[groupName].HidesGroup] = "";
                }
            }

        }

        private bool LoadGroupChoice(string groupName, string optionChoice) {
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.LoadGroupChoice({groupName}, {optionChoice})");

            if (optionGroups.ContainsKey(groupName) == false) {
                Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.LoadGroupChoice({groupName}, {optionChoice}) option group did not exist");
                return false;
            }

            if (unitModelController.UnitModel == null) {
                Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.LoadGroupChoice({groupName}, {optionChoice}) could not find the model to search");
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
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.SetGroupChoice({groupName}, {optionChoice})");

            if (LoadGroupChoice(groupName, optionChoice) == false) {
                return;
            }
        }

        private void ApplyConfiguration() {
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.ApplyConfiguration()");

            // get a list of meshes to enable
            List<string> enabledMeshes = new List<string>();
            foreach (string groupName in optionGroupAppliedConfiguration.Keys) {
                //Debug.Log(groupName + " " + optionGroupAppliedConfiguration[groupName]);
                if (optionGroupAppliedConfiguration[groupName] != "") {
                    // extract the actual mesh name from the allModelOptions dictionary based on the chosen option for the group
                    //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.ApplyConfiguration() adding {optionGroupAppliedConfiguration[groupName]}");
                    if (allModelOptions.ContainsKey(groupName) == true && allModelOptions[groupName].ContainsKey(optionGroupAppliedConfiguration[groupName]) == true) {
                        enabledMeshes.Add(allModelOptions[groupName][optionGroupAppliedConfiguration[groupName]]);
                    }
                }
            }

            // enable all chosen meshes and disable all others
            //foreach (Transform child in unitModelController.UnitModel.transform) {
            foreach (GameObject go in meshRenderers) {
                if (enabledMeshes.Contains(go.name)) {
                    // enable chosen mesh
                    go.SetActive(true);
                } else {
                    // disable meshes that were not chosen
                    go.SetActive(false);
                }
            }

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

        public override void SetInitialSavedAppearance(CharacterAppearanceData characterAppearanceData) {
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.SetInitialSavedAppearance()");

            foreach (SwappableMeshSaveData swappableMeshSaveData in characterAppearanceData.swappableMeshSaveDataList) {
                LoadGroupChoice(swappableMeshSaveData.groupName, swappableMeshSaveData.optionName);
            }

        }

        public override void BuildModelAppearance() {
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.BuildModelAppearance()");

            SetupAppliedConfiguration();
            ApplyConfiguration();
            unitModelController.SetModelReady();
        }

        public override int RebuildModelAppearance() {
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.RebuildModelAppearance()");

            int updateCount = base.RebuildModelAppearance();
            if (updateCount > 0) {
                BuildModelAppearance();
            }
            return updateCount;
        }

        public override bool IsBuilding() {
            return false;
        }

        public override void ResetSettings() {
            // nothing to do here for now
        }

        public override void EquipItemModels(EquipmentSlotProfile equipmentSlotProfile, Equipment equipment) {
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.EquipItemModels()");

            base.EquipItemModels(equipmentSlotProfile, equipment);

            if (equipment == null) {
                return;
            }

            EquipItemModel(equipment.GetEquipmentModel<SwappableMeshEquipmentModel>());

        }

        private void EquipItemModel(SwappableMeshEquipmentModel swappableMeshEquipmentModel) {
            if (swappableMeshEquipmentModel == null) {
                return;
            }

            foreach (SwappableMeshEquipmentModelNode swappableMeshEquipmentModelNode in swappableMeshEquipmentModel.Properties.Meshes) {
                LoadGroupChoice(swappableMeshEquipmentModelNode.GroupName, swappableMeshEquipmentModelNode.OptionName);
            }
        }

        protected override void UnequipItemModels(EquipmentSlotProfile equipmentSlotProfile) {
            if (equippedEquipment[equipmentSlotProfile] != null) {
                UnequipItemModel(equippedEquipment[equipmentSlotProfile].GetEquipmentModel<SwappableMeshEquipmentModel>());
            }
            
            base.UnequipItemModels(equipmentSlotProfile);
        }

        private void UnequipItemModel(SwappableMeshEquipmentModel swappableMeshEquipmentModel) {
            if (swappableMeshEquipmentModel == null) {
                return;
            }

            foreach (SwappableMeshEquipmentModelNode swappableMeshEquipmentModelNode in swappableMeshEquipmentModel.Properties.Meshes) {
                LoadGroupChoice(swappableMeshEquipmentModelNode.GroupName, "");
            }
        }

        public override void DespawnModel() {
            // nothing to do here for now
        }

        public override void ConfigureUnitModel() {
            Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.ConfigureUnitModel()");

            if (unitModelController.UnitModel == null) {
                return;
            }

            GetMeshRenderers();

            //RebuildModelAppearance();

            // call base so that equipment is updated whether anything is equipped or not (such as in the case of equipment suppression for appearance editors)
            base.RebuildModelAppearance();

            // build the model appearance based on current settings
            BuildModelAppearance();
        }

        private void GetMeshRenderers() {
            //Debug.Log($"{unitController.gameObject.name}.SwappableMeshModelController.GetMeshRenderers()");

            foreach (Transform childTransform in unitModelController.UnitModel.transform) {
                if (childTransform.GetComponent<SkinnedMeshRenderer>() != null) {
                    meshRenderers.Add(childTransform.gameObject);
                }
            }
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