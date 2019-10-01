﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreatorInteractable : InteractableOption {

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    [SerializeField]
    private GameObject spawnPrefab;

    private GameObject spawnReference;

    private BoxCollider boxCollider;


    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyCharacterCreatorInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyCharacterCreatorInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyCharacterCreatorNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyCharacterCreatorNamePlateImage : base.MyNamePlateImage); }

    protected override void Awake() {
        //Debug.Log("Portal.Awake()");
        base.Awake();
    }

    protected override void Start() {
        //Debug.Log("Portal.Start()")
        base.Start();
        boxCollider = GetComponent<BoxCollider>();
        Spawn();
    }

    private void Spawn() {
        //Debug.Log("Portal.Spawn(): Spawning " + spawnPrefab.name);
        if (spawnPrefab != null) {
            spawnReference = Instantiate(spawnPrefab, gameObject.transform);
        }
        boxCollider.enabled = true;
        //interactable.InitializeMaterials();
        MiniMapStatusUpdateHandler(this);
    }

    private void DestroySpawn() {
        Debug.Log("Portal.DestroySpawn(): Destroying " + spawnPrefab.name);
        if (spawnReference != null) {
            Destroy(spawnReference);
            spawnReference = null;
        }
        boxCollider.enabled = false;
        MiniMapStatusUpdateHandler(this);
    }

    public void CleanupEventReferences(ICloseableWindowContents windowContents) {
        CleanupEventReferences();
    }

    public override void CleanupEventReferences() {
        base.CleanupEventReferences();
        if (SystemWindowManager.MyInstance != null) {
            (SystemWindowManager.MyInstance.characterCreatorWindow.MyCloseableWindowContents as CharacterCreatorPanel).OnConfirmAction -= HandleConfirmAction;
            (SystemWindowManager.MyInstance.characterCreatorWindow.MyCloseableWindowContents as CharacterCreatorPanel).OnCloseWindowHandler -= CleanupEventReferences;
        }
    }

    public override bool Interact(CharacterUnit source) {
        SystemWindowManager.MyInstance.characterCreatorWindow.OpenWindow();
        (SystemWindowManager.MyInstance.characterCreatorWindow.MyCloseableWindowContents as CharacterCreatorPanel).OnConfirmAction += HandleConfirmAction;
        (SystemWindowManager.MyInstance.characterCreatorWindow.MyCloseableWindowContents as CharacterCreatorPanel).OnCloseWindowHandler += CleanupEventReferences;
        return true;
    }

    /// <summary>
    /// Pick an item up off the ground and put it in the inventory
    /// </summary>

    public override void StopInteract() {
        base.StopInteract();
        SystemWindowManager.MyInstance.characterCreatorWindow.CloseWindow();
    }

    public override bool HasMiniMapText() {
        return true;
    }

    public override bool SetMiniMapText(Text text) {
        Debug.Log(gameObject.name + ".CharacterCreatorInteractable.SetMiniMapText(" + text + ")");
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

    public override int GetCurrentOptionCount() {
        //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
        return GetValidOptionCount();
    }

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }
}