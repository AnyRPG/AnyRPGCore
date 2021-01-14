using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
// this is almost identical to questscript

public class TextOptionHighlightArea : MonoBehaviour {

    [SerializeField]
    private List<HighlightButton> highlightButtons = new List<HighlightButton>();

    public List<HighlightButton> MyHighlightButtons { get => highlightButtons; set => highlightButtons = value; }

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