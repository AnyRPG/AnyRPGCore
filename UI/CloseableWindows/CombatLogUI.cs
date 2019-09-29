using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatLogUI : WindowContentController {

    #region Singleton
    private static CombatLogUI instance;

    public static CombatLogUI MyInstance
    {
        get
        {
            if (instance == null) {
                instance = FindObjectOfType<CombatLogUI>();
            }

            return instance;
        }
    }

    #endregion

    [SerializeField]
    private GameObject textPrefab;

    [SerializeField]
    private GameObject chatArea;

    [SerializeField]
    private GameObject chatContentArea;

    [SerializeField]
    private Scrollbar chatScrollBar;

    [SerializeField]
    private HighlightButton chatHighlightButton;

    [SerializeField]
    private GameObject combatArea;

    [SerializeField]
    private GameObject combatContentArea;

    [SerializeField]
    private Scrollbar combatScrollBar;

    [SerializeField]
    private HighlightButton combatHighlightButton;

    [SerializeField]
    private GameObject systemArea;

    [SerializeField]
    private GameObject systemContentArea;

    [SerializeField]
    private Scrollbar systemScrollBar;

    [SerializeField]
    private Button systemButton;
    
    [SerializeField]
    private HighlightButton systemHighlightButton;

    private string welcomeString = "Welcome to AnyRPG Alpha v0.3a";

    // a list to hold the messages
    //private List<string> combatMessageList = new List<string>();

    private List<GameObject> combatMessageList = new List<GameObject>();

    // a list to hold the messages
    //private List<string> systemMessageList = new List<string>();

    private List<GameObject> systemMessageList = new List<GameObject>();

    // a list to hold the messages
    //private List<string> chatMessageList = new List<string>();

    private List<GameObject> chatMessageList = new List<GameObject>();

    protected bool startHasRun = false;
    protected bool eventReferencesInitialized = false;

    private List<QuestTrackerQuestScript> questScripts = new List<QuestTrackerQuestScript>();

    public override event System.Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };

    private void Start() {
        //Debug.Log("QuestTrackerUI.Start()");
        startHasRun = true;
        CreateEventReferences();
        ClearLog();
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

    public void WriteChatMessage(string newMessage) {
        float scrollBarValue = chatScrollBar.value;
        GameObject go = Instantiate(textPrefab, chatContentArea.transform);
        chatMessageList.Add(go);
        go.GetComponent<Text>().text = newMessage;
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContentArea.GetComponent<RectTransform>());

    }

    public void WriteCombatMessage(string newMessage) {
        //Debug.Log("CombatLogUI.WriteCombatMessage(" + newMessage + ")");

        float scrollBarValue = combatScrollBar.value;
        //Debug.Log("CombatLogUI.WriteCombatMessage(" + newMessage + "): scrollbarValue: " + combatScrollBar.value);

        GameObject go = Instantiate(textPrefab, combatContentArea.transform);
        combatMessageList.Add(go);
        go.GetComponent<Text>().text = newMessage;
        LayoutRebuilder.ForceRebuildLayoutImmediate(combatContentArea.GetComponent<RectTransform>());

    }

    public void WriteSystemMessage(string newMessage) {
        float scrollBarValue = systemScrollBar.value;
        GameObject go = Instantiate(textPrefab, systemContentArea.transform);
        systemMessageList.Add(go);
        go.GetComponent<Text>().text = newMessage;
        LayoutRebuilder.ForceRebuildLayoutImmediate(systemContentArea.GetComponent<RectTransform>());

    }

    private void OnEnable() {
        //Debug.Log("QuestTrackerUI.OnEnable()");
        CreateEventReferences();
    }

    private void CreateEventReferences() {
        ////Debug.Log("PlayerManager.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnTakeDamage += HandleTakeDamage;
        SystemEventManager.MyInstance.OnPlayerConnectionDespawn += ClearLog;
        eventReferencesInitialized = true;
    }

    private void CleanupEventReferences() {
        ////Debug.Log("PlayerManager.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnTakeDamage -= HandleTakeDamage;
        SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= ClearLog;
        eventReferencesInitialized = false;
    }

    public void OnDisable() {
        //Debug.Log("QuestTrackerUI.OnDisable()");
        CleanupEventReferences();
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
        //Debug.Log("QuestTrackerUI.ClearQuests()");

        WriteChatMessage(welcomeString);
        WriteCombatMessage(welcomeString);
        WriteSystemMessage(welcomeString);
    }

    public override void OnCloseWindow() {
        //Debug.Log("QuestTrackerUI.OnCloseWindow()");
        base.OnCloseWindow();
    }

    public override void OnOpenWindow() {
        //Debug.Log("QuestTrackerUI.OnOpenWindow()");
        ShowChatLog();
        OnOpenWindowHandler(this);
    }
}
