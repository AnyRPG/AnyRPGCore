using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class InspectCharacterPanel : WindowPanel {

        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        // buttons
        [SerializeField]
        private List<CharacterEquipmentButtonBase> characterButtons = new List<CharacterEquipmentButtonBase>();

        [SerializeField]
        private TextMeshProUGUI statsDescription = null;

        [SerializeField]
        private PreviewCameraController previewCameraController = null;

        [SerializeField]
        private Color emptySlotColor = new Color32(0, 0, 0, 0);

        [SerializeField]
        private Color fullSlotColor = new Color32(255, 255, 255, 255);

        // game manager references
        private PlayerManager playerManager = null;
        private UIManager uIManager = null;
        private CameraManager cameraManager = null;
        private InspectCharacterService inspectCharacterService = null;
        private UnitPreviewManager unitPreviewManager = null;

        public CharacterEquipmentButton SelectedButton { get; set; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (CharacterEquipmentButtonBase characterButton in characterButtons) {
                //characterButton.Configure(systemGameManager);
                characterButton.EmptyBackGroundColor = emptySlotColor;
                characterButton.FullBackGroundColor = fullSlotColor;
                characterButton.ParentPanel = this;
                //Debug.Log("InspectCharacterPanel.Start(): checking icon");
                if (characterButton.EquipmentSlotProfile != null && characterButton.EquipmentSlotProfile.Icon != null) {
                    //Debug.Log("InspectCharacterPanel.Start(): equipment slot profile is not null, setting icon");
                    characterButton.EmptySlotImage.sprite = characterButton.EquipmentSlotProfile.Icon;
                }
                characterButton.UpdateVisual(null);
            }

            previewCameraController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            cameraManager = systemGameManager.CameraManager;
            unitPreviewManager = systemGameManager.UnitPreviewManager;
            inspectCharacterService = systemGameManager.InspectCharacterService;
        }

        public void UpdateCharacterButtons() {
            //Debug.Log("InspectCharacterPanel.UpdateCharacterButtons()");

            foreach (CharacterEquipmentButtonBase characterButton in characterButtons) {
                if (characterButton != null) {
                    characterButton.UpdateVisual(inspectCharacterService.TargetUnitController);
                }
            }
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("InspectCharacterPanel.RecieveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
            //characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            unitPreviewManager.OnUnitCreated -= HandleUnitCreated;
            previewCameraController.ClearTarget();
            ClearPreviewTarget();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("InspectCharacterPanel.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            //SetPreviewTarget();
            UpdateStatsDescription();
            if (inspectCharacterService.TargetUnitController != null) {
                uIManager.characterPanelWindow.SetWindowTitle(inspectCharacterService.TargetUnitController.BaseCharacter.CharacterName);
            }
            unitPreviewManager.OnUnitCreated += HandleUnitCreated;
            SetPreviewTarget();
        }

        private void HandleUnitCreated() {
            //Debug.Log("InspectCharacterPanel.HandleUnitCreated()");

            CharacterEquipmentManager characterEquipmentManager = unitPreviewManager.PreviewUnitController.CharacterEquipmentManager;

            if (characterEquipmentManager != null) {
                if (inspectCharacterService.TargetUnitController?.CharacterEquipmentManager != null) {

                    // testing new code to avoid just making a pointer to the player gear, which results in equip/unequip not working properly
                    characterEquipmentManager.ClearSubscriptions();
                    foreach (EquipmentSlotProfile equipmentSlotProfile in inspectCharacterService.TargetUnitController.CharacterEquipmentManager.CurrentEquipment.Keys) {
                        characterEquipmentManager.AddCurrentEquipmentSlot(equipmentSlotProfile, inspectCharacterService.TargetUnitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile]);
                    }
                    //characterEquipmentManager.CreateSubscriptions();
                }
            } else {
                Debug.LogWarning("InspectCharacterPanel.HandleUnitCreated(): could not find a characterEquipmentManager");
            }
        }

        public void ClearPreviewTarget() {
            //Debug.Log("LoadGamePanel.ClearPreviewTarget()");
            // not really close window, but it will despawn the preview unit
            unitPreviewManager.HandleCloseWindow();
        }

        public void SetPreviewTarget() {
            //Debug.Log("CharacterPanel.SetPreviewTarget()");

            if (unitPreviewManager.PreviewUnitController != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget() character is already spawned!");
                return;
            }

            //spawn correct preview unit
            //unitPreviewManager.OnTargetCreated += HandleTargetCreated;
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(inspectCharacterService.TargetUnitController.UnitProfile);
            characterConfigurationRequest.faction = inspectCharacterService.TargetUnitController.BaseCharacter.Faction;
            characterConfigurationRequest.characterClass = inspectCharacterService.TargetUnitController.BaseCharacter.CharacterClass;
            characterConfigurationRequest.classSpecialization = inspectCharacterService.TargetUnitController.BaseCharacter.ClassSpecialization;
            characterConfigurationRequest.unitLevel = inspectCharacterService.TargetUnitController.CharacterStats.Level;

            // if the game is in lobby mode, there will be no save data
            if (inspectCharacterService.TargetUnitController.CharacterSaveManager.SaveData != null) {
                characterConfigurationRequest.characterAppearanceData = new CharacterAppearanceData(inspectCharacterService.TargetUnitController.CharacterSaveManager.SaveData);
            }

            unitPreviewManager.HandleOpenWindow(characterConfigurationRequest);

            if (cameraManager.UnitPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (previewCameraController != null) {
                    previewCameraController.InitializeCamera(unitPreviewManager.PreviewUnitController);
                    //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting Target Ready Callback");
                    //previewCameraController.OnTargetReady += TargetReadyCallback;
                } else {
                    Debug.LogError("UnitSpawnController.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
                }
            }
        }

        /*
        public void HandleTargetReady() {
            //Debug.Log("CharacterPreviewPanelController.TargetReadyCallback()");
            previewCameraController.OnTargetReady -= HandleTargetReady;
            characterReady = true;

            OnTargetReady();
        }
        */

        /*
        public void HandleEquipmentChanged() {
            //Debug.Log("InspectCharacterPanel.HandleEquipmentChanged()");

            if (uIManager != null && uIManager.characterPanelWindow != null && uIManager.characterPanelWindow.IsOpen) {

                UpdateStatsDescription();
            }
        }

        private void HandleAddEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            HandleEquipmentChanged();
        }

        private void HandleRemoveEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            HandleEquipmentChanged();
        }
        */

        public void UpdateStatsDescription() {
            //Debug.Log("InspectCharacterPanel.UpdateStatsDescription()");

            // update images on character buttons
            UpdateCharacterButtons();

            if (statsDescription == null) {
                Debug.LogError("Must set statsdescription text in inspector!");
            }
            string updateString = string.Empty;
            updateString += "Name: " + inspectCharacterService.TargetUnitController.BaseCharacter.CharacterName + "\n";
            updateString += "Class: " + (inspectCharacterService.TargetUnitController.BaseCharacter.CharacterClass == null ? "None" : inspectCharacterService.TargetUnitController.BaseCharacter.CharacterClass.DisplayName) + "\n";
            updateString += "Specialization: " + (inspectCharacterService.TargetUnitController.BaseCharacter.ClassSpecialization == null ? "None" : inspectCharacterService.TargetUnitController.BaseCharacter.ClassSpecialization.DisplayName) + "\n";
            updateString += "Faction: " + (inspectCharacterService.TargetUnitController.BaseCharacter.Faction == null ? "None" : inspectCharacterService.TargetUnitController.BaseCharacter.Faction.DisplayName) + "\n";
            updateString += "Unit Type: " + (inspectCharacterService.TargetUnitController.BaseCharacter.UnitType == null ? "None" : inspectCharacterService.TargetUnitController.BaseCharacter.UnitType.DisplayName) + "\n";
            updateString += "Race: " + (inspectCharacterService.TargetUnitController.BaseCharacter.CharacterRace == null ? "None" : inspectCharacterService.TargetUnitController.BaseCharacter.CharacterRace.DisplayName) + "\n";
            updateString += "Level: " + inspectCharacterService.TargetUnitController.CharacterStats.Level + "\n";
            updateString += "Experience: " + inspectCharacterService.TargetUnitController.CharacterStats.CurrentXP + " / " + LevelEquations.GetXPNeededForLevel(inspectCharacterService.TargetUnitController.CharacterStats.Level, systemConfigurationManager) + "\n\n";

            foreach (string statName in inspectCharacterService.TargetUnitController.CharacterStats.PrimaryStats.Keys) {
                updateString += statName + ": " + inspectCharacterService.TargetUnitController.CharacterStats.PrimaryStats[statName].CurrentValue;
                if (inspectCharacterService.TargetUnitController.CharacterStats.PrimaryStats[statName].CurrentValue != inspectCharacterService.TargetUnitController.CharacterStats.PrimaryStats[statName].BaseValue) {
                    updateString += " ( " + inspectCharacterService.TargetUnitController.CharacterStats.PrimaryStats[statName].BaseValue +
                        ((inspectCharacterService.TargetUnitController.CharacterStats.PrimaryStats[statName].CurrentValue - inspectCharacterService.TargetUnitController.CharacterStats.PrimaryStats[statName].BaseValue) > 0 ? " <color=green>+" : " <color=red>") +
                        (inspectCharacterService.TargetUnitController.CharacterStats.PrimaryStats[statName].CurrentValue - inspectCharacterService.TargetUnitController.CharacterStats.PrimaryStats[statName].BaseValue) +
                        "</color> )";
                }
                updateString += "\n";
            }

            updateString += "\n";

            if (inspectCharacterService.TargetUnitController.CharacterStats.PrimaryResource != null) {
                updateString += inspectCharacterService.TargetUnitController.CharacterStats.PrimaryResource.DisplayName + ": " + inspectCharacterService.TargetUnitController.CharacterStats.CurrentPrimaryResource + " / " + inspectCharacterService.TargetUnitController.CharacterStats.MaxPrimaryResource + "\n\n";
            }

            updateString += "Amor: " + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue + "\n";
            /*
            updateString += "Armor: " + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue;
            if (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue != inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue) {
                updateString += " ( " + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue + " + <color=green>" + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].GetAddValue() + "</color> )";
            }
            */

            updateString += "Physical Power: " +
                (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue +
                inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue != inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue ||
                inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "SpellPower: " +
                (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue +
                inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue != inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue ||
                inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "Critical Hit Chance: " +
                inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue + "%";
            if (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue != inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) {
                updateString += " ( " +
                    inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue +
                    ((inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Accuracy: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Accuracy, inspectCharacterService.TargetUnitController) +"%";
            if (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue != inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) {
                updateString += " ( " +
                    inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue +
                    ((inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Attack/Casting Speed: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Speed, inspectCharacterService.TargetUnitController) + "%";
            if (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue != inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) {
                updateString += " ( "
                    //inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue +
                    + ((inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) > 0 ? "<color=green>+" : " + <color=red>")
                    + (inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - inspectCharacterService.TargetUnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) + "%</color> )";
            }
            updateString += "\n";

            updateString += "Movement Speed: " + Mathf.Clamp(inspectCharacterService.TargetUnitController.CharacterStats.RunSpeed, 0, playerManager.MaxMovementSpeed).ToString("F2") + " (m/s)\n\n";

            statsDescription.text = updateString;
        }

    }

}