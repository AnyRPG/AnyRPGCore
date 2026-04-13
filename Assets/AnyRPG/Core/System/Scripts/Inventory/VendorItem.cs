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
        private InstantiatedItem instantiatedItem = null;

        [SerializeField]
        private int quantity = 1;

        [SerializeField]
        private bool unlimited = true;

        [Tooltip("The name of the item quality to use.  Leave blank to use the items default item quality")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ItemQuality))]
        private string itemQualityName = string.Empty;

        private ItemQuality itemQuality = null;

        public int itemIndex = 0;

        public Item Item {
            get {
                return item;
            }
            set {
                item = value;
            }
        }

        public InstantiatedItem InstantiatedItem {
            get {
                return instantiatedItem;
            }
            set {
                instantiatedItem = value;
                item = instantiatedItem.Item;
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
            set {
                unlimited = value;
            }
        }

        Sprite IDescribable.Icon => item.Icon;

        string IDescribable.ResourceName => item.ResourceName;
        string IDescribable.DisplayName => item.DisplayName;

        string IDescribable.Description => item.Description;

        public string ItemName { get => itemName; set => itemName = value; }

        public ItemQuality GetItemQuality() {
            if (itemQuality != null) {
                return itemQuality;
            }
            return item.ItemQuality;
        }

        public int BuyPrice(UnitController sourceUnitController) {
            if (itemQuality != null) {
                return item.BuyPrice(sourceUnitController, itemQuality);
            }
            return item.BuyPrice(sourceUnitController);
        }

        public void ProcessShowTooltip(TooltipController tooltipController) {
            if (instantiatedItem != null) {
                tooltipController.UpdateCurrencyAmount(instantiatedItem, "Sell Price: ");
            }
        }

        public void SetupScriptableObjects(SystemDataFactory systemDataFactory, IDescribable describable) {

            if (itemName != string.Empty) {
                Item tmpItem = systemDataFactory.GetResource<Item>(itemName);
                if (tmpItem != null) {
                    item = tmpItem;
                } else {
                    Debug.LogError($"VendorItem.SetupScriptableObjects(): Could not find item : {itemName} while inititalizing a vendor item for {describable.ResourceName}.  CHECK INSPECTOR");
                }
            }

            if (itemQualityName != string.Empty) {
                ItemQuality tmpItemQuality = systemDataFactory.GetResource<ItemQuality>(itemQualityName);
                if (tmpItemQuality != null) {
                    itemQuality = tmpItemQuality;
                } else {
                    Debug.LogError($"VendorItem.SetupScriptableObjects(): Could not find item quality : {itemQualityName} while inititalizing a vendor item for {describable.ResourceName}.  CHECK INSPECTOR");
                }
            }
        }

        string IDescribable.GetSummary() {
            return item.GetSummary(GetItemQuality(), item.ItemLevel);
        }

        string IDescribable.GetDescription() {
            return item.GetDescription(GetItemQuality(), item.ItemLevel);
        }

    }

}