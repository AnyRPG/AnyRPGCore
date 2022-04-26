using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class OnScreenKeyboardManager : ConfiguredMonoBehaviour {

        // events
        public event System.Action<TMP_InputField> OnActivateKeyboard = delegate { };

        // game manager references
        private UIManager uIManager = null;


        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public void ActivateKeyboard(TMP_InputField inputField) {
            //Debug.Log("OnScreenKeyboardManager.ActivateKeyboard()");

            OnActivateKeyboard(inputField);
            uIManager.onScreenKeyboardWindow.OpenWindow();
        }


    }

}