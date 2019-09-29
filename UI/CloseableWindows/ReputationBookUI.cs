using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReputationBookUI : MonoBehaviour, IPagedWindowContents {

    public event System.Action OnPageCountUpdateHandler = delegate { };
    public event System.Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };
    public event System.Action<ICloseableWindowContents> OnCloseWindowHandler = delegate { };

    [SerializeField]
    private FactionButton[] factionButtons;

    private List<List<FactionDisposition>> pages = new List<List<FactionDisposition>>();

    private int pageSize = 10;

    private int pageIndex;

    [SerializeField]
    private Image backGroundImage;

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
        Debug.Log("ReputationBookUI.CreatePages()");
        pages.Clear();
        List<FactionDisposition> page = new List<FactionDisposition>();
        for (int i = 0; i < PlayerManager.MyInstance.MyCharacter.MyPlayerFactionManager.MyDispositionDictionary.Count; i++) {
            page.Add(PlayerManager.MyInstance.MyCharacter.MyPlayerFactionManager.MyDispositionDictionary[i]);
            if (page.Count == pageSize) {
                pages.Add(page);
                page = new List<FactionDisposition>();
            }
        }
        if (page.Count > 0) {
            pages.Add(page);
        }
        AddReputations();
        OnPageCountUpdateHandler();

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
                    factionButtons[i].AddFaction(pages[pageIndex][i].faction.MyName);
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

    public void OnCloseWindow() {
    }

    public void OnOpenWindow() {
        OnOpenWindowHandler(this);
        CreatePages();
    }
}