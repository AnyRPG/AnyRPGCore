using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FactionChangeInteractable : InteractableOption {

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    // the faction that this interactable option offers
    [SerializeField]
    private string factionName;

    public string MyFactionName { get => factionName; set => factionName = value; }

    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyFactionChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyFactionChangeInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyFactionChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyFactionChangeNamePlateImage : base.MyNamePlateImage); }

    protected override void Awake() {
        //Debug.Log("FactionChangeInteractable.Awake()");
        base.Awake();
    }

    protected override void Start() {
        //Debug.Log("FactionChangeInteractable.Start()");
        base.Start();
    }

    public void CleanupEventReferences(ICloseableWindowContents windowContents) {
        //Debug.Log(gameObject.name + ".FactionChangeInteractable.CleanupEventReferences(ICloseableWindowContents)");
        CleanupEventReferences();
    }

    public override void CleanupEventReferences() {
        //Debug.Log(gameObject.name + ".FactionChangeInteractable.CleanupEventReferences()");
        base.CleanupEventReferences();
        if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.factionChangeWindow != null && PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents != null && (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as NameChangePanelController) != null) {
            (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnConfirmAction -= HandleConfirmAction;
            (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnCloseWindowHandler -= CleanupEventReferences;
        }
        eventReferencesInitialized = false;
    }

    public override void HandleConfirmAction() {
        //Debug.Log(gameObject.name + ".FactionChangeInteractable.HandleConfirmAction()");
        base.HandleConfirmAction();

        // just to be safe
        CleanupEventReferences();
    }

    public override bool Interact(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".FactionChangeInteractable.Interact()");
        if (eventReferencesInitialized == true) {
            return false;
        }
        (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).Setup(MyFactionName);
        (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnConfirmAction += HandleConfirmAction;
        (PopupWindowManager.MyInstance.factionChangeWindow.MyCloseableWindowContents as FactionChangePanelController).OnCloseWindowHandler += CleanupEventReferences;
        eventReferencesInitialized = true;
        return true;
    }

    /// <summary>
    /// Pick an item up off the ground and put it in the inventory
    /// </summary>

    public override void StopInteract() {
        base.StopInteract();
        PopupWindowManager.MyInstance.factionChangeWindow.CloseWindow();
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
        text.color = Color.cyan;
        return true;
    }

    public override void OnDisable() {
        base.OnDisable();
        CleanupEventReferences();
    }

    public override int GetCurrentOptionCount() {
        //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
        return GetValidOptionCount();
    }

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }
}
