using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class UnitModelController : ConfiguredClass {

        public event System.Action OnModelCreated = delegate { };
        public event System.Action OnModelUpdated = delegate { };

        // reference to unit
        private UnitController unitController = null;
        private GameObject unitModel = null;
        private CharacterEquipmentManager characterEquipmentManager = null;

        // track model
        private bool modelCreated = false;

        // specific controllers
        private ModelAppearanceController modelAppearanceController = null;
        private MecanimModelController mecanimModelController = null;

        // properties
        private Transform floatTransform = null;

        // options
        private bool suppressEquipment = false;

        private CharacterAppearanceData characterAppearanceData = null;

        // game manager references
        private ObjectPooler objectPooler = null;
        private CharacterManager characterManager = null;

        public ModelAppearanceController ModelAppearanceController { get => modelAppearanceController; }
        public MecanimModelController MecanimModelController { get => mecanimModelController; }
        public bool ModelCreated { get => modelCreated; }
        public GameObject UnitModel { get => unitModel; }
        public bool SuppressEquipment { get => suppressEquipment; set => suppressEquipment = value; }
        public CharacterEquipmentManager CharacterEquipmentManager { get => characterEquipmentManager; }

        public UnitModelController(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);

            mecanimModelController = new MecanimModelController(unitController, this, systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            characterManager = systemGameManager.CharacterManager;
        }

        public void Initialize() {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.Initialize()");

            characterEquipmentManager = unitController.CharacterEquipmentManager;
            if (characterEquipmentManager == null) {
                Debug.LogWarning("CharacterEquipmentManager was null");
            }
            mecanimModelController.Initialize();
        }

        public void HideEquipment() {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.HideEquipment()");

            suppressEquipment = true;
            RebuildModelAppearance();
        }

        public void ShowEquipment() {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.ShowEquipment()");

            suppressEquipment = false;
            RebuildModelAppearance();
        }

        public void SetAppearanceController(UnitProfile unitProfile) {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.SetAppearanceController(" + (unitProfile == null ? "null" : unitProfile.ResourceName) + ")");

            if (unitProfile?.UnitPrefabProps.ModelProvider != null) {
                modelAppearanceController = unitProfile.UnitPrefabProps.ModelProvider.GetAppearanceController(unitController, this, systemGameManager);
            } else {
                // no model provider was configured, create null object
                modelAppearanceController = new DefaultModelController(unitController, this, systemGameManager);
            }

            modelAppearanceController.Initialize();
        }

        public bool IsBuilding() {
            return modelAppearanceController.IsBuilding();
        }

        public void ResetSettings() {
            // check for null here because this could happen from a network disconnect
            modelAppearanceController?.ResetSettings();
        }

        public void SetUnitModel(GameObject go) {
            unitModel = go;
        }

        public void SpawnUnitModel() {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.SpawnUnitModel()");

            if (unitController.UnitProfile?.UnitPrefabProps?.ModelPrefab != null) {
                unitModel = systemGameManager.CharacterManager.SpawnModelPrefab(unitController, unitController.UnitProfile, unitController.transform, unitController.transform.position, unitController.transform.forward);
            }
        }

        public void FindUnitModel(Animator animator) {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.FindUnitModel()");

            // this may have been called from a unit which already had a model attached
            // if so, the model is the animator gameobject, since no model will have been passed to this call
            if (animator != null && unitModel == null) {
                unitModel = animator.gameObject;
            }

            if (modelAppearanceController == null) {
                return;
            }

            modelAppearanceController.FindUnitModel(unitModel);

        }

        public void BuildModelAppearance() {
            modelAppearanceController.BuildModelAppearance();
        }

        public void RebuildModelAppearance() {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.RebuildModelAppearance()");

            modelAppearanceController.RebuildModelAppearance();
            mecanimModelController.RebuildModelAppearance();
            unitController.UnitEventController.NotifyOnRebuildModelAppearance();
        }

        /*
        public void LoadSavedAppearanceSettings(string recipeString = null, bool rebuildAppearance = false) {
            umaModelController.LoadSavedAppearanceSettings(recipeString, rebuildAppearance);
        }
        */

        public void SaveAppearanceSettings(CharacterSaveData saveData) {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.SaveAppearanceSettings()");
            
            if (modelAppearanceController == null) {
                // this function will be called once before the model is spawned
                return;
            }
            modelAppearanceController.SaveAppearanceSettings(saveData);
        }

        public void SetAnimatorOverrideController(AnimatorOverrideController animatorOverrideController) {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.SetAnimatorOverrideController({animatorOverrideController.GetInstanceID()})");

            //if (modelAppearanceController == null) {
                //Debug.LogWarning("Null model appearance controller!");
            //}
            modelAppearanceController.SetAnimatorOverrideController(animatorOverrideController);
        }

        /*
        public void EquipEquipmentModels() {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.EquipEquipmentModels()");

            if (suppressEquipment == true) {
                return;
            }

            if (characterEquipmentManager.CurrentEquipment == null || characterEquipmentManager.CurrentEquipment.Count == 0) {
                //Debug.Log($"{unitController.gameObject.name}.UnitModelController.EquipCharacter(): currentEquipment == null!");
                // no point building model appearance if there was nothing equipped
                return;
            }
            foreach (EquipmentSlotProfile equipmentSlotProfile in characterEquipmentManager.CurrentEquipment.Keys) {
                if (characterEquipmentManager.CurrentEquipment[equipmentSlotProfile] != null) {
                    // armor and weapon models handling
                    EquipItemModels(characterEquipmentManager, equipmentSlotProfile, characterEquipmentManager.CurrentEquipment[equipmentSlotProfile]);
                }
            }

            //umaModelController.BuildModelAppearance();
        }
        */

        /*
        private void EquipItemModels(CharacterEquipmentManager characterEquipmentManager, EquipmentSlotProfile equipmentSlotProfile, Equipment equipment) {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.EquipItemModels(" + equipment.DisplayName + ", " + equipModels + ", " + setAppearance + ", " + rebuildAppearance + ")");

            if (suppressEquipment == true) {
                return;
            }

            if (characterEquipmentManager.CurrentEquipment == null) {
                Debug.LogError("UnitModelController.EquipItemModels(" + equipmentSlotProfile.DisplayName + "): currentEquipment is null!");
                return;
            }
            if (!characterEquipmentManager.CurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                Debug.LogError("UnitModelController.EquipItemModels(" + equipmentSlotProfile.DisplayName + "): currentEquipment does not have key");
                return;
            }
            //Equipment equipment = characterEquipmentManager.CurrentEquipment[equipmentSlotProfile];
            //public void HandleWeaponSlot(Equipment newItem) {
            if (equipment == null || unitController == null) {
                //Debug.Log($"{gameObject.name}.CharacterEquipmentManager.HandleWeaponSlot(): MyHoldableObjectName is empty on " + newItem.DisplayName);
                return;
            }

            modelAppearanceController.EquipItemModels(equipmentSlotProfile, equipment);
            mecanimModelController.EquipItemModels(equipmentSlotProfile, equipment);
        }
        */

        /*
        public void UnequipItemModels(EquipmentSlotProfile equipmentSlot, Equipment equipment) {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.UnequipItemModels(" + equipment.DisplayName + ", " + unequipModels + ", " + unequipAppearance + ", " + rebuildAppearance + ")");

            modelAppearanceController.UnequipItemModels(equipmentSlot);
            mecanimModelController.UnequipItemModels(equipmentSlot);
        }
        */

        /*
        public void SetInitialAppearance(string appearance) {
            umaModelController.SetInitialAppearance(appearance);
        }
        */

        public void LoadInitialSavedAppearance(CharacterAppearanceData characterAppearanceData) {
            this.characterAppearanceData = characterAppearanceData;
        }

        public void SetInitialSavedAppearance() {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.SetInitialSavedAppearance()");

            if (characterAppearanceData == null) {
                // in empty game mode, this can be null
                return;
            }
            //if (modelAppearanceController == null) {
              //Debug.Log($"{unitController.gameObject.name}.UnitModelController.SetInitialSavedAppearance() model appearance controller is null");
              //return;
            //}
            modelAppearanceController.SetInitialSavedAppearance(characterAppearanceData);
        }

        public void ConfigureUnitModel() {
            //Debug.Log($"{unitController.gameObject.name}UnitModelController.ConfigureUnitModel()");

            if (modelAppearanceController == null) {
                if (unitModel != null) {
                    SetModelReady();
                }
                return;
            }

            modelAppearanceController.ConfigureUnitModel();
        }

        public bool KeepMonoBehaviorEnabled(MonoBehaviour monoBehaviour) {
            return modelAppearanceController.KeepMonoBehaviorEnabled(monoBehaviour);
        }

        public void SetAttachmentProfile(AttachmentProfile attachmentProfile) {
            mecanimModelController.SetAttachmentProfile(attachmentProfile);
        }

        public void DespawnModel() {
            //Debug.Log($"{unitController.gameObject.name}UnitModelController.DespawnModel()");
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }
            // check for null here because this could happen from a network disconnect
            mecanimModelController?.DespawnModel();
            modelAppearanceController?.DespawnModel();
            if (unitController.UnitProfile?.UnitPrefabProps?.ModelPrefab != null) {
                if (networkManagerServer.ServerModeActive == true) {
                    // this is happening on the server, return the object to the pool
                    networkManagerServer.ReturnObjectToPool(unitModel);
                } else {
                    // this is happening on the client
                    if (characterManager.LocalUnits.Contains(unitController)) {
                        // this unit was requested in a local game, pool it
                        objectPooler.ReturnObjectToPool(unitModel);
                    } else {
                        // this unit was requested in a network game, deactivate it and let it wait for the network pooler to claim it
                        // if client crashes during spawn process, this could be null so must check for that
                        unitModel?.SetActive(false);
                    }
                }
            }
        }



        public void SetDefaultLayer(string layerName) {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.SetDefaultLayer(" + layerName + ")");

            if (layerName != null && layerName != string.Empty) {
                int defaultLayer = LayerMask.NameToLayer(layerName);
                int finalmask = (1 << defaultLayer);
                if (!LayerUtility.IsInLayerMask(unitController.gameObject.layer, finalmask)) {
                    unitController.gameObject.layer = defaultLayer;
                }

                if (unitModel != null) {
                    int equipmentMask = 1 << LayerMask.NameToLayer("Equipment");
                    int spellMask = 1 << LayerMask.NameToLayer("SpellEffects");
                    int raycastmask = 1 << LayerMask.NameToLayer("Ignore Raycast");
                    int ignoreMask = (equipmentMask | spellMask | raycastmask);
                    LayerUtility.SetTransformLayerRecursive(unitModel, defaultLayer, ignoreMask);
                }
            }
        }

        public void SheathWeapons() {
            mecanimModelController.SheathWeapons();
        }

        public void HoldWeapons() {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.HoldWeapons()");

            mecanimModelController.HoldWeapons();
        }

        public void SetModelReady() {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.SetModelReady()");

            bool modelWasCreated = modelCreated;

            //RebuildModelAppearance();
            // give mecanim model controller a chance to spawn or despawn weapons now that character skeleton is available (if UMA was used)
            mecanimModelController.RebuildModelAppearance();

            if (modelAppearanceController.ShouldCalculateFloatHeight()) {
                CalculateFloatHeight();
            }
            if (modelCreated == false) {
                modelCreated = true;
                //Debug.Log("OnModelCreated()");
                OnModelCreated();
            } else {
                //Debug.Log("OnModelUpdated()");
                OnModelUpdated();
            }

            if (modelWasCreated == false) {
                unitController.CharacterStats.HandleCharacterUnitSpawn();
            }

            unitController.SetModelReady();

            if (modelWasCreated == false) {
                unitController.UnitMountManager.ProcessModelCreated();
            }
        }

        public void CalculateFloatHeight() {
            unitController.FloatHeight += unitController.UnitProfile.UnitPrefabProps.FloatHeight;

            if (unitController.UnitProfile?.UnitPrefabProps?.FloatTransform == string.Empty) {
                return;
            }

            floatTransform = unitController.transform.FindChildByRecursive(unitController.UnitProfile.UnitPrefabProps.FloatTransform);
            if (floatTransform == null) {
                return;
            }

            unitController.FloatHeight = floatTransform.position.y - unitController.transform.position.y;
            if (unitController.UnitProfile.UnitPrefabProps.AddFloatHeightToTransform == false) {
                return;
            }
            unitController.FloatHeight += unitController.UnitProfile.UnitPrefabProps.FloatHeight;


            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.CalculateFloatHeight() new float height: " + unitController.FloatHeight);
        }

        public void ProcessAddStatusEffect(StatusEffectNode newStatusEffectNode, StatusEffectProperties statusEffect, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{unitController.gameObject.name}.UnitModelController.ProcessAddStatusEffect({statusEffect.DisplayName})");

            if (modelCreated == false) {
                return;
            }

            // on non authoritative network clients, do not spawn visual effect when stealthed
            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false && unitController.IsStealth == true) {
                return;
            }

            Dictionary<PrefabProfile, List<GameObject>> returnObjects = unitController.CharacterAbilityManager.SpawnStatusEffectPrefabs(unitController, statusEffect, abilityEffectContext);
            if (returnObjects != null) {
                // pass in the ability effect object so we can independently destroy it and let it last as long as the status effect (which could be refreshed).
                newStatusEffectNode.PrefabObjects = returnObjects;
            }
            statusEffect.PerformMaterialChange(unitController);

        }
    }

}