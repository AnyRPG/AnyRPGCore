using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VendorUI : WindowContentController, IPagedWindowContents {

    public event System.Action<bool> OnPageCountUpdate = delegate { };
    public override event System.Action<ICloseableWindowContents> OnOpenWindow = delegate { };

    [SerializeField]
    private VendorButton[] vendorButtons;

    private List<List<VendorItem>> pages = new List<List<VendorItem>>();

    private int pageIndex;

    public override void Awake() {
        base.Awake();
        //vendorUI.CreatePages(items);
    }

    public int GetPageCount() {
        return pages.Count;
    }

    public void CreatePages(VendorItem[] items) {
        //Debug.Log("VendorUI.CreatePages()");
        ClearPages();
        List<VendorItem> page = new List<VendorItem>();
        for (int i = 0; i < items.Length; i++) {
            page.Add(items[i]);
            if (page.Count == 10 || i == items.Length - 1) {
                pages.Add(page);
                page = new List<VendorItem>();
            }
        }
        AddItems();
        OnPageCountUpdate(false);
    }

    public void AddItems() {
        //Debug.Log("VendorUI.AddItems()");
        if (pages.Count > 0) {
            for (int i = 0; i < pages[pageIndex].Count; i++) {
                if (pages[pageIndex][i] != null) {
                    vendorButtons[i].AddItem(pages[pageIndex][i]);
                }
            }
        }
    }

    public void ClearButtons() {
        //Debug.Log("VendorUI.ClearButtons()");
        foreach (VendorButton btn in vendorButtons) {
            //Debug.Log("VendorUI.ClearButtons() setting a button to not active");
            btn.gameObject.SetActive(false);
        }
    }

    public void LoadPage(int pageIndex) {
        //Debug.Log("VendorUI.LoadPage()");
        ClearButtons();
        this.pageIndex = pageIndex;
        AddItems();
    }

    public override void RecieveClosedWindowNotification() {
        //Debug.Log("VendorUI.OnCloseWindow()");
        base.RecieveClosedWindowNotification();
        ClearButtons();
        ClearPages();
    }

    public override void ReceiveOpenWindowNotification() {
        //Debug.Log("VendorUI.OnOpenWindow()");
        ClearButtons();
        ClearPages();
        base.ReceiveOpenWindowNotification();
        OnOpenWindow(this);
        LoadPage(0);
        OnPageCountUpdate(false);
    }

    private void ClearPages() {
        ClearButtons();
        pages.Clear();
        pageIndex = 0;
    }

}