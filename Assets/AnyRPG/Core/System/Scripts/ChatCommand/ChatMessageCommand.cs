using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Chat Message Command", menuName = "AnyRPG/Chat Commands/Chat Message Command")]
    public class ChatMessageCommand : ChatCommand {

        [Header("Chat Message Command")]

        [Tooltip("The type of message to send")]
        [SerializeField]
        private ChatMessageType messageType = ChatMessageType.Private;

        // game manager references
        MessageLogServer messageLogServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            messageLogServer = systemGameManager.MessageLogServer;
        }

        public override void ExecuteCommand(string commandParameters, int accountId) {
            //Debug.Log($"ChatMessageCommand.ExecuteCommand() Executing command {DisplayName} with parameters ({commandParameters})");

            if (messageType == ChatMessageType.Group) {
                SendGroupMessage(accountId, commandParameters);
                return;
            }

            if (commandParameters.Contains(" ") == false) {
                return;
            }

            string[] parameterList = commandParameters.Split(' ');
            string playerName = parameterList[0];
            string[] messageArray = parameterList.Skip(1).ToArray();
            SendPrivateMessage(accountId, playerName, string.Join(' ', messageArray));
        }

        private void SendPrivateMessage(int accountId, string targetPlayerName, string messageText) {
            //Debug.Log($"ChatMessageCommand.SendPrivateMessage({accountId}, {targetPlayerName}, {messageText})");

            messageLogServer.SendPrivateMessage(accountId, targetPlayerName, messageText);
        }

        private void SendGroupMessage(int accountId, string messageText) {
            messageLogServer.SendGroupMessage(accountId, messageText);
        }


    }

    public enum ChatMessageType { Private, Group }

}