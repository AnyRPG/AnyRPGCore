using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Chat Command", menuName = "AnyRPG/ChatCommand")]
    public abstract class ChatCommand : DescribableResource {

        //[Header("Chat Command")]

        /*
        [SerializeField]
        private ChatCommandType commandType = ChatCommandType.Action;
        */

        // game manager references
        protected PlayerManagerClient playerManagerClient = null;
        protected PlayerManagerServer playerManagerServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public virtual void ExecuteCommand(string commandParameters, int accountId) {
            //Debug.Log("ChatCommand.ExecuteCommand(): Executing command " + ResourceName + " with parameters (" + commandParameters + ")");
        }

    }

    //public enum ChatCommandType { Action, GainItem, GainCurrency }

}