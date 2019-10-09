using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VendorUI : WindowContentController, IPagedWindowContents {

    public event System.Action OnPageCountUpdateHandler = delegate { };
    public override event System.Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };

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
        pages.Clear();
        List<VendorItem> page = new List<VendorItem>();
        for (int i = 0; i < items.Length; i++) {
            page.Add(items[i]);
            if (page.Count == 10 || i == items.Length - 1) {
                pages.Add(page);
                page = new List<VendorItem>();
            }
        }
        AddItems();
        OnPageCountUpdateHandler();
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

    public override void OnCloseWindow() {
        //Debug.Log("VendorUI.OnCloseWindow()");
        base.OnCloseWindow();
        ClearButtons();
        pages.Clear();
    }

    public override void OnOpenWindow() {
        //Debug.Log("VendorUI.OnOpenWindow()");
        ClearButtons();
        pages.Clear();
        base.OnOpenWindow();
        OnOpenWindowHandler(this);
        LoadPage(0);
        OnPageCountUpdateHandler();
    }
}