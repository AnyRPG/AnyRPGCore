using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        private GameObject buttonPrefab = null;

        [SerializeField]
        private GameObject buttonArea = null;

        [SerializeField]
        private GameObject levelOptionsArea = null;

        [SerializeField]
        private GameObject extraLevelsOptionsArea = null;

        [SerializeField]
        private TMP_Dropdown levelTypeDropdown = null;

        [SerializeField]
        private TMP_Dropdown levelDropdown = null;

        [SerializeField]
        private TMP_Dropdown extraLevelsDropdown = null;

        [SerializeField]
        private TMP_Dropdown toughnessDropdown = null;

        [SerializeField]
        private UnitPreviewCameraController previewCameraController;

        [SerializeField]
        private TextMeshProUGUI nameText = null;


        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        private List<UnitSpawnNode> unitSpawnNodeList = new List<UnitSpawnNode>();

        private List<UnitSpawnButton> unitSpawnButtons = new List<UnitSpawnButton>();

        private int extraLevels;

        private int unitLevel = 1;

        private UnitToughness unitToughness;

        public UnitPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }
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
            for (int i = 1; i < SystemConfigurationManager.MyInstance.MaxLevel; i++) {
                options.Add(i.ToString());
            }
            levelDropdown.AddOptions(options);
            //levelDropdown.value = currentHairIndex;
            //levelDropdown.RefreshShownValue();
            options.Clear();

            // EXTRA LEVELS
            for (int i = 0; i < SystemConfigurationManager.MyInstance.MaxLevel - PlayerManager.MyInstance.MyCharacter.CharacterStats.Level; i++) {
                options.Add(i.ToString());
            }
            extraLevelsDropdown.AddOptions(options);
            //extraLevelsDropdown.value = currentHairIndex;
            //extraLevelsDropdown.RefreshShownValue();
            options.Clear();

            // TOUGHNESS
            /*
            for (int i = 1; i <= 5; i++) {
                options.Add(i.ToString());
            }
            */

            options.Add("Default");
            foreach (UnitToughness unitToughness in SystemUnitToughnessManager.MyInstance.GetResourceList()) {
                options.Add(unitToughness.DisplayName);
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
            if (unitSpawnButton.MyUnitProfile.DefaultToughness != null) {
                //toughnessDropdown.value = unitSpawnButton.MyUnitProfile.MyDefaultToughness - 1;
                int counter = 0;
                foreach (TMP_Dropdown.OptionData data in toughnessDropdown.options) {
                    if (data.text == unitSpawnButton.MyUnitProfile.DefaultToughness.DisplayName) {
                        toughnessDropdown.value = counter;
                        break;
                    }
                    counter++;
                }
            } else {
                toughnessDropdown.value = 0;
            }

            nameText.text = unitSpawnButton.MyUnitProfile.CharacterName;
        }

        public void ClearPreviewTarget() {
            //Debug.Log("LoadGamePanel.ClearPreviewTarget()");
            // not really close window, but it will despawn the preview unit
            UnitPreviewManager.MyInstance.HandleCloseWindow();
        }

        public void SetPreviewTarget() {
            //Debug.Log("CharacterPanel.SetPreviewTarget()");
            
            if (UnitPreviewManager.MyInstance.PreviewUnitController != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget() UMA avatar is already spawned!");
                return;
            }

            //spawn correct preview unit
            UnitPreviewManager.MyInstance.HandleOpenWindow();

            if (CameraManager.MyInstance != null && CameraManager.MyInstance.UnitPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (MyPreviewCameraController != null) {
                    MyPreviewCameraController.InitializeCamera(UnitPreviewManager.MyInstance.PreviewUnitController);
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
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            SystemWindowManager.MyInstance.unitSpawnWindow.CloseWindow();
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            previewCameraController.ClearTarget();
            UnitPreviewManager.MyInstance.HandleCloseWindow();
            //SaveManager.MyInstance.ClearSharedData();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");
            PopulateDropDownValues();
            ShowPreviewButtonsCommon();
            SetLevelType(0);

        }

        public void ShowPreviewButtonsCommon() {
            //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon()");
            ClearPreviewTarget();
            ClearPreviewButtons();

            foreach (UnitProfile unitProfile in unitProfileList) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                GameObject go = ObjectPooler.MyInstance.GetPooledObject(buttonPrefab, buttonArea.transform);
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
                    ObjectPooler.MyInstance.ReturnObjectToPool(unitSpawnButton.gameObject);
                }
            }
            unitSpawnButtons.Clear();
            MySelectedUnitSpawnButton = null;
            nameText.text = "";
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
            //Debug.Log("CharacterCreatorPanel.SetLevel(" + dropdownIndex + ")");
            unitLevel = levelDropdown.value + 1;
        }

        public void SetToughness(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetEyebrows(" + dropdownIndex + "): " + eyebrowsAppearanceDropdown.options[eyebrowsAppearanceDropdown.value].text);
            if (dropdownIndex == 0) {
                unitToughness = null;
            } else {
                UnitToughness tmpToughness = SystemUnitToughnessManager.MyInstance.GetResource(toughnessDropdown.options[toughnessDropdown.value].text);
                if (tmpToughness != null) {
                    unitToughness = tmpToughness;
                }
            }
        }

        public void SetExtraLevels(int dropdownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetBeard(" + dropdownIndex + "): " + beardAppearanceDropdown.options[beardAppearanceDropdown.value].text);
            extraLevels = levelDropdown.value;
        }

        public void SpawnUnit() {
            foreach (UnitSpawnNode unitSpawnNode in unitSpawnNodeList) {
                bool useDynamicLevel = (levelTypeDropdown.options[levelTypeDropdown.value].text == "Fixed" ? false : true);
                if (unitSpawnNode != null) {
                    unitSpawnNode.ManualSpawn(unitLevel, extraLevels, useDynamicLevel, MySelectedUnitSpawnButton.MyUnitProfile, unitToughness);
                }
            }
            ClosePanel();
        }


    }

}