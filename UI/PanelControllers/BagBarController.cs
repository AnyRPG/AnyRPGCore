using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagBarController : MonoBehaviour {

    [SerializeField]
    private GameObject BagButtonPrefab;

    [SerializeField]
    private List<BagButton> bagButtons;

    [SerializeField]
    protected Image backGroundImage;

    private bool localComponentsGotten = false;

    protected bool startHasRun = false;
    protected bool eventReferencesInitialized = false;


    public List<BagButton> MyBagButtons { get => bagButtons; set => bagButtons = value; }

    private void Awake() {
        Debug.Log("BagBarController.Awake()");
        GetLocalComponents();
    }

    private void Start() {
        //Debug.Log("BagBarController.Start()");
        SetBackGroundColor();
        startHasRun = true;
        CreateEventReferences();
    }

    private void CreateEventReferences() {
        //Debug.Log("BagBarController.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnInventoryTransparencyUpdate += SetBackGroundColor;
        eventReferencesInitialized = true;
    }

    private void CleanupEventReferences() {
        //Debug.Log("PlayerManager.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnInventoryTransparencyUpdate -= SetBackGroundColor;
        }
        eventReferencesInitialized = false;
    }

    public void GetLocalComponents() {
        if (localComponentsGotten == true) {
            return;
        }
        if (backGroundImage == null) {
            //Debug.Log(gameObject.name + "SlotScript.Awake(): background image is null, trying to get component");
            backGroundImage = GetComponent<Image>();
        }
        localComponentsGotten = true;
    }

    public BagButton AddBagButton() {
        //Debug.Log(gameObject.name + "BagBarController.AddBagButton()");
        foreach (BagButton _bagButton in bagButtons) {
            if (_bagButton.MyBagNode == null) {
                //Debug.Log("BagBarController.AddBagButton(): found an empty bag button");
                return _bagButton;
            }
        }
        //Debug.Log("BagBarController.AddBagButton(): Could not find an unused bag button!");
        return null;
    }

    public BagButton InstantiateBagButton() {
        //Debug.Log("BagBarController.InstantiateBagButton()");
        BagButton bagButton = Instantiate(BagButtonPrefab, this.gameObject.transform).GetComponent<BagButton>();
        bagButtons.Add(bagButton);
        return bagButton;
    }

    public void ClearBagButtons() {
        foreach (BagButton _bagButton in bagButtons) {
            if (_bagButton.MyBagNode != null) {
                if (_bagButton.MyBagNode.MyBag != null) {
                    Destroy(_bagButton.MyBagNode.MyBag);
                    _bagButton.MyBagNode.MyBag = null;
                }
            }
        }
    }

    public void SetBackGroundColor() {
        //Debug.Log("BagBarController.SetBackGroundColor()");
        int opacityLevel = (int)(PlayerPrefs.GetFloat("InventoryOpacity") * 255);
        if (backGroundImage != null) {
            backGroundImage.color = new Color32(0, 0, 0, (byte)opacityLevel);
            RebuildLayout();
        }
    }

    public void RebuildLayout() {
        //Debug.Log("ActionBarController.RebuildLayout()");
        //LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.transform.parent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.transform.GetComponent<RectTransform>());
    }

    public void OnEnable() {
        // testing - here since it probably won't get done if the window is closed
        SetBackGroundColor();
    }

    public void OnDestroy() {
        CleanupEventReferences();
    }

}
