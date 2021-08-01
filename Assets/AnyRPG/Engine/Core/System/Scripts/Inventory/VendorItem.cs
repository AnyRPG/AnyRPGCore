using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class VendorItem {

        [SerializeField]
        private string itemName = string.Empty;

        //[SerializeField]
        private Item item;

        [SerializeField]
        private int quantity = 1;

        [SerializeField]
        private bool unlimited = false;

        public Item MyItem {
            get {
                return item;
            }
            set {
                item = value;
            }
        }

        public int MyQuantity {
            get {
                return quantity;
            }

            set {
                quantity = value;
            }
        }

        public bool Unlimited {
            get {
                return unlimited;
            }
        }

        public void SetupScriptableObjects() {

            item = null;
            if (itemName != null) {
                Item tmpItem = SystemDataFactory.Instance.GetResource<Item>(itemName);
                if (tmpItem != null) {
                    item = tmpItem;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing a vendor item.  CHECK INSPECTOR");
                }
            }
        }

    }

}