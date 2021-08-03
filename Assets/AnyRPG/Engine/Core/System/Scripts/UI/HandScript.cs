using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class HandScript : MonoBehaviour {

        public IMoveable Moveable { get; set; }

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
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && Moveable != null) {
                if (Moveable is Item) {
                    SystemGameManager.Instance.UIManager.SystemWindowManager.confirmDestroyMenuWindow.OpenWindow();
                } else if (Moveable is BaseAbility) {
                    // DROP ABILITY SAFELY
                    if (SystemGameManager.Instance.UIManager.ActionBarManager.FromButton != null) {
                        SystemGameManager.Instance.UIManager.ActionBarManager.FromButton.ClearUseable();
                    }
                    Drop();
                }
            }
            if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("CANCEL")) {
                Drop();
            }
        }

        public void TakeMoveable(IMoveable moveable) {
            //Debug.Log("HandScript.TakeMoveable(" + moveable.ToString() + ")");
            this.Moveable = moveable;
            icon.sprite = moveable.Icon;
            icon.color = Color.white;
        }

        public IMoveable Put() {
            //Debug.Log("HandScript.Put().  Putting " + MyMoveable.ToString());
            IMoveable tmp = Moveable;
            ClearMoveable();

            return tmp;
        }

        public void Drop() {
            //Debug.Log("HandScript.Drop()");
            ClearMoveable();
            SystemGameManager.Instance.InventoryManager.FromSlot = null;
            SystemGameManager.Instance.UIManager.ActionBarManager.FromButton = null;
        }

        private void ClearMoveable() {
            //Debug.Log("HandScript.ClearMoveable()");
            if (SystemGameManager.Instance.InventoryManager.FromSlot != null) {
                SystemGameManager.Instance.InventoryManager.FromSlot.PutItemBack();
            }
            Moveable = null;
            icon.sprite = null;
            icon.color = new Color(0, 0, 0, 0);
        }

        public void DeleteItem() {
            //Debug.Log("HandScript.DeleteItem()");
            if (Moveable is Item) {
                Item item = (Item)Moveable;
                if (item.MySlot != null) {
                    item.MySlot.Clear();
                } else {
                    // first we want to get this items equipment slot
                    // next we want to query the equipmentmanager on the charcter to see if he has an item in this items slot, and if it is the item we are dropping
                    // if it is, then we will unequip it, and then destroy it
                    if (item is Equipment) {
                        SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterEquipmentManager.Unequip(item as Equipment);
                        if (item.MySlot != null) {
                            item.MySlot.Clear();
                        }
                    }
                }
            }
            SystemGameManager.Instance.LogManager.WriteSystemMessage("Destroyed " + Moveable.DisplayName);
            Drop();
            // done in drop... ?
            //SystemGameManager.Instance.InventoryManager.FromSlot = null;
        }
    }

}