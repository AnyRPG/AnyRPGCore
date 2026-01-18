using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class ChatCommandManager : ConfiguredClass {


        protected Dictionary<string, ChatCommand> commandDictionary = new Dictionary<string, ChatCommand>();

        // game manager references
        protected MessageLogClient logManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            // populate the dictionary
            foreach (ChatCommand chatCommand in systemDataFactory.GetResourceList<ChatCommand>()) {
                commandDictionary.Add(chatCommand.ResourceName.ToLower().Replace(" ", ""), chatCommand);
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            logManager = systemGameManager.MessageLogClient;
        }

        public void ParseChatCommand(string commandText, int accountId) {
            //Debug.Log($"ChatCommandManager.ParseChatCommand({commandText}, {accountId})");

            if (commandText == string.Empty) {
                //Debug.Log("Empty Chat Message");
                return;
            }

            // all dictionaries used for commands, items, resources, etc have lowercase keys
            // convert incoming command to lowercase for compatibility
            commandText = commandText.ToLower();

            string chatCommandString = string.Empty;
            string commandParameters = string.Empty;
            if (commandText.Contains(" ")) {
                int index = commandText.IndexOf(' ');
                chatCommandString = commandText.Substring(0, index);
                if (commandText.Length > index) {
                    commandParameters = commandText.Substring(index + 1);
                }
            } else {
                chatCommandString = commandText;
            }

            if (commandDictionary.ContainsKey(chatCommandString)) {
                commandDictionary[chatCommandString].ExecuteCommand(commandParameters, accountId);
            }/* else {
                logManager.RequestChatMessageClient("Unknown command : " + chatCommandString);
            }*/
        }

    }

}