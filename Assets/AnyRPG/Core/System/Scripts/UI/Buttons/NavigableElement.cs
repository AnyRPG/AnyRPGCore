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

        public virtual bool DeselectOnLeave { get => true; }
        public virtual bool CaptureCancelButton { get => false; }

        public virtual void Accept() {
            Interact();
        }

        public virtual void Cancel() {

        }

        public virtual void Select() {

        }

        public virtual void DeSelect() {

        }

        public virtual void Interact() {
            OnInteract();
        }

        public virtual void LeaveElement() {
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