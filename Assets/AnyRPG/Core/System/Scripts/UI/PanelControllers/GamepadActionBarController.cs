using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class GamepadActionBarController : ActionBarController {

        [Header("Gamepad")]

        [SerializeField]
        TMP_Text dPadDownHint = null;

        [SerializeField]
        TMP_Text dPadRightHint = null;

        [SerializeField]
        TMP_Text dPadLeftHint = null;

        [SerializeField]
        TMP_Text dPadUpHint = null;

        [SerializeField]
        TMP_Text buttonDownHint = null;

        [SerializeField]
        TMP_Text buttonRightHint = null;

        [SerializeField]
        TMP_Text buttonLeftHint = null;

        [SerializeField]
        TMP_Text buttonUpHint = null;

        [SerializeField]
        protected Image dPadButtonHints = null;

        [SerializeField]
        protected Image aButtonHint = null;

        [SerializeField]
        protected Image bButtonHint = null;

        [SerializeField]
        protected Image xButtonHint = null;

        [SerializeField]
        protected Image yButtonHint = null;

        protected Color hiddenColor = new Color32(0, 0, 0, 0);

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            HideHints();
        }


        public void HideHints() {
            dPadDownHint.text = "";
            dPadRightHint.text = "";
            dPadLeftHint.text = "";
            dPadUpHint.text = "";
            buttonDownHint.text = "";
            buttonRightHint.text = "";
            buttonLeftHint.text = "";
            buttonUpHint.text = "";
            dPadButtonHints.color = hiddenColor;
            aButtonHint.color = hiddenColor;
            bButtonHint.color = hiddenColor;
            xButtonHint.color = hiddenColor;
            yButtonHint.color = hiddenColor;
        }

        public void ShowHints() {
            if (actionButtons[0].Useable != null) {
                dPadDownHint.text = actionButtons[0].Useable.DisplayName;
            }
            if (actionButtons[1].Useable != null) {
                dPadRightHint.text = actionButtons[1].Useable.DisplayName;
            }
            if (actionButtons[2].Useable != null) {
                dPadLeftHint.text = actionButtons[2].Useable.DisplayName;
            }
            if (actionButtons[3].Useable != null) {
                dPadUpHint.text = actionButtons[3].Useable.DisplayName;
            }
            if (actionButtons[4].Useable != null) {
                buttonDownHint.text = actionButtons[4].Useable.DisplayName;
            }
            if (actionButtons[5].Useable != null) {
                buttonRightHint.text = actionButtons[5].Useable.DisplayName;
            }
            if (actionButtons[6].Useable != null) {
                buttonLeftHint.text = actionButtons[6].Useable.DisplayName;
            }
            if (actionButtons[7].Useable != null) {
                buttonUpHint.text = actionButtons[7].Useable.DisplayName;
            }
            dPadButtonHints.color = Color.white;
            aButtonHint.color = Color.white;
            bButtonHint.color = Color.white;
            xButtonHint.color = Color.white;
            yButtonHint.color = Color.white;

        }

    }

}