using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    /// <summary>
    /// Superclass for all items
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "AnyRPG/Inventory/Item")]
    public class Item : DescribableResource, IMoveable {

        [Header("Item")]

        [Tooltip("Size of the stack, less than 2 is not stackable")]
        [SerializeField]
        private int stackSize = 1;

        [Tooltip("The name of the item quality to use")]
        [SerializeField]
        protected string itemQuality = string.Empty;

        [Tooltip("If true, a random item quality will be selected")]
        [SerializeField]
        protected bool randomItemQuality = false;

        [Header("Item Level")]

        [Tooltip("If true, this item level will scale to match the character level")]
        [SerializeField]
        protected bool dynamicLevel = false;

        [Tooltip("If dynamic level is true and this value is greater than zero, the item scaling will be capped at this level")]
        [SerializeField]
        protected int levelCap = 0;

        [Tooltip("If dynamic level is not true, this value will be used for the static level")]
        [SerializeField]
        private int itemLevel = 1;

        [Header("Price")]

        [Tooltip("The currency used when buying or selling this item")]
        [SerializeField]
        private string currencyName = string.Empty;

        [Tooltip("If true, the purchase and sale price will scale with level")]
        [SerializeField]
        protected bool dynamicCurrencyAmount = true;

        [Tooltip("If dynamic currency amount is true, this is the amount per level this item will cost")]
        [SerializeField]
        protected int pricePerLevel = 10;

        [Tooltip("Base item price. If dynamic currency is true, this price is added to the dynamic price.")]
        [SerializeField]
        private int basePrice = 0;

        [Header("Restrictions")]

        [Tooltip("if an item is unique, it will not drop from a loot table if it already exists in the bags")]
        [SerializeField]
        private bool uniqueItem = false;

        [Tooltip("If not empty, the character must be one of these classes to use this item.")]
        [SerializeField]
        private List<string> characterClassRequirementList = new List<string>();

        private List<CharacterClass> realCharacterClassRequirementList = new List<CharacterClass>();

        // a reference to the real item quality
        protected ItemQuality realItemQuality = null;

        // a reference to the actual currency
        private Currency currency = null;

        // A reference to the slot that this item is sitting on
        private SlotScript slot;

        public int MyMaximumStackSize { get => stackSize; set => stackSize = value; }
        public SlotScript MySlot { get => slot; set => slot = value; }
        public int BuyPrice {
            get {

                if (dynamicCurrencyAmount) {
                    return (int)(((pricePerLevel * GetItemLevel(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level)) + basePrice) * (realItemQuality == null ? 1 : realItemQuality.BuyPriceMultiplier));
                }
                return (int)(basePrice * (realItemQuality == null ? 1 : realItemQuality.BuyPriceMultiplier));
            }
            set => basePrice = value;
        }

        public int SellPrice {
            get {

                if (dynamicCurrencyAmount) {
                    if (realItemQuality == null) {
                        //Debug.Log("realItemQuality was null");
                    }
                    return (int)(((pricePerLevel * GetItemLevel(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level)) + basePrice) * (realItemQuality == null ? 1 : realItemQuality.SellPriceMultiplier));
                }
                return (int)(basePrice * (realItemQuality == null ? 1 : realItemQuality.SellPriceMultiplier));
            }
            set => basePrice = value;
        }

        public bool MyUniqueItem { get => uniqueItem; }
        public Currency MyCurrency { get => currency; set => currency = value; }
        public ItemQuality MyItemQuality { get => realItemQuality; set => realItemQuality = value; }
        public int GetItemLevel(int characterLevel) {
            int returnLevel = itemLevel;
            if (dynamicLevel == true) {
                returnLevel = (int)Mathf.Clamp(characterLevel, 1, (levelCap > 0 ? levelCap : Mathf.Infinity));
            }

            // item quality can override regular individual item scaling (example, heirlooms always scale)
            if (MyItemQuality == null) {
                return returnLevel;
            } else {
                if (MyItemQuality.MyDynamicItemLevel) {
                    return (int)Mathf.Clamp(characterLevel, 1, (levelCap > 0 ? levelCap : Mathf.Infinity));
                } else {
                    return returnLevel;
                }
            }
        }

        public KeyValuePair<Currency, int> MySellPrice {
            get {
                //Debug.Log(MyName + ".Item.MySellPrice()");
                int sellAmount = SellPrice;
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
        public bool RandomItemQuality { get => randomItemQuality; set => randomItemQuality = value; }

        public virtual void Awake() {
        }

        public virtual bool Use() {
            //Debug.Log("Base item class: using " + itemName);
            if (!CharacterClassRequirementIsMet()) {
                MessageFeedManager.MyInstance.WriteMessage("You are not the right character class to use " + MyDisplayName);
                return false;
            }

            return true;
        }

        public bool CharacterClassRequirementIsMet() {
            if (MyCharacterClassRequirementList != null && MyCharacterClassRequirementList.Count > 0) {
                if (!MyCharacterClassRequirementList.Contains(PlayerManager.MyInstance.MyCharacter.CharacterClass)) {
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
            return string.Format("<color={0}>{1}</color>\n{2}", QualityColor.GetQualityColorString(this), MyDisplayName, GetSummary());
            //return string.Format("<color=yellow>{0}</color>\n{1}", MyName, GetSummary());
        }

        
        public override string GetSummary() {
            //Debug.Log(MyDisplayName + ".Item.GetSummary()");
            //Debug.Log("Quality is " + quality.ToString() + QualityColor.MyColors.ToString());
            string summaryString = string.Empty;
            if (characterClassRequirementList.Count > 0) {
                string colorString = "red";
                if (realCharacterClassRequirementList.Contains(PlayerManager.MyInstance.MyCharacter.CharacterClass)) {
                    colorString = "white";
                }
                summaryString += string.Format("\n<color={0}>Required Classes: {1}</color>", colorString, string.Join(",", characterClassRequirementList));
            }
            if (MyCurrency == null) {
                summaryString += "\nNo Sell Price";
            }


            return string.Format("{0}", summaryString);
        }

        public virtual void InitializeNewItem() {
            //Debug.Log(MyDisplayName + ".Item.InitializeNewItem()");

            // choose the random item quality
            if (randomItemQuality == true) {
                // get number of item qualities that are valid for random item quality creation
                List<ItemQuality> validItemQualities = new List<ItemQuality>();
                foreach (ItemQuality itemQuality in SystemItemQualityManager.MyInstance.GetResourceList()) {
                    if (itemQuality.AllowRandomItems) {
                        validItemQualities.Add(itemQuality);
                    }
                }
                if (validItemQualities.Count > 0) {
                    //Debug.Log(MyName + ".Item.InitilizeNewItem(): validQualities: " + validItemQualities.Count);

                    int usedIndex = 0;

                    int sum_of_weight = 0;

                    int accumulatedWeight = 0;

                    for (int i = 0; i < validItemQualities.Count; i++) {
                        sum_of_weight += validItemQualities[i].RandomWeight;
                    }
                    //Debug.Log(MyName + ".Item.InitilizeNewItem(): sum_of_weight: " + sum_of_weight);
                    int rnd = UnityEngine.Random.Range(0, sum_of_weight);
                    //Debug.Log(MyName + ".Item.InitilizeNewItem(): sum_of_weight: " + sum_of_weight + "; rnd: " + rnd);
                    for (int i = 0; i < validItemQualities.Count; i++) {
                        //Debug.Log(MyName + ".Item.InitilizeNewItem(): weightCompare: " + validItemQualities[i].RandomWeight + "; rnd: " + rnd);
                        accumulatedWeight += validItemQualities[i].RandomWeight;
                        if (rnd < accumulatedWeight) {
                            usedIndex = i;
                            //Debug.Log(MyName + ".Item.InitilizeNewItem(): break");
                            break;
                        }
                        //rnd -= validItemQualities[i].RandomWeight;
                    }

                    realItemQuality = validItemQualities[usedIndex];

                    if (realItemQuality.RandomQualityPrefix != null && realItemQuality.RandomQualityPrefix != string.Empty) {
                        //Debug.Log(MyDisplayName + ".Item.InitializeNewItem() setting displayName: " + realItemQuality.RandomQualityPrefix + " " + MyDisplayName);
                        displayName = realItemQuality.RandomQualityPrefix + " " + MyDisplayName;
                        //Debug.Log(MyDisplayName + ".Item.InitializeNewItem() setting displayName: " + displayName);
                    }
                }
            }
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            currency = null;
            if (currencyName != null && currencyName != string.Empty) {
                Currency tmpCurrency = SystemCurrencyManager.MyInstance.GetResource(currencyName);
                if (tmpCurrency != null) {
                    currency = tmpCurrency;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find currency : " + currencyName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            }

            realItemQuality = null;
            if (itemQuality != null && itemQuality != string.Empty) {
                ItemQuality tmpItemQuality = SystemItemQualityManager.MyInstance.GetResource(itemQuality);
                if (tmpItemQuality != null) {
                    realItemQuality = tmpItemQuality;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item quality : " + itemQuality + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                }
            }

            realCharacterClassRequirementList = new List<CharacterClass>();
            if (characterClassRequirementList != null) {
                foreach (string characterClassName in characterClassRequirementList) {
                    CharacterClass tmpCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(characterClassName);
                    if (tmpCharacterClass != null) {
                        realCharacterClassRequirementList.Add(tmpCharacterClass);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + characterClassName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }



    }

}