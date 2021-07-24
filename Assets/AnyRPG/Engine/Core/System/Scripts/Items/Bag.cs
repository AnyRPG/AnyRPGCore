using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "Bag", menuName = "AnyRPG/Inventory/Items/Bag", order = 1)]
    public class Bag : Item, IUseable {

        [SerializeField]
        private int slots;

        [SerializeField]
        private GameObject bagPrefab;

        /// <summary>
        /// A reference to the bagScript, that this bag belongs to
        /// </summary>
        public BagPanel MyBagPanel { get; set; }

        public BagNode MyBagNode { get; set; }

        /// <summary>
        /// Property for getting the slots
        /// </summary>
        public int MySlots {
            get {
                return slots;
            }
        }

        public void Initalize(int slots, string title, Sprite bagIcon) {
            //Debug.Log("Bag.Initialize(" + slots + ", " + title + ")");
            this.slots = slots;
            DisplayName = title;
            if (bagIcon != null) {
                //Debug.Log("Bag.Initialize(): loading icon from resources at: " + spriteLocation);
                Sprite newIcon = bagIcon;
                if (newIcon == null) {
                    Debug.Log("Bag.Initialize(): unable to load bag icon from resources!");
                } else {
                    icon = newIcon;
                }
            }
        }

        public override bool Use() {
            //Debug.Log("Bag.Use()");
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }
            bool addToBank = false;
            if (MyBagNode != null) {
                addToBank = MyBagNode.IsBankNode;
            }
            if (InventoryManager.Instance.CanAddBag(addToBank)) {
                //Debug.Log("Bag.Use(): we can add the bag");

                if (MyBagNode == null) {
                    InventoryManager.Instance.AddBag(this);
                } else {
                    //Debug.Log("Bag.Use(): i have a bagnode");
                    InventoryManager.Instance.AddBag(this, MyBagNode);
                }
                Remove();
            } else {
                //Debug.Log("Bag.Use(): we can not add the bag!!!");
            }
            return true;
        }

        public override string GetSummary() {
            return base.GetSummary() + string.Format("\n<color=green>Use: Equip</color>");
        }

    }

}