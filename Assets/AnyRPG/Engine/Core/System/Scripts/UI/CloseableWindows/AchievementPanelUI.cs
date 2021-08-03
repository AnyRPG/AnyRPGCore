using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class AchievementPanelUI : PagedWindowContents {

        [SerializeField]
        private List<AchievementButton> resourceButtons = new List<AchievementButton>();

        protected override void PopulatePages() {
            //Debug.Log("AchievementPanelUI.CreatePages()");
            QuestContentList page = new QuestContentList();
            foreach (Quest quest in SystemDataFactory.Instance.GetResourceList<Quest>()) {
                if (quest.MyIsAchievement && quest.TurnedIn) {
                    page.quests.Add(quest);
                }
                if (page.quests.Count == pageSize) {
                    pages.Add(page);
                    page = new QuestContentList();
                }
            }
            if (page.quests.Count > 0) {
                pages.Add(page);
            }
            AddResources();
        }

        public void AddResources() {
            //Debug.Log("AchievementPanelUI.AddResources()");
            if (pages.Count > 0) {
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log("AchievementPanelUI.AddResources(): i: " + i);
                    if (i < (pages[pageIndex] as QuestContentList).quests.Count) {
                        //Debug.Log("adding ability");
                        resourceButtons[i].gameObject.SetActive(true);
                        resourceButtons[i].AddResource((pages[pageIndex] as QuestContentList).quests[i]);
                    } else {
                        //Debug.Log("clearing ability");
                        resourceButtons[i].ClearResource();
                        resourceButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        public override void LoadPage(int pageIndex) {
            base.LoadPage(pageIndex);
            AddResources();
        }

        public override void ClearButtons() {
            base.ClearButtons();
            foreach (AchievementButton btn in resourceButtons) {
                btn.gameObject.SetActive(false);
            }
        }

    }

    public class QuestContentList : PagedContentList {
        public List<Quest> quests = new List<Quest>();
    }
}