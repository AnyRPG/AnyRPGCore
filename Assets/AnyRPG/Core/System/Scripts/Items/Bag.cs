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

        public BagNode BagNode { get; set; }

        /// <summary>
        /// Property for getting the slots
        /// </summary>
        public int Slots {
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
            AddToInventoryManager();
            return true;
        }

        public void AddToInventoryManager() {
            /*
            bool addToBank = false;
            if (BagNode != null) {
                addToBank = BagNode.IsBankNode;
            }
            */
            //if (playerManager.MyCharacter.CharacterInventoryManager.CanAddBag(addToBank)) {
            if (playerManager.MyCharacter.CharacterInventoryManager.CanAddBag()) {
                //Debug.Log("Bag.Use(): we can add the bag");

                if (BagNode == null) {
                    playerManager.MyCharacter.CharacterInventoryManager.AddBag(this);
                } else {
                    //Debug.Log("Bag.Use(): i have a bagnode");
                    playerManager.MyCharacter.CharacterInventoryManager.AddBag(this, BagNode);
                }
                Remove();
            } else {
                //Debug.Log("Bag.Use(): we can not add the bag!!!");
            }

        }

        public override string GetSummary(ItemQuality usedItemQuality) {
            return base.GetSummary(usedItemQuality) + string.Format("\n<color=green>Use: Equip</color>");
        }

    }

}