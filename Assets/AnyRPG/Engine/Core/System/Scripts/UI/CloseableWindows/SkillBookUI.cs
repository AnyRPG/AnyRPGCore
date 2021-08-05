using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SkillBookUI : PagedWindowContents {

        [SerializeField]
        private List<SkillButton> skillButtons = new List<SkillButton>();


        private PlayerManager playerManager = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);
            playerManager = systemGameManager.PlayerManager;

            foreach (SkillButton skillButton in skillButtons) {
                skillButton.Init(systemGameManager);
            }
        }

        protected override void PopulatePages() {
            //Debug.Log("SkillBookUI.CreatePages()");
            SkillContentList page = new SkillContentList();
            foreach (Skill playerSkill in playerManager.MyCharacter.CharacterSkillManager.MySkillList.Values) {
                page.skills.Add(playerSkill);
                if (page.skills.Count == pageSize) {
                    pages.Add(page);
                    page = new SkillContentList();
                }
            }
            if (page.skills.Count > 0) {
                pages.Add(page);
            }
            AddSkills();
        }

        public void AddSkills() {
            //Debug.Log("SkillBookUI.AddSkills()");
            if (pages.Count > 0) {
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log("SkillBookUI.AddSkills(): i: " + i);
                    if (i < (pages[pageIndex] as SkillContentList).skills.Count) {
                        //Debug.Log("adding skill");
                        skillButtons[i].gameObject.SetActive(true);
                        skillButtons[i].AddSkill((pages[pageIndex] as SkillContentList).skills[i]);
                    } else {
                        //Debug.Log("clearing skill");
                        skillButtons[i].ClearSkill();
                        skillButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        public override void LoadPage(int pageIndex) {
            base.LoadPage(pageIndex);
            AddSkills();
        }

        public override void ClearButtons() {
            base.ClearButtons();
            foreach (SkillButton btn in skillButtons) {
                btn.gameObject.SetActive(false);
            }
        }

    }

    public class SkillContentList : PagedContentList {
        public List<Skill> skills = new List<Skill>();
    }
}