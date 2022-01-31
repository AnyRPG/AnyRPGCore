using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Chat Command", menuName = "AnyRPG/ChatCommand")]
    public class ChatCommand : DescribableResource {

        [Header("Chat Command")]

        [SerializeField]
        private ChatCommandType commandType = ChatCommandType.Action;

        // game manager references
        protected PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void ExecuteCommand(string commandParameters) {
            Debug.Log("Executing command " + DisplayName + " with parameters (" + commandParameters + ")");
        }

    }

    public enum ChatCommandType { Action, GainItem, GainCurrency }

}