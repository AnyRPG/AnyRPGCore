using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class CharacterPanel : WindowContentController {

        [SerializeField]
        private List<CharacterButton> characterButtons = new List<CharacterButton>();

        [SerializeField]
        private TextMeshProUGUI statsDescription = null;

        [SerializeField]
        private CharacterPreviewCameraController previewCameraController;

        [SerializeField]
        private Color emptySlotColor = new Color32(0, 0, 0, 0);

        [SerializeField]
        private Color fullSlotColor = new Color32(255, 255, 255, 255);

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        public CharacterButton SelectedButton { get; set; }
        public CharacterPreviewCameraController PreviewCameraController { get => previewCameraController; set => previewCameraController = value; }

        private void Start() {
            //Debug.Log("CharacterPanel.Start()");
            CreateEventSubscriptions();

            foreach (CharacterButton characterButton in characterButtons) {
                characterButton.MyEmptyBackGroundColor = emptySlotColor;
                characterButton.MyFullBackGroundColor = fullSlotColor;
                characterButton.CharacterPanel = this;
                //Debug.Log("CharacterPanel.Start(): checking icon");
                if (characterButton.MyEquipmentSlotProfile != null && characterButton.MyEquipmentSlotProfile.Icon != null) {
                    //Debug.Log("CharacterPanel.Start(): equipment slot profile is not null, setting icon");
                    characterButton.MyEmptySlotImage.sprite = characterButton.MyEquipmentSlotProfile.Icon;
                }
                characterButton.UpdateVisual();
            }

        }

        protected override void CreateEventSubscriptions() {
            //Debug.Log("CharacterPanel.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            if (SystemGameManager.Instance.SystemEventManager != null) {
                SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.StartListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            }
            if (SystemGameManager.Instance.PlayerManager != null && SystemGameManager.Instance.PlayerManager.PlayerUnitSpawned == true) {
                ProcessPlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        protected override void CleanupEventSubscriptions() {
            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            if (SystemGameManager.Instance.SystemEventManager != null) {
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.StopListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            }
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn()");
            if (SystemGameManager.Instance.PlayerManager != null && SystemGameManager.Instance.PlayerManager.MyCharacter != null && SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats != null) {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): subscribing to statChanged event");
                SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.OnStatChanged += UpdateStatsDescription;
            } else {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): could not find characterstats");
            }
            if (SystemGameManager.Instance.SystemEventManager != null) {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): subscribing to statChanged event");
                SystemGameManager.Instance.SystemEventManager.OnEquipmentChanged += HandleEquipmentChanged;
            } else {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): could not find characterstats");
            }

        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("CharacterPanel.HandlePlayerUnitDespawn()");
            if (SystemGameManager.Instance.PlayerManager != null && SystemGameManager.Instance.PlayerManager.MyCharacter != null && SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats != null) {
                SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.OnStatChanged -= UpdateStatsDescription;
            }
            if (SystemGameManager.Instance.SystemEventManager != null) {
                SystemGameManager.Instance.SystemEventManager.OnEquipmentChanged -= HandleEquipmentChanged;
            } else {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): could not find characterstats");
            }
        }

        public void UpdateCharacterButtons() {
            //Debug.Log("CharacterPanel.UpdateCharacterButtons");
            foreach (CharacterButton characterButton in characterButtons) {
                if (characterButton != null) {
                    characterButton.UpdateVisual();
                }
            }
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterPanel.RecieveClosedWindowNotification()");
            base.RecieveClosedWindowNotification();
            SystemGameManager.Instance.CharacterCreatorManager.HandleCloseWindow();
            previewCameraController.ClearTarget();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("CharacterPanel.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            SetPreviewTarget();
            UpdateStatsDescription();
            if (SystemGameManager.Instance.PlayerManager.MyCharacter != null) {
                SystemGameManager.Instance.UIManager.PopupWindowManager.characterPanelWindow.SetWindowTitle(SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterName);
            }
        }

        public void ResetDisplay() {
            //Debug.Log("CharacterPanel.ResetDisplay()");
            if (SystemGameManager.Instance.UIManager.PopupWindowManager != null && SystemGameManager.Instance.UIManager.PopupWindowManager.characterPanelWindow != null && SystemGameManager.Instance.UIManager.PopupWindowManager.characterPanelWindow.IsOpen) {
                // reset display
                previewCameraController.ClearTarget();
                SystemGameManager.Instance.CharacterCreatorManager.HandleCloseWindow();

                // ADD CODE TO LOOP THROUGH BUTTONS AND RE-DISPLAY ANY ITEMS

                // update display
                SetPreviewTarget();
                //EquipmentManager.Instance.EquipCharacter(SystemGameManager.Instance.CharacterCreatorManager.MyPreviewUnit, false);
                //UpdateStatsDescription();
            }
        }

        public void HandleEquipmentChanged(Equipment newEquipment, Equipment oldEquipment) {
            //Debug.Log("CharacterPanel.HandleEquipmentChange()");
            if (SystemGameManager.Instance.UIManager.PopupWindowManager != null && SystemGameManager.Instance.UIManager.PopupWindowManager.characterPanelWindow != null && SystemGameManager.Instance.UIManager.PopupWindowManager.characterPanelWindow.IsOpen) {
                ResetDisplay();
                UpdateStatsDescription();
            }
        }

        public void UpdateStatsDescription() {
            //Debug.Log("CharacterPanel.UpdateStatsDescription");

            if (SystemGameManager.Instance.UIManager.PopupWindowManager.characterPanelWindow.IsOpen == false) {
                return;
            }

            // update images on character buttons
            UpdateCharacterButtons();

            if (statsDescription == null) {
                Debug.LogError("Must set statsdescription text in inspector!");
            }
            string updateString = string.Empty;
            updateString += "Name: " + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterName + "\n";
            updateString += "Class: " + (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterClass == null ? "None" : SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterClass.DisplayName) + "\n";
            updateString += "Specialization: " + (SystemGameManager.Instance.PlayerManager.MyCharacter.ClassSpecialization == null ? "None" : SystemGameManager.Instance.PlayerManager.MyCharacter.ClassSpecialization.DisplayName) + "\n";
            updateString += "Faction: " + (SystemGameManager.Instance.PlayerManager.MyCharacter.Faction == null ? "None" : SystemGameManager.Instance.PlayerManager.MyCharacter.Faction.DisplayName) + "\n";
            updateString += "Unit Type: " + (SystemGameManager.Instance.PlayerManager.MyCharacter.UnitType == null ? "None" : SystemGameManager.Instance.PlayerManager.MyCharacter.UnitType.DisplayName) + "\n";
            updateString += "Race: " + (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterRace == null ? "None" : SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterRace.DisplayName) + "\n";
            updateString += "Level: " + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.Level + "\n";
            updateString += "Experience: " + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.CurrentXP + " / " + LevelEquations.GetXPNeededForLevel(SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.Level) + "\n\n";

            foreach (string statName in SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryStats.Keys) {
                updateString += statName + ": " + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue;
                if (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue != SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) {
                    updateString += " ( " + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue +
                        ((SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue - SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) > 0 ? " <color=green>+" : " <color=red>") +
                        (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue - SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) +
                        "</color> )";
                }
                updateString += "\n";
            }

            updateString += "\n";

            if (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryResource != null) {
                updateString += SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.PrimaryResource.DisplayName + ": " + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.CurrentPrimaryResource + " / " + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.MaxPrimaryResource + "\n\n";
            }

            updateString += "Amor: " + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue + "\n";
            /*
            updateString += "Armor: " + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue;
            if (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue != SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue) {
                updateString += " ( " + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue + " + <color=green>" + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].GetAddValue() + "</color> )";
            }
            */

            updateString += "Physical Power: " +
                (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue +
                SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue != SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue ||
                SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "SpellPower: " +
                (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue +
                SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue != SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue ||
                SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "Critical Hit Chance: " +
                SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue + "%";
            if (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue != SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) {
                updateString += " ( " +
                    SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue +
                    ((SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Accuracy: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Accuracy, SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats) +"%";
            if (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue != SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) {
                updateString += " ( " +
                    SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue +
                    ((SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Attack/Casting Speed: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Speed, SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats) + "%";
            if (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue != SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) {
                updateString += " ( "
                    //SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue +
                    + ((SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) > 0 ? "<color=green>+" : " + <color=red>")
                    + (SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) + "%</color> )";
            }
            updateString += "\n";

            updateString += "Movement Speed: " + Mathf.Clamp(SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.RunSpeed, 0, SystemGameManager.Instance.PlayerManager.MaxMovementSpeed).ToString("F2") + " (m/s)\n\n";

            statsDescription.text = updateString;
        }

        private void SetPreviewTarget() {
            //Debug.Log("CharacterPanel.SetPreviewTarget()");


            //spawn correct preview unit
            SystemGameManager.Instance.CharacterCreatorManager.HandleOpenWindow(SystemGameManager.Instance.PlayerManager.MyCharacter.UnitProfile);

            // testing do this earlier
            LoadUMARecipe();

            if (SystemGameManager.Instance.CameraManager != null && SystemGameManager.Instance.CameraManager.CharacterPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (PreviewCameraController != null) {
                    PreviewCameraController.InitializeCamera(SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController);
                    PreviewCameraController.OnTargetReady += TargetReadyCallback;
                } else {
                    Debug.LogError("CharacterPanel.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
                }
            }
        }

        public void TargetReadyCallback() {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallback()");
            PreviewCameraController.OnTargetReady -= TargetReadyCallback;
            TargetReadyCallbackCommon();
        }

        public void LoadUMARecipe() {
            SystemGameManager.Instance.SaveManager.LoadUMASettings(SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.DynamicCharacterAvatar, false);

        }

        public void TargetReadyCallbackCommon() {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallbackCommon(" + updateCharacterButton + ")");

            CharacterEquipmentManager characterEquipmentManager = SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;
            if (characterEquipmentManager != null) {
                if (SystemGameManager.Instance.PlayerManager != null && SystemGameManager.Instance.PlayerManager.MyCharacter != null && SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterEquipmentManager != null) {
                    characterEquipmentManager.CurrentEquipment = SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment;
                    characterEquipmentManager.EquipEquipmentModels();
                }
            }
        }


        public void OpenReputationWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            SystemGameManager.Instance.UIManager.PopupWindowManager.reputationBookWindow.ToggleOpenClose();
        }

        public void OpenPetWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            SystemGameManager.Instance.UIManager.PopupWindowManager.characterPanelWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.petSpawnWindow.ToggleOpenClose();
        }

        public void OpenSkillsWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            SystemGameManager.Instance.UIManager.PopupWindowManager.skillBookWindow.ToggleOpenClose();
        }

        public void OpenCurrencyWindow() {
            //Debug.Log("CharacterPanel.OpenCurrencyWindow()");
            SystemGameManager.Instance.UIManager.PopupWindowManager.currencyListWindow.ToggleOpenClose();
        }

        public void OpenAchievementWindow() {
            //Debug.Log("CharacterPanel.OpenAchievementWindow()");
            SystemGameManager.Instance.UIManager.PopupWindowManager.achievementListWindow.ToggleOpenClose();
        }

    }

}