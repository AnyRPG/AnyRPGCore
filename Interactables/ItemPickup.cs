using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : InteractableOption {

    public override event Action<IInteractable> MiniMapStatusUpdateHandler;

    public Item item;

    public override bool Interact(CharacterUnit source) {
        return PickUp();
    }

    public override void StopInteract() {
        base.StopInteract();
    }

    /// <summary>
    /// Pick an item up off the ground and put it in the inventory
    /// </summary>
    bool PickUp () {

        //Debug.Log("picking up " + item.name);
        // old inventory system
        //bool wasPickedUp = Inventory.instance.Add(item);

        // new inventory system
        bool wasPickedUp = InventoryManager.MyInstance.AddItem(item);

        if (wasPickedUp) {
            PlayerManager.MyInstance.MyCharacter.MyCharacterController.RemoveInteractable(gameObject.GetComponent<Interactable>());
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

}
