using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class SwappableMeshOptionChoiceButton : HighlightButton {

        [Header("Swappable Mesh Image Button")]

        [SerializeField]
        protected Image backGroundImage = null;

        [SerializeField]
        protected Image icon = null;

        private SwappableMeshAppearancePanelController appearancePanelController = null;
        private string optionGroupName = string.Empty;
        private string optionName = string.Empty;

        public void ConfigureButton(SwappableMeshAppearancePanelController appearancePanelController, string groupName, Sprite image, string displayName, string optionName) {
            this.appearancePanelController = appearancePanelController;
            optionGroupName = groupName;
            if (icon != null) {
                icon.sprite = image;
            }
            this.optionName = optionName;
            if (text != null) {
                text.text = displayName;
            }
        }

        public void MakeSelection() {
            appearancePanelController.ChooseOptionChoice(this, optionGroupName, optionName);
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

    }

}