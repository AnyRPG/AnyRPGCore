using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class ChatCommandManager : ConfiguredMonoBehaviour {


        protected Dictionary<string, ChatCommand> commandDictionary = new Dictionary<string, ChatCommand>();

        // game manager references
        protected SystemDataFactory systemDataFactory = null;
        protected LogManager logManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            // populate the dictionary
            foreach (ChatCommand chatCommand in systemDataFactory.GetResourceList<ChatCommand>()) {
                commandDictionary.Add(chatCommand.DisplayName.ToLower().Replace(" ", ""), chatCommand);
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            logManager = systemGameManager.LogManager;
        }

        public void ParseChatCommand(string commandText) {

            if (commandText == string.Empty) {
                Debug.Log("Empty Chat Message");
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
                commandDictionary[chatCommandString].ExecuteCommand(commandParameters);
            } else {
                logManager.WriteChatMessage("Unknown command : " + chatCommandString);
            }
        }

    }

}