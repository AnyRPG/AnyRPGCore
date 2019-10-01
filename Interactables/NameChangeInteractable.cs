using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NameChangeInteractable : InteractableOption {

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    [SerializeField]
    private GameObject spawnPrefab;

    private GameObject spawnReference;

    private BoxCollider boxCollider;


    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyNameChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyNameChangeInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyNameChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyNameChangeNamePlateImage : base.MyNamePlateImage); }


    protected override void Awake() {
        //Debug.Log("NameChangeInteractable.Awake()");
        base.Awake();
    }

    protected override void Start() {
        //Debug.Log("NameChangeInteractable.Start()");
        base.Start();
        boxCollider = GetComponent<BoxCollider>();
        Spawn();
    }

    public void CleanupEventReferences(ICloseableWindowContents windowContents) {
        //Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventReferences(ICloseableWindowContents)");
        CleanupEventReferences();
    }

    public override void CleanupEventReferences() {
        //Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventReferences()");
        base.CleanupEventReferences();
        if (SystemWindowManager.MyInstance != null && SystemWindowManager.MyInstance.nameChangeWindow != null && SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents != null) {
            (SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents as NameChangePanelController).OnConfirmAction -= HandleConfirmAction;
            (SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents as NameChangePanelController).OnCloseWindowHandler -= CleanupEventReferences;
        }

    }

    public override void HandleConfirmAction() {
        //Debug.Log(gameObject.name + ".NameChangeInteractable.HandleConfirmAction()");
        base.HandleConfirmAction();

        // just to be safe
        CleanupEventReferences();
    }

    private void Spawn() {
        //Debug.Log(gameObject.name + ".NameChangeInteractable.Spawn()");
        if (spawnPrefab != null) {
            spawnReference = Instantiate(spawnPrefab, gameObject.transform);
        }
        if (boxCollider != null) {
            boxCollider.enabled = true;
        }
        //interactable.InitializeMaterials();
        MiniMapStatusUpdateHandler(this);
    }

    private void DestroySpawn() {
        Debug.Log(gameObject.name + ".NameChangeInteractable.DestroySpawn()");
        if (spawnReference != null) {
            Destroy(spawnReference);
            spawnReference = null;
        }
        boxCollider.enabled = false;
        MiniMapStatusUpdateHandler(this);
    }

    public override bool Interact(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".NameChangeInteractable.Interact()");
        if (eventReferencesInitialized == true) {
            Debug.Log(gameObject.name + ".NameChangeInteractable.Interact(): EVENT REFERENCES WERE ALREADY INITIALIZED!!! RETURNING");
            return false;
        }
        SystemWindowManager.MyInstance.nameChangeWindow.OpenWindow();
        (SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents as NameChangePanelController).OnConfirmAction += HandleConfirmAction;
        (SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents as NameChangePanelController).OnCloseWindowHandler += CleanupEventReferences;
        eventReferencesInitialized = true;
        return true;
    }



    /// <summary>
    /// Pick an item up off the ground and put it in the inventory
    /// </summary>

    public override void StopInteract() {
        base.StopInteract();
        SystemWindowManager.MyInstance.nameChangeWindow.CloseWindow();
    }

    public override bool HasMiniMapText() {
        return true;
    }

    public override bool SetMiniMapText(Text text) {
        //Debug.Log(gameObject.name + ".NameChangeInteractable.SetMiniMapText(" + text + ")");
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
        //Debug.Log(gameObject.name + ".NameChangeInteractable.GetCurrentOptionCount(): returning " + GetValidOptionCount());
        return GetValidOptionCount();
    }

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }
}
