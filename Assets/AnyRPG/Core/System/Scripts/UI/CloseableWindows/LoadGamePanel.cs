using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class LoadGamePanel : WindowContentController {

        //public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };
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

            /*
            returnButton.Configure(systemGameManager);
            loadGameButton.Configure(systemGameManager);
            newGameButton.Configure(systemGameManager);
            deleteGameButton.Configure(systemGameManager);
            copyGameButton.Configure(systemGameManager);
            */

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
            // testing - character will load its own equipment when it spawns
            //characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.OnTargetCreated -= HandleTargetCreated;
            characterPreviewPanel.CapabilityConsumer = null;
            characterPreviewPanel.ReceiveClosedWindowNotification();
            //saveManager.ClearSharedData();

            ClearLoadButtons();

            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");
            base.ProcessOpenWindowNotification();

            //ShowLoadButtonsCommon();

            // inform the preview panel so the character can be rendered
            // testing - character will load its own equipment when it spawns
            //characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.OnTargetCreated += HandleTargetCreated;
            characterPreviewPanel.CapabilityConsumer = loadGameManager;
            characterPreviewPanel.ReceiveOpenWindowNotification();

            // testing - move this down here so re-used UMA units don't trigger handleTargetCreated before we can subscribe to it
            ShowLoadButtonsCommon();

            // this needs to be run here because the initial run in ShowLoadButtonsCommon will have done nothing because the preview panel wasn't open yet
            //LoadSavedAppearanceSettings();
        }

        public void ShowSavedGame(LoadGameButton loadButton) {
            //Debug.Log("LoadGamePanel.ShowSavedGame()");

            selectedLoadGameButton = loadButton;

            loadGameManager.SetSavedGame(loadButton.SaveData);

            // testing avoid naked spawn
            // seems to make no difference to have this disabled here
            //LoadUMARecipe();

            // ensure the correct unit and character model is spawned
            characterPreviewPanel.ReloadUnit();

            // testing get proper appearance
            //LoadSavedAppearanceSettings();

            // apply capabilities to it so equipment can work
            //characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.ApplyCapabilityConsumerSnapshot(capabilityConsumerSnapshot);

            loadGameButton.Button.interactable = true;
            copyGameButton.Button.interactable = true;
            deleteGameButton.Button.interactable = true;
            uINavigationControllers[1].UpdateNavigationList();
            uINavigationControllers[0].UnHightlightButtons(loadButton);

            nameText.text = loadGameManager.AnyRPGSaveData.playerName;
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
                loadGameButton.Configure(systemGameManager);
                loadGameButton.AddSaveData(this, anyRPGSaveData);
                loadGameButtons.Add(loadGameButton);
                uINavigationControllers[0].AddActiveButton(loadGameButton);
                if (anyRPGSaveData.DataFileName == fileName) {
                    selectedButton = count;
                }
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

        public void HandleTargetCreated() {
            //Debug.Log("LoadGamePanel.HandleTargetCreated()");

            if (characterCreatorManager.PreviewUnitController?.UnitModelController != null) {
                characterCreatorManager.PreviewUnitController?.UnitModelController.SetInitialSavedAppearance();
            }

            // set level before attempting to load equipment in case equipment has level restrictions
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.Initialize(loadGameManager.AnyRPGSaveData.playerName, loadGameManager.AnyRPGSaveData.PlayerLevel);
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

                //LoadUMARecipe();

                // apply capabilities to it so equipment can work
                characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.ApplyCapabilityConsumerSnapshot(loadGameManager.CapabilityConsumerSnapshot);

                BaseCharacter baseCharacter = characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter;
                if (baseCharacter != null) {
                    //saveManager.LoadEquipmentData(loadGameButton.MySaveData, characterEquipmentManager);
                    // results in equipment being sheathed
                    saveManager.LoadEquipmentData(loadGameManager.AnyRPGSaveData, baseCharacter.CharacterEquipmentManager);
                    
                    /*
                    if (characterCreatorManager.PreviewUnitController.UnitModelController.ModelReady == true) {
                        // any mecanim, and re-used UMAs will already be initialized, so the equipment models can be loaded right away
                        characterCreatorManager.PreviewUnitController.UnitModelController.EquipEquipmentModels(baseCharacter.CharacterEquipmentManager);
                    }
                    */
                    
                }
            }
        }

        /*
        public void ClosePanel() {
            //Debug.Log("LoadGamePanel.ClosePanel()");
            uIManager.loadGameWindow.CloseWindow();
        }
        */

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
                AnyRPGSaveData saveData = SelectedLoadGameButton.SaveData;
                Close();
                loadGameManager.LoadGame(saveData);
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
            ShowLoadButtonsCommon(SelectedLoadGameButton.SaveData.DataFileName);
        }

    }

}