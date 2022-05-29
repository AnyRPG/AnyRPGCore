using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Composite Command", menuName = "AnyRPG/Chat Commands/Composite Command")]
    public class CompositeCommand : ChatCommand {

        [Header("Composite Command")]

        [Tooltip("A list of chat commands to execute")]
        [SerializeField]
        private List<string> chatCommands = new List<string>();

        // game manager references
        protected ChatCommandManager chatCommandManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            chatCommandManager = systemGameManager.ChatCommandManager;
        }

        public override void ExecuteCommand(string commandParameters) {
            //Debug.Log("GainItemCommand.ExecuteCommand() Executing command " + DisplayName + " with parameters (" + commandParameters + ")");

            foreach (string chatCommand in chatCommands) {
                chatCommandManager.ParseChatCommand(chatCommand);
            }
        }


    }

}