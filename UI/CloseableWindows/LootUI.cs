using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootUI : WindowContentController, IPagedWindowContents {

    #region Singleton
    private static LootUI instance;

    public static LootUI MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<LootUI>();
            }

            return instance;
        }
    }

    #endregion

    [SerializeField]
    private LootButton[] lootButtons;

    private List<List<LootDrop>> pages = new List<List<LootDrop>>();

    private List<LootDrop> droppedLoot = new List<LootDrop>();

    private int pageIndex = 0;

    public event System.Action OnPageCountUpdateHandler = delegate { };
    public override event Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };
    public override event Action<ICloseableWindowContents> OnCloseWindowHandler = delegate { };

    public void CreatePages(List<LootDrop> items) {
        //Debug.Log("LootUI.CreatePages()");
        ClearPages();
        //Debug.Log("LootUI.CreatePages(): done clearing pages");
        List<LootDrop> page = new List<LootDrop>();
        droppedLoot = items;
        for (int i = 0; i < items.Count; i++) {
            page.Add(items[i]);
            if (page.Count == 4 || i == items.Count - 1) {
                pages.Add(page);
                page = new List<LootDrop>();
            }
        }
        //Debug.Log("LootUI.CreatePages(): pages.count: " + pages.Count);
        OnPageCountUpdateHandler();


        AddLoot();
    }

    private void AddLoot() {
        //Debug.Log("LootUI.AddLoot()");
        if (pages.Count > 0) {
            //Debug.Log("LootUI.AddLoot() pages.count: " + pages.Count);

            for (int i = 0; i < pages[pageIndex].Count; i++) {
                if (pages[pageIndex][i] != null) {
                    // set the loot button icon
                    lootButtons[i].MyIcon.sprite = pages[pageIndex][i].MyItem.MyIcon;

                    lootButtons[i].MyLoot = pages[pageIndex][i].MyItem;

                    // make sure the loot button is visible
                    lootButtons[i].gameObject.SetActive(true);

                    string title = string.Format("<color={0}>{1}</color>", QualityColor.MyColors[pages[pageIndex][i].MyItem.MyQuality], pages[pageIndex][i].MyItem.MyName);
                    // set the title
                    lootButtons[i].MyTitle.text = title;
                }
            }
        } else {
            //Debug.Log("LootUI.AddLoot() pages.count: " + pages.Count);
        }
    }

    public void ClearButtons () {
        //Debug.Log("LootUI.ClearButtons()");
        foreach (LootButton button in lootButtons) {
            button.gameObject.SetActive(false);
        }
    }

    public void TakeAllLoot() {
        //Debug.Log("LootUI.TakeAllLoot()");

        while (pages.Count > 0) {
            foreach (LootButton lootButton in lootButtons) {
                if (lootButton.gameObject.activeSelf == true) {
                    lootButton.TakeLoot();
                }
            }
        }
        // TESTING THIS CODE SHOULD NOT BE NEEDED
        /*
        List<LootDrop> lootList = new List<LootDrop>();

        // get all loot into a list
        foreach (List<LootDrop> page in pages) {
            foreach (LootDrop lootDrop in page) {
                lootList.Add(lootDrop);
            }
        }
        Debug.Log("LootUI.TakeAllLoot(): lootList size: " + lootList.Count);
        */
        // take all items in the list
        //foreach (LootDrop lootDrop in lootList) {
        /*
        Item newItem = SystemItemManager.MyInstance.GetNewItem(lootDrop.MyItem.MyName);
        if (newItem != null) {
            Debug.Log("RewardButton.CompleteQuest(): newItem is not null, adding to inventory");
            //InventoryManager.MyInstance.AddItem(newItem);
        }
        */
        /*
            if ((lootDrop.MyItem as CurrencyItem) is CurrencyItem) {
                Debug.Log("LootUI.TakeAllLoot(): item is currency: " + lootDrop.MyItem.MyName);
                if (InventoryManager.MyInstance.AddItem(lootDrop.MyItem)) {
                    Debug.Log("LootUI.TakeAllLoot(): successfully added to inventory: " + lootDrop.MyItem.MyName);
                    TakeLoot(lootDrop.MyItem);
                    Debug.Log("LootUI.TakeAllLoot(): using: " + lootDrop.MyItem.MyName);
                    (lootDrop.MyItem as CurrencyItem).Use();
                }
            } else if ((lootDrop.MyItem as QuestStartItem) is QuestStartItem) {
                Debug.Log("LootUI.TakeAllLoot(): item is questStartItem: " + lootDrop.MyItem.MyName);
                if (InventoryManager.MyInstance.AddItem(lootDrop.MyItem)) {
                    Debug.Log("LootUI.TakeAllLoot(): successfully added to inventory: " + lootDrop.MyItem.MyName);
                    TakeLoot(lootDrop.MyItem);
                    Debug.Log("LootUI.TakeAllLoot(): using: " + lootDrop.MyItem.MyName);
                    (lootDrop.MyItem as QuestStartItem).Use();
                }
            } else {
                Debug.Log("LootUI.TakeAllLoot(): item is normal item");
                if (InventoryManager.MyInstance.AddItem(lootDrop.MyItem)) {
                    TakeLoot(lootDrop.MyItem);
                }
            }
        }
        */
    }

    public void TakeLoot(Item loot) {
        Debug.Log("LootUI.TakeLoot(" + loot.MyName + ")");

        LootDrop lootDrop = pages[pageIndex].Find(x => x.MyItem.MyName == loot.MyName);

        pages[pageIndex].Remove(lootDrop);
        lootDrop.Remove();
        SystemEventManager.MyInstance.NotifyOnTakeLoot();

        if (pages[pageIndex].Count == 0) {
            
            // removes the empty page
            pages.Remove(pages[pageIndex]);

            if (pageIndex == pages.Count && pageIndex > 0) {
                pageIndex--;
            }
            AddLoot();
            OnPageCountUpdateHandler();
        }
    }

    private void ClearPages() {
        //Debug.Log("LootUI.ClearPages(): clearing pages");
        pageIndex = 0;
        pages.Clear();
        ClearButtons();
    }

    public override void OnCloseWindow() {
        //Debug.Log("LootUI.OnCloseWindow(): clearing pages");
        base.OnCloseWindow();
        ClearPages();
        OnCloseWindowHandler(this);
    }

    public override void OnOpenWindow() {
        //Debug.Log("LootUI.OnOpenWindow()");
        base.OnOpenWindow();
        OnOpenWindowHandler(this);
    }

    public int GetPageCount() {
        //Debug.Log("LootUI.GetPageCount()");

        return pages.Count;
    }

    public void LoadPage(int pageIndex) {
        this.pageIndex = pageIndex;
        ClearButtons();
        AddLoot();
    }
}
