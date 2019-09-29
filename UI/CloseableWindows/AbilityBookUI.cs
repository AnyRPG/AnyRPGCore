using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityBookUI : MonoBehaviour, IPagedWindowContents {

    public event System.Action OnPageCountUpdateHandler = delegate { };
    public event System.Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };
    public event System.Action<ICloseableWindowContents> OnCloseWindowHandler = delegate { };

    [SerializeField]
    private AbilityButton[] abilityButtons;

    private List<List<IAbility>> pages = new List<List<IAbility>>();

    private int pageSize = 10;

    private int pageIndex;

    private void Start() {
        //Debug.Log("AbilityBookUI.Start()");
    }

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
        Debug.Log("AbilityBookUI.CreatePages()");
        pages.Clear();
        List<IAbility> page = new List<IAbility>();
        foreach (IAbility newAbility in PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.MyAbilityList.Values) {
            page.Add(newAbility);
            if (page.Count == pageSize) {
                pages.Add(page);
                page = new List<IAbility>();
            }
        }
        if (page.Count > 0) {
            pages.Add(page);
        }
        AddAbilities();
        OnPageCountUpdateHandler();

    }

    public void AddAbilities() {
        //Debug.Log("AbilityBookUI.AddAbilities()");
        if (pages.Count > 0) {
            for (int i = 0; i < pageSize; i++) {
                //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                //Debug.Log("AbilityBookUI.AddAbilities(): i: " + i);
                if (i < pages[pageIndex].Count) {
                    //Debug.Log("adding ability");
                    abilityButtons[i].gameObject.SetActive(true);
                    abilityButtons[i].AddAbility(pages[pageIndex][i]);
                    abilityButtons[i].SetBackGroundTransparency();
                } else {
                    //Debug.Log("clearing ability");
                    abilityButtons[i].ClearAbility();
                    abilityButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ClearButtons() {
        foreach (AbilityButton btn in abilityButtons) {
            btn.gameObject.SetActive(false);
        }
    }

    public void LoadPage(int pageIndex) {
        ClearButtons();
        this.pageIndex = pageIndex;
        AddAbilities();
    }

    public void OnCloseWindow() {
    }

    public void OnOpenWindow() {
        OnOpenWindowHandler(this);
        CreatePages();
    }
}