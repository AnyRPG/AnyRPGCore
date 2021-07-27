using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CombatLogUI : WindowContentController {

        #region Singleton
        private static CombatLogUI instance;

        public static CombatLogUI Instance {
            get {
                return instance;
            }
        }

        private void Awake() {
            instance = this;
        }
        #endregion

        //[SerializeField]
        //private GameObject textPrefab = null;

        [SerializeField]
        private GameObject chatArea = null;

        [SerializeField]
        private GameObject chatContentArea = null;

        [SerializeField]
        private RectTransform chatRectTranform = null;

        //[SerializeField]
        //private Scrollbar chatScrollBar = null;

        [SerializeField]
        private HighlightButton chatHighlightButton = null;

        [SerializeField]
        private GameObject combatArea = null;

        [SerializeField]
        private GameObject combatContentArea = null;

        [SerializeField]
        private RectTransform combatRectTransform = null;

        //[SerializeField]
        //private Scrollbar combatScrollBar = null;

        [SerializeField]
        private HighlightButton combatHighlightButton = null;

        [SerializeField]
        private GameObject systemArea = null;

        [SerializeField]
        private GameObject systemContentArea = null;

        [SerializeField]
        private RectTransform systemRectTransform = null;

        //[SerializeField]
        //private Scrollbar systemScrollBar = null;

        //[SerializeField]
        //private Button systemButton = null;

        [SerializeField]
        private HighlightButton systemHighlightButton = null;

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

        private List<TextLogController> combatLogControllers = new List<TextLogController>();

        private List<TextLogController> usedCombatLogControllers = new List<TextLogController>();

        private List<TextLogController> chatLogControllers = new List<TextLogController>();

        private List<TextLogController> usedChatLogControllers = new List<TextLogController>();

        private List<TextLogController> systemLogControllers = new List<TextLogController>();

        private List<TextLogController> usedSystemLogControllers = new List<TextLogController>();

        public override void Init() {
            //Debug.Log("CombatLogUI.Awake()");

            PopulateObjectPool();
            SetWelcomeString();
            ClearLog();

            base.Init();
        }

        public void PopulateObjectPool() {
            //Debug.Log("CombatLogUI.PopulateObjectPool()");
            foreach (Transform child in combatContentArea.transform) {
                //Debug.Log("CombatLogUI.PopulateObjectPool(): found a child");
                TextLogController textLogController = child.GetComponent<TextLogController>();
                if (textLogController != null) {
                    //Debug.Log("CombatLogUI.PopulateObjectPool(): found a child, adding to combat list");
                    combatLogControllers.Add(textLogController);
                }
            }
            foreach (Transform child in chatContentArea.transform) {
                TextLogController textLogController = child.GetComponent<TextLogController>();
                if (textLogController != null) {
                    chatLogControllers.Add(textLogController);
                }
            }
            foreach (Transform child in systemContentArea.transform) {
                TextLogController textLogController = child.GetComponent<TextLogController>();
                if (textLogController != null) {
                    systemLogControllers.Add(textLogController);
                }
            }
        }


        private void SetWelcomeString() {
            completeWelcomeString = welcomeString;
            if (SystemConfigurationManager.Instance != null) {
                if (SystemConfigurationManager.Instance.GameName != null && SystemConfigurationManager.Instance.GameName != string.Empty) {
                    completeWelcomeString += string.Format(" {0}", SystemConfigurationManager.Instance.GameName);
                }
                if (SystemConfigurationManager.Instance.GameVersion != null && SystemConfigurationManager.Instance.GameVersion != string.Empty) {
                    completeWelcomeString += string.Format(" {0}", SystemConfigurationManager.Instance.GameVersion);
                }
            }

        }

        public void ShowChatLog() {
            chatHighlightButton.Select();
            combatHighlightButton.DeSelect();
            systemHighlightButton.DeSelect();
            combatArea.SetActive(false);
            systemArea.SetActive(false);
            chatArea.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //chatScrollBar.value = 0;
        }

        public void ShowCombatLog() {
            chatHighlightButton.DeSelect();
            combatHighlightButton.Select();
            systemHighlightButton.DeSelect();
            systemArea.SetActive(false);
            chatArea.SetActive(false);
            combatArea.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(combatContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //combatScrollBar.value = 0;
        }

        public void ShowSystemLog() {
            chatHighlightButton.DeSelect();
            systemHighlightButton.Select();
            combatHighlightButton.DeSelect();
            chatArea.SetActive(false);
            combatArea.SetActive(false);
            systemArea.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(systemContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //systemScrollBar.value = 0;
        }

        public TextLogController GetTextLogController(ref List<TextLogController> freeTextLogControllers, ref List<TextLogController> usedTextLogControllers) {
            TextLogController returnValue = null;
            if (freeTextLogControllers.Count > 0) {
                // we are getting an unused one
                returnValue = freeTextLogControllers[0];
                usedTextLogControllers.Add(freeTextLogControllers[0]);
                freeTextLogControllers.RemoveAt(0);
            } else {
                // we are recycling a used one
                if (usedTextLogControllers.Count > 0) {
                    returnValue = usedTextLogControllers[0];
                    usedTextLogControllers.RemoveAt(0);
                    usedTextLogControllers.Add(returnValue);
                }
            }
            return returnValue;
        }

        
        public void returnControllerToPool(TextLogController textLogController, ref List<TextLogController> freeTextLogControllers, ref List<TextLogController> usedTextLogControllers) {
            if (usedTextLogControllers.Contains(textLogController)) {
                usedTextLogControllers.Remove(textLogController);
                freeTextLogControllers.Add(textLogController);
            }
            textLogController.gameObject.SetActive(false);
        }
        

        public void WriteChatMessage(string newMessage) {
            TextLogController textLogController = GetTextLogController(ref chatLogControllers, ref usedChatLogControllers);
            textLogController.InitializeTextLogController(newMessage);
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatRectTranform);

        }

        public void WriteCombatMessage(string newMessage) {
            //Debug.Log("CombatLogUI.WriteCombatMessage(" + newMessage + ")");

            TextLogController textLogController = GetTextLogController(ref combatLogControllers, ref usedCombatLogControllers);
            textLogController.InitializeTextLogController(newMessage);

            LayoutRebuilder.ForceRebuildLayoutImmediate(combatRectTransform);

        }

        public void WriteSystemMessage(string newMessage) {
            TextLogController textLogController = GetTextLogController(ref systemLogControllers, ref usedSystemLogControllers);

            textLogController.InitializeTextLogController(newMessage);

            LayoutRebuilder.ForceRebuildLayoutImmediate(systemRectTransform);

        }

        protected override void CreateEventSubscriptions() {
            ////Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            SystemEventManager.Instance.OnTakeDamage += HandleTakeDamage;
            SystemEventManager.StartListening("OnPlayerConnectionSpawn", handlePlayerConnectionSpawn);
            SystemEventManager.StartListening("OnPlayerConnectionDespawn", handlePlayerConnectionDespawn);
            if (PlayerManager.Instance.PlayerConnectionSpawned == true) {
                PrintWelcomeMessages();
            }
            eventSubscriptionsInitialized = true;
        }

        protected override void CleanupEventSubscriptions() {
            ////Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            if (SystemEventManager.Instance != null) {
                SystemEventManager.Instance.OnTakeDamage -= HandleTakeDamage;
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

        // although we usually use OnDisable, this is a static UI element, and should really keep it's references for the entire time the game is active
        // moved to OnDestroy() instead because it was already disabled before the player connection despawned.
        /*
        public void OnDisable() {
            //Debug.Log("QuestTrackerUI.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }
        */

        public void HandleTakeDamage(IAbilityCaster source, CharacterUnit target, int damage, string abilityName) {
            Color textColor = Color.white;
            if (PlayerManager.Instance.UnitController != null && target == PlayerManager.Instance.UnitController.CharacterUnit) {
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
            foreach (TextLogController textLogController in combatLogControllers) {
                returnControllerToPool(textLogController, ref combatLogControllers, ref usedCombatLogControllers);
            }
        }

        private void ClearChatMessages() {
            //Debug.Log("CombatLogUI.ClearChatMessages()");
            foreach (TextLogController textLogController in chatLogControllers) {
                returnControllerToPool(textLogController, ref chatLogControllers, ref usedChatLogControllers);
            }
        }

        private void ClearSystemMessages() {
            //Debug.Log("CombatLogUI.ClearSystemMessages()");
            foreach (TextLogController textLogController in systemLogControllers) {
                returnControllerToPool(textLogController, ref systemLogControllers, ref usedSystemLogControllers);
            }
        }

        public void PrintWelcomeMessages() {
            //Debug.Log("CombatLogUI.PrintWelcomeMessages()");

            WriteChatMessage(completeWelcomeString);
            WriteCombatMessage(completeWelcomeString);
            WriteSystemMessage(completeWelcomeString);

        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("QuestTrackerUI.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("QuestTrackerUI.OnOpenWindow()");
            ShowChatLog();
        }
    }

}