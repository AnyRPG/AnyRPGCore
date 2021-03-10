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

        public static LoadGamePanel MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<LoadGamePanel>();
                }

                return instance;
            }
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

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        public LoadGameButton SelectedLoadGameButton { get => selectedLoadGameButton; set => selectedLoadGameButton = value; }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.RecieveClosedWindowNotification()");
            base.RecieveClosedWindowNotification();
            characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.RecieveClosedWindowNotification();
            //SaveManager.MyInstance.ClearSharedData();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");

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

            capabilityConsumerSnapshot = SaveManager.MyInstance.GetCapabilityConsumerSnapshot(selectedLoadGameButton.SaveData);

            unitProfile = capabilityConsumerSnapshot.UnitProfile;
            UnitType = capabilityConsumerSnapshot.UnitProfile.UnitType;
            characterRace = capabilityConsumerSnapshot.CharacterRace;
            characterClass = capabilityConsumerSnapshot.CharacterClass;
            classSpecialization = capabilityConsumerSnapshot.ClassSpecialization;
            faction = capabilityConsumerSnapshot.Faction;

            SaveManager.MyInstance.ClearSharedData();
            SaveManager.MyInstance.LoadUMARecipe(loadButton.SaveData);

            // testing avoid naked spawn
            // seems to make no difference to have this disabled here
            //LoadUMARecipe();

            // ensure the correct unit and character model is spawned
            characterPreviewPanel.ReloadUnit();

            // testing get proper appearance
            LoadUMARecipe();

            // apply capabilities to it so equipment can work
            //CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter.ApplyCapabilityConsumerSnapshot(capabilityConsumerSnapshot);

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
                    ObjectPooler.MyInstance.ReturnObjectToPool(loadGameButton.gameObject);
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
            foreach (AnyRPGSaveData anyRPGSaveData in SaveManager.MyInstance.GetSaveDataList()) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                GameObject go = ObjectPooler.MyInstance.GetPooledObject(buttonPrefab, buttonArea.transform);
                LoadGameButton loadGameButton = go.GetComponent<LoadGameButton>();
                loadGameButton.AddSaveData(anyRPGSaveData);
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
            if (CharacterCreatorManager.MyInstance?.PreviewUnitController?.DynamicCharacterAvatar != null) {
                SaveManager.MyInstance.LoadUMASettings(CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar, false);
            }
        }

        public void HandleTargetReady() {
            //LoadUMARecipe();

            if (CharacterCreatorManager.MyInstance.PreviewUnitController != null) {

                //LoadUMARecipe();

                // apply capabilities to it so equipment can work
                CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter.ApplyCapabilityConsumerSnapshot(capabilityConsumerSnapshot);

                BaseCharacter baseCharacter = CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter;
                if (baseCharacter != null) {
                    //SaveManager.MyInstance.LoadEquipmentData(loadGameButton.MySaveData, characterEquipmentManager);
                    // results in equipment being sheathed
                    SaveManager.MyInstance.LoadEquipmentData(anyRPGSaveData, baseCharacter.CharacterEquipmentManager);
                }
            }
        }

        public void UnHighlightAllButtons() {
            //Debug.Log("CharacterCreatorPanel.UnHighlightAllButtons()");

        }

        public void ClosePanel() {
            //Debug.Log("LoadGamePanel.ClosePanel()");
            SystemWindowManager.MyInstance.loadGameWindow.CloseWindow();
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
                SaveManager.MyInstance.LoadGame(SelectedLoadGameButton.SaveData);
            }
        }

        public void NewGame() {
            //Debug.Log("LoadGamePanel.NewGame()");
            if (SystemConfigurationManager.MyInstance.UseNewGameWindow == true) {
                ClosePanel();
                SystemWindowManager.MyInstance.newGameWindow.OpenWindow();
            } else {
                SystemWindowManager.MyInstance.confirmNewGameMenuWindow.OpenWindow();
            }
        }

        public void DeleteGame() {
            if (SelectedLoadGameButton != null) {
                SystemWindowManager.MyInstance.deleteGameMenuWindow.OpenWindow();
            }
        }

        public void DeleteGame(bool confirmDelete = false) {
            if (SelectedLoadGameButton != null) {
                if (confirmDelete) {
                    SaveManager.MyInstance.DeleteGame(SelectedLoadGameButton.SaveData);
                    SystemWindowManager.MyInstance.deleteGameMenuWindow.CloseWindow();
                    ShowLoadButtonsCommon();
                } else {
                    SystemWindowManager.MyInstance.deleteGameMenuWindow.OpenWindow();
                }
            }
        }

        public void CopyGame() {
            if (SelectedLoadGameButton != null) {
                SystemWindowManager.MyInstance.copyGameMenuWindow.OpenWindow();
            }
        }

        public void CopyGame(bool confirmCopy = false) {
            if (SelectedLoadGameButton != null) {
                if (confirmCopy) {
                    SaveManager.MyInstance.CopyGame(SelectedLoadGameButton.SaveData);
                    SystemWindowManager.MyInstance.copyGameMenuWindow.CloseWindow();
                    ShowLoadButtonsCommon(SelectedLoadGameButton.SaveData.DataFileName);
                } else {
                    SystemWindowManager.MyInstance.copyGameMenuWindow.OpenWindow();
                }
            }
        }

    }

}