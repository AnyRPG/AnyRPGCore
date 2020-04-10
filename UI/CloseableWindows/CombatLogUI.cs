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

        public static CombatLogUI MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CombatLogUI>();
                }

                return instance;
            }
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

        protected bool eventSubscriptionsInitialized = false;

        private List<TextLogController> combatLogControllers = new List<TextLogController>();

        private List<TextLogController> usedCombatLogControllers = new List<TextLogController>();

        private List<TextLogController> chatLogControllers = new List<TextLogController>();

        private List<TextLogController> usedChatLogControllers = new List<TextLogController>();

        private List<TextLogController> systemLogControllers = new List<TextLogController>();

        private List<TextLogController> usedSystemLogControllers = new List<TextLogController>();

        public override event System.Action<ICloseableWindowContents> OnOpenWindow = delegate { };

        public override void Awake() {
            //Debug.Log("CombatLogUI.Awake()");

            base.Awake();
            PopulateObjectPool();
            SetWelcomeString();
            ClearLog();
        }

        private void Start() {
            //Debug.Log("QuestTrackerUI.Start()");

            // do this last because it will print the chat messages and we don't want them to just get auto-cleared again
            CreateEventSubscriptions();
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
            if (SystemConfigurationManager.MyInstance != null) {
                if (SystemConfigurationManager.MyInstance.MyGameName != null && SystemConfigurationManager.MyInstance.MyGameName != string.Empty) {
                    completeWelcomeString += string.Format(" {0}", SystemConfigurationManager.MyInstance.MyGameName);
                }
                if (SystemConfigurationManager.MyInstance.MyGameVersion != null && SystemConfigurationManager.MyInstance.MyGameVersion != string.Empty) {
                    completeWelcomeString += string.Format(" {0}", SystemConfigurationManager.MyInstance.MyGameVersion);
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
            //float scrollBarValue = chatScrollBar.value;
            //GameObject go = Instantiate(textPrefab, chatContentArea.transform);
            TextLogController textLogController = GetTextLogController(ref chatLogControllers, ref usedChatLogControllers);
            //chatMessageList.Add(go);
            textLogController.InitializeTextLogController(newMessage);
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatRectTranform);

        }

        public void WriteCombatMessage(string newMessage) {
            //Debug.Log("CombatLogUI.WriteCombatMessage(" + newMessage + ")");

            //float scrollBarValue = combatScrollBar.value;
            //Debug.Log("CombatLogUI.WriteCombatMessage(" + newMessage + "): scrollbarValue: " + combatScrollBar.value);

            //GameObject go = Instantiate(textPrefab, combatContentArea.transform);
            //combatMessageList.Add(go);
            TextLogController textLogController = GetTextLogController(ref combatLogControllers, ref usedCombatLogControllers);
            textLogController.InitializeTextLogController(newMessage);

            LayoutRebuilder.ForceRebuildLayoutImmediate(combatRectTransform);

        }

        public void WriteSystemMessage(string newMessage) {
            //float scrollBarValue = systemScrollBar.value;
            //GameObject go = Instantiate(textPrefab, systemContentArea.transform);
            //systemMessageList.Add(go);
            TextLogController textLogController = GetTextLogController(ref systemLogControllers, ref usedSystemLogControllers);

            textLogController.InitializeTextLogController(newMessage);

            LayoutRebuilder.ForceRebuildLayoutImmediate(systemRectTransform);

        }

        private void OnEnable() {
            //Debug.Log("CombatLogUI.OnEnable()");
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            ////Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnTakeDamage += HandleTakeDamage;
            SystemEventManager.MyInstance.OnPlayerConnectionDespawn += ClearLog;
            SystemEventManager.MyInstance.OnPlayerConnectionSpawn += PrintWelcomeMessages;
            if (PlayerManager.MyInstance.MyPlayerConnectionSpawned == true) {
                PrintWelcomeMessages();
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            ////Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnTakeDamage -= HandleTakeDamage;
                SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= ClearLog;
                SystemEventManager.MyInstance.OnPlayerConnectionSpawn -= PrintWelcomeMessages;
            }
            eventSubscriptionsInitialized = false;
        }

        // although we usually use OnDisable, this is a static UI element, and should really keep it's references for the entire time the game is active
        // moved to OnDestroy() instead because it was already disabled before the player connection despawned.
        /*
        public void OnDisable() {
            //Debug.Log("QuestTrackerUI.OnDisable()");
            CleanupEventSubscriptions();
        }
        */

        public void OnDestroy() {
            //Debug.Log("QuestTrackerUI.OnDisable()");
            CleanupEventSubscriptions();
        }

        public void HandleTakeDamage(BaseCharacter source, CharacterUnit target, int damage, string abilityName) {
            Color textColor = Color.white;
            if (target == PlayerManager.MyInstance.MyCharacter.MyCharacterUnit) {
                textColor = Color.red;
            }
            string combatMessage = string.Format("<color=#{0}>{1} Takes {2} damage from {3}'s {4}</color>", ColorUtility.ToHtmlStringRGB(textColor), target.MyDisplayName, damage, source.MyCharacterName, abilityName);

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
            OnOpenWindow(this);
        }
    }

}