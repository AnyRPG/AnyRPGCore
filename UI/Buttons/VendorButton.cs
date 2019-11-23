using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorButton : TransparencyButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text title;

        [SerializeField]
        private Text price;

        [SerializeField]
        private Text descriptionText;

        [SerializeField]
        private Outline qualityColorOutline;

        [SerializeField]
        private Text quantity;

        [SerializeField]
        private DescribableIcon currencyIcon;

        private VendorItem vendorItem;

        public void AddItem(VendorItem vendorItem) {
            this.vendorItem = vendorItem;

            if (vendorItem.MyQuantity > 0 || vendorItem.Unlimited) {
                icon.sprite = vendorItem.MyItem.MyIcon;
                //title.text = string.Format("<color={0}>{1}</color>", QualityColor.MyColors[vendorItem.MyItem.MyQuality], vendorItem.MyItem.MyName);
                title.text = string.Format("{0}", vendorItem.MyItem.MyName);

                Color newColor = Color.white;
                if (ColorUtility.TryParseHtmlString(QualityColor.MyColors[vendorItem.MyItem.MyQuality], out newColor)) {
                    qualityColorOutline.effectColor = newColor;
                }

                if (!vendorItem.Unlimited) {
                    quantity.text = vendorItem.MyQuantity.ToString();
                } else {
                    quantity.text = string.Empty;
                }
                descriptionText.text = vendorItem.MyItem.MyDescription;
                if (vendorItem.MyItem.MyPrice > 0 && vendorItem.MyItem.MyCurrency != null && vendorItem.MyItem.MyCurrency.MyName != null && vendorItem.MyItem.MyCurrency.MyName != string.Empty) {
                    price.text = "Price: " + vendorItem.MyItem.MyPrice.ToString() + " " + vendorItem.MyItem.MyCurrency.MyName;
                    if (currencyIcon != null) {
                        currencyIcon.SetDescribable(vendorItem.MyItem.MyCurrency as IDescribable);
                    }
                } else {
                    price.text = "Price: FREE";
                    if (currencyIcon != null) {
                        currencyIcon.SetDescribable(null);
                    }

                }
                gameObject.SetActive(true);
            }
        }


        public void OnPointerClick(PointerEventData eventData) {
            if (vendorItem.MyItem.MyPrice == 0 || vendorItem.MyItem.MyCurrency == null || (CurrencyConverter.GetConvertedValue(vendorItem.MyItem.MyCurrency, vendorItem.MyItem.MyPrice) < (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.GetBaseCurrencyValue(vendorItem.MyItem.MyCurrency))) {
                Item tmpItem = Instantiate(vendorItem.MyItem);
                //Debug.Log("Instantiated an item with id: " + tmpItem.GetInstanceID().ToString());
                if (InventoryManager.MyInstance.AddItem(tmpItem)) {
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
            if (vendorItem.MyItem.MyPrice == 0 || vendorItem.MyItem.MyCurrency == null) {
                priceString = "FREE";
            } else {
                PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.SpendCurrency(vendorItem.MyItem.MyCurrency, vendorItem.MyItem.MyPrice);
                priceString = vendorItem.MyItem.MyPrice + " " + vendorItem.MyItem.MyCurrency.MyName;
            }
            MessageFeedManager.MyInstance.WriteMessage("Purchased " + vendorItem.MyItem.MyName + " for " + priceString);

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