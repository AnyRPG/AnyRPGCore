using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA.CharacterSystem;


public class CharacterPanel : WindowContentController {

    #region Singleton
    private static CharacterPanel instance;

    public static CharacterPanel MyInstance
    {
        get
        {
            if (instance == null) {
                instance = FindObjectOfType<CharacterPanel>();
            }

            return instance;
        }
    }
    #endregion

    [SerializeField]
    private CharacterButton head, shoulders, chest, hands, legs, feet, mainhand, offhand;

    [SerializeField]
    private Text statsDescription;

    [SerializeField]
    private AnyRPGCharacterPreviewCameraController previewCameraController;

    [SerializeField]
    private Color emptySlotColor;

    [SerializeField]
    private Color fullSlotColor;

    protected bool startHasRun = false;
    protected bool eventReferencesInitialized = false;

    public override event Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };
    public override event Action<ICloseableWindowContents> OnCloseWindowHandler = delegate { };

    public CharacterButton MySelectedButton { get; set; }
    public AnyRPGCharacterPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }

    public override void Awake() {
        base.Awake();

    }

    private void Start() {
        //Debug.Log("CharacterPanel.Start()");
        startHasRun = true;
        CreateEventReferences();

        head.MyEmptyBackGroundColor = emptySlotColor;
        head.MyFullBackGroundColor = fullSlotColor;
        head.UpdateVisual();
        shoulders.MyEmptyBackGroundColor = emptySlotColor;
        shoulders.MyFullBackGroundColor = fullSlotColor;
        shoulders.UpdateVisual();
        chest.MyEmptyBackGroundColor = emptySlotColor;
        chest.MyFullBackGroundColor = fullSlotColor;
        chest.UpdateVisual();
        hands.MyEmptyBackGroundColor = emptySlotColor;
        hands.MyFullBackGroundColor = fullSlotColor;
        hands.UpdateVisual();
        legs.MyEmptyBackGroundColor = emptySlotColor;
        legs.MyFullBackGroundColor = fullSlotColor;
        legs.UpdateVisual();
        feet.MyEmptyBackGroundColor = emptySlotColor;
        feet.MyFullBackGroundColor = fullSlotColor;
        feet.UpdateVisual();
        mainhand.MyEmptyBackGroundColor = emptySlotColor;
        mainhand.MyFullBackGroundColor = fullSlotColor;
        mainhand.UpdateVisual();
        offhand.MyEmptyBackGroundColor = emptySlotColor;
        offhand.MyFullBackGroundColor = fullSlotColor;
        offhand.UpdateVisual();
    }

    protected virtual void CreateEventReferences() {
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
        }
        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
            HandlePlayerUnitSpawn();
        }
        eventReferencesInitialized = true;
    }

    protected virtual void CleanupEventReferences() {
        //Debug.Log("PlayerCombat.CleanupEventReferences()");
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
        }
    }

    public virtual void OnEnable() {
        CreateEventReferences();
    }

    public virtual void OnDestroy() {
        //Debug.Log("CharacterPanel.OnDestroy()");
        CleanupEventReferences();
    }

    public void HandlePlayerUnitSpawn() {
        //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn()");
        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterStats != null) {
            PlayerManager.MyInstance.MyCharacter.MyCharacterStats.OnStatChanged += UpdateStatsDescription;
        }
    }

    public void HandlePlayerUnitDespawn() {
        //Debug.Log("CharacterPanel.HandlePlayerUnitDespawn()");
        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterStats != null) {
            PlayerManager.MyInstance.MyCharacter.MyCharacterStats.OnStatChanged -= UpdateStatsDescription;
        }

        // 
        head.ClearButton(false);
        shoulders.ClearButton(false);
        chest.ClearButton(false);
        hands.ClearButton(false);
        legs.ClearButton(false);
        feet.ClearButton(false);
        mainhand.ClearButton(false);
        offhand.ClearButton(false);

    }

    public void EquipEquipment(Equipment newEquipment, bool partialEquip = false) {
        //Debug.Log("CharacterPanel.EquipEquipment(" + (newEquipment != null ? newEquipment.MyName : "null") + ", " + partialEquip + ")");
        switch (newEquipment.equipSlot) {
            case EquipmentSlot.Helm:
                head.EquipEquipment(newEquipment, partialEquip);
                break;
            case EquipmentSlot.Chest:
                chest.EquipEquipment(newEquipment, partialEquip);
                break;
            case EquipmentSlot.Legs:
                legs.EquipEquipment(newEquipment, partialEquip);
                break;
            case EquipmentSlot.MainHand:
                mainhand.EquipEquipment(newEquipment, partialEquip);
                break;
            case EquipmentSlot.OffHand:
                offhand.EquipEquipment(newEquipment, partialEquip);
                break;
            case EquipmentSlot.Feet:
                feet.EquipEquipment(newEquipment, partialEquip);
                break;
            case EquipmentSlot.Hands:
                hands.EquipEquipment(newEquipment, partialEquip);
                break;
            case EquipmentSlot.Shoulders:
                shoulders.EquipEquipment(newEquipment, partialEquip);
                break;
            default:
                break;
        }
    }

    public override void OnCloseWindow() {
        //Debug.Log("CharacterPanel.OnCloseWindow()");
        previewCameraController.ClearTarget();
        base.OnCloseWindow();
        CharacterCreatorManager.MyInstance.HandleCloseWindow();
    }

    public override void OnOpenWindow() {
        //Debug.Log("CharacterPanel.OnOpenWindow()");
        base.OnOpenWindow();
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

            // update display
            SetPreviewTarget(false);
            //EquipmentManager.MyInstance.EquipCharacter(CharacterCreatorManager.MyInstance.MyPreviewUnit, false);
            UpdateStatsDescription();
        }
    }

    public void UpdateStatsDescription() {
        //Debug.Log("CharacterPanel.UpdateStatsDescription");
        if (statsDescription == null) {
            Debug.LogError("Must set statsdescription text in inspector!");
        }
        string updateString = string.Empty;
        updateString += "Name: " + PlayerManager.MyInstance.MyCharacter.MyCharacterName + "\n";
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
        updateString += "Damage: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMeleeDamage + "\n";
        updateString += "SpellPower: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MySpellPower + "\n\n";
        updateString += "Movement Speed: " + Mathf.Clamp(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMovementSpeed, 0, PlayerManager.MyInstance.MyMaxMovementSpeed) + " (m/s)\n\n";

        statsDescription.text = updateString;
    }

    private void SetPreviewTarget(bool updateCharacterButtons = true) {
        //Debug.Log("CharacterPanel.SetPreviewTarget()");

        //spawn correct preview unit
        CharacterCreatorManager.MyInstance.HandleOpenWindow(false);

        if (CameraManager.MyInstance != null && CameraManager.MyInstance.MyCharacterPreviewCamera != null) {
            //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
            if (MyPreviewCameraController != null) {
                MyPreviewCameraController.InitializeCamera(CharacterCreatorManager.MyInstance.MyPreviewUnit.transform);
                if (updateCharacterButtons) {
                    MyPreviewCameraController.OnTargetReady += TargetReadyCallback;
                } else {
                    MyPreviewCameraController.OnTargetReady += TargetReadyCallbackReset;
                }
            } else {
                Debug.LogError("CharacterPanel.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
            }
        }
    }

    public void TargetReadyCallbackReset() {
        //Debug.Log("CharacterCreatorPanel.TargetReadyCallbackReset()");
        MyPreviewCameraController.OnTargetReady -= TargetReadyCallbackReset;
        TargetReadyCallbackCommon(false);
    }

    public void TargetReadyCallback() {
        //Debug.Log("CharacterCreatorPanel.TargetReadyCallback()");
        MyPreviewCameraController.OnTargetReady -= TargetReadyCallback;
        TargetReadyCallbackCommon(true);
    }

    public void TargetReadyCallbackCommon(bool updateCharacterButton = true) {
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
        EquipmentManager.MyInstance.EquipCharacter(CharacterCreatorManager.MyInstance.MyPreviewUnit, updateCharacterButton);

        // TESTING SEE WEAPONS AND ARMOR IN PLAYER PREVIEW SCRENE
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
