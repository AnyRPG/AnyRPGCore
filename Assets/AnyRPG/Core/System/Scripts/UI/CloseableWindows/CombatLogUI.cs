using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CombatLogUI : WindowContentController {

        //[SerializeField]
        //private GameObject textPrefab = null;

        [Header("Buttons")]

        [SerializeField]
        private HighlightButton chatHighlightButton = null;

        [SerializeField]
        private HighlightButton combatHighlightButton = null;

        [SerializeField]
        private HighlightButton systemHighlightButton = null;

        [Header("Areas")]

        [SerializeField]
        private GameObject chatArea = null;

        [SerializeField]
        private GameObject chatContentArea = null;

        [SerializeField]
        private RectTransform chatRectTranform = null;

        //[SerializeField]
        //private Scrollbar chatScrollBar = null;


        [SerializeField]
        private GameObject combatArea = null;

        [SerializeField]
        private GameObject combatContentArea = null;

        [SerializeField]
        private RectTransform combatRectTransform = null;

        //[SerializeField]
        //private Scrollbar combatScrollBar = null;


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

        private LogManager logManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("CombatLogUI.Awake()");
            base.Configure(systemGameManager);

            chatHighlightButton.Configure(systemGameManager);
            combatHighlightButton.Configure(systemGameManager);
            systemHighlightButton.Configure(systemGameManager);

            PopulateObjectPool();
            ClearLog();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            logManager = systemGameManager.LogManager;
        }

        private void ClearLog() {
            HandleClearChatMessages();
            HandleClearCombatMessages();
            HandleClearSystemMessages();
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
        

        public void HandleWriteChatMessage(string newMessage) {
            TextLogController textLogController = GetTextLogController(ref chatLogControllers, ref usedChatLogControllers);
            textLogController.InitializeTextLogController(newMessage);
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatRectTranform);

        }

        public void HandleWriteCombatMessage(string newMessage) {
            //Debug.Log("CombatLogUI.WriteCombatMessage(" + newMessage + ")");

            TextLogController textLogController = GetTextLogController(ref combatLogControllers, ref usedCombatLogControllers);
            textLogController.InitializeTextLogController(newMessage);

            LayoutRebuilder.ForceRebuildLayoutImmediate(combatRectTransform);

        }

        public void HandleWriteSystemMessage(string newMessage) {
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
            logManager.OnWriteChatMessage += HandleWriteChatMessage;
            logManager.OnWriteSystemMessage += HandleWriteSystemMessage;
            logManager.OnWriteCombatMessage += HandleWriteCombatMessage;
            logManager.OnClearChatMessages += HandleClearChatMessages;
            logManager.OnClearSystemMessages += HandleClearSystemMessages;
            logManager.OnClearCombatMessages += HandleClearCombatMessages;
            eventSubscriptionsInitialized = true;
        }

        protected override void CleanupEventSubscriptions() {
            ////Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            logManager.OnWriteChatMessage += HandleWriteChatMessage;
            logManager.OnWriteSystemMessage += HandleWriteSystemMessage;
            logManager.OnWriteCombatMessage += HandleWriteCombatMessage;
            logManager.OnClearChatMessages += HandleClearChatMessages;
            logManager.OnClearSystemMessages += HandleClearSystemMessages;
            logManager.OnClearCombatMessages += HandleClearCombatMessages;
            eventSubscriptionsInitialized = false;
        }

        public void HandleClearCombatMessages() {
            //Debug.Log("CombatLogUI.ClearCombatMessages()");
            foreach (TextLogController textLogController in combatLogControllers) {
                returnControllerToPool(textLogController, ref combatLogControllers, ref usedCombatLogControllers);
            }
        }

        public void HandleClearChatMessages() {
            //Debug.Log("CombatLogUI.ClearChatMessages()");
            foreach (TextLogController textLogController in chatLogControllers) {
                returnControllerToPool(textLogController, ref chatLogControllers, ref usedChatLogControllers);
            }
        }

        public void HandleClearSystemMessages() {
            //Debug.Log("CombatLogUI.ClearSystemMessages()");
            foreach (TextLogController textLogController in systemLogControllers) {
                returnControllerToPool(textLogController, ref systemLogControllers, ref usedSystemLogControllers);
            }
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("QuestTrackerUI.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("QuestTrackerUI.OnOpenWindow()");
            ShowChatLog();
        }
    }

}