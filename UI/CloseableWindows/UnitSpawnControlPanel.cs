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

    public class UnitSpawnControlPanel : WindowContentController {

        #region Singleton
        private static UnitSpawnControlPanel instance;

        public static UnitSpawnControlPanel MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<UnitSpawnControlPanel>();
                }

                return instance;
            }
        }

        #endregion

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        private UnitSpawnButton selectedUnitSpawnButton;

        [SerializeField]
        private GameObject buttonPrefab;

        [SerializeField]
        private GameObject buttonArea;

        [SerializeField]
        private GameObject levelOptionsArea;

        [SerializeField]
        private GameObject extraLevelsOptionsArea;

        [SerializeField]
        private Dropdown levelTypeDropdown;

        [SerializeField]
        private Dropdown levelDropdown;

        [SerializeField]
        private Dropdown extraLevelsDropdown;

        [SerializeField]
        private Dropdown toughnessDropdown;

        [SerializeField]
        private AnyRPGUnitPreviewCameraController previewCameraController;

        [SerializeField]
        private LayoutElement panelLayoutElement;

        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        private List<UnitSpawnNode> unitSpawnNodeList = new List<UnitSpawnNode>();


        private List<UnitSpawnButton> unitSpawnButtons = new List<UnitSpawnButton>();

        private DynamicCharacterAvatar umaAvatar;

        private int extraLevels;

        private int unitLevel;

        private int unitToughness;

        public AnyRPGUnitPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }
        public UnitSpawnButton MySelectedUnitSpawnButton { get => selectedUnitSpawnButton; set => selectedUnitSpawnButton = value; }
        public List<UnitProfile> MyUnitProfileList { get => unitProfileList; set => unitProfileList = value; }
        public List<UnitSpawnNode> MyUnitSpawnNodeList { get => unitSpawnNodeList; set => unitSpawnNodeList = value; }

        protected void Start() {
            CloseExtraLevelsOptionsArea();
        }

        public void PopulateDropDownValues() {

            levelDropdown.ClearOptions();
            extraLevelsDropdown.ClearOptions();
            toughnessDropdown.ClearOptions();

            //int counter = 0;
            List<string> options = new List<string>();

            // LEVELS
            for (int i = 1; i < SystemConfigurationManager.MyInstance.MyMaxLevel; i++) {
                options.Add(i.ToString());
            }
            levelDropdown.AddOptions(options);
            //levelDropdown.value = currentHairIndex;
            //levelDropdown.RefreshShownValue();
            options.Clear();

            // EXTRA LEVELS
            for (int i = 0; i < SystemConfigurationManager.MyInstance.MyMaxLevel - PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel; i++) {
                options.Add(i.ToString());
            }
            extraLevelsDropdown.AddOptions(options);
            //extraLevelsDropdown.value = currentHairIndex;
            //extraLevelsDropdown.RefreshShownValue();
            options.Clear();

            // TOUGHNESS
            for (int i = 1; i <= 5; i++) {
                options.Add(i.ToString());
            }
            toughnessDropdown.AddOptions(options);
            //toughnessDropdown.value = currentHairIndex;
            //toughnessDropdown.RefreshShownValue();
            options.Clear();

        }

        public void ShowUnit(UnitSpawnButton unitSpawnButton) {
            //Debug.Log("LoadGamePanel.ShowSavedGame()");

            selectedUnitSpawnButton = unitSpawnButton;

            ClearPreviewTarget();
            SetPreviewTarget();
            toughnessDropdown.value = unitSpawnButton.MyUnitProfile.MyDefaultToughness - 1;
        }

        public void ClearPreviewTarget() {
            //Debug.Log("LoadGamePanel.ClearPreviewTarget()");
            // not really close window, but it will despawn the preview unit
            umaAvatar = null;
            UnitPreviewManager.MyInstance.HandleCloseWindow();
        }

        public void SetPreviewTarget() {
            //Debug.Log("CharacterPanel.SetPreviewTarget()");
            if (umaAvatar != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget() UMA avatar is already spawned!");
                return;
            }
            //spawn correct preview unit
            UnitPreviewManager.MyInstance.HandleOpenWindow();

            if (CameraManager.MyInstance != null && CameraManager.MyInstance.MyUnitPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (MyPreviewCameraController != null) {
                    MyPreviewCameraController.InitializeCamera(UnitPreviewManager.MyInstance.MyPreviewUnit.transform);
                    //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting Target Ready Callback");
                    MyPreviewCameraController.OnTargetReady += TargetReadyCallback;
                } else {
                    Debug.LogError("UnitSpawnController.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
                }
            }
        }

        public void TargetReadyCallback() {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallback()");
            MyPreviewCameraController.OnTargetReady -= TargetReadyCallback;

            // get reference to avatar
            umaAvatar = UnitPreviewManager.MyInstance.MyPreviewUnit.GetComponent<DynamicCharacterAvatar>();
            if (umaAvatar == null) {
                //Debug.Log("CharacterCreatorPanel.TargetReadyCallback() DID NOT get UMA avatar");
            } else {
                //Debug.Log("CharacterCreatorPanel.TargetReadyCallback() DID get UMA avatar");
            }

        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            SystemWindowManager.MyInstance.unitSpawnWindow.CloseWindow();
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        }

        /*
        public void RebuildUMA() {
            //Debug.Log("CharacterCreatorPanel.RebuildUMA()");
            umaAvatar.BuildCharacter();
            //umaAvatar.BuildCharacter(true);
            //umaAvatar.ForceUpdate(true, true, true);
        }
        */

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            umaAvatar = null;
            previewCameraController.ClearTarget();
            UnitPreviewManager.MyInstance.HandleCloseWindow();
            //SaveManager.MyInstance.ClearSharedData();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");
            PopulateDropDownValues();
            panelLayoutElement.preferredWidth = Screen.width;
            panelLayoutElement.preferredHeight = Screen.height;
            //Debug.Log("MainMapController.OnOpenWindow(); panelLayoutElement.preferredWidth: " + panelLayoutElement.preferredWidth);
            //Debug.Log("MainMapController.OnOpenWindow(); panelLayoutElement.preferredHeight: " + panelLayoutElement.preferredHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
            ShowPreviewButtonsCommon();

        }

        public void ShowPreviewButtonsCommon() {
            //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon()");
            ClearPreviewTarget();
            ClearPreviewButtons();

            foreach (UnitProfile unitProfile in unitProfileList) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                GameObject go = Instantiate(buttonPrefab, buttonArea.transform);
                UnitSpawnButton unitSpawnButton = go.GetComponent<UnitSpawnButton>();
                if (unitSpawnButton != null) {
                    unitSpawnButton.AddUnitProfile(unitProfile);
                    unitSpawnButtons.Add(unitSpawnButton);
                }

            }
            if (unitSpawnButtons.Count > 0) {
                unitSpawnButtons[0].Select();
            }
            //SetPreviewTarget();
        }

        public void ClearPreviewButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (UnitSpawnButton unitSpawnButton in unitSpawnButtons) {
                if (unitSpawnButton != null) {
                    Destroy(unitSpawnButton.gameObject);
                }
            }
            unitSpawnButtons.Clear();
            MySelectedUnitSpawnButton = null;
        }


        public void CloseLevelOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseEyebrowsAppearanceOptionsArea()");
            levelOptionsArea.gameObject.SetActive(false);
        }

        public void OpenLevelOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenEyebrowsAppearanceOptionsArea()");
            levelOptionsArea.gameObject.SetActive(true);
        }

        public void CloseExtraLevelsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseBeardAppearanceOptionsArea()");
            extraLevelsOptionsArea.gameObject.SetActive(false);
        }

        public void OpenExtraLevelsOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenBeardAppearanceOptionsArea()");
            extraLevelsOptionsArea.gameObject.SetActive(true);
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

        public void SetLevelType(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetHair(" + dropdownIndex + "): " + hairAppearanceDropdown.options[hairAppearanceDropdown.value].text);
            if (levelTypeDropdown.options[levelTypeDropdown.value].text == "Fixed") {
                CloseExtraLevelsOptionsArea();
                OpenLevelOptionsArea();
            } else {
                OpenExtraLevelsOptionsArea();
                CloseLevelOptionsArea();
            }
        }

        public void SetLevel(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetEyebrows(" + dropdownIndex + "): " + eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text);
            unitLevel = levelDropdown.value + 1;
        }

        public void SetToughness(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetEyebrows(" + dropdownIndex + "): " + eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text);
            unitToughness = toughnessDropdown.value + 1;
        }

        public void SetExtraLevels(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetBeard(" + dropdownIndex + "): " + beardAppearanceDropdown.options[beardAppearanceDropdown.value].text);
            extraLevels = levelDropdown.value;
        }

        /*
        public void RebuildUMA() {
            //Debug.Log("CharacterCreatorPanel.RebuildUMA()");
            umaAvatar.BuildCharacter();
            //umaAvatar.BuildCharacter(true);
            //umaAvatar.ForceUpdate(true, true, true);
        }
        */

        public void SpawnUnit() {
            foreach (UnitSpawnNode unitSpawnNode in unitSpawnNodeList) {
                bool useDynamicLevel = (levelTypeDropdown.options[levelTypeDropdown.value].text == "Fixed" ? false : true);
                GameObject spawnPrefab = MySelectedUnitSpawnButton.MyUnitProfile.MyUnitPrefab;
                if (spawnPrefab != null) {
                    unitSpawnNode.ManualSpawn(unitLevel, extraLevels, useDynamicLevel, spawnPrefab, unitToughness);
                }
            }
            ClosePanel();
        }


    }

}