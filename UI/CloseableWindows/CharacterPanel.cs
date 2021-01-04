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

        public static CharacterPanel MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CharacterPanel>();
                }

                return instance;
            }
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

        protected bool eventSubscriptionsInitialized = false;

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
                } else {
                    if (characterButton.MyEquipmentSlotProfile == null) {
                        //Debug.Log("CharacterPanel.Start(): equipmentslotprofile is null");
                    }
                    if (characterButton.MyEquipmentSlotProfile.Icon == null) {
                        //Debug.Log("CharacterPanel.Start(): equipmentslotprofile.myicon is null");
                    }
                }
                characterButton.UpdateVisual();
            }

        }

        protected virtual void CreateEventSubscriptions() {
            //Debug.Log("CharacterPanel.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            }
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.PlayerUnitSpawned == true) {
                ProcessPlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        protected virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            }
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        public virtual void OnEnable() {
            CreateEventSubscriptions();
        }

        public virtual void OnDestroy() {
            //Debug.Log("CharacterPanel.OnDestroy()");
            CleanupEventSubscriptions();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn()");
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterStats != null) {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): subscribing to statChanged event");
                PlayerManager.MyInstance.MyCharacter.CharacterStats.OnStatChanged += UpdateStatsDescription;
            } else {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): could not find characterstats");
            }
            if (SystemEventManager.MyInstance != null) {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): subscribing to statChanged event");
                SystemEventManager.MyInstance.OnEquipmentChanged += HandleEquipmentChanged;
            } else {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): could not find characterstats");
            }

        }

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("CharacterPanel.HandlePlayerUnitDespawn()");
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterStats != null) {
                PlayerManager.MyInstance.MyCharacter.CharacterStats.OnStatChanged -= UpdateStatsDescription;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnEquipmentChanged -= HandleEquipmentChanged;
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
            CharacterCreatorManager.MyInstance.HandleCloseWindow();
            previewCameraController.ClearTarget();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("CharacterPanel.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            SetPreviewTarget();
            UpdateStatsDescription();
            if (PlayerManager.MyInstance.MyCharacter != null) {
                PopupWindowManager.MyInstance.characterPanelWindow.SetWindowTitle(PlayerManager.MyInstance.MyCharacter.CharacterName);
            }
        }

        public void ResetDisplay() {
            //Debug.Log("CharacterPanel.ResetDisplay()");
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.characterPanelWindow != null && PopupWindowManager.MyInstance.characterPanelWindow.IsOpen) {
                // reset display
                previewCameraController.ClearTarget();
                CharacterCreatorManager.MyInstance.HandleCloseWindow();

                // ADD CODE TO LOOP THROUGH BUTTONS AND RE-DISPLAY ANY ITEMS

                // update display
                SetPreviewTarget();
                //EquipmentManager.MyInstance.EquipCharacter(CharacterCreatorManager.MyInstance.MyPreviewUnit, false);
                //UpdateStatsDescription();
            }
        }

        public void HandleEquipmentChanged(Equipment newEquipment, Equipment oldEquipment) {
            //Debug.Log("CharacterPanel.HandleEquipmentChange()");
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.characterPanelWindow != null && PopupWindowManager.MyInstance.characterPanelWindow.IsOpen) {
                ResetDisplay();
                UpdateStatsDescription();
            }
        }

        public void UpdateStatsDescription() {
            //Debug.Log("CharacterPanel.UpdateStatsDescription");

            if (PopupWindowManager.MyInstance.characterPanelWindow.IsOpen == false) {
                return;
            }

            // update images on character buttons
            UpdateCharacterButtons();

            if (statsDescription == null) {
                Debug.LogError("Must set statsdescription text in inspector!");
            }
            string updateString = string.Empty;
            updateString += "Name: " + PlayerManager.MyInstance.MyCharacter.CharacterName + "\n";
            updateString += "Class: " + (PlayerManager.MyInstance.MyCharacter.CharacterClass == null ? "None" : PlayerManager.MyInstance.MyCharacter.CharacterClass.DisplayName) + "\n";
            updateString += "Specialization: " + (PlayerManager.MyInstance.MyCharacter.ClassSpecialization == null ? "None" : PlayerManager.MyInstance.MyCharacter.ClassSpecialization.DisplayName) + "\n";
            updateString += "Faction: " + (PlayerManager.MyInstance.MyCharacter.Faction == null ? "None" : PlayerManager.MyInstance.MyCharacter.Faction.DisplayName) + "\n";
            updateString += "Unit Type: " + (PlayerManager.MyInstance.MyCharacter.UnitType == null ? "None" : PlayerManager.MyInstance.MyCharacter.UnitType.DisplayName) + "\n";
            updateString += "Race: " + (PlayerManager.MyInstance.MyCharacter.CharacterRace == null ? "None" : PlayerManager.MyInstance.MyCharacter.CharacterRace.DisplayName) + "\n";
            updateString += "Level: " + PlayerManager.MyInstance.MyCharacter.CharacterStats.Level + "\n";
            updateString += "Experience: " + PlayerManager.MyInstance.MyCharacter.CharacterStats.CurrentXP + " / " + LevelEquations.GetXPNeededForLevel(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level) + "\n\n";

            foreach (string statName in PlayerManager.MyInstance.MyCharacter.CharacterStats.PrimaryStats.Keys) {
                updateString += statName + ": " + PlayerManager.MyInstance.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue;
                if (PlayerManager.MyInstance.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue != PlayerManager.MyInstance.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) {
                    updateString += " ( " + PlayerManager.MyInstance.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue +
                        " + <color=green>" +
                        (PlayerManager.MyInstance.MyCharacter.CharacterStats.PrimaryStats[statName].CurrentValue - PlayerManager.MyInstance.MyCharacter.CharacterStats.PrimaryStats[statName].BaseValue) +
                        "</color> )";
                }
                updateString += "\n";
            }

            updateString += "\n";

            if (PlayerManager.MyInstance.MyCharacter.CharacterStats.PrimaryResource != null) {
                updateString += PlayerManager.MyInstance.MyCharacter.CharacterStats.PrimaryResource.DisplayName + ": " + PlayerManager.MyInstance.MyCharacter.CharacterStats.CurrentPrimaryResource + " / " + PlayerManager.MyInstance.MyCharacter.CharacterStats.MaxPrimaryResource + "\n\n";
            }

            updateString += "Amor: " + PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue + "\n";
            /*
            updateString += "Armor: " + PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue;
            if (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue != PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue) {
                updateString += " ( " + PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].BaseValue + " + <color=green>" + PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].GetAddValue() + "</color> )";
            }
            */

            updateString += "Physical Power: " +
                (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue +
                PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue != PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue ||
                PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    " + <color=green>" +
                    ((PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].CurrentValue + PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.PhysicalDamage].BaseValue + PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "SpellPower: " +
                (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue +
                PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue);
            if (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue != PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue ||
                PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue != PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) {
                updateString += " ( " +
                    (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue) +
                    " + <color=green>" +
                    ((PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].CurrentValue + PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].CurrentValue) - (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.SpellDamage].BaseValue + PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Damage].BaseValue)) +
                    "</color> )";
            }
            updateString += "\n";

            updateString += "Critical Hit Chance: " +
                PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue + "%";
            if (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue != PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) {
                updateString += " ( " +
                    PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue +
                    " + <color=green>" + (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].CurrentValue - PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.CriticalStrike].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Accuracy: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Accuracy, PlayerManager.MyInstance.MyCharacter.CharacterStats) +"%";
            if (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue != PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) {
                updateString += " ( " +
                    PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue +
                    " + <color=green>" + (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].CurrentValue - PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Accuracy].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Attack/Casting Speed: " +
                LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Speed, PlayerManager.MyInstance.MyCharacter.CharacterStats) + "%";
            if (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue != PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) {
                updateString += " ( " +
                    PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue +
                    " + <color=green>" + (PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue - PlayerManager.MyInstance.MyCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].BaseValue) + "</color> )";
            }
            updateString += "\n";

            updateString += "Movement Speed: " + Mathf.Clamp(PlayerManager.MyInstance.MyCharacter.CharacterStats.RunSpeed, 0, PlayerManager.MyInstance.MaxMovementSpeed).ToString("F2") + " (m/s)\n\n";

            statsDescription.text = updateString;
        }

        private void SetPreviewTarget() {
            //Debug.Log("CharacterPanel.SetPreviewTarget()");


            //spawn correct preview unit
            CharacterCreatorManager.MyInstance.HandleOpenWindow(PlayerManager.MyInstance.MyCharacter.UnitProfile);

            // testing do this earlier
            LoadUMARecipe();

            if (CameraManager.MyInstance != null && CameraManager.MyInstance.CharacterPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (MyPreviewCameraController != null) {
                    MyPreviewCameraController.InitializeCamera(CharacterCreatorManager.MyInstance.PreviewUnitController);
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
            SaveManager.MyInstance.LoadUMASettings(CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar, false);

        }

        public void TargetReadyCallbackCommon() {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallbackCommon(" + updateCharacterButton + ")");

            CharacterEquipmentManager characterEquipmentManager = CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;
            if (characterEquipmentManager != null) {
                if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager != null) {
                    characterEquipmentManager.CurrentEquipment = PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.CurrentEquipment;
                    characterEquipmentManager.EquipCharacter();
                }
            }

            // SEE WEAPONS AND ARMOR IN PLAYER PREVIEW SCREEN
            // should not be necessary anymore - handled in unitcontroller
            //CharacterCreatorManager.MyInstance.PreviewUnitController.gameObject.layer = LayerMask.NameToLayer("PlayerPreview");
            /*
            foreach (Transform childTransform in CharacterCreatorManager.MyInstance.PreviewUnitController.GetComponentsInChildren<Transform>(true)) {
                childTransform.gameObject.layer = CharacterCreatorManager.MyInstance.PreviewUnitController.gameObject.layer;
            }
            */

            // new code for weapons
        }


        public void OpenReputationWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            PopupWindowManager.MyInstance.reputationBookWindow.ToggleOpenClose();
        }

        public void OpenPetWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            PopupWindowManager.MyInstance.characterPanelWindow.CloseWindow();
            SystemWindowManager.MyInstance.petSpawnWindow.ToggleOpenClose();
        }

        public void OpenSkillsWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            PopupWindowManager.MyInstance.skillBookWindow.ToggleOpenClose();
        }

        public void OpenCurrencyWindow() {
            //Debug.Log("CharacterPanel.OpenCurrencyWindow()");
            PopupWindowManager.MyInstance.currencyListWindow.ToggleOpenClose();
        }

        public void OpenAchievementWindow() {
            //Debug.Log("CharacterPanel.OpenAchievementWindow()");
            PopupWindowManager.MyInstance.achievementListWindow.ToggleOpenClose();
        }

    }

}