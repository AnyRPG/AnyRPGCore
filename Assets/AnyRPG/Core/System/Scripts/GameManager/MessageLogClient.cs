using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MessageLogClient : ConfiguredClass {

        public event System.Action<string> OnWriteGeneralMessage = delegate { };
        public event System.Action<string> OnWriteGroupMessage = delegate { };
        public event System.Action<string> OnWriteGuildMessage = delegate { };
        public event System.Action<string> OnWritePrivateMessage = delegate { };
        public event System.Action<string> OnWriteSystemMessage = delegate { };
        public event System.Action<string> OnWriteCombatMessage = delegate { };
        public event System.Action OnClearGeneralMessages = delegate { };
        public event System.Action OnClearGroupMessages = delegate { };
        public event System.Action OnClearGuildMessages = delegate { };
        public event System.Action OnClearPrivateMessages = delegate { };
        public event System.Action OnClearSystemMessages = delegate { };
        public event System.Action OnClearCombatMessages = delegate { };
        public event System.Action<string> OnBeginPrivateMessage = delegate { };

        private string welcomeString = "Welcome to";
        private string completeWelcomeString = string.Empty;

        private bool eventSubscriptionsInitialized = false;

        // game manager references
        SystemEventManager systemEventManager = null;
        PlayerManager playerManager = null;
        ChatCommandManager chatCommandManager = null;
        MessageLogServer messageLogServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("LogManager.Awake()");
            base.Configure(systemGameManager);

            SetWelcomeString();
            ClearLog();
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
            chatCommandManager = systemGameManager.ChatCommandManager;
            messageLogServer = systemGameManager.MessageLogServer;
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

        public void RequestChatMessageClient(string newMessage) {
            //Debug.Log($"LogManager.RequestChatMessageClient({newMessage})");

            if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.SendSceneChatMessage(newMessage);
            } else {
                messageLogServer.WriteChatMessage(0, newMessage);
            }
        }

        public void WriteGeneralMessage(string newMessage) {
            //Debug.Log($"LogManager.WriteChatMessageClient({newMessage})");

            OnWriteGeneralMessage(newMessage);
        }

        public void WriteGroupMessage(string newMessage) {
            //Debug.Log("LogManager.WriteGroupMessage(" + newMessage + ")");

            OnWriteGroupMessage(newMessage);
            WriteGeneralMessage($"[group] {newMessage}");
        }

        public void WriteGuildMessage(string newMessage) {
            //Debug.Log($"MessageLogClient.WriteGuildMessage({newMessage})");

            OnWriteGuildMessage(newMessage);
            WriteGeneralMessage($"[guild] {newMessage}");
        }

        public void WritePrivateMessage(string newMessage) {
            //Debug.Log("LogManager.WritePrivateMessage(" + newMessage + ")");

            OnWritePrivateMessage(newMessage);
            WriteGeneralMessage($"[private] {newMessage}");
        }

        public void WriteCombatMessage(string newMessage) {
            //Debug.Log("LogManager.WriteCombatMessage(" + newMessage + ")");

            OnWriteCombatMessage(newMessage);
        }

        public void WriteSystemMessage(string newMessage) {
            //Debug.Log($"LogManager.WriteSystemMessage({newMessage})");

            OnWriteSystemMessage(newMessage);
            WriteGeneralMessage($"{newMessage}");
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
            //Debug.Log("LogManager.ClearLog()");

            ClearGroupMessages();
            ClearGuildMessages();
            ClearPrivateMessages();
            ClearCombatMessages();
            ClearChatMessages();
            ClearSystemMessages();
        }

        private void ClearGroupMessages() {
            //Debug.Log("LogManager.ClearGroupMessages()");
            OnClearGroupMessages();
        }

        private void ClearGuildMessages() {
            //Debug.Log("LogManager.ClearGroupMessages()");
            OnClearGuildMessages();
        }

        private void ClearPrivateMessages() {
            //Debug.Log("LogManager.ClearGroupMessages()");
            OnClearPrivateMessages();
        }

        private void ClearCombatMessages() {
            //Debug.Log("LogManager.ClearCombatMessages()");
            OnClearCombatMessages();
        }

        private void ClearChatMessages() {
            //Debug.Log("LogManager.ClearChatMessages()");
            OnClearGeneralMessages();
        }

        private void ClearSystemMessages() {
            //Debug.Log("LogManager.ClearSystemMessages()");
            OnClearSystemMessages();
        }

        public void PrintWelcomeMessages() {
            //Debug.Log("LogManager.PrintWelcomeMessages()");

            WriteGeneralMessage(completeWelcomeString);

        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }

        public void BeginPrivateMessage(string commandText) {
            OnBeginPrivateMessage(commandText);
        }
    }

}