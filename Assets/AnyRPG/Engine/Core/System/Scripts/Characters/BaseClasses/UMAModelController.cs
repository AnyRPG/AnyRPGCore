using AnyRPG;
using UnityEngine;
using UnityEngine.AI;
using UMA;
using UMA.CharacterSystem;

namespace AnyRPG {
    public class UMAModelController {

        // reference to unit
        private UnitController unitController = null;
        private DynamicCharacterAvatar dynamicCharacterAvatar = null;
        private UnitModelController unitModelController = null;

        public DynamicCharacterAvatar DynamicCharacterAvatar { get => dynamicCharacterAvatar; }

        public UMAModelController(UnitController unitController, UnitModelController unitModelController) {
            this.unitController = unitController;
            this.unitModelController = unitModelController;
        }

        public void InitializeModel() {
            // testing - pooled UMA units will already be initialized
            // so they should be considered ready if they have umaData
            if (dynamicCharacterAvatar.umaData == null) {
                //Debug.Log(gameObject.name + "UnitController.ConfigureUnitModel(): dynamicCharacterAvatar.Initialize()");
                dynamicCharacterAvatar.Initialize();
            } else {
                //Debug.Log(gameObject.name + "UnitController.ConfigureUnitModel(): dynamicCharacterAvatar has been re-used and is already initialized");
                unitModelController.SetModelReady();
            }

            SubscribeToUMACreate();

        }

        public void FindUnitModel(GameObject unitModel) {
            if (unitModel != null && dynamicCharacterAvatar == null) {
                dynamicCharacterAvatar = unitModel.GetComponent<DynamicCharacterAvatar>();
            }
            if (dynamicCharacterAvatar == null) {
                dynamicCharacterAvatar = unitController.GetComponentInChildren<DynamicCharacterAvatar>();
            }
        }

        public void SubscribeToUMACreate() {

            dynamicCharacterAvatar.umaData.OnCharacterCreated += HandleCharacterCreated;
            /*
            umaData.OnCharacterBeforeDnaUpdated += HandleCharacterBeforeDnaUpdated;
            umaData.OnCharacterBeforeUpdated += HandleCharacterBeforeUpdated;
            umaData.OnCharacterDnaUpdated += HandleCharacterDnaUpdated;
            umaData.OnCharacterDestroyed += HandleCharacterDestroyed;
            */
            dynamicCharacterAvatar.umaData.OnCharacterUpdated += HandleCharacterUpdated;
        }

        public void UnsubscribeFromUMACreate() {
            if (dynamicCharacterAvatar?.umaData != null) {
                dynamicCharacterAvatar.umaData.OnCharacterCreated -= HandleCharacterCreated;
                dynamicCharacterAvatar.umaData.OnCharacterUpdated -= HandleCharacterUpdated;
            }
        }

        public void HandleCharacterCreated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.HandleCharacterCreated(): " + umaData);
            //UnsubscribeFromUMACreate();
            unitModelController.SetModelReady();
        }

        public void HandleCharacterBeforeDnaUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.BeforeDnaUpdated(): " + umaData);
        }
        public void HandleCharacterBeforeUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.OnCharacterBeforeUpdated(): " + umaData);
        }
        public void HandleCharacterDnaUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.OnCharacterDnaUpdated(): " + umaData);
        }
        public void HandleCharacterDestroyed(UMAData umaData) {
            //Debug.Log("PreviewCameraController.OnCharacterDestroyed(): " + umaData);
        }
        public void HandleCharacterUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.HandleCharacterUpdated(): " + umaData + "; frame: " + Time.frameCount);
            //HandleCharacterCreated(umaData);
            unitModelController.SetModelReady();
        }

        public void DespawnModel() {
            UnsubscribeFromUMACreate();

        }


        public void UnequipItemModels(Equipment equipment, bool rebuildAppearance) {
            if (equipment.MyUMARecipes != null && equipment.MyUMARecipes.Count > 0 && dynamicCharacterAvatar != null) {
                // Clear the item from the UMA slot on the UMA character
                //Debug.Log("Clearing UMA slot " + oldItem.UMARecipe.wardrobeSlot);
                //avatar.SetSlot(newItem.UMARecipe.wardrobeSlot, newItem.UMARecipe.name);
                foreach (UMATextRecipe uMARecipe in equipment.MyUMARecipes) {
                    if (uMARecipe != null && uMARecipe.compatibleRaces.Contains(dynamicCharacterAvatar.activeRace.name)) {
                        dynamicCharacterAvatar.ClearSlot(uMARecipe.wardrobeSlot);
                    }
                }
                if (rebuildAppearance) {
                    BuildModelAppearance();
                }
            }
        }

        public void EquipItemModels(CharacterEquipmentManager characterEquipmentManager, Equipment equipment, bool rebuildAppearance = true) {

            if (equipment.MyUMARecipes != null
            && equipment.MyUMARecipes.Count > 0
            && dynamicCharacterAvatar != null) {
                //Debug.Log("EquipmentManager.HandleItemUMARecipe(): " + newItem.DisplayName);
                // Put the item in the UMA slot on the UMA character
                //Debug.Log("Putting " + newItem.UMARecipe.name + " in slot " + newItem.UMARecipe.wardrobeSlot);
                foreach (UMATextRecipe uMARecipe in equipment.MyUMARecipes) {
                    if (uMARecipe != null && uMARecipe.compatibleRaces.Contains(dynamicCharacterAvatar.activeRace.name)) {
                        //Debug.Log("EquipmentManager.HandleItemUMARecipe(): SetSlot: " + uMARecipe.wardrobeSlot + ", " + uMARecipe.name);
                        dynamicCharacterAvatar.SetSlot(uMARecipe.wardrobeSlot, uMARecipe.name);
                    }
                }
                if (rebuildAppearance) {
                    BuildModelAppearance();
                }
            }
        }

        public void SetAnimatorOverrideController(AnimatorOverrideController animatorOverrideController) {
            if (dynamicCharacterAvatar != null) {
                dynamicCharacterAvatar.raceAnimationControllers.defaultAnimationController = animatorOverrideController;
                dynamicCharacterAvatar.animationController = animatorOverrideController;
            }
        }

        public string GetAppearanceSettings() {
            if (dynamicCharacterAvatar != null) {
                return dynamicCharacterAvatar.GetCurrentRecipe();
            }
            return string.Empty;
        }

        public void LoadSavedAppearanceSettings() {
            if (dynamicCharacterAvatar != null) {
                SystemGameManager.Instance.SaveManager.LoadUMASettings(dynamicCharacterAvatar, false);
            }
        }

        public void SaveAppearanceSettings() {
            if (dynamicCharacterAvatar != null) {
                SystemGameManager.Instance.SaveManager.SaveRecipeString(dynamicCharacterAvatar.GetCurrentRecipe());
            }
        }

        public void BuildModelAppearance() {
            Debug.Log(unitController.gameObject.name + ".UMAModelController.BuildModelAppearance()");
            if (dynamicCharacterAvatar != null) {
                dynamicCharacterAvatar.BuildCharacter();
            }

        }

        public void ResetSettings() {
            if (dynamicCharacterAvatar != null) {

                dynamicCharacterAvatar.ClearSlots();
                dynamicCharacterAvatar.RestoreCachedBodyColors();
                dynamicCharacterAvatar.LoadDefaultWardrobe();
                // doing the rebuild on despawn so there isn't a frame with this appearance until a rebuild happens when re-using the avatar
                dynamicCharacterAvatar.BuildCharacter();
            }

        }

        public bool KeepMonoBehaviorEnabled(MonoBehaviour monoBehaviour) {
            if ((monoBehaviour as DynamicCharacterAvatar) is DynamicCharacterAvatar) {
                return true;
            }
            return false;
        }

    }

}