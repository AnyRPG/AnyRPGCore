using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class UnitSpawnControlPanel : WindowContentController {

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

        /*
        [SerializeField]
        private HighlightButton returnButton = null;

        [SerializeField]
        private HighlightButton spawnButton = null;
        */

        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        private List<UnitSpawnNode> unitSpawnNodeList = new List<UnitSpawnNode>();

        private List<UnitSpawnButton> unitSpawnButtons = new List<UnitSpawnButton>();

        private int extraLevels;

        private int unitLevel = 1;

        private UnitToughness unitToughness;

        // game manager references
        private PlayerManager playerManager = null;
        private SystemDataFactory systemDataFactory = null;
        private UnitPreviewManager unitPreviewManager = null;
        private CameraManager cameraManager = null;
        private UIManager uIManager = null;
        private ObjectPooler objectPooler = null;

        public UnitPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }
        public UnitSpawnButton SelectedUnitSpawnButton { get => selectedUnitSpawnButton; set => selectedUnitSpawnButton = value; }
        public List<UnitProfile> UnitProfileList { get => unitProfileList; set => unitProfileList = value; }
        public List<UnitSpawnNode> UnitSpawnNodeList { get => unitSpawnNodeList; set => unitSpawnNodeList = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //returnButton.Configure(systemGameManager);
            //spawnButton.Configure(systemGameManager);
            previewCameraController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            unitPreviewManager = systemGameManager.UnitPreviewManager;
            cameraManager = systemGameManager.CameraManager;
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
        }

        /*
        protected void Start() {
            CloseExtraLevelsOptionsArea();
        }
        */

        public void PopulateDropDownValues() {

            levelDropdown.ClearOptions();
            extraLevelsDropdown.ClearOptions();
            toughnessDropdown.ClearOptions();

            //int counter = 0;
            List<string> options = new List<string>();

            // LEVELS
            for (int i = 1; i < systemConfigurationManager.MaxLevel; i++) {
                options.Add(i.ToString());
            }
            levelDropdown.AddOptions(options);
            //levelDropdown.value = currentHairIndex;
            //levelDropdown.RefreshShownValue();
            options.Clear();

            // EXTRA LEVELS
            for (int i = 0; i < systemConfigurationManager.MaxLevel - playerManager.MyCharacter.CharacterStats.Level; i++) {
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
            foreach (UnitToughness unitToughness in systemDataFactory.GetResourceList<UnitToughness>()) {
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
            if (unitSpawnButton.UnitProfile.DefaultToughness != null) {
                //toughnessDropdown.value = unitSpawnButton.MyUnitProfile.MyDefaultToughness - 1;
                int counter = 0;
                foreach (TMP_Dropdown.OptionData data in toughnessDropdown.options) {
                    if (data.text == unitSpawnButton.UnitProfile.DefaultToughness.DisplayName) {
                        toughnessDropdown.value = counter;
                        break;
                    }
                    counter++;
                }
            } else {
                toughnessDropdown.value = 0;
            }

            nameText.text = unitSpawnButton.UnitProfile.CharacterName;
        }

        public void ClearPreviewTarget() {
            //Debug.Log("LoadGamePanel.ClearPreviewTarget()");
            // not really close window, but it will despawn the preview unit
            unitPreviewManager.HandleCloseWindow();
        }

        public void SetPreviewTarget() {
            //Debug.Log("CharacterPanel.SetPreviewTarget()");
            
            if (unitPreviewManager.PreviewUnitController != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget() UMA avatar is already spawned!");
                return;
            }

            //spawn correct preview unit
            //unitPreviewManager.OnTargetCreated += HandleTargetCreated;
            unitPreviewManager.HandleOpenWindow(this);

            if (cameraManager.UnitPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (MyPreviewCameraController != null) {
                    MyPreviewCameraController.InitializeCamera(unitPreviewManager.PreviewUnitController);
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
            uIManager.unitSpawnWindow.CloseWindow();
            uIManager.interactionWindow.CloseWindow();
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            previewCameraController.ClearTarget();
            unitPreviewManager.HandleCloseWindow();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
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
                GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                UnitSpawnButton unitSpawnButton = go.GetComponent<UnitSpawnButton>();
                if (unitSpawnButton != null) {
                    unitSpawnButton.Configure(systemGameManager);
                    unitSpawnButton.UnitSpawnControlPanel = this;
                    unitSpawnButton.AddUnitProfile(unitProfile);
                    unitSpawnButtons.Add(unitSpawnButton);
                    uINavigationControllers[0].AddActiveButton(unitSpawnButton);
                }

            }
            if (unitSpawnButtons.Count > 0) {
                SetNavigationController(uINavigationControllers[0]);

                //unitSpawnButtons[0].Select();
            }
            //SetPreviewTarget();
        }

        /// <summary>
        /// clear the unit list so any units left over from a previous time opening the window aren't shown
        /// </summary>
        public void ClearPreviewButtons() {
            ///Debug.Log("UnitSpawnControlPanel.ClearPreviewButtons()");
            foreach (UnitSpawnButton unitSpawnButton in unitSpawnButtons) {
                if (unitSpawnButton != null) {
                    unitSpawnButton.DeSelect();
                    objectPooler.ReturnObjectToPool(unitSpawnButton.gameObject);
                }
            }
            unitSpawnButtons.Clear();
            uINavigationControllers[0].ClearActiveButtons();
            SelectedUnitSpawnButton = null;
            nameText.text = "";
        }


        public void CloseLevelOptionsArea() {
            Debug.Log("UnitSpawnControlPanel.CloseLevelOptionsArea()");
            levelOptionsArea.gameObject.SetActive(false);
            uINavigationControllers[1].UpdateNavigationList();
        }

        public void OpenLevelOptionsArea() {
            Debug.Log("UnitSpawnControlPanel.OpenLevelOptionsArea()");
            levelOptionsArea.gameObject.SetActive(true);
            uINavigationControllers[1].UpdateNavigationList();
        }

        public void CloseExtraLevelsOptionsArea() {
            Debug.Log("UnitSpawnControlPanel.CloseExtraLevelsOptionsArea()");
            extraLevelsOptionsArea.gameObject.SetActive(false);
            uINavigationControllers[1].UpdateNavigationList();
        }

        public void OpenExtraLevelsOptionsArea() {
            Debug.Log("UnitSpawnControlPanel.OpenExtraLevelsOptionsArea()");
            extraLevelsOptionsArea.gameObject.SetActive(true);
            uINavigationControllers[1].UpdateNavigationList();
        }

        public void SetLevelType(int dropdownIndex) {
            Debug.Log("UnitSpawnControlPanel.SetLevelType(" + dropdownIndex + ")");
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
                UnitToughness tmpToughness = systemDataFactory.GetResource<UnitToughness>(toughnessDropdown.options[toughnessDropdown.value].text);
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
                    unitSpawnNode.ManualSpawn(unitLevel, extraLevels, useDynamicLevel, SelectedUnitSpawnButton.UnitProfile, unitToughness);
                }
            }
            ClosePanel();
        }


    }

}