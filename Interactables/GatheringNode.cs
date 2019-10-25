using AnyRPG;
﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
public class GatheringNode : InteractableOption {

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    // gathering nodes are special.  The image is based on what ability it supports
    public override Sprite MyIcon {
        get {
            return (MyAbility.MyIcon != null ? MyAbility.MyIcon : base.MyIcon);
        }
    }

    public override Sprite MyNamePlateImage {
        get {
            return (MyAbility.MyIcon != null ? MyAbility.MyIcon : base.MyNamePlateImage);
        }
    }
    public override string MyInteractionPanelTitle { get => (MyAbility != null ? MyAbility.MyName : base.MyInteractionPanelTitle); }


    /// <summary>
    /// The ability to cast in order to mine this node
    /// </summary>
    [SerializeField]
    private BaseAbility ability;

    private BaseAbility realAbility;

    [SerializeField]
    private GatherLootTable lootTable;

    [SerializeField]
    private float spawnTimer = 5f;

    private float currentTimer = 0f;

    public BaseAbility MyAbility { get => realAbility; }

    protected override void Awake() {
        //Debug.Log(gameObject.name + ".GatheringNode.Awake();");
        base.Awake();
        realAbility = SystemAbilityManager.MyInstance.GetResource(ability.MyName);
    }

    public override bool Interact(CharacterUnit source) {
        if (lootTable == null) {
            Debug.Log(gameObject.name + ".GatheringNode.Interact(" + source.name + "): lootTable was null!");
            return true;
        }
        if (lootTable.MyDroppedItems.Count > 0) {
            Gather();
        } else {
            source.GetComponent<CharacterUnit>().MyCharacter.MyCharacterAbilityManager.BeginAbility(MyAbility);
        }
        PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        return true;
        //return PickUp();
    }

    private IEnumerator StartSpawnCountdown() {
        //Debug.Log(gameObject.name + ".GatheringNode.StartSpawnCountdown()");
        // DISABLE MINIMAP ICON WHILE ITEM IS NOT SPAWNED
        HandlePrerequisiteUpdates();
        currentTimer = spawnTimer;
        while (currentTimer > 0) {
            //Debug.Log("Spawn Timer: " + currentTimer);
            currentTimer -= 1;
            yield return new WaitForSeconds(1);
        }
        interactable.Spawn();
        HandlePrerequisiteUpdates();
    }

    public void Gather() {
        PickUp();
    }

    /// <summary>
    /// Pick an item up off the ground and put it in the inventory
    /// </summary>
    void PickUp () {
        //Debug.Log("GatheringNode.Pickup()");
        LootUI.MyInstance.CreatePages(lootTable.GetLoot());
        CreateEventReferences();
        PopupWindowManager.MyInstance.lootWindow.OpenWindow();
    }

    public void ClearTakeLootHandler(ICloseableWindowContents windowContents) {
        CleanupEventReferences();
    }

    public void CreateEventReferences() {
        //Debug.Log("GatheringNode.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnTakeLoot += CheckDropListSize;
        LootUI.MyInstance.OnCloseWindow += ClearTakeLootHandler;
        eventReferencesInitialized = true;
    }

    public override void CleanupEventReferences() {
        //Debug.Log("GatheringNode.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnTakeLoot -= CheckDropListSize;
            LootUI.MyInstance.OnCloseWindow -= ClearTakeLootHandler;
        }
        eventReferencesInitialized = false;
    }

    public override void OnDisable() {
        base.OnDisable();
        CleanupEventReferences();
        StopAllCoroutines();
    }

    public void CheckDropListSize() {
        //Debug.Log("GatheringNode.CheckDropListSize()");
        if (lootTable.MyDroppedItems.Count == 0) {
            PlayerManager.MyInstance.MyCharacter.MyCharacterController.RemoveInteractable(gameObject.GetComponent<Interactable>());
            interactable.DestroySpawn();
            lootTable.Reset();
            StartCoroutine(StartSpawnCountdown());
        }
    }

    public override void StopInteract() {
        base.StopInteract();

        PopupWindowManager.MyInstance.lootWindow.CloseWindow();
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
        text.color = Color.blue;
        return true;
    }

    public override int GetCurrentOptionCount() {
        //Debug.Log(gameObject.name + ".GatheringNode.GetCurrentOptionCount()");
        return (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(MyAbility.MyName) == true && interactable.MySpawnReference != null ? 1 : 0);
    }

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }

    public override bool CanInteract(CharacterUnit source) {
        bool returnValue = base.CanInteract(source);
        if (returnValue == false) {
            return false;
        }
        return (GetCurrentOptionCount() == 0 ? false : true);
    }
}

}