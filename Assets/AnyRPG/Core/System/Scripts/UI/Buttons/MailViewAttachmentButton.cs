using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class MailViewAttachmentButton : HighlightButton, IDescribable {

        [Header("Mail View Attachment Button")]

        [SerializeField]
        protected Image tradeBackGroundImage;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI title = null;

        [SerializeField]
        protected TextMeshProUGUI stackSize = null;

        private List<InstantiatedItem> items = new List<InstantiatedItem>();

        private int attachmentSlotId = 0;
        private MailMessage mailMessage = null;

        // game manager references
        protected PlayerManager playerManager = null;
        protected HandScript handScript = null;
        protected TradeServiceClient tradeServiceClient = null;
        protected MailboxManagerClient mailboxManagerClient = null;

        public TextMeshProUGUI Title { get => title; }
        public Image Image { get => icon; }

        public Sprite Icon { get => icon.sprite; set => icon.sprite = value; }

        public string ResourceName { get => DisplayName; }
        public string DisplayName {
            get {
                if (items.Count > 0) {
                    return items[0].DisplayName;
                } else {
                    return "Empty Trade Slot";
                }
            }
        }

        public string Description {
            get {
                if (items.Count > 0) {
                    return items[0].Description;
                }
                return "Drop items here";
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            UpdateVisual();
        }

        public override void SetGameManagerReferences() {
            //Debug.Log($"TradeButton.SetGameManagerReferences()");

            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            handScript = systemGameManager.UIManager.HandScript;
            tradeServiceClient = systemGameManager.TradeServiceClient;
            mailboxManagerClient = systemGameManager.MailboxManagerClient;
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            //Debug.Log("LootButton.OnPointerEnter(): " + GetInstanceID());
            base.OnPointerEnter(eventData);
            if (items.Count == 0) {
                return;
            }

            ShowGamepadTooltip();
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            uIManager.HideToolTip();
        }

        protected override void HandleRightClick() {
            //Debug.Log($"MailViewAttachmentButton.HandleRightClick()");

            if (items.Count == 0) {
                return;
            }
            base.HandleRightClick();
            RequestTakeAttachment();
        }

        protected override void HandleLeftClick() {
            //Debug.Log($"MailViewAttachmentButton.HandleLeftClick()");

            if (items.Count == 0) {
                return;
            }
            base.HandleLeftClick();
            RequestTakeAttachment();
        }

        private void RequestTakeAttachment() {
            //Debug.Log($"MailViewAttachmentButton.RequestTakeAttachment()");

            mailboxManagerClient.RequestTakeAttachment(mailMessage.MessageId, attachmentSlotId);
            uIManager.HideToolTip();
        }

        public void AddItemsFromMailMessage(int attachmentSlotId, List<InstantiatedItem> instantiatedItems, MailMessage mailMessage) {
            //Debug.Log($"MailViewAttachmentButton.AddItemsFromMailMessage(slotId: {attachmentSlotId}, itemCount: {instantiatedItems.Count})");

            this.attachmentSlotId = attachmentSlotId;
            this.mailMessage = mailMessage;
            items.Clear();
            items.AddRange(instantiatedItems);
            UpdateVisual();
        }

        private void UpdateVisual() {
            //Debug.Log($"MailViewAttachmentButton.UpdateVisual(): item count: {items.Count}");

            if (items.Count == 0) {
                icon.sprite = null;
                icon.color = new Color32(0, 0, 0, 0);
                tradeBackGroundImage.sprite = null;
                tradeBackGroundImage.color = new Color32(0, 0, 0, 0);
                title.text = string.Empty;
                stackSize.text = string.Empty;
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            icon.color = Color.white;
            icon.sprite = items[0].Icon;
            //tradeBackGroundImage.color = Color.white;
            //tradeBackGroundImage.sprite = items[0].Icon;
            uIManager.SetItemBackground(items[0].Item, tradeBackGroundImage, new Color32(0, 0, 0, 255), items[0].ItemQuality);

            string colorString = "white";
            if (items[0].ItemQuality != null) {
                colorString = "#" + ColorUtility.ToHtmlStringRGB(items[0].ItemQuality.QualityColor);
            }
            string removeString = "\n(click to take)";
            string itemName = string.Format("<color={0}>{1}</color>{2}", colorString, items[0].DisplayName, removeString);
            title.text = itemName;
            stackSize.text = items.Count.ToString();
        }

        public string GetSummary() {
            if (items.Count > 0) {
                return items[0].GetSummary();
            }
            return string.Empty;
        }

        public virtual string GetDescription() {
            if (items.Count > 0) {
                return items[0].GetDescription();
            }
            return string.Empty;
        }

        public override void Accept() {
            base.Accept();
            RequestTakeAttachment();
        }

        public void ShowGamepadTooltip() {
            uIManager.ShowGamepadTooltip(owner.transform as RectTransform, transform, this, "");
        }

        public override void Select() {
            base.Select();

            ShowGamepadTooltip();
        }

        public override void DeSelect() {
            base.DeSelect();

            uIManager.HideToolTip();
        }

        public void AddItems(List<InstantiatedItem> itemList) {
            items.Clear();
            items.AddRange(itemList);
            UpdateVisual();
        }

        public List<long> GetItemInstanceIds() {
            return items.Select(item => item.InstanceId).ToList();
        }
    }

}