using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// this is almost identical to questscript

public class HighlightButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {

    [SerializeField]
    protected Text text;

    [SerializeField]
    protected Image highlightImage;

    [SerializeField]
    protected Button highlightButton;

    [SerializeField]
    protected bool useHighlightColor;

    [SerializeField]
    protected bool useHighlightColorOnButton;

    [SerializeField]
    protected Color highlightColor;

    [SerializeField]
    protected Color baseColor;

    [SerializeField]
    protected Color baseHighlightColor;

    [SerializeField]
    protected bool CapitalizeText = false;

    public Text MyText { get => text; }

    public virtual void Select() {
        //Debug.Log(gameObject.name + ".HighlightButton.Select()");
        if (highlightImage != null) {
            //Debug.Log(gameObject.name + ".HighlightButton.Select(): highlightimage is not null");
            if (useHighlightColor) {
                //Debug.Log(gameObject.name + ".HighlightButton.Select(): highlightimage is not null: setting highlightcolor on image");
                highlightImage.color = highlightColor;
            }
        }
        if (highlightButton != null && useHighlightColorOnButton == true) {
            ColorBlock colorBlock = highlightButton.colors;
            colorBlock.normalColor = highlightColor;
            colorBlock.highlightedColor = highlightColor;
            colorBlock.selectedColor = highlightColor;
            highlightButton.colors = colorBlock;
        }
        if (CapitalizeText == true) {
            text.text = text.text.ToUpper();
        }
    }

    public virtual void DeSelect() {
        //Debug.Log(gameObject.name + ".HightlightButton.DeSelect()");
        if (highlightImage != null) {
            if (useHighlightColor) {
                highlightImage.color = baseColor;
            }
        }
        if (highlightButton != null && useHighlightColorOnButton == true) {
            ColorBlock colorBlock = highlightButton.colors;
            colorBlock.normalColor = baseColor;
            colorBlock.highlightedColor = baseHighlightColor;
            colorBlock.selectedColor = baseHighlightColor;
            highlightButton.colors = colorBlock;
        }
        if (CapitalizeText == true) {
            text.text = text.text.ToLower();
        }
    }

    public virtual void OnHoverSound() {
        AudioManager.MyInstance.PlayUIHoverSound();
    }

    public virtual void OnClickSound() {
        AudioManager.MyInstance.PlayUIClickSound();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        OnHoverSound();
    }

    public void OnPointerClick(PointerEventData eventData) {
        OnClickSound();
    }

    public void OnPointerDown(PointerEventData eventData) {
    }

    public void OnPointerUp(PointerEventData eventData) {
    }
}
