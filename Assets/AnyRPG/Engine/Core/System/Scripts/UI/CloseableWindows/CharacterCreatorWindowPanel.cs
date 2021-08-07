using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class CharacterCreatorWindowPanel : WindowContentController, ICapabilityConsumer {

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private CharacterPreviewPanelController characterPreviewPanel = null;

        [SerializeField]
        private UMACharacterEditorPanelController umaCharacterPanel = null;

        // appearance buttons
        [SerializeField]
        private HighlightButton faceButton = null;

        [SerializeField]
        private HighlightButton colorsButton = null;

        [SerializeField]
        private HighlightButton sexButton = null;

        [SerializeField]
        private HighlightButton maleButton = null;

        [SerializeField]
        private HighlightButton femaleButton = null;

        [SerializeField]
        private HighlightButton hairButton = null;

        [SerializeField]
        private HighlightButton skinButton = null;

        [SerializeField]
        private HighlightButton eyesButton = null;

        // bottom row
        [SerializeField]
        private HighlightButton closeButton = null;

        [SerializeField]
        private HighlightButton saveButton = null;

        //private string playerName = "Player Name";
        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private AnyRPGSaveData saveData;

        // game manager references
        private UIManager uIManager = null;
        private SystemConfigurationManager systemConfigurationManager = null;
        private PlayerManager playerManager = null;
        private CharacterCreatorManager characterCreatorManager = null;
        private SaveManager saveManager = null;
        private LevelManager levelManager = null;

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
            faceButton.Configure(systemGameManager);
            colorsButton.Configure(systemGameManager);
            sexButton.Configure(systemGameManager);
            maleButton.Configure(systemGameManager);
            femaleButton.Configure(systemGameManager);
            hairButton.Configure(systemGameManager);
            skinButton.Configure(systemGameManager);
            eyesButton.Configure(systemGameManager);
            closeButton.Configure(systemGameManager);
            saveButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            playerManager = systemGameManager.PlayerManager;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            saveManager = systemGameManager.SaveManager;
            levelManager = systemGameManager.LevelManager;
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.RecieveClosedWindowNotification();
            umaCharacterPanel.RecieveClosedWindowNotification();
            OnCloseWindow(this);
            // close interaction window too for smoother experience
            uIManager.interactionWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            saveButton.Button.interactable = false;
            umaCharacterPanel.ReceiveOpenWindowNotification();
            umaCharacterPanel.ShowPanel();

            // set unit profile to default
            if (systemConfigurationManager.UseFirstCreatorProfile) {
                unitProfile = systemConfigurationManager.CharacterCreatorUnitProfile;
            } else {
                unitProfile = playerManager.ActiveCharacter.UnitProfile;
            }

            // inform the preview panel so the character can be rendered
            characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.CapabilityConsumer = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();

        }

        public void LoadUMARecipe() {
            //Debug.Log("CharacterCreatorWindowPanel.LoadUMARecipe()");
            //saveManager.SaveUMASettings();
            saveManager.LoadUMASettings(characterCreatorManager.PreviewUnitController.DynamicCharacterAvatar, false);
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            uIManager.characterCreatorWindow.CloseWindow();
        }

        public void SaveCharacter() {
            //Debug.Log("CharacterCreatorPanel.SaveCharacter()");

            if (characterCreatorManager.PreviewUnitController.DynamicCharacterAvatar != null) {
                saveManager.SaveUMASettings(characterCreatorManager.PreviewUnitController.DynamicCharacterAvatar.GetCurrentRecipe());
            }

            // replace a default player unit with an UMA player unit when a save occurs
            // testing : old if statement would cause a character that switched between 2 UMA profiles to not get unit profile properties set
            // from the second profile.  Just go ahead and always despawn units if their appearance changes.
            //if (playerManager.UnitController.DynamicCharacterAvatar == null) {
            Vector3 currentPlayerLocation = playerManager.ActiveUnitController.transform.position;
            levelManager.SpawnRotationOverride = playerManager.ActiveUnitController.transform.forward;
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

            OnConfirmAction();
        }

        public void HandleTargetReady() {
            //Debug.Log("CharacterCreatorWindowPanel.HandleTargetReady()");
            LoadUMARecipe();
            umaCharacterPanel.HandleTargetReady();
            if (umaCharacterPanel.MainNoOptionsArea.activeSelf == false) {
                saveButton.Button.interactable = true;
            }
        }

    }

}