using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    /// <summary>
    /// Superclass for all items
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "AnyRPG/Inventory/Item")]
    public class Item : DescribableResource, IMoveable {

        public bool isDefaultItem = false;

        /// <summary>
        /// Size of the stack, less than 2 is not stackable
        /// </summary>
        [SerializeField]
        private int stackSize;

        [SerializeField]
        private Quality quality;

        [SerializeField]
        protected string itemQuality;

        [SerializeField]
        private bool dynamicLevel;

        [SerializeField]
        private int itemLevel = 1;

        // if an item is unique, it will not drop from a loot table if it already exists in the bags
        [SerializeField]
        private bool uniqueItem = false;

        /// <summary>
        /// A reference to the slot that this item is sitting on
        /// </summary>
        private SlotScript slot;

        //public CharacterButton MyCharacterButton { get; set; }

        [SerializeField]
        private Currency currency;

        [SerializeField]
        private int price;

        public int MyMaximumStackSize { get => stackSize; set => stackSize = value; }
        public SlotScript MySlot { get => slot; set => slot = value; }
        public Quality MyQuality { get => quality; }
        public int MyPrice { get => price; set => price = value; }
        public bool MyUniqueItem { get => uniqueItem; }
        public Currency MyCurrency { get => currency; set => currency = value; }
        public string MyItemQuality { get => itemQuality; set => itemQuality = value; }
        public int MyItemLevel {
            get {
                int returnLevel = itemLevel;
                if (dynamicLevel == true) {
                    if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterStats != null) {
                        returnLevel = PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel;
                    } else {
                        returnLevel = itemLevel;
                    }
                }
                if (GetItemQuality() == null) {
                    return returnLevel;
                } else {
                    if (GetItemQuality().MyDynamicItemLevel) {
                        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterStats != null) {
                            return PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel;
                        } else {
                            return returnLevel;
                        }
                    } else {
                        return returnLevel;
                    }
                }
            }
            set => itemLevel = value;
        }

        public virtual void Awake() {
        }

        public virtual void Start() {
            // do nothing for now
            return;
        }

        public ItemQuality GetItemQuality() {
            if (itemQuality != null && itemQuality != string.Empty) {
                return SystemItemQualityManager.MyInstance.GetResource(itemQuality);
            }
            return null;
        }

        public virtual void Use() {
            // use the item
            //Debug.Log("Base item class: using " + itemName);
        }

        /// <summary>
        /// removes the item from the inventory.  new inventory system.
        /// </summary>
        public void Remove() {
            //Debug.Log("Item " + GetInstanceID().ToString() + " is about to ask the slot to remove itself");
            if (MySlot != null) {
                //Debug.Log("The item's myslot is not null");
                MySlot.RemoveItem(this);
                MySlot = null;
            } else {
                //Debug.Log("The item's myslot is null!!!");
            }
        }

        public override string GetDescription() {
            return string.Format("<color={0}>{1}</color>\n{2}", QualityColor.GetQualityColorString(this), MyName, GetSummary());
            //return string.Format("<color=yellow>{0}</color>\n{1}", MyName, GetSummary());
        }

        /*
        public override string GetSummary() {
            //Debug.Log("Quality is " + quality.ToString() + QualityColor.MyColors.ToString());
            return string.Format("{1}", MyDescription);
        }
        */
    }

}