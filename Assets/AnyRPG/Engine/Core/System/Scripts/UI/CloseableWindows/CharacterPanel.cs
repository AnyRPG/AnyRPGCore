using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class CharacterPanel : WindowContentController {

        #region Singleton
        private static CharacterPanel instance;

        public static CharacterPanel Instance {
            get {
                return instance;
            }
        }

        private void Awake() {
            instance = this;
        }
        #endregion

        /*
        [SerializeField]
        private CharacterButton head, shoulders, chest, hands, legs, feet, mainhand, offhand;
        */

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

        public CharacterButton MySelectedButton { get; set; }
        public CharacterPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }

        private void Start() {
            //Debug.Log("CharacterPanel.Start()");
            CreateEventSubscriptions();

            foreach (CharacterButton characterButton in characterButtons) {
                characterButton.MyEmptyBackGroundColor = emptySlotColor;
                characterButton.MyFullBackGroundColor = fullSlotColor;
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
            if (SystemEventManager.Instance != null) {
                SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.StartListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            }
            if (PlayerManager.Instance != null && PlayerManager.Instance.PlayerUnitSpawned == true) {
                ProcessPlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        protected override void CleanupEventSubscriptions() {
            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            if (SystemEventManager.Instance != null) {
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
            if (PlayerManager.Instance != null && PlayerManager.Instance.MyCharacter != null && PlayerManager.Instance.MyCharacter.CharacterStats != null) {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): subscribing to statChanged event");
                PlayerManager.Instance.MyCharacter.CharacterStats.OnStatChanged += UpdateStatsDescription;
            } else {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): could not find characterstats");
            }
            if (SystemEventManager.Instance != null) {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): subscribing to statChanged event");
                SystemEventManager.Instance.OnEquipmentChanged += HandleEquipmentChanged;
            } else {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): could not find characterstats");
            }

        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("CharacterPanel.HandlePlayerUnitDespawn()");
            if (PlayerManager.Instance != null && PlayerManager.Instance.MyCharacter != null && PlayerManager.Instance.MyCharacter.CharacterStats != null) {
                PlayerManager.Instance.MyCharacter.CharacterStats.OnStatChanged -= UpdateStatsDescription;
            }
            if (SystemEventManager.Instance != null) {
                SystemEventManager.Instance.OnEquipmentChanged -= HandleEquipmentChanged;
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
            CharacterCreatorManager.Instance.HandleCloseWindow();
            previewCameraController.ClearTarget();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("CharacterPanel.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            SetPreviewTarget();
            UpdateStatsDescription();
            if (PlayerManager.Instance.MyCharacter != null) {
                PopupWindowManager.Instance.characterPanelWindow.SetWindowTitle(PlayerManager.Instance.MyCharacter.CharacterName);
            }
        }

        public void ResetDisplay() {
            //Debug.Log("CharacterPanel.ResetDisplay()");
            if (PopupWindowManager.Instance != null && PopupWindowManager.Instance.characterPanelWindow != null && PopupWindowManager.Instance.characterPanelWindow.IsOpen) {
                // reset display
                previewCameraController.ClearTarget();
                CharacterCreatorManager.Instance.HandleCloseWindow();

                // ADD CODE TO LOOP THROUGH BUTTONS AND RE-DISPLAY ANY ITEMS

                // update display
                SetPreviewTarget();
                //EquipmentManager.Instance.EquipCharacter(CharacterCreatorManager.Instance.MyPreviewUnit, false);
                //UpdateStatsDescription();
            }
        }

        public void HandleEquipmentChanged(Equipment newEquipment, Equipment oldEquipment) {
            //Debug.Log("CharacterPanel.HandleEquipmentChange()");
            if (PopupWindowManager.Instance != null && PopupWindowManager.Instance.characterPanelWindow != null && PopupWindowManager.Instance.characterPanelWindow.IsOpen) {
                ResetDisplay();
                UpdateStatsDescription();
            }
        }

        public void UpdateStatsDescription() {
            //Debug.Log("CharacterPanel.UpdateStatsDescription");

            if (PopupWindowManager.Instance.characterPanelWindow.IsOpen == false) {
                return;
            }

            // update images on character buttons
            UpdateCharacterButtons();

            if (statsDescription == null) {
                Debug.LogError("Must set statsdescription text in inspector!");
            }
            string updateString = string.Empty;
            updateString += "Name: " + PlayerManager.Instance.MyCharacter.CharacterName + "\n";
            updateString += "Class: " + (PlayerManager.Instance.MyCharacter.CharacterClass == null ? "None" : PlayerManager.Instance.MyCharacter.CharacterClass.DisplayName) + "\n";
            updateString += "Specialization: " + (PlayerManager.Instance.MyCharacter.ClassSpecialization == null ? "None" : PlayerManager.Instance.MyCharacter.ClassSpecialization.DisplayName) + "\n";
            updateString += "Faction: " + (PlayerManager.Instance.MyCharacter.Faction == null ? "None" : PlayerManager.Instance.MyCharacter.Faction.DisplayName) + "\n";
            updateString += "Unit Type: " + (PlayerManager.Instance.MyCharacter.UnitType == null ? "None" : PlayerManager.Instance.MyCharacter.UnitType.DisplayName) + "\n";
            updateString += "Race: " + (PlayerManager.Instance.MyCharacter.CharacterRace == null ? "None" : PlayerManager.Instance.MyCharacter.CharacterRace.DisplayName) + "\n";
            updateString += "Level: " + PlayerManager.Instance.MyCharacter.CharacterStats.Level + "\n";
            updateString += "Experience: " + PlayerManager.Instance.MyCharacter.CharacterStats.CurrentXP + " / " + LevelEquations.GetXPNeededForLevel(PlayerManager.Instance.MyCharacter.CharacterStats.Level) + "\n\n";

            foreach (string statName in PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryStats.Keys) {
                updateString += statName + ": " + PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue;
                if (PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue != PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) {
                    updateString += " ( " + PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue +
                        ((PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue - PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) > 0 ? " <color=green>+" : " <color=red>") +
                        (PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue - PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) +
                        "</color> )";
                }
                updateString += "\n";
            }

            updateString += "\n";

            if (PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryResource != null) {
                updateString += PlayerManager.Instance.MyCharacter.CharacterStats.PrimaryResource.DisplayName + ": " + PlayerManager.Instance.MyCharacter.CharacterStats.CurrentPrimaryResource + " / " + PlayerManager.Instance.MyCharacter.CharacterStats.MaxPrimaryResource + "\n\n";
            }

            updateString += "Amor: " + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue + "\n";
            /*
            updateString += "Armor: " + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue;
            if (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue != PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue) {
                updateString += " ( " + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue + " + <color=green>" + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].GetAddValue() + "</color> )";
            }
            */

            updateString += "Physical Power: " +
                (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue +
                PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue != PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue ||
                PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "SpellPower: " +
                (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue +
                PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue != PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue ||
                PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    (((PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) > 0 ? " <color=green>+" : " <color=red>") +
                    ((PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "Critical Hit Chance: " +
                PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue + "%";
            if (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue != PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) {
                updateString += " ( " +
                    PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue +
                    ((PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Accuracy: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Accuracy, PlayerManager.Instance.MyCharacter.CharacterStats) +"%";
            if (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue != PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) {
                updateString += " ( " +
                    PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue +
                    ((PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) > 0 ? " <color=green>+" : " <color=red>")
                    + (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Attack/Casting Speed: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Speed, PlayerManager.Instance.MyCharacter.CharacterStats) + "%";
            if (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue != PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) {
                updateString += " ( "
                    //PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue +
                    + ((PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) > 0 ? "<color=green>+" : " + <color=red>")
                    + (PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - PlayerManager.Instance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) + "%</color> )";
            }
            updateString += "\n";

            updateString += "Movement Speed: " + Mathf.Clamp(PlayerManager.Instance.MyCharacter.CharacterStats.RunSpeed, 0, PlayerManager.Instance.MaxMovementSpeed).ToString("F2") + " (m/s)\n\n";

            statsDescription.text = updateString;
        }

        private void SetPreviewTarget() {
            //Debug.Log("CharacterPanel.SetPreviewTarget()");


            //spawn correct preview unit
            CharacterCreatorManager.Instance.HandleOpenWindow(PlayerManager.Instance.MyCharacter.UnitProfile);

            // testing do this earlier
            LoadUMARecipe();

            if (CameraManager.Instance != null && CameraManager.Instance.CharacterPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (MyPreviewCameraController != null) {
                    MyPreviewCameraController.InitializeCamera(CharacterCreatorManager.Instance.PreviewUnitController);
                    MyPreviewCameraController.OnTargetReady += TargetReadyCallback;
                } else {
                    Debug.LogError("CharacterPanel.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
                }
            }
        }

        public void TargetReadyCallback() {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallback()");
            MyPreviewCameraController.OnTargetReady -= TargetReadyCallback;
            TargetReadyCallbackCommon();
        }

        public void LoadUMARecipe() {
            SaveManager.Instance.LoadUMASettings(CharacterCreatorManager.Instance.PreviewUnitController.DynamicCharacterAvatar, false);

        }

        public void TargetReadyCallbackCommon() {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallbackCommon(" + updateCharacterButton + ")");

            CharacterEquipmentManager characterEquipmentManager = CharacterCreatorManager.Instance.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;
            if (characterEquipmentManager != null) {
                if (PlayerManager.Instance != null && PlayerManager.Instance.MyCharacter != null && PlayerManager.Instance.MyCharacter.CharacterEquipmentManager != null) {
                    characterEquipmentManager.CurrentEquipment = PlayerManager.Instance.MyCharacter.CharacterEquipmentManager.CurrentEquipment;
                    characterEquipmentManager.EquipEquipmentModels();
                }
            }
        }


        public void OpenReputationWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            PopupWindowManager.Instance.reputationBookWindow.ToggleOpenClose();
        }

        public void OpenPetWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            PopupWindowManager.Instance.characterPanelWindow.CloseWindow();
            SystemWindowManager.Instance.petSpawnWindow.ToggleOpenClose();
        }

        public void OpenSkillsWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            PopupWindowManager.Instance.skillBookWindow.ToggleOpenClose();
        }

        public void OpenCurrencyWindow() {
            //Debug.Log("CharacterPanel.OpenCurrencyWindow()");
            PopupWindowManager.Instance.currencyListWindow.ToggleOpenClose();
        }

        public void OpenAchievementWindow() {
            //Debug.Log("CharacterPanel.OpenAchievementWindow()");
            PopupWindowManager.Instance.achievementListWindow.ToggleOpenClose();
        }

    }

}