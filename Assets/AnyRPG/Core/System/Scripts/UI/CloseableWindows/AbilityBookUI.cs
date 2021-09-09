using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class AbilityBookUI : PagedWindowContents {

        [SerializeField]
        private List<AbilityButton> abilityButtons = new List<AbilityButton>();

        [SerializeField]
        private List<GameObject> abilityButtonHolders = new List<GameObject>();

        private PlayerManager playerManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            playerManager = systemGameManager.PlayerManager;

            foreach (AbilityButton abilityButton in abilityButtons) {
                abilityButton.Configure(systemGameManager);
            }
        }

        protected override void PopulatePages() {
            //Debug.Log("AbilityBookUI.CreatePages()");
            BaseAbilityContentList page = new BaseAbilityContentList();
            foreach (BaseAbility newAbility in playerManager.MyCharacter.CharacterAbilityManager.AbilityList.Values) {
                if (newAbility.RequirementsAreMet()) {
                    page.baseAbilities.Add(newAbility);
                    if (page.baseAbilities.Count == pageSize) {
                        pages.Add(page);
                        page = new BaseAbilityContentList();
                    }
                }
            }
            if (page.baseAbilities.Count > 0) {
                pages.Add(page);
            }
            AddAbilities();
        }

        public void AddAbilities() {
            //Debug.Log("AbilityBookUI.AddAbilities()");
            if (pages.Count > 0) {
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log("AbilityBookUI.AddAbilities(): i: " + i);
                    if (i < (pages[pageIndex] as BaseAbilityContentList).baseAbilities.Count) {
                        //Debug.Log("adding ability");
                        abilityButtonHolders[i].SetActive(true);
                        abilityButtons[i].AddAbility((pages[pageIndex] as BaseAbilityContentList).baseAbilities[i]);
                        abilityButtons[i].SetBackGroundTransparency();
                    } else {
                        //Debug.Log("clearing ability");
                        abilityButtons[i].ClearAbility();
                        abilityButtonHolders[i].SetActive(false);
                    }
                }
            }
        }

        public override void LoadPage(int pageIndex) {
            base.LoadPage(pageIndex);
            AddAbilities();
        }

        public override void ClearButtons() {
            base.ClearButtons();
            foreach (GameObject go in abilityButtonHolders) {
                go.SetActive(false);
            }
        }

    }

    public class BaseAbilityContentList : PagedContentList {
        public List<BaseAbility> baseAbilities = new List<BaseAbility>();
    }

}