using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA.CharacterSystem;

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
        private Text statsDescription;

        [SerializeField]
        private AnyRPGCharacterPreviewCameraController previewCameraController;

        [SerializeField]
        private Color emptySlotColor;

        [SerializeField]
        private Color fullSlotColor;

        protected bool eventSubscriptionsInitialized = false;

        public override event Action<ICloseableWindowContents> OnOpenWindow = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        public CharacterButton MySelectedButton { get; set; }
        public AnyRPGCharacterPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }

        private void Start() {
            //Debug.Log("CharacterPanel.Start()");
            CreateEventSubscriptions();

            foreach (CharacterButton characterButton in characterButtons) {
                characterButton.MyEmptyBackGroundColor = emptySlotColor;
                characterButton.MyFullBackGroundColor = fullSlotColor;
                //Debug.Log("CharacterPanel.Start(): checking icon");
                if (characterButton.MyEquipmentSlotProfile != null && characterButton.MyEquipmentSlotProfile.MyIcon != null) {
                    //Debug.Log("CharacterPanel.Start(): equipment slot profile is not null, setting icon");
                    characterButton.MyEmptySlotImage.sprite = characterButton.MyEquipmentSlotProfile.MyIcon;
                } else {
                    if (characterButton.MyEquipmentSlotProfile == null) {
                        //Debug.Log("CharacterPanel.Start(): equipmentslotprofile is null");
                    }
                    if (characterButton.MyEquipmentSlotProfile.MyIcon == null) {
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
                SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
                SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            }
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
                HandlePlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        protected virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            }
        }

        public virtual void OnEnable() {
            CreateEventSubscriptions();
        }

        public virtual void OnDestroy() {
            //Debug.Log("CharacterPanel.OnDestroy()");
            CleanupEventSubscriptions();
        }

        public void HandlePlayerUnitSpawn() {
            //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn()");
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterStats != null) {
                //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn(): subscribing to statChanged event");
                PlayerManager.MyInstance.MyCharacter.MyCharacterStats.OnStatChanged += UpdateStatsDescription;
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
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterStats != null) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterStats.OnStatChanged -= UpdateStatsDescription;
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
            //Debug.Log("CharacterPanel.OnCloseWindow()");
            previewCameraController.ClearTarget();
            base.RecieveClosedWindowNotification();
            CharacterCreatorManager.MyInstance.HandleCloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("CharacterPanel.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            SetPreviewTarget();
            UpdateStatsDescription();
            if (PlayerManager.MyInstance.MyCharacter != null) {
                PopupWindowManager.MyInstance.characterPanelWindow.SetWindowTitle(PlayerManager.MyInstance.MyCharacter.MyCharacterName);
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

            // update images on character buttons
            UpdateCharacterButtons();

            if (statsDescription == null) {
                Debug.LogError("Must set statsdescription text in inspector!");
            }
            string updateString = string.Empty;
            updateString += "Name: " + PlayerManager.MyInstance.MyCharacter.MyCharacterName + "\n";
            updateString += "Class: " + (PlayerManager.MyInstance.MyCharacter.MyCharacterClass == null ? "None" : PlayerManager.MyInstance.MyCharacter.MyCharacterClass.MyName) + "\n";
            updateString += "Level: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel + "\n";
            updateString += "Experience: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyCurrentXP + " / " + LevelEquations.GetXPNeededForLevel(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel) + "\n\n";
            updateString += "Stamina: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyStamina;
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyStamina != PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseStamina) {
                updateString += " ( " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseStamina + " + <color=green>" + (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyStamina - PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseStamina) + "</color> )";
            }
            updateString += "\n";
            updateString += "Strength: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyStrength;
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyStrength != PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseStrength) {
                updateString += " ( " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseStrength + " + <color=green>" + (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyStrength - PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseStrength) + "</color> )";
            }
            updateString += "\n";
            updateString += "Intellect: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyIntellect;
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyIntellect != PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseIntellect) {
                updateString += " ( " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseIntellect + " + <color=green>" + (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyIntellect - PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseIntellect) + "</color> )";
            }
            updateString += "\n";
            updateString += "Agility: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyAgility;
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyAgility != PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseAgility) {
                updateString += " ( " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseAgility + " + <color=green>" + (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyAgility - PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseAgility) + "</color> )";
            }

            updateString += "\n\n";
            updateString += "Health: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.currentHealth + " / " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMaxHealth + "\n";
            updateString += "Mana: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.currentMana + " / " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMaxMana + "\n\n";
            updateString += "Amor: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyArmor + "\n";
            updateString += "Damage: " + (LevelEquations.GetPhysicalPowerForCharacter(PlayerManager.MyInstance.MyCharacter) + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyPhysicalDamage);
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyPhysicalDamage != 0f) {
                updateString += " ( " + LevelEquations.GetPhysicalPowerForCharacter(PlayerManager.MyInstance.MyCharacter) + " + <color=green>" + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyPhysicalDamage + "</color> )";
            }
            /*
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMeleeDamage != PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseMeleeDamage) {
                updateString += " ( " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseMeleeDamage + " + <color=green>" + (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMeleeDamage - PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyBaseMeleeDamage) + "</color> )";
            }
            */
            updateString += "\n";
            updateString += "SpellPower: " + LevelEquations.GetSpellPowerForCharacter(PlayerManager.MyInstance.MyCharacter);
            updateString += "\n";
            updateString += "Critical Hit Chance: " + LevelEquations.GetCritChanceForCharacter(PlayerManager.MyInstance.MyCharacter) + "%\n\n";
            updateString += "Movement Speed: " + Mathf.Clamp(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMovementSpeed, 0, PlayerManager.MyInstance.MyMaxMovementSpeed).ToString("F2") + " (m/s)\n\n";

            statsDescription.text = updateString;
        }

        private void SetPreviewTarget() {
            //Debug.Log("CharacterPanel.SetPreviewTarget()");


            //spawn correct preview unit
            CharacterCreatorManager.MyInstance.HandleOpenWindow(PlayerManager.MyInstance.MyCharacter.MyUnitProfile);

            if (CameraManager.MyInstance != null && CameraManager.MyInstance.MyCharacterPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (MyPreviewCameraController != null) {
                    MyPreviewCameraController.InitializeCamera(CharacterCreatorManager.MyInstance.MyPreviewUnit.transform);
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

        public void TargetReadyCallbackCommon() {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallbackCommon(" + updateCharacterButton + ")");

            // get reference to avatar
            DynamicCharacterAvatar umaAvatar = CharacterCreatorManager.MyInstance.MyPreviewUnit.GetComponent<DynamicCharacterAvatar>();
            if (umaAvatar == null) {
                //Debug.Log("CharacterCreatorPanel.TargetReadyCallback() DID NOT get UMA avatar");
            } else {
                //Debug.Log("CharacterCreatorPanel.TargetReadyCallback() DID get UMA avatar");
            }

            // update character creator avatar to whatever recipe the actual character currently has, if any
            // disabled for now.  recipe should be already in recipestring anyway
            //SaveManager.MyInstance.SaveUMASettings();
            SaveManager.MyInstance.LoadUMASettings(umaAvatar);
            CharacterEquipmentManager characterEquipmentManager = CharacterCreatorManager.MyInstance.MyPreviewUnit.GetComponent<CharacterEquipmentManager>();
            if (characterEquipmentManager != null) {
                if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager != null) {
                    characterEquipmentManager.MyCurrentEquipment = PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment;
                    characterEquipmentManager.EquipCharacter();
                }
            }
            /*
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager != null) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.EquipCharacter();
            }
            */

            // SEE WEAPONS AND ARMOR IN PLAYER PREVIEW SCREEN
            CharacterCreatorManager.MyInstance.MyPreviewUnit.layer = 12;
            foreach (Transform childTransform in CharacterCreatorManager.MyInstance.MyPreviewUnit.GetComponentsInChildren<Transform>(true)) {
                childTransform.gameObject.layer = 12;
            }

            // new code for weapons
        }


        public void OpenReputationWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
            PopupWindowManager.MyInstance.reputationBookWindow.ToggleOpenClose();
        }

        public void OpenPetWindow() {
            //Debug.Log("CharacterPanel.OpenReputationWindow()");
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