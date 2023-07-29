using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorButton : NavigableElement {

        [Header("Vendor Button")]

        [SerializeField]
        protected Image backGroundImage = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI title = null;

        [SerializeField]
        protected TextMeshProUGUI price = null;

        [SerializeField]
        protected TextMeshProUGUI descriptionText = null;

        //[SerializeField]
        //private Outline qualityColorOutline = null;

        [SerializeField]
        protected TextMeshProUGUI quantity = null;

        [SerializeField]
        protected CurrencyBarController currencyBarController = null;

        protected VendorItem vendorItem = null;

        protected bool buyBackButton;

        // game manager references
        protected SystemItemManager systemItemManager = null;
        protected PlayerManager playerManager = null;
        //protected InventoryManager inventoryManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected CurrencyConverter currencyConverter = null;
        protected UIManager uIManager = null;

        public bool BuyBackButton { get => buyBackButton; set => buyBackButton = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            currencyBarController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemItemManager = systemGameManager.SystemItemManager;
            playerManager = systemGameManager.PlayerManager;
            //inventoryManager = systemGameManager.InventoryManager;
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            currencyConverter = systemGameManager.CurrencyConverter;
            uIManager = systemGameManager.UIManager;
        }

        public override void SetController(UINavigationController uINavigationController) {
            base.SetController(uINavigationController);
            currencyBarController.SetToolTipTransform(owner.transform as RectTransform);
        }

        public void AddItem(VendorItem vendorItem, bool buyBackButton = false) {
            this.vendorItem = vendorItem;
            this.buyBackButton = buyBackButton;

            if (vendorItem.Quantity > 0 || vendorItem.Unlimited) {
                icon.sprite = vendorItem.Item.Icon;
                uIManager.SetItemBackground(vendorItem.Item, backGroundImage, new Color32(0, 0, 0, 255), vendorItem.GetItemQuality());
                //title.text = string.Format("<color={0}>{1}</color>", QualityColor.MyColors[vendorItem.MyItem.MyQuality], vendorItem.MyItem.DisplayName);
                title.text = string.Format("{0}", vendorItem.Item.DisplayName);

                if (vendorItem.GetItemQuality() != null) {
                    //qualityColorOutline.effectColor = vendorItem.MyItem.MyItemQuality.MyQualityColor;
                    title.outlineColor = vendorItem.GetItemQuality().QualityColor;
                }

                if (!vendorItem.Unlimited) {
                    quantity.text = vendorItem.Quantity.ToString();
                } else {
                    quantity.text = string.Empty;
                }
                descriptionText.text = vendorItem.Item.Description;
                if (vendorItem.BuyPrice() > 0
                    && vendorItem.Item.Currency != null
                    && vendorItem.Item.Currency.ResourceName != null
                    && vendorItem.Item.Currency.ResourceName != string.Empty) {
                    price.gameObject.SetActive(false);
                    //price.text = "Price:";
                    if (currencyBarController != null) {
                        if (buyBackButton == false) {
                            currencyBarController.UpdateCurrencyAmount(vendorItem.Item.Currency, vendorItem.BuyPrice());
                        } else {
                            currencyBarController.UpdateCurrencyAmount(vendorItem.Item.GetSellPrice().Key, vendorItem.Item.GetSellPrice().Value, "Buy Back Price: ");
                        }
                    }
                } else {
                    price.gameObject.SetActive(true);
                    price.text = "Price: FREE";
                    if (currencyBarController != null) {
                        currencyBarController.ClearCurrencyAmounts();
                    }

                }
                gameObject.SetActive(true);
                if (UIManager.MouseInRect(rectTransform)) {
                    ProcessMouseEnter();
                }
            }
        }

        protected bool CanAfford() {
            if (buyBackButton == false) {
                if (currencyConverter.GetBaseCurrencyAmount(vendorItem.Item.Currency, vendorItem.BuyPrice()) <= playerManager.UnitController.CharacterCurrencyManager.GetBaseCurrencyValue(vendorItem.Item.Currency)) {
                    return true;
                }
                return false;
            }

            if (vendorItem.Item.GetSellPrice().Value <= playerManager.UnitController.CharacterCurrencyManager.GetBaseCurrencyValue(vendorItem.Item.Currency)) {
                return true;
            }
            return false;

        }


        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);
            //Debug.Log("VendorButton.OnPointerClick()");
            ProcessMouseClick();
        }

        public override void Interact() {
            base.Interact();
            ProcessMouseClick();
        }

        public void ProcessMouseClick() {
            if (vendorItem.BuyPrice() == 0
                            || vendorItem.Item.Currency == null
                            || CanAfford()) {
                Item tmpItem = null;
                if (buyBackButton == true) {
                    // if this is a buyback, the item has already been instantiated so it is safe to reference it directly
                    tmpItem = vendorItem.Item;
                } else {
                    // if this is a new purchase, a new copy of the item must be instantiated since the button is referring to the original factory item template
                    tmpItem = systemItemManager.GetNewResource(vendorItem.Item.ResourceName, vendorItem.GetItemQuality());
                    //Debug.Log("Instantiated an item with id: " + tmpItem.GetInstanceID().ToString());
                }

                if (playerManager.UnitController.CharacterInventoryManager.AddItem(tmpItem, false)) {
                    if (buyBackButton == false) {
                        tmpItem.DropLevel = playerManager.UnitController.CharacterStats.Level;
                    }
                    SellItem();
                    if (tmpItem is CurrencyItem) {
                        (tmpItem as CurrencyItem).Use();
                    }
                }
            } else {
                messageFeedManager.WriteMessage("You cannot afford " + vendorItem.Item.DisplayName);
            }
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);
            ProcessMouseEnter();
        }

        private void ProcessMouseEnter() {
            //uIManager.ShowToolTip(transform.position, vendorItem);
            ShowGamepadTooltip();
        }

        private void ShowGamepadTooltip() {
            uIManager.ShowGamepadTooltip(owner.transform as RectTransform, transform, vendorItem, "Sell Price: ");
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            uIManager.HideToolTip();
        }

        public virtual void OnDisable() {
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CheckMouse();
        }

        public virtual void CheckMouse() {
            if (UIManager.MouseInRect(transform as RectTransform)) {
                uIManager.HideToolTip();
            }
        }

        private void SellItem() {
            //Debug.Log("VendorButton.SellItem()");
            string priceString = string.Empty;
            if (vendorItem.BuyPrice() == 0 || vendorItem.Item.Currency == null) {
                priceString = "FREE";
            } else {
                KeyValuePair<Currency, int> usedSellPrice = new KeyValuePair<Currency, int>();
                if (buyBackButton == false) {
                    usedSellPrice = new KeyValuePair<Currency, int>(vendorItem.Item.Currency, vendorItem.BuyPrice());
                    priceString = vendorItem.BuyPrice() + " " + vendorItem.Item.Currency.DisplayName;
                } else {
                    usedSellPrice = vendorItem.Item.GetSellPrice();
                    priceString = currencyConverter.GetCombinedPriceString(usedSellPrice);
                }
                playerManager.UnitController.CharacterCurrencyManager.SpendCurrency(usedSellPrice.Key, usedSellPrice.Value);
            }
            if (systemConfigurationManager.VendorAudioClip != null) {
                audioManager.PlayEffect(systemConfigurationManager.VendorAudioClip);
            }
            messageFeedManager.WriteMessage("Purchased " + vendorItem.Item.DisplayName + " for " + priceString);

            if (!vendorItem.Unlimited) {
                vendorItem.Quantity--;
                quantity.text = vendorItem.Quantity.ToString();
                if (vendorItem.Quantity == 0) {
                    gameObject.SetActive(false);
                    uIManager.HideToolTip();
                    // if there is no longer anything in this slot, re-create the pages so there is no empty slot in the middle of the page
                    (uIManager.vendorWindow.CloseableWindowContents as VendorUI).RefreshPage();
                }
            }
        }

        public override void Select() {
            //Debug.Log("VendorButton.Select()");
            base.Select();
            ShowGamepadTooltip();
            if (owner != null) {
                owner.SetControllerHints("Purchase", "", "", "", "", "");
            }
        }

        public override void DeSelect() {
            //Debug.Log("VendorButton.DeSelect()");
            base.DeSelect();
            uIManager.HideToolTip();
            if (owner != null) {
                owner.HideControllerHints();
            }
        }

    }

}