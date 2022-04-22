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

        protected PagedWindowContents pagedWindowContents = null;

        //private int pageIndex = 0;

        // game manager references

        protected ControlsManager controlsManager = null;

        public int PageIndex {
            get {
                if (pagedWindowContents != null) {
                    return pagedWindowContents.PageIndex;
                }
                return 0;
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            if (windowContents != null) {
                pagedWindowContents.OnPageCountUpdate += UpdateNavigationArea;
            } else {
                Debug.Log(gameObject.name + ".PagedWindow.Awake(): Could not find window contents.  Check inspector.");
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            controlsManager = systemGameManager.ControlsManager;
        }

        public override void GetWindowContents() {
            base.GetWindowContents();
            pagedWindowContents = contentGameObject.GetComponent<PagedWindowContents>();
        }

        public override void SetContentOwner() {
            //Debug.Log(gameObject.name + ".PagedWindow.SetContentOwner()");
            if (pagedWindowContents != null) {
                pagedWindowContents.SetPagedWindow(this);
            }
            base.SetContentOwner();
        }

        public override void OpenWindow() {
            //Debug.Log("PagedWindow.OpenWindow()");
            // do this first because openwindow will update the page count
            //pageIndex = 0;

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
            if (PageIndex < pagedWindowContents.GetPageCount() - 1) {
                //pageIndex++;
                pagedWindowContents.LoadPage(PageIndex + 1);
            }
            UpdateNavigationArea();
        }

        public void PreviousPage() {
            //Debug.Log("PagedWindow.PreviousPage()");

            // ensure we are not on the first page
            if (PageIndex > 0) {
                //pageIndex--;
                pagedWindowContents.LoadPage(PageIndex - 1);
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
            if (pagedWindowContents.GetPageCount() == 0 && closeEmptyWindow == true) {
                CloseWindow();
            }

            /*
            if ((windowContents as PagedWindowContents).GetPageCount() <= PageIndex) {
                // set the page index to the last page
                pageIndex = Mathf.Clamp(pagedWindowContents.GetPageCount() - 1, 0, int.MaxValue);
            }
            */

            previousBtn.GetComponent<Button>().interactable = (PageIndex > 0);
            nextBtn.GetComponent<Button>().interactable = (pagedWindowContents.GetPageCount() > 1 && PageIndex < pagedWindowContents.GetPageCount() - 1);

            pageNumber.text = PageIndex + 1 + "/" + Mathf.Clamp(pagedWindowContents.GetPageCount(), 1, int.MaxValue);
        }

        public void OnScroll(PointerEventData eventData) {
            //Debug.Log(eventData.scrollDelta + ", " + eventData.IsScrolling());
            if (eventData.scrollDelta.y < 0 && eventData.IsScrolling()) {
                if (pagedWindowContents.GetPageCount() > 1) {
                    NextPage();
                }
            } else if (eventData.scrollDelta.y > 0 && eventData.IsScrolling()) {
                if (pagedWindowContents.GetPageCount() > 1) {
                    PreviousPage();
                }
            }

        }

    }

}