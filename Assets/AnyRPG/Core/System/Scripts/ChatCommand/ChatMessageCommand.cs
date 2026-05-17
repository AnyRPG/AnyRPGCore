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

            if (messageType == ChatMessageType.Guild) {
                SendGuildMessage(accountId, commandParameters);
                return;
            }

            if (commandParameters.Contains(" ") == false) {
                return;
            }

            string[] parameterList = commandParameters.Split(' ');
            string playerName = parameterList[0];
            // check if the player name has double quotes and combine parameters until the matching quote is found
            if (playerName.StartsWith('"') && playerName.EndsWith('"') == false) {
                for (int i = 1; i < parameterList.Length; i++) {
                    playerName += " " + parameterList[i];
                    if (parameterList[i].EndsWith('"')) {
                        // found the matching quote
                        // remove quotes from player name
                        playerName = playerName.Substring(1, playerName.Length - 2);
                        // remove used parameters from parameter list
                        parameterList = parameterList.Skip(i + 1).ToArray();
                        break;
                    }
                }
            } else {
                // remove quotes from player name if present
                playerName = playerName.Trim('"');
                parameterList = parameterList.Skip(1).ToArray();
            }
            SendPrivateMessage(accountId, playerName, string.Join(' ', parameterList));
        }

        private void SendPrivateMessage(int accountId, string targetPlayerName, string messageText) {
            //Debug.Log($"ChatMessageCommand.SendPrivateMessage({accountId}, {targetPlayerName}, {messageText})");

            messageLogServer.SendPrivateMessage(accountId, targetPlayerName, messageText);
        }

        private void SendGroupMessage(int accountId, string messageText) {
            messageLogServer.SendGroupMessage(accountId, messageText);
        }

        private void SendGuildMessage(int accountId, string messageText) {
            messageLogServer.SendGuildMessage(accountId, messageText);
        }

    }

    public enum ChatMessageType { Private, Group, Guild }

}