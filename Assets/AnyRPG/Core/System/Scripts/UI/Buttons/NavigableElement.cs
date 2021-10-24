using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class NavigableElement : ConfiguredMonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {

        public event System.Action OnInteract = delegate { };

        private RectTransform rectTransform = null;

        public virtual bool DeselectOnLeave { get => true; }
        public virtual bool CaptureCancelButton { get => false; }
        public RectTransform RectTransform { get => rectTransform; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            rectTransform = transform as RectTransform;
        }

        public virtual void Accept() {
            Debug.Log(gameObject.name + "NavigableElement.Accept()");
            Interact();
        }

        public virtual void Cancel() {

        }

        public virtual void Select() {

        }

        public virtual void DeSelect() {
            Debug.Log(gameObject.name + "NavigableElement.DeSelect()");
        }

        public virtual void Interact() {
            OnInteract();
        }

        public virtual void LeaveElement() {
            Debug.Log(gameObject.name + ".NavigableElement.LeaveElement()");
            if (DeselectOnLeave) {
                DeSelect();
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData) {
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
        }

        public virtual void OnPointerDown(PointerEventData eventData) {
        }

        public virtual void OnPointerUp(PointerEventData eventData) {
        }
    }

}