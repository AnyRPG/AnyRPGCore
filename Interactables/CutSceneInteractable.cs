using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutSceneInteractable : InteractableOption {

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    [SerializeField]
    private string CutSceneName;

    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyCutSceneInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyCutSceneInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyCutSceneNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyCutSceneNamePlateImage : base.MyNamePlateImage); }

    protected override void Awake() {
        //Debug.Log("NameChangeInteractable.Awake()");
        base.Awake();
    }

    protected override void Start() {
        //Debug.Log("NameChangeInteractable.Start()");
        base.Start();
    }

    public void CleanupEventReferences(ICloseableWindowContents windowContents) {
        Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventReferences(ICloseableWindowContents)");
        CleanupEventReferences();
    }

    public override void CleanupEventReferences() {
        //Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventReferences()");
        base.CleanupEventReferences();
    }

    public override void HandleConfirmAction() {
        Debug.Log(gameObject.name + ".NameChangeInteractable.HandleConfirmAction()");
        base.HandleConfirmAction();

        // just to be safe
        CleanupEventReferences();
    }

    public override bool Interact(CharacterUnit source) {
        base.Interact(source);
        //Debug.Log(gameObject.name + ".CutSceneInteractable.Interact()");
        // save character position and stuff here
        //PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        LevelManager.MyInstance.LoadCutSceneWithDelay(CutSceneName);
        // TESTING, CLOSE WINDOWS BEFORE CUTSCENE LOADS TO PREVENT INVALID REFERENCE ON LOAD
        PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
        return true;
    }

    /// <summary>
    /// Pick an item up off the ground and put it in the inventory
    /// </summary>

    public override void StopInteract() {
        base.StopInteract();
        //PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
    }

    public override bool HasMiniMapText() {
        return true;
    }

    public override bool SetMiniMapText(Text text) {
        if (!base.SetMiniMapText(text)) {
            text.text = "";
            text.color = new Color32(0, 0, 0, 0);
            return false;
        }
        text.text = "o";
        text.fontSize = 50;
        text.color = Color.white;
        return true;
    }

    public override void OnDisable() {
        base.OnDisable();
        CleanupEventReferences();
    }


}
