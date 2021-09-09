using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class HandScript : ConfiguredMonoBehaviour {

        public IMoveable Moveable { get; set; }

        private Image icon;

        [SerializeField]
        private Vector3 offset = Vector3.zero;

        // game manager references
        UIManager uIManager = null;
        ActionBarManager actionBarManager = null;
        InputManager inputManager = null;
        InventoryManager inventoryManager = null;
        PlayerManager playerManager = null;
        LogManager logManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            uIManager = systemGameManager.UIManager;
            actionBarManager = uIManager.ActionBarManager;
            inputManager = systemGameManager.InputManager;
            inventoryManager = systemGameManager.InventoryManager;
            playerManager = systemGameManager.PlayerManager;
            logManager = systemGameManager.LogManager;

            icon = GetComponent<Image>();
        }

        // Update is called once per frame
        void Update() {
            icon.transform.position = Input.mousePosition + offset;
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && Moveable != null) {
                if (Moveable is Item) {
                    uIManager.confirmDestroyMenuWindow.OpenWindow();
                } else if (Moveable is BaseAbility) {
                    // DROP ABILITY SAFELY
                    if (actionBarManager.FromButton != null) {
                        actionBarManager.FromButton.ClearUseable();
                    }
                    Drop();
                }
            }
            if (inputManager.KeyBindWasPressed("CANCEL")) {
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
            inventoryManager.FromSlot = null;
            actionBarManager.FromButton = null;
        }

        private void ClearMoveable() {
            //Debug.Log("HandScript.ClearMoveable()");
            if (inventoryManager.FromSlot != null) {
                inventoryManager.FromSlot.PutItemBack();
            }
            Moveable = null;
            icon.sprite = null;
            icon.color = new Color(0, 0, 0, 0);
        }

        public void DeleteItem() {
            //Debug.Log("HandScript.DeleteItem()");
            if (Moveable is Item) {
                Item item = (Item)Moveable;
                if (item.Slot != null) {
                    item.Slot.Clear();
                } else {
                    // first we want to get this items equipment slot
                    // next we want to query the equipmentmanager on the charcter to see if he has an item in this items slot, and if it is the item we are dropping
                    // if it is, then we will unequip it, and then destroy it
                    if (item is Equipment) {
                        playerManager.MyCharacter.CharacterEquipmentManager.Unequip(item as Equipment);
                        if (item.Slot != null) {
                            item.Slot.Clear();
                        }
                    }
                }
            }
            logManager.WriteSystemMessage("Destroyed " + Moveable.DisplayName);
            Drop();
        }
    }

}