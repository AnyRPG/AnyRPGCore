using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class SlotScript : DescribableIcon, IPointerClickHandler, IClickable, IContextMenuTarget, IDescribable, IMoveableOwner {

        [Header("Slot Script")]

        [SerializeField]
        protected Image backGroundImage = null;

        protected InventorySlot inventorySlot = null;

        // game manager references
        //protected InventoryManager inventoryManager = null;
        protected HandScript handScript = null;
        protected PlayerManagerClient playerManagerClient = null;
        protected ActionBarManager actionBarManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected ContextMenuService contextMenuService = null;

        /// <summary>
        /// A reference to the bag that this slot belongs to
        /// </summary>
        public BagPanel BagPanel { get; set; }

        public InventorySlot InventorySlot { get => inventorySlot; }

        public IMoveable Moveable => inventorySlot?.InstantiatedItem;

        public override int Count {
            get {
                if (inventorySlot != null) {
                    return inventorySlot.Count;
                }
                return 0;
            }
        }

        public override bool CaptureCancelButton {
            get {
                if (handScript.MoveableOwner != null) {
                    return true;
                }
                return base.CaptureCancelButton;
            }
        }

        Sprite IDescribable.Icon => inventorySlot.InstantiatedItem?.Icon;

        public string ResourceName => inventorySlot.InstantiatedItem?.ResourceName;

        public string DisplayName => inventorySlot.InstantiatedItem?.DisplayName;

        public string Description => inventorySlot.InstantiatedItem?.Description;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //inventoryManager = systemGameManager.InventoryManager;
            handScript = systemGameManager.UIManager.HandScript;
            playerManagerClient = systemGameManager.PlayerManagerClient;
            actionBarManager = systemGameManager.UIManager.ActionBarManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            contextMenuService = systemGameManager.ContextMenuService;
        }

        public void SetInventorySlot(InventorySlot inventorySlot) {
            //Debug.Log($"SlotScript.SetInventorySlot() instanceId: {GetInstanceID()}");

            this.inventorySlot = inventorySlot;
            inventorySlot.OnUpdateSlot += UpdateSlot;
        }

        
        public void ClearInventorySlot() {
            //Debug.Log($"SlotScript.ClearInventorySlot() instanceId: {GetInstanceID()}");

            if (inventorySlot != null) {
                inventorySlot.OnUpdateSlot -= UpdateSlot;
                ClearSlot();
            }
        }

        private void DropItemFromNonInventorySlot(InstantiatedItem instantiatedItem) {
            // item comes from somewhere else, like bag bar or character panel
            BagPanel.DropItemFromNonInventorySlot(this, instantiatedItem);
            handScript.CancelMove();
        }

        private void SwapItemFromNonInventorySlot(InstantiatedItem instantiatedItem) {
            // item comes from somewhere else, like bag bar or character panel
            BagPanel.SwapItemFromNonInventorySlot(this, instantiatedItem);
            handScript.CompleteMove();
        }

        public void SendItemToHandScript() {
            //Debug.Log("SlotScript.SendItemToHandScript()");

            handScript.TakeMoveable(this);
            playerManagerClient.UnitController.CharacterInventoryManager.FromSlot = this;
            if (controlsManager.GamepadModeActive == true) {
                handScript.SetPosition(transform.position);
            }
        }

        public void DropItemFromInventorySlot() {
            //Debug.Log("SlotScript.DropItemFromInventorySlot()");

            //Debug.Log("Dropping an item from an inventory slot");
            BagPanel.DropItemFromInventorySlot(this, playerManagerClient.UnitController.CharacterInventoryManager.FromSlot);
            handScript.CompleteMove();
        }

        protected override void HandleLeftClick() {
            // we have something to move and it came from the inventory, therefore we are trying to drop something from this slot or another slot onto this slot
            if (playerManagerClient.UnitController.CharacterInventoryManager.FromSlot != null) {
                DropItemFromInventorySlot();
                return;
            }


            if (!inventorySlot.IsEmpty) {
                // This slot has something in it, and the hand script is empty, so we are trying to pick it up
                if (handScript.MoveableOwner == null) {
                    SendItemToHandScript();
                    return;
                }

                // the slot has something in it, and the handscript is not empty, so we are trying to swap with something
                if (handScript.MoveableOwner.Moveable is InstantiatedItem) {
                    SwapItemFromNonInventorySlot(handScript.MoveableOwner.Moveable as InstantiatedItem);
                }
            } else {
                // this slot has nothing in it
                if (handScript.MoveableOwner != null && handScript.MoveableOwner.Moveable is InstantiatedItem) {
                    DropItemFromNonInventorySlot(handScript.MoveableOwner.Moveable as InstantiatedItem);
                }
            }
            
        }

        protected override void HandleRightClick() {
            //Debug.Log("SlotScript.HandleRightClick()");
            base.HandleRightClick();
            InteractWithSlot();
            ProcessMouseEnter();
        }

        public void InteractWithSlot() {
            //Debug.Log("SlotScript.InteractWithSlot()");

            // ignore right clicks when something is in the handscript
            if (handScript.MoveableOwner != null) {
                return;
            }

            // show context menu if something is in the slot
            if (inventorySlot.InstantiatedItem == null) {
                return;
            }
            contextMenuService.ShowContextMenu(this, Input.mousePosition);
        }

        public override void Accept() {
            base.Accept();
            InteractWithSlot();
        }

        public override void Cancel() {
            base.Cancel();
            if (handScript.MoveableOwner != null) {
                handScript.CancelMove();
                ShowContextInfo();
            }
        }

        // x button on gamepad
        public override void JoystickButton2() {
            //Debug.Log("SlotScript.JoystickButton2()");
            base.JoystickButton2();

            if (handScript.MoveableOwner != null) {
                return;
            }

            if (inventorySlot.InstantiatedItem == null) {
                return;
            }

            if (inventorySlot.InstantiatedItem is IUseable) {
                actionBarManager.StartUseableAssignment(inventorySlot.InstantiatedItem as IUseable);
                uIManager.assignToActionBarsWindow.OpenWindow();
            }
        }

        // y button on gamepad
        public override void JoystickButton3() {
            base.JoystickButton3();
        }

        public override void JoystickButton9() {
            base.JoystickButton9();

            if (controlsManager.GamepadModeActive == true && handScript.MoveableOwner != null) {
                if (playerManagerClient.UnitController.CharacterInventoryManager.FromSlot != null) {
                    DropItemFromInventorySlot();
                    ShowContextInfo();
                    return;
                }
            }

            if (!inventorySlot.IsEmpty) {
                // This slot has something in it, and the hand script is empty, so we are trying to pick it up
                if (handScript.MoveableOwner == null) {
                    SendItemToHandScript();
                    ShowContextInfo();
                    return;
                }
            }
        }

        public override void Select() {
            //Debug.Log("SlotScript.Select()");
            base.Select();

            if (controlsManager.GamepadModeActive == true && handScript.MoveableOwner != null) {
                handScript.SetPosition(transform.position);
            }

            ShowContextInfo();
        }

        /// <summary>
        /// show or hide the appropriate tooltip and controller hints
        /// </summary>
        private void ShowContextInfo() {
            if (owner != null) {

                if (handScript.MoveableOwner != null) {
                    owner.SetControllerHints("", "", "", "Cancel Reorder", "Move", "Place");
                    return;
                }

                if (inventorySlot.InstantiatedItem == null) {
                    uIManager.HideToolTip();
                    owner.HideControllerHints();
                    return;
                }
                ShowGamepadTooltip();

                if (inventorySlot.InstantiatedItem is IUseable) {
                    owner.SetControllerHints("Menu", "Add To Action Bars", "", "", "", "Reorder");
                } else {
                    owner.SetControllerHints("Menu", "", "", "", "", "Reorder");
                }
            }
        }



        public override void DeSelect() {
            //Debug.Log("SlotScript.DeSelect()");
            base.DeSelect();
            if (owner != null) {
                owner.HideControllerHints();
            }
            uIManager.HideToolTip();
        }

        public void CancelHandscriptMove() {
            Debug.Log("SlotScript.CancelHandscriptMove() instanceId: {GetInstanceID()}");

            UpdateSlot();
        }

        /// <summary>
        /// Updates the Stack Size count graphic
        /// </summary>
        public void UpdateSlot() {
            //Debug.Log($"SlotScript.UpdateSlot(): Update Slot called on slot {GetInstanceID().ToString()}");

            if (inventorySlot == null) {
                // the inventory slot that this script was referencing no longer exists
                return;
            }

            SetDescribable(inventorySlot.InstantiatedItem?.Item);
            uIManager.UpdateStackSize(this, Count);
            SetBackGroundColor();

            if (selected) {
                ShowContextInfo();
            }
        }

        private void ClearSlot() {
            //Debug.Log($"SlotScript.ClearSlot(): Clear Slot called on slot {GetInstanceID().ToString()}");

            inventorySlot = null;
            SetDescribable(null, 0);
            uIManager.UpdateStackSize(this, Count);
            SetBackGroundColor();
        }

        public void SetBackGroundColor() {
            //Debug.Log($"{gameObject.name}.SlotScript.SetBackGroundColor()");

            Color finalColor;
            if (inventorySlot?.InstantiatedItem == null) {
                int slotOpacityLevel = (int)(PlayerPrefs.GetFloat("InventorySlotOpacity") * 255);
                finalColor = new Color32(0, 0, 0, (byte)slotOpacityLevel);
                backGroundImage.sprite = null;
                if (backGroundImage != null) {
                    //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor(): background image is not null, setting color: " + slotOpacityLevel);
                    backGroundImage.color = finalColor;
                } else {
                    //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor(): background image IS NULL!");
                }
            } else {
                // check if the item has a quality.  if not, just do the default color
                uIManager.SetItemBackground(inventorySlot.InstantiatedItem.Item, backGroundImage, new Color32(0, 0, 0, 255), inventorySlot.InstantiatedItem.ItemQuality);

            }
            //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor()");
        }

        
        public override void ShowToolTip() {
            //base.ShowToolTip
            //uIManager.ShowToolTip(transform.position, describable, "Sell Price: ");
            ShowGamepadTooltip();
        }

        public void ShowGamepadTooltip() {
            //Debug.Log($"SlotScript.ShowGamepadTooltip() bagPanel: {(BagPanel == null ? "null" : "not null")} inventoryslot: {(inventorySlot.InstantiatedItem == null ? "null" : "not null")}");

            uIManager.ShowGamepadTooltip((BagPanel.ContentArea as RectTransform), transform, this);
        }

        public void ProcessShowTooltip(TooltipController tooltipController) {
            if (inventorySlot.InstantiatedItem != null) {
                tooltipController.UpdateCurrencyAmount(inventorySlot.InstantiatedItem, "Sell Price: ");
            }
        }

        public override void OnSendObjectToPool() {
            //Debug.Log("SlotScript.OnSendObjectToPool()");
            // this is being called manually for now because if the bag is closed, the message will not be received

            base.OnSendObjectToPool();

            ClearInventorySlot();
        }

        public void SetupContextMenu(ContextMenuPanel contextMenuPanel) {
            //Debug.Log("SlotScript.SetupContextMenu()");

            // there are no actions for an empty slot
            if (inventorySlot == null || inventorySlot.InstantiatedItem == null) {
                return;
            }
            BagPanel.SetupContextMenu(contextMenuPanel, inventorySlot);
        }

        public void PerformContextMenuAction(string actionName) {
            //Debug.Log($"SlotScript.PerformContextMenuAction({actionName})");
            BagPanel.PerformContextMenuAction(this, actionName);
        }

        public string GetSummary() {
            return inventorySlot.InstantiatedItem?.GetSummary() + GetWeightString();
        }

        public string GetDescription() {
            return inventorySlot.InstantiatedItem?.GetDescription() + GetWeightString();
        }

        public string GetWeightString() {
            if (inventorySlot.InstantiatedItem == null) {
                return string.Empty;
            }
            if (inventorySlot.InstantiatedItems.Count <= 1) {
                return string.Empty;
            }
            return $"\n<size=12><color=yellow>Stack Weight: {inventorySlot.InstantiatedItem.Item.Weight * inventorySlot.Count} kg</color></size>";
        }
    }

}