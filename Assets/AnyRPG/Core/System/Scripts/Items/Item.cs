using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    /// <summary>
    /// Superclass for all items
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "AnyRPG/Inventory/Item")]
    public class Item : DescribableResource, IRewardable {

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

        [Tooltip("If dynamic level is true and this value is greater than zero, the item scaling will be capped at this level")]
        [SerializeField]
        protected int levelCap = 0;

        [Tooltip("If dynamic level is not true, this value will be used for the static level")]
        [SerializeField]
        private int itemLevel = 1;

        [Tooltip("The level the character must be to use this item")]
        [SerializeField]
        protected int useLevel = 1;

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

        // a reference to the item quality
        protected ItemQuality itemQualityRef = null;

        // a reference to the actual currency
        private Currency currency = null;

        // game manager references
        //protected InventoryManager inventoryManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected UIManager uIManager = null;
        protected SystemItemManager systemItemManager = null;
        protected PlayerManager playerManager = null;
        protected CurrencyConverter currencyConverter = null;

        public int MaximumStackSize { get => stackSize; set => stackSize = value; }
        public List<CharacterClass> CharacterClassRequirementList { get => realCharacterClassRequirementList; set => realCharacterClassRequirementList = value; }
        public bool RandomItemQuality { get => randomItemQuality; set => randomItemQuality = value; }
        public bool FreezeDropLevel { get => freezeDropLevel; set => freezeDropLevel = value; }
        public string ItemQualityName { get => itemQuality; set => itemQuality = value; }
        public bool DynamicLevel { get => dynamicLevel; set => dynamicLevel = value; }
        public int LevelCap { get => levelCap; set => levelCap = value; }
        public int ItemLevel { get => itemLevel; set => itemLevel = value; }
        public int UseLevel { get => useLevel; set => useLevel = value; }
        public bool DynamicCurrencyAmount { get => dynamicCurrencyAmount; set => dynamicCurrencyAmount = value; }
        public int PricePerLevel { get => pricePerLevel; set => pricePerLevel = value; }
        public bool UniqueItem { get => uniqueItem; }
        public Currency Currency { get => currency; set => currency = value; }
        public ItemQuality ItemQuality { get => itemQualityRef; set => itemQualityRef = value; }
        public int BasePrice { get => basePrice; set => basePrice = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            //inventoryManager = systemGameManager.InventoryManager;
            uIManager = systemGameManager.UIManager;
            messageFeedManager = uIManager.MessageFeedManager;
            systemItemManager = systemGameManager.SystemItemManager;
            playerManager = systemGameManager.PlayerManager;
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        public void GiveReward(UnitController sourceUnitController) {
            InstantiatedItem newItem = sourceUnitController.CharacterInventoryManager.GetNewInstantiatedItem(this, itemQualityRef);
            if (newItem != null) {
                //Debug.Log("RewardButton.CompleteQuest(): newItem is not null, adding to inventory");
                newItem.DropLevel = sourceUnitController.CharacterStats.Level;
                sourceUnitController.CharacterInventoryManager.AddItem(newItem, false);
            }
        }

        public bool HasReward(UnitController sourceUnitController) {
            // this is not actually checked anywhere, but may need to be changed in the future
            // if anything actually needs to query through IRewardable to see if the character has the item
            return false;
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

        public virtual bool RequirementsAreMet(UnitController sourceUnitController) {
            //Debug.Log(DisplayName + ".Item.RequirementsAreMet()");

            // NOTE : currently this is only called from places that apply to characters (quest and loot)
            // if in the future this function is called from somewhere an npc or preview character is used, it would be better to accept the
            // character as a parameter, rather than hard coding to the player
            if (!CharacterClassRequirementIsMet(sourceUnitController.BaseCharacter)) {
                //Debug.Log(DisplayName + ".Item.RequirementsAreMet(): return false");
                return false;
            }

            //Debug.Log(DisplayName + ".Item.RequirementsAreMet(): return true");
            return true;
        }

        public override string GetSummary() {
            //Debug.Log($"{ResourceName}.Item.GetSummary() itemQualityRef: {(itemQualityRef == null ? "null" : itemQualityRef.ResourceName)} player: {(playerManager?.UnitController == null ? "null" : "not null")}");
            return GetSummary(itemQualityRef, GetItemLevel(playerManager.UnitController.CharacterStats.Level));
        }

        public virtual string GetSummary(ItemQuality usedItemQuality, int usedItemLevel) {

            return string.Format("<color={0}>{1}</color>\n{2}", QualityColor.GetQualityColorString(usedItemQuality), DisplayName, GetDescription(usedItemQuality, usedItemLevel));
        }

        public override string GetDescription() {
            //Debug.Log(DisplayName + ".Item.GetDescription()");
            return GetDescription(itemQualityRef, GetItemLevel(playerManager.UnitController.CharacterStats.Level));
        }

        public virtual string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {

            return GetItemDescription(usedItemQuality, usedItemLevel);
        }

        public virtual string GetItemDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            string descriptionString = base.GetDescription();
            if (descriptionString != string.Empty) {
                descriptionString = string.Format("\n<color=yellow><size=14>{0}</size></color>", descriptionString);
            }
            if (characterClassRequirementList.Count > 0) {
                string colorString = "red";
                if (realCharacterClassRequirementList.Contains(playerManager.UnitController.BaseCharacter.CharacterClass)) {
                    colorString = "white";
                }
                descriptionString += string.Format("\n\n<color={0}>Required Classes: {1}</color>", colorString, string.Join(",", characterClassRequirementList));
            }
            if (Currency == null) {
                descriptionString += "\n\nNo Sell Price";
            }

            return string.Format("{0}", descriptionString);
        }

        public virtual InstantiatedItem GetNewInstantiatedItem(SystemGameManager systemGameManager, long itemId, Item item, ItemQuality usedItemQuality) {
            return new InstantiatedItem(systemGameManager, itemId, item, usedItemQuality);
        }

        /*
        public bool Use(UnitController sourceUnitController) {
            return ActionButtonUse(sourceUnitController);
        }
        */

        public virtual bool HadSpecialIcon(ActionButton actionButton) {
            return false;
        }

        public int BuyPrice(UnitController sourceUnitController) {
            return BuyPrice(sourceUnitController, itemQualityRef);
        }

        public int BuyPrice(UnitController sourceUnitController, ItemQuality usedItemQuality) {
            if (dynamicCurrencyAmount) {
                //Debug.Log(DisplayName + ".Item.BuyPrice(" + (usedItemQuality == null ? "null" : usedItemQuality.DisplayName) + "): return: " + (int)(((pricePerLevel * GetItemLevel(playerManager.UnitController.CharacterStats.Level)) + basePrice) * (usedItemQuality == null ? 1 : usedItemQuality.BuyPriceMultiplier)));
                return (int)(((pricePerLevel * GetItemLevel(sourceUnitController.CharacterStats.Level)) + basePrice) * (usedItemQuality == null ? 1 : usedItemQuality.BuyPriceMultiplier));
            }
            //Debug.Log(DisplayName + ".Item.BuyPrice(" + (usedItemQuality == null ? "null" : usedItemQuality.DisplayName) + "): return: " + (int)(basePrice * (usedItemQuality == null ? 1 : usedItemQuality.BuyPriceMultiplier)));
            return (int)(basePrice * (usedItemQuality == null ? 1 : usedItemQuality.BuyPriceMultiplier));
        }

        public int GetItemLevel(int characterLevel) {
            int returnLevel = (int)Mathf.Clamp(ItemLevel, 1, Mathf.Infinity);

            if (dynamicLevel == true) {
                returnLevel = (int)Mathf.Clamp(characterLevel, 1, (levelCap > 0 ? levelCap : Mathf.Infinity));
            }

            // item quality can override regular individual item scaling (example, heirlooms always scale)
            if (itemQualityRef == null) {
                return returnLevel;
            } else {
                if (itemQualityRef.DynamicItemLevel) {
                    return (int)Mathf.Clamp(characterLevel, 1, (levelCap > 0 ? levelCap : Mathf.Infinity));
                } else {
                    return returnLevel;
                }
            }
        }

        public virtual void InitializeNewItem(InstantiatedItem instantiatedItem, ItemQuality usedItemQuality) {
            //Debug.Log($"Item.InitializeNewItem({instantiatedItem.ResourceName}, {(usedItemQuality == null ? "null" : usedItemQuality.ResourceName)})");

            // for now items that have item quality set by non random means (vendor overrides) will not change their display name
            if (usedItemQuality != null) {
                instantiatedItem.ItemQuality = usedItemQuality;
                instantiatedItem.DisplayName = DisplayName;
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

                    instantiatedItem.ItemQuality = validItemQualities[usedIndex];
                    //Debug.Log($"{ResourceName}.Item.InitializeNewItem() setting itemQuality: {instantiatedItem.ItemQuality.ResourceName}");


                    if (instantiatedItem.ItemQuality.RandomQualityPrefix != null && instantiatedItem.ItemQuality.RandomQualityPrefix != string.Empty) {
                        //Debug.Log($"{ResourceName}.Item.InitializeNewItem() setting displayName: {instantiatedItem.ItemQuality.RandomQualityPrefix} {DisplayName}");
                        instantiatedItem.DisplayName = $"{instantiatedItem.ItemQuality.RandomQualityPrefix} {DisplayName}";
                        //Debug.Log($"Item.InitializeNewItem() setting displayName: {instantiatedItem.DisplayName}");
                    }
                }
            }
        }

        /// <summary>
        /// return the sell price in the base currency
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<Currency, int> GetSellPrice(InstantiatedItem instantiatedItem, UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.Item.GetSellPrice()");

            // make a copy of the currency to work with so we don't change the original value later
            Currency usedCurrency = currency;

            if (usedCurrency == null) {
                // there was no sell currency so this item cannot be sold
                return new KeyValuePair<Currency, int>(usedCurrency, 0);
            }

            int sellPrice = 0;

            if (dynamicCurrencyAmount) {
                sellPrice = (pricePerLevel * instantiatedItem.GetItemLevel(sourceUnitController.CharacterStats.Level)) + basePrice;
            } else {
                sellPrice = basePrice;
            }

            if (sellPrice == 0) {
                // the item had a currency, but no sell price was set so it cannot be sold
                return new KeyValuePair<Currency, int>(usedCurrency, 0);
            }

            // convert currency to base currency to prevent higher level currencies with a value of 1 from not being divided
            CurrencyGroup currencyGroup = currencyConverter.FindCurrencyGroup(usedCurrency);
            if (currencyGroup != null) {
                sellPrice = currencyConverter.GetBaseCurrencyAmount(usedCurrency, sellPrice);
                usedCurrency = currencyGroup.BaseCurrency;
            }

            sellPrice = (int)Mathf.Clamp(sellPrice * (instantiatedItem.ItemQuality == null ? 1f : instantiatedItem.ItemQuality.SellPriceMultiplier) * systemConfigurationManager.VendorPriceMultiplier, 1f, Mathf.Infinity);

            return new KeyValuePair<Currency, int>(usedCurrency, sellPrice);
        }


        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            currency = null;
            if (currencyName != null && currencyName != string.Empty) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(currencyName);
                if (tmpCurrency != null) {
                    currency = tmpCurrency;
                } else {
                    Debug.LogError($"Item.SetupScriptableObjects(): Could not find currency : {currencyName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

            itemQualityRef = null;
            if (itemQuality != null && itemQuality != string.Empty) {
                ItemQuality tmpItemQuality = systemDataFactory.GetResource<ItemQuality>(itemQuality);
                if (tmpItemQuality != null) {
                    itemQualityRef = tmpItemQuality;
                } else {
                    Debug.LogError($"Item.SetupScriptableObjects(): Could not find item quality : {itemQuality} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

            realCharacterClassRequirementList = new List<CharacterClass>();
            if (characterClassRequirementList != null) {
                foreach (string characterClassName in characterClassRequirementList) {
                    CharacterClass tmpCharacterClass = systemDataFactory.GetResource<CharacterClass>(characterClassName);
                    if (tmpCharacterClass != null) {
                        realCharacterClassRequirementList.Add(tmpCharacterClass);
                    } else {
                        Debug.LogError($"Item.SetupScriptableObjects(): Could not find character class : {characterClassName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                    }
                }
            }

        }

    }

 }