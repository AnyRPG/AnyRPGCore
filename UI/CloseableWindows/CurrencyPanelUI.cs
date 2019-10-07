using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyPanelUI : MonoBehaviour, IPagedWindowContents {

    public event System.Action OnPageCountUpdateHandler = delegate { };
    public event System.Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };
    public event System.Action<ICloseableWindowContents> OnCloseWindowHandler = delegate { };

    [SerializeField]
    private CurrencyButton[] currencyButtons;

    private List<List<CurrencySaveData>> pages = new List<List<CurrencySaveData>>();

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
        //Debug.Log("ReputationBookUI.CreatePages()");
        pages.Clear();
        List<CurrencySaveData> page = new List<CurrencySaveData>();
        foreach (CurrencySaveData currencySaveData in PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.MyCurrencyList.Values) {
            page.Add(currencySaveData);
            if (page.Count == pageSize) {
                pages.Add(page);
                page = new List<CurrencySaveData>();
            }
        }
        if (page.Count > 0) {
            pages.Add(page);
        }
        AddCurrencies();
        OnPageCountUpdateHandler();

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
                    currencyButtons[i].AddCurrency(pages[pageIndex][i].MyName);
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

    public void OnCloseWindow() {
    }

    public void OnOpenWindow() {
        OnOpenWindowHandler(this);
        CreatePages();
    }
}