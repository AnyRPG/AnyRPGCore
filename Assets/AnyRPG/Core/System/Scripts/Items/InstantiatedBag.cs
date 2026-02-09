using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace AnyRPG {
    public class InstantiatedBag : InstantiatedItem {

        private Bag bag = null;

        private int slots;
        //private Sprite icon = null;

        public BagNode BagNode { get; set; }
        /*
        public override Sprite Icon {
            get {
                if (icon == null) {
                    return icon;
                }
                return base.Icon;
            }
        }
        */

        /// <summary>
        /// Property for getting the slots
        /// </summary>
        public int Slots { get => slots; }
        public Bag Bag { get => bag; }

        public InstantiatedBag(SystemGameManager systemGameManager, long instanceId, Bag bag, ItemQuality itemQuality) : base(systemGameManager, instanceId, bag, itemQuality) {
            this.bag = bag;
            this.slots = bag.Slots;
        }

        /*
        public virtual void LoadSaveData(EquippedBagSaveData equippedBagSaveData) {
            displayName = equippedBagSaveData.DisplayName;
            dropLevel = equippedBagSaveData.DropLevel;
        }
        */

        public override string GetDescription() {
            //Debug.Log($"{item.ResourceName}.InstantiatedCurrencyItem.GetDescription()");

            return base.GetDescription() + bag.GetBagDescription();
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

    }
}