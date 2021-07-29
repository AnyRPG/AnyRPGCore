using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangePanelController : WindowContentController {

        [SerializeField]
        private TMP_InputField textInput;

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };


        public void CancelAction() {
            //Debug.Log("NameChangePanelController.CancelAction()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.nameChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NameChangePanelController.ConfirmAction()");
            if (textInput.text != null && textInput.text != string.Empty) {
                SystemGameManager.Instance.PlayerManager.SetPlayerName(textInput.text);
                OnConfirmAction();
                SystemGameManager.Instance.UIManager.SystemWindowManager.nameChangeWindow.CloseWindow();
            }
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("NameChangePanelController.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            textInput.text = SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterName;
        }

        public override void RecieveClosedWindowNotification() {
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public void HandlePointerClick() {
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnDisableMovement", eventParam);
        }

        public void HandleEndEdit() {
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnEnableMovement", eventParam);
        }

    }

}