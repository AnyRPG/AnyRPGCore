using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class BagButton : ConfiguredMonoBehaviour, IPointerClickHandler, IDescribable, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

        private BagNode bagNode = null;

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private Sprite emptySprite = null;

        [SerializeField]
        private Image backGroundImage = null;

        private bool localComponentsGotten = false;

        // game manager references
        private InventoryManager inventoryManager = null;
        private HandScript handScript = null;
        private UIManager uIManager = null;

        public BagNode MyBagNode {
            get {
                return bagNode;
            }

            set {
                if (value != null) {
                    value.OnAddBagHandler += OnAddBag;
                    value.OnRemoveBagHandler += OnRemoveBag;
                } else {
                    if (bagNode != null) {
                        bagNode.OnAddBagHandler -= OnAddBag;
                        bagNode.OnRemoveBagHandler -= OnRemoveBag;
                    }
                }
                bagNode = value;
            }
        }

        public Image Image { get => icon; set => icon = value; }

        public Sprite Icon { get => (MyBagNode.MyBag != null ? MyBagNode.MyBag.Icon : null); }
        public string DisplayName { get => (MyBagNode.MyBag != null ? MyBagNode.MyBag.DisplayName : null); }

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            inventoryManager = systemGameManager.InventoryManager;
            uIManager = systemGameManager.UIManager;
            handScript = uIManager.HandScript;

            GetLocalComponents();
            SetBackGroundColor();
        }

        public void OnAddBag(Bag bag) {
            //Debug.Log("BagButton.OnAddBag: setting icon: " + bag.MyIcon.name);
            icon.GetComponent<Image>().sprite = bag.Icon;
            icon.GetComponent<Image>().color = Color.white;
            SetBackGroundColor();
        }

        public void OnRemoveBag() {
            //Debug.Log("BagButton.OnRemoveBag(): setting icon to null");
            icon.GetComponent<Image>().sprite = null;
            icon.GetComponent<Image>().color = new Color32(0, 0, 0, 0);
            SetBackGroundColor();
        }

        /*
        public int MyBagIndex {
            get {
                return bagIndex;
            }

            set {
                bagIndex = value;
            }
        }
        */

        public void GetLocalComponents() {
            if (localComponentsGotten == true) {
                return;
            }
            if (backGroundImage == null) {
                //Debug.Log(gameObject.name + "SlotScript.Awake(): background image is null, trying to get component");
                backGroundImage = GetComponent<Image>();
            }
            if (emptySprite != null) {
                GetComponent<Image>().sprite = emptySprite;
            }
            localComponentsGotten = true;
        }


        public void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("BagButton.OnPointerClick()");
            if (bagNode == null) {
                return;
            }
            if (eventData.button == PointerEventData.InputButton.Left) {
                //Debug.Log("BagButton.OnPointerClick() LEFT CLICK DETECTED");
                if (inventoryManager.FromSlot != null && handScript.Moveable != null && handScript.Moveable is Bag) {
                    if (MyBagNode.MyBag != null) {
                        inventoryManager.SwapBags(MyBagNode.MyBag, handScript.Moveable as Bag);
                    } else {
                        Bag tmp = (Bag)handScript.Moveable;
                        tmp.MyBagNode = bagNode;
                        tmp.Use();
                        MyBagNode.MyBag = tmp;
                        handScript.Drop();
                        inventoryManager.FromSlot = null;

                    }
                } else if (Input.GetKey(KeyCode.LeftShift)) {
                    //Debug.Log("BagButton.OnPointerClick() LEFT CLICK DETECTED WITH SHIFT KEY on bagNode.mybag: " + bagNode.MyBag.GetInstanceID());
                    //Debug.Log("InventoryManager.RemoveBag(): Found matching bag in bagNode: " + bagNode.MyBag.GetInstanceID() + "; " + bag.GetInstanceID());
                    handScript.TakeMoveable(MyBagNode.MyBag);
                } else if (bagNode?.MyBag != null) {
                    bagNode.BagWindow.ToggleOpenClose();
                }
            }
        }

        public string GetDescription() {
            if (MyBagNode?.MyBag != null) {
                return MyBagNode.MyBag.GetDescription();
            }
            // cyan
            return string.Format("<color=#00FFFF>Empty Bag Slot</color>\n{0}", GetSummary());
        }

        public string GetSummary() {
            if (MyBagNode?.MyBag != null) {
                return MyBagNode.MyBag.GetSummary();
            }
            return "Place a bag in this slot to expand your storage";
        }

        public void OnPointerEnter(PointerEventData eventData) {

            uIManager.ShowToolTip(transform.position, this);
        }

        public void OnPointerExit(PointerEventData eventData) {
            uIManager.HideToolTip();
        }

        public void OnDestroy() {
            MyBagNode = null;
        }

        public void OnPointerDown(PointerEventData eventData) {
        }

        public void OnPointerUp(PointerEventData eventData) {
        }

        public void SetBackGroundColor() {
            GetLocalComponents();
            Color finalColor;
            if (bagNode?.MyBag == null) {
                int slotOpacityLevel = (int)(PlayerPrefs.GetFloat("InventorySlotOpacity") * 255);
                finalColor = new Color32(0, 0, 0, (byte)slotOpacityLevel);
            } else {
                finalColor = new Color32(0, 0, 0, 255);
            }
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