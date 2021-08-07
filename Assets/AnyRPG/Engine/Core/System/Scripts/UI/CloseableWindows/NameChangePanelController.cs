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

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private TMP_InputField textInput;

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
        }


        public void CancelAction() {
            //Debug.Log("NameChangePanelController.CancelAction()");
            uIManager.nameChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NameChangePanelController.ConfirmAction()");
            if (textInput.text != null && textInput.text != string.Empty) {
                playerManager.SetPlayerName(textInput.text);
                OnConfirmAction();
                uIManager.nameChangeWindow.CloseWindow();
            }
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("NameChangePanelController.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            textInput.text = playerManager.MyCharacter.CharacterName;
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