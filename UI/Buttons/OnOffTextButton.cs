using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
// this is almost identical to questscript

public class OnOffTextButton : MonoBehaviour {

    [SerializeField]
    private Text text;

    [SerializeField]
    private GameObject hightlightImage;

    [SerializeField]
    private bool useHighlightColor;

    [SerializeField]
    private Color highlightColor = Color.blue;

    [SerializeField]
    private Color baseColor = Color.gray;

    public Text MyText { get => text; }

    public void Select() {
        //Debug.Log(gameObject.name + ".HightlightButton.Select()");
        text.text = "on";
    }

    public void DeSelect() {
        //Debug.Log(gameObject.name + ".HightlightButton.DeSelect()");
        text.text = "off";
    }

    public virtual void OnHoverSound() {
        AudioManager.MyInstance.PlayUIHoverSound();
    }

    public virtual void OnClickSound() {
        AudioManager.MyInstance.PlayUIClickSound();
    }

}

}