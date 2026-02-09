using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    public class AuctionListAttachmentButton : HighlightButton, IDescribable {

        public event Action OnAddAttachment = delegate { };
        public event Action OnRemoveAttachment = delegate { };

        [Header("Auction Attachment")]

        [SerializeField]
        protected Image buttonBackgroundImage;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI title = null;

        [SerializeField]
        protected TextMeshProUGUI stackSize = null;

        private List<InstantiatedItem> items = new List<InstantiatedItem>();

        private AuctionPanel auctionPanel = null;

        // game manager references
        protected PlayerManager playerManager = null;
        protected HandScript handScript = null;

        public TextMeshProUGUI Title { get => title; }
        public Image Image { get => icon; }

        public Sprite Icon { get => icon.sprite; set => icon.sprite = value; }

        public string ResourceName { get => DisplayName; }
        public string DisplayName {
            get {
                if (items.Count > 0) {
                    return items[0].DisplayName;
                } else {
                    return "Empty Attachment Slot";
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
            //Debug.Log($"MailComposeAttachmentButton.SetGameManagerReferences()");

            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            handScript = systemGameManager.UIManager.HandScript;
        }

        public void SetAuctionPanel(AuctionPanel auctionPanel) {
            this.auctionPanel = auctionPanel;
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            //Debug.Log("LootButton.OnPointerEnter(): " + GetInstanceID());
            base.OnPointerEnter(eventData);
            if (items.Count == 0) {
                return;
            }

            //ShowGamepadTooltip();
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            uIManager.HideToolTip();
        }

        protected override void HandleRightClick() {
            if (items.Count == 0) {
                return;
            }
            RemoveItem();
        }

        protected override void HandleLeftClick() {
            if (playerManager.UnitController.CharacterInventoryManager.FromSlot != null) {
                AddItemFromInventorySlot();
                return;
            }
        }

        public void AddItemFromInventorySlot() {
            //Debug.Log("SlotScript.DropItemFromInventorySlot()");

            //Debug.Log("Dropping an item from an inventory slot");
            items.Clear();
            items.AddRange(playerManager.UnitController.CharacterInventoryManager.FromSlot.InventorySlot.InstantiatedItems.Values);
            UpdateVisual();
            handScript.Drop();
            OnAddAttachment();
        }

        private void UpdateVisual() {
            if (items.Count == 0) {
                icon.sprite = null;
                icon.color = new Color32(0, 0, 0, 0);
                buttonBackgroundImage.sprite = null;
                buttonBackgroundImage.color = new Color32(0, 0, 0, 0);
                title.text = string.Format("<color=#00FFFF>Empty Auction Slot</color>\nDrop item here");
                stackSize.text = string.Empty;
                return;
            }
            icon.color = Color.white;
            icon.sprite = items[0].Icon;
            uIManager.SetItemBackground(items[0].Item, buttonBackgroundImage, new Color32(0, 0, 0, 255), items[0].ItemQuality);

            string colorString = "white";
            if (items[0].ItemQuality != null) {
                colorString = "#" + ColorUtility.ToHtmlStringRGB(items[0].ItemQuality.QualityColor);
            }
            string removeString = string.Empty;
            removeString = "\n(right click to remove)";
            string itemName = string.Format("<color={0}>{1}</color>{2}", colorString, items[0].DisplayName, removeString);
            title.text = itemName;
            stackSize.text = items.Count.ToString();
        }

        public string GetSummary() {
            if (items.Count > 0) {
                return items[0].GetSummary();
            }
            // cyan
            return string.Format("<color=#00FFFF>Empty Auction Slot</color>\nDrop Item here");
        }

        public virtual string GetDescription() {
            if (items.Count > 0) {
                return items[0].GetDescription();
            }
            return "Drop items here";
        }

        public override void Accept() {
            base.Accept();
            RemoveItem();
        }

        public void RemoveItem() {
            ClearItems();
            OnRemoveAttachment();
        }

        public void ClearItems() {
            items.Clear();
            UpdateVisual();
        }

        public void ShowGamepadTooltip() {
            uIManager.ShowGamepadTooltip(owner.transform as RectTransform, transform, this, "");
        }

        public override void Select() {
            base.Select();

            //ShowGamepadTooltip();
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

        public bool HasItemId(long itemInstanceId) {
            return items.Any(item => item.InstanceId == itemInstanceId);
        }
    }

}