using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ReputationBookUI : PagedWindowContents {

        [SerializeField]
        private List<FactionButton> factionButtons = new List<FactionButton>();

        private PlayerManager playerManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            playerManager = systemGameManager.PlayerManager;

            foreach (FactionButton factionButton in factionButtons) {
                factionButton.Configure(systemGameManager);
            }
        }

        protected override void PopulatePages() {
            //Debug.Log("ReputationBookUI.CreatePages()");
            FactionDispositionContentList page = new FactionDispositionContentList();
            for (int i = 0; i < playerManager.MyCharacter.CharacterFactionManager.DispositionDictionary.Count; i++) {
                page.factionDispositions.Add(playerManager.MyCharacter.CharacterFactionManager.DispositionDictionary[i]);
                if (page.factionDispositions.Count == pageSize) {
                    pages.Add(page);
                    page = new FactionDispositionContentList();
                }
            }
            if (page.factionDispositions.Count > 0) {
                pages.Add(page);
            }
            AddReputations();
        }

        public void AddReputations() {
            //Debug.Log("ReputationBookUI.AddAbilities()");
            if (pages.Count > 0) {
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log("ReputationBookUI.AddAbilities(): i: " + i);
                    if (i < (pages[pageIndex] as FactionDispositionContentList).factionDispositions.Count) {
                        //Debug.Log("adding ability");
                        factionButtons[i].gameObject.SetActive(true);
                        factionButtons[i].AddFaction((pages[pageIndex] as FactionDispositionContentList).factionDispositions[i].Faction);
                    } else {
                        //Debug.Log("clearing ability");
                        factionButtons[i].ClearFaction();
                        factionButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        public override void LoadPage(int pageIndex) {
            base.LoadPage(pageIndex);
            AddReputations();
        }

        public override void ClearButtons() {
            base.ClearButtons();
            foreach (FactionButton btn in factionButtons) {
                btn.gameObject.SetActive(false);
            }
        }

    }

    public class FactionDispositionContentList : PagedContentList {
        public List<FactionDisposition> factionDispositions = new List<FactionDisposition>();
    }

}