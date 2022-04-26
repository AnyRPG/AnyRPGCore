using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CombatLogUI : NavigableInterfaceElement {

        //[SerializeField]
        //private GameObject textPrefab = null;

        [Header("Buttons")]

        [SerializeField]
        protected HighlightButton chatHighlightButton = null;

        [SerializeField]
        protected HighlightButton combatHighlightButton = null;

        [SerializeField]
        protected HighlightButton systemHighlightButton = null;

        [SerializeField]
        protected HighlightButton sendButton = null;

        [Header("Areas")]

        [SerializeField]
        protected GameObject chatArea = null;

        [SerializeField]
        protected GameObject chatContentArea = null;

        [SerializeField]
        protected RectTransform chatRectTranform = null;

        //[SerializeField]
        //private Scrollbar chatScrollBar = null;


        [SerializeField]
        protected GameObject combatArea = null;

        [SerializeField]
        protected GameObject combatContentArea = null;

        [SerializeField]
        protected RectTransform combatRectTransform = null;

        //[SerializeField]
        //private Scrollbar combatScrollBar = null;


        [SerializeField]
        protected GameObject systemArea = null;

        [SerializeField]
        protected GameObject systemContentArea = null;

        [SerializeField]
        protected RectTransform systemRectTransform = null;

        [SerializeField]
        protected TMP_InputField textInput = null;

        //[SerializeField]
        //private Scrollbar systemScrollBar = null;

        //[SerializeField]
        //private Button systemButton = null;


        // a list to hold the messages
        //private List<string> combatMessageList = new List<string>();

        protected List<GameObject> combatMessageList = new List<GameObject>();

        // a list to hold the messages
        //private List<string> systemMessageList = new List<string>();

        protected List<GameObject> systemMessageList = new List<GameObject>();

        // a list to hold the messages
        //private List<string> chatMessageList = new List<string>();

        protected List<GameObject> chatMessageList = new List<GameObject>();

        protected List<TextLogController> combatLogControllers = new List<TextLogController>();

        protected List<TextLogController> usedCombatLogControllers = new List<TextLogController>();

        protected List<TextLogController> chatLogControllers = new List<TextLogController>();

        protected List<TextLogController> usedChatLogControllers = new List<TextLogController>();

        protected List<TextLogController> systemLogControllers = new List<TextLogController>();

        protected List<TextLogController> usedSystemLogControllers = new List<TextLogController>();

        protected Dictionary<string, ChatCommand> commandDictionary = new Dictionary<string, ChatCommand>();

        // game manager references
        protected LogManager logManager = null;
        protected SystemDataFactory systemDataFactory = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("CombatLogUI.Awake()");
            base.Configure(systemGameManager);

            chatHighlightButton.Configure(systemGameManager);
            combatHighlightButton.Configure(systemGameManager);
            systemHighlightButton.Configure(systemGameManager);

            PopulateObjectPool();
            ClearLog();

            textInput.onSubmit.AddListener(ProcessEnterKey);

            // populate the dictionary
            foreach (ChatCommand chatCommand in systemDataFactory.GetResourceList<ChatCommand>()) {
                commandDictionary.Add(chatCommand.DisplayName.ToLower().Replace(" ", ""), chatCommand);
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            logManager = systemGameManager.LogManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
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
            chatHighlightButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtons(chatHighlightButton);
            combatArea.SetActive(false);
            systemArea.SetActive(false);
            chatArea.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //chatScrollBar.value = 0;
        }

        public void ShowCombatLog() {
            combatHighlightButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtons(combatHighlightButton);
            systemArea.SetActive(false);
            chatArea.SetActive(false);
            combatArea.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(combatContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //combatScrollBar.value = 0;
        }

        public void ShowSystemLog() {
            systemHighlightButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtons(systemHighlightButton);
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

        protected override void ProcessCreateEventSubscriptions() {
            ////Debug.Log("PlayerManager.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();
            logManager.OnWriteChatMessage += HandleWriteChatMessage;
            logManager.OnWriteSystemMessage += HandleWriteSystemMessage;
            logManager.OnWriteCombatMessage += HandleWriteCombatMessage;
            logManager.OnClearChatMessages += HandleClearChatMessages;
            logManager.OnClearSystemMessages += HandleClearSystemMessages;
            logManager.OnClearCombatMessages += HandleClearCombatMessages;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            ////Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            logManager.OnWriteChatMessage += HandleWriteChatMessage;
            logManager.OnWriteSystemMessage += HandleWriteSystemMessage;
            logManager.OnWriteCombatMessage += HandleWriteCombatMessage;
            logManager.OnClearChatMessages += HandleClearChatMessages;
            logManager.OnClearSystemMessages += HandleClearSystemMessages;
            logManager.OnClearCombatMessages += HandleClearCombatMessages;
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

        /// <summary>
        /// disable hotkeys and movement while text input is active
        /// </summary>
        public void ActivateTextInput() {
            controlsManager.ActivateTextInput();
        }


        public void DeativateTextInput() {
            controlsManager.DeactivateTextInput();
        }

        /// <summary>
        /// respond to send key button clicked, sending message directly to parser
        /// </summary>
        public void ProcessSendKey() {
            ParseChatMessage(textInput.text);
        }

        /// <summary>
        /// respond to text input onSubmit() event and send to be parsed if escape key was not pressed
        /// </summary>
        /// <param name="ChatMessage"></param>
        public void ProcessEnterKey(string ChatMessage) {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKey(KeyCode.Escape)) {
                return;
            }
            ParseChatMessage(ChatMessage);
        }

        public void ParseChatMessage(string chatMessage) {
            textInput.text = "";
            if (chatMessage.StartsWith("/") == true ) {
                ParseChatCommand(chatMessage.Substring(1));
                return;
            }
            HandleWriteChatMessage(chatMessage);
        }

        private void ParseChatCommand(string commandText) {

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
                HandleWriteChatMessage("Unknown command : " + chatCommandString);
            }
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("QuestTrackerUI.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("QuestTrackerUI.OnOpenWindow()");
            base.ProcessOpenWindowNotification();
            //SetNavigationController();
            chatHighlightButton.HighlightBackground();
            ShowChatLog();
        }
    }

}