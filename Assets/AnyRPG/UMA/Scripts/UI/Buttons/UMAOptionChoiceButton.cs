using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class UMAOptionChoiceButton : HighlightButton {

        [Header("Swappable Mesh Image Button")]

        [SerializeField]
        protected Image backGroundImage = null;

        [SerializeField]
        protected Image icon = null;

        private UMAAppearanceEditorPanelController appearancePanelController = null;
        private string optionGroupName = string.Empty;
        private string optionChoice = string.Empty;

        public void ConfigureButton(UMAAppearanceEditorPanelController appearancePanelController, string groupName, Sprite image, string optionName, string optionChoice) {
            this.appearancePanelController = appearancePanelController;
            optionGroupName = groupName;
            if (icon != null) {
                icon.sprite = image;
            }
            this.optionChoice = optionChoice;
            if (text != null) {
                text.text = optionName;
            }
        }

        public void MakeSelection() {
            Debug.Log("UMAOptionChoiceButton.MakeSelection()");

            appearancePanelController.SetRecipe(this, optionGroupName, optionChoice);
        }


        public override void JoystickButton2() {
            //Debug.Log("SlotScript.JoystickButton2()");
            base.JoystickButton2();

            MakeSelection();
        }

        public override void JoystickButton3() {
            //Debug.Log("SlotScript.JoystickButton3()");
            base.JoystickButton3();

        }

        public override void JoystickButton9() {
            base.JoystickButton9();

        }

        /*
        public override void Select() {
            //Debug.Log("SlotScript.Select()");
            base.Select();
        }
        */


    }

}