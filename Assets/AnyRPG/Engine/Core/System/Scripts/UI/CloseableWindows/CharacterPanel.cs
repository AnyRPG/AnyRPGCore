using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class CharacterPanel : WindowContentController {

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        // buttons
        [SerializeField]
        private List<CharacterButton> characterButtons = new List<CharacterButton>();

        [SerializeField]
        private HighlightButton reputationButton = null;

        [SerializeField]
        private HighlightButton achievementsButton = null;

        [SerializeField]
        private HighlightButton skillsButton = null;

        [SerializeField]
        private HighlightButton currencyButton = null;

        [SerializeField]
        private HighlightButton petButton = null;

        [SerializeField]
        private TextMeshProUGUI statsDescription = null;

        [SerializeField]
        private CharacterPreviewPanelController characterPreviewPanel;

        [SerializeField]
        private Color emptySlotColor = new Color32(0, 0, 0, 0);

        [SerializeField]
        private Color fullSlotColor = new Color32(255, 255, 255, 255);

        // game manager references
        private PlayerManager playerManager = null;
        private SystemEventManager systemEventManager = null;
        private UIManager uIManager = null;
        private CharacterCreatorManager characterCreatorManager = null;
        private CameraManager cameraManager = null;
        private SaveManager saveManager = null;

        public CharacterButton SelectedButton { get; set; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (CharacterButton characterButton in characterButtons) {
                characterButton.Configure(systemGameManager);
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

            characterPreviewPanel.Configure(systemGameManager);
            reputationButton.Configure(systemGameManager);
            achievementsButton.Configure(systemGameManager);
            skillsButton.Configure(systemGameManager);
            currencyButton.Configure(systemGameManager);
            petButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
            uIManager = systemGameManager.UIManager;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            cameraManager = systemGameManager.CameraManager;
            saveManager = systemGameManager.SaveManager;
        }

        /*
        private void Start() {
            //Debug.Log("CharacterPanel.Start()");
        }
        */

        protected override void CreateEventSubscriptions() {
            //Debug.Log("CharacterPanel.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StartListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            if (playerManager.PlayerUnitSpawned == true) {
                ProcessPlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        protected override void CleanupEventSubscriptions() {
            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StopListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn()");
            if (playerManager != null && playerManager.MyCharacter != null && playerManager.MyCharacter.CharacterStats != null) {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): subscribing to statChanged event");
                playerManager.MyCharacter.CharacterStats.OnStatChanged += UpdateStatsDescription;
            } else {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): could not find characterstats");
            }
            systemEventManager.OnEquipmentChanged += HandleEquipmentChanged;

        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("CharacterPanel.HandlePlayerUnitDespawn()");
            if (playerManager != null && playerManager.MyCharacter != null && playerManager.MyCharacter.CharacterStats != null) {
                playerManager.MyCharacter.CharacterStats.OnStatChanged -= UpdateStatsDescription;
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

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterPanel.RecieveClosedWindowNotification()");
            base.RecieveClosedWindowNotification();
            characterCreatorManager.HandleCloseWindow();
            characterPreviewPanel.OnTargetCreated -= HandleTargetCreated;
            //characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.RecieveClosedWindowNotification();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("CharacterPanel.ReceiveOpenWindowNotification()");
            base.ReceiveOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            //SetPreviewTarget();
            UpdateStatsDescription();
            if (playerManager.MyCharacter != null) {
                uIManager.characterPanelWindow.SetWindowTitle(playerManager.MyCharacter.CharacterName);
            }
            characterPreviewPanel.OnTargetCreated += HandleTargetCreated;
            characterPreviewPanel.CapabilityConsumer = playerManager.MyCharacter;
            characterPreviewPanel.ReceiveOpenWindowNotification();

        }

        /*
        public void ResetDisplay() {
            //Debug.Log("CharacterPanel.ResetDisplay()");
            if (uIManager != null && uIManager.characterPanelWindow != null && uIManager.characterPanelWindow.IsOpen) {
                // reset display
                //characterPreviewPanel.ClearTarget();
                characterCreatorManager.HandleCloseWindow();

                // TODO : ADD CODE TO LOOP THROUGH BUTTONS AND RE-DISPLAY ANY ITEMS

                // update display
                SetPreviewTarget();
            }
        }
        */

        public void HandleEquipmentChanged(Equipment newEquipment, Equipment oldEquipment) {
            //Debug.Log("CharacterPanel.HandleEquipmentChanged(" + (newEquipment == null ? "null" : newEquipment.DisplayName) + ", " + (oldEquipment == null ? "null" : oldEquipment.DisplayName) + ")");
            if (uIManager != null && uIManager.characterPanelWindow != null && uIManager.characterPanelWindow.IsOpen) {
                //ResetDisplay();
                //characterPreviewPanel.ReloadUnit();
                if (oldEquipment != null) {
                    characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.Unequip(oldEquipment, true, true, false);
                }
                if (newEquipment != null) {
                    characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.Equip(newEquipment, null, true, true, false);
                }
                characterCreatorManager.PreviewUnitController.UnitModelController.BuildModelAppearance();
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
            updateString += "Name: " + playerManager.MyCharacter.CharacterName + "\n";
            updateString += "Class: " + (playerManager.MyCharacter.CharacterClass == null ? "None" : playerManager.MyCharacter.CharacterClass.DisplayName) + "\n";
            updateString += "Specialization: " + (playerManager.MyCharacter.ClassSpecialization == null ? "None" : playerManager.MyCharacter.ClassSpecialization.DisplayName) + "\n";
            updateString += "Faction: " + (playerManager.MyCharacter.Faction == null ? "None" : playerManager.MyCharacter.Faction.DisplayName) + "\n";
            updateString += "Unit Type: " + (playerManager.MyCharacter.UnitType == null ? "None" : playerManager.MyCharacter.UnitType.DisplayName) + "\n";
            updateString += "Race: " + (playerManager.MyCharacter.CharacterRace == null ? "None" : playerManager.MyCharacter.CharacterRace.DisplayName) + "\n";
            updateString += "Level: " + playerManager.MyCharacter.CharacterStats.Level + "\n";
            updateString += "Experience: " + playerManager.MyCharacter.CharacterStats.CurrentXP + " / " + LevelEquations.GetXPNeededForLevel(playerManager.MyCharacter.CharacterStats.Level) + "\n\n";

            foreach (string statName in playerManager.MyCharacter.CharacterStats.PrimaryStats.Keys) {
                updateString += statName + ": " + playerManager.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue;
                if (playerManager.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue != playerManager.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) {
                    updateString += " ( " + playerManager.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue +
                        ((playerManager.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue - playerManager.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) > 0 ? " <color=green>+" : " <color=red>") +
                        (playerManager.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue - playerManager.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) +
                        "</color> )";
                }
                updateString += "\n";
            }

            updateString += "\n";

            if (playerManager.MyCharacter.CharacterStats.PrimaryResource != null) {
                updateString += playerManager.MyCharacter.CharacterStats.PrimaryResource.DisplayName + ": " + playerManager.MyCharacter.CharacterStats.CurrentPrimaryResource + " / " + playerManager.MyCharacter.CharacterStats.MaxPrimaryResource + "\n\n";
            }

            updateString += "Amor: " + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue + "\n";
            /*
            updateString += "Armor: " + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue;
            if (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue != playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue) {
                updateString += " ( " + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue + " + <color=green>" + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].GetAddValue() + "</color> )";
            }
            */

            updateString += "Physical Power: " +
                (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue +
                playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue != playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue ||
                playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "SpellPower: " +
                (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue +
                playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue != playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue ||
                playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "Critical Hit Chance: " +
                playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue + "%";
            if (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue != playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) {
                updateString += " ( " +
                    playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue +
                    ((playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Accuracy: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Accuracy, playerManager.MyCharacter.CharacterStats) +"%";
            if (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue != playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) {
                updateString += " ( " +
                    playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue +
                    ((playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Attack/Casting Speed: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Speed, playerManager.MyCharacter.CharacterStats) + "%";
            if (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue != playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) {
                updateString += " ( "
                    //playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue +
                    + ((playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) > 0 ? "<color=green>+" : " + <color=red>")
                    + (playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - playerManager.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) + "%</color> )";
            }
            updateString += "\n";

            updateString += "Movement Speed: " + Mathf.Clamp(playerManager.MyCharacter.CharacterStats.RunSpeed, 0, playerManager.MaxMovementSpeed).ToString("F2") + " (m/s)\n\n";

            statsDescription.text = updateString;
        }

        /*
        private void SetPreviewTarget() {
            Debug.Log("CharacterPanel.SetPreviewTarget()");


            //spawn correct preview unit
            characterCreatorManager.HandleOpenWindow(playerManager.MyCharacter.UnitProfile);
            */
            // testing do this earlier
            //LoadSavedAppearanceSettings();

            /*
            if (cameraManager != null && cameraManager.CharacterPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (PreviewCameraController != null) {
                    //Debug.Log("CharacterPanel.SetPreviewTarget(): subscribing to OnTargetReady()");
                    PreviewCameraController.OnTargetReady += TargetReadyCallback;
                    PreviewCameraController.InitializeCamera(characterCreatorManager.PreviewUnitController);
                } else {
                    Debug.LogError("CharacterPanel.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
                }
            }
            */
            /*
        }
        */

        /*
        public void TargetReadyCallback() {
            //Debug.Log("CharacterPanel.TargetReadyCallback()");
            PreviewCameraController.OnTargetReady -= TargetReadyCallback;
            TargetReadyCallbackCommon();
        }


        public void LoadSavedAppearanceSettings() {
            characterCreatorManager.PreviewUnitController?.UnitModelController.LoadSavedAppearanceSettings();
        }
        */

        public void HandleTargetCreated() {
            //Debug.Log("CharacterPanel.HandleTargetCreated()");
            characterCreatorManager.PreviewUnitController?.UnitModelController.SetInitialSavedAppearance();
            CharacterEquipmentManager characterEquipmentManager = characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;

            // providers need to be set or equipment won't be able to be equipped
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetUnitType(playerManager.MyCharacter.UnitType, true, false, false);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterRace(playerManager.MyCharacter.CharacterRace, true, false, false);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterFaction(playerManager.MyCharacter.Faction, true, false, false);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterClass(playerManager.MyCharacter.CharacterClass, true, false, false);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetClassSpecialization(playerManager.MyCharacter.ClassSpecialization, true, false, false);

            if (characterEquipmentManager != null) {
                if (playerManager != null && playerManager.MyCharacter != null && playerManager.MyCharacter.CharacterEquipmentManager != null) {
                    
                    //characterEquipmentManager.CurrentEquipment = playerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment;
                    // testing new code to avoid just making a pointer to the player gear, which results in equip/unequip not working properly
                    characterEquipmentManager.CurrentEquipment.Clear();
                    foreach (EquipmentSlotProfile equipmentSlotProfile in playerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment.Keys) {
                        characterEquipmentManager.CurrentEquipment.Add(equipmentSlotProfile, playerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile]);
                    }
                }
            } else {
                Debug.Log("CharacterPanel.HandleTargetCreated(): could not find a characterEquipmentManager");
            }
        }

        /*
        public void TargetReadyCallbackCommon() {
            //Debug.Log("CharacterPanel.TargetReadyCallbackCommon()");

            CharacterEquipmentManager characterEquipmentManager = characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;
            if (characterEquipmentManager != null) {
                if (playerManager != null && playerManager.MyCharacter != null && playerManager.MyCharacter.CharacterEquipmentManager != null) {
                    characterEquipmentManager.CurrentEquipment = playerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment;
                    characterCreatorManager.PreviewUnitController.UnitModelController.EquipEquipmentModels(characterEquipmentManager);
                }
            } else {
                Debug.Log("CharacterPanel.TargetReadyCallbackCommon(): could not find a characterEquipmentManager");
            }
        }
        */


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