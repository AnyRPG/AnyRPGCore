using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class LoadGamePanel : WindowContentController {

        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private CharacterPreviewPanelController characterPreviewPanel = null;

        [SerializeField]
        private GameObject buttonPrefab = null;

        [SerializeField]
        private GameObject buttonArea = null;

        /*
        [SerializeField]
        private HighlightButton returnButton = null;
        */

        [SerializeField]
        private HighlightButton loadGameButton = null;

        /*
        [SerializeField]
        private HighlightButton newGameButton = null;
        */

        [SerializeField]
        private HighlightButton deleteGameButton = null;

        [SerializeField]
        private HighlightButton copyGameButton = null;

        [SerializeField]
        private TextMeshProUGUI nameText = null;

        private List<LoadGameButton> loadGameButtons = new List<LoadGameButton>();

        private LoadGameButton selectedLoadGameButton = null;

        // game manager references
        private SaveManager saveManager = null;
        private ObjectPooler objectPooler = null;
        private CharacterCreatorManager characterCreatorManager = null;
        private UIManager uIManager = null;
        private LoadGameManager loadGameManager = null;

        public LoadGameButton SelectedLoadGameButton { get => selectedLoadGameButton; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            loadGameManager.OnDeleteGame += HandleDeleteGame;
            loadGameManager.OnCopyGame += HandleCopyGame;
            loadGameManager.OnLoadCharacterList += HandleLoadCharacterList;

            characterPreviewPanel.Configure(systemGameManager);
            characterPreviewPanel.SetParentPanel(this);
        }

        public override void SetGameManagerReferences() {
            //Debug.Log("LoadGamePanel.SetGameManagerReferences()");
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            objectPooler = systemGameManager.ObjectPooler;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            uIManager = systemGameManager.UIManager;
            loadGameManager = systemGameManager.LoadGameManager;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.RecieveClosedWindowNotification()");

            characterPreviewPanel.OnUnitCreated -= HandleUnitCreated;
            characterPreviewPanel.CharacterConfigurationProvider = null;
            characterPreviewPanel.ReceiveClosedWindowNotification();
            //saveManager.ClearSharedData();

            ClearLoadButtons();

            characterCreatorManager.DisableLight();

            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();

            loadGameManager.LoadCharacterList();

            // inform the preview panel so the character can be rendered
            characterPreviewPanel.OnUnitCreated += HandleUnitCreated;
            characterPreviewPanel.CharacterConfigurationProvider = loadGameManager;
            characterPreviewPanel.ReceiveOpenWindowNotification();


            // This is down here so re-used UMA units don't trigger handleTargetCreated before we can subscribe to it
            ShowLoadButtonsCommon();

            // this needs to be run here because the initial run in ShowLoadButtonsCommon will have done nothing because the preview panel wasn't open yet
            //LoadSavedAppearanceSettings();

            characterCreatorManager.EnableLight();
        }

        public void ShowSavedGame(LoadGameButton loadButton) {
            //Debug.Log("LoadGamePanel.ShowSavedGame()");

            selectedLoadGameButton = loadButton;

            loadGameManager.SetSavedGame(loadButton.SaveData);

            // ensure the correct unit and character model is spawned
            characterPreviewPanel.ReloadUnit();

            // testing get proper appearance
            //LoadSavedAppearanceSettings();

            // apply capabilities to it so equipment can work
            //characterCreatorManager.PreviewUnitController.BaseCharacter.ApplyCapabilityConsumerSnapshot(capabilityConsumerSnapshot);

            loadGameButton.Button.interactable = true;
            copyGameButton.Button.interactable = true;
            deleteGameButton.Button.interactable = true;
            uINavigationControllers[1].UpdateNavigationList();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(loadButton);

            nameText.text = loadGameManager.PlayerCharacterSaveData.SaveData.playerName;
        }


        public void ClearLoadButtons() {
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            foreach (LoadGameButton loadGameButton in loadGameButtons) {
                if (loadGameButton != null) {
                    loadGameButton.DeSelect();
                    objectPooler.ReturnObjectToPool(loadGameButton.gameObject);
                }
            }
            loadGameButtons.Clear();
            uINavigationControllers[0].ClearActiveButtons();
            selectedLoadGameButton = null;
            loadGameButton.Button.interactable = false;
            copyGameButton.Button.interactable = false;
            deleteGameButton.Button.interactable = false;
            nameText.text = "";
            loadGameManager.ResetData();
        }


        public void ShowLoadButtonsCommon() {
            Debug.Log("LoadGamePanel.ShowLoadButtonsCommon()");
            ClearLoadButtons();
            characterPreviewPanel.ClearPreviewTarget();
            int count = 0;
            foreach (PlayerCharacterSaveData playerCharacterSaveData in loadGameManager.CharacterList) {
                Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                AddLoadButton(playerCharacterSaveData);
                count++;
            }
            uINavigationControllers[1].UpdateNavigationList();
            if (loadGameButtons.Count > 0) {
                SetNavigationController(uINavigationControllers[0]);
            } else {
                SetNavigationController(uINavigationControllers[1]);
            }
            //SetPreviewTarget();
        }

        private void AddLoadButton(PlayerCharacterSaveData playerCharacterSaveData) {
            GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
            LoadGameButton loadGameButton = go.GetComponent<LoadGameButton>();
            loadGameButton.Configure(systemGameManager);
            loadGameButton.AddSaveData(this, playerCharacterSaveData);
            loadGameButtons.Add(loadGameButton);
            uINavigationControllers[0].AddActiveButton(loadGameButton);
        }

        public void HandleLoadCharacterList() {
            Debug.Log("LoadGamePanel.HandleLoadCharacterList()");

            ShowLoadButtonsCommon();
        }

        public void HandleUnitCreated() {
            //Debug.Log("LoadGamePanel.HandleTargetCreated()");

            if (characterCreatorManager.PreviewUnitController?.UnitModelController != null) {
                characterCreatorManager.PreviewUnitController?.UnitModelController.SetInitialSavedAppearance(loadGameManager.PlayerCharacterSaveData.SaveData);
            }

            LoadEquipmentData();
        }

        /*
        public void LoadSavedAppearanceSettings() {
            if (characterCreatorManager.PreviewUnitController?.UnitModelController  != null) {
                characterCreatorManager.PreviewUnitController?.UnitModelController.LoadSavedAppearanceSettings();
            }

            // testing - try this here to allow the unit to equip itself naturally when it finishes spawning
            LoadEquipmentData();
        }
        */

        public void LoadEquipmentData() {
            //Debug.Log("LoadGamePanel.LoadEquipmentData()");

            if (characterCreatorManager.PreviewUnitController != null) {
                //Debug.Log("LoadGamePanel.LoadEquipmentData(): preview controller found");

                saveManager.LoadEquipmentData(loadGameManager.PlayerCharacterSaveData.SaveData, characterCreatorManager.PreviewUnitController.CharacterEquipmentManager);
            }
        }

        /*
        public void ClosePanel() {
            //Debug.Log("LoadGamePanel.ClosePanel()");
            uIManager.loadGameWindow.CloseWindow();
        }
        */

        public void LoadGame() {
            if (SelectedLoadGameButton != null) {
                Close();
                loadGameManager.LoadGame(SelectedLoadGameButton.SaveData);
            }
        }

        public void NewGame() {
            //Debug.Log("LoadGamePanel.NewGame()");
            if (systemConfigurationManager.UseNewGameWindow == true) {
                Close();
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

        public void HandleDeleteGame() {
            uIManager.deleteGameMenuWindow.CloseWindow();
            ShowLoadButtonsCommon();
        }

        public void CopyGame() {
            if (SelectedLoadGameButton != null) {
                uIManager.copyGameMenuWindow.OpenWindow();
            }
        }

        public void HandleCopyGame() {
            uIManager.copyGameMenuWindow.CloseWindow();
            //ShowLoadButtonsCommon(SelectedLoadGameButton.SaveData.SaveData.DataFileName);
            ShowLoadButtonsCommon();
        }

    }

}