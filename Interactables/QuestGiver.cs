using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class QuestGiver : InteractableOption, IQuestGiver {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private QuestGiverConfig questGiverConfig = new QuestGiverConfig();

        [Header("QuestGiver")]

        [SerializeField]
        private List<string> questGiverProfileNames = new List<string>();

        private List<QuestGiverProfile> questGiverProfiles = new List<QuestGiverProfile>();

        private List<QuestNode> quests = new List<QuestNode>();

        private bool questGiverInitialized = false;

        public List<QuestNode> MyQuests { get => quests; }

        public override Sprite Icon { get => questGiverConfig.Icon; }
        public override Sprite NamePlateImage { get => questGiverConfig.NamePlateImage; }

        public QuestGiver(Interactable interactable, QuestGiverConfig interactableConfig) : base(interactable) {
            this.questGiverConfig = interactableConfig;
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".QuestGiver.Start()");
            InitializeQuestGiver();

            base.Start();

            CreateEventSubscriptions();

            AddUnitProfileSettings();

            // this could run after the character spawn.  check it just in case
            UpdateQuestStatus();
        }

        public void AddUnitProfileSettings() {
            CharacterUnit characterUnit = GetComponent<CharacterUnit>();
            if (characterUnit != null && characterUnit.BaseCharacter != null && characterUnit.BaseCharacter.UnitProfile != null) {
                if (characterUnit.BaseCharacter.UnitProfile.Quests != null) {
                    foreach (QuestNode quest in characterUnit.BaseCharacter.UnitProfile.Quests) {
                        quest.MyQuest.OnQuestStatusUpdated += HandlePrerequisiteUpdates;
                        quests.Add(quest);
                    }
                }
            }

            HandlePrerequisiteUpdates();
        }


        public override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".QuestGiver.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            if (namePlateUnit != null) {
                namePlateUnit.NamePlateController.OnInitializeNamePlate += HandlePrerequisiteUpdates;
            }
        }

        public void CleanupWindowEventSubscriptions() {
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.questGiverWindow != null && PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents != null) {
                PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindow -= InitWindow;
                PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnCloseWindow -= CloseWindowHandler;
            }
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("QuestGiver.CleanupEventSubscriptions()");
            if (namePlateUnit != null) {
                namePlateUnit.NamePlateController.OnInitializeNamePlate -= HandlePrerequisiteUpdates;
            }
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override void OnDisable() {
            //Debug.Log("UnitSpawnNode.OnDisable(): stopping any outstanding coroutines");
            CleanupEventSubscriptions();
        }

        public override bool CanInteract() {
            //Debug.Log(gameObject.name + ".QuestGiver.CanInteract()");
            if (Quest.GetCompleteQuests(MyQuests).Count + Quest.GetAvailableQuests(MyQuests).Count == 0) {
                return false;
            }
            return base.CanInteract();

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
                    //Debug.Log(gameObject.name + ".InitializeQuestGiver(): questnode.MyQuestTemplate is null!!!!");
                    return;
                }
                if (questNode.MyQuest.DisplayName == null) {
                    //Debug.Log(gameObject.name + ".InitializeQuestGiver(): questnode.MyQuestTemplate.MyTitle is null!!!!");
                    return;
                } else {
                    //Debug.Log(gameObject.name + ".InitializeQuestGiver(): Adding watches on " + questNode.MyQuestTemplate.MyTitle);
                }
                questNode.MyQuest = SystemQuestManager.MyInstance.GetResource(questNode.MyQuest.DisplayName);
            }
            questGiverInitialized = true;
        }

        public override void HandlePlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".QuestGiver.HandleCharacterSpawn()");
            base.HandlePlayerUnitSpawn();
            InitializeQuestGiver();
            foreach (QuestNode questNode in quests) {
                //if (questNode.MyQuest.TurnedIn != true) {
                    questNode.MyQuest.UpdatePrerequisites(false);
                //}
            }

            UpdateQuestStatus();
            MiniMapStatusUpdateHandler(this);

            /*
            bool statusChanged = false;
            foreach (QuestNode questNode in quests) {
                if (questNode.MyQuest.TurnedIn != true) {
                    if (questNode.MyQuest.MyPrerequisitesMet) {
                        statusChanged = true;
                    }
                }
            }
            if (statusChanged) {
                HandlePrerequisiteUpdates();
            }
            */
        }

        public void InitWindow(ICloseableWindowContents questGiverUI) {
            //Debug.Log(gameObject.name + ".QuestGiver.InitWindow()");
            (questGiverUI as QuestGiverUI).ShowQuests(this);
            PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindow -= InitWindow;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".QuestGiver.Interact()");
            base.Interact(source);
            if (Quest.GetCompleteQuests(MyQuests, true).Count + Quest.GetAvailableQuests(MyQuests).Count > 1) {
                interactable.OpenInteractionWindow();
                return true;
            } else if (Quest.GetAvailableQuests(MyQuests).Count == 1 && Quest.GetCompleteQuests(MyQuests).Count == 0) {
                if (Quest.GetAvailableQuests(MyQuests)[0].MyHasOpeningDialog == true && Quest.GetAvailableQuests(MyQuests)[0].MyOpeningDialog.TurnedIn == false) {
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
                PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindow += InitWindow;
                PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnCloseWindow += CloseWindowHandler;
                QuestGiverUI.MyInstance.MyInteractable = this.interactable;
                PopupWindowManager.MyInstance.questGiverWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public void CloseWindowHandler(ICloseableWindowContents questGiverUI) {
            CleanupWindowEventSubscriptions();
        }

        public override void StopInteract() {
            //Debug.Log(gameObject.name + ".QuestGiver.StopInteract()");
            base.StopInteract();
            //vendorUI.ClearPages();
            PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
            CleanupWindowEventSubscriptions();
        }

        public void UpdateQuestStatus() {
            //Debug.Log(gameObject.name + ".QuestGiver.UpdateQuestStatus()");
            if (PlayerManager.MyInstance == null) {
                Debug.LogError(gameObject.name + ": PlayerManager not found.  Is the GameManager in the scene?");
                return;
            }
            if (PlayerManager.MyInstance.MyCharacter == null) {
                //Debug.Log(gameObject.name + ".QuestGiver.UpdateQuestStatus(): player has no character");
                return;
            }
            if (namePlateUnit.NamePlateController.NamePlate == null) {
                //Debug.Log(gameObject.name + ":QuestGiver.UpdateQuestStatus() Nameplate is null");
                return;
            }

            string indicatorType = GetIndicatorType();

            if (indicatorType == string.Empty) {
                namePlateUnit.NamePlateController.NamePlate.MyQuestIndicatorBackground.SetActive(false);
            } else {
                namePlateUnit.NamePlateController.NamePlate.MyQuestIndicatorBackground.SetActive(true);
                //Debug.Log(gameObject.name + ":QuestGiver.UpdateQuestStatus() Indicator is active.  Setting to: " + indicatorType);
                SetIndicatorText(indicatorType, namePlateUnit.NamePlateController.NamePlate.MyQuestIndicator);
            }
            //Debug.Log(gameObject.name + ":QuestGiver.UpdateQuestStatus() About to fire MiniMapUpdateHandler");
        }

        public string GetIndicatorType() {
            //Debug.Log(gameObject.name + ".QuestGiver.GetIndicatorType()");

            if (PlayerManager.MyInstance.MyCharacter == null) {
                //Debug.Log(gameObject.name + ".QuestGiver.GetIndicatorType(): PlayerManager.MyInstance.MyCharacter is null. returning empty");
                return string.Empty;
            }

            if (CanInteract() == false) {
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
                    if (QuestLog.MyInstance.HasQuest(questNode.MyQuest.DisplayName)) {
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
                        //Debug.Log(gameObject.name + ": The quest node was null");
                    }
                    if (questNode.MyQuest == null) {
                        //Debug.Log(gameObject.name + ": The questNode.MyQuest was null");
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

        private void SetIndicatorText(string indicatorType, TextMeshProUGUI text) {
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

        public override bool SetMiniMapText(TextMeshProUGUI text) {
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
            //Debug.Log(gameObject.name + ".QuestGiver.HandlePrerequisiteUpdates()");

            base.HandlePrerequisiteUpdates();
            UpdateQuestStatus();
            MiniMapStatusUpdateHandler(this);
        }


        public bool EndsQuest(string questName) {
            foreach (QuestNode questNode in quests) {
                if (SystemResourceManager.MatchResource(questNode.MyQuest.DisplayName, questName)) {
                    if (questNode.MyEndQuest == true) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            return false;
        }

        public override void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".QuestGiver.SetupScriptableObjects()");
            
            base.SetupScriptableObjects();

            questGiverProfiles = new List<QuestGiverProfile>();
            if (questGiverProfileNames != null) {
                foreach (string questGiverProfileName in questGiverProfileNames) {
                    QuestGiverProfile tmpQuestGiverProfile = SystemQuestGiverProfileManager.MyInstance.GetResource(questGiverProfileName);
                    if (tmpQuestGiverProfile != null) {
                        questGiverProfiles.Add(tmpQuestGiverProfile);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find QuestGiverProfile : " + questGiverProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            foreach (QuestGiverProfile questGiverProfile in questGiverProfiles) {
                if (questGiverProfile != null && questGiverProfile.MyQuests != null) {
                    foreach (QuestNode questNode in questGiverProfile.MyQuests) {
                        //Debug.Log(gameObject.name + ".SetupScriptableObjects(): Adding quest: " + questNode.MyQuest.MyName);
                        quests.Add(questNode);
                        questNode.MyQuest.OnQuestStatusUpdated += HandlePrerequisiteUpdates;
                    }
                }
            }

        }

        public override void CleanupScriptableObjects() {
            base.CleanupScriptableObjects();
            foreach (QuestNode questNode in quests) {
                questNode.MyQuest.OnQuestStatusUpdated -= HandlePrerequisiteUpdates;
            }
        }
    }

}