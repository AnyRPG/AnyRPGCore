using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "Bag", menuName = "AnyRPG/Inventory/Items/Bag", order = 1)]
    public class Bag : Item {

        [Header("Bag")]

        [SerializeField]
        private int slots;

        /// <summary>
        /// Property for getting the slots
        /// </summary>
        public int Slots {
            get {
                return slots;
            }
        }

        public override InstantiatedItem GetNewInstantiatedItem(SystemGameManager systemGameManager, long itemId, Item item, ItemQuality usedItemQuality) {
            if ((item is Bag) == false) {
                return null;
            }
            return new InstantiatedBag(systemGameManager, itemId, item as Bag, usedItemQuality);
        }

        /*
        public void Initalize(int slots, string title, Sprite bagIcon) {
            //Debug.Log("Bag.Initialize(" + slots + ", " + title + ")");
            this.slots = slots;
            DisplayName = title;
            if (bagIcon != null) {
                //Debug.Log("Bag.Initialize(): loading icon from resources at: " + spriteLocation);
                Sprite newIcon = bagIcon;
                if (newIcon == null) {
                    //Debug.Log("Bag.Initialize(): unable to load bag icon from resources!");
                } else {
                    icon = newIcon;
                }
            }
        }
        */

        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            //return base.GetSummary(usedItemQuality) + string.Format("\n<color=green>Use: Equip</color>");
            return base.GetDescription(usedItemQuality, usedItemLevel) + GetBagDescription();
        }

        public string GetBagDescription() {
            return string.Format("\n\n{0} slots", slots);
        }



    }

}