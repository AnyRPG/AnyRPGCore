using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class NavigableDropdown : HighlightButton {

        [Header("Dropdown")]

        [SerializeField]
        TMP_Dropdown dropDown = null;

        protected bool interacting = false;

        protected ScrollRect scrollRect = null;
        protected RectTransform contentPane = null;
        protected Toggle[] dropDownList = null;
        protected Toggle selectedItem = null;

        // game managager references


        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

        }

        public override bool CaptureCancelButton {
            get {
                if (interacting == true) {
                    return true;
                }
                return base.CaptureCancelButton;
            }
        }

        public override bool CaptureDPad {
            get {
                if (interacting == true) {
                    return true;
                }
                return base.CaptureDPad;
            }
        }

        /*
        public override void Interact() {
            base.Interact();
            interacting = true;
            //inputField.ActivateInputField();
            dropDown.Show();
        }
        */

        public override void UpButton() {
            base.UpButton();
            if (dropDown.value > 0) {
                dropDown.value--;
                dropDown.RefreshShownValue();
                dropDown.Hide();
                dropDown.Show();
                ScrollToCurrentItem();
            }
        }

        public override void DownButton() {
            base.DownButton();
            if (dropDown.value < dropDown.options.Count) {
                dropDown.value++;
                dropDown.RefreshShownValue();
                dropDown.Hide();
                dropDown.Show();
                ScrollToCurrentItem();
            }
        }

        public override void Cancel() {
            base.Cancel();
            interacting = false;
            dropDown.Hide();
        }

        public override void Accept() {
            base.Accept();
            if (!interacting) {
                interacting = true;
                dropDown.Show();
                //Debug.Log("Dropdown value: " + dropDown.value);
                ScrollToCurrentItem();
            } else {
                interacting = false;
                dropDown.Hide();
            }
        }

        protected void ScrollToCurrentItem() {
            StartCoroutine(ScrollDelay());
        }

        public IEnumerator ScrollDelay() {
            yield return new WaitForEndOfFrame();
            scrollRect = dropDown.gameObject.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null) {
                //Debug.Log("scrollRect: " + scrollRect.gameObject.name);
                contentPane = scrollRect.content;
                if (contentPane != null) {
                    dropDownList = contentPane.GetComponentsInChildren<Toggle>();
                    //Debug.Log("Toggle Count: " + dropDownList.Length + "; current: " + dropDown.value + "; current position: " + contentPane.localPosition);
                    contentPane.localPosition = GetSnapToPositionToBringChildIntoView(scrollRect, dropDownList[dropDown.value].transform as RectTransform);
                }
            }
        }

        public Vector2 GetSnapToPositionToBringChildIntoView(ScrollRect instance, RectTransform child) {
            //Debug.Log($"{gameObject.name}.UINavigationListVertical.GetSnapToPositionToBringChildIntoView()");
            Canvas.ForceUpdateCanvases();
            Vector2 viewportLocalPosition = instance.viewport.localPosition;
            Vector2 childLocalPosition = child.localPosition;
            Vector2 result = new Vector2(
                //0 - (viewportLocalPosition.x + childLocalPosition.x),
                instance.content.localPosition.x,
                0 - (viewportLocalPosition.y + childLocalPosition.y + (child.rect.height / 2))
            );
            //Debug.Log("return " + result);
            return result;
        }

    }

}