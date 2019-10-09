using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandScript : MonoBehaviour {

    #region Singleton
    private static HandScript instance;

    public static HandScript MyInstance
    {
        get
        {
            if (instance == null) {
                instance = FindObjectOfType<HandScript>();
            }

            return instance;
        }
    }
    #endregion

    public IMoveable MyMoveable { get; set; }

    private Image icon;

    [SerializeField]
    private Vector3 offset;

    // Start is called before the first frame update
    void Start() {
        //Debug.Log("HandScript.Start()");
        icon = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update() {
        icon.transform.position = Input.mousePosition+offset;
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && MyInstance.MyMoveable != null) {
            if (MyInstance.MyMoveable is Item) {
                SystemWindowManager.MyInstance.confirmDestroyMenuWindow.OpenWindow();
            } else if (MyInstance.MyMoveable is BaseAbility) {
                // DROP ABILITY SAFELY
                if (UIManager.MyInstance.MyActionBarManager.MyFromButton != null) {
                    UIManager.MyInstance.MyActionBarManager.MyFromButton.ClearUseable();
                }
                Drop();
            }
        }
        if (InputManager.MyInstance.KeyBindWasPressed("CANCEL")) {
            Drop();
        }
    }

    public void TakeMoveable(IMoveable moveable) {
        //Debug.Log("HandScript.TakeMoveable(" + moveable.ToString() + ")");
        this.MyMoveable = moveable;
        icon.sprite = moveable.MyIcon;
        icon.color = Color.white;
    }

    public IMoveable Put() {
        //Debug.Log("HandScript.Put().  Putting " + MyMoveable.ToString());
        IMoveable tmp = MyMoveable;
        ClearMoveable();

        return tmp;
    }

    public void Drop() {
        //Debug.Log("HandScript.Drop()");
        ClearMoveable();
        InventoryManager.MyInstance.FromSlot = null;
        UIManager.MyInstance.MyActionBarManager.MyFromButton = null;
    }

    private void ClearMoveable() {
        //Debug.Log("HandScript.ClearMoveable()");
        if (InventoryManager.MyInstance.FromSlot != null) {
            InventoryManager.MyInstance.FromSlot.PutItemBack();
        }
        MyMoveable = null;
        icon.sprite = null;
        icon.color = new Color(0, 0, 0, 0);
    }

    public void DeleteItem() {
        //Debug.Log("HandScript.DeleteItem()");
        if (MyMoveable is Item) {
            Item item = (Item)MyMoveable;
            if (item.MySlot != null) {
                item.MySlot.Clear();
            } else if (item.MyCharacterButton != null) {
                // this allows us to delete items directly by dropping them from the character panel onto the screen.
                // I may disallow this as I want items to only be deleted from the bag
                // this actually just unequips it for now since our inventorymanager actually puts an item back in the bag when dequipped.
                item.MyCharacterButton.DequipEquipment();
            }
        }
        CombatLogUI.MyInstance.WriteSystemMessage("Destroyed " + MyMoveable.MyName);
        Drop();
        // done in drop... ?
        //InventoryManager.MyInstance.FromSlot = null;
    }
}
