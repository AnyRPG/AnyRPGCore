using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class PagedWindow : CloseableWindow, IScrollHandler {

        [Header("Paged Window")]

        [SerializeField]
        private TextMeshProUGUI pageNumber = null;

        [SerializeField]
        private GameObject previousBtn = null;

        [SerializeField]
        private GameObject nextBtn = null;

        private int pageIndex = 0;

        protected override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);
            if (windowContents != null) {
                (windowContents as IPagedWindowContents).OnPageCountUpdate += UpdateNavigationArea;
            } else {
                Debug.Log(gameObject.name + ".PagedWindow.Awake(): Could not find window contents.  Check inspector.");
            }
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