using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class CharacterCreatorWindowPanel : CloseableWindowContents, ICapabilityConsumer {

        [Header("Character Creator")]

        [SerializeField]
        private CharacterPreviewPanelController characterPreviewPanel = null;

        [SerializeField]
        private GameObject panelParent = null;

        [SerializeField]
        private DefaultAppearancePanel defaultAppearancePanel = null;

        [SerializeField]
        private HighlightButton saveButton = null;

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

            defaultAppearancePanel.SetCapabilityConsumer(this);

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
            characterPreviewPanel.OnTargetCreated -= HandleTargetCreated;
            characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.ReceiveClosedWindowNotification();
            
            foreach (AppearancePanel appearancePanel in appearanceEditorPanels.Values) {
                appearancePanel.ReceiveClosedWindowNotification();
            }
            defaultAppearancePanel.ReceiveClosedWindowNotification();

            characterCreatorInteractableManager.EndInteraction();
            
            // close interaction window too for smoother experience
            uIManager.interactionWindow.CloseWindow();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            saveButton.Button.interactable = false;
            uINavigationControllers[0].UpdateNavigationList();
            uINavigationControllers[0].FocusCurrentButton();

            //OpenAppearancePanel();

            // set unit profile to default
            if (systemConfigurationManager.UseFirstCreatorProfile) {
                unitProfile = systemConfigurationManager.CharacterCreatorUnitProfile;
            } else {
                unitProfile = playerManager.ActiveCharacter.UnitProfile;
            }


            // inform the preview panel so the character can be rendered
            characterPreviewPanel.OnTargetCreated += HandleTargetCreated;
            //characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.CapabilityConsumer = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();

            // do this last because we need to know what appearance panel to open from the character preview unit
            OpenAppearanceEditorPanel();

            characterPreviewPanel.OnTargetReady += HandleTargetReady;
            if (characterCreatorManager.PreviewUnitController.UnitModelController.ModelCreated == true) {
                HandleTargetReady();
            }
        }

        private void OpenAppearanceEditorPanel() {

            ClosePanels();

            if (unitProfile.UnitPrefabProps.ModelProvider == null) {
                OpenDefaultAppearanceEditorPanel();
                return;
            }

            //Debug.Log("provider type is " + unitProfile.UnitPrefabProps.ModelProvider.GetType());

            if (appearanceEditorPanelTypes.ContainsKey(unitProfile.UnitPrefabProps.ModelProvider.GetType()) == false) {
                OpenDefaultAppearanceEditorPanel();
                return;
            }

            GameObject panelPrefab = appearanceEditorPanelTypes[unitProfile.UnitPrefabProps.ModelProvider.GetType()];

            if (appearanceEditorPanels.ContainsKey(panelPrefab) == false) {
                AppearancePanel appearancePanel = objectPooler.GetPooledObject(panelPrefab, panelParent.transform).GetComponent<AppearancePanel>();
                appearancePanel.Configure(systemGameManager);
                appearancePanel.SetParentPanel(this);
                appearancePanel.SetCapabilityConsumer(this);
                appearancePanel.ReceiveOpenWindowNotification();
                appearancePanel.transform.SetSiblingIndex(1);
                appearanceEditorPanels.Add(panelPrefab, appearancePanel);
                subPanels.Add(appearancePanel);
                currentAppearanceEditorPanel = appearancePanel;
            }
            //appearanceEditorPanels[unitProfile.UnitPrefabProps.ModelProvider.AppearancePanel].ShowPanel();
            SetOpenSubPanel(appearanceEditorPanels[panelPrefab], true);
        }

        private void ClosePanels() {
            foreach (AppearancePanel appearancePanel in appearanceEditorPanels.Values) {
                appearancePanel.HidePanel();
            }
            defaultAppearancePanel.HidePanel();
        }

        private void OpenDefaultAppearanceEditorPanel() {
            defaultAppearancePanel.ShowPanel();
            currentAppearanceEditorPanel = defaultAppearancePanel;
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            uIManager.characterCreatorWindow.CloseWindow();
        }

        public void SaveCharacter() {
            //Debug.Log("CharacterCreatorPanel.SaveCharacter()");

            if (characterCreatorManager.PreviewUnitController.UnitModelController != null) {
                characterCreatorManager.PreviewUnitController.UnitModelController.SaveAppearanceSettings(saveManager.CurrentSaveData);
            }

            // replace a default player unit with an UMA player unit when a save occurs
            // testing : old if statement would cause a character that switched between 2 UMA profiles to not get unit profile properties set
            // from the second profile.  Just go ahead and always despawn units if their appearance changes.
            //if (playerManager.UnitController.DynamicCharacterAvatar == null) {
            Vector3 currentPlayerLocation = playerManager.ActiveUnitController.transform.position;
            levelManager.SetSpawnRotationOverride(playerManager.ActiveUnitController.transform.forward);
            playerManager.DespawnPlayerUnit();
            playerManager.MyCharacter.SetUnitProfile(unitProfile.DisplayName, true, -1, false);
            playerManager.SpawnPlayerUnit(currentPlayerLocation);
            if (playerManager.MyCharacter.CharacterAbilityManager != null) {
                playerManager.MyCharacter.CharacterAbilityManager.LearnDefaultAutoAttackAbility();
            }

            //}
            // testing this is not needed because subscribing to the player unit spawn already handles this through the playermanager
            //saveManager.LoadUMASettings();
            //ClosePanel();

            characterCreatorInteractableManager.ConfirmAction();
        }

        public void HandleTargetCreated() {
            //Debug.Log("CharacterCreatorWindowPanel.HandleTargetCreated()");

            characterCreatorManager.PreviewUnitController.UnitModelController.SetInitialSavedAppearance(saveManager.CurrentSaveData);
            foreach (EquipmentSlotProfile equipmentSlotProfile in playerManager.ActiveCharacter.CharacterEquipmentManager.CurrentEquipment.Keys) {
                characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.CurrentEquipment.Add(equipmentSlotProfile, playerManager.ActiveCharacter.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile]);
            }
        }

        public void HandleTargetReady() {
            //Debug.Log("CharacterCreatorWindowPanel.HandleTargetReady()");
            //LoadSavedAppearanceSettings();

            currentAppearanceEditorPanel.ShowPanel();

            // showPanel makes same call as HandleTargetReady() so no need to call it
            //currentAppearanceEditorPanel.HandleTargetReady();
            if (currentAppearanceEditorPanel.MainNoOptionsArea.activeSelf == false) {
                saveButton.Button.interactable = true;
            } else {
                openSubPanel = null;
            }
            uINavigationControllers[0].UpdateNavigationList();
            if (activeSubPanel == null) {
                uINavigationControllers[0].FocusCurrentButton();
            }
            
        }

        /*
        public void LoadSavedAppearanceSettings() {
            Debug.Log("CharacterCreatorWindowPanel.LoadSavedAppearanceSettings()");
            characterCreatorManager.PreviewUnitController.UnitModelController.LoadSavedAppearanceSettings();
        }
        */

    }

}