using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class TextOptionHighlightArea : NavigableElement {

        [SerializeField]
        private List<HighlightButton> highlightButtons = new List<HighlightButton>();

        public List<HighlightButton> HighlightButtons { get => highlightButtons; set => highlightButtons = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            foreach (HighlightButton highlightButton in highlightButtons) {
                highlightButton.Configure(systemGameManager);
            }
        }

        public void SelectButton(int buttonIndex) {
            DeSelectButtons();
            //Debug.Log(gameObject.name + ".TextOptionHighlightArea.SelectButton(" + buttonIndex + ")");
            highlightButtons[buttonIndex].Select();
        }

        public void DeSelectButtons() {
            //Debug.Log(gameObject.name + ".HightlightButton.DeSelectButtons()");
            foreach (HighlightButton highlightButton in highlightButtons) {
                //Debug.Log(gameObject.name + ".HightlightButton.DeSelectButtons(): deselecting a button");
                highlightButton.DeSelect();
            }
        }

    }

}