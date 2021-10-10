using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UMA;
using UMA.CharacterSystem;
using UMA.PoseTools;

namespace AnyRPG {
    public class UMAModelController : ConfiguredClass {


        // reference to unit
        private UnitController unitController = null;
        private DynamicCharacterAvatar dynamicCharacterAvatar = null;
        private UMAExpressionPlayer expressionPlayer = null;
        private UnitModelController unitModelController = null;
        private AvatarDefinition originalAvatarDefinition = new AvatarDefinition();
        private AvatarDefinition avatarDefinition = new AvatarDefinition();

        public DynamicCharacterAvatar DynamicCharacterAvatar { get => dynamicCharacterAvatar; }

        // track settings that should be applied on initialization
        private string initialAppearance = null;

        private bool buildInProgress = false;

        // game manager references
        private SaveManager saveManager = null;

        public UMAModelController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager) {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController()");
            this.unitController = unitController;
            this.unitModelController = unitModelController;
            Configure(systemGameManager);

            // avatarDefintion is a struct so needs to have its properties set to something other than null
            avatarDefinition.RaceName = string.Empty;
            avatarDefinition.Wardrobe = new string[0];
            avatarDefinition.Dna = new DnaDef[0];
            avatarDefinition.Colors = new SharedColorDef[0];
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
        }

        public bool IsBuilding() {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.IsBuilding()");
            if (dynamicCharacterAvatar?.umaData == null) {
                //Debug.Log(unitController.gameObject.name + ".UMAModelController.IsBuilding() : no dynamicCharacterAvatar was found");
                return false;
            }
            /*
            Debug.Log(unitController.gameObject.name + ".UMAModelController.IsBuilding() : uma dirty status: " + dynamicCharacterAvatar.umaData.dirty);
            Debug.Log(unitController.gameObject.name + ".UMAModelController.IsBuilding() : update pending status : " + dynamicCharacterAvatar.UpdatePending());
            Debug.Log(unitController.gameObject.name + ".UMAModelController.IsBuilding() : build in progress status: " + buildInProgress);
            */
            //return dynamicCharacterAvatar.umaData.dirty || dynamicCharacterAvatar.UpdatePending() || buildInProgress;
            return dynamicCharacterAvatar.umaData.dirty || dynamicCharacterAvatar.UpdatePending() || buildInProgress;
        }

        /*
        public void SetInitialAppearance(string appearance) {
            initialAppearance = appearance;
        }
        */

        public void SetAppearance(string appearance) {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.SetAppearance()");
            initialAppearance = appearance;
            avatarDefinition = AvatarDefinition.FromCompressedString(initialAppearance, '|');
            CheckAvatarDefinition();
            dynamicCharacterAvatar.LoadAvatarDefinition(avatarDefinition);
            BuildModelAppearance();
        }

        public bool ShouldCalculateFloatHeight() {
            return true;
        }

        private void CheckAvatarDefinition() {
            /*if (avatarDefinition.Wardrobe == null
                || avatarDefinition.RaceName == null
                || avatarDefinition.Dna == null
                || avatarDefinition.Colors == null) {*/
            if (avatarDefinition.RaceName == null) {
                Debug.LogWarning("AvatarDefinition could not be loaded.  The save file may be in an older format.  You will need to reset the character appearance (using the CharacterCreator) and save the game again to update it.");
                avatarDefinition = new AvatarDefinition();

                // avatarDefintion is a struct so needs to have its properties set to something other than null
                avatarDefinition.RaceName = "HumanMaleDCS";
                avatarDefinition.Wardrobe = new string[0];
                avatarDefinition.Dna = new DnaDef[0];
                avatarDefinition.Colors = new SharedColorDef[0];
            }
        }

        public void SetInitialSavedAppearance() {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.SetInitialSavedAppearance()");
            if (saveManager.RecipeString != null
                && saveManager.RecipeString != string.Empty) {
                initialAppearance = saveManager.RecipeString;
            }
        }

        public void InitializeModel() {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.InitializeModel()");
            int preloadedModels = 0;

            if (initialAppearance != null && initialAppearance != string.Empty) {
                avatarDefinition = AvatarDefinition.FromCompressedString(initialAppearance, '|');
                CheckAvatarDefinition();
            } else {
                avatarDefinition = GetAvatarDefinition(dynamicCharacterAvatar);
            }
            if (unitController.CharacterUnit?.BaseCharacter?.CharacterEquipmentManager != null) {
                preloadedModels = PreloadEquipmentModels(unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager);
            }
            if ((initialAppearance != null && initialAppearance != string.Empty) || preloadedModels > 0) {
                // only needed if the UMA had some custom appearance or extra equipment loaded onto it
                dynamicCharacterAvatar.LoadAvatarDefinition(avatarDefinition);
            }
            if (dynamicCharacterAvatar.umaData == null || dynamicCharacterAvatar.umaData.firstBake == true) {
                dynamicCharacterAvatar.Initialize();
                buildInProgress = true;
            } else {
                if ((initialAppearance == null || initialAppearance == string.Empty) && preloadedModels == 0) {
                    SetModelReady();
                } else {
                    BuildModelAppearance();
                }
            }

            SubscribeToUMACreate();
        }

        public void SetAvatarDefinitionRace(string raceName) {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.SetAvatarDefinitionRace(" + raceName + ")");
            avatarDefinition = dynamicCharacterAvatar.GetAvatarDefinition(true);
            avatarDefinition.RaceName = raceName;
        }

        public void ReloadAvatarDefinition() {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.ReloadAvatarDefinition()");
            dynamicCharacterAvatar.LoadAvatarDefinition(avatarDefinition, true);
            BuildModelAppearance();
        }

        private AvatarDefinition GetAvatarDefinition(DynamicCharacterAvatar dynamicCharacterAvatar) {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.GetAvatarDefinition()");

            AvatarDefinition adf = new AvatarDefinition();
            // race
            adf.RaceName = dynamicCharacterAvatar.activeRace.name;

            // wardrobe
            List<string> wardrobeRecipes = new List<string>();
            foreach (DynamicCharacterAvatar.WardrobeRecipeListItem item in dynamicCharacterAvatar.preloadWardrobeRecipes.recipes) {
                wardrobeRecipes.Add(item._recipeName);
            }
            adf.Wardrobe = wardrobeRecipes.ToArray();

            // dna
            List<DnaDef> dnaDefs = new List<DnaDef>();
            foreach (DnaValue dnaValue in dynamicCharacterAvatar.predefinedDNA.PreloadValues) {
                DnaDef def = new DnaDef(dnaValue.Name, dnaValue.Value);
                dnaDefs.Add(def);
            }
            adf.Dna = dnaDefs.ToArray();

            // colors
            List<SharedColorDef> Colors = new List<SharedColorDef>();
            foreach (DynamicCharacterAvatar.ColorValue colorValueList in dynamicCharacterAvatar.characterColors.Colors) {
                SharedColorDef scd = new SharedColorDef(colorValueList.name, colorValueList.channelCount);
                List<ColorDef> colorchannels = new List<ColorDef>();

                for (int i = 0; i < colorValueList.channelCount; i++) {
                    if (colorValueList.isDefault(i)) continue;
                    Color Mask = colorValueList.channelMask[i];
                    Color Additive = colorValueList.channelAdditiveMask[i];
                    colorchannels.Add(new ColorDef(i, ColorDef.ToUInt(Mask), ColorDef.ToUInt(Additive)));
                }
                if (colorchannels.Count > 0) {
                    scd.SetChannels(colorchannels.ToArray());
                    Colors.Add(scd);
                }
            }
            adf.Colors = Colors.ToArray();

            return adf;
        }

        /*
        public void LoadSavedAppearanceSettings(string recipeString = null, bool rebuildAppearance = false) {
            Debug.Log(unitController.gameObject.name + ".UMAModelController.LoadSavedAppearanceSettings()");
            if (dynamicCharacterAvatar != null) {
                if (recipeString != null && recipeString != string.Empty) {
                    //Debug.Log(unitController.gameObject.name + ".UMAModelController.LoadSavedAppearanceSettings() : loading string from parameters : " + recipeString);
                    buildInProgress = true;
                    //dynamicCharacterAvatar.SetLoadString(recipeString);

                } else if (recipeString == null
                    && saveManager.RecipeString != null
                    && saveManager.RecipeString != string.Empty) {
                    //Debug.Log(unitController.gameObject.name + ".UMAModelController.LoadSavedAppearanceSettings() : loading string from SaveManager : " + saveManager.RecipeString);
                    buildInProgress = true;
                    dynamicCharacterAvatar.SetLoadString(saveManager.RecipeString);
                }
                if (rebuildAppearance == true && dynamicCharacterAvatar.BuildCharacterEnabled == false) {
                    // by default an UMA will build appearance unless the option is disabled, so this call is redundant unless the UMA is configured to not build
                    //Debug.Log(unitController.gameObject.name + ".UMAModelController.LoadSavedAppearanceSettings() : building model appearance");
                    BuildModelAppearance();
                }
            }
        }
        */

        public void FindUnitModel(GameObject unitModel) {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.FindUnitModel(" + (unitModel == null ? "null" : unitModel.name) + ")");
            if (unitModel != null && dynamicCharacterAvatar == null) {
                dynamicCharacterAvatar = unitModel.GetComponent<DynamicCharacterAvatar>();
            }
            if (dynamicCharacterAvatar == null) {
                dynamicCharacterAvatar = unitController.GetComponentInChildren<DynamicCharacterAvatar>();
            }
            if (dynamicCharacterAvatar != null) {
                originalAvatarDefinition = GetAvatarDefinition(dynamicCharacterAvatar);

            }
        }

        public void SubscribeToUMACreate() {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.SubscribeToUMACreate()");

            //dynamicCharacterAvatar.umaData.OnCharacterCreated += HandleCharacterCreated;
            /*
            umaData.OnCharacterBeforeDnaUpdated += HandleCharacterBeforeDnaUpdated;
            umaData.OnCharacterBeforeUpdated += HandleCharacterBeforeUpdated;
            umaData.OnCharacterDnaUpdated += HandleCharacterDnaUpdated;
            umaData.OnCharacterDestroyed += HandleCharacterDestroyed;
            */
            dynamicCharacterAvatar.umaData.OnCharacterUpdated += HandleCharacterUpdated;
        }

        public void UnsubscribeFromUMACreate() {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.UnsubscribeFromUMACreate()");
            if (dynamicCharacterAvatar?.umaData != null) {
                //dynamicCharacterAvatar.umaData.OnCharacterCreated -= HandleCharacterCreated;
                dynamicCharacterAvatar.umaData.OnCharacterUpdated -= HandleCharacterUpdated;
            }
        }

        public void HandleCharacterCreated(UMAData umaData) {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.HandleCharacterCreated()");
            //UnsubscribeFromUMACreate();
            SetModelReady();
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
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.HandleCharacterUpdated()");
            //Debug.Log("UMAModelController.HandleCharacterUpdated(): " + umaData + "; frame: " + Time.frameCount);
            //HandleCharacterCreated(umaData);

            SetModelReady();
        }

        public void SetModelReady() {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.SetModelReady()");

            buildInProgress = false;
            unitModelController.SetModelReady();
        }

        public void DespawnModel() {
            UnsubscribeFromUMACreate();

        }

        public int PreloadEquipmentModels(bool resetWardrobe = false) {
            return PreloadEquipmentModels(unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager, resetWardrobe);
        }

        public int PreloadEquipmentModels(CharacterEquipmentManager characterEquipmentManager, bool resetWardrobe = false) {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.PreloadEquipmentModels(" + resetWardrobe + ")");
            int returnValue = 0;
            if (resetWardrobe == true) {
                avatarDefinition.Wardrobe = new string[0];
            }
            foreach (EquipmentSlotProfile equipmentSlotProfile in characterEquipmentManager.CurrentEquipment.Keys) {
                if (characterEquipmentManager.CurrentEquipment[equipmentSlotProfile] != null) {
                    // armor and weapon models handling
                    returnValue += PreloadItemModels(characterEquipmentManager, characterEquipmentManager.CurrentEquipment[equipmentSlotProfile]);
                }
            }
            return returnValue;
        }


        public void UnequipItemModels(Equipment equipment, bool rebuildAppearance) {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.UnequipItemModels(" + equipment.DisplayName + ", " + rebuildAppearance + ")");
            if (equipment.UMARecipeProfile?.UMARecipes != null && equipment.UMARecipeProfile.UMARecipes.Count > 0 && dynamicCharacterAvatar != null) {
                // Clear the item from the UMA slot on the UMA character
                //Debug.Log("Clearing UMA slot " + oldItem.UMARecipe.wardrobeSlot);
                //avatar.SetSlot(newItem.UMARecipe.wardrobeSlot, newItem.UMARecipe.name);
                foreach (UMATextRecipe uMARecipe in equipment.UMARecipeProfile.UMARecipes) {
                    if (uMARecipe != null && uMARecipe.compatibleRaces.Contains(dynamicCharacterAvatar.activeRace.name)) {
                        dynamicCharacterAvatar.ClearSlot(uMARecipe.wardrobeSlot);
                        if (equipment.UMARecipeProfile?.SharedColors != null && equipment.UMARecipeProfile.SharedColors.Count > 0) {
                            foreach (SharedColorNode sharedColorNode in equipment.UMARecipeProfile.SharedColors) {
                                dynamicCharacterAvatar.ClearColor(sharedColorNode.SharedColorname, false);
                            }
                        }
                    }
                }
                if (rebuildAppearance) {
                    BuildModelAppearance();
                }
            }
        }

        public int PreloadItemModels(CharacterEquipmentManager characterEquipmentManager, Equipment equipment) {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.PreloadItemModels(" + equipment.DisplayName + ")");
            int returnValue = 0;
            if (equipment.UMARecipeProfile?.UMARecipes != null
            && equipment.UMARecipeProfile.UMARecipes.Count > 0
            && dynamicCharacterAvatar != null) {
                //Debug.Log("EquipmentManager.HandleItemUMARecipe(): " + newItem.DisplayName);
                // Put the item in the UMA slot on the UMA character
                //Debug.Log("Putting " + newItem.UMARecipe.name + " in slot " + newItem.UMARecipe.wardrobeSlot);
                List<string> newWardrobe = new List<string>(avatarDefinition.Wardrobe);
                List<SharedColorDef> newColors = new List<SharedColorDef>(avatarDefinition.Colors);

                // create a dictionary to ensure the last item valid for a slot in the list is the one that gets set
                Dictionary<string, string> wardrobeRecipes = new Dictionary<string, string>();

                foreach (UMATextRecipe uMARecipe in equipment.UMARecipeProfile.UMARecipes) {
                    //if (uMARecipe != null && uMARecipe.compatibleRaces.Contains(dynamicCharacterAvatar.activeRace.name)) {
                    if (uMARecipe != null && uMARecipe.compatibleRaces.Contains(avatarDefinition.RaceName)) {
                        //Debug.Log(unitController.gameObject.name + ".UMAModelController.PreloadItemModels(): Adding: " + uMARecipe.wardrobeSlot + ", " + uMARecipe.name);
                        //dynamicCharacterAvatar.SetSlot(uMARecipe.wardrobeSlot, uMARecipe.name);
                        if (wardrobeRecipes.ContainsKey(uMARecipe.wardrobeSlot) == false) {
                            wardrobeRecipes.Add(uMARecipe.wardrobeSlot, uMARecipe.name);
                        } else {
                            wardrobeRecipes[uMARecipe.wardrobeSlot] = uMARecipe.name;
                        }
                    }
                }
                foreach (string wardrobeSlot in wardrobeRecipes.Keys) {
                    //Debug.Log(unitController.gameObject.name + ".UMAModelController.PreloadItemModels(): Checking: " + wardrobeSlot + ", " + wardrobeRecipes[wardrobeSlot]);
                    if (newWardrobe.Contains(wardrobeRecipes[wardrobeSlot]) == false) {
                        //Debug.Log(unitController.gameObject.name + ".UMAModelController.PreloadItemModels(): Setting: " + wardrobeSlot + ", " + wardrobeRecipes[wardrobeSlot]);
                        newWardrobe.Add(wardrobeRecipes[wardrobeSlot]);
                        returnValue++;
                        AddOrUpdateSharedColors(equipment.UMARecipeProfile, newColors);
                    }
                }
                if (returnValue != 0) {
                    avatarDefinition.Wardrobe = newWardrobe.ToArray();
                    avatarDefinition.Colors = newColors.ToArray();
                }
            }
            return returnValue;
        }

        public void AddOrUpdateSharedColors(UMARecipeProfile uMARecipeProfile, List<SharedColorDef> sharedColorDefs) {
            foreach (SharedColorNode sharedColorNode in uMARecipeProfile.SharedColors) {
                
                // attempt to find the color in the existing list of sharedColorDefs
                bool foundColor = false;
                foreach (SharedColorDef sharedColorDef in sharedColorDefs) {
                    if (sharedColorDef.name == sharedColorNode.SharedColorname) {
                        sharedColorDef.channels[0].mCol = ColorDef.ToUInt(sharedColorNode.Color);
                        //Debug.Log("Setting shared color " + sharedColorNode.SharedColorname + " to " + sharedColorNode.Color.ToString());
                        foundColor = true;
                        break;
                    }
                }

                // if the color could not be found, add it
                if (foundColor == false) {
                    SharedColorDef sharedColorDef = new SharedColorDef(sharedColorNode.SharedColorname, 1);
                    List<ColorDef> colorchannels = new List<ColorDef>();

                    Color Additive = new Color32(0, 0, 0, 0);
                    colorchannels.Add(new ColorDef(0, ColorDef.ToUInt(sharedColorNode.Color), ColorDef.ToUInt(Additive)));
                    //Debug.Log("Adding shared color " + sharedColorNode.SharedColorname + " as " + sharedColorNode.Color.ToString());

                    sharedColorDef.SetChannels(colorchannels.ToArray());
                    sharedColorDefs.Add(sharedColorDef);
                }
            }
        }

        public void EquipItemModels(CharacterEquipmentManager characterEquipmentManager, Equipment equipment, bool rebuildAppearance = true) {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.EquipItemModels(" + equipment.DisplayName + ", " + rebuildAppearance + ")");

            if (equipment.UMARecipeProfile?.UMARecipes != null
            && equipment.UMARecipeProfile.UMARecipes.Count > 0
            && dynamicCharacterAvatar != null) {
                //Debug.Log("EquipmentManager.HandleItemUMARecipe(): " + newItem.DisplayName);
                // Put the item in the UMA slot on the UMA character
                //Debug.Log("Putting " + newItem.UMARecipe.name + " in slot " + newItem.UMARecipe.wardrobeSlot);
                
                // create a dictionary to ensure the last item valid for a slot in the list is the one that gets set
                Dictionary<string, string> wardrobeRecipes = new Dictionary<string, string>();

                foreach (UMATextRecipe uMARecipe in equipment.UMARecipeProfile.UMARecipes) {
                    if (uMARecipe != null && uMARecipe.compatibleRaces.Contains(dynamicCharacterAvatar.activeRace.name)) {
                        //Debug.Log(unitController.gameObject.name + ".UMAModelController.EquipItemModels(): Adding: " + uMARecipe.wardrobeSlot + ", " + uMARecipe.name);
                        if (equipment.UMARecipeProfile.SharedColors != null && equipment.UMARecipeProfile.SharedColors.Count > 0) {
                            foreach (SharedColorNode sharedColorNode in equipment.UMARecipeProfile.SharedColors) {
                                dynamicCharacterAvatar.SetColor(sharedColorNode.SharedColorname, sharedColorNode.Color);
                            }
                        }
                        if (wardrobeRecipes.ContainsKey(uMARecipe.wardrobeSlot) == false) {
                            wardrobeRecipes.Add(uMARecipe.wardrobeSlot, uMARecipe.name);
                        } else {
                            wardrobeRecipes[uMARecipe.wardrobeSlot] = uMARecipe.name;
                        }
                    }
                }
                foreach (string wardrobeSlot in wardrobeRecipes.Keys) {
                    //Debug.Log(unitController.gameObject.name + ".UMAModelController.EquipItemModels(): SetSlot: " + wardrobeSlot + ", " + wardrobeRecipes[wardrobeSlot]);
                    dynamicCharacterAvatar.SetSlot(wardrobeSlot, wardrobeRecipes[wardrobeSlot]);
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

        /*
        public string GetAppearanceSettings() {
            if (dynamicCharacterAvatar != null) {
                return dynamicCharacterAvatar.GetCurrentRecipe();
            }
            return string.Empty;
        }
        */

        public void SaveAppearanceSettings() {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.SaveAppearanceSettings()");
            if (dynamicCharacterAvatar != null) {
                //saveManager.SaveRecipeString(dynamicCharacterAvatar.GetCurrentRecipe());
                saveManager.SaveRecipeString(GetAppearanceString());
            }
        }

        public string GetAppearanceString() {
            if (dynamicCharacterAvatar != null) {
                return dynamicCharacterAvatar.GetAvatarDefinition(true).ToCompressedString("|");
            }
            return string.Empty;
        }

        public void RebuildModelAppearance() {
            if (dynamicCharacterAvatar != null) {
                //dynamicCharacterAvatar.ClearSlots();
            }

            foreach (Equipment equipment in unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.CurrentEquipment.Values) {
                //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): ask to equip: " + equipment.DisplayName);
                if (equipment != null) {
                    EquipItemModels(unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager, equipment, false);
                }
            }
            BuildModelAppearance();
        }

        public void BuildModelAppearance() {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.BuildModelAppearance()");
            if (dynamicCharacterAvatar != null) {
                //Debug.Log(unitController.gameObject.name + ".UMAModelController.BuildModelAppearance() : " + dynamicCharacterAvatar.GetCurrentRecipe());
                buildInProgress = true;
                dynamicCharacterAvatar.BuildCharacter();
            }
        }

        public void ResetSettings() {
            //Debug.Log(unitController.gameObject.name + ".UMAModelController.ResetSettings()");
            if (dynamicCharacterAvatar != null) {

                // attempt clear expression player
                expressionPlayer = dynamicCharacterAvatar.gameObject.GetComponent<UMAExpressionPlayer>();
                if (expressionPlayer != null) {
                    //expressionPlayer.enabled = false;
                    GameObject.Destroy(expressionPlayer);
                }
                //dynamicCharacterAvatar.umaAdditionalRecipes = new UMARecipeBase[0];

                /*
                dynamicCharacterAvatar.ClearSlots();
                Debug.Log(unitController.gameObject.name + ".UMAModelController.ResetSettings(): Restoring cached body colors");
                dynamicCharacterAvatar.RestoreCachedBodyColors(false, true);
                dynamicCharacterAvatar.LoadDefaultWardrobe();
                // doing the rebuild on despawn so there isn't a frame with this appearance until a rebuild happens when re-using the avatar
                // testing - see if we don't get extra handleCharacterUpdated after respawn
                */

                // testing code to make resetting character compatible with avatar definition usage
                dynamicCharacterAvatar.LoadAvatarDefinition(originalAvatarDefinition, true);
                BuildModelAppearance();
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