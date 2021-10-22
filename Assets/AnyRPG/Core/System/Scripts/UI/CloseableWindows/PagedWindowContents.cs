using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class PagedWindowContents : CloseableWindowContents, IPagedWindowContents {

        public event System.Action<bool> OnPageCountUpdate = delegate { };
        public override event System.Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        protected List<PagedContentList> pages = new List<PagedContentList>();

        protected int pageSize = 10;

        protected int pageIndex = 0;

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

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
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