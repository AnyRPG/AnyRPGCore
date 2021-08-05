using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class LoadGamePanel : WindowContentController, ICapabilityConsumer {

        #region Singleton
        private static LoadGamePanel instance;

        public static LoadGamePanel Instance {
            get {
                return instance;
            }
        }

        private void Awake() {
            instance = this;
        }
        #endregion

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private CharacterPreviewPanelController characterPreviewPanel = null;

        [SerializeField]
        private GameObject buttonPrefab = null;

        [SerializeField]
        private GameObject buttonArea = null;

        [SerializeField]
        private Button loadGameButton = null;

        [SerializeField]
        private Button deleteGameButton = null;

        [SerializeField]
        private Button copyGameButton = null;

        [SerializeField]
        private TextMeshProUGUI nameText = null;

        private List<LoadGameButton> loadGameButtons = new List<LoadGameButton>();

        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private AnyRPGSaveData anyRPGSaveData;

        private LoadGameButton selectedLoadGameButton = null;

        private CapabilityConsumerSnapshot capabilityConsumerSnapshot = null;

        // game manager references
        private SaveManager saveManager = null;
        private ObjectPooler objectPooler = null;
        private CharacterCreatorManager characterCreatorManager = null;
        private SystemConfigurationManager systemConfigurationManager = null;
        private UIManager uIManager = null;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        public LoadGameButton SelectedLoadGameButton { get => selectedLoadGameButton; set => selectedLoadGameButton = value; }

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            saveManager = systemGameManager.SaveManager;
            objectPooler = systemGameManager.ObjectPooler;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            uIManager = systemGameManager.UIManager;

            characterPreviewPanel.Init(systemGameManager);
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.RecieveClosedWindowNotification()");
            base.RecieveClosedWindowNotification();
            characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.RecieveClosedWindowNotification();
            //saveManager.ClearSharedData();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();

            ShowLoadButtonsCommon();

            // inform the preview panel so the character can be rendered
            characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.CapabilityConsumer = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();

            // this needs to be run here because the initial run in ShowLoadButtonsCommon will have done nothing because the preview panel wasn't open yet
            LoadUMARecipe();
        }

        public void ShowSavedGame(LoadGameButton loadButton) {
            //Debug.Log("LoadGamePanel.ShowSavedGame()");

            selectedLoadGameButton = loadButton;
            anyRPGSaveData = loadButton.SaveData;

            capabilityConsumerSnapshot = saveManager.GetCapabilityConsumerSnapshot(selectedLoadGameButton.SaveData);

            unitProfile = capabilityConsumerSnapshot.UnitProfile;
            UnitType = capabilityConsumerSnapshot.UnitProfile.UnitType;
            characterRace = capabilityConsumerSnapshot.CharacterRace;
            characterClass = capabilityConsumerSnapshot.CharacterClass;
            classSpecialization = capabilityConsumerSnapshot.ClassSpecialization;
            faction = capabilityConsumerSnapshot.Faction;

            saveManager.ClearSharedData();
            saveManager.LoadUMARecipe(loadButton.SaveData);

            // testing avoid naked spawn
            // seems to make no difference to have this disabled here
            //LoadUMARecipe();

            // ensure the correct unit and character model is spawned
            characterPreviewPanel.ReloadUnit();

            // testing get proper appearance
            LoadUMARecipe();

            // apply capabilities to it so equipment can work
            //characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.ApplyCapabilityConsumerSnapshot(capabilityConsumerSnapshot);

            loadGameButton.interactable = true;
            copyGameButton.interactable = true;
            deleteGameButton.interactable = true;

            nameText.text = anyRPGSaveData.playerName;
        }


        public void ClearLoadButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (LoadGameButton loadGameButton in loadGameButtons) {
                if (loadGameButton != null) {
                    loadGameButton.DeSelect();
                    objectPooler.ReturnObjectToPool(loadGameButton.gameObject);
                }
            }
            loadGameButtons.Clear();
            selectedLoadGameButton = null;
            loadGameButton.interactable = false;
            copyGameButton.interactable = false;
            deleteGameButton.interactable = false;
            nameText.text = "";
            unitProfile = null;
            unitType = null;
            characterRace = null;
            characterClass = null;
            classSpecialization = null;
            faction = null;
            anyRPGSaveData = new AnyRPGSaveData();
            capabilityConsumerSnapshot = null;
        }


        public void ShowLoadButtonsCommon(string fileName = "") {
            //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon()");
            ClearLoadButtons();
            characterPreviewPanel.ClearPreviewTarget();
            int selectedButton = 0;
            int count = 0;
            foreach (AnyRPGSaveData anyRPGSaveData in saveManager.GetSaveDataList()) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                LoadGameButton loadGameButton = go.GetComponent<LoadGameButton>();
                loadGameButton.Init(systemGameManager);
                loadGameButton.AddSaveData(this, anyRPGSaveData);
                loadGameButtons.Add(loadGameButton);
                if (anyRPGSaveData.DataFileName == fileName) {
                    selectedButton = count;
                }
                count++;
            }
            if (loadGameButtons.Count > 0) {
                loadGameButtons[selectedButton].Select();
            }
            //SetPreviewTarget();
        }


        public void LoadUMARecipe() {
            if (characterCreatorManager.PreviewUnitController?.DynamicCharacterAvatar != null) {
                saveManager.LoadUMASettings(characterCreatorManager.PreviewUnitController.DynamicCharacterAvatar, false);
            }
        }

        public void HandleTargetReady() {
            //LoadUMARecipe();

            if (characterCreatorManager.PreviewUnitController != null) {

                //LoadUMARecipe();

                // apply capabilities to it so equipment can work
                characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.ApplyCapabilityConsumerSnapshot(capabilityConsumerSnapshot);

                BaseCharacter baseCharacter = characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter;
                if (baseCharacter != null) {
                    //saveManager.LoadEquipmentData(loadGameButton.MySaveData, characterEquipmentManager);
                    // results in equipment being sheathed
                    saveManager.LoadEquipmentData(anyRPGSaveData, baseCharacter.CharacterEquipmentManager);
                }
            }
        }

        public void UnHighlightAllButtons() {
            //Debug.Log("CharacterCreatorPanel.UnHighlightAllButtons()");

        }

        public void ClosePanel() {
            //Debug.Log("LoadGamePanel.ClosePanel()");
            uIManager.loadGameWindow.CloseWindow();
        }

        /*
        public void RebuildUMA() {
            //Debug.Log("CharacterCreatorPanel.RebuildUMA()");
            Debug.Log("LoadGamePanel.RebuildUMA(): BuildCharacter()");
            umaAvatar.BuildCharacter();
            //umaAvatar.BuildCharacter(true);
            //umaAvatar.ForceUpdate(true, true, true);
        }
        */

        public void LoadGame() {
            if (SelectedLoadGameButton != null) {
                saveManager.LoadGame(SelectedLoadGameButton.SaveData);
            }
        }

        public void NewGame() {
            //Debug.Log("LoadGamePanel.NewGame()");
            if (systemConfigurationManager.UseNewGameWindow == true) {
                ClosePanel();
                uIManager.newGameWindow.OpenWindow();
            } else {
                uIManager.confirmNewGameMenuWindow.OpenWindow();
            }
        }

        public void DeleteGame() {
            if (SelectedLoadGameButton != null) {
                uIManager.deleteGameMenuWindow.OpenWindow();
            }
        }

        public void DeleteGame(bool confirmDelete = false) {
            if (SelectedLoadGameButton != null) {
                if (confirmDelete) {
                    saveManager.DeleteGame(SelectedLoadGameButton.SaveData);
                    uIManager.deleteGameMenuWindow.CloseWindow();
                    ShowLoadButtonsCommon();
                } else {
                    uIManager.deleteGameMenuWindow.OpenWindow();
                }
            }
        }

        public void CopyGame() {
            if (SelectedLoadGameButton != null) {
                uIManager.copyGameMenuWindow.OpenWindow();
            }
        }

        public void CopyGame(bool confirmCopy = false) {
            if (SelectedLoadGameButton != null) {
                if (confirmCopy) {
                    saveManager.CopyGame(SelectedLoadGameButton.SaveData);
                    uIManager.copyGameMenuWindow.CloseWindow();
                    ShowLoadButtonsCommon(SelectedLoadGameButton.SaveData.DataFileName);
                } else {
                    uIManager.copyGameMenuWindow.OpenWindow();
                }
            }
        }

    }

}