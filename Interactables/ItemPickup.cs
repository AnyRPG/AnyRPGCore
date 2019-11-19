using AnyRPG;
﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
public class ItemPickup : InteractableOption {

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    [SerializeField]
    protected string itemName;

    [SerializeField]
    private float spawnTimer = 5f;

    private float currentTimer = 0f;

    public override bool Interact(CharacterUnit source) {
        bool returnValue = PickUp();
        PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        return returnValue;
    }

    public override void StopInteract() {
        base.StopInteract();
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

    /// <summary>
    /// Pick an item up off the ground and put it in the inventory
    /// </summary>
    bool PickUp () {

        //Debug.Log("picking up " + item.name);
        // old inventory system
        //bool wasPickedUp = Inventory.instance.Add(item);

        // new inventory system
        bool wasPickedUp = InventoryManager.MyInstance.AddItem(SystemItemManager.MyInstance.GetNewResource(itemName));

        if (wasPickedUp) {
                (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).RemoveInteractable(gameObject.GetComponent<Interactable>());

            interactable.DestroySpawn();
            StartCoroutine(StartSpawnCountdown());

            return true;
            // do this next part in base class.
            //Destroy(gameObject);
        }
        return false;
    }

    public override bool HasMiniMapText() {
        return false;
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

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }
}

}