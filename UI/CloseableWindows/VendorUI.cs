using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorUI : WindowContentController, IPagedWindowContents {

        public event System.Action<bool> OnPageCountUpdate = delegate { };
        public override event System.Action<ICloseableWindowContents> OnOpenWindow = delegate { };

        [SerializeField]
        private VendorButton[] vendorButtons;

        [SerializeField]
        private Dropdown dropdown;

        private List<List<VendorItem>> pages = new List<List<VendorItem>>();

        private List<VendorCollection> vendorCollections = new List<VendorCollection>();

        private int pageIndex;

        private int dropDownIndex;

        public override void Awake() {
            base.Awake();
            //vendorUI.CreatePages(items);
        }

        public int GetPageCount() {
            return pages.Count;
        }

        public void CreatePages(List<VendorItem> items) {
            //Debug.Log("VendorUI.CreatePages()");
            ClearPages();
            List<VendorItem> page = new List<VendorItem>();
            for (int i = 0; i < items.Count; i++) {
                page.Add(items[i]);
                if (page.Count == 10 || i == items.Count - 1) {
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
            ClearVendorCollections();
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

        public void PopulateDropDownList(List<VendorCollection> vendorCollections) {
            dropDownIndex = 1;
            VendorCollection buyBackCollection = new VendorCollection();
            this.vendorCollections = new List<VendorCollection>(1 + vendorCollections.Count);
            this.vendorCollections.Add(buyBackCollection);
            this.vendorCollections.AddRange(vendorCollections);
            dropdown.ClearOptions();
            List<string> vendorCollectionNames = new List<string>();
            vendorCollectionNames.Add("Buy Back Items");
            foreach (VendorCollection vendorCollection in vendorCollections) {
                vendorCollectionNames.Add(vendorCollection.MyName);
            }
            dropdown.AddOptions(vendorCollectionNames);
            dropdown.value = dropDownIndex;
            CreatePages(this.vendorCollections[dropDownIndex].MyVendorItems);
        }

        private void ClearVendorCollections() {
            vendorCollections.Clear();
        }

        private void ClearPages() {
            ClearButtons();
            pages.Clear();
            pageIndex = 0;
        }

        public void SetCollection(int dropDownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetHair(" + dropdownIndex + "): " + hairAppearanceDropdown.options[hairAppearanceDropdown.value].text);
            ClearButtons();
            ClearPages();
            this.dropDownIndex = dropDownIndex;
            CreatePages(vendorCollections[dropDownIndex].MyVendorItems);
            LoadPage(0);
            OnPageCountUpdate(false);
        }


    }
}