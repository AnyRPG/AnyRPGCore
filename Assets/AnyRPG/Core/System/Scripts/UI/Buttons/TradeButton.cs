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
    public class TradeButton : HighlightButton, IDescribable {

        [Header("Trade")]

        [SerializeField]
        protected Image tradeBackGroundImage;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI title = null;

        [SerializeField]
        protected TextMeshProUGUI stackSize = null;

        private List<InstantiatedItem> items = new List<InstantiatedItem>();

        private int buttonIndex = 0;
        private bool isInteractable = false;

        // game manager references
        protected PlayerManager playerManager = null;
        protected HandScript handScript = null;
        protected TradeServiceClient tradeServiceClient = null;

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
        }

        public void SetIsInteractable(bool isInteractable) {
            this.isInteractable = isInteractable;
        }

        public void SetButtonIndex(int buttonIndex) {
            this.buttonIndex = buttonIndex;
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
            if (isInteractable == false) {
                return;
            }
            base.HandleRightClick();
            if (items.Count == 0) {
                return;
            }
            RequestRemoveItem();
        }

        protected override void HandleLeftClick() {
            if (isInteractable == false) {
                return;
            }
            base.HandleLeftClick();
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
            if (items.Count > 0) {
                tradeServiceClient.RequestAddItemsToTradeSlot(buttonIndex, items.Select(item => item.InstanceId).ToList());
            }
        }

        private void UpdateVisual() {
            if (items.Count == 0) {
                icon.sprite = null;
                icon.color = new Color32(0, 0, 0, 0);
                tradeBackGroundImage.sprite = null;
                tradeBackGroundImage.color = new Color32(0, 0, 0, 0);
                if (isInteractable == true) {
                    title.text = string.Format("<color=#00FFFF>Empty Trade Slot</color>\nDrop item here");
                } else {
                    title.text = string.Format("<color=#666666>Empty Trade Slot</color>");
                }
                stackSize.text = string.Empty;
                return;
            }
            icon.color = Color.white;
            icon.sprite = items[0].Icon;
            //tradeBackGroundImage.color = Color.white;
            //tradeBackGroundImage.sprite = items[0].Icon;
            uIManager.SetItemBackground(items[0].Item, tradeBackGroundImage, new Color32(0, 0, 0, 255), items[0].ItemQuality);

            string colorString = "white";
            if (items[0].ItemQuality != null) {
                colorString = "#" + ColorUtility.ToHtmlStringRGB(items[0].ItemQuality.QualityColor);
            }
            string removeString = string.Empty;
            if (isInteractable == true) {
                removeString = "\n(right click to remove)";
            }
            string itemName = string.Format("<color={0}>{1}</color>{2}", colorString, items[0].DisplayName, removeString);
            title.text = itemName;
            stackSize.text = items.Count.ToString();
        }

        public string GetSummary() {
            if (items.Count > 0) {
                return items[0].GetSummary();
            }
            // cyan
            return string.Format("<color=#00FFFF>Empty Trade Slot</color>\nDrop Item here");
        }

        public virtual string GetDescription() {
            if (items.Count > 0) {
                return items[0].GetDescription();
            }
            return "Drop items here";
        }

        public override void Accept() {
            base.Accept();
            RequestRemoveItem();
        }

        public void RequestRemoveItem() {
            //Debug.Log($"TradeButton.RemoveItem() instance: {GetInstanceID()}");

            RemoveItem();
            tradeServiceClient.RequestAddItemsToTradeSlot(buttonIndex, new List<long>());
        }

        public void RemoveItem() {
            items.Clear();
            UpdateVisual();
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
    }

}