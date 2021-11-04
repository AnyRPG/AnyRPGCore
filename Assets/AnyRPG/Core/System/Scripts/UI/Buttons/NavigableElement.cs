using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class NavigableElement : ConfiguredMonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {

        public event System.Action OnInteract = delegate { };

        protected RectTransform rectTransform = null;

        public virtual bool DeselectOnLeave { get => true; }
        public virtual bool CaptureCancelButton { get => false; }
        public virtual bool CaptureDPad { get => false; }
        public RectTransform RectTransform { get => rectTransform; }

        private int configureCount = 0;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            configureCount++;
            if (configureCount > 1) {
                // disabled because these objects can be pooled
                //Debug.LogWarning(gameObject.name + ".NavigableElement.Configure() This element has been configured multiple times");
                return;
            }

            rectTransform = transform as RectTransform;
        }

        public virtual void Accept() {
            //Debug.Log(gameObject.name + "NavigableElement.Accept()");
            Interact();
        }

        public virtual void Cancel() {

        }

        public virtual void Select() {
            Debug.Log(gameObject.name + "NavigableElement.Select()");
        }

        public virtual void DeSelect() {
            //Debug.Log(gameObject.name + "NavigableElement.DeSelect()");
        }

        public virtual void Interact() {
            OnInteract();
        }

        public virtual void UpButton() {
        }

        public virtual void DownButton() {
        }

        public virtual void LeftButton() {
        }

        public virtual void RightButton() {
        }

        public virtual void LeaveElement() {
            //Debug.Log(gameObject.name + ".NavigableElement.LeaveElement()");
            if (DeselectOnLeave) {
                DeSelect();
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData) {
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
        }

        public virtual void OnPointerDown(PointerEventData eventData) {
        }

        public virtual void OnPointerUp(PointerEventData eventData) {
        }
    }

}