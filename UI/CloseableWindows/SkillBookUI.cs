using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillBookUI : MonoBehaviour, IPagedWindowContents {

    public event System.Action OnPageCountUpdateHandler = delegate { };
    public event System.Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };
    public event System.Action<ICloseableWindowContents> OnCloseWindowHandler = delegate { };

    [SerializeField]
    private SkillButton[] skillButtons;

    private List<List<string>> pages = new List<List<string>>();

    private int pageSize = 10;

    private int pageIndex;

    private void Start() {
        //Debug.Log("SkillBookUI.Start()");
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
        Debug.Log("SkillBookUI.CreatePages()");
        ClearButtons();
        pages.Clear();
        List<string> page = new List<string>();
        foreach (string skillName in PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.MySkillList.Keys) {
            page.Add(skillName);
            if (page.Count == pageSize) {
                pages.Add(page);
                page = new List<string>();
            }
        }
        if (page.Count > 0) {
            pages.Add(page);
        }
        AddSkills();
        OnPageCountUpdateHandler();

    }

    public void AddSkills() {
        //Debug.Log("SkillBookUI.AddSkills()");
        if (pages.Count > 0) {
            for (int i = 0; i < pageSize; i++) {
                //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                //Debug.Log("SkillBookUI.AddSkills(): i: " + i);
                if (i < pages[pageIndex].Count) {
                    //Debug.Log("adding skill");
                    skillButtons[i].gameObject.SetActive(true);
                    skillButtons[i].AddSkill(pages[pageIndex][i]);
                } else {
                    //Debug.Log("clearing skill");
                    skillButtons[i].ClearSkill();
                    skillButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ClearButtons() {
        foreach (SkillButton btn in skillButtons) {
            btn.gameObject.SetActive(false);
        }
    }

    public void LoadPage(int pageIndex) {
        ClearButtons();
        this.pageIndex = pageIndex;
        AddSkills();
    }

    public void OnCloseWindow() {
    }

    public void OnOpenWindow() {
        OnOpenWindowHandler(this);
        CreatePages();
    }
}