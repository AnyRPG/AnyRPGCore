using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
public class AchievementPanelUI : MonoBehaviour, IPagedWindowContents {

    public event System.Action<bool> OnPageCountUpdate = delegate { };
    public event System.Action<ICloseableWindowContents> OnCloseWindow = delegate { };

    [SerializeField]
    private List<AchievementButton> resourceButtons = new List<AchievementButton>();

    private List<List<Quest>> pages = new List<List<Quest>>();

    private int pageSize = 10;

    private int pageIndex;

    [SerializeField]
    private Image backGroundImage;

    public Image MyBackGroundImage { get => backGroundImage; set => backGroundImage = value; }

    public virtual void Awake() {
        //Debug.Log("AchievementPanelUI.Awake()");
        if (backGroundImage == null) {
            backGroundImage = GetComponent<Image>();
        }
    }

    public void SetBackGroundColor(Color color) {
        //Debug.Log("AchievementPanelUI.SetBackGroundColor()");
        if (backGroundImage != null) {
            backGroundImage.color = color;
        }
    }

    public int GetPageCount() {
        //Debug.Log("AchievementPanelUI.GetPageCount()");
        return pages.Count;
    }

    public void CreatePages() {
        //Debug.Log("AchievementPanelUI.CreatePages()");
        ClearPages();
        List<Quest> page = new List<Quest>();
        int i = 0;
        foreach (Quest quest in SystemQuestManager.MyInstance.MyResourceList.Values) {
            //Debug.Log("AchievementPanelUI.CreatePages(): questName: " + quest.MyName + "; complete: " + quest.IsComplete + "; turnedin: " + quest.TurnedIn + "; achievement: " + quest.MyIsAchievement);
            if (quest.MyIsAchievement && quest.TurnedIn) {
                //Debug.Log("AchievementPanelUI.CreatePages(): questName: " + quest.MyName + "; complete: " + quest.IsComplete + "; turnedin: " + quest.TurnedIn + "; achievement: " + quest.MyIsAchievement);
                page.Add(quest);
            }
            if (page.Count == pageSize) {
                pages.Add(page);
                page = new List<Quest>();
            }
            i++;
        }
        if (page.Count > 0) {
            pages.Add(page);
        }
        AddResources();
        OnPageCountUpdate(false);

    }

    public void AddResources() {
        //Debug.Log("AchievementPanelUI.AddResources()");
        if (pages.Count > 0) {
            for (int i = 0; i < pageSize; i++) {
                //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                //Debug.Log("AchievementPanelUI.AddResources(): i: " + i);
                if (i < pages[pageIndex].Count) {
                    //Debug.Log("adding ability");
                    resourceButtons[i].gameObject.SetActive(true);
                    resourceButtons[i].AddResource(pages[pageIndex][i]);
                } else {
                    //Debug.Log("clearing ability");
                    resourceButtons[i].ClearResource();
                    resourceButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ClearButtons() {
        //Debug.Log("AchievementPanelUI.ClearButtons()");
        foreach (AchievementButton btn in resourceButtons) {
            btn.gameObject.SetActive(false);
        }
    }

    public void LoadPage(int pageIndex) {
        //Debug.Log("AchievementPanelUI.LoadPage(" + pageIndex + ")");
        ClearButtons();
        this.pageIndex = pageIndex;
        AddResources();
    }

    public void RecieveClosedWindowNotification() {
        //Debug.Log("AchievementPanelUI.OnCloseWindow()");
    }

    public void ReceiveOpenWindowNotification() {
        //Debug.Log("AchievementPanelUI.OnOpenWindow()");
        CreatePages();
    }

    private void ClearPages() {
        ClearButtons();
        pages.Clear();
        pageIndex = 0;
    }

}
}