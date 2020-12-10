using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
public class ReputationBookUI : MonoBehaviour, IPagedWindowContents {

    public event System.Action<bool> OnPageCountUpdate = delegate { };
    public event System.Action<ICloseableWindowContents> OnOpenWindow = delegate { };
    public event System.Action<ICloseableWindowContents> OnCloseWindow = delegate { };

    [SerializeField]
    private List<FactionButton> factionButtons = new List<FactionButton>();

    private List<List<FactionDisposition>> pages = new List<List<FactionDisposition>>();

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
        List<FactionDisposition> page = new List<FactionDisposition>();
        for (int i = 0; i < PlayerManager.MyInstance.MyCharacter.CharacterFactionManager.DispositionDictionary.Count; i++) {
            page.Add(PlayerManager.MyInstance.MyCharacter.CharacterFactionManager.DispositionDictionary[i]);
            if (page.Count == pageSize) {
                pages.Add(page);
                page = new List<FactionDisposition>();
            }
        }
        if (page.Count > 0) {
            pages.Add(page);
        }
        AddReputations();
        OnPageCountUpdate(false);

    }

    public void AddReputations() {
        //Debug.Log("ReputationBookUI.AddAbilities()");
        if (pages.Count > 0) {
            for (int i = 0; i < pageSize; i++) {
                //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                //Debug.Log("ReputationBookUI.AddAbilities(): i: " + i);
                if (i < pages[pageIndex].Count) {
                    //Debug.Log("adding ability");
                    factionButtons[i].gameObject.SetActive(true);
                    factionButtons[i].AddFaction(pages[pageIndex][i].Faction);
                } else {
                    //Debug.Log("clearing ability");
                    factionButtons[i].ClearFaction();
                    factionButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ClearButtons() {
        foreach (FactionButton btn in factionButtons) {
            btn.gameObject.SetActive(false);
        }
    }

    public void LoadPage(int pageIndex) {
        ClearButtons();
        this.pageIndex = pageIndex;
        AddReputations();
    }

    public void RecieveClosedWindowNotification() {
    }

    public void ReceiveOpenWindowNotification() {
        OnOpenWindow(this);
        CreatePages();
    }

    private void ClearPages() {
        ClearButtons();
        pages.Clear();
        pageIndex = 0;
    }

}
}