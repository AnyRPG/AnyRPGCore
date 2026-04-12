using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class CharacterPanel : WindowPanel {

        //public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };
        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        // buttons
        [SerializeField]
        private List<CharacterEquipmentButton> characterButtons = new List<CharacterEquipmentButton>();

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
        private PlayerManagerClient playerManagerClient = null;
        private UIManager uIManager = null;
        private CameraManager cameraManager = null;
        private SaveManager saveManager = null;
        private CharacterPanelManager characterPanelManager = null;

        public CharacterEquipmentButton SelectedButton { get; set; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (CharacterEquipmentButton characterButton in characterButtons) {
                //characterButton.Configure(systemGameManager);
                characterButton.EmptyBackGroundColor = emptySlotColor;
                characterButton.FullBackGroundColor = fullSlotColor;
                characterButton.CharacterPanel = this;
                characterButton.ParentPanel = this;
                //Debug.Log("CharacterPanel.Start(): checking icon");
                if (characterButton.EquipmentSlotProfile != null && characterButton.EquipmentSlotProfile.Icon != null) {
                    //Debug.Log("CharacterPanel.Start(): equipment slot profile is not null, setting icon");
                    characterButton.EmptySlotImage.sprite = characterButton.EquipmentSlotProfile.Icon;
                }
                characterButton.UpdateVisual(null);
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

            playerManagerClient = systemGameManager.PlayerManagerClient;
            uIManager = systemGameManager.UIManager;
            cameraManager = systemGameManager.CameraManager;
            saveManager = systemGameManager.SaveManager;
            characterPanelManager = systemGameManager.CharacterPanelManager;
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("CharacterPanel.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();
            systemEventManager.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            if (playerManagerClient.PlayerUnitSpawned == true) {
                ProcessPlayerUnitSpawn();
            }
        }

        protected override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            systemEventManager.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            systemEventManager.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
        }

        public void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn()");
            if (playerManagerClient != null && playerManagerClient.UnitController != null && playerManagerClient.UnitController.CharacterStats != null) {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): subscribing to statChanged event");
                playerManagerClient.UnitController.UnitEventController.OnStatChanged += UpdateStatsDescription;
            } else {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): could not find characterstats");
            }
            systemEventManager.OnAddEquipment += HandleAddEquipment;
            systemEventManager.OnRemoveEquipment += HandleRemoveEquipment;
        }

        public void HandlePlayerUnitDespawn(UnitController unitController) {
            //Debug.Log("CharacterPanel.HandlePlayerUnitDespawn()");
            if (playerManagerClient != null && playerManagerClient.UnitController != null && playerManagerClient.UnitController.CharacterStats != null) {
                playerManagerClient.UnitController.UnitEventController.OnStatChanged -= UpdateStatsDescription;
            }
            systemEventManager.OnAddEquipment -= HandleAddEquipment;
            systemEventManager.OnRemoveEquipment -= HandleRemoveEquipment;
        }

        public void UpdateCharacterButtons() {
            //Debug.Log("CharacterPanel.UpdateCharacterButtons");
            foreach (CharacterEquipmentButton characterButton in characterButtons) {
                if (characterButton != null) {
                    characterButton.UpdateVisual(playerManagerClient.UnitController);
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
            if (playerManagerClient.UnitController != null) {
                uIManager.characterPanelWindow.SetWindowTitle(playerManagerClient.UnitController.BaseCharacter.CharacterName);
            }
            SetPreviewTarget();
        }

        private void SetPreviewTarget() {
            //Debug.Log("CharacterPreviewPanelController.SetPreviewTarget()");

            if (cameraManager.CharacterPanelCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (previewCameraController != null) {
                    //previewCameraController.OnTargetReady += HandleTargetReady;
                    previewCameraController.InitializeCamera(characterPanelManager.UnitController);
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

        public void HandleEquipmentChanged() {
            //Debug.Log("CharacterPanel.HandleEquipmentChanged()");

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
            updateString += "Name: " + playerManagerClient.UnitController.BaseCharacter.CharacterName + "\n";
            updateString += "Class: " + (playerManagerClient.UnitController.BaseCharacter.CharacterClass == null ? "None" : playerManagerClient.UnitController.BaseCharacter.CharacterClass.DisplayName) + "\n";
            updateString += "Specialization: " + (playerManagerClient.UnitController.BaseCharacter.ClassSpecialization == null ? "None" : playerManagerClient.UnitController.BaseCharacter.ClassSpecialization.DisplayName) + "\n";
            updateString += "Faction: " + (playerManagerClient.UnitController.BaseCharacter.Faction == null ? "None" : playerManagerClient.UnitController.BaseCharacter.Faction.DisplayName) + "\n";
            updateString += "Unit Type: " + (playerManagerClient.UnitController.BaseCharacter.UnitType == null ? "None" : playerManagerClient.UnitController.BaseCharacter.UnitType.DisplayName) + "\n";
            updateString += "Race: " + (playerManagerClient.UnitController.BaseCharacter.CharacterRace == null ? "None" : playerManagerClient.UnitController.BaseCharacter.CharacterRace.DisplayName) + "\n";
            updateString += "Level: " + playerManagerClient.UnitController.CharacterStats.Level + "\n";
            updateString += "Experience: " + playerManagerClient.UnitController.CharacterStats.CurrentXP + " / " + LevelEquations.GetXPNeededForLevel(playerManagerClient.UnitController.CharacterStats.Level, systemConfigurationManager) + "\n\n";

            foreach (string statName in playerManagerClient.UnitController.CharacterStats.PrimaryStats.Keys) {
                updateString += statName + ": " + playerManagerClient.UnitController.CharacterStats.PrimaryStats[statName].CurrentValue;
                if (playerManagerClient.UnitController.CharacterStats.PrimaryStats[statName].CurrentValue != playerManagerClient.UnitController.CharacterStats.PrimaryStats[statName].BaseValue) {
                    updateString += " ( " + playerManagerClient.UnitController.CharacterStats.PrimaryStats[statName].BaseValue +
                        ((playerManagerClient.UnitController.CharacterStats.PrimaryStats[statName].CurrentValue - playerManagerClient.UnitController.CharacterStats.PrimaryStats[statName].BaseValue) > 0 ? " <color=green>+" : " <color=red>") +
                        (playerManagerClient.UnitController.CharacterStats.PrimaryStats[statName].CurrentValue - playerManagerClient.UnitController.CharacterStats.PrimaryStats[statName].BaseValue) +
                        "</color> )";
                }
                updateString += "\n";
            }

            updateString += "\n";

            if (playerManagerClient.UnitController.CharacterStats.PrimaryResource != null) {
                updateString += $"{playerManagerClient.UnitController.CharacterStats.PrimaryResource.DisplayName}: {playerManagerClient.UnitController.CharacterStats.CurrentPrimaryResource} / {playerManagerClient.UnitController.CharacterStats.MaxPrimaryResource}\n";
            }
            // add other resources
            foreach (KeyValuePair<PowerResource, PowerResourceNode> _powerResource in playerManagerClient.UnitController.CharacterStats.PowerResourceDictionary) {
                if (playerManagerClient.UnitController.CharacterStats.PrimaryResource == _powerResource.Key) {
                    continue;
                }
                updateString += $"{_powerResource.Key.DisplayName}: {_powerResource.Value.currentValue} / {playerManagerClient.UnitController.CharacterStats.GetPowerResourceMaxAmount(_powerResource.Key)}\n";
            }
            updateString += "\n";

            updateString += $"Armor: {playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue}\n";
            /*
            updateString += "Armor: " + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue;
            if (playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue != playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue) {
                updateString += " ( " + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue + " + <color=green>" + playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].GetAddValue() + "</color> )";
            }
            */

            updateString += "Physical Power: " +
                (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue +
                playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue != playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue ||
                playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "SpellPower: " +
                (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue +
                playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue != playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue ||
                playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "Critical Hit Chance: " +
                playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue + "%";
            if (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue != playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) {
                updateString += " ( " +
                    playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue +
                    ((playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Accuracy: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Accuracy, playerManagerClient.UnitController) +"%";
            if (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue != playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) {
                updateString += " ( " +
                    playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue +
                    ((playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Attack/Casting Speed: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Speed, playerManagerClient.UnitController) + "%";
            if (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue != playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) {
                updateString += " ( "
                    //playerManager.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue +
                    + ((playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) > 0 ? "<color=green>+" : " + <color=red>")
                    + (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) + "%</color> )";
            }
            updateString += "\n";

            updateString += "Movement Speed: " + Mathf.Clamp(playerManagerClient.UnitController.CharacterStats.RunSpeed, 0, systemConfigurationManager.MaxMovementSpeed).ToString("F2") + " (m/s)\n\n";

            if (systemConfigurationManager.UseEncumberance == true) {
                float currentWeightLoad = playerManagerClient.UnitController.CharacterEquipmentManager.EquippedWeight + playerManagerClient.UnitController.CharacterInventoryManager.Weight;
                float carryWeight = playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CarryWeight].CurrentValue + systemConfigurationManager.BaseCarryWeight;
                string colorString = currentWeightLoad > carryWeight ? "red" : "white";
                // 2 decimals accuracy should be enough for weight, and it looks better in the UI.  do not show decimals if the value is an integer to avoid cluttering the UI with unnecessary decimals.
                updateString += $"Carry Capacity (kg): <color={colorString}>{(currentWeightLoad % 1 == 0 ? currentWeightLoad.ToString("F0") : currentWeightLoad.ToString("F2"))} / {(carryWeight % 1 == 0 ? carryWeight.ToString("F0") : carryWeight.ToString("F2"))}</color>";
                if (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CarryWeight].CurrentValue != playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CarryWeight].BaseValue) {
                    updateString += " ( " +
                        (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CarryWeight].BaseValue) +
                        (((playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CarryWeight].CurrentValue + systemConfigurationManager.BaseCarryWeight) - (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CarryWeight].BaseValue + systemConfigurationManager.BaseCarryWeight)) > 0 ? " <color=green>+" : " <color=red>") +
                        ((playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CarryWeight].CurrentValue + systemConfigurationManager.BaseCarryWeight) - (playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CarryWeight].BaseValue + systemConfigurationManager.BaseCarryWeight)) +
                        "</color> )";
                }
                updateString += "\n";
            }

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