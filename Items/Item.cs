using AnyRPG;
using System.Collections.Generic;
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
        private int stackSize = 1;

        [SerializeField]
        protected string itemQuality = string.Empty;

        protected ItemQuality realItemQuality = null;

        [SerializeField]
        private bool dynamicLevel = false;

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
        private string currencyName = string.Empty;

        //[SerializeField]
        private Currency currency = null;

        [SerializeField]
        private int price = 0;

        [Tooltip("If not empty, the character must be one of these classes to use this item.")]
        [SerializeField]
        private List<string> characterClassRequirementList = new List<string>();

        private List<CharacterClass> realCharacterClassRequirementList = new List<CharacterClass>();

        public int MyMaximumStackSize { get => stackSize; set => stackSize = value; }
        public SlotScript MySlot { get => slot; set => slot = value; }
        public int MyPrice { get => price; set => price = value; }
        public bool MyUniqueItem { get => uniqueItem; }
        public Currency MyCurrency { get => currency; set => currency = value; }
        public ItemQuality MyItemQuality { get => realItemQuality; set => realItemQuality = value; }
        public int MyItemLevel {
            get {
                int returnLevel = itemLevel;
                if (dynamicLevel == true) {
                    if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterStats != null) {
                        returnLevel = PlayerManager.MyInstance.MyCharacter.CharacterStats.Level;
                    } else {
                        returnLevel = itemLevel;
                    }
                }
                if (MyItemQuality == null) {
                    return returnLevel;
                } else {
                    if (MyItemQuality.MyDynamicItemLevel) {
                        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterStats != null) {
                            return PlayerManager.MyInstance.MyCharacter.CharacterStats.Level;
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

        public KeyValuePair<Currency, int> MySellPrice {
            get {
                //Debug.Log(MyName + ".Item.MySellPrice()");
                int sellAmount = MyPrice;
                Currency currency = MyCurrency;
                if (currency != null) {
                    CurrencyGroup currencyGroup = CurrencyConverter.FindCurrencyGroup(currency);
                    if (currencyGroup != null) {
                        int convertedSellAmount = CurrencyConverter.GetConvertedValue(currency, sellAmount);
                        currency = currencyGroup.MyBaseCurrency;
                        sellAmount = (int)Mathf.Ceil((float)convertedSellAmount * SystemConfigurationManager.MyInstance.MyVendorPriceMultiplier);
                    } else {
                        sellAmount = (int)Mathf.Ceil((float)sellAmount * SystemConfigurationManager.MyInstance.MyVendorPriceMultiplier);
                    }
                }
                return new KeyValuePair<Currency, int>(currency, sellAmount);
            }
        }

        public List<CharacterClass> MyCharacterClassRequirementList { get => realCharacterClassRequirementList; set => realCharacterClassRequirementList = value; }

        public virtual void Awake() {
        }

        public virtual bool Use() {
            //Debug.Log("Base item class: using " + itemName);
            if (!CharacterClassRequirementIsMet()) {
                MessageFeedManager.MyInstance.WriteMessage("You are not the right character class to use " + MyName);
                return false;
            }

            return true;
        }

        public bool CharacterClassRequirementIsMet() {
            if (MyCharacterClassRequirementList != null && MyCharacterClassRequirementList.Count > 0) {
                if (!MyCharacterClassRequirementList.Contains(PlayerManager.MyInstance.MyCharacter.MyCharacterClass)) {
                    return false;
                }
            }
            return true;
        }

        public virtual bool RequirementsAreMet() {
            if (!CharacterClassRequirementIsMet()) {
                return false;
            }

            return true;
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
            //Debug.Log(MyName + ".Item.GetDescription()");
            return string.Format("<color={0}>{1}</color>\n{2}", QualityColor.GetQualityColorString(this), MyName, GetSummary());
            //return string.Format("<color=yellow>{0}</color>\n{1}", MyName, GetSummary());
        }

        
        public override string GetSummary() {
            //Debug.Log("Quality is " + quality.ToString() + QualityColor.MyColors.ToString());
            string summaryString = string.Empty;
            if (characterClassRequirementList.Count > 0) {
                string colorString = "red";
                if (realCharacterClassRequirementList.Contains(PlayerManager.MyInstance.MyCharacter.MyCharacterClass)) {
                    colorString = "white";
                }
                summaryString += string.Format("\n<color={0}>Required Classes: {1}</color>", colorString, string.Join(",", characterClassRequirementList));
            }
            if (MyCurrency == null) {
                summaryString += "\nNo Sell Price";
            }


            return string.Format("{0}", summaryString);
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            currency = null;
            if (currencyName != null && currencyName != string.Empty) {
                Currency tmpCurrency = SystemCurrencyManager.MyInstance.GetResource(currencyName);
                if (tmpCurrency != null) {
                    currency = tmpCurrency;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find currency : " + currencyName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            realItemQuality = null;
            if (itemQuality != null && itemQuality != string.Empty) {
                ItemQuality tmpItemQuality = SystemItemQualityManager.MyInstance.GetResource(itemQuality);
                if (tmpItemQuality != null) {
                    realItemQuality = tmpItemQuality;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item quality : " + itemQuality + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            realCharacterClassRequirementList = new List<CharacterClass>();
            if (characterClassRequirementList != null) {
                foreach (string characterClassName in characterClassRequirementList) {
                    CharacterClass tmpCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(characterClassName);
                    if (tmpCharacterClass != null) {
                        realCharacterClassRequirementList.Add(tmpCharacterClass);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + characterClassName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }



    }

}