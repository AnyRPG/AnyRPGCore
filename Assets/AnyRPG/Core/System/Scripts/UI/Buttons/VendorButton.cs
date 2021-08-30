using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorButton : TransparencyButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI title = null;

        [SerializeField]
        private TextMeshProUGUI price = null;

        [SerializeField]
        private TextMeshProUGUI descriptionText = null;

        //[SerializeField]
        //private Outline qualityColorOutline = null;

        [SerializeField]
        private TextMeshProUGUI quantity = null;

        [SerializeField]
        private CurrencyBarController currencyBarController = null;

        [SerializeField]
        private RectTransform rectTransform = null;

        private VendorItem vendorItem = null;

        private bool buyBackButton;

        // game manager references
        private SystemItemManager systemItemManager = null;
        private PlayerManager playerManager = null;
        private InventoryManager inventoryManager = null;
        private AudioManager audioManager = null;
        private MessageFeedManager messageFeedManager = null;
        private CurrencyConverter currencyConverter = null;

        public bool MyBuyBackButton { get => buyBackButton; set => buyBackButton = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            currencyBarController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemItemManager = systemGameManager.SystemItemManager;
            playerManager = systemGameManager.PlayerManager;
            inventoryManager = systemGameManager.InventoryManager;
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            audioManager = systemGameManager.AudioManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        public void AddItem(VendorItem vendorItem, bool buyBackButton = false) {
            this.vendorItem = vendorItem;
            this.buyBackButton = buyBackButton;

            if (vendorItem.Quantity > 0 || vendorItem.Unlimited) {
                icon.sprite = vendorItem.Item.Icon;
                uIManager.SetItemBackground(vendorItem.Item, backGroundImage, new Color32(0, 0, 0, 255), vendorItem.GetItemQuality());
                //title.text = string.Format("<color={0}>{1}</color>", QualityColor.MyColors[vendorItem.MyItem.MyQuality], vendorItem.MyItem.MyName);
                title.text = string.Format("{0}", vendorItem.Item.DisplayName);

                if (vendorItem.GetItemQuality() != null) {
                    //qualityColorOutline.effectColor = vendorItem.MyItem.MyItemQuality.MyQualityColor;
                    title.outlineColor = vendorItem.GetItemQuality().MyQualityColor;
                }

                if (!vendorItem.Unlimited) {
                    quantity.text = vendorItem.Quantity.ToString();
                } else {
                    quantity.text = string.Empty;
                }
                descriptionText.text = vendorItem.Item.MyDescription;
                if (vendorItem.BuyPrice() > 0
                    && vendorItem.Item.Currency != null
                    && vendorItem.Item.Currency.DisplayName != null
                    && vendorItem.Item.Currency.DisplayName != string.Empty) {
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

        private bool CanAfford() {
            if (buyBackButton == false) {
                if (currencyConverter.GetBaseCurrencyAmount(vendorItem.Item.Currency, vendorItem.BuyPrice()) <= playerManager.MyCharacter.CharacterCurrencyManager.GetBaseCurrencyValue(vendorItem.Item.Currency)) {
                    return true;
                }
                return false;
            }

            if (vendorItem.Item.GetSellPrice().Value <= playerManager.MyCharacter.CharacterCurrencyManager.GetBaseCurrencyValue(vendorItem.Item.Currency)) {
                return true;
            }
            return false;

        }


        public void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("VendorButton.OnPointerClick()");
            if (vendorItem.BuyPrice() == 0
                || vendorItem.Item.Currency == null
                || CanAfford()) {
                Item tmpItem = null;
                if (buyBackButton == true) {
                    // if this is a buyback, the item has already been instantiated so it is safe to reference it directly
                    tmpItem = vendorItem.Item;
                } else {
                    // if this is a new purchase, a new copy of the item must be instantiated since the button is referring to the original factory item template
                    tmpItem = systemItemManager.GetNewResource(vendorItem.Item.DisplayName, vendorItem.GetItemQuality());
                    //Debug.Log("Instantiated an item with id: " + tmpItem.GetInstanceID().ToString());
                }

                if (inventoryManager.AddItem(tmpItem)) {
                    if (buyBackButton == false) {
                        tmpItem.DropLevel = playerManager.MyCharacter.CharacterStats.Level;
                    }
                    SellItem();
                    if (tmpItem is CurrencyItem) {
                        (tmpItem as CurrencyItem).Use();
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            ProcessMouseEnter();
        }

        private void ProcessMouseEnter() {
            uIManager.ShowToolTip(transform.position, vendorItem);
        }

        public void OnPointerExit(PointerEventData eventData) {
            uIManager.HideToolTip();
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
                playerManager.MyCharacter.CharacterCurrencyManager.SpendCurrency(usedSellPrice.Key, usedSellPrice.Value);
            }
            if (systemConfigurationManager.VendorAudioProfile?.AudioClip != null) {
                audioManager.PlayEffect(systemConfigurationManager.VendorAudioProfile.AudioClip);
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

    }

}