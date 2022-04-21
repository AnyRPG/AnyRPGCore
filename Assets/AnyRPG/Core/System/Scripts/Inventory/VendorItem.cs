using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class VendorItem : IDescribable {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        private string itemName = string.Empty;

        //[SerializeField]
        private Item item = null;

        [SerializeField]
        private int quantity = 1;

        [SerializeField]
        private bool unlimited = true;

        [Tooltip("The name of the item quality to use.  Leave blank to use the items default item quality")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ItemQuality))]
        private string itemQualityName = string.Empty;

        private ItemQuality itemQuality = null;

        public Item Item {
            get {
                return item;
            }
            set {
                item = value;
            }
        }

        public int Quantity {
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

        Sprite IDescribable.Icon => item.Icon;

        string IDescribable.DisplayName => item.DisplayName;

        string IDescribable.Description => item.Description;

        public ItemQuality GetItemQuality() {
            if (itemQuality != null) {
                return itemQuality;
            }
            return item.ItemQuality;
        }

        public int BuyPrice() {
            if (itemQuality != null) {
                return item.BuyPrice(itemQuality);
            }
            return item.BuyPrice();
        }

        public void SetupScriptableObjects(SystemDataFactory systemDataFactory) {

            item = null;
            if (itemName != null) {
                Item tmpItem = systemDataFactory.GetResource<Item>(itemName);
                if (tmpItem != null) {
                    item = tmpItem;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing a vendor item.  CHECK INSPECTOR");
                }
            }

            itemQuality = null;
            if (itemQualityName != null && itemQualityName != string.Empty) {
                ItemQuality tmpItemQuality = systemDataFactory.GetResource<ItemQuality>(itemQualityName);
                if (tmpItemQuality != null) {
                    itemQuality = tmpItemQuality;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing a vendor item.  CHECK INSPECTOR");
                }
            }
        }

        string IDescribable.GetSummary() {
            return item.GetSummary(GetItemQuality());
        }

        string IDescribable.GetDescription() {
            return item.GetDescription(GetItemQuality());
        }
    }

}