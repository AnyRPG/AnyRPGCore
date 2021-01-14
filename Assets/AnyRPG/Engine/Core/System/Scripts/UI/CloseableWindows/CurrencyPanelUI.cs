using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
public class CurrencyPanelUI : MonoBehaviour, IPagedWindowContents {

    public event System.Action<bool> OnPageCountUpdate = delegate { };
    public event System.Action<ICloseableWindowContents> OnCloseWindow = delegate { };

    [SerializeField]
    private List<CurrencyButton> currencyButtons = new List<CurrencyButton>();

    private List<List<CurrencyNode>> pages = new List<List<CurrencyNode>>();

    private int pageSize = 10;

    private int pageIndex = 0;

    [SerializeField]
    private Image backGroundImage = null;

    public Image MyBackGroundImage { get => backGroundImage; set => backGroundImage = value; }

    public virtual void Awake() {
        if (backGroundImage == null) {
            backGroundImage = GetComponent<Image>();
        }
    }

    public void SetBackGroundColor(Color color) {
        if (backGroundImage != null) {
            backGroundImage.color = color;
        }
    }

    public int GetPageCount() {
        return pages.Count;
    }

    public void CreatePages() {
        //Debug.Log("ReputationBookUI.CreatePages()");
        ClearPages();
        List<CurrencyNode> page = new List<CurrencyNode>();
        foreach (CurrencyNode currencySaveData in PlayerManager.MyInstance.MyCharacter.CharacterCurrencyManager.MyCurrencyList.Values) {
            page.Add(currencySaveData);
            if (page.Count == pageSize) {
                pages.Add(page);
                page = new List<CurrencyNode>();
            }
        }
        if (page.Count > 0) {
            pages.Add(page);
        }
        AddCurrencies();
        OnPageCountUpdate(false);

    }

    public void AddCurrencies() {
        //Debug.Log("ReputationBookUI.AddAbilities()");
        if (pages.Count > 0) {
            for (int i = 0; i < pageSize; i++) {
                //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                //Debug.Log("ReputationBookUI.AddAbilities(): i: " + i);
                if (i < pages[pageIndex].Count) {
                    //Debug.Log("adding ability");
                    currencyButtons[i].gameObject.SetActive(true);
                    currencyButtons[i].AddCurrency(pages[pageIndex][i].currency);
                } else {
                    //Debug.Log("clearing ability");
                    currencyButtons[i].ClearCurrency();
                    currencyButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ClearButtons() {
        foreach (CurrencyButton btn in currencyButtons) {
            btn.gameObject.SetActive(false);
        }
    }

    public void LoadPage(int pageIndex) {
        ClearButtons();
        this.pageIndex = pageIndex;
        AddCurrencies();
    }

    public void RecieveClosedWindowNotification() {
    }

    public void ReceiveOpenWindowNotification() {
        CreatePages();
    }

    private void ClearPages() {
        ClearButtons();
        pages.Clear();
        pageIndex = 0;
    }

}
}