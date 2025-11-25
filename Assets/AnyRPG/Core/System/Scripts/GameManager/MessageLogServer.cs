using UnityEngine;

namespace AnyRPG {
    public class MessageLogServer : ConfiguredClass {

        // game manager references
        private ChatCommandManager chatCommandManager = null;
        private MessageLogClient messageLogClient = null;
        private CharacterGroupServiceServer characterGroupServiceServer = null;
        private PlayerManagerServer playerManagerServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("LogManager.Awake()");
            base.Configure(systemGameManager);

            chatCommandManager = systemGameManager.ChatCommandManager;
            messageLogClient = systemGameManager.MessageLogClient;
            characterGroupServiceServer = systemGameManager.CharacterGroupServiceServer;
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public void WriteChatMessage(int accountId, string newMessage) {
            //Debug.Log($"LogManager.WriteChatMessageServer({accountId}, {newMessage})");

            if (newMessage.StartsWith("/") == true) {
                chatCommandManager.ParseChatCommand(newMessage.Substring(1), accountId);
                return;
            }

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == 0) {
                return;
            }
            string playerName = playerCharacterService.GetPlayerNameFromId(playerCharacterId);
            string addedText = $"{playerName}: {newMessage}";


            if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerServer.AdvertiseSceneChatMessage(newMessage, addedText, accountId);
            } else {
                messageLogClient.WriteGeneralMessage(addedText);
            }
        }


        public void WriteSystemMessage(UnitController sourceUnitController, string message) {
            //Debug.Log($"LogManager.WriteSystemMessage({sourceUnitController.gameObject.name}, {message})");

            if (systemGameManager.GameMode == GameMode.Local) {
                messageLogClient.WriteSystemMessage(message);
            } else {
                networkManagerServer.AdvertiseSystemMessage(sourceUnitController, message);
            }
        }

        public void SendGroupMessage(int accountId, string messageText) {
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == 0) {
                return;
            }

            string playerName = playerCharacterService.GetPlayerNameFromId(playerCharacterId);
            messageText = $"{playerName}: {messageText}";

            CharacterGroup characterGroup = characterGroupServiceServer.GetCharacterGroupFromCharacterId(playerCharacterId);

            if (systemGameManager.GameMode == GameMode.Local) {
                messageLogClient.WriteGroupMessage(messageText);
            } else {
                networkManagerServer.AdvertiseGroupMessage(characterGroup, messageText);
            }

        }

        public void SendPrivateMessage(int sourceAccountId, string targetPlayerName, string messageText) {
            //Debug.Log($"MessageLogServer.SendPrivateMessage({sourceAccountId}, {targetPlayerName}, {messageText})");

            int sourcePlayerCharacterId = playerManagerServer.GetPlayerCharacterId(sourceAccountId);
            if (sourcePlayerCharacterId == 0) {
                return;
            }
            string sourcePlayerName = playerCharacterService.GetPlayerNameFromId(sourcePlayerCharacterId);

            int targetPlayerCharacterId = playerCharacterService.GetPlayerIdFromName(targetPlayerName);
            if (targetPlayerCharacterId == 0) {
                return;
            }
            // we already have the target player name, but it could have come in lowercase, so we need to lookup the real one
            targetPlayerName = playerCharacterService.GetPlayerNameFromId(targetPlayerCharacterId);

            string sourceMessageText = $"To {targetPlayerName}: {messageText}";
            string targetMessageText = $"From {sourcePlayerName}: {messageText}";

            int targetAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(targetPlayerCharacterId);

            if (systemGameManager.GameMode == GameMode.Local) {
                messageLogClient.WritePrivateMessage(messageText);
            } else {
                networkManagerServer.AdvertisePrivateMessage(sourceAccountId, sourceMessageText);
                networkManagerServer.AdvertisePrivateMessage(targetAccountId, targetMessageText);
            }
        }
    }

}