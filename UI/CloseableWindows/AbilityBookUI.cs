using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class AbilityBookUI : MonoBehaviour, IPagedWindowContents {

        public event System.Action<bool> OnPageCountUpdate = delegate { };
        public event System.Action<ICloseableWindowContents> OnOpenWindow = delegate { };
        public event System.Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private List<AbilityButton> abilityButtons = new List<AbilityButton>();

        [SerializeField]
        private List<GameObject> abilityButtonHolders = new List<GameObject>();

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
            //Debug.Log("AbilityBookUI.CreatePages()");
            ClearPages();
            List<IAbility> page = new List<IAbility>();
            foreach (IAbility newAbility in PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityList.Values) {
                if (newAbility.RequirementsAreMet()) {
                    page.Add(newAbility);
                    if (page.Count == pageSize) {
                        pages.Add(page);
                        page = new List<IAbility>();
                    }
                }
            }
            if (page.Count > 0) {
                pages.Add(page);
            }
            AddAbilities();
            OnPageCountUpdate(false);

        }

        public void AddAbilities() {
            //Debug.Log("AbilityBookUI.AddAbilities()");
            if (pages.Count > 0) {
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log("AbilityBookUI.AddAbilities(): i: " + i);
                    if (i < pages[pageIndex].Count) {
                        //Debug.Log("adding ability");
                        abilityButtonHolders[i].SetActive(true);
                        abilityButtons[i].AddAbility(pages[pageIndex][i]);
                        abilityButtons[i].SetBackGroundTransparency();
                    } else {
                        //Debug.Log("clearing ability");
                        abilityButtons[i].ClearAbility();
                        abilityButtonHolders[i].SetActive(false);
                    }
                }
            }
        }

        public void ClearButtons() {
            foreach (GameObject go in abilityButtonHolders) {
                go.SetActive(false);
            }
        }

        public void LoadPage(int pageIndex) {
            ClearButtons();
            this.pageIndex = pageIndex;
            AddAbilities();
        }

        public void RecieveClosedWindowNotification() {
        }

        public void ReceiveOpenWindowNotification() {
            OnOpenWindow(this);
            CreatePages();
        }

        private void ClearPages() {
            ClearButtons();
            pages.Clear();
            pageIndex = 0;
        }

    }
}