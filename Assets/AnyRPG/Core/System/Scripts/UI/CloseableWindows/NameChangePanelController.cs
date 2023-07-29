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
        private TMP_InputField textInput = null;

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;
        private NameChangeManager nameChangeManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
            nameChangeManager = systemGameManager.NameChangeManager;
        }

        /// <summary>
        /// disable hotkeys and movement while text input is active
        /// </summary>
        public void ActivateTextInput() {
            controlsManager.ActivateTextInput();
        }

        public void DeativateTextInput() {
            controlsManager.DeactivateTextInput();
        }


        public void CancelAction() {
            //Debug.Log("NameChangePanelController.CancelAction()");
            uIManager.nameChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NameChangePanelController.ConfirmAction()");
            if (textInput.text != null && textInput.text != string.Empty) {
                nameChangeManager.ChangePlayerName(textInput.text);
                uIManager.nameChangeWindow.CloseWindow();
            }
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("NameChangePanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            textInput.text = playerManager.UnitController.BaseCharacter.CharacterName;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("NameChangePanelController.ReceiveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
            nameChangeManager.EndInteraction();
        }

        public void HandlePointerClick() {
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnDisableMovement", eventParam);
            controlsManager.ActivateTextInput();
        }

        public void HandleEndEdit() {
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnEnableMovement", eventParam);
            controlsManager.DeactivateTextInput();
        }

    }

}