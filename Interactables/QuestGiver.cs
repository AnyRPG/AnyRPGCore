using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestGiver : InteractableOption, IQuestGiver {

    public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    [SerializeField]
    private QuestNode[] quests;

    [SerializeField]
    private INamePlateUnit namePlateUnit;

    private bool questGiverInitialized = false;

    public QuestNode[] MyQuests { get => quests; }

    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyQuestGiverInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyQuestGiverInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyQuestGiverNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyQuestGiverNamePlateImage : base.MyNamePlateImage); }

    protected override void Awake() {
        //Debug.Log(gameObject.name + ".QuestGiver.Awake()");
        base.Awake();
        namePlateUnit = GetComponent<INamePlateUnit>();
    }

    protected override void Start() {
        //Debug.Log(gameObject.name + ".QuestGiver.Start()");
        base.Start();

        InitializeQuestGiver();

        CreateEventReferences();

        // this could run after the character spawn.  check it just in case
        UpdateQuestStatus();
    }

    private void CreateEventReferences() {
        //Debug.Log(gameObject.name + ".QuestGiver.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
        if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
            //Debug.Log(gameObject.name + ".QuestGiver.Awake(): player unit is already spawned.");
            HandlePlayerUnitSpawn();
        }

        namePlateUnit.OnInitializeNamePlate += HandlePrerequisiteUpdates;
        eventReferencesInitialized = true;
    }

    public override void CleanupEventReferences() {
        //Debug.Log("QuestGiver.CleanupEventReferences()");
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
        }
        if (namePlateUnit != null) {
            namePlateUnit.OnInitializeNamePlate -= HandlePrerequisiteUpdates;
        }
        if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.questGiverWindow != null && PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents != null) {
            PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindowHandler -= InitWindow;
            PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnCloseWindowHandler -= CloseWindowHandler;
        }
        eventReferencesInitialized = false;
    }

    public override void OnDisable() {
        //Debug.Log("UnitSpawnNode.OnDisable(): stopping any outstanding coroutines");
        CleanupEventReferences();
    }

    public override bool CanInteract(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".QuestGiver.CanInteract()");
        if (Quest.GetCompleteQuests(MyQuests).Count + Quest.GetAvailableQuests(MyQuests).Count == 0) {
            return false;
        }
        return base.CanInteract(source);

    }

    public void InitializeQuestGiver() {
        //Debug.Log(gameObject.name + ".QuestGiver.InitializeQuestGiver()");
        if (questGiverInitialized == true) {
            return;
        }

        interactionPanelTitle = "Quests";
        foreach (QuestNode questNode in quests) {
            //Type questType = questNode.MyQuestTemplate.GetType();
            if (questNode.MyQuest == null) {
                Debug.Log(gameObject.name + ".InitializeQuestGiver(): questnode.MyQuestTemplate is null!!!!");
            }
            if (questNode.MyQuest.MyName == null) {
                Debug.Log(gameObject.name + ".InitializeQuestGiver(): questnode.MyQuestTemplate.MyTitle is null!!!!");
            } else {
                //Debug.Log(gameObject.name + ".InitializeQuestGiver(): Adding watches on " + questNode.MyQuestTemplate.MyTitle);
            }
            questNode.MyQuest = SystemQuestManager.MyInstance.GetResource(questNode.MyQuest.MyName);
        }
        questGiverInitialized = true;
    }

    public void HandlePlayerUnitSpawn() {
        //Debug.Log(gameObject.name + ".QuestGiver.HandleCharacterSpawn()");
        InitializeQuestGiver();

        HandlePrerequisiteUpdates();
    }

    public void InitWindow(ICloseableWindowContents questGiverUI) {
        //Debug.Log(gameObject.name + ".QuestGiver.InitWindow()");
        (questGiverUI as QuestGiverUI).ShowQuests(this);
        PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindowHandler -= InitWindow;
    }

    public override bool Interact(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".QuestGiver.Interact()");
        base.Interact(source);
        if (Quest.GetCompleteQuests(MyQuests, true).Count + Quest.GetAvailableQuests(MyQuests).Count > 1) {
            interactable.OpenInteractionWindow();
            return true;
        } else if (Quest.GetAvailableQuests(MyQuests).Count == 1 && Quest.GetCompleteQuests(MyQuests).Count == 0) {
            if (Quest.GetAvailableQuests(MyQuests)[0].MyHasOpeningDialog == true && SystemDialogManager.MyInstance.GetResource(Quest.GetAvailableQuests(MyQuests)[0].MyName).TurnedIn == false) {
                (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).Setup(Quest.GetAvailableQuests(MyQuests)[0], interactable);
                return true;
            } else {
                // do nothing will skip to below and open questlog to the available quest
                /*
                interactable.OpenInteractionWindow();
                return true;
                */
            }
        }
        // we got here: we only have a single complete quest, or a single available quest with the opening dialog competed already
        if (!PopupWindowManager.MyInstance.questGiverWindow.IsOpen) {
            //Debug.Log(source + " interacting with " + gameObject.name);
            PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindowHandler += InitWindow;
            PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnCloseWindowHandler += CloseWindowHandler;
            QuestGiverUI.MyInstance.MyInteractable = this.interactable;
            PopupWindowManager.MyInstance.questGiverWindow.OpenWindow();
            return true;
        }
        return false;
    }

    public void CloseWindowHandler(ICloseableWindowContents questGiverUI) {
        PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindowHandler -= InitWindow;
        PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnCloseWindowHandler -= CloseWindowHandler;
    }

    public override void StopInteract() {
        //Debug.Log(gameObject.name + ".QuestGiver.StopInteract()");
        base.StopInteract();
        //vendorUI.ClearPages();
        PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
        PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindowHandler -= InitWindow;
        PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnCloseWindowHandler -= CloseWindowHandler;
    }

    public void UpdateQuestStatus() {
        //Debug.Log(gameObject.name + ".QuestGiver.UpdateQuestStatus()");
        if (PlayerManager.MyInstance.MyCharacter == null) {
            //Debug.Log(gameObject.name + ".QuestGiver.UpdateQuestStatus(): player has no character");
            return;
        }
        if (namePlateUnit.MyNamePlate == null) {
            //Debug.Log(gameObject.name + ":QuestGiver.UpdateQuestStatus() Nameplate is null");
            return;
        }

        string indicatorType = GetIndicatorType();

        if (indicatorType == string.Empty) {
            namePlateUnit.MyNamePlate.MyQuestIndicatorBackground.SetActive(false);
        } else {
            namePlateUnit.MyNamePlate.MyQuestIndicatorBackground.SetActive(true);
            //Debug.Log(gameObject.name + ":QuestGiver.UpdateQuestStatus() Indicator is active.  Setting to: " + indicatorType);
            SetIndicatorText(indicatorType, namePlateUnit.MyNamePlate.MyQuestIndicator);
        }
        //Debug.Log(gameObject.name + ":QuestGiver.UpdateQuestStatus() About to fire MiniMapUpdateHandler");
    }

    public string GetIndicatorType() {
        //Debug.Log(gameObject.name + ".QuestGiver.GetIndicatorType()");

        if (PlayerManager.MyInstance.MyCharacter == null) {
            //Debug.Log(gameObject.name + ".QuestGiver.GetIndicatorType(): PlayerManager.MyInstance.MyCharacter is null. returning empty");
            return string.Empty;
        }

        if (CanInteract(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit) == false) {
            //Debug.Log(gameObject.name + ".QuestGiver.GetIndicatorType(): Cannot interact.  Return empty string");
            return string.Empty;
        }

        string indicatorType = string.Empty;
        int completeCount = 0;
        int inProgressCount = 0;
        int availableCount = 0;
        //Debug.Log(gameObject.name + "QuestGiver.GetIndicatorType(): quests.length: " + quests.Length);
        foreach (QuestNode questNode in quests) {
            if (questNode != null && questNode.MyQuest != null) {
                if (QuestLog.MyInstance.HasQuest(questNode.MyQuest.MyName)) {
                    if (questNode.MyQuest.IsComplete && !questNode.MyQuest.TurnedIn && questNode.MyEndQuest) {
                        //Debug.Log(gameObject.name + ": There is a complete quest to turn in.  Incrementing inProgressCount.");
                        completeCount++;
                    } else if (!questNode.MyQuest.IsComplete && questNode.MyEndQuest) {
                        //Debug.Log(gameObject.name + ": A quest is in progress.  Incrementing inProgressCount.");
                        inProgressCount++;
                    } else {
                        //Debug.Log(gameObject.name + ": This quest must have been turned in already or we are not responsible for ending it.  doing nothing.");
                    }
                } else if (!questNode.MyQuest.TurnedIn && questNode.MyStartQuest && questNode.MyQuest.MyPrerequisitesMet == true) {
                    availableCount++;
                    //Debug.Log(gameObject.name + ": The quest is not in the log and hasn't been turned in yet.  Incrementing available count");
                }
            } else {
                if (questNode == null) {
                    Debug.Log(gameObject.name + ": The quest node was null");
                }
                if (questNode.MyQuest == null) {
                    Debug.Log(gameObject.name + ": The questNode.MyQuest was null");
                }
            }
        }
        //Debug.Log(gameObject.name + ": complete: " + completeCount.ToString() + "; available: " + availableCount.ToString() + "; inProgress: " + inProgressCount.ToString() + ";");
        if (completeCount > 0) {
            indicatorType = "complete";
        } else if (availableCount > 0) {
            indicatorType = "available";
        } else if (inProgressCount > 0) {
            indicatorType = "inProgress";
        }

        return indicatorType;
    }

    private void SetIndicatorText(string indicatorType, Text text) {
        //Debug.Log(gameObject.name + ".QuestGiver.SetIndicatorText(" + indicatorType + ")");
        if (indicatorType == "complete") {
            text.text = "?";
            text.color = Color.yellow;
        } else if (indicatorType == "inProgress") {
            text.text = "?";
            text.color = Color.gray;
        } else if (indicatorType == "available") {
            text.text = "!";
            text.color = Color.yellow;
        } else {
            text.text = string.Empty;
            text.color = new Color32(0, 0, 0, 0);
        }
    }

    public override bool HasMiniMapText() {
        //Debug.Log(gameObject.name + ".QuestGiver.HasMiniMapText()");
        return true;
    }

    public override bool SetMiniMapText(Text text) {
        //Debug.Log(gameObject.name + ".QuestGiver.SetMiniMapText()");
        if (!base.SetMiniMapText(text)) {
            text.text = "";
            text.color = new Color32(0, 0, 0, 0);
            return false;
        }
        SetIndicatorText(GetIndicatorType(), text);
        return true;
    }

    public override int GetCurrentOptionCount() {
        return Quest.GetCompleteQuests(MyQuests).Count + Quest.GetAvailableQuests(MyQuests).Count;
    }

    public void HandleAcceptQuest() {
        // do nothing for now - used in questStartItem
    }

    public void HandleCompleteQuest() {
        // do nothing for now - used in questStartItem
    }

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        UpdateQuestStatus();
        MiniMapStatusUpdateHandler(this);
    }

    public bool EndsQuest(string questName) {
        foreach (QuestNode questNode in quests) {
            if (SystemResourceManager.MatchResource(questNode.MyQuest.MyName, questName)) {
                if (questNode.MyEndQuest == true) {
                    return true;
                } else {
                    return false;
                }
            }
        }
        return false;
    }
}
