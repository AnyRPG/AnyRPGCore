using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LogManager : MonoBehaviour {

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

        public void Init() {
            //Debug.Log("CombatLogUI.Awake()");

            SetWelcomeString();
            ClearLog();
        }

        private void SetWelcomeString() {
            completeWelcomeString = welcomeString;
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
                if (SystemGameManager.Instance.SystemConfigurationManager.GameName != null && SystemGameManager.Instance.SystemConfigurationManager.GameName != string.Empty) {
                    completeWelcomeString += string.Format(" {0}", SystemGameManager.Instance.SystemConfigurationManager.GameName);
                }
                if (SystemGameManager.Instance.SystemConfigurationManager.GameVersion != null && SystemGameManager.Instance.SystemConfigurationManager.GameVersion != string.Empty) {
                    completeWelcomeString += string.Format(" {0}", SystemGameManager.Instance.SystemConfigurationManager.GameVersion);
                }
            }

        }

        public void WriteChatMessage(string newMessage) {
            OnWriteChatMessage(newMessage);
        }

        public void WriteCombatMessage(string newMessage) {
            //Debug.Log("LogManager.WriteCombatMessage(" + newMessage + ")");

            OnWriteCombatMessage(newMessage);
        }

        public void WriteSystemMessage(string newMessage) {
            OnWriteSystemMessage(newMessage);
        }

        private void CreateEventSubscriptions() {
            ////Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemGameManager.Instance.EventManager.OnTakeDamage += HandleTakeDamage;
            SystemEventManager.StartListening("OnPlayerConnectionSpawn", handlePlayerConnectionSpawn);
            SystemEventManager.StartListening("OnPlayerConnectionDespawn", handlePlayerConnectionDespawn);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            ////Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemGameManager.Instance?.EventManager != null) {
                SystemGameManager.Instance.EventManager.OnTakeDamage -= HandleTakeDamage;
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

        public void HandleTakeDamage(IAbilityCaster source, CharacterUnit target, int damage, string abilityName) {
            Color textColor = Color.white;
            if (SystemGameManager.Instance.PlayerManager.UnitController != null && target == SystemGameManager.Instance.PlayerManager.UnitController.CharacterUnit) {
                textColor = Color.red;
            }
            string combatMessage = string.Format("<color=#{0}>{1} Takes {2} damage from {3}'s {4}</color>", ColorUtility.ToHtmlStringRGB(textColor), target.DisplayName, damage, source.AbilityManager.Name, abilityName);

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

            WriteChatMessage(completeWelcomeString);
            WriteCombatMessage(completeWelcomeString);
            WriteSystemMessage(completeWelcomeString);

        }

    }

}