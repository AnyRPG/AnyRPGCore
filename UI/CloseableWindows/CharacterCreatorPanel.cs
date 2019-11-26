using AnyRPG;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.Examples;
using UMA.CharacterSystem;
using UMA.CharacterSystem.Examples;

namespace AnyRPG {

    public class CharacterCreatorPanel : WindowContentController {

        #region Singleton
        private static CharacterCreatorPanel instance;

        public static CharacterCreatorPanel MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CharacterCreatorPanel>();
                }

                return instance;
            }
        }

        #endregion

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private HighlightButton appearanceButton;

        [SerializeField]
        private HighlightButton colorsButton;

        [SerializeField]
        private HighlightButton hairColorsButton;

        [SerializeField]
        private HighlightButton skinColorsButton;

        [SerializeField]
        private HighlightButton eyesColorsButton;

        [SerializeField]
        private HighlightButton sexButton;

        [SerializeField]
        private HighlightButton maleButton;

        [SerializeField]
        private HighlightButton femaleButton;

        [SerializeField]
        private GameObject appearanceOptionsArea;

        [SerializeField]
        private GameObject hairAppearanceOptionsArea;

        [SerializeField]
        private GameObject eyebrowsAppearanceOptionsArea;

        [SerializeField]
        private GameObject beardAppearanceOptionsArea;

        [SerializeField]
        private GameObject colorsOptionsArea;

        [SerializeField]
        private GameObject hairColorsOptionsArea;

        [SerializeField]
        private GameObject skinColorsOptionsArea;

        [SerializeField]
        private GameObject eyesColorsOptionsArea;

        [SerializeField]
        private GameObject sexOptionsArea;

        [SerializeField]
        private Dropdown hairAppearanceDropdown;

        [SerializeField]
        private Dropdown eyebrowsAppearanceDropdown;

        [SerializeField]
        private Dropdown beardAppearanceDropdown;

        [SerializeField]
        private AnyRPGCharacterPreviewCameraController previewCameraController;

        [SerializeField]
        private LayoutElement panelLayoutElement;

        public GameObject ColorPrefab;

        public SharedColorTable HairColor;
        public SharedColorTable SkinColor;
        public SharedColorTable EyesColor;
        public SharedColorTable ClothingColor;

        // hold data so changes are not reset on switch between male and female
        private string maleRecipe = string.Empty;
        private string femaleRecipe = string.Empty;

        private DynamicCharacterAvatar umaAvatar;

        private List<UMATextRecipe> hairRecipes = new List<UMATextRecipe>();
        private List<UMATextRecipe> eyebrowsRecipes = new List<UMATextRecipe>();
        private List<UMATextRecipe> beardRecipes = new List<UMATextRecipe>();

        public AnyRPGCharacterPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            umaAvatar = null;
            previewCameraController.ClearTarget();
            CharacterCreatorManager.MyInstance.HandleCloseWindow();
            OnCloseWindow(this);
            // close interaction window too for smoother experience
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnOpenWindow()");

            SetPreviewTarget();
            panelLayoutElement.preferredWidth = Screen.width;
            panelLayoutElement.preferredHeight = Screen.height;
            //Debug.Log("MainMapController.OnOpenWindow(); panelLayoutElement.preferredWidth: " + panelLayoutElement.preferredWidth);
            //Debug.Log("MainMapController.OnOpenWindow(); panelLayoutElement.preferredHeight: " + panelLayoutElement.preferredHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());

        }

        private void InitializeSexButtons() {
            //Debug.Log("CharacterCreatorPanel.InitializeSexButtons()");

            if (umaAvatar.activeRace.name == "HumanMaleDCS" || umaAvatar.activeRace.name == "HumanMale") {
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

        private void SetPreviewTarget() {
            //Debug.Log("CharacterPanel.SetPreviewTarget()");
            if (umaAvatar != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget() UMA avatar is already spawned!");
                return;
            }
            //spawn correct preview unit
            CharacterCreatorManager.MyInstance.HandleOpenWindow(PlayerManager.MyInstance.MyDefaultCharacterCreatorUnitProfile);

            if (CameraManager.MyInstance != null && CameraManager.MyInstance.MyCharacterPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (MyPreviewCameraController != null) {
                    MyPreviewCameraController.InitializeCamera(CharacterCreatorManager.MyInstance.MyPreviewUnit.transform);
                    //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting Target Ready Callback");
                    MyPreviewCameraController.OnTargetReady += TargetReadyCallback;
                } else {
                    Debug.LogError("CharacterPanel.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
                }
            }
        }

        public void TargetReadyCallback() {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallback()");
            MyPreviewCameraController.OnTargetReady -= TargetReadyCallback;

            // get reference to avatar
            umaAvatar = CharacterCreatorManager.MyInstance.MyPreviewUnit.GetComponent<DynamicCharacterAvatar>();
            if (umaAvatar == null) {
                //Debug.Log("CharacterCreatorPanel.TargetReadyCallback() DID NOT get UMA avatar");
            } else {
                //Debug.Log("CharacterCreatorPanel.TargetReadyCallback() DID get UMA avatar");
            }

            // update character creator avatar to whatever recipe the actual character currently has, if any
            // disabled for now.  recipe should be already in recipestring anyway
            //SaveManager.MyInstance.SaveUMASettings();
            SaveManager.MyInstance.LoadUMASettings(umaAvatar);

            //CloseAppearanceOptionsArea();
            //CloseColorsOptionsArea();
            CloseOptionsAreas();
            OpenAppearanceOptionsArea();
            InitializeSexButtons();

        }

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
            colorSelectionController.Setup(umaAvatar, "Skin", skinColorsOptionsArea, SkinColor);
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
            colorSelectionController.Setup(umaAvatar, "Hair", hairColorsOptionsArea, HairColor);
        }

        public void InitializeEyesColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeEyesColors()");
            ColorSelectionController colorSelectionController = eyesColorsOptionsArea.GetComponent<ColorSelectionController>();
            colorSelectionController.Setup(umaAvatar, "Eyes", eyesColorsOptionsArea, EyesColor);
        }

        public void ColorsClick() {
            //Debug.Log("CharacterCreatorPanel.ColorsClick()");

            CleanupDynamicMenus();
            GameObject setupParent = null;

            foreach (UMA.OverlayColorData overlayColorData in umaAvatar.CurrentSharedColors) {
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

                availableColorsHandler.Setup(umaAvatar, overlayColorData.name, setupParent, currColors);

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

            if (umaAvatar.activeRace.name == "HumanMaleDCS" || umaAvatar.activeRace.name == "HumanMale") {
                //Debug.Log("CharacterCreatorPanel.SetMale(): already male. returning");
                return;
            }
            femaleRecipe = umaAvatar.GetCurrentRecipe();
            femaleButton.DeSelect();
            maleButton.Select();
            if (maleRecipe != string.Empty) {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): maleRecipe != string.Empty");
                umaAvatar.ChangeRace("HumanMaleDCS");
                SaveManager.MyInstance.LoadUMASettings(maleRecipe, umaAvatar);
            } else {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): maleRecipe == string.Empty");
                umaAvatar.ChangeRace("HumanMaleDCS");
            }
        }

        public void SetFemale() {
            //Debug.Log("CharacterCreatorPanel.SetFemale()");

            if (umaAvatar.activeRace.name == "HumanFemaleDCS" || umaAvatar.activeRace.name == "HumanFemale") {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): already female. returning");
                return;
            }
            maleRecipe = umaAvatar.GetCurrentRecipe();
            maleButton.DeSelect();
            femaleButton.Select();
            if (femaleRecipe != string.Empty) {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): femaleRecipe != string.Empty");
                umaAvatar.ChangeRace("HumanFemaleDCS");
                SaveManager.MyInstance.LoadUMASettings(femaleRecipe, umaAvatar);
            } else {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): femaleRecipe == string.Empty");
                umaAvatar.ChangeRace("HumanFemaleDCS");
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
            if (umaAvatar == null) {
                Debug.Log("CharacterCreatorPanel.CheckAppearance(): umaAvatar is null!!!!");
            }
            Dictionary<string, List<UMATextRecipe>> recipes = umaAvatar.AvailableRecipes;

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
                    if (GetRecipeName(option, hairRecipes) == umaAvatar.GetWardrobeItemName("Hair")) {
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
                    if (GetRecipeName(option, eyebrowsRecipes) == umaAvatar.GetWardrobeItemName("Eyebrows")) {
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
                    if (GetRecipeName(option, beardRecipes) == umaAvatar.GetWardrobeItemName("Beard")) {
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
                umaAvatar.ClearSlot("Hair");
            }
            umaAvatar.SetSlot("Hair", GetRecipeName(hairAppearanceDropdown.options[hairAppearanceDropdown.value].text, hairRecipes));
            RebuildUMA();
        }

        public void SetEyebrows(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetEyebrows(" + dropdownIndex + "): " + eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text);
            if (eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text == "None") {
                umaAvatar.ClearSlot("Eyebrows");
            }
            umaAvatar.SetSlot("Eyebrows", GetRecipeName(eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text, eyebrowsRecipes));
            RebuildUMA();
        }

        public void SetBeard(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetBeard(" + dropdownIndex + "): " + beardAppearanceDropdown.options[beardAppearanceDropdown.value].text);
            if (beardAppearanceDropdown.options[beardAppearanceDropdown.value].text == "None") {
                umaAvatar.ClearSlot("Beard");
            }
            umaAvatar.SetSlot("Beard", GetRecipeName(beardAppearanceDropdown.options[beardAppearanceDropdown.value].text, beardRecipes));
            RebuildUMA();
        }

        public void RebuildUMA() {
            //Debug.Log("CharacterCreatorPanel.RebuildUMA()");
            umaAvatar.BuildCharacter();
            //umaAvatar.BuildCharacter(true);
            //umaAvatar.ForceUpdate(true, true, true);
        }

        public void SaveCharacter() {
            //Debug.Log("CharacterCreatorPanel.SaveCharacter()");
            SaveManager.MyInstance.SaveUMASettings(umaAvatar.GetCurrentRecipe());

            // replace a default player unit with an UMA player unit when a save occurs
            if (PlayerManager.MyInstance.MyAvatar == null) {
                Vector3 currentPlayerLocation = PlayerManager.MyInstance.MyPlayerUnitObject.transform.position;
                PlayerManager.MyInstance.DespawnPlayerUnit();
                //PlayerManager.MyInstance.SetUMAPrefab();
                PlayerManager.MyInstance.MyCharacter.SetUnitProfile(PlayerManager.MyInstance.MyDefaultCharacterCreatorUnitProfileName);
                PlayerManager.MyInstance.SpawnPlayerUnit(currentPlayerLocation);
                if (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager != null) {
                    PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.LearnDefaultAutoAttackAbility();
                }

            }
            SaveManager.MyInstance.LoadUMASettings();
            //ClosePanel();

            OnConfirmAction();
        }


    }

}