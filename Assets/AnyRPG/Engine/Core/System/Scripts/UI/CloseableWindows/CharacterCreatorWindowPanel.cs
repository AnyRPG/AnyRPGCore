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

        [SerializeField]
        private Button saveButton = null;

        //private string playerName = "Player Name";
        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private AnyRPGSaveData saveData;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public AnyRPGSaveData SaveData { get => saveData; set => saveData = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.RecieveClosedWindowNotification();
            umaCharacterPanel.RecieveClosedWindowNotification();
            OnCloseWindow(this);
            // close interaction window too for smoother experience
            SystemGameManager.Instance.UIManager.PopupWindowManager.interactionWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            saveButton.interactable = false;
            umaCharacterPanel.ReceiveOpenWindowNotification();
            umaCharacterPanel.ShowPanel();

            // set unit profile to default
            if (SystemGameManager.Instance.SystemConfigurationManager.UseFirstCreatorProfile) {
                unitProfile = SystemGameManager.Instance.SystemConfigurationManager.CharacterCreatorUnitProfile;
            } else {
                unitProfile = SystemGameManager.Instance.PlayerManager.ActiveCharacter.UnitProfile;
            }

            // inform the preview panel so the character can be rendered
            characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.CapabilityConsumer = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();

        }

        public void LoadUMARecipe() {
            //Debug.Log("CharacterCreatorWindowPanel.LoadUMARecipe()");
            //SystemGameManager.Instance.SaveManager.SaveUMASettings();
            SystemGameManager.Instance.SaveManager.LoadUMASettings(SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.DynamicCharacterAvatar, false);
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.characterCreatorWindow.CloseWindow();
        }

        public void SaveCharacter() {
            //Debug.Log("CharacterCreatorPanel.SaveCharacter()");

            if (SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.DynamicCharacterAvatar != null) {
                SystemGameManager.Instance.SaveManager.SaveUMASettings(SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.DynamicCharacterAvatar.GetCurrentRecipe());
            }

            // replace a default player unit with an UMA player unit when a save occurs
            // testing : old if statement would cause a character that switched between 2 UMA profiles to not get unit profile properties set
            // from the second profile.  Just go ahead and always despawn units if their appearance changes.
            //if (SystemGameManager.Instance.PlayerManager.UnitController.DynamicCharacterAvatar == null) {
            Vector3 currentPlayerLocation = SystemGameManager.Instance.PlayerManager.ActiveUnitController.transform.position;
            SystemGameManager.Instance.LevelManager.SpawnRotationOverride = SystemGameManager.Instance.PlayerManager.ActiveUnitController.transform.forward;
            SystemGameManager.Instance.PlayerManager.DespawnPlayerUnit();
            SystemGameManager.Instance.PlayerManager.MyCharacter.SetUnitProfile(unitProfile.DisplayName, true, -1, false);
            SystemGameManager.Instance.PlayerManager.SpawnPlayerUnit(currentPlayerLocation);
            if (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterAbilityManager != null) {
                SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterAbilityManager.LearnDefaultAutoAttackAbility();
            }

            //}
            // testing this is not needed because subscribing to the player unit spawn already handles this through the playermanager
            //SystemGameManager.Instance.SaveManager.LoadUMASettings();
            //ClosePanel();

            OnConfirmAction();
        }

        public void HandleTargetReady() {
            //Debug.Log("CharacterCreatorWindowPanel.HandleTargetReady()");
            LoadUMARecipe();
            umaCharacterPanel.HandleTargetReady();
            if (umaCharacterPanel.MainNoOptionsArea.activeSelf == false) {
                saveButton.interactable = true;
            }
        }

    }

}