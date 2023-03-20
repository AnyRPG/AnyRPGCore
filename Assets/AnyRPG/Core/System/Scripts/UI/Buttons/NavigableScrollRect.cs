using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class NavigableScrollRect : NavigableElement {

        [Header("ScrollRect")]

        [Tooltip("This scroll rect can be scrolled directly by using the analog stick")]
        [SerializeField]
        protected ScrollRect scrollRect = null;


        public override void LeftAnalog(float inputHorizontal, float inputVertical) {
            //Debug.Log($"{gameObject.name}.NavigableScrollRect.LeftAnalog(" + inputHorizontal + ", " + inputVertical + ")");

            base.LeftAnalog(inputHorizontal, inputVertical);

            if (inputVertical != 0f) {
                float contentHeight = scrollRect.content.sizeDelta.y;
                // 2f is arbitrary number for scrollspeed
                float contentShift = 2f * inputVertical;
                scrollRect.verticalNormalizedPosition += contentShift / contentHeight;
                //Debug.Log("verticalNormalizedPosition: " + scrollRect.verticalNormalizedPosition + "; movement: " + (contentShift / contentHeight));
            }
        }

    }

}