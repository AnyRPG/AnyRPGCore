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
        protected TextMeshProUGUI pageNumber = null;

        [SerializeField]
        protected GameObject previousBtn = null;

        [SerializeField]
        protected GameObject nextBtn = null;

        [SerializeField]
        protected GameObject leftTriggerHint = null;

        [SerializeField]
        protected GameObject rightTriggerHint = null;

        private int pageIndex = 0;

        // game manager references

        protected ControlsManager controlsManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            if (windowContents != null) {
                (windowContents as PagedWindowContents).OnPageCountUpdate += UpdateNavigationArea;
            } else {
                Debug.Log(gameObject.name + ".PagedWindow.Awake(): Could not find window contents.  Check inspector.");
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            controlsManager = systemGameManager.ControlsManager;
        }

        public override void SetContentOwner() {
            Debug.Log(gameObject.name + ".PagedWindow.SetContentOwner()");
            if (windowContents != null) {
                (windowContents as PagedWindowContents).SetPagedWindow(this);
            }
            base.SetContentOwner();
        }

        public override void OpenWindow() {
            //Debug.Log("PagedWindow.OpenWindow()");
            // do this first because openwindow will update the page count
            pageIndex = 0;

            if (controlsManager.GamePadModeActive == true) {
                leftTriggerHint.SetActive(true);
                rightTriggerHint.SetActive(true);
            } else {
                leftTriggerHint.SetActive(false);
                rightTriggerHint.SetActive(false);
            }

            base.OpenWindow();
        }

        public void NextPage() {
            //Debug.Log("PagedWindow.NextPage()");

            // ensure we are not on the last page
            if (pageIndex < (windowContents as PagedWindowContents).GetPageCount() - 1) {
                pageIndex++;
                (windowContents as PagedWindowContents).LoadPage(pageIndex);
            }
            UpdateNavigationArea();
        }

        public void PreviousPage() {
            //Debug.Log("PagedWindow.PreviousPage()");

            // ensure we are not on the first page
            if (pageIndex > 0) {
                pageIndex--;
                (windowContents as PagedWindowContents).LoadPage(pageIndex);
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
            if ((windowContents as PagedWindowContents).GetPageCount() == 0 && closeEmptyWindow == true) {
                CloseWindow();
            }

            if ((windowContents as PagedWindowContents).GetPageCount() <= pageIndex) {
                // set the page index to the last page
                pageIndex = Mathf.Clamp((windowContents as PagedWindowContents).GetPageCount() - 1, 0, int.MaxValue);
            }

            previousBtn.GetComponent<Button>().interactable = (pageIndex > 0);
            nextBtn.GetComponent<Button>().interactable = ((windowContents as PagedWindowContents).GetPageCount() > 1 && pageIndex < (windowContents as PagedWindowContents).GetPageCount() - 1);

            pageNumber.text = pageIndex + 1 + "/" + Mathf.Clamp((windowContents as PagedWindowContents).GetPageCount(), 1, int.MaxValue);
        }

        public void OnScroll(PointerEventData eventData) {
            //Debug.Log(eventData.scrollDelta + ", " + eventData.IsScrolling());
            if (eventData.scrollDelta.y < 0 && eventData.IsScrolling()) {
                if ((windowContents as PagedWindowContents).GetPageCount() > 1) {
                    NextPage();
                }
            } else if (eventData.scrollDelta.y > 0 && eventData.IsScrolling()) {
                if ((windowContents as PagedWindowContents).GetPageCount() > 1) {
                    PreviousPage();
                }
            }

        }

    }

}