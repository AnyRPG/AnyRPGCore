using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class CharacterCreatorWindowPanel : CloseableWindowContents, ICharacterEditor, ICharacterConfigurationProvider {

        [Header("Character Creator")]

        [SerializeField]
        private CharacterPreviewPanelController characterPreviewPanel = null;

        [SerializeField]
        private GameObject panelParent = null;

        [SerializeField]
        private DefaultAppearancePanel defaultAppearancePanel = null;

        //[SerializeField]
        //private HighlightButton saveButton = null;

        private Dictionary<GameObject, AppearancePanel> appearanceEditorPanels = new Dictionary<GameObject, AppearancePanel>();
        private Dictionary<Type, GameObject> appearanceEditorPanelTypes = new Dictionary<Type, GameObject>();

        private AppearancePanel currentAppearanceEditorPanel = null;

        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private AnyRPGSaveData saveData;

        // game manager references
        protected UIManager uIManager = null;
        protected PlayerManager playerManager = null;
        protected CharacterCreatorManager characterCreatorManager = null;
        protected SaveManager saveManager = null;
        protected LevelManager levelManager = null;
        protected CharacterCreatorInteractableManager characterCreatorInteractableManager = null;
        protected ObjectPooler objectPooler = null;
        protected SystemDataFactory systemDataFactory = null;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public AnyRPGSaveData SaveData { get => saveData; set => saveData = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            characterPreviewPanel.Configure(systemGameManager);

            defaultAppearancePanel.SetCharacterEditor(this);
            
            AddDefaultAppearancePanel();
            GetAvailableAppearancePanels();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            saveManager = systemGameManager.SaveManager;
            levelManager = systemGameManager.LevelManager;
            characterCreatorInteractableManager = systemGameManager.CharacterCreatorInteractableManager;
            objectPooler = systemGameManager.ObjectPooler;
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        private void AddDefaultAppearancePanel() {
            appearanceEditorPanels.Add(defaultAppearancePanel.gameObject, defaultAppearancePanel);
        }

        private void GetAvailableAppearancePanels() {
            foreach (AppearanceEditorProfile appearanceEditorProfile in systemDataFactory.GetResourceList<AppearanceEditorProfile>()) {
                if (appearanceEditorProfile.ModelProviderType != null) {
                    appearanceEditorPanelTypes.Add(appearanceEditorProfile.ModelProviderType, appearanceEditorProfile.Prefab);
                }
            }
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");

            base.ReceiveClosedWindowNotification();

            characterCreatorManager.OnUnitCreated -= HandleUnitCreated;
            characterCreatorManager.OnModelCreated -= HandleModelCreated;

            characterPreviewPanel.ReceiveClosedWindowNotification();
            
            foreach (AppearancePanel appearancePanel in appearanceEditorPanels.Values) {
                appearancePanel.ReceiveClosedWindowNotification();
            }
            defaultAppearancePanel.ReceiveClosedWindowNotification();

            characterCreatorInteractableManager.EndInteraction();
            
            // close interaction window too for smoother experience
            uIManager.interactionWindow.CloseWindow();

            characterCreatorManager.DisableLight();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();

            ProcessOpenWindow();
        }

        private void UpdateUnitProfile(UnitProfile unitProfile) {
            //Debug.Log("CharacterCreatorWindowPanel.UpdateUnitProfile(" + (unitProfile == null ? "null" : unitProfile.ResourceName) + ")");

            this.unitProfile = unitProfile;
            this.characterRace = unitProfile.CharacterRace;
        }

        public void SetUnitProfile(UnitProfile unitProfile) {
            UpdateUnitProfile(unitProfile);
            characterPreviewPanel.ReloadUnit();
        }

        private void ProcessOpenWindow() {

            ApplyInitialUnitProfile();

            characterCreatorManager.OnUnitCreated += HandleUnitCreated;
            characterCreatorManager.OnModelCreated += HandleModelCreated;

            characterPreviewPanel.CharacterConfigurationProvider = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();

            defaultAppearancePanel.ReceiveOpenWindowNotification();

            //saveButton.Button.interactable = false;
            uINavigationControllers[0].UpdateNavigationList();
            uINavigationControllers[0].FocusCurrentButton();

            /*
            if (characterCreatorManager.PreviewUnitController.UnitModelController.ModelCreated == true) {
                HandleModelCreated();
            }
            */
            characterCreatorManager.EnableLight();
        }

        private void ApplyInitialUnitProfile() {

            if (characterCreatorInteractableManager.CharacterCreator == null) {
                // the window was not opened from an interaction with a character creator. nothing to do
                return;
            }

            // set unit profile to default
            if (characterCreatorInteractableManager.CharacterCreator.Props.UnitProfileList.Count == 0) {
                if (characterCreatorInteractableManager.CharacterCreator.Props.AllowGenderChange == true) {
                    characterRace = playerManager.UnitController.UnitProfile.CharacterRace;
                }
                UpdateUnitProfile(playerManager.UnitController.UnitProfile);
                return;
            }

            if (characterCreatorInteractableManager.CharacterCreator.Props.UnitProfileList.Contains(playerManager.UnitController.UnitProfile)) {
                if (characterCreatorInteractableManager.CharacterCreator.Props.AllowGenderChange == true) {
                    characterRace = playerManager.UnitController.UnitProfile.CharacterRace;
                }
                UpdateUnitProfile(playerManager.UnitController.UnitProfile);
            } else {
                if (characterCreatorInteractableManager.CharacterCreator.Props.AllowGenderChange == true) {
                    characterRace = characterCreatorInteractableManager.CharacterCreator.Props.UnitProfileList[0].CharacterRace;
                }
                UpdateUnitProfile(characterCreatorInteractableManager.CharacterCreator.Props.UnitProfileList[0]);
            }

        }

        private void ActivateCorrectAppearancePanel() {
            //Debug.Log("NewGamePanel.ActivateCorrectAppearancePanel()");

            if (characterCreatorManager.PreviewUnitController.UnitProfile.UnitPrefabProps.ModelProvider == null) {
                currentAppearanceEditorPanel = defaultAppearancePanel;
                return;
            }

            //Debug.Log("provider type is " + characterCreatorManager.PreviewUnitController.UnitProfile.UnitPrefabProps.ModelProvider.GetType());

            if (appearanceEditorPanelTypes.ContainsKey(characterCreatorManager.PreviewUnitController.UnitProfile.UnitPrefabProps.ModelProvider.GetType()) == false) {
                currentAppearanceEditorPanel = defaultAppearancePanel;
                return;
            }

            GameObject panelPrefab = appearanceEditorPanelTypes[characterCreatorManager.PreviewUnitController.UnitProfile.UnitPrefabProps.ModelProvider.GetType()];

            if (appearanceEditorPanels.ContainsKey(panelPrefab) == false) {
                AppearancePanel appearancePanel = objectPooler.GetPooledObject(panelPrefab, panelParent.transform).GetComponent<AppearancePanel>();
                appearancePanel.Configure(systemGameManager);
                appearancePanel.SetParentPanel(this);
                appearancePanel.SetCharacterEditor(this);
                appearancePanel.ReceiveOpenWindowNotification();
                appearancePanel.transform.SetSiblingIndex(1);
                appearanceEditorPanels.Add(panelPrefab, appearancePanel);
                subPanels.Add(appearancePanel);
            }

            currentAppearanceEditorPanel = appearanceEditorPanels[panelPrefab];
            //currentAppearancePanel.SetupOptions();
        }

        /*
        private void ClosePanels() {
            foreach (AppearancePanel appearancePanel in appearanceEditorPanels.Values) {
                appearancePanel.HidePanel();
            }
            defaultAppearancePanel.HidePanel();
        }
        */

        /*
        private void OpenDefaultAppearanceEditorPanel() {
            defaultAppearancePanel.ShowPanel();
            currentAppearanceEditorPanel = defaultAppearancePanel;
        }
        */

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            currentAppearanceEditorPanel.DisablePanelDisplay();
            uIManager.characterCreatorWindow.CloseWindow();
        }

        public void SaveCharacter() {
            //Debug.Log("CharacterCreatorPanel.SaveCharacter()");

            if (characterCreatorManager.PreviewUnitController.UnitModelController != null) {
                characterCreatorManager.PreviewUnitController.UnitModelController.SaveAppearanceSettings(/*saveManager,*/ playerManager.PlayerCharacterSaveData.SaveData);
            }

            // Always despawn units if their appearance changes.
            Vector3 currentPlayerLocation = playerManager.ActiveUnitController.transform.position;
            levelManager.SetSpawnRotationOverride(playerManager.ActiveUnitController.transform.forward);
            playerManager.DespawnPlayerUnit();
            playerManager.PlayerCharacterSaveData.SaveData.unitProfileName = unitProfile.ResourceName;
            playerManager.SpawnPlayerUnit(currentPlayerLocation);
            if (playerManager.UnitController.CharacterAbilityManager != null) {
                playerManager.UnitController.CharacterAbilityManager.LearnDefaultAutoAttackAbility();
            }

            characterCreatorInteractableManager.ConfirmAction();
        }

        public void OpenAppearancePanel() {
            //Debug.Log("NewGamePanel.OpenAppearancePanel()");

            currentAppearanceEditorPanel.ShowPanel();
            SetOpenSubPanel(currentAppearanceEditorPanel, false);

            //if (openSubPanel != currentAppearanceEditorPanel) {
            //    ClosePanels(currentAppearanceEditorPanel);
            //    currentAppearanceEditorPanel.ShowPanel();
            //    SetOpenSubPanel(currentAppearanceEditorPanel, true);
            //}
        }

        public void HandleUnitCreated() {
            //Debug.Log("CharacterCreatorWindowPanel.HandleTargetCreated()");

            ActivateCorrectAppearancePanel();
            currentAppearanceEditorPanel.HandleUnitCreated();

            EquipCharacter();
        }

        private void EquipCharacter() {
            //Debug.Log("CharacterCreatorWindowPanel.EquipCharacter()");

            // only set saved appearance if displaying the same unit as the existing player unit
            if (playerManager.UnitController.UnitProfile == UnitProfile) {
                characterCreatorManager.PreviewUnitController.UnitModelController.SetInitialSavedAppearance(playerManager.PlayerCharacterSaveData.SaveData);
            }

            foreach (EquipmentSlotProfile equipmentSlotProfile in playerManager.UnitController.CharacterEquipmentManager.CurrentEquipment.Keys) {
                characterCreatorManager.PreviewUnitController.CharacterEquipmentManager.CurrentEquipment.Add(equipmentSlotProfile, playerManager.UnitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile]);
            }

        }

        public void HandleModelCreated() {
            //Debug.Log("CharacterCreatorWindowPanel.HandleTargetReady()");

            currentAppearanceEditorPanel.SetupOptions();
            
            OpenAppearancePanel();

            /*

            // showPanel makes same call as HandleTargetReady() so no need to call it
            if (currentAppearanceEditorPanel.MainNoOptionsArea.activeSelf == false) {
                saveButton.Button.interactable = true;
            } else {
                openSubPanel = null;
            }
            uINavigationControllers[0].UpdateNavigationList();
            if (activeSubPanel == null) {
                uINavigationControllers[0].FocusCurrentButton();
            }
            */

        }

        public CharacterConfigurationRequest GetCharacterConfigurationRequest() {
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(this);
            return characterConfigurationRequest;
        }

        /*
        public void LoadSavedAppearanceSettings() {
            Debug.Log("CharacterCreatorWindowPanel.LoadSavedAppearanceSettings()");
            characterCreatorManager.PreviewUnitController.UnitModelController.LoadSavedAppearanceSettings();
        }
        */

    }

}