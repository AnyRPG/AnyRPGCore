using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class BagButton : HighlightButton, IDescribable, IMoveableOwner {

        [Header("Bag Button")]

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected Sprite emptySprite = null;

        [SerializeField]
        protected Image backGroundImage = null;

        protected BagNode bagNode = null;

        protected bool localComponentsGotten = false;

        protected BagPanel bagPanel = null;

        // game manager references
        protected PlayerManagerClient playerManagerClient = null;
        protected HandScript handScript = null;
        protected MessageFeedManager messageFeedManager = null;

        public IMoveable Moveable { get => (BagNode != null ? BagNode.InstantiatedBag : null); }

        public BagNode BagNode {
            get {
                return bagNode;
            }

            set {
                //Debug.Log("BagButton.SetBagNode(" + (value == null ? "null" : "valid bagNode") + ")");
                if (value != null) {
                    value.OnAddBag += HandleAddBag;
                    value.OnRemoveBag += HandleRemoveBag;
                } else {
                    if (bagNode != null) {
                        bagNode.OnAddBag -= HandleAddBag;
                        bagNode.OnRemoveBag -= HandleRemoveBag;
                    }
                }
                bagNode = value;
            }
        }

        public Image Image { get => icon; set => icon = value; }

        public Sprite Icon { get => (BagNode.InstantiatedBag != null ? BagNode.InstantiatedBag.Icon : null); }
        public string ResourceName { get => (BagNode.InstantiatedBag != null ? BagNode.InstantiatedBag.ResourceName : null); }
        public string DisplayName { get => (BagNode.InstantiatedBag != null ? BagNode.InstantiatedBag.DisplayName : null); }
        public string Description { get => (BagNode.InstantiatedBag != null ? BagNode.InstantiatedBag.Description : null); }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //GetLocalComponents();
            SetBackGroundColor();
            SetDefaultIcon();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            //inventoryManager = systemGameManager.InventoryManager;
            uIManager = systemGameManager.UIManager;
            handScript = uIManager.HandScript;
            playerManagerClient = systemGameManager.PlayerManagerClient;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
        }

        public void SetBagpanel(BagPanel bagPanel) {
            this.bagPanel = bagPanel;
        }

        public void HandleAddBag(InstantiatedBag instantiatedBag) {
            //Debug.Log($"{gameObject.name}.BagButton.HandleAddBag()");
            icon.sprite = instantiatedBag.Icon;
            icon.color = Color.white;
            SetBackGroundColor();
        }

        public void HandleRemoveBag() {
            //Debug.Log("BagButton.HandleRemoveBag(): setting icon to null");
            /*
            icon.GetComponent<Image>().sprite = null;
            icon.GetComponent<Image>().color = new Color32(0, 0, 0, 0);
            */
            SetDefaultIcon();
            SetBackGroundColor();
        }

        private void SetDefaultIcon() {
            if (emptySprite != null) {
                icon.sprite = emptySprite;
                icon.color = Color.white;
            } else {
                icon.sprite = null;
                icon.color = new Color32(0, 0, 0, 0);
            }
        }

        /*
        public void GetLocalComponents() {
            if (localComponentsGotten == true) {
                return;
            }
            if (backGroundImage == null) {
                //Debug.Log($"{gameObject.name}SlotScript.Awake(): background image is null, trying to get component");
                backGroundImage = GetComponent<Image>();
            }
            SetDefaultIcon();
            localComponentsGotten = true;
        }
        */

        protected override void HandleLeftClick() {
            if (bagNode == null) {
                return;
            }
            base.HandleLeftClick();
            if (handScript.MoveableOwner != null && handScript.MoveableOwner.Moveable is InstantiatedBag) {
                if (bagNode.InstantiatedBag != null) {
                    // there is a bag in this slot already
                    if ((handScript.MoveableOwner.Moveable as InstantiatedBag).BagNode != null) {
                        // bag was moved from a bag bar slot to another bag bar slot with a bag in it, swap equipped bags
                        playerManagerClient.UnitController.CharacterInventoryManager.RequestSwapBags(BagNode.InstantiatedBag, handScript.MoveableOwner.Moveable as InstantiatedBag);
                    } else if (playerManagerClient.UnitController.CharacterInventoryManager.FromSlot != null) {
                        // bag was moved from an inventory slot, swap unequipped bag with equipped bag
                        playerManagerClient.UnitController.CharacterInventoryManager.RequestSwapBags(BagNode.InstantiatedBag, handScript.MoveableOwner.Moveable as InstantiatedBag);
                    }
                    handScript.CancelMove();
                } else {
                    // there is no bag in this slot
                    InstantiatedBag tmpBag = (InstantiatedBag)handScript.MoveableOwner.Moveable;
                    if (tmpBag.BagNode != null) {
                        // bag was moved from a bag bar slot, to an empty bag bar slot, ensure there is enough space to remove bag from old slot before dropping in this slot
                        if (playerManagerClient.UnitController.CharacterInventoryManager.EmptySlotCount(tmpBag.BagNode.IsBankNode) - tmpBag.Slots >= 0) {
                            playerManagerClient.UnitController.CharacterInventoryManager.RequestMoveBag(tmpBag, bagNode);
                            handScript.CancelMove();
                        }
                    } else {
                        // bag came from an inventory slot
                        playerManagerClient.UnitController.CharacterInventoryManager.RequestAddBagFromInventory(tmpBag, bagNode);
                        handScript.CancelMove();
                    }
                }
            } else if (Input.GetKey(KeyCode.LeftShift)) {
                //Debug.Log("BagButton.OnPointerClick() LEFT CLICK DETECTED WITH SHIFT KEY on bagNode.mybag: " + bagNode.MyBag.GetInstanceID());
                //Debug.Log("InventoryManager.RemoveBag(): Found matching bag in bagNode: " + bagNode.MyBag.GetInstanceID() + "; " + bag.GetInstanceID());
                handScript.TakeMoveable(this);
            }
        }

        public string GetSummary() {
            if (BagNode?.InstantiatedBag != null) {
                return BagNode.InstantiatedBag.GetSummary() + "\n\n<color=#00FFFF>Shift + click to remove</color>";
            }
            // cyan
            return string.Format("<color=#00FFFF>Empty Bag Slot</color>\n{0}", GetDescription());
        }

        public string GetDescription() {
            if (BagNode?.InstantiatedBag != null) {
                return BagNode.InstantiatedBag.GetDescription();
            }
            return "Place a bag in this slot to expand your storage";
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);

            //uIManager.ShowToolTip(transform.position, this);
            ShowGamepadTooltip();
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);

            uIManager.HideToolTip();
        }

        public override void Select() {
            base.Select();

            if (controlsManager.GamePadInputActive == false && controlsManager.GamepadModeActive == false) {
                // no reason to show gamepad tooltip if gamepad mode is not active
                return;
            }
            ShowGamepadTooltip();
        }

        private void ShowGamepadTooltip() {
            if (bagNode?.InstantiatedBag != null) {
                uIManager.ShowGamepadTooltip((bagPanel.ContentArea as RectTransform), transform, this);
                bagPanel.SetControllerHints("Unequip", "", "", "", "", "");
            } else {
                uIManager.ShowGamepadTooltip((bagPanel.ContentArea as RectTransform), transform, this);
                bagPanel.HideControllerHints();
            }
        }

        public void ProcessShowTooltip(TooltipController tooltipController) {
            if (bagNode?.InstantiatedBag != null) {
                tooltipController.UpdateCurrencyAmount(bagNode.InstantiatedBag, "Sell Price: ");
            }
        }


        public override void DeSelect() {
            base.DeSelect();
            if (bagPanel != null) {
                bagPanel.HideControllerHints();
            }
            uIManager.HideToolTip();
        }

        public override void Accept() {
            base.Accept();
            if (bagNode?.InstantiatedBag == null) {
                return;
            }
            if (playerManagerClient.UnitController.CharacterInventoryManager.EmptySlotCount((bagPanel is BankPanel)) - bagNode.InstantiatedBag.Slots > 0) {
                //Debug.Log("SlotScript.HandleLeftClick(): We are trying to drop a bag into the inventory. There is enough empty space.");
                playerManagerClient.UnitController.CharacterInventoryManager.RequestUnequipBag(bagNode.InstantiatedBag, (bagPanel is BankPanel));
                ShowGamepadTooltip();
            } else {
                messageFeedManager.WriteMessage("Not enough free inventory slots");
            }
        }

        /*
        // this call never happens so is useless
        public void OnDestroy() {
            BagNode = null;
        }
        */

        public void SetBackGroundColor() {
            //GetLocalComponents();
            Color finalColor;
            finalColor = new Color32(0, 0, 0, 255);
            //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor()");
            if (backGroundImage != null) {
                //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor(): background image is not null, setting color: " + slotOpacityLevel);
                backGroundImage.color = finalColor;
            } else {
                //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor(): background image IS NULL!");
            }
        }

        public void CancelHandscriptMove() {
            Debug.Log("BagButton.CancelHandscriptMove()");
        }
    }

}