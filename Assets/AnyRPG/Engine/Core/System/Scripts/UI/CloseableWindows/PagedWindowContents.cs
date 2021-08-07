using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class PagedWindowContents : ConfiguredMonoBehaviour, IPagedWindowContents {

        public event System.Action<bool> OnPageCountUpdate = delegate { };
        public event System.Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        protected List<PagedContentList> pages = new List<PagedContentList>();

        protected int pageSize = 10;

        protected int pageIndex = 0;

        [SerializeField]
        protected Image backGroundImage;

        public Image BackGroundImage { get => backGroundImage; set => backGroundImage = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
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

        public virtual void CreatePages() {
            //Debug.Log("SkillBookUI.CreatePages()");
            ClearPages();
            PopulatePages();
            OnPageCountUpdate(false);
        }

        protected virtual void PopulatePages() {
            // meant to be overwritten
        }

        public virtual void ClearButtons() {
            //Debug.Log("SkillBookUI.ClearButtons()");
            // meant to be overwritten
        }

        public virtual void LoadPage(int pageIndex) {
            //Debug.Log("PagedWindowContents.LoadPage(" + pageIndex + ")");
            ClearButtons();
            this.pageIndex = pageIndex;
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

    public class PagedContentList {
        // meant to be overwritten
    }
}