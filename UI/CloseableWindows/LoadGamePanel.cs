using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.Examples;
using UMA.CharacterSystem;
using UMA.CharacterSystem.Examples;


public class LoadGamePanel : WindowContentController {

    #region Singleton
    private static LoadGamePanel instance;

    public static LoadGamePanel MyInstance
    {
        get
        {
            if (instance == null) {
                instance = FindObjectOfType<LoadGamePanel>();
            }

            return instance;
        }
    }

    #endregion

    public event System.Action OnConfirmAction = delegate { };
    public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

    private LoadGameButton selectedLoadGameButton;

    [SerializeField]
    private AnyRPGCharacterPreviewCameraController previewCameraController;

    [SerializeField]
    private LayoutElement panelLayoutElement;

    [SerializeField]
    private GameObject buttonPrefab;

    [SerializeField]
    private GameObject buttonArea;

    private List<LoadGameButton> loadGameButtons = new List<LoadGameButton>();

    // hold data so changes are not reset on switch between male and female
    private string maleRecipe = string.Empty;
    private string femaleRecipe = string.Empty;

    private DynamicCharacterAvatar umaAvatar;

    private AnyRPGSaveData anyRPGSaveData;

    public AnyRPGCharacterPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }
    public LoadGameButton MySelectedLoadGameButton { get => selectedLoadGameButton; set => selectedLoadGameButton = value; }

    public override void RecieveClosedWindowNotification() {
        //Debug.Log("LoadGamePanel.OnCloseWindow()");
        base.RecieveClosedWindowNotification();
        umaAvatar = null;
        previewCameraController.ClearTarget();
        CharacterCreatorManager.MyInstance.HandleCloseWindow();
        //SaveManager.MyInstance.ClearSharedData();
        OnCloseWindow(this);
    }

    public override void ReceiveOpenWindowNotification() {
        //Debug.Log("LoadGamePanel.OnOpenWindow()");

        panelLayoutElement.preferredWidth = Screen.width;
        panelLayoutElement.preferredHeight = Screen.height;
        //Debug.Log("MainMapController.OnOpenWindow(); panelLayoutElement.preferredWidth: " + panelLayoutElement.preferredWidth);
        //Debug.Log("MainMapController.OnOpenWindow(); panelLayoutElement.preferredHeight: " + panelLayoutElement.preferredHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
        ShowLoadButtonsCommon();

    }

    public void ShowSavedGame(LoadGameButton loadGameButton) {
        //Debug.Log("LoadGamePanel.ShowSavedGame()");

        selectedLoadGameButton = loadGameButton;

        SaveManager.MyInstance.ClearSharedData();
        SaveManager.MyInstance.SetPlayerManagerPrefab(loadGameButton.MySaveData);

        ClearPreviewTarget();
        SetPreviewTarget();

        anyRPGSaveData = loadGameButton.MySaveData;
    }

    public void ClearPreviewTarget() {
        //Debug.Log("LoadGamePanel.ClearPreviewTarget()");
        // not really close window, but it will despawn the preview unit
        umaAvatar = null;
        CharacterCreatorManager.MyInstance.HandleCloseWindow();
    }


    public void ClearLoadButtons() {
        // clear the quest list so any quests left over from a previous time opening the window aren't shown
        //Debug.Log("LoadGamePanel.ClearLoadButtons()");
        foreach (LoadGameButton loadGameButton in loadGameButtons) {
            if (loadGameButton != null) {
                Destroy(loadGameButton.gameObject);
            }
        }
        loadGameButtons.Clear();
        MySelectedLoadGameButton = null;
    }


    public void ShowLoadButtonsCommon() {
        //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon()");
        ClearPreviewTarget();
        ClearLoadButtons();

        foreach (AnyRPGSaveData anyRPGSaveData in SaveManager.MyInstance.GetSaveDataList()) {
            //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
            GameObject go = Instantiate(buttonPrefab, buttonArea.transform);
            LoadGameButton loadGameButton = go.GetComponent<LoadGameButton>();
            loadGameButton.AddSaveData(anyRPGSaveData);
            //quests.Add(go);
            loadGameButtons.Add(loadGameButton);

        }
        if (loadGameButtons.Count > 0) {
            loadGameButtons[0].Select();
        }
        //SetPreviewTarget();
    }


    public void SetPreviewTarget() {
        Debug.Log("CharacterPanel.SetPreviewTarget()");
        if (umaAvatar != null) {
            Debug.Log("CharacterPanel.SetPreviewTarget() UMA avatar is already spawned!");
            return;
        }
        //spawn correct preview unit
        CharacterCreatorManager.MyInstance.HandleOpenWindow(false);

        if (CameraManager.MyInstance != null && CameraManager.MyInstance.MyCharacterPreviewCamera != null) {
            //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
            if (MyPreviewCameraController != null) {
                MyPreviewCameraController.InitializeCamera(CharacterCreatorManager.MyInstance.MyPreviewUnit.transform);
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting Target Ready Callback");
                MyPreviewCameraController.OnTargetReady += TargetReadyCallback;
            } else {
                Debug.LogError("CharacterPanel.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
            }
        }
    }

    public void TargetReadyCallback() {
        Debug.Log("CharacterCreatorPanel.TargetReadyCallback()");
        MyPreviewCameraController.OnTargetReady -= TargetReadyCallback;

        if (CharacterCreatorManager.MyInstance.MyPreviewUnit != null) {
            CharacterEquipmentManager characterEquipmentManager = CharacterCreatorManager.MyInstance.MyPreviewUnit.GetComponent<CharacterEquipmentManager>();
            if (characterEquipmentManager != null) {
                //SaveManager.MyInstance.LoadEquipmentData(loadGameButton.MySaveData, characterEquipmentManager);
                SaveManager.MyInstance.LoadEquipmentData(anyRPGSaveData, characterEquipmentManager);
            }
        }

        // get reference to avatar
        umaAvatar = CharacterCreatorManager.MyInstance.MyPreviewUnit.GetComponent<DynamicCharacterAvatar>();
        if (umaAvatar == null) {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallback() DID NOT get UMA avatar");
        } else {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallback() DID get UMA avatar");
        }

        // update character creator avatar to whatever recipe the actual character currently has, if any
        // disabled for now.  recipe should be already in recipestring anyway
        //SaveManager.MyInstance.SaveUMASettings();
        SaveManager.MyInstance.LoadUMASettings(umaAvatar);

        // FIX ME
        CharacterEquipmentManager previewUnitEquipmentManager = CharacterCreatorManager.MyInstance.MyPreviewUnit.GetComponent<CharacterEquipmentManager>();
        if (previewUnitEquipmentManager != null) {
            previewUnitEquipmentManager.EquipCharacter();
        }

        // SEE WEAPONS AND ARMOR IN PLAYER PREVIEW SCREEN
        CharacterCreatorManager.MyInstance.MyPreviewUnit.layer = 12;
        foreach (Transform childTransform in CharacterCreatorManager.MyInstance.MyPreviewUnit.GetComponentsInChildren<Transform>(true)) {
            childTransform.gameObject.layer = 12;
        }

        // new code for weapons

    }

    public void UnHighlightAllButtons() {
        //Debug.Log("CharacterCreatorPanel.UnHighlightAllButtons()");
    
    }

    public void ClosePanel() {
        //Debug.Log("CharacterCreatorPanel.ClosePanel()");
        SystemWindowManager.MyInstance.loadGameWindow.CloseWindow();
    }


    public void RebuildUMA() {
        //Debug.Log("CharacterCreatorPanel.RebuildUMA()");
        umaAvatar.BuildCharacter();
        //umaAvatar.BuildCharacter(true);
        //umaAvatar.ForceUpdate(true, true, true);
    }

    public void SaveCharacter() {
        Debug.Log("LoadGamePanel.SaveCharacter()");
        SaveManager.MyInstance.SaveUMASettings(umaAvatar.GetCurrentRecipe());

        // replace a default player unit with an UMA player unit when a save occurs
        if (PlayerManager.MyInstance.MyAvatar == null) {
            Vector3 currentPlayerLocation = PlayerManager.MyInstance.MyPlayerUnitObject.transform.position;
            PlayerManager.MyInstance.DespawnPlayerUnit();
            PlayerManager.MyInstance.SetUMAPrefab();
            PlayerManager.MyInstance.SpawnPlayerUnit(currentPlayerLocation);
        }
        SaveManager.MyInstance.LoadUMASettings();
        //ClosePanel();

        OnConfirmAction();
    }

    public void LoadGame() {
        SaveManager.MyInstance.LoadGame(MySelectedLoadGameButton.MySaveData);
    }

    public void NewGame() {
        //Debug.Log("LoadGamePanel.NewGame()");
        SystemWindowManager.MyInstance.newGameMenuWindow.OpenWindow();
    }

    public void DeleteGame() {
        SystemWindowManager.MyInstance.deleteGameMenuWindow.OpenWindow();
    }

    public void DeleteGame(bool confirmDelete = false) {
        if (confirmDelete) {
            SaveManager.MyInstance.DeleteGame(MySelectedLoadGameButton.MySaveData);
            SystemWindowManager.MyInstance.deleteGameMenuWindow.CloseWindow();
            ShowLoadButtonsCommon();
        } else {
            SystemWindowManager.MyInstance.deleteGameMenuWindow.OpenWindow();
        }
    }


}
