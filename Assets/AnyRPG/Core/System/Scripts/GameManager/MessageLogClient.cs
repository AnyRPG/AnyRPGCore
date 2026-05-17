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
        PlayerManagerClient playerManagerClient = null;
        MessageLogServer messageLogServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("MessageLogClient.Awake()");
            base.Configure(systemGameManager);

            SetWelcomeString();
            ClearLog();
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemEventManager = systemGameManager.SystemEventManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
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
            //Debug.Log("MessageLogClient.WriteGroupMessage(" + newMessage + ")");

            OnWriteGroupMessage(newMessage);
            WriteGeneralMessage($"[group] {newMessage}");
        }

        public void WriteGuildMessage(string newMessage) {
            //Debug.Log($"MessageLogClient.WriteGuildMessage({newMessage})");

            OnWriteGuildMessage(newMessage);
            WriteGeneralMessage($"[guild] {newMessage}");
        }

        public void WritePrivateMessage(string newMessage) {
            //Debug.Log("MessageLogClient.WritePrivateMessage(" + newMessage + ")");

            OnWritePrivateMessage(newMessage);
            WriteGeneralMessage($"[private] {newMessage}");
        }

        public void WriteCombatMessage(string newMessage) {
            //Debug.Log($"MessageLogClient.WriteCombatMessage({newMessage})");

            OnWriteCombatMessage(newMessage);
        }

        public void WriteSystemMessage(string newMessage) {
            //Debug.Log($"LogManager.WriteSystemMessage({newMessage})");

            OnWriteSystemMessage(newMessage);
            WriteGeneralMessage($"{newMessage}");
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("MessageLogClient.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            //systemEventManager.OnTakeDamage += HandleTakeDamage;
            //playerManagerClient.OnTakeFallDamage += HandleTakeFallDamage;
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
                //systemEventManager.OnTakeDamage -= HandleTakeDamage;
                //playerManagerClient.OnTakeFallDamage -= HandleTakeFallDamage;
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

        /*
        public void HandleTakeDamage(IAbilityCaster source, UnitController targetUnitController, int damage, string abilityName) {
            //Debug.Log("MessageLogClient.HandleTakeDamage()");
            Color textColor = Color.white;
            if (playerManagerClient.UnitController != null && targetUnitController == playerManagerClient.UnitController) {
                textColor = Color.red;
            }
            string combatMessage = string.Format("<color=#{0}>{1} Takes {2} damage from {3}'s {4}</color>", ColorUtility.ToHtmlStringRGB(textColor), targetUnitController.BaseCharacter.CharacterName, damage, source.AbilityManager.Name, abilityName);

            WriteCombatMessage(combatMessage);
        }
        */

        /*
        public void HandleTakeFallDamage(UnitController targetUnitController, int damage) {
            //Debug.Log("MessageLogClient.HandleTakeFallDamage()");
            Color textColor = Color.white;
            if (playerManagerClient.UnitController != null && targetUnitController == playerManagerClient.UnitController) {
                textColor = Color.red;
            }
            string combatMessage = $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>You take {damage} fall damage</color>";
            WriteCombatMessage(combatMessage);
        }
        */

        public void ClearLog() {
            //Debug.Log("MessageLogClient.ClearLog()");

            ClearGroupMessages();
            ClearGuildMessages();
            ClearPrivateMessages();
            ClearCombatMessages();
            ClearChatMessages();
            ClearSystemMessages();
        }

        private void ClearGroupMessages() {
            //Debug.Log("MessageLogClient.ClearGroupMessages()");
            OnClearGroupMessages();
        }

        private void ClearGuildMessages() {
            //Debug.Log("MessageLogClient.ClearGroupMessages()");
            OnClearGuildMessages();
        }

        private void ClearPrivateMessages() {
            //Debug.Log("MessageLogClient.ClearGroupMessages()");
            OnClearPrivateMessages();
        }

        private void ClearCombatMessages() {
            //Debug.Log("MessageLogClient.ClearCombatMessages()");
            OnClearCombatMessages();
        }

        private void ClearChatMessages() {
            //Debug.Log("MessageLogClient.ClearChatMessages()");
            OnClearGeneralMessages();
        }

        private void ClearSystemMessages() {
            //Debug.Log("MessageLogClient.ClearSystemMessages()");
            OnClearSystemMessages();
        }

        public void PrintWelcomeMessages() {
            //Debug.Log("MessageLogClient.PrintWelcomeMessages()");

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