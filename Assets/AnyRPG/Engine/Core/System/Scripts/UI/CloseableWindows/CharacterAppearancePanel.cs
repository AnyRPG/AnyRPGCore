using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.Examples;
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

        public GameObject MainNoOptionsArea { get => mainNoOptionsArea; }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }

        protected void InitializeSexButtons() {
            //Debug.Log("CharacterCreatorPanel.InitializeSexButtons()");

            if (CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.activeRace.name == "HumanMaleDCS" || CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.activeRace.name == "HumanMale") {
                if (maleButton != null) {
                    maleButton.Select();
                }
                if (femaleButton != null) {
                    femaleButton.DeSelect();
                }
            } else {
                if (maleButton != null) {
                    maleButton.DeSelect();
                }
                if (femaleButton != null) {
                    femaleButton.Select();
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

        public void OpenAppearanceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenAppearanceOptions()");

            // reset areas and buttons
            UnHighlightAllButtons();
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

        public void OpenColorsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenColorsOptionsArea()");

            // reset areas and buttons
            UnHighlightAllButtons();
            CloseAppearanceOptionsArea();
            CloseSexOptionsArea();

            // configure colors options display
            colorsButton.Select();
            colorsOptionsArea.gameObject.SetActive(true);
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

        public void OpenSexOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenSexOptionsArea()");

            // reset areas and buttons
            UnHighlightAllButtons();
            CloseColorsOptionsArea();
            CloseAppearanceOptionsArea();

            // configure sex options display
            sexButton.Select();
            sexOptionsArea.gameObject.SetActive(true);
            InitializeSexButtons();
        }

        public void CloseEyesColorsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseEyesColorsOptionsArea()");
            eyesColorsOptionsArea.gameObject.SetActive(false);
        }

        public void OpenEyesColorsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenEyesColorsOptionsArea()");

            CloseColorsOptionsAreas();
            UnHighlightColorsButtons();

            eyesColorsButton.Select();
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
            UnHighlightColorsButtons();

            skinColorsButton.Select();
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
            UnHighlightColorsButtons();

            hairColorsButton.Select();
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

            appearanceButton.DeSelect();
            colorsButton.DeSelect();
            hairColorsButton.DeSelect();
            skinColorsButton.DeSelect();
            eyesColorsButton.DeSelect();
            sexButton.DeSelect();
        }

        public void UnHighlightColorsButtons() {
            //Debug.Log("CharacterCreatorPanel.UnHighlightColorsButtons()");
            hairColorsButton.DeSelect();
            skinColorsButton.DeSelect();
            eyesColorsButton.DeSelect();
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            SystemWindowManager.MyInstance.characterCreatorWindow.CloseWindow();
        }

        public void InitializeSkinColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeSkinColors()");
            ColorSelectionController colorSelectionController = skinColorsOptionsArea.GetComponent<ColorSelectionController>();
            colorSelectionController.Setup(CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar, "Skin", skinColorsOptionsArea, SkinColor);
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
            colorSelectionController.Setup(CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar, "Hair", hairColorsOptionsArea, HairColor);
        }

        public void InitializeEyesColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeEyesColors()");
            ColorSelectionController colorSelectionController = eyesColorsOptionsArea.GetComponent<ColorSelectionController>();
            colorSelectionController.Setup(CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar, "Eyes", eyesColorsOptionsArea, EyesColor);
        }

        public void ColorsClick() {
            //Debug.Log("CharacterCreatorPanel.ColorsClick()");

            CleanupDynamicMenus();
            GameObject setupParent = null;

            foreach (UMA.OverlayColorData overlayColorData in CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.CurrentSharedColors) {
                //Debug.Log("CharacterCreatorPanel.ColorsClick(): overlayColorData.name: " + overlayColorData.name);

                GameObject go = GameObject.Instantiate(ColorPrefab);
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

                availableColorsHandler.Setup(CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar, overlayColorData.name, setupParent, currColors);

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

            if (CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.activeRace.name == "HumanMaleDCS" || CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.activeRace.name == "HumanMale") {
                //Debug.Log("CharacterCreatorPanel.SetMale(): already male. returning");
                return;
            }
            femaleRecipe = CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.GetCurrentRecipe();
            femaleButton.DeSelect();
            maleButton.Select();
            if (maleRecipe != string.Empty) {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): maleRecipe != string.Empty");
                CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.ChangeRace("HumanMaleDCS");
                SaveManager.MyInstance.LoadUMASettings(maleRecipe, CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar);
            } else {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): maleRecipe == string.Empty");
                CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.ChangeRace("HumanMaleDCS");
            }
        }

        public void SetFemale() {
            //Debug.Log("CharacterCreatorPanel.SetFemale()");

            if (CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.activeRace.name == "HumanFemaleDCS" || CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.activeRace.name == "HumanFemale") {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): already female. returning");
                return;
            }
            maleRecipe = CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.GetCurrentRecipe();
            maleButton.DeSelect();
            femaleButton.Select();
            if (femaleRecipe != string.Empty) {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): femaleRecipe != string.Empty");
                CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.ChangeRace("HumanFemaleDCS");
                SaveManager.MyInstance.LoadUMASettings(femaleRecipe, CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar);
            } else {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): femaleRecipe == string.Empty");
                CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.ChangeRace("HumanFemaleDCS");
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
            if (CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar == null) {
                Debug.Log("NewGameCharacterPanelController.CheckAppearance(): umaAvatar is null!!!!");
            }
            Dictionary<string, List<UMATextRecipe>> recipes = CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.AvailableRecipes;

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
                    if (GetRecipeName(option, hairRecipes) == CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.GetWardrobeItemName("Hair")) {
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
                    if (GetRecipeName(option, eyebrowsRecipes) == CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.GetWardrobeItemName("Eyebrows")) {
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
                    if (GetRecipeName(option, beardRecipes) == CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.GetWardrobeItemName("Beard")) {
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
                CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.ClearSlot("Hair");
            }
            CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.SetSlot("Hair", GetRecipeName(hairAppearanceDropdown.options[hairAppearanceDropdown.value].text, hairRecipes));
            RebuildUMA();
        }

        public void SetEyebrows(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetEyebrows(" + dropdownIndex + "): " + eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text);
            if (eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text == "None") {
                CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.ClearSlot("Eyebrows");
            }
            CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.SetSlot("Eyebrows", GetRecipeName(eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text, eyebrowsRecipes));
            RebuildUMA();
        }

        public void SetBeard(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetBeard(" + dropdownIndex + "): " + beardAppearanceDropdown.options[beardAppearanceDropdown.value].text);
            if (beardAppearanceDropdown.options[beardAppearanceDropdown.value].text == "None") {
                CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.ClearSlot("Beard");
            }
            CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.SetSlot("Beard", GetRecipeName(beardAppearanceDropdown.options[beardAppearanceDropdown.value].text, beardRecipes));
            RebuildUMA();
        }

        public void RebuildUMA() {
            //Debug.Log("CharacterCreatorPanel.RebuildUMA()");
            //Debug.Log("NewGameCharacterPanelController.RebuildUMA(): BuildCharacter(): buildenabled: " + umaAvatar.BuildCharacterEnabled + "; frame: " + Time.frameCount);

            CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.BuildCharacter();
        }


    }

}