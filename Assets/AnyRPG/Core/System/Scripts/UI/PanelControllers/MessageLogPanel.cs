using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MessageLogPanel : NavigableInterfaceElement {

        //[SerializeField]
        //private GameObject textPrefab = null;

        [Header("Buttons")]

        [SerializeField]
        protected HighlightButton generalHighlightButton = null;

        [SerializeField]
        protected HighlightButton groupHighlightButton = null;

        [SerializeField]
        protected HighlightButton guildHighlightButton = null;

        [SerializeField]
        protected HighlightButton privateHighlightButton = null;

        [SerializeField]
        protected HighlightButton combatHighlightButton = null;

        [SerializeField]
        protected HighlightButton systemHighlightButton = null;

        /*
        [SerializeField]
        protected HighlightButton sendButton = null;
        */

        [Header("Areas")]

        [SerializeField]
        protected GameObject generalArea = null;

        /*
        [SerializeField]
        protected GameObject generalContentArea = null;

        [SerializeField]
        protected RectTransform generalRectTranform = null;
        */

        [SerializeField]
        private TextMeshProUGUI generalText = null;


        [SerializeField]
        protected GameObject groupArea = null;

        /*
        [SerializeField]
        protected GameObject groupContentArea = null;

        [SerializeField]
        protected RectTransform groupRectTranform = null;
        */

        [SerializeField]
        private TextMeshProUGUI groupText = null;

        [SerializeField]
        protected GameObject guildArea = null;

        /*
        [SerializeField]
        protected GameObject guildContentArea = null;

        [SerializeField]
        protected RectTransform guildRectTranform = null;
        */

        [SerializeField]
        private TextMeshProUGUI guildText = null;


        [SerializeField]
        protected GameObject privateArea = null;

        /*
        [SerializeField]
        protected GameObject privateContentArea = null;

        [SerializeField]
        protected RectTransform privateRectTranform = null;
        */

        [SerializeField]
        private TextMeshProUGUI privateText = null;


        [SerializeField]
        protected GameObject combatArea = null;

        /*
        [SerializeField]
        protected GameObject combatContentArea = null;

        [SerializeField]
        protected RectTransform combatRectTransform = null;
        */

        [SerializeField]
        private TextMeshProUGUI combatText = null;


        [SerializeField]
        protected GameObject systemArea = null;

        /*
        [SerializeField]
        protected GameObject systemContentArea = null;

        [SerializeField]
        protected RectTransform systemRectTransform = null;
        */

        [SerializeField]
        private TextMeshProUGUI systemText = null;

        [SerializeField]
        protected TMP_InputField textInput = null;

        [SerializeField]
        protected UINavigationController logButtonsNavigationController = null;

        // log content
        protected string generalLog = string.Empty;
        protected string groupLog = string.Empty;
        protected string guildLog = string.Empty;
        protected string privateLog = string.Empty;
        protected string combatLog = string.Empty;
        protected string systemLog = string.Empty;

        // game manager references
        protected MessageLogClient messageLogClient = null;
        protected ChatCommandManager chatCommandManager = null;
        protected UIManager uiManager = null;
        protected NetworkManagerClient networkManagerClient = null;
        protected ContextMenuService contextMenuService = null;
        protected LevelManager levelManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("MessageLogPanel.Awake()");
            base.Configure(systemGameManager);

            /*
            generalHighlightButton.Configure(systemGameManager);
            combatHighlightButton.Configure(systemGameManager);
            systemHighlightButton.Configure(systemGameManager);
            */

            ClearLog();
            textInput.onSubmit.AddListener(ProcessEnterKey);
            DisableNetworkButtons();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            messageLogClient = systemGameManager.MessageLogClient;
            chatCommandManager = systemGameManager.ChatCommandManager;
            uiManager = systemGameManager.UIManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            contextMenuService = systemGameManager.ContextMenuService;
            levelManager = systemGameManager.LevelManager;
        }

        private void ClearLog() {
            HandleClearGeneralMessages();
            HandleClearGroupMessages();
            HandleClearGuildMessages();
            HandleClearPrivateMessages();
            HandleClearCombatMessages();
            HandleClearSystemMessages();
        }

        public void ShowGeneralLog() {
            generalHighlightButton.HighlightBackground();
            logButtonsNavigationController.UnHightlightButtonBackgrounds(generalHighlightButton);
            logButtonsNavigationController.SetCurrentButton(generalHighlightButton);
            groupArea.SetActive(false);
            guildArea.SetActive(false);
            privateArea.SetActive(false);
            combatArea.SetActive(false);
            systemArea.SetActive(false);
            generalArea.SetActive(true);
            
            //LayoutRebuilder.ForceRebuildLayoutImmediate(generalContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //chatScrollBar.value = 0;
        }

        public void ShowGroupLog() {
            groupHighlightButton.HighlightBackground();
            logButtonsNavigationController.UnHightlightButtonBackgrounds(groupHighlightButton);
            logButtonsNavigationController.SetCurrentButton(groupHighlightButton);
            privateArea.SetActive(false);
            combatArea.SetActive(false);
            systemArea.SetActive(false);
            generalArea.SetActive(false);
            groupArea.SetActive(true);
            guildArea.SetActive(false);

            //LayoutRebuilder.ForceRebuildLayoutImmediate(generalContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //chatScrollBar.value = 0;
        }

        public void ShowGuildLog() {
            guildHighlightButton.HighlightBackground();
            logButtonsNavigationController.UnHightlightButtonBackgrounds(guildHighlightButton);
            logButtonsNavigationController.SetCurrentButton(guildHighlightButton);
            privateArea.SetActive(false);
            combatArea.SetActive(false);
            systemArea.SetActive(false);
            generalArea.SetActive(false);
            groupArea.SetActive(false);
            guildArea.SetActive(true);

            //LayoutRebuilder.ForceRebuildLayoutImmediate(generalContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //chatScrollBar.value = 0;
        }

        public void ShowPrivateLog() {
            privateHighlightButton.HighlightBackground();
            logButtonsNavigationController.UnHightlightButtonBackgrounds(privateHighlightButton);
            logButtonsNavigationController.SetCurrentButton(privateHighlightButton);
            combatArea.SetActive(false);
            systemArea.SetActive(false);
            generalArea.SetActive(false);
            groupArea.SetActive(false);
            guildArea.SetActive(false);
            privateArea.SetActive(true);

            //LayoutRebuilder.ForceRebuildLayoutImmediate(generalContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //chatScrollBar.value = 0;
        }

        public void ShowCombatLog() {
            combatHighlightButton.HighlightBackground();
            logButtonsNavigationController.UnHightlightButtonBackgrounds(combatHighlightButton);
            logButtonsNavigationController.SetCurrentButton(combatHighlightButton);
            groupArea.SetActive(false);
            guildArea.SetActive(false);
            privateArea.SetActive(false);
            systemArea.SetActive(false);
            generalArea.SetActive(false);
            combatArea.SetActive(true);
            //LayoutRebuilder.ForceRebuildLayoutImmediate(combatContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //combatScrollBar.value = 0;
        }

        public void ShowSystemLog() {
            systemHighlightButton.HighlightBackground();
            logButtonsNavigationController.UnHightlightButtonBackgrounds(systemHighlightButton);
            logButtonsNavigationController.SetCurrentButton(systemHighlightButton);
            groupArea.SetActive(false);
            guildArea.SetActive(false);
            privateArea.SetActive(false);
            generalArea.SetActive(false);
            combatArea.SetActive(false);
            systemArea.SetActive(true);
            //LayoutRebuilder.ForceRebuildLayoutImmediate(systemContentArea.GetComponent<RectTransform>());

            // set to the bottom so user doesn't have to scroll all the way down on existing long logs
            //systemScrollBar.value = 0;
        }

        public void HandleWriteGeneralMessage(string newMessage) {
            //Debug.Log($"MessageLogPanel.HandleWriteGeneralMessage({newMessage})");

            generalLog += newMessage + "\n";
            generalText.text = generalLog;

            //LayoutRebuilder.ForceRebuildLayoutImmediate(generalRectTranform);
        }

        public void HandleWriteGroupMessage(string newMessage) {
            //Debug.Log($"MessageLogPanel.WriteGroupMessage({newMessage})");
            groupLog += newMessage + "\n";
            groupText.text = groupLog;
            //LayoutRebuilder.ForceRebuildLayoutImmediate(groupRectTranform);
        }

        public void HandleWriteGuildMessage(string newMessage) {
            //Debug.Log($"MessageLogPanel.WriteGroupMessage({newMessage})");
            guildLog += newMessage + "\n";
            guildText.text = guildLog;
            //LayoutRebuilder.ForceRebuildLayoutImmediate(groupRectTranform);
        }

        public void HandleWritePrivateMessage(string newMessage) {
            //Debug.Log($"MessageLogPanel.WritePrivateMessage({newMessage})");
            privateLog += newMessage + "\n";
            privateText.text = privateLog;
            //LayoutRebuilder.ForceRebuildLayoutImmediate(privateRectTranform);
        }

        public void HandleWriteCombatMessage(string newMessage) {
            //Debug.Log($"MessageLogPanel.WriteCombatMessage({newMessage})");

            combatLog += newMessage + "\n";
            combatText.text = combatLog;

            //LayoutRebuilder.ForceRebuildLayoutImmediate(combatRectTransform);

        }

        public void HandleWriteSystemMessage(string newMessage) {
            //Debug.Log($"MessageLogPanel.WriteSystemMessage({newMessage})");

            systemLog += newMessage + "\n";
            systemText.text = systemLog;

            //LayoutRebuilder.ForceRebuildLayoutImmediate(systemRectTransform);
        }

        protected override void ProcessCreateEventSubscriptions() {
            ////Debug.Log("PlayerManager.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();
            messageLogClient.OnWriteGeneralMessage += HandleWriteGeneralMessage;
            messageLogClient.OnWriteGroupMessage += HandleWriteGroupMessage;
            messageLogClient.OnWriteGuildMessage += HandleWriteGuildMessage;
            messageLogClient.OnWritePrivateMessage += HandleWritePrivateMessage;
            messageLogClient.OnWriteSystemMessage += HandleWriteSystemMessage;
            messageLogClient.OnWriteCombatMessage += HandleWriteCombatMessage;
            messageLogClient.OnClearGeneralMessages += HandleClearGeneralMessages;
            messageLogClient.OnClearGroupMessages += HandleClearGroupMessages;
            messageLogClient.OnClearGuildMessages += HandleClearGuildMessages;
            messageLogClient.OnClearPrivateMessages += HandleClearPrivateMessages;
            messageLogClient.OnClearSystemMessages += HandleClearSystemMessages;
            messageLogClient.OnClearCombatMessages += HandleClearCombatMessages;
            uiManager.OnBeginChatCommand += HandleBeginChatCommand;
            contextMenuService.OnBeginPrivateMessage += HandleBeginPrivateMessage;
            messageLogClient.OnBeginPrivateMessage += HandleBeginPrivateMessage;
            networkManagerClient.OnClientConnectionStarted += HandleClientConnectionStarted;
            networkManagerClient.OnClientConnectionStopped += HandleClientConnectionStopped;
            //levelManager.OnExitToMainMenu += HandleExitToMainMenu;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            ////Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            messageLogClient.OnWriteGeneralMessage -= HandleWriteGeneralMessage;
            messageLogClient.OnWriteGroupMessage -= HandleWriteGroupMessage;
            messageLogClient.OnWriteGuildMessage -= HandleWriteGuildMessage;
            messageLogClient.OnWritePrivateMessage -= HandleWritePrivateMessage;
            messageLogClient.OnWriteSystemMessage -= HandleWriteSystemMessage;
            messageLogClient.OnWriteCombatMessage -= HandleWriteCombatMessage;
            messageLogClient.OnClearGeneralMessages -= HandleClearGeneralMessages;
            messageLogClient.OnClearGroupMessages -= HandleClearGroupMessages;
            messageLogClient.OnClearGuildMessages -= HandleClearGuildMessages;
            messageLogClient.OnClearPrivateMessages -= HandleClearPrivateMessages;
            messageLogClient.OnClearSystemMessages -= HandleClearSystemMessages;
            messageLogClient.OnClearCombatMessages -= HandleClearCombatMessages;
            uiManager.OnBeginChatCommand -= HandleBeginChatCommand;
            contextMenuService.OnBeginPrivateMessage -= HandleBeginPrivateMessage;
            messageLogClient.OnBeginPrivateMessage -= HandleBeginPrivateMessage;
            networkManagerClient.OnClientConnectionStarted -= HandleClientConnectionStarted;
            networkManagerClient.OnClientConnectionStopped -= HandleClientConnectionStopped;
            //levelManager.OnExitToMainMenu -= HandleExitToMainMenu;
        }

        public void HandleExitToMainMenu() {
            ClearLog();
        }

        private void HandleClientConnectionStarted() {
            EnableNetworkButtons();
        }

        private void EnableNetworkButtons() {
            //Debug.Log("MessageLogPanel.EnableNetworkButtons()");

            groupHighlightButton.gameObject.SetActive(true);
            guildHighlightButton.gameObject.SetActive(true);
            privateHighlightButton.gameObject.SetActive(true);
        }

        private void HandleClientConnectionStopped() {
            DisableNetworkButtons();
        }

        private void DisableNetworkButtons() {
            //Debug.Log("MessageLogPanel.DisableNetworkButtons()");

            groupHighlightButton.gameObject.SetActive(false);
            guildHighlightButton.gameObject.SetActive(false);
            privateHighlightButton.gameObject.SetActive(false);
        }

        private void HandleBeginPrivateMessage(string messageText) {
            //Debug.Log($"MessageLogPanel.HandleBeginPrivateMessage({messageText})");

            HandleBeginChatCommand($"/{messageText}");
        }

        public void HandleBeginChatCommand() {
            HandleBeginChatCommand("/");
        }

        public void HandleBeginChatCommand(string messageText) {
            //Debug.Log($"MessageLogPanel.HandleBeginChatCommand({messageText})");

            // disable input of other keys while entering text
            ActivateTextInput();

            // focus the text input field
            textInput.ActivateInputField();

            // set the text to the slash that was just entered and move to end of the line so
            // the text isn't selected and the '/' doesn't get overwritten with the next keystroke
            textInput.text = messageText;
            textInput.caretPosition = messageText.Length;
            textInput.selectionAnchorPosition = messageText.Length;
            textInput.selectionFocusPosition = messageText.Length;
        }

        public void HandleClearGeneralMessages() {
            //Debug.Log("MessageLogPanel.ClearGeneralMessages()");
            generalLog = string.Empty;
            generalText.text = generalLog;
        }

        public void HandleClearGroupMessages() {
            //Debug.Log("MessageLogPanel.ClearGroupMessages()");
            groupLog = string.Empty;
            groupText.text = generalLog;
        }

        public void HandleClearGuildMessages() {
            //Debug.Log("MessageLogPanel.HandleClearGuildMessages()");
            guildLog = string.Empty;
            guildText.text = generalLog;
        }

        public void HandleClearPrivateMessages() {
            //Debug.Log("MessageLogPanel.ClearPrivateMessages()");
            privateLog = string.Empty;
            privateText.text = generalLog;
        }

        public void HandleClearCombatMessages() {
            //Debug.Log("MessageLogPanel.ClearCombatMessages()");

            combatLog = string.Empty;
            combatText.text = generalLog;
        }

        public void HandleClearSystemMessages() {
            //Debug.Log("MessageLogPanel.ClearSystemMessages()");

            systemLog = string.Empty;
            systemText.text = generalLog;
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
            /*
            if (chatMessage.StartsWith("/") == true ) {
                chatCommandManager.ParseChatCommand(chatMessage.Substring(1));
                return;
            }

            if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.SendSceneChatMessage(chatMessage);
            } else {
                HandleWriteChatMessage(chatMessage);
                logManager.WriteChatMessage(chatMessage);
            }
            */
            messageLogClient.RequestChatMessageClient(chatMessage);
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("MessageLogPanel.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("MessageLogPanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();

            //generalHighlightButton.HighlightBackground();
            ShowGeneralLog();
        }

        private void OnEnable() {
            // there isn't really a better way to do this for now since the ui element isn't directly opened or closed
            logButtonsNavigationController.UpdateNavigationList();
            logButtonsNavigationController.HighlightCurrentNavigableElement();
        }
    }

}