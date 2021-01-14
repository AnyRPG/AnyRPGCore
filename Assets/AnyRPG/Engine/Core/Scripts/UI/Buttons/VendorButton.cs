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

        public bool MyBuyBackButton { get => buyBackButton; set => buyBackButton = value; }

        public void AddItem(VendorItem vendorItem, bool buyBackButton = false) {
            this.vendorItem = vendorItem;
            this.buyBackButton = buyBackButton;

            if (vendorItem.MyQuantity > 0 || vendorItem.Unlimited) {
                icon.sprite = vendorItem.MyItem.Icon;
                UIManager.MyInstance.SetItemBackground(vendorItem.MyItem, backGroundImage, new Color32(0, 0, 0, 255));
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
                if (vendorItem.MyItem.BuyPrice > 0 && vendorItem.MyItem.MyCurrency != null && vendorItem.MyItem.MyCurrency.DisplayName != null && vendorItem.MyItem.MyCurrency.DisplayName != string.Empty) {
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
            if (vendorItem.MyItem.BuyPrice == 0 || vendorItem.MyItem.MyCurrency == null || (CurrencyConverter.GetConvertedValue(vendorItem.MyItem.MyCurrency, vendorItem.MyItem.BuyPrice) <= PlayerManager.MyInstance.MyCharacter.CharacterCurrencyManager.GetBaseCurrencyValue(vendorItem.MyItem.MyCurrency))) {
                //Item tmpItem = Instantiate(vendorItem.MyItem);
                Item tmpItem = SystemItemManager.MyInstance.GetNewResource(vendorItem.MyItem.DisplayName);
                //Debug.Log("Instantiated an item with id: " + tmpItem.GetInstanceID().ToString());
                if (InventoryManager.MyInstance.AddItem(tmpItem)) {
                    tmpItem.DropLevel = PlayerManager.MyInstance.MyCharacter.CharacterStats.Level;
                    SellItem();
                    if (tmpItem is CurrencyItem) {
                        (tmpItem as CurrencyItem).Use();
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            UIManager.MyInstance.ShowToolTip(transform.position, vendorItem.MyItem);
        }

        public void OnPointerExit(PointerEventData eventData) {
            UIManager.MyInstance.HideToolTip();
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
                PlayerManager.MyInstance.MyCharacter.CharacterCurrencyManager.SpendCurrency(usedSellPrice.Key, usedSellPrice.Value);
            }
            if (SystemConfigurationManager.MyInstance.VendorAudioProfile != null && SystemConfigurationManager.MyInstance.VendorAudioProfile.AudioClip != null) {
                AudioManager.MyInstance.PlayEffect(SystemConfigurationManager.MyInstance.VendorAudioProfile.AudioClip);
            }
            MessageFeedManager.MyInstance.WriteMessage("Purchased " + vendorItem.MyItem.DisplayName + " for " + priceString);

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