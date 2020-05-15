using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangePanelController : WindowContentController {

        public InputField textInput;
        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };


        public void CancelAction() {
            //Debug.Log("NameChangePanelController.CancelAction()");
            SystemWindowManager.MyInstance.nameChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NameChangePanelController.ConfirmAction()");
            if (textInput.text != null && textInput.text != string.Empty) {
                PlayerManager.MyInstance.SetPlayerName(textInput.text);
                OnConfirmAction();
                SystemWindowManager.MyInstance.nameChangeWindow.CloseWindow();
            }
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("NameChangePanelController.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            textInput.text = PlayerManager.MyInstance.MyCharacter.CharacterName;
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