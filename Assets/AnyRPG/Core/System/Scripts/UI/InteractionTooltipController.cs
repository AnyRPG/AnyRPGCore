using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class InteractionTooltipController : ConfiguredMonoBehaviour {

        [SerializeField]
        private TMP_Text descriptionText = null;

        [SerializeField]
        private TMP_Text interactionText = null;

        [SerializeField]
        private RectTransform rectTransform = null;

        // game manager references
        protected UIManager uIManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public void ShowInteractionTooltip(Interactable interactable) {
            if (interactable == null) {
                HideInteractionTooltip();
                return;
            }

            rectTransform.pivot = new Vector2(0, 1);
            gameObject.SetActive(true);

            transform.position = uIManager.MouseOverWindow.transform.position;
            descriptionText.text = interactable.GetDescription();

            if (interactable.InteractionTooltipText != null && interactable.InteractionTooltipText != string.Empty) {
                interactionText.text = interactable.InteractionTooltipText;
            } else {
                interactionText.text = "Interact";
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            float topPoint = rectTransform.rect.yMax + transform.position.y;
            float bottomPoint = rectTransform.rect.yMin + transform.position.y;
            //Debug.Log("screen height : " + Screen.height + "; position: " + position + "; top: " + tooltipRect.rect.yMax + "; bottom: " + tooltipRect.rect.yMin);

            // move up if too low
            if (bottomPoint < 0f) {
                transform.position = new Vector3(transform.position.x, (transform.position.y - bottomPoint) + 20, transform.position.z);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }

            // move down if too high
            if (topPoint > Screen.height) {
                transform.position = new Vector3(transform.position.x, transform.position.y - ((topPoint - Screen.height) + 20), transform.position.z);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        public void HideInteractionTooltip() {
            gameObject.SetActive(false);
        }
    }

}