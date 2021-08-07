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

        private VendorItem vendorItem = null;

        private bool buyBackButton;

        // game manager references
        private SystemItemManager systemItemManager = null;
        private PlayerManager playerManager = null;
        private InventoryManager inventoryManager = null;
        private SystemConfigurationManager systemConfigurationManager = null;
        private AudioManager audioManager = null;
        private MessageFeedManager messageFeedManager = null;

        public bool MyBuyBackButton { get => buyBackButton; set => buyBackButton = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemItemManager = systemGameManager.SystemItemManager;
            playerManager = systemGameManager.PlayerManager;
            inventoryManager = systemGameManager.InventoryManager;
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            audioManager = systemGameManager.AudioManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;

            currencyBarController.Configure(systemGameManager);
        }

        public void AddItem(VendorItem vendorItem, bool buyBackButton = false) {
            this.vendorItem = vendorItem;
            this.buyBackButton = buyBackButton;

            if (vendorItem.MyQuantity > 0 || vendorItem.Unlimited) {
                icon.sprite = vendorItem.MyItem.Icon;
                uIManager.SetItemBackground(vendorItem.MyItem, backGroundImage, new Color32(0, 0, 0, 255));
                //title.text = string.Format("<color={0}>{1}</color>", QualityColor.MyColors[vendorItem.MyItem.MyQuality], vendorItem.MyItem.MyName);
                title.text = string.Format("{0}", vendorItem.MyItem.DisplayName);

                if (vendorItem.MyItem.ItemQuality != null) {
                    //qualityColorOutline.effectColor = vendorItem.MyItem.MyItemQuality.MyQualityColor;
                    title.outlineColor = vendorItem.MyItem.ItemQuality.MyQualityColor;
                }

                if (!vendorItem.Unlimited) {
                    quantity.text = vendorItem.MyQuantity.ToString();
                } else {
                    quantity.text = string.Empty;
                }
                descriptionText.text = vendorItem.MyItem.MyDescription;
                if (vendorItem.MyItem.BuyPrice > 0
                    && vendorItem.MyItem.MyCurrency != null
                    && vendorItem.MyItem.MyCurrency.DisplayName != null
                    && vendorItem.MyItem.MyCurrency.DisplayName != string.Empty) {
                    price.gameObject.SetActive(false);
                    //price.text = "Price:";
                    if (currencyBarController != null) {
                        if (buyBackButton == false) {
                            currencyBarController.UpdateCurrencyAmount(vendorItem.MyItem.MyCurrency, vendorItem.MyItem.BuyPrice);
                        } else {
                            currencyBarController.UpdateCurrencyAmount(vendorItem.MyItem.MySellPrice.Key, vendorItem.MyItem.MySellPrice.Value, "Buy Back Price: ");
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
            }
        }


        public void OnPointerClick(PointerEventData eventData) {
            if (vendorItem.MyItem.BuyPrice == 0
                || vendorItem.MyItem.MyCurrency == null
                || (CurrencyConverter.GetConvertedValue(vendorItem.MyItem.MyCurrency, vendorItem.MyItem.BuyPrice) <= playerManager.MyCharacter.CharacterCurrencyManager.GetBaseCurrencyValue(vendorItem.MyItem.MyCurrency))) {
                Item tmpItem = systemItemManager.GetNewResource(vendorItem.MyItem.DisplayName);
                //Debug.Log("Instantiated an item with id: " + tmpItem.GetInstanceID().ToString());
                if (inventoryManager.AddItem(tmpItem)) {
                    tmpItem.DropLevel = playerManager.MyCharacter.CharacterStats.Level;
                    SellItem();
                    if (tmpItem is CurrencyItem) {
                        (tmpItem as CurrencyItem).Use();
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            uIManager.ShowToolTip(transform.position, vendorItem.MyItem);
        }

        public void OnPointerExit(PointerEventData eventData) {
            uIManager.HideToolTip();
        }

        private void SellItem() {
            //Debug.Log("VendorButton.SellItem()");
            string priceString = string.Empty;
            if (vendorItem.MyItem.BuyPrice == 0 || vendorItem.MyItem.MyCurrency == null) {
                priceString = "FREE";
            } else {
                KeyValuePair<Currency, int> usedSellPrice = new KeyValuePair<Currency, int>();
                if (buyBackButton == false) {
                    usedSellPrice = new KeyValuePair<Currency, int>(vendorItem.MyItem.MyCurrency, vendorItem.MyItem.BuyPrice);
                    priceString = vendorItem.MyItem.BuyPrice + " " + vendorItem.MyItem.MyCurrency.DisplayName;
                } else {
                    usedSellPrice = new KeyValuePair<Currency, int>(vendorItem.MyItem.MySellPrice.Key, vendorItem.MyItem.MySellPrice.Value);
                    priceString = CurrencyConverter.GetCombinedPriceSring(vendorItem.MyItem.MySellPrice.Key, vendorItem.MyItem.MySellPrice.Value);
                }
                playerManager.MyCharacter.CharacterCurrencyManager.SpendCurrency(usedSellPrice.Key, usedSellPrice.Value);
            }
            if (systemConfigurationManager.VendorAudioProfile?.AudioClip != null) {
                audioManager.PlayEffect(systemConfigurationManager.VendorAudioProfile.AudioClip);
            }
            messageFeedManager.WriteMessage("Purchased " + vendorItem.MyItem.DisplayName + " for " + priceString);

            if (!vendorItem.Unlimited) {
                vendorItem.MyQuantity--;
                quantity.text = vendorItem.MyQuantity.ToString();
                if (vendorItem.MyQuantity == 0) {
                    gameObject.SetActive(false);
                }
            }
        }

    }

}