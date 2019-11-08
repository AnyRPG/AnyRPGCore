using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class Chest : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private CloseableWindow chestWindow;

        private List<Item> items = new List<Item>();

        [SerializeField]
        private BagPanel bag;

        public override bool Interact(CharacterUnit source) {
            if (!chestWindow.IsOpen) {
                AddItems();
                chestWindow.OnCloseWindowCallback += OnCloseWindow;
                chestWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            base.StopInteract();
            chestWindow.CloseWindow();
        }

        public void AddItems() {
            if (items != null) {
                foreach (Item item in items) {
                    item.MySlot.AddItem(item);
                }
            }
        }

        public void StoreItems() {
            items = bag.GetItems();
        }

        public void OnCloseWindow() {
            //Debug.Log("Closing Chest");
            StoreItems();
            bag.Clear();
            chestWindow.OnCloseWindowCallback -= OnCloseWindow;
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