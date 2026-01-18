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
        protected int collectionIndex = 0;
        protected int itemIndex = 0;

        protected bool buyBackButton;

        // game manager references
        protected SystemItemManager systemItemManager = null;
        protected PlayerManager playerManager = null;
        //protected InventoryManager inventoryManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected CurrencyConverter currencyConverter = null;
        protected UIManager uIManager = null;
        protected VendorManagerClient vendorManager = null;

        public bool BuyBackButton { get => buyBackButton; set => buyBackButton = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            currencyBarController.Configure(systemGameManager);
            currencyBarController.HideZeroAmounts = true;
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
            vendorManager = systemGameManager.VendorManagerClient;
        }

        public override void SetController(UINavigationController uINavigationController) {
            base.SetController(uINavigationController);
            currencyBarController.SetToolTipTransform(owner.transform as RectTransform);
        }

        public void AddItem(VendorItem vendorItem, int collectionIndex, int itemIndex, bool buyBackButton = false) {
            this.vendorItem = vendorItem;
            this.collectionIndex = collectionIndex;
            this.itemIndex = itemIndex;
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
                if (vendorItem.BuyPrice(playerManager.UnitController) > 0
                    && vendorItem.Item.Currency != null
                    && vendorItem.Item.Currency.ResourceName != null
                    && vendorItem.Item.Currency.ResourceName != string.Empty) {
                    price.gameObject.SetActive(false);
                    //price.text = "Price:";
                    if (currencyBarController != null) {
                        if (buyBackButton == false) {
                            currencyBarController.UpdateCurrencyAmount(vendorItem.Item.Currency, vendorItem.BuyPrice(playerManager.UnitController));
                        } else {
                            currencyBarController.UpdateCurrencyAmount(vendorItem.Item.GetSellPrice(vendorItem.InstantiatedItem, playerManager.UnitController).Key, vendorItem.Item.GetSellPrice(vendorItem.InstantiatedItem, playerManager.UnitController).Value, "Buy Back Price:");
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

        protected override void HandleLeftClick() {
            base.HandleLeftClick();
            ProcessMouseClick();
        }

        protected override void HandleRightClick() {
            base.HandleRightClick();
            ProcessMouseClick();
        }

        public override void Interact() {
            base.Interact();
            ProcessMouseClick();
        }

        public void ProcessMouseClick() {
            vendorManager.RequestBuyItemFromVendor(playerManager.UnitController, vendorItem, collectionIndex, itemIndex);
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