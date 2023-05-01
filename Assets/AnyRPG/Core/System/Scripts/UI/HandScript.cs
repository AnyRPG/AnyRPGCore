using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class HandScript : ConfiguredMonoBehaviour {

        public IMoveable Moveable { get; set; }

        [SerializeField]
        private Vector3 offset = Vector3.zero;

        [SerializeField]
        private RectTransform rectTransform = null;

        [SerializeField]
        private Image backgroundImage = null;

        [SerializeField]
        private Image icon = null;

        // game manager references
        private UIManager uIManager = null;
        private ActionBarManager actionBarManager = null;
        private InputManager inputManager = null;
        //InventoryManager inventoryManager = null;
        private PlayerManager playerManager = null;
        private LogManager logManager = null;
        private ControlsManager controlsManager = null;


        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            uIManager = systemGameManager.UIManager;
            actionBarManager = uIManager.ActionBarManager;
            inputManager = systemGameManager.InputManager;
            //inventoryManager = systemGameManager.InventoryManager;
            playerManager = systemGameManager.PlayerManager;
            logManager = systemGameManager.LogManager;
            controlsManager = systemGameManager.ControlsManager;
        }

        public void SetPosition(Vector3 position) {
            transform.position = position;
        }

        // Update is called once per frame
        public void ProcessInput() {
            if (controlsManager.GamePadModeActive == false) {
                transform.position = Input.mousePosition + offset;
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
            }
            if (inputManager.KeyBindWasPressed("CANCELALL")) {
                Drop();
            }
        }

        public void TakeMoveable(IMoveable moveable) {
            //Debug.Log("HandScript.TakeMoveable(" + moveable.ToString() + ")");
            this.Moveable = moveable;

            moveable.AssignToHandScript(backgroundImage);

            icon.sprite = moveable.Icon;
            icon.color = Color.white;


            if (controlsManager.GamePadModeActive == true) {
                rectTransform.pivot = new Vector2(0, 1);
            } else {
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

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
            playerManager.MyCharacter.CharacterInventoryManager.FromSlot = null;
            actionBarManager.FromButton = null;
        }

        private void ClearMoveable() {
            //Debug.Log("HandScript.ClearMoveable()");
            if (playerManager.MyCharacter.CharacterInventoryManager.FromSlot?.InventorySlot != null) {
                playerManager.MyCharacter.CharacterInventoryManager.FromSlot.PutItemBack();
            }
            Moveable = null;

            // clear background image
            backgroundImage.color = new Color32(0, 0, 0, 0);
            backgroundImage.sprite = null;

            // clear icon
            icon.sprite = null;
            icon.color = new Color(0, 0, 0, 0);

            /*
            if (controlsManager.GamePadModeActive == true) {
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
            */
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
                        playerManager.UnitController.UnitModelController.RebuildModelAppearance();
                    }
                }
            }
            logManager.WriteSystemMessage("Destroyed " + Moveable.DisplayName);
            Drop();
        }
    }

}