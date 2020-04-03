using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class PagedWindow : CloseableWindow, IScrollHandler {
        [SerializeField]
        private Text pageNumber = null;

        [SerializeField]
        private GameObject previousBtn = null;

        [SerializeField]
        private GameObject nextBtn = null;

        private int pageIndex = 0;

        protected override void Awake() {
            base.Awake();
            (windowContents as IPagedWindowContents).OnPageCountUpdate += UpdateNavigationArea;
        }

        public override void OpenWindow() {
            //Debug.Log("PagedWindow.OpenWindow()");
            // do this first because openwindow will update the page count
            pageIndex = 0;

            base.OpenWindow();
        }

        public void NextPage() {
            //Debug.Log("PagedWindow.NextPage()");

            // ensure we are not on the last page
            if (pageIndex < (windowContents as IPagedWindowContents).GetPageCount() - 1) {
                pageIndex++;
                (windowContents as IPagedWindowContents).LoadPage(pageIndex);
            }
            UpdateNavigationArea();
        }

        public void PreviousPage() {
            //Debug.Log("PagedWindow.PreviousPage()");

            // ensure we are not on the first page
            if (pageIndex > 0) {
                pageIndex--;
                (windowContents as IPagedWindowContents).LoadPage(pageIndex);
            }
            UpdateNavigationArea();
        }

        /*
        public void UpdateNavigationArea() {
            UpdateNavigationArea(false);
        }
        */

        private void UpdateNavigationArea(bool closeEmptyWindow = false) {
            //Debug.Log("PagedWindow.UpdateNavigationArea()");
            if ((windowContents as IPagedWindowContents).GetPageCount() == 0 && closeEmptyWindow == true) {
                CloseWindow();
            }

            previousBtn.GetComponent<Button>().interactable = (pageIndex > 0);
            nextBtn.GetComponent<Button>().interactable = ((windowContents as IPagedWindowContents).GetPageCount() > 1 && pageIndex < (windowContents as IPagedWindowContents).GetPageCount() - 1);

            pageNumber.text = pageIndex + 1 + "/" + Mathf.Clamp((windowContents as IPagedWindowContents).GetPageCount(), 1, Mathf.Infinity);
        }

        public void OnScroll(PointerEventData eventData) {
            //Debug.Log(eventData.scrollDelta + ", " + eventData.IsScrolling());
            if (eventData.scrollDelta.y < 0 && eventData.IsScrolling()) {
                if ((windowContents as IPagedWindowContents).GetPageCount() > 1) {
                    NextPage();
                }
            } else if (eventData.scrollDelta.y > 0 && eventData.IsScrolling()) {
                if ((windowContents as IPagedWindowContents).GetPageCount() > 1) {
                    PreviousPage();
                }
            }

        }
    }

}