using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SkillBookUI : MonoBehaviour, IPagedWindowContents {

        public event System.Action<bool> OnPageCountUpdate = delegate { };
        public event System.Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private List<SkillButton> skillButtons = new List<SkillButton>();

        private List<List<Skill>> pages = new List<List<Skill>>();

        private int pageSize = 10;

        private int pageIndex = 0;

        [SerializeField]
        private Image backGroundImage;

        public Image MyBackGroundImage { get => backGroundImage; set => backGroundImage = value; }

        public virtual void Awake() {
            if (backGroundImage == null) {
                backGroundImage = GetComponent<Image>();
            }
        }

        public void Init() {
            // nothing for now, here to satisfy interface.  fix me at some point if possible
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
            //Debug.Log("SkillBookUI.CreatePages()");
            ClearPages();
            List<Skill> page = new List<Skill>();
            foreach (Skill playerSkill in SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterSkillManager.MySkillList.Values) {
                page.Add(playerSkill);
                if (page.Count == pageSize) {
                    pages.Add(page);
                    page = new List<Skill>();
                }
            }
            if (page.Count > 0) {
                pages.Add(page);
            }
            AddSkills();
            OnPageCountUpdate(false);

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
            //Debug.Log("SkillBookUI.ClearButtons()");
            foreach (SkillButton btn in skillButtons) {
                btn.gameObject.SetActive(false);
            }
        }

        public void LoadPage(int pageIndex) {
            //Debug.Log("SkillBookUI.LoadPage(" + pageIndex + ")");
            ClearButtons();
            this.pageIndex = pageIndex;
            AddSkills();
        }

        public void RecieveClosedWindowNotification() {
        }

        public void ReceiveOpenWindowNotification() {
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            CreatePages();
        }

        private void ClearPages() {
            ClearButtons();
            pages.Clear();
            pageIndex = 0;
        }

    }
}