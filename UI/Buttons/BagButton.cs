using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagButton : MonoBehaviour, IPointerClickHandler, IDescribable, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
    private BagNode bagNode;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Sprite emptySprite;

    [SerializeField]
    private Image backGroundImage;

    private bool localComponentsGotten = false;

    /*
    [SerializeField]
    private int bagIndex;
    */

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

    public Image Icon { get => icon; set => icon = value; }

    public Sprite MyIcon { get => (MyBagNode.MyBag != null ? MyBagNode.MyBag.MyIcon : null);  }
    public string MyName { get => (MyBagNode.MyBag != null ? MyBagNode.MyBag.MyName : null); }

    public void OnAddBag(Bag bag) {
        //Debug.Log("BagButton.OnAddBag: setting icon: " + bag.MyIcon.name);
        icon.GetComponent<Image>().sprite = bag.MyIcon;
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

    private void Awake() {
        GetLocalComponents();
    }

    private void Start() {
        SetBackGroundColor();
    }

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
        Debug.Log("BagButton.OnPointerClick()");
        if (eventData.button == PointerEventData.InputButton.Left) {
            Debug.Log("BagButton.OnPointerClick() LEFT CLICK DETECTED");
            if (InventoryManager.MyInstance.FromSlot != null && HandScript.MyInstance.MyMoveable != null && HandScript.MyInstance.MyMoveable is Bag) {
                if (MyBagNode.MyBag != null) {
                    InventoryManager.MyInstance.SwapBags(MyBagNode.MyBag, HandScript.MyInstance.MyMoveable as Bag);
                } else {
                    Bag tmp = (Bag)HandScript.MyInstance.MyMoveable;
                    tmp.MyBagNode = bagNode;
                    tmp.Use();
                    MyBagNode.MyBag = tmp;
                    HandScript.MyInstance.Drop();
                    InventoryManager.MyInstance.FromSlot = null;

                }
            } else if (Input.GetKey(KeyCode.LeftShift)) {
                Debug.Log("BagButton.OnPointerClick() LEFT CLICK DETECTED WITH SHIFT KEY on bagNode.mybag: " + bagNode.MyBag.GetInstanceID());
                //Debug.Log("InventoryManager.RemoveBag(): Found matching bag in bagNode: " + bagNode.MyBag.GetInstanceID() + "; " + bag.GetInstanceID());
                HandScript.MyInstance.TakeMoveable(MyBagNode.MyBag);
            } else if (bagNode.MyBag != null) {
                bagNode.MyBagWindow.ToggleOpenClose();
            }
        }
    }

    public string GetDescription() {
        if (MyBagNode.MyBag != null) {
            return MyBagNode.MyBag.GetDescription();
        }
        return string.Format("<color=cyan>Empty Bag Slot</color>\n{0}", GetSummary());
    }

    public string GetSummary() {
        if (MyBagNode.MyBag != null) {
            return MyBagNode.MyBag.GetSummary();
        }
        return "Place a bag in this slot to expand your storage";
    }

    public void OnPointerEnter(PointerEventData eventData) {

        UIManager.MyInstance.ShowToolTip(transform.position, this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.MyInstance.HideToolTip();
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
        if (bagNode.MyBag == null) {
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
