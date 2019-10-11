using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PagedWindow : CloseableWindow, IScrollHandler {
    [SerializeField]
    private Text pageNumber;

    [SerializeField]
    private GameObject previousBtn, nextBtn;

    private int pageIndex = 0;

    protected override void Awake() {
        base.Awake();
        (windowContents as IPagedWindowContents).OnPageCountUpdateHandler += UpdateNavigationArea;
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

    private void UpdateNavigationArea() {
        //Debug.Log("PagedWindow.UpdateNavigationArea()");
        if ((windowContents as IPagedWindowContents).GetPageCount() == 0) {
            CloseWindow();
        }

        previousBtn.GetComponent<Button>().interactable = (pageIndex > 0);
        nextBtn.GetComponent<Button>().interactable = ((windowContents as IPagedWindowContents).GetPageCount() > 1 && pageIndex < (windowContents as IPagedWindowContents).GetPageCount() - 1);

        pageNumber.text = pageIndex + 1 + "/" + (windowContents as IPagedWindowContents).GetPageCount();
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
