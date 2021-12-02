using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ActionBarController : ConfiguredMonoBehaviour {

        [SerializeField]
        protected List<ActionButton> actionButtons = new List<ActionButton>();

        //private bool initialized = false;

        public List<ActionButton> ActionButtons { get => actionButtons; set => actionButtons = value; }

        /*
        private void CommonInitialization() {
            this.gameObject.SetActive(true);
        }
        */

        [SerializeField]
        protected Image backGroundImage;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            if (backGroundImage == null) {
                backGroundImage = GetComponent<Image>();
            }

            InitializeActionButtons(systemGameManager);
        }

        public void SetTooltipTransform(RectTransform rectTransform) {
            for (int i = 0; i < actionButtons.Count; i++) {
                actionButtons[i].SetTooltipTransform(rectTransform);
            }
        }

        public void InitializeActionButtons(SystemGameManager systemGameManager) {
            for (int i = 0; i < actionButtons.Count; i++) {
                actionButtons[i].Configure(systemGameManager);
            }
        }


        public void ClearActionBar(bool clearSavedUseables = false) {
            //Debug.Log(gameObject.name + ".ActionBarController.ClearActionBar()");
            for (int i = 0; i < actionButtons.Count; i++) {
                //Debug.Log(gameObject.name + ".ActionBarController.ClearActionBar(): clearing button: " + i);
                actionButtons[i].ClearUseable();
                if (clearSavedUseables) {
                    actionButtons[i].SavedUseable = null;
                }
            }
        }

        public bool AddSavedAbility(BaseAbility newAbility) {
            //Debug.Log("AbilityBarController.AddNewAbility(" + newAbility + ")");
            for (int i = 0; i < actionButtons.Count; i++) {
                if (actionButtons[i].Useable == null && actionButtons[i].SavedUseable != null && actionButtons[i].SavedUseable.DisplayName == newAbility.DisplayName) {
                    //Debug.Log("Adding ability: " + newAbility + " to empty action button " + i);
                    actionButtons[i].SetUseable(newAbility);
                    return true;
                } else if (actionButtons[i].Useable == (newAbility as IUseable)) {
                    //Debug.Log("Ability exists on bars already!");
                    return true;
                }
            }
            return false;
        }

        public bool AddNewAbility(BaseAbility newAbility) {
            //Debug.Log("AbilityBarController.AddNewAbility(" + newAbility + ")");
            for (int i = 0; i < actionButtons.Count; i++) {
                if (actionButtons[i].Useable == null) {
                    //Debug.Log("Adding ability: " + newAbility + " to empty action button " + i);
                    actionButtons[i].SetUseable(newAbility);
                    return true;
                } else if (actionButtons[i].Useable == (newAbility as IUseable)) {
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
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
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

}