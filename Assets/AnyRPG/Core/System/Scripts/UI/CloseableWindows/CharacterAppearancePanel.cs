using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;
using UMA.CharacterSystem.Examples;

namespace AnyRPG {

    public class CharacterAppearancePanel : WindowContentController {

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [Header("Appearance")]

        [SerializeField]
        protected GameObject mainButtonsArea = null;

        [SerializeField]
        protected GameObject mainNoOptionsArea = null;

        [SerializeField]
        protected HighlightButton appearanceButton = null;

        [SerializeField]
        protected HighlightButton colorsButton = null;

        [SerializeField]
        protected HighlightButton hairColorsButton = null;

        [SerializeField]
        protected HighlightButton skinColorsButton = null;

        [SerializeField]
        protected HighlightButton eyesColorsButton = null;

        [SerializeField]
        protected HighlightButton sexButton = null;

        [SerializeField]
        protected HighlightButton maleButton = null;

        [SerializeField]
        protected HighlightButton femaleButton = null;

        [SerializeField]
        protected GameObject appearanceOptionsArea = null;

        [SerializeField]
        protected GameObject hairAppearanceOptionsArea = null;

        [SerializeField]
        protected GameObject eyebrowsAppearanceOptionsArea = null;

        [SerializeField]
        protected GameObject beardAppearanceOptionsArea = null;

        [SerializeField]
        protected GameObject colorsOptionsArea = null;

        [SerializeField]
        protected GameObject hairColorsOptionsArea = null;

        [SerializeField]
        protected GameObject skinColorsOptionsArea = null;

        [SerializeField]
        protected GameObject eyesColorsOptionsArea = null;

        [SerializeField]
        protected GameObject sexOptionsArea = null;

        [SerializeField]
        protected TMP_Dropdown hairAppearanceDropdown = null;

        [SerializeField]
        protected TMP_Dropdown eyebrowsAppearanceDropdown = null;

        [SerializeField]
        protected TMP_Dropdown beardAppearanceDropdown = null;


        public GameObject ColorPrefab;

        public SharedColorTable HairColor;
        public SharedColorTable SkinColor;
        public SharedColorTable EyesColor;
        public SharedColorTable ClothingColor;

        // hold data so changes are not reset on switch between male and female
        protected string maleRecipe = string.Empty;
        protected string femaleRecipe = string.Empty;

        protected List<UMATextRecipe> hairRecipes = new List<UMATextRecipe>();
        protected List<UMATextRecipe> eyebrowsRecipes = new List<UMATextRecipe>();
        protected List<UMATextRecipe> beardRecipes = new List<UMATextRecipe>();

        // game manager references
        protected CharacterCreatorManager characterCreatorManager = null;
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SaveManager saveManager = null;

        public GameObject MainNoOptionsArea { get => mainNoOptionsArea; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //appearanceButton.Configure(systemGameManager);
            //colorsButton.Configure(systemGameManager);
            //hairColorsButton.Configure(systemGameManager);
            //skinColorsButton.Configure(systemGameManager);
            //eyesColorsButton.Configure(systemGameManager);
            //sexButton.Configure(systemGameManager);
            //maleButton.Configure(systemGameManager);
            //femaleButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            //Debug.Log("CharacterAppearancePanel.SetGameManagerReferences()");
            base.SetGameManagerReferences();

            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            saveManager = systemGameManager.SaveManager;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }

        protected void InitializeSexButtons() {
            //Debug.Log("CharacterCreatorPanel.InitializeSexButtons()");

            if (characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.activeRace.name == "HumanMaleDCS"
                || characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.activeRace.name == "HumanMale") {
                if (maleButton != null) {
                    maleButton.HighlightBackground();
                }
                if (femaleButton != null) {
                    femaleButton.DeSelect();
                }
            } else {
                if (maleButton != null) {
                    maleButton.DeSelect();
                }
                if (femaleButton != null) {
                    femaleButton.HighlightBackground();
                }
            }
        }
        
        /*
        public void TargetReadyCallback() {
            //Debug.Log("NewGameCharacterPanelController.TargetReadyCallback()");


            CloseOptionsAreas();
            OpenAppearanceOptionsArea();
            InitializeSexButtons();
        }
        */

        public virtual void OpenAppearanceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenAppearanceOptions()");

            // reset areas and buttons
            uINavigationControllers[0].UnHightlightButtons(appearanceButton);
            CloseColorsOptionsArea();
            CloseSexOptionsArea();

            // configure appearance options display
            appearanceButton.Select();
            appearanceOptionsArea.gameObject.SetActive(true);

            // get valid appearance options values
            CheckAppearance();
        }

        public void CloseAppearanceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseAppearanceOptions()");
            appearanceOptionsArea.gameObject.SetActive(false);
        }

        public virtual void OpenColorsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenColorsOptionsArea()");

            // reset areas and buttons
            uINavigationControllers[0].UnHightlightButtons(colorsButton);
            CloseAppearanceOptionsArea();
            CloseSexOptionsArea();

            // configure colors options display
            //colorsButton.Select();
            colorsOptionsArea.gameObject.SetActive(true);
            hairColorsButton.HighlightBackground();
            OpenHairColorsOptionsArea();
        }

        public void CloseColorsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseColorsOptionsArea()");
            colorsOptionsArea.gameObject.SetActive(false);
        }

        public void CloseSexOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseSexOptionsArea()");
            sexOptionsArea.gameObject.SetActive(false);
        }

        public virtual void OpenSexOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenSexOptionsArea()");

            // reset areas and buttons
            uINavigationControllers[0].UnHightlightButtons(sexButton);
            CloseColorsOptionsArea();
            CloseAppearanceOptionsArea();

            // configure sex options display
            //sexButton.HighlightBackground();
            sexOptionsArea.gameObject.SetActive(true);
            InitializeSexButtons();
        }

        public void CloseEyesColorsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseEyesColorsOptionsArea()");
            eyesColorsOptionsArea.gameObject.SetActive(false);
        }

        public void OpenEyesColorsOptionsArea() {
            Debug.Log("CharacterAppearancePanel.OpenEyesColorsOptionsArea()");

            CloseColorsOptionsAreas();
            uINavigationControllers[2].UnHightlightButtons(eyesColorsButton);
            //UnHighlightColorsButtons();

            //eyesColorsButton.Select();
            InitializeEyesColors();
            eyesColorsOptionsArea.gameObject.SetActive(true);
        }

        public void CloseColorsOptionsAreas() {
            //Debug.Log("CharacterCreatorPanel.CloseColorsOptionsAreas()");
            CloseEyesColorsOptionsArea();
            CloseHairColorsOptionsArea();
            CloseSkinColorsOptionsArea();
        }

        public void CloseSkinColorsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseSkinColorsOptionsArea()");
            skinColorsOptionsArea.gameObject.SetActive(false);
        }

        public void OpenSkinColorsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenSkinColorsOptionsArea()");

            CloseColorsOptionsAreas();
            uINavigationControllers[2].UnHightlightButtons(skinColorsButton);
            //UnHighlightColorsButtons();

            //skinColorsButton.Select();
            InitializeSkinColors();
            skinColorsOptionsArea.gameObject.SetActive(true);
        }

        public void CloseHairColorsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseHairColorsOptionsArea()");
            hairColorsOptionsArea.gameObject.SetActive(false);
        }

        public void OpenHairColorsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenHairColorsOptionsArea()");

            CloseColorsOptionsAreas();
            uINavigationControllers[2].UnHightlightButtons(hairColorsButton);
            //UnHighlightColorsButtons();

            //hairColorsButton.Select();
            InitializeHairColors();
            hairColorsOptionsArea.gameObject.SetActive(true);
        }

        public void CloseHairAppearanceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseHairAppearanceOptionsArea()");
            hairAppearanceOptionsArea.gameObject.SetActive(false);
        }

        public void OpenHairAppearanceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenHairAppearanceOptionsArea()");
            hairAppearanceOptionsArea.gameObject.SetActive(true);
        }

        public void CloseEyebrowsAppearanceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseEyebrowsAppearanceOptionsArea()");
            eyebrowsAppearanceOptionsArea.gameObject.SetActive(false);
        }

        public void OpenEyebrowsAppearanceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenEyebrowsAppearanceOptionsArea()");
            eyebrowsAppearanceOptionsArea.gameObject.SetActive(true);
        }

        public void CloseBeardAppearanceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseBeardAppearanceOptionsArea()");
            beardAppearanceOptionsArea.gameObject.SetActive(false);
        }

        public void OpenBeardAppearanceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenBeardAppearanceOptionsArea()");
            beardAppearanceOptionsArea.gameObject.SetActive(true);
        }

        public void UnHighlightAllButtons() {
            //Debug.Log("CharacterCreatorPanel.UnHighlightAllButtons()");

            appearanceButton.UnHighlightBackground();
            colorsButton.UnHighlightBackground();
            sexButton.UnHighlightBackground();
            hairColorsButton.DeSelect();
            skinColorsButton.DeSelect();
            eyesColorsButton.DeSelect();
        }

        public void UnHighlightColorsButtons() {
            //Debug.Log("CharacterCreatorPanel.UnHighlightColorsButtons()");
            hairColorsButton.DeSelect();
            skinColorsButton.DeSelect();
            eyesColorsButton.DeSelect();
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            uIManager.characterCreatorWindow.CloseWindow();
        }

        public void InitializeSkinColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeSkinColors()");
            ColorSelectionController colorSelectionController = skinColorsOptionsArea.GetComponent<ColorSelectionController>();
            colorSelectionController.Configure(systemGameManager);
            colorSelectionController.Setup(characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar, "Skin", skinColorsOptionsArea, SkinColor);
            /*
                    foreach (UMA.OverlayColorData overlayColorData in umaAvatar.CurrentSharedColors) {
                        Debug.Log("CharacterCreatorPanel.InitializeSkinColors(): overlayColorData.name: " + overlayColorData.name);

                        if (overlayColorData.name.ToLower() == "skin") {
                            ColorSelectionController colorSelectionController = skinColorsOptionsArea.GetComponentInChildren<ColorSelectionController>();
                            colorSelectionController.Setup(umaAvatar, overlayColorData.name, skinColorsOptionsArea, SkinColor);
                        }
                    }
                    */
        }

        public void InitializeHairColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeHairColors()");
            ColorSelectionController colorSelectionController = hairColorsOptionsArea.GetComponent<ColorSelectionController>();
            colorSelectionController.Configure(systemGameManager);
            colorSelectionController.Setup(characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar, "Hair", hairColorsOptionsArea, HairColor);
        }

        public void InitializeEyesColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeEyesColors()");
            ColorSelectionController colorSelectionController = eyesColorsOptionsArea.GetComponent<ColorSelectionController>();
            colorSelectionController.Configure(systemGameManager);
            colorSelectionController.Setup(characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar, "Eyes", eyesColorsOptionsArea, EyesColor);
        }

        public void ColorsClick() {
            Debug.Log("CharacterAppearancePanel.ColorsClick()");

            CleanupDynamicMenus();
            GameObject setupParent = null;

            foreach (UMA.OverlayColorData overlayColorData in characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.CurrentSharedColors) {
                //Debug.Log("CharacterCreatorPanel.ColorsClick(): overlayColorData.name: " + overlayColorData.name);

                GameObject go = objectPooler.GetPooledObject(ColorPrefab);
                AvailableColorsHandler availableColorsHandler = go.GetComponent<AvailableColorsHandler>();

                SharedColorTable currColors = ClothingColor;

                if (overlayColorData.name.ToLower() == "skin") {
                    currColors = SkinColor;
                    setupParent = skinColorsOptionsArea;
                } else if (overlayColorData.name.ToLower() == "hair") {
                    currColors = HairColor;
                    setupParent = hairColorsOptionsArea;
                } else if (overlayColorData.name.ToLower() == "eyes") {
                    currColors = EyesColor;
                    setupParent = eyesColorsOptionsArea;
                } else {
                    //Debug.Log("CharacterCreatorPanel.ColorsClick(): setupParent.name: " + setupParent.name + " does not match skin or hair or eyes");

                }
                //Debug.Log("CharacterCreatorPanel.ColorsClick(): setupParent.name: " + setupParent.name);

                availableColorsHandler.Setup(characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar, overlayColorData.name, setupParent, currColors);

                // delete next part?

                Text txt = go.GetComponentInChildren<Text>();
                txt.text = overlayColorData.name;
                go.transform.SetParent(setupParent.transform);
                //go.transform.SetParent(SlotPanel.transform);

                availableColorsHandler.OnClick();

            }
        }

        private void CleanupDynamicMenus() {

            /*
            foreach (Transform t in SlotPanel.transform) {
                UMAUtils.DestroySceneObject(t.gameObject);
            }
            foreach (Transform t in WardrobePanel.transform) {
                UMAUtils.DestroySceneObject(t.gameObject);
            }
            */
        }

        public void SetMale() {
            //Debug.Log("CharacterCreatorPanel.SetMale()");

            if (characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.activeRace.name == "HumanMaleDCS"
                || characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.activeRace.name == "HumanMale") {
                //Debug.Log("CharacterCreatorPanel.SetMale(): already male. returning");
                return;
            }
            femaleRecipe = characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.GetAppearanceString();
            femaleButton.UnHighlightBackground();
            //maleButton.HighlightBackground();
            if (maleRecipe != string.Empty) {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): maleRecipe != string.Empty");
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ChangeRace("HumanMaleDCS");
                //characterCreatorManager.PreviewUnitController.UnitModelController.LoadSavedAppearanceSettings(maleRecipe, true);
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.SetAppearance(maleRecipe);
            } else {
                //Debug.Log("CharacterCreatorPanel.SetMale(): maleRecipe == string.Empty");
                //characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ChangeRace("HumanMaleDCS");
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.SetAvatarDefinitionRace("HumanMaleDCS");
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.PreloadEquipmentModels(true);
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.ReloadAvatarDefinition();
            }
        }

        public void SetFemale() {
            //Debug.Log("CharacterCreatorPanel.SetFemale()");

            if (characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.activeRace.name == "HumanFemaleDCS"
                || characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.activeRace.name == "HumanFemale") {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): already female. returning");
                return;
            }
            //maleRecipe = characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.GetCurrentRecipe();
            maleRecipe = characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.GetAppearanceString();
            maleButton.UnHighlightBackground();
            //femaleButton.Select();
            if (femaleRecipe != string.Empty) {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): femaleRecipe != string.Empty");
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ChangeRace("HumanFemaleDCS");
                //characterCreatorManager.PreviewUnitController.UnitModelController.LoadSavedAppearanceSettings(femaleRecipe, true);
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.SetAppearance(femaleRecipe);
            } else {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): femaleRecipe == string.Empty");
                //characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ChangeRace("HumanFemaleDCS");
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.SetAvatarDefinitionRace("HumanFemaleDCS");
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.PreloadEquipmentModels(true);
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.ReloadAvatarDefinition();
            }
        }

        public void CloseOptionsAreas() {
            //Debug.Log("CharacterCreatorPanel.CloseOptionsAreas()");
            /*
            CloseBeardAppearanceOptionsArea();
            CloseEyebrowsAppearanceOptionsArea();
            CloseHairAppearanceOptionsArea();
            CloseHairColorsOptionsArea();
            CloseSkinColorsOptionsArea();
            CloseEyesColorsOptionsArea();
            */
            CloseAppearanceOptionsArea();
            CloseColorsOptionsArea();
            CloseSexOptionsArea();
        }

        public void CheckAppearance() {
            //Debug.Log("CharacterCreatorPanel.CheckAppearance()");
            if (characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar == null) {
                Debug.Log("NewGameCharacterPanelController.CheckAppearance(): umaAvatar is null!!!!");
            }
            Dictionary<string, List<UMATextRecipe>> recipes = characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.AvailableRecipes;

            hairAppearanceDropdown.ClearOptions();
            eyebrowsAppearanceDropdown.ClearOptions();
            beardAppearanceDropdown.ClearOptions();

            int counter = 0;
            List<string> options = new List<string>();

            // Hair
            if (recipes.ContainsKey("Hair")) {
                hairRecipes = recipes["Hair"];
                int currentHairIndex = 0;
                counter = 1;
                options.Add("None");
                foreach (UMATextRecipe recipeName in hairRecipes) {
                    //Debug.Log("CharacterCreatorPanel.CheckAppearance(): recipeName.DisplayValue: " + recipeName.DisplayValue + "; recipeName.name: " + recipeName.name);
                    string option = recipeName.DisplayValue;
                    options.Add(option);
                    if (GetRecipeName(option, hairRecipes) == characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.GetWardrobeItemName("Hair")) {
                        currentHairIndex = counter;
                    }
                    counter++;
                }
                hairAppearanceDropdown.AddOptions(options);
                hairAppearanceDropdown.value = currentHairIndex;
                hairAppearanceDropdown.RefreshShownValue();

                options.Clear();
                OpenHairAppearanceOptionsArea();
            } else {
                CloseHairAppearanceOptionsArea();
            }

            // Eyebrows
            if (recipes.ContainsKey("Eyebrows")) {
                eyebrowsRecipes = recipes["Eyebrows"];
                int currentEyebrowsIndex = 0;
                counter = 1;
                options.Add("None");
                foreach (UMATextRecipe recipeName in eyebrowsRecipes) {
                    string option = recipeName.DisplayValue;
                    options.Add(option);
                    if (GetRecipeName(option, eyebrowsRecipes) == characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.GetWardrobeItemName("Eyebrows")) {
                        currentEyebrowsIndex = counter;
                    }
                    counter++;
                }
                eyebrowsAppearanceDropdown.AddOptions(options);
                eyebrowsAppearanceDropdown.value = currentEyebrowsIndex;
                eyebrowsAppearanceDropdown.RefreshShownValue();

                options.Clear();
                OpenEyebrowsAppearanceOptionsArea();
            } else {
                CloseEyebrowsAppearanceOptionsArea();
            }


            // Beard
            if (recipes.ContainsKey("Beard")) {
                beardRecipes = recipes["Beard"];
                int currentBeardIndex = 0;
                counter = 1;
                options.Add("None");
                foreach (UMATextRecipe recipeName in beardRecipes) {
                    string option = recipeName.DisplayValue;
                    options.Add(option);
                    if (GetRecipeName(option, beardRecipes) == characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.GetWardrobeItemName("Beard")) {
                        currentBeardIndex = counter;
                    }
                    counter++;
                }
                beardAppearanceDropdown.AddOptions(options);
                beardAppearanceDropdown.value = currentBeardIndex;
                beardAppearanceDropdown.RefreshShownValue();

                OpenBeardAppearanceOptionsArea();
            } else {
                CloseBeardAppearanceOptionsArea();
            }

        }

        private string GetRecipeName(string recipeDisplayName, List<UMATextRecipe> recipeList) {
            //Debug.Log("CharacterCreatorPanel.GetRecipeName(" + recipeDisplayName + ")");
            foreach (UMATextRecipe umaTextRecipe in recipeList) {
                if (umaTextRecipe.DisplayValue == recipeDisplayName) {
                    return umaTextRecipe.name;
                }
            }
            //Debug.Log("CharacterCreatorPanel.GetRecipeName(" + recipeDisplayName + "): Could not find recipe.  return string.Empty!!!");
            return string.Empty;
        }

        public void SetHair(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetHair(" + dropdownIndex + "): " + hairAppearanceDropdown.options[hairAppearanceDropdown.value].text);
            if (hairAppearanceDropdown.options[hairAppearanceDropdown.value].text == "None") {
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ClearSlot("Hair");
            }
            characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.SetSlot("Hair", GetRecipeName(hairAppearanceDropdown.options[hairAppearanceDropdown.value].text, hairRecipes));
            RebuildUMA();
        }

        public void SetEyebrows(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetEyebrows(" + dropdownIndex + "): " + eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text);
            if (eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text == "None") {
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ClearSlot("Eyebrows");
            }
            characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.SetSlot("Eyebrows", GetRecipeName(eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text, eyebrowsRecipes));
            RebuildUMA();
        }

        public void SetBeard(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetBeard(" + dropdownIndex + "): " + beardAppearanceDropdown.options[beardAppearanceDropdown.value].text);
            if (beardAppearanceDropdown.options[beardAppearanceDropdown.value].text == "None") {
                characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ClearSlot("Beard");
            }
            characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.SetSlot("Beard", GetRecipeName(beardAppearanceDropdown.options[beardAppearanceDropdown.value].text, beardRecipes));
            RebuildUMA();
        }

        public void RebuildUMA() {
            //Debug.Log("CharacterCreatorPanel.RebuildUMA()");
            //Debug.Log("NewGameCharacterPanelController.RebuildUMA(): BuildCharacter(): buildenabled: " + umaAvatar.BuildCharacterEnabled + "; frame: " + Time.frameCount);

            characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.BuildModelAppearance();
        }


    }

}