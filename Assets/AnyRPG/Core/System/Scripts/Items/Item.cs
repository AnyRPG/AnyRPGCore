using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    /// <summary>
    /// Superclass for all items
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "AnyRPG/Inventory/Item")]
    public class Item : DescribableResource, IMoveable, IUseable {

        [Header("Item")]

        [Tooltip("Size of the stack, less than 2 is not stackable")]
        [SerializeField]
        private int stackSize = 1;

        [Tooltip("The name of the item quality to use")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ItemQuality))]
        protected string itemQuality = string.Empty;

        [Tooltip("If true, a random item quality will be selected")]
        [SerializeField]
        protected bool randomItemQuality = false;

        [Header("Item Level")]

        [Tooltip("If true, this item level will scale to match the character level")]
        [SerializeField]
        protected bool dynamicLevel = false;

        [Tooltip("If true, and dynamic level is true, the item level will be frozen at the level it dropped at")]
        [SerializeField]
        protected bool freezeDropLevel = false;

        protected int dropLevel = 1;

        [Tooltip("If dynamic level is true and this value is greater than zero, the item scaling will be capped at this level")]
        [SerializeField]
        protected int levelCap = 0;

        [Tooltip("If dynamic level is not true, this value will be used for the static level")]
        [SerializeField]
        private int itemLevel = 1;

        [Tooltip("The level the character must be to use this item")]
        [SerializeField]
        private int useLevel = 1;

        [Header("Price")]

        [Tooltip("The currency used when buying or selling this item")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Currency))]
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
        [ResourceSelector(resourceType = typeof(CharacterClass))]
        private List<string> characterClassRequirementList = new List<string>();

        private List<CharacterClass> realCharacterClassRequirementList = new List<CharacterClass>();

        // a reference to the real item quality
        protected ItemQuality realItemQuality = null;

        // a reference to the actual currency
        private Currency currency = null;

        // A reference to the slot that this item is sitting on
        private InventorySlot slot = null;

        // game manager references
        protected CurrencyConverter currencyConverter = null;
        //protected InventoryManager inventoryManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected UIManager uIManager = null;
        protected PlayerManager playerManager = null;

        public int MaximumStackSize { get => stackSize; set => stackSize = value; }
        public InventorySlot Slot { get => slot; set => slot = value; }
        public virtual float CoolDown { get => 0f; }
        public virtual bool RequireOutOfCombat { get => false; }
        public virtual bool RequireStealth { get => false; }


        public int BuyPrice() {
            return BuyPrice(realItemQuality);
        }

        public int BuyPrice(ItemQuality usedItemQuality) {
            if (dynamicCurrencyAmount) {
                //Debug.Log(DisplayName + ".Item.BuyPrice(" + (usedItemQuality == null ? "null" : usedItemQuality.DisplayName) + "): return: " + (int)(((pricePerLevel * GetItemLevel(playerManager.MyCharacter.CharacterStats.Level)) + basePrice) * (usedItemQuality == null ? 1 : usedItemQuality.BuyPriceMultiplier)));
                return (int)(((pricePerLevel * GetItemLevel(playerManager.MyCharacter.CharacterStats.Level)) + basePrice) * (usedItemQuality == null ? 1 : usedItemQuality.BuyPriceMultiplier));
            }
            //Debug.Log(DisplayName + ".Item.BuyPrice(" + (usedItemQuality == null ? "null" : usedItemQuality.DisplayName) + "): return: " + (int)(basePrice * (usedItemQuality == null ? 1 : usedItemQuality.BuyPriceMultiplier)));
            return (int)(basePrice * (usedItemQuality == null ? 1 : usedItemQuality.BuyPriceMultiplier));
        }

        public bool UniqueItem { get => uniqueItem; }
        public Currency Currency { get => currency; set => currency = value; }
        public ItemQuality ItemQuality { get => realItemQuality; set => realItemQuality = value; }
        public int GetItemLevel(int characterLevel) {
            int returnLevel = (int)Mathf.Clamp(itemLevel, 1, Mathf.Infinity);
            
            // frozen drop level overrides all other calculations
            if (freezeDropLevel == true) {
                return (int)Mathf.Clamp(dropLevel, 1, Mathf.Infinity);
            }
            if (dynamicLevel == true) {
                returnLevel = (int)Mathf.Clamp(characterLevel, 1, (levelCap > 0 ? levelCap : Mathf.Infinity));
            }

            // item quality can override regular individual item scaling (example, heirlooms always scale)
            if (ItemQuality == null) {
                return returnLevel;
            } else {
                if (ItemQuality.DynamicItemLevel) {
                    return (int)Mathf.Clamp(characterLevel, 1, (levelCap > 0 ? levelCap : Mathf.Infinity));
                } else {
                    return returnLevel;
                }
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            //inventoryManager = systemGameManager.InventoryManager;
            currencyConverter = systemGameManager.CurrencyConverter;
            uIManager = systemGameManager.UIManager;
            messageFeedManager = uIManager.MessageFeedManager;
            playerManager = systemGameManager.PlayerManager;
        }

        public virtual void UpdateChargeCount(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".Item.UpdateChargeCount()");
            int chargeCount = playerManager.MyCharacter.CharacterInventoryManager.GetUseableCount(this);
            uIManager.UpdateStackSize(actionButton, chargeCount, true);
        }

        public void UpdateTargetRange(ActionBarManager actionBarManager, ActionButton actionButton) {
            // do nothing
        }

        public void AssignToActionButton(ActionButton actionButton) {
            //Debug.Log("the useable is an item");
            if (playerManager.MyCharacter.CharacterInventoryManager.FromSlot != null) {
                // white, really?  this doesn't actually happen...
                playerManager.MyCharacter.CharacterInventoryManager.FromSlot.Icon.color = Color.white;
                playerManager.MyCharacter.CharacterInventoryManager.FromSlot = null;
            } else {
                //Debug.Log("ActionButton.SetUseable(): This must have come from another actionbar, not the inventory");
            }
            uIManager.SetItemBackground(this, actionButton.BackgroundImage, new Color32(0, 0, 0, 255));
        }

        public void AssignToHandScript(Image backgroundImage) {
            //Debug.Log("the useable is an item");
            
            uIManager.SetItemBackground(this, backgroundImage, new Color32(0, 0, 0, 255));
        }

        public virtual void UpdateActionButtonVisual(ActionButton actionButton) {
            int count = playerManager.MyCharacter.CharacterInventoryManager.GetUseableCount(this);
            // we have to do this to ensure we have a reference to the top item on the stack, otherwise we will try to use an item that has been used already
            //if ((count == 0 && removeStaleActions) || count > 0) {
            /*
            if (count > 0) {
                Useable = inventoryManager.GetUseable(Useable as IUseable);
            }
            */
            uIManager.UpdateStackSize(actionButton, count, true);

            
            if (count == 0) {
                actionButton.EnableFullCoolDownIcon();
            } else {
            
                // check for ability cooldown here and only disable if no cooldown exists
                if (!HadSpecialIcon(actionButton)) {
                    actionButton.DisableCoolDownIcon();
                }
            }
        }

        public virtual bool HadSpecialIcon(ActionButton actionButton) {
            return false;
        }

        private int SellPrice() {
            return SellPrice(realItemQuality);
        }

        private int SellPrice(ItemQuality usedItemQuality) {
            //get {

                if (dynamicCurrencyAmount) {
                    if (realItemQuality == null) {
                        //Debug.Log("realItemQuality was null");
                    }
                    return (int)(((pricePerLevel * GetItemLevel(playerManager.MyCharacter.CharacterStats.Level)) + basePrice) * (usedItemQuality == null ? 1 : usedItemQuality.SellPriceMultiplier));
                }
                return (int)(basePrice * (usedItemQuality == null ? 1 : usedItemQuality.SellPriceMultiplier));
            //}
            //set => basePrice = value;
        }

        /// <summary>
        /// return the sell price in the base currency
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<Currency, int> GetSellPrice() {
            //get {
                //Debug.Log(DisplayName + ".Item.MySellPrice()");
                int sellAmount = SellPrice();
                Currency currency = Currency;
                if (currency != null) {
                    CurrencyGroup currencyGroup = currencyConverter.FindCurrencyGroup(currency);
                    if (currencyGroup != null) {
                        int convertedSellAmount = currencyConverter.GetBaseCurrencyAmount(currency, sellAmount);
                        currency = currencyGroup.MyBaseCurrency;
                        sellAmount = (int)Mathf.Ceil((float)convertedSellAmount * systemConfigurationManager.VendorPriceMultiplier);
                    } else {
                        sellAmount = (int)Mathf.Ceil((float)sellAmount * systemConfigurationManager.VendorPriceMultiplier);
                    }
                }
                return new KeyValuePair<Currency, int>(currency, sellAmount);
            //}
        }

        public List<CharacterClass> CharacterClassRequirementList { get => realCharacterClassRequirementList; set => realCharacterClassRequirementList = value; }
        public bool RandomItemQuality { get => randomItemQuality; set => randomItemQuality = value; }
        public bool FreezeDropLevel { get => freezeDropLevel; set => freezeDropLevel = value; }
        public int DropLevel {
            get => dropLevel;
            set {
                dropLevel = (int)Mathf.Clamp(Mathf.Min(value, (levelCap > 0 ? levelCap : value)), 1, Mathf.Infinity);
            }
        }

        public IUseable GetFactoryUseable() {
            return systemDataFactory.GetResource<Item>(DisplayName);
        }

        public bool ActionButtonUse() {
            List<Item> itemList = playerManager.MyCharacter.CharacterInventoryManager?.GetItems(DisplayName, 1);
            if (itemList == null || itemList.Count == 0) {
                return false;
            }
            Item newItem = itemList[0];
            if (newItem == null) {
                return false;
            }
            return newItem.Use();
        }

        public virtual bool IsUseableStale() {
            // items are never stale
            // they should stay on action buttons in case the player picks up more
            return false;
        }

        public virtual Coroutine ChooseMonitorCoroutine(ActionButton actionButton) {
            return null;
        }

        public virtual bool Use() {
            //Debug.Log("Base item class: using " + itemName);
            if (!CharacterClassRequirementIsMet(playerManager.MyCharacter)) {
                messageFeedManager.WriteMessage("You are not the right character class to use " + DisplayName);
                return false;
            }
            //if (GetItemLevel(playerManager.MyCharacter.CharacterStats.Level) > playerManager.MyCharacter.CharacterStats.Level) {
            if (useLevel > playerManager.MyCharacter.CharacterStats.Level) {
                messageFeedManager.WriteMessage("You are too low level use " + DisplayName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// return true if the character class requirement of this item is met
        /// </summary>
        /// <returns></returns>
        public bool CharacterClassRequirementIsMet(BaseCharacter baseCharacter) {
            //Debug.Log(DisplayName + ".Item.CharacterClassRequirementIsMet()");
            if (CharacterClassRequirementList != null && CharacterClassRequirementList.Count > 0) {
                if (!CharacterClassRequirementList.Contains(baseCharacter.CharacterClass)) {
                    //Debug.Log(DisplayName + ".Item.CharacterClassRequirementIsMet(): return false");
                    return false;
                }
            }
            //Debug.Log(DisplayName + ".Item.CharacterClassRequirementIsMet(): return true");
            return true;
        }

        public virtual bool RequirementsAreMet() {
            //Debug.Log(DisplayName + ".Item.RequirementsAreMet()");

            // NOTE : currently this is only called from places that apply to characters (quest and loot)
            // if in the future this function is called from somewhere an npc or preview character is used, it would be better to accept the
            // character as a parameter, rather than hard coding to the player
            if (!CharacterClassRequirementIsMet(playerManager.MyCharacter)) {
                //Debug.Log(DisplayName + ".Item.RequirementsAreMet(): return false");
                return false;
            }

            //Debug.Log(DisplayName + ".Item.RequirementsAreMet(): return true");
            return true;
        }

        /// <summary>
        /// removes the item from the inventory system
        /// </summary>
        public void Remove() {
            //Debug.Log("Item " + GetInstanceID().ToString() + " is about to ask the slot to remove itself");
            if (Slot != null) {
                //Debug.Log("The item's myslot is not null");
                Slot.RemoveItem(this);
                Slot = null;
            } else {
                //Debug.Log("The item's myslot is null!!!");
            }
        }

        public virtual string GetDescription(ItemQuality usedItemQuality) {

            return string.Format("<color={0}>{1}</color>\n{2}", QualityColor.GetQualityColorString(usedItemQuality), DisplayName, GetSummary(usedItemQuality));
        }

        public override string GetDescription() {
            //Debug.Log(DisplayName + ".Item.GetDescription()");
            return GetDescription(realItemQuality);
        }

        public virtual string GetSummary(ItemQuality usedItemQuality) {
            string summaryString = string.Empty;
            if (characterClassRequirementList.Count > 0) {
                string colorString = "red";
                if (realCharacterClassRequirementList.Contains(playerManager.MyCharacter.CharacterClass)) {
                    colorString = "white";
                }
                summaryString += string.Format("\n<color={0}>Required Classes: {1}</color>", colorString, string.Join(",", characterClassRequirementList));
            }
            if (Currency == null) {
                summaryString += "\nNo Sell Price";
            }


            return string.Format("{0}", summaryString);
        }

        public override string GetSummary() {
            //Debug.Log(MyDisplayName + ".Item.GetSummary()");
            return GetSummary(realItemQuality);
        }

        public virtual void InitializeNewItem(ItemQuality usedItemQuality = null) {
            //Debug.Log(MyDisplayName + ".Item.InitializeNewItem()");

            // for now items that have item quality set by non random means (vendor overrides) will not change their display name
            if (usedItemQuality != null) {
                realItemQuality = usedItemQuality;
                displayName = DisplayName;
                return;
            }

            // choose the random item quality
            if (randomItemQuality == true) {
                // get number of item qualities that are valid for random item quality creation
                List<ItemQuality> validItemQualities = new List<ItemQuality>();
                foreach (ItemQuality itemQuality in systemDataFactory.GetResourceList<ItemQuality>()) {
                    if (itemQuality.AllowRandomItems) {
                        validItemQualities.Add(itemQuality);
                    }
                }
                if (validItemQualities.Count > 0) {
                    //Debug.Log(DisplayName + ".Item.InitilizeNewItem(): validQualities: " + validItemQualities.Count);

                    int usedIndex = 0;

                    int sum_of_weight = 0;

                    int accumulatedWeight = 0;

                    for (int i = 0; i < validItemQualities.Count; i++) {
                        sum_of_weight += validItemQualities[i].RandomWeight;
                    }
                    //Debug.Log(DisplayName + ".Item.InitilizeNewItem(): sum_of_weight: " + sum_of_weight);
                    int rnd = UnityEngine.Random.Range(0, sum_of_weight);
                    //Debug.Log(DisplayName + ".Item.InitilizeNewItem(): sum_of_weight: " + sum_of_weight + "; rnd: " + rnd);
                    for (int i = 0; i < validItemQualities.Count; i++) {
                        //Debug.Log(DisplayName + ".Item.InitilizeNewItem(): weightCompare: " + validItemQualities[i].RandomWeight + "; rnd: " + rnd);
                        accumulatedWeight += validItemQualities[i].RandomWeight;
                        if (rnd < accumulatedWeight) {
                            usedIndex = i;
                            //Debug.Log(DisplayName + ".Item.InitilizeNewItem(): break");
                            break;
                        }
                        //rnd -= validItemQualities[i].RandomWeight;
                    }

                    realItemQuality = validItemQualities[usedIndex];

                    if (realItemQuality.RandomQualityPrefix != null && realItemQuality.RandomQualityPrefix != string.Empty) {
                        //Debug.Log(MyDisplayName + ".Item.InitializeNewItem() setting displayName: " + realItemQuality.RandomQualityPrefix + " " + MyDisplayName);
                        displayName = realItemQuality.RandomQualityPrefix + " " + DisplayName;
                        //Debug.Log(MyDisplayName + ".Item.InitializeNewItem() setting displayName: " + displayName);
                    }
                }
            }
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            currency = null;
            if (currencyName != null && currencyName != string.Empty) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(currencyName);
                if (tmpCurrency != null) {
                    currency = tmpCurrency;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find currency : " + currencyName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            realItemQuality = null;
            if (itemQuality != null && itemQuality != string.Empty) {
                ItemQuality tmpItemQuality = systemDataFactory.GetResource<ItemQuality>(itemQuality);
                if (tmpItemQuality != null) {
                    realItemQuality = tmpItemQuality;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find item quality : " + itemQuality + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            realCharacterClassRequirementList = new List<CharacterClass>();
            if (characterClassRequirementList != null) {
                foreach (string characterClassName in characterClassRequirementList) {
                    CharacterClass tmpCharacterClass = systemDataFactory.GetResource<CharacterClass>(characterClassName);
                    if (tmpCharacterClass != null) {
                        realCharacterClassRequirementList.Add(tmpCharacterClass);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + characterClassName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }



    }

}