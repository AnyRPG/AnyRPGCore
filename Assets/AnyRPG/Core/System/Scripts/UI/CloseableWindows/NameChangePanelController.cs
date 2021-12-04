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
        private TMP_InputField textInput = null;

        /*
        [SerializeField]
        private HighlightButton confirmButton = null;

        [SerializeField]
        private HighlightButton cancelButton = null;
        */

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //confirmButton.Configure(systemGameManager);
            //cancelButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
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
            //Debug.Log("NameChangePanelController.ReceiveOpenWindowNotification()");
            base.ReceiveOpenWindowNotification();
            textInput.text = playerManager.MyCharacter.CharacterName;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("NameChangePanelController.ReceiveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
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