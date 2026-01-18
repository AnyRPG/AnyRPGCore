using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class AuctionItemButton : HighlightButton, IPointerClickHandler {

        [Header("Auction Item Button")]

        [SerializeField]
        protected Image attachmentBackgroundImage = null;

        [SerializeField]
        protected Image attachmentImage = null;

        [SerializeField]
        protected DescribableIcon describableIcon = null;

        [SerializeField]
        protected TextMeshProUGUI amountText = null;

        [SerializeField]
        protected TextMeshProUGUI itemNameText = null;

        [SerializeField]
        protected TextMeshProUGUI sellerText = null;

        [SerializeField]
        protected CurrencyBarController priceCurrencyBar = null;

        [SerializeField]
        protected HighlightButton cancelAuctionButton = null;

        [SerializeField]
        protected HighlightButton buyAuctionButton = null;

        private AuctionItemSearchResult auctionItem = null;
        private InstantiatedItem instantiatedItem = null;

        // game manager references
        private AuctionManagerClient auctionManagerClient = null;
        private SystemItemManager systemItemManager = null;
        private PlayerManager playerManager = null;

        public AuctionItemSearchResult AuctionItem { get => auctionItem; set => auctionItem = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            buyAuctionButton.Configure(systemGameManager);
            cancelAuctionButton.Configure(systemGameManager);
            priceCurrencyBar.Configure(systemGameManager);
            describableIcon.Configure(systemGameManager);
            describableIcon.SetSellPriceString("Vendor Price: ");
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            auctionManagerClient = systemGameManager.AuctionManagerClient;
            systemItemManager = systemGameManager.SystemItemManager;
            playerManager = systemGameManager.PlayerManager;
        }

        public void AddAuctionItem(AuctionItemSearchResult auctionItem) {
            //Debug.Log($"{gameObject.name}.AuctionButton.AddMessage({mailMessage.MessageId})");

            this.auctionItem = auctionItem;
            instantiatedItem = GetFirstInstantiatedItem();
            describableIcon.SetDescribable(instantiatedItem);
            if (instantiatedItem != null) {
                //attachmentImage.color = Color.white;
                //attachmentImage.sprite = instantiatedItem.Icon;
                attachmentBackgroundImage.color = Color.white;
                uIManager.SetItemBackground(instantiatedItem.Item, attachmentBackgroundImage, new Color32(0, 0, 0, 255), instantiatedItem.ItemQuality);
            } else {
                //attachmentImage.sprite = null;
                //attachmentImage.color = new Color32(0, 0, 0, 0);
                attachmentBackgroundImage.sprite = null;
                attachmentBackgroundImage.color = new Color32(0, 0, 0, 0);
            }
            string displayNameColorString = "white";
            if (instantiatedItem.ItemQuality != null) {
                displayNameColorString = "#" + ColorUtility.ToHtmlStringRGB(instantiatedItem.ItemQuality.QualityColor);
            }
            amountText.text = auctionItem.Items.Count.ToString();
            itemNameText.text = $"<color={displayNameColorString}>{instantiatedItem.DisplayName}</color>";
            sellerText.text = auctionItem.SellerName;

            if (auctionItem.SellerPlayerCharacterId == playerManager.UnitController.CharacterId) {
                buyAuctionButton.gameObject.SetActive(false);
                cancelAuctionButton.gameObject.SetActive(true);
            } else {
                buyAuctionButton.gameObject.SetActive(true);
                cancelAuctionButton.gameObject.SetActive(false);
                //Debug.Log($"AuctionItemButton.AddAuctionItem(): Checking currency for auction item. Player has {playerManager.UnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency)}, auction item costs {auctionItem.CurrencyAmount}");
                if (playerManager.UnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < auctionItem.CurrencyAmount) {
                    buyAuctionButton.Button.interactable = false;
                } else {
                    buyAuctionButton.Button.interactable = true;
                }
            }
            priceCurrencyBar.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, auctionItem.CurrencyAmount);
        }

        public InstantiatedItem GetFirstInstantiatedItem() {
            foreach (InstantiatedItem instantiatedItem in auctionItem.Items) {
                return instantiatedItem;
            }
            return null;
        }

        public void ClearButton() {
            attachmentImage.sprite = null;
            attachmentImage.color = new Color32(0, 0, 0, 0);
            itemNameText.text = string.Empty;
            amountText.text = string.Empty;
            sellerText.text = string.Empty;
        }

        public void CancelAuction() {
            auctionManagerClient.CancelAuctionItem = auctionItem;
            uIManager.confirmCancelAuctionWindow.OpenWindow();
        }

        public void BuyAuction() {
            auctionManagerClient.BuyAuctionItem = auctionItem;
            uIManager.confirmBuyAuctionWindow.OpenWindow();
        }

        /*
        public override void OnPointerEnter(PointerEventData eventData) {
            //Debug.Log("LootButton.OnPointerEnter(): " + GetInstanceID());
            base.OnPointerEnter(eventData);
            if (auctionItem.ItemIds.Count == 0) {
                return;
            }

            // only show gamepad tooltip if mouse is within image bounds
            Vector2 localMousePosition = attachmentImage.rectTransform.InverseTransformPoint(eventData.position);
            if (!attachmentImage.rectTransform.rect.Contains(localMousePosition)) {
                return;
            }

            ShowGamepadTooltip();
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            uIManager.HideToolTip();
        }

        public void ShowGamepadTooltip() {
            uIManager.ShowGamepadTooltip(attachmentImage.rectTransform, transform, instantiatedItem, "Vendor Price:");
        }
        */


        /*
        protected override void HandleLeftClick() {
            base.HandleLeftClick();
            auctionManagerClient.SetCurrentMailMessageId(auctionItem.MessageId);
            uIManager.mailViewWindow.OpenWindow();
        }
        */
    }

}