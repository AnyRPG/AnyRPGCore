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

    public class UMAAppearanceEditorPanelController : AppearancePanel {

        [Header("UMA")]

        [SerializeField]
        protected GameObject mainButtonsArea = null;

        [SerializeField]
        protected UINavigationListVertical mainOptionsNavigationController = null;

        [Header("Main Buttons")]

        [SerializeField]
        protected HighlightButton hairButton = null;

        [SerializeField]
        protected HighlightButton eyesButton = null;

        [SerializeField]
        protected HighlightButton faceButton = null;

        [SerializeField]
        protected HighlightButton bodyButton = null;

        [Header("Option Areas")]

        [SerializeField]
        protected UINavigationListVertical hairOptionsArea = null;

        [SerializeField]
        protected ColorSelectionController hairColorSelectionController = null;

        [SerializeField]
        protected GameObject hairStyleLabel = null;

        [SerializeField]
        protected UINavigationListVertical eyeOptionsArea = null;

        [SerializeField]
        protected ColorSelectionController eyeColorSelectionController = null;

        [SerializeField]
        protected UINavigationListVertical faceOptionsArea = null;

        [SerializeField]
        protected GameObject eyebrowsLabel = null;

        [SerializeField]
        protected GameObject beardLabel = null;

        [SerializeField]
        protected UINavigationListVertical bodyOptionsArea = null;

        [SerializeField]
        protected ColorSelectionController skinColorSelectionController = null;


        [Header("Image")]

        [SerializeField]
        protected Sprite noOptionImage = null;

        [Header("Prefabs")]

        [SerializeField]
        private GameObject imageButtonPrefab;

        [SerializeField]
        private GameObject mixedButtonPrefab;

        [SerializeField]
        private GameObject textButtonPrefab;

        [Header("Color Tables")]

        public SharedColorTable HairColor;
        public SharedColorTable SkinColor;
        public SharedColorTable EyesColor;

        // hold data so changes are not reset on switch between male and female
        protected string maleRecipe = string.Empty;
        protected string femaleRecipe = string.Empty;

        protected Dictionary<string, List<UMATextRecipe>> allRecipes = new Dictionary<string, List<UMATextRecipe>>();
        protected List<UMATextRecipe> hairRecipes = new List<UMATextRecipe>();
        protected List<UMATextRecipe> eyebrowsRecipes = new List<UMATextRecipe>();
        protected List<UMATextRecipe> beardRecipes = new List<UMATextRecipe>();

        protected UMAModelController umaModelController = null;
        protected DynamicCharacterAvatar dynamicCharacterAvatar = null;

        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SaveManager saveManager = null;

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

            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            saveManager = systemGameManager.SaveManager;
        }

        public override void SetupOptions() {
            //Debug.Log("UMACharacterEditorPanelController.SetupOptions()");

            base.SetupOptions();

            CloseOptionsAreas();
            mainButtonsArea.SetActive(false);
            mainNoOptionsArea.SetActive(false);

            umaModelController = characterCreatorManager.PreviewUnitController?.UnitModelController.ModelAppearanceController.GetModelAppearanceController<UMAModelController>();

            if (umaModelController == null) {
                // somehow this panel was opened but the preview model is not configured as an UMA model
                mainNoOptionsArea.SetActive(true);
                return;
            }

            dynamicCharacterAvatar = umaModelController.DynamicCharacterAvatar;

            // there are no options to show if this is not an UMA
            if (dynamicCharacterAvatar == null) {
                mainNoOptionsArea.SetActive(true);
                return;
            }

            CheckAppearance();

            if (characterCreatorManager.PreviewUnitController?.UnitModelController?.ModelReady == true) {
                mainButtonsArea.SetActive(true);
                OpenHairOptionsArea();
                //appearanceButton.HighlightBackground();
                mainOptionsNavigationController.FocusCurrentButton();
            }
        }

        public override void HandleTargetReady() {
            //Debug.Log("UMAAppearanceEditorPanelController.HandleTargetReady()");

            base.HandleTargetReady();

            SetupOptions();
        }

        public virtual void OpenHairOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenAppearanceOptions()");

            // reset areas and buttons
            hairButton.HighlightBackground();
            mainOptionsNavigationController.UnHightlightButtonBackgrounds(hairButton);
            CloseEyeOptionsArea();
            CloseFaceOptionsArea();
            CloseBodyOptionsArea();

            hairOptionsArea.gameObject.SetActive(true);
        }

        public void CloseHairOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseAppearanceOptions()");
            hairOptionsArea.gameObject.SetActive(false);
        }

        public virtual void OpenEyeOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenEyesOptionsArea()");

            // reset areas and buttons
            eyesButton.HighlightBackground();
            mainOptionsNavigationController.UnHightlightButtonBackgrounds(eyesButton);
            CloseHairOptionsArea();
            CloseFaceOptionsArea();
            CloseBodyOptionsArea();

            eyeOptionsArea.gameObject.SetActive(true);
        }

        public void CloseEyeOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseColorsOptionsArea()");
            eyeOptionsArea.gameObject.SetActive(false);
        }

        public virtual void OpenFaceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenEyesOptionsArea()");

            // reset areas and buttons
            faceButton.HighlightBackground();
            mainOptionsNavigationController.UnHightlightButtonBackgrounds(faceButton);
            CloseEyeOptionsArea();
            CloseHairOptionsArea();
            CloseBodyOptionsArea();

            faceOptionsArea.gameObject.SetActive(true);
        }

        public void CloseFaceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseColorsOptionsArea()");
            faceOptionsArea.gameObject.SetActive(false);
        }

        public virtual void OpenBodyOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenEyesOptionsArea()");

            // reset areas and buttons
            bodyButton.HighlightBackground();
            mainOptionsNavigationController.UnHightlightButtonBackgrounds(bodyButton);
            CloseEyeOptionsArea();
            CloseHairOptionsArea();
            CloseFaceOptionsArea();

            bodyOptionsArea.gameObject.SetActive(true);
        }

        public void CloseBodyOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseColorsOptionsArea()");
            bodyOptionsArea.gameObject.SetActive(false);
        }

        public void UnHighlightAllButtons() {
            //Debug.Log("CharacterCreatorPanel.UnHighlightAllButtons()");

            hairButton.UnHighlightBackground();
            eyesButton.UnHighlightBackground();
            faceButton.UnHighlightBackground();
            bodyButton.UnHighlightBackground();
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            uIManager.characterCreatorWindow.CloseWindow();
        }

        public void InitializeSkinColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeSkinColors()");

            skinColorSelectionController.Configure(systemGameManager);
            skinColorSelectionController.Setup(dynamicCharacterAvatar, "Skin", bodyOptionsArea.gameObject, SkinColor);
        }

        public void InitializeHairColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeHairColors()");

            hairColorSelectionController.Configure(systemGameManager);
            hairColorSelectionController.Setup(dynamicCharacterAvatar, "Hair", hairOptionsArea.gameObject, HairColor);
        }

        public void InitializeEyeColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeEyeColors()");

            eyeColorSelectionController.Configure(systemGameManager);
            eyeColorSelectionController.Setup(dynamicCharacterAvatar, "Eyes", eyeOptionsArea.gameObject, EyesColor);
        }

        /*
        public void ColorsClick() {
            Debug.Log("CharacterAppearancePanel.ColorsClick()");

            CleanupDynamicMenus();
            GameObject setupParent = null;

            foreach (UMA.OverlayColorData overlayColorData in dynamicCharacterAvatar.CurrentSharedColors) {
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

                availableColorsHandler.Setup(dynamicCharacterAvatar, overlayColorData.name, setupParent, currColors);

                // delete next part?

                Text txt = go.GetComponentInChildren<Text>();
                txt.text = overlayColorData.name;
                go.transform.SetParent(setupParent.transform);
                //go.transform.SetParent(SlotPanel.transform);

                availableColorsHandler.OnClick();

            }
        }

        */

        public override void ProcessBeforeSetMale() {
            base.ProcessBeforeSetMale();
            femaleRecipe = umaModelController.GetAppearanceString();
        }

        public override void ProcessBeforeSetFemale() {
            base.ProcessBeforeSetFemale();
            maleRecipe = umaModelController.GetAppearanceString();
        }

        public override void ProcessSetMale() {
            //Debug.Log("CharacterCreatorPanel.ProcessSetMale()");

            base.ProcessSetMale();

            if (maleRecipe != string.Empty) {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): maleRecipe != string.Empty");
                //dynamicCharacterAvatar.ChangeRace("HumanMale");
                //characterCreatorManager.PreviewUnitController.UnitModelController.LoadSavedAppearanceSettings(maleRecipe, true);
                umaModelController.SetAppearance(maleRecipe);
            } else {
                //Debug.Log("CharacterCreatorPanel.SetMale(): maleRecipe == string.Empty");
                //characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ChangeRace("HumanMale");
                //umaModelController.SetAvatarDefinitionRace("HumanMale");
                umaModelController.PreloadEquipmentModels(true);
                umaModelController.ReloadAvatarDefinition();
            }
        }

        public override void ProcessSetFemale() {
            //Debug.Log("UMAAppearancePanel.ProcessSetFemale()");

            base.ProcessSetFemale();

            if (femaleRecipe != string.Empty) {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): femaleRecipe != string.Empty");
                //dynamicCharacterAvatar.ChangeRace("HumanFemale");
                //characterCreatorManager.PreviewUnitController.UnitModelController.LoadSavedAppearanceSettings(femaleRecipe, true);
                umaModelController.SetAppearance(femaleRecipe);
            } else {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): femaleRecipe == string.Empty");
                //characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ChangeRace("HumanFemale");
                //umaModelController.SetAvatarDefinitionRace("HumanFemale");
                umaModelController.PreloadEquipmentModels(true);
                umaModelController.ReloadAvatarDefinition();
            }
        }

        public void CloseOptionsAreas() {
            //Debug.Log("CharacterCreatorPanel.CloseOptionsAreas()");
            
            CloseHairOptionsArea();
            CloseEyeOptionsArea();
            CloseFaceOptionsArea();
            CloseBodyOptionsArea();
        }

        public void CheckAppearance() {
            Debug.Log("UMAAppearanceEditorPanel.CheckAppearance()");

            if (dynamicCharacterAvatar == null) {
                Debug.Log("UMAAppearanceEditorPanel.CheckAppearance(): umaAvatar is null!!!!");
            }

            // clear old buttons
            hairOptionsArea.DeleteActiveButtons();
            eyeOptionsArea.DeleteActiveButtons();
            faceOptionsArea.DeleteActiveButtons();
            bodyOptionsArea.DeleteActiveButtons();

            allRecipes = dynamicCharacterAvatar.AvailableRecipes;

            // Hair
            if (allRecipes.ContainsKey("Hair")) {
                hairStyleLabel.SetActive(true);
                hairRecipes = allRecipes["Hair"];
                PopulateHairOptions(hairRecipes);
            } else {
                hairStyleLabel.SetActive(false);
            }
            InitializeHairColors();
            hairOptionsArea.UpdateNavigationList();

            // eyes
            InitializeEyeColors();
            eyeOptionsArea.UpdateNavigationList();


            // Eyebrows
            if (allRecipes.ContainsKey("Eyebrows")) {
                eyebrowsLabel.SetActive(true);
                eyebrowsRecipes = allRecipes["Eyebrows"];
                PopulateEyebrowOptions(eyebrowsRecipes);
            } else {
                eyebrowsLabel.SetActive(false);
            }

            // Beard
            if (allRecipes.ContainsKey("Beard")) {
                beardLabel.SetActive(true);
                beardRecipes = allRecipes["Beard"];
                PopulateBeardOptions(beardRecipes);
            } else {
                beardLabel.SetActive(false);
            }
            faceOptionsArea.UpdateNavigationList();

            // body
            InitializeSkinColors();
            bodyOptionsArea.UpdateNavigationList();

        }

        private void PopulateHairOptions(List<UMATextRecipe> hairRecipes) {
            int index = 2;
            UMAOptionChoiceButton optionChoiceButton = AddOptionListButton(hairOptionsArea, "Hair", noOptionImage, "None", "");
            optionChoiceButton.transform.SetSiblingIndex(1);
            foreach (UMATextRecipe recipeName in hairRecipes) {
                optionChoiceButton = AddOptionListButton(hairOptionsArea, "Hair", null, recipeName.DisplayValue, recipeName.DisplayValue);
                optionChoiceButton.transform.SetSiblingIndex(index);
                index++;
            }
        }

        private void PopulateEyebrowOptions(List<UMATextRecipe> eyebrowRecipes) {
            int index = eyebrowsLabel.transform.GetSiblingIndex() + 1;
            UMAOptionChoiceButton optionChoiceButton = AddOptionListButton(faceOptionsArea, "Eyebrows", noOptionImage, "None", "");
            optionChoiceButton.transform.SetSiblingIndex(index);
            index++;
            foreach (UMATextRecipe recipeName in eyebrowRecipes) {
                optionChoiceButton = AddOptionListButton(faceOptionsArea, "Eyebrows", null, recipeName.DisplayValue, recipeName.DisplayValue);
                optionChoiceButton.transform.SetSiblingIndex(index);
                index++;
            }
        }

        private void PopulateBeardOptions(List<UMATextRecipe> beardRecipes) {
            int index = beardLabel.transform.GetSiblingIndex() + 1;
            UMAOptionChoiceButton optionChoiceButton = AddOptionListButton(faceOptionsArea, "Beard", noOptionImage, "None", "");
            optionChoiceButton.transform.SetSiblingIndex(index);
            index++;
            foreach (UMATextRecipe recipeName in beardRecipes) {
                AddOptionListButton(faceOptionsArea, "Beard", null, recipeName.DisplayValue, recipeName.DisplayValue);
                optionChoiceButton.transform.SetSiblingIndex(index);
                index++;
            }
        }

        private UMAOptionChoiceButton AddOptionListButton(UINavigationListVertical listOptionsArea, string groupName, Sprite optionImage, string displayName, string optionChoice) {
            //Debug.Log("SwappableMeshAppearancePanelController.AddOptionListButton()");

            UMAOptionChoiceButton optionChoiceButton = null;
            if (optionImage != null) {
                optionChoiceButton = objectPooler.GetPooledObject(mixedButtonPrefab, listOptionsArea.transform).GetComponent<UMAOptionChoiceButton>();
            } else {
                optionChoiceButton = objectPooler.GetPooledObject(textButtonPrefab, listOptionsArea.transform).GetComponent<UMAOptionChoiceButton>();
            }
            if (optionChoiceButton != null) {
                optionChoiceButton.Configure(systemGameManager);
                optionChoiceButton.ConfigureButton(this, groupName, optionImage, displayName, optionChoice);
                listOptionsArea.AddActiveButton(optionChoiceButton);

                // highlight the button if it is already the selected choice for the group
                if (dynamicCharacterAvatar.GetWardrobeItemName(groupName) == optionChoice) {
                    optionChoiceButton.HighlightOutline();
                }
            }
            return optionChoiceButton;
        }

        public void SetRecipe(UMAOptionChoiceButton optionChoiceButton, string groupName, string optionChoice) {
            Debug.Log("UMAAppearanceEditorPanel.SetRecipe(" + groupName + ", " + optionChoice + ")");

            if (optionChoice == string.Empty) {
                dynamicCharacterAvatar.ClearSlot(groupName);
            }
            dynamicCharacterAvatar.SetSlot(groupName, GetRecipeName(optionChoice, allRecipes[groupName]));
            RebuildUMA();
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

        public void RebuildUMA() {
            //Debug.Log("CharacterCreatorPanel.RebuildUMA()");

            umaModelController.BuildModelAppearance();
        }

    }

}