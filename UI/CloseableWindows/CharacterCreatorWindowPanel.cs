using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class CharacterCreatorWindowPanel : WindowContentController, ICapabilityConsumer {

        #region Singleton
        private static CharacterCreatorWindowPanel instance;

        public static CharacterCreatorWindowPanel MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CharacterCreatorWindowPanel>();
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
        private UMACharacterEditorPanelController umaCharacterPanel = null;

        private string playerName = "Player Name";
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
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            umaCharacterPanel.ReceiveOpenWindowNotification();

            // set unit profile to default
            unitProfile = SystemConfigurationManager.MyInstance.CharacterCreatorUnitProfile;

            // inform the preview panel so the character can be rendered
            characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.CapabilityConsumer = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();
        }

        public void LoadUMARecipe() {
            //SaveManager.MyInstance.SaveUMASettings();
            SaveManager.MyInstance.LoadUMASettings(CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar, false);
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            SystemWindowManager.MyInstance.characterCreatorWindow.CloseWindow();
        }

        public void SaveCharacter() {
            //Debug.Log("CharacterCreatorPanel.SaveCharacter()");

            if (CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar != null) {
                SaveManager.MyInstance.SaveUMASettings(CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.GetCurrentRecipe());
            }

            // replace a default player unit with an UMA player unit when a save occurs
            if (PlayerManager.MyInstance.UnitController.DynamicCharacterAvatar == null) {
                Vector3 currentPlayerLocation = PlayerManager.MyInstance.ActiveUnitController.transform.position;
                PlayerManager.MyInstance.DespawnPlayerUnit();
                //PlayerManager.MyInstance.SetUMAPrefab();
                PlayerManager.MyInstance.MyCharacter.SetUnitProfile(SystemConfigurationManager.MyInstance.CharacterCreatorUnitProfileName);
                PlayerManager.MyInstance.SpawnPlayerUnit(currentPlayerLocation);
                if (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager != null) {
                    PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.LearnDefaultAutoAttackAbility();
                }

            }
            SaveManager.MyInstance.LoadUMASettings();
            //ClosePanel();

            OnConfirmAction();
        }

        public void HandleTargetReady() {
            LoadUMARecipe();
            umaCharacterPanel.HandleTargetReady();
        }

    }

}