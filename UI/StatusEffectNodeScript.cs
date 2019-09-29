﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusEffectNodeScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Image coolDownIcon;

    [SerializeField]
    private bool useTimerText;

    [SerializeField]
    private Text timer;

    [SerializeField]
    private bool useStackText;

    [SerializeField]
    private Text stackCount;

    private StatusEffectNode statusEffectNode;

    private CharacterUnit target;

    public Text MyTimer { get => timer; }
    public Text MyStackCount { get => stackCount; set => stackCount = value; }
    public Image MyIcon { get => icon; set => icon = value; }
    public bool MyUseTimerText { get => useTimerText; set => useTimerText = value; }
    public bool MyUseStackText { get => useStackText; set => useStackText = value; }

    public void Initialize(StatusEffectNode statusEffectNode, CharacterUnit target) {
        //Debug.Log("StatusEffectNodeScript.Initialize()");
        icon.sprite = statusEffectNode.MyStatusEffect.MyIcon;
        this.statusEffectNode = statusEffectNode;
        this.target = target;
        statusEffectNode.MyStatusEffect.SetStatusNode(this);
    }

    public void OnPointerClick(PointerEventData eventData) {
        //Debug.Log("StatusEffectNodeScript.OnPointerClick()");

        if (eventData.button == PointerEventData.InputButton.Right) {
            HandleRightClick();
        }
    }

    public void HandleRightClick() {
        //Debug.Log("StatusEffectNodeScript.HandleRightClick()");
        if (statusEffectNode != null) {
            //Debug.Log("StatusEffectNodeScript.HandleRightClick(): statusEffect is not null, destroying");
            statusEffectNode.CancelStatusEffect();
        }
        UIManager.MyInstance.HideToolTip();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        //Debug.Log("StatusEffectNodeScript.OnPointerEnter()");

        // show tooltip
        UIManager.MyInstance.ShowToolTip(transform.position, statusEffectNode.MyStatusEffect);
    }

    public void OnPointerExit(PointerEventData eventData) {
        //Debug.Log("StatusEffectNodeScript.OnPointerExit()");

        // hide tooltip
        UIManager.MyInstance.HideToolTip();
    }

    public void UpdateFillIcon(float fillAmount) {
        //Debug.Log("StatusEffectNodeScript.UpdateFillIcon(" + fillAmount + ")");
        if (fillAmount == 0) {
            coolDownIcon.enabled = false;
            return;
        }
        coolDownIcon.enabled = true;
        if (coolDownIcon.sprite != MyIcon.sprite) {
            //Debug.Log("Setting coolDownIcon to match MyIcon");
            coolDownIcon.sprite = MyIcon.sprite;
            coolDownIcon.color = new Color32(0, 0, 0, 150);
            coolDownIcon.fillMethod = Image.FillMethod.Radial360;
            //coolDownIcon.fillOrigin = Image.Origin360.Top;
            coolDownIcon.fillClockwise = true;
        }
        coolDownIcon.fillAmount = fillAmount;
    }

    public void OnPointerDown(PointerEventData eventData) {
    }

    public void OnPointerUp(PointerEventData eventData) {
    }


    /*
    void FixedUpdate()
    {
        
    }
    */
}
