using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class PagedWindowContents : CloseableWindowContents, IPagedWindowContents {

        public virtual event System.Action<bool> OnPageCountUpdate = delegate { };
        public override event System.Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        protected PagedWindow pagedWindow = null;

        protected List<PagedContentList> pages = new List<PagedContentList>();

        protected int pageSize = 10;

        protected int pageIndex = 0;

        public void SetPagedWindow(PagedWindow pagedWindow) {
            this.pagedWindow = pagedWindow;
        }

        public virtual int GetPageCount() {
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
            //Debug.Log("PagedWindowContents.ClearButtons()");
            // meant to be overwritten
        }

        public virtual void LoadPage(int pageIndex) {
            Debug.Log("PagedWindowContents.LoadPage(" + pageIndex + ")");
            ClearButtons();
            this.pageIndex = pageIndex;
            AddPageContent();
            /*
            if (controlsManager.GamePadModeActive) {
            // future use - highlight page content so context menus can be used, like add to ability bars etc
            }
            */
        }

        public virtual void AddPageContent() {
            // meant to be overwritten
        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            CreatePages();
        }

        public virtual void ClearPages() {
            ClearButtons();
            pages.Clear();
            pageIndex = 0;
        }

        public override bool LeftTrigger() {
            if (base.LeftTrigger()) {
                return true;
            }
            if (pagedWindow != null) {
                pagedWindow.PreviousPage();
            }
            return false;
        }

        public override bool RightTrigger() {
            if (base.RightTrigger()) {
                return true;
            }
            if (pagedWindow != null) {
                pagedWindow.NextPage();
            }
            return false;
        }


    }

    public class PagedContentList {
        // meant to be overwritten
    }
}