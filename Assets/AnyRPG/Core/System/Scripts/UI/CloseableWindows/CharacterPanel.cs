using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class CharacterPanel : WindowContentController {

        //public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };
        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        // buttons
        [SerializeField]
        private List<CharacterButton> characterButtons = new List<CharacterButton>();

        /*
        [SerializeField]
        private HighlightButton reputationButton = null;

        [SerializeField]
        private HighlightButton achievementsButton = null;

        [SerializeField]
        private HighlightButton skillsButton = null;

        [SerializeField]
        private HighlightButton currencyButton = null;
        */

        [SerializeField]
        private HighlightButton petButton = null;

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
        private SystemEventManager systemEventManager = null;
        private UIManager uIManager = null;
        private CameraManager cameraManager = null;
        private SaveManager saveManager = null;
        private CharacterPanelManager characterPanelManager = null;

        public CharacterButton SelectedButton { get; set; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (CharacterButton characterButton in characterButtons) {
                //characterButton.Configure(systemGameManager);
                characterButton.EmptyBackGroundColor = emptySlotColor;
                characterButton.FullBackGroundColor = fullSlotColor;
                characterButton.CharacterPanel = this;
                //Debug.Log("CharacterPanel.Start(): checking icon");
                if (characterButton.EquipmentSlotProfile != null && characterButton.EquipmentSlotProfile.Icon != null) {
                    //Debug.Log("CharacterPanel.Start(): equipment slot profile is not null, setting icon");
                    characterButton.EmptySlotImage.sprite = characterButton.EquipmentSlotProfile.Icon;
                }
                characterButton.UpdateVisual();
            }

            previewCameraController.Configure(systemGameManager);
            //reputationButton.Configure(systemGameManager);
            //achievementsButton.Configure(systemGameManager);
            //skillsButton.Configure(systemGameManager);
            //currencyButton.Configure(systemGameManager);
            petButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
            uIManager = systemGameManager.UIManager;
            cameraManager = systemGameManager.CameraManager;
            saveManager = systemGameManager.SaveManager;
            characterPanelManager = systemGameManager.CharacterPanelManager;
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("CharacterPanel.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StartListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            if (playerManager.PlayerUnitSpawned == true) {
                ProcessPlayerUnitSpawn();
            }
        }

        protected override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StopListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log($"{gameObject.name}.InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn()");
            if (playerManager != null && playerManager.UnitController != null && playerManager.UnitController.CharacterStats != null) {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): subscribing to statChanged event");
                playerManager.UnitController.UnitEventController.OnStatChanged += UpdateStatsDescription;
            } else {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): could not find characterstats");
            }
            systemEventManager.OnEquipmentChanged += HandleEquipmentChanged;

        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("CharacterPanel.HandlePlayerUnitDespawn()");
            if (playerManager != null && playerManager.UnitController != null && playerManager.UnitController.CharacterStats != null) {
                playerManager.UnitController.UnitEventController.OnStatChanged -= UpdateStatsDescription;
            }
            systemEventManager.OnEquipmentChanged -= HandleEquipmentChanged;
        }

        public void UpdateCharacterButtons() {
            //Debug.Log("CharacterPanel.UpdateCharacterButtons");
            foreach (CharacterButton characterButton in characterButtons) {
                if (characterButton != null) {
                    characterButton.UpdateVisual();
                }
            }
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("CharacterPanel.RecieveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
            //characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            previewCameraController.ClearTarget();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("CharacterPanel.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            //SetPreviewTarget();
            UpdateStatsDescription();
            if (playerManager.UnitController != null) {
                uIManager.characterPanelWindow.SetWindowTitle(playerManager.UnitController.BaseCharacter.CharacterName);
            }
            SetPreviewTarget();
        }

        private void SetPreviewTarget() {
            //Debug.Log("CharacterPreviewPanelController.SetPreviewTarget()");

            if (cameraManager.CharacterPanelCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (previewCameraController != null) {
                    //previewCameraController.OnTargetReady += HandleTargetReady;
                    previewCameraController.InitializeCamera(characterPanelManager.PreviewUnitController);
                    //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting Target Ready Callback");
                } else {
                    Debug.LogError("CharacterPanel.SetPreviewTarget(): Character Panel Camera Controller is null. Please set it in the inspector");
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

        public void HandleEquipmentChanged(Equipment newEquipment, Equipment oldEquipment) {
            //Debug.Log("CharacterPanel.HandleEquipmentChanged(" + (newEquipment == null ? "null" : newEquipment.DisplayName) + ", " + (oldEquipment == null ? "null" : oldEquipment.DisplayName) + ")");
            if (uIManager != null && uIManager.characterPanelWindow != null && uIManager.characterPanelWindow.IsOpen) {

                UpdateStatsDescription();
            }
        }

        public void UpdateStatsDescription() {
            //Debug.Log("CharacterPanel.UpdateStatsDescription");

            if (uIManager.characterPanelWindow.IsOpen == false) {
                return;
            }

            // update images on character buttons
            UpdateCharacterButtons();

            if (statsDescription == null) {
                Debug.LogError("Must set statsdescription text in inspector!");
            }
            string updateString = string.Empty;
            updateString += "Name: " + playerManager.UnitController.BaseCharacter.CharacterName + "\n";
            updateString += "Class: " + (playerManager.UnitController.BaseCharacter.CharacterClass == null ? "None" : playerManager.UnitController.BaseCharacter.CharacterClass.DisplayName) + "\n";
            updateString += "Specialization: " + (playerManager.UnitController.BaseCharacter.ClassSpecialization == null ? "None" : playerManager.UnitController.BaseCharacter.ClassSpecialization.DisplayName) + "\n";
            updateString += "Faction: " + (playerManager.UnitController.BaseCharacter.Faction == null ? "None" : playerManager.UnitController.BaseCharacter.Faction.DisplayName) + "\n";
            updateString += "Unit Type: " + (playerManager.UnitController.BaseCharacter.UnitType == null ? "None" : playerManager.UnitController.BaseCharacter.UnitType.DisplayName) + "\n";
            updateString += "Race: " + (playerManager.UnitController.BaseCharacter.CharacterRace == null ? "None" : playerManager.UnitController.BaseCharacter.CharacterRace.DisplayName) + "\n";
            updateString += "Level: " + playerManager.UnitController.CharacterStats.Level + "\n";
            updateString += "Experience: " + playerManager.UnitController.CharacterStats.CurrentXP + " / " + LevelEquations.GetXPNeededForLevel(playerManager.UnitController.CharacterStats.Level, systemConfigurationManager) + "\n\n";

            foreach (string statName in playerManager.UnitController.CharacterStats.PrimaryStats.Keys) {
                updateString += statName + ": " + playerManager.UnitController.CharacterStats.PrimaryStats[statName].CurrentValue;
                if (playerManager.UnitController.CharacterStats.PrimaryStats[statName].CurrentValue != playerManager.UnitController.CharacterStats.PrimaryStats[statName].BaseValue) {
                    updateString += " ( " + playerManager.UnitController.CharacterStats.PrimaryStats[statName].BaseValue +
                        ((playerManager.UnitController.CharacterStats.PrimaryStats[statName].CurrentValue - playerManager.UnitController.CharacterStats.PrimaryStats[statName].BaseValue) > 0 ? " <color=green>+" : " <color=red>") +
                        (playerManager.UnitController.CharacterStats.PrimaryStats[statName].CurrentValue - playerManager.UnitController.CharacterStats.PrimaryStats[statName].BaseValue) +
                        "</color> )";
                }
                updateString += "\n";
            }

            updateString += "\n";

            if (playerManager.UnitController.CharacterStats.PrimaryResource != null) {
                updateString += playerManager.UnitController.CharacterStats.PrimaryResource.DisplayName + ": " + playerManager.UnitController.CharacterStats.CurrentPrimaryResource + " / " + playerManager.UnitController.CharacterStats.MaxPrimaryResource + "\n\n";
            }

            updateString += "Amor: " + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue + "\n";
            /*
            updateString += "Armor: " + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue;
            if (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue != playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue) {
                updateString += " ( " + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue + " + <color=green>" + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].GetAddValue() + "</color> )";
            }
            */

            updateString += "Physical Power: " +
                (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue +
                playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue != playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue ||
                playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "SpellPower: " +
                (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue +
                playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue != playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue ||
                playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "Critical Hit Chance: " +
                playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue + "%";
            if (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue != playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) {
                updateString += " ( " +
                    playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue +
                    ((playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Accuracy: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Accuracy, playerManager.UnitController) +"%";
            if (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue != playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) {
                updateString += " ( " +
                    playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue +
                    ((playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Attack/Casting Speed: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Speed, playerManager.UnitController) + "%";
            if (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue != playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) {
                updateString += " ( "
                    //playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue +
                    + ((playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) > 0 ? "<color=green>+" : " + <color=red>")
                    + (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) + "%</color> )";
            }
            updateString += "\n";

            updateString += "Movement Speed: " + Mathf.Clamp(playerManager.UnitController.CharacterStats.RunSpeed, 0, playerManager.MaxMovementSpeed).ToString("F2") + " (m/s)\n\n";

            statsDescription.text = updateString;
        }

        public void OpenReputationWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            uIManager.reputationBookWindow.ToggleOpenClose();
        }

        public void OpenPetWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            uIManager.characterPanelWindow.CloseWindow();
            uIManager.petSpawnWindow.ToggleOpenClose();
        }

        public void OpenSkillsWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            uIManager.skillBookWindow.ToggleOpenClose();
        }

        public void OpenCurrencyWindow() {
            //Debug.Log("CharacterPanel.OpenCurrencyWindow()");
            uIManager.currencyListWindow.ToggleOpenClose();
        }

        public void OpenAchievementWindow() {
            //Debug.Log("CharacterPanel.OpenAchievementWindow()");
            uIManager.achievementListWindow.ToggleOpenClose();
        }

    }

}