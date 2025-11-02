using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LogManager : ConfiguredClass {

        public event System.Action<string> OnWriteChatMessage = delegate { };
        public event System.Action<string> OnWriteSystemMessage = delegate { };
        public event System.Action<string> OnWriteCombatMessage = delegate { };
        public event System.Action OnClearChatMessages = delegate { };
        public event System.Action OnClearSystemMessages = delegate { };
        public event System.Action OnClearCombatMessages = delegate { };

        private string welcomeString = "Welcome to";
        private string completeWelcomeString = string.Empty;

        // a list to hold the messages
        //private List<string> combatMessageList = new List<string>();

        private List<GameObject> combatMessageList = new List<GameObject>();

        // a list to hold the messages
        //private List<string> systemMessageList = new List<string>();

        private List<GameObject> systemMessageList = new List<GameObject>();

        // a list to hold the messages
        //private List<string> chatMessageList = new List<string>();

        private List<GameObject> chatMessageList = new List<GameObject>();

        private bool eventSubscriptionsInitialized = false;

        // game manager references
        SystemEventManager systemEventManager = null;
        PlayerManager playerManager = null;
        NetworkManagerClient networkManagerClient = null;
        ChatCommandManager chatCommandManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("CombatLogUI.Awake()");
            base.Configure(systemGameManager);

            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            chatCommandManager = systemGameManager.ChatCommandManager;

            SetWelcomeString();
            ClearLog();
            CreateEventSubscriptions();
        }

        private void SetWelcomeString() {
            completeWelcomeString = welcomeString;
            if (systemConfigurationManager != null) {
                if (systemConfigurationManager.GameName != null && systemConfigurationManager.GameName != string.Empty) {
                    completeWelcomeString += string.Format(" {0}", systemConfigurationManager.GameName);
                }
                if (systemConfigurationManager.GameVersion != null && systemConfigurationManager.GameVersion != string.Empty) {
                    completeWelcomeString += string.Format(" {0}", systemConfigurationManager.GameVersion);
                }
            }
        }

        public void WriteChatMessageClient(string newMessage) {
            //Debug.Log($"LogManager.WriteChatMessageClient({newMessage})");

            OnWriteChatMessage(newMessage);
        }

        public void WriteChatMessageServer(int accountId, string newMessage) {
            //Debug.Log($"LogManager.WriteChatMessageServer({accountId}, {newMessage})");

            if (newMessage.StartsWith("/") == true) {
                chatCommandManager.ParseChatCommand(newMessage.Substring(1), accountId);
                return;
            }

            if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerServer.AdvertiseSceneChatMessage(newMessage, accountId);
            } else {
                WriteChatMessageClient(newMessage);
            }
        }

        public void RequestChatMessageClient(string newMessage) {
            //Debug.Log($"LogManager.RequestChatMessageClient({newMessage})");

            if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.SendSceneChatMessage(newMessage);
            } else {
                WriteChatMessageServer(0, newMessage);
            }
        }

        public void WriteCombatMessage(string newMessage) {
            //Debug.Log("LogManager.WriteCombatMessage(" + newMessage + ")");

            OnWriteCombatMessage(newMessage);
        }

        public void WriteSystemMessage(UnitController sourceUnitController, string message) {
            //Debug.Log($"LogManager.WriteSystemMessage({sourceUnitController.gameObject.name}, {message})");

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) {
                WriteSystemMessage(message);
            } else {
                networkManagerServer.AdvertiseSystemMessage(sourceUnitController, message);
            }
        }

        public void WriteSystemMessage(string newMessage) {
            //Debug.Log($"LogManager.WriteSystemMessage({newMessage})");

            OnWriteSystemMessage(newMessage);
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("LogManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            systemEventManager.OnTakeDamage += HandleTakeDamage;
            SystemEventManager.StartListening("OnPlayerConnectionSpawn", handlePlayerConnectionSpawn);
            SystemEventManager.StartListening("OnPlayerConnectionDespawn", handlePlayerConnectionDespawn);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            ////Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (systemEventManager != null) {
                systemEventManager.OnTakeDamage -= HandleTakeDamage;
                SystemEventManager.StopListening("OnPlayerConnectionSpawn", handlePlayerConnectionSpawn);
                SystemEventManager.StopListening("OnPlayerConnectionDespawn", handlePlayerConnectionDespawn);
            }
            eventSubscriptionsInitialized = false;
        }

        public void handlePlayerConnectionSpawn(string eventName, EventParamProperties eventParamProperties) {
            PrintWelcomeMessages();
        }

        public void handlePlayerConnectionDespawn(string eventName, EventParamProperties eventParamProperties) {
            ClearLog();
        }


        public void HandleTakeDamage(IAbilityCaster source, UnitController targetUnitController, int damage, string abilityName) {
            //Debug.Log("LogManager.HandleTakeDamage()");
            Color textColor = Color.white;
            if (playerManager.UnitController != null && targetUnitController == playerManager.UnitController) {
                textColor = Color.red;
            }
            string combatMessage = string.Format("<color=#{0}>{1} Takes {2} damage from {3}'s {4}</color>", ColorUtility.ToHtmlStringRGB(textColor), targetUnitController.BaseCharacter.CharacterName, damage, source.AbilityManager.Name, abilityName);

            WriteCombatMessage(combatMessage);
        }

        public void ClearLog() {
            //Debug.Log("CombatLogUI.ClearLog()");
            ClearCombatMessages();
            ClearChatMessages();
            ClearSystemMessages();
        }

        private void ClearCombatMessages() {
            //Debug.Log("CombatLogUI.ClearCombatMessages()");
            OnClearCombatMessages();
        }

        private void ClearChatMessages() {
            //Debug.Log("CombatLogUI.ClearChatMessages()");
            OnClearChatMessages();
        }

        private void ClearSystemMessages() {
            //Debug.Log("CombatLogUI.ClearSystemMessages()");
            OnClearSystemMessages();
        }

        public void PrintWelcomeMessages() {
            //Debug.Log("CombatLogUI.PrintWelcomeMessages()");

            WriteChatMessageClient(completeWelcomeString);
            WriteCombatMessage(completeWelcomeString);
            WriteSystemMessage(completeWelcomeString);

        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }

    }

}