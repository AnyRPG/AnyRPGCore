using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class HandScript : MonoBehaviour {

        #region Singleton
        private static HandScript instance;

        public static HandScript MyInstance {
            get {
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
        private Vector3 offset = Vector3.zero;

        // Start is called before the first frame update
        void Start() {
            //Debug.Log("HandScript.Start()");
            icon = GetComponent<Image>();
        }

        // Update is called once per frame
        void Update() {
            icon.transform.position = Input.mousePosition + offset;
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && MyInstance.MyMoveable != null) {
                if (MyInstance.MyMoveable is Item) {
                    SystemWindowManager.MyInstance.confirmDestroyMenuWindow.OpenWindow();
                } else if (MyInstance.MyMoveable is BaseAbility) {
                    // DROP ABILITY SAFELY
                    if (UIManager.MyInstance.ActionBarManager.FromButton != null) {
                        UIManager.MyInstance.ActionBarManager.FromButton.ClearUseable();
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
            icon.sprite = moveable.Icon;
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
            UIManager.MyInstance.ActionBarManager.FromButton = null;
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
                } else {
                    // first we want to get this items equipment slot
                    // next we want to query the equipmentmanager on the charcter to see if he has an item in this items slot, and if it is the item we are dropping
                    // if it is, then we will unequip it, and then destroy it
                    if (item is Equipment) {
                        PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.Unequip(item as Equipment);
                        if (item.MySlot != null) {
                            item.MySlot.Clear();
                        }
                    }
                }
            }
            CombatLogUI.MyInstance.WriteSystemMessage("Destroyed " + MyMoveable.DisplayName);
            Drop();
            // done in drop... ?
            //InventoryManager.MyInstance.FromSlot = null;
        }
    }

}