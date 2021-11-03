using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class BagButton : HighlightButton, IDescribable {

        [Header("Bag Button")]

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected Sprite emptySprite = null;

        [SerializeField]
        protected Image backGroundImage = null;

        protected BagNode bagNode = null;

        protected bool localComponentsGotten = false;

        // game manager references
        protected InventoryManager inventoryManager = null;
        protected HandScript handScript = null;

        public BagNode BagNode {
            get {
                return bagNode;
            }

            set {
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

        public Sprite Icon { get => (BagNode.Bag != null ? BagNode.Bag.Icon : null); }
        public string DisplayName { get => (BagNode.Bag != null ? BagNode.Bag.DisplayName : null); }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //GetLocalComponents();
            SetBackGroundColor();
            SetDefaultIcon();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inventoryManager = systemGameManager.InventoryManager;
            uIManager = systemGameManager.UIManager;
            handScript = uIManager.HandScript;
        }

        public void HandleAddBag(Bag bag) {
            //Debug.Log(gameObject.name + ".BagButton.OnAddBag()");
            icon.sprite = bag.Icon;
            icon.color = Color.white;
            SetBackGroundColor();
        }

        public void HandleRemoveBag() {
            //Debug.Log("BagButton.OnRemoveBag(): setting icon to null");
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
                //Debug.Log(gameObject.name + "SlotScript.Awake(): background image is null, trying to get component");
                backGroundImage = GetComponent<Image>();
            }
            SetDefaultIcon();
            localComponentsGotten = true;
        }
        */


        public override void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("BagButton.OnPointerClick()");
            if (bagNode == null) {
                return;
            }

            base.OnPointerClick(eventData);

            if (eventData.button == PointerEventData.InputButton.Left) {
                //Debug.Log("BagButton.OnPointerClick() LEFT CLICK DETECTED");
                if (inventoryManager.FromSlot != null && handScript.Moveable != null && handScript.Moveable is Bag) {
                    if (BagNode.Bag != null) {
                        inventoryManager.SwapBags(BagNode.Bag, handScript.Moveable as Bag);
                    } else {
                        Bag tmp = (Bag)handScript.Moveable;
                        tmp.BagNode = bagNode;
                        tmp.Use();
                        BagNode.AddBag(tmp);
                        handScript.Drop();
                        inventoryManager.FromSlot = null;

                    }
                } else if (Input.GetKey(KeyCode.LeftShift)) {
                    //Debug.Log("BagButton.OnPointerClick() LEFT CLICK DETECTED WITH SHIFT KEY on bagNode.mybag: " + bagNode.MyBag.GetInstanceID());
                    //Debug.Log("InventoryManager.RemoveBag(): Found matching bag in bagNode: " + bagNode.MyBag.GetInstanceID() + "; " + bag.GetInstanceID());
                    handScript.TakeMoveable(BagNode.Bag);
                }
            }
        }

        public string GetDescription() {
            if (BagNode?.Bag != null) {
                return BagNode.Bag.GetDescription();
            }
            // cyan
            return string.Format("<color=#00FFFF>Empty Bag Slot</color>\n{0}", GetSummary());
        }

        public string GetSummary() {
            if (BagNode?.Bag != null) {
                return BagNode.Bag.GetSummary();
            }
            return "Place a bag in this slot to expand your storage";
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);

            uIManager.ShowToolTip(transform.position, this);
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);

            uIManager.HideToolTip();
        }

        public void OnDestroy() {
            BagNode = null;
        }

        public void SetBackGroundColor() {
            //GetLocalComponents();
            Color finalColor;
            finalColor = new Color32(0, 0, 0, 255);
            //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor()");
            if (backGroundImage != null) {
                //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor(): background image is not null, setting color: " + slotOpacityLevel);
                backGroundImage.color = finalColor;
            } else {
                //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor(): background image IS NULL!");
            }
        }

    }

}