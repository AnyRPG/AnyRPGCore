﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionBarController : MonoBehaviour {

    [SerializeField]
    private List<ActionButton> actionButtons = new List<ActionButton>();

    private bool initialized = false;

    public List<ActionButton> MyActionButtons { get => actionButtons; set => actionButtons = value; }

    /*
    private void CommonInitialization() {
        this.gameObject.SetActive(true);
    }
    */

    [SerializeField]
    protected Image backGroundImage;

    public virtual void Awake() {
        if (backGroundImage == null) {
            backGroundImage = GetComponent<Image>();
        }
    }

    /*
    public void SetTarget(GameObject target) {
        CommonInitialization();
    }

    public void ClearTarget() {
        this.gameObject.SetActive(false);
    }
    */

    public void ClearActionBar() {
        //Debug.Log(gameObject.name + ".ActionBarController.ClearActionBar()");
        for (int i = 0; i < actionButtons.Count; i++) {
            //Debug.Log(gameObject.name + ".ActionBarController.ClearActionBar(): clearing button: " + i);
            actionButtons[i].ClearUseable();
        }
    }


    public bool AddNewAbility(string newAbility) {
        //Debug.Log("AbilityBarController.AddNewAbility(" + newAbility.MyName + ")");
        BaseAbility ability = SystemAbilityManager.MyInstance.GetResource(newAbility);
        for (int i = 0; i < actionButtons.Count; i++) {
            if (actionButtons[i].MyUseable == null) {
                //Debug.Log("Adding ability: " + newAbility.MyName + " to empty action button " + i);
                actionButtons[i].SetUseable(ability);
                return true;
            } else if (actionButtons[i].MyUseable == (ability as IUseable)) {
                //Debug.Log("Ability exists on bars already!");
                return true;
            }
        }
        return false;
    }

    public void SetBackGroundColor(Color color) {
        if (backGroundImage != null) {
            backGroundImage.color = color;
            RebuildLayout();
        }
    }

    public void OnEnable() {
        //Debug.Log("ActionBarController.OnEnable()");
        RebuildLayout();
    }

    public void OnDisable() {
        //Debug.Log("ActionBarController.OnDisable()");
        RebuildLayout();
    }

    public void RebuildLayout() {
        //Debug.Log("ActionBarController.RebuildLayout()");
        LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.transform.parent.GetComponent<RectTransform>());
    }

    public void UpdateVisuals(bool removeStaleActions = false) {
        for (int i = 0; i < actionButtons.Count; i++) {
            //Debug.Log(gameObject.name + ".ActionBarController.ClearActionBar(): clearing button: " + i);
            actionButtons[i].UpdateVisual(removeStaleActions);
        }
    }


}
