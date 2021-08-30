using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class QuestGiverComponent : InteractableOptionComponent, IQuestGiver {

        private bool questGiverInitialized = false;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private QuestLog questLog = null;

        public QuestGiverProps Props { get => interactableOptionProps as QuestGiverProps; }

        public QuestGiverComponent(Interactable interactable, QuestGiverProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            foreach (QuestNode questNode in Props.Quests) {
                questNode.MyQuest.OnQuestStatusUpdated += HandlePrerequisiteUpdates;
            }

            // moved here from Init() monitor for breakage
            InitializeQuestGiver();
            UpdateQuestStatus();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
            questLog = systemGameManager.QuestLog;
        }

        /*
        public override void Init() {
            InitializeQuestGiver();
            base.Init();
            // this could run after the character spawn.  check it just in case
            UpdateQuestStatus();
        }
        */

        public override void ProcessStatusIndicatorSourceInit() {
            base.ProcessStatusIndicatorSourceInit();
            HandlePrerequisiteUpdates();
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("QuestGiver.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
        }

        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false, float factionValue = 0f, bool processNonCombatCheck = true) {
            //Debug.Log(gameObject.name + ".QuestGiver.CanInteract()");
            if (questLog.GetCompleteQuests(Props.Quests).Count + questLog.GetAvailableQuests(Props.Quests).Count == 0) {
                return false;
            }
            return base.CanInteract(processRangeCheck, passedRangeCheck, factionValue, processNonCombatCheck);

        }

        public void InitializeQuestGiver() {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.InitializeQuestGiver()");
            if (questGiverInitialized == true) {
                return;
            }

            interactableOptionProps.InteractionPanelTitle = "Quests";
            foreach (QuestNode questNode in Props.Quests) {
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
                questNode.MyQuest = systemDataFactory.GetResource<Quest>(questNode.MyQuest.DisplayName);
            }
            questGiverInitialized = true;
        }

        public override void HandlePlayerUnitSpawn() {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.HandleCharacterSpawn()");

            base.HandlePlayerUnitSpawn();
            InitializeQuestGiver();
            foreach (QuestNode questNode in Props.Quests) {
                //if (questNode.MyQuest.TurnedIn != true) {
                    questNode.MyQuest.UpdatePrerequisites(false);
                //}
            }

            UpdateQuestStatus();
            CallMiniMapStatusUpdateHandler();

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

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.Interact()");
            base.Interact(source, optionIndex);
            if (questLog.GetCompleteQuests(Props.Quests, true).Count + questLog.GetAvailableQuests(Props.Quests).Count > 1) {
                interactable.OpenInteractionWindow();
                return true;
            } else if (questLog.GetAvailableQuests(Props.Quests).Count == 1 && questLog.GetCompleteQuests(Props.Quests).Count == 0) {
                if (questLog.GetAvailableQuests(Props.Quests)[0].HasOpeningDialog == true && questLog.GetAvailableQuests(Props.Quests)[0].OpeningDialog.TurnedIn == false) {
                    (uIManager.dialogWindow.CloseableWindowContents as DialogPanelController).Setup(questLog.GetAvailableQuests(Props.Quests)[0], interactable);
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
            if (!uIManager.questGiverWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                uIManager.questGiverWindow.OpenWindow();
                questLog.ShowQuestGiverDescription(questLog.GetAvailableQuests(Props.Quests).Union(questLog.GetCompleteQuests(Props.Quests)).ToList()[0], this);
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            //Debug.Log(gameObject.name + ".QuestGiver.StopInteract()");
            base.StopInteract();
            //vendorUI.ClearPages();
            uIManager.questGiverWindow.CloseWindow();
        }

        public void UpdateQuestStatus() {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.UpdateQuestStatus()");
            if (playerManager.MyCharacter == null) {
                //Debug.Log(gameObject.name + ".QuestGiver.UpdateQuestStatus(): player has no character");
                return;
            }
            if (interactable == null) {
                //Debug.Log(gameObject.name + ":QuestGiver.UpdateQuestStatus() Nameplate is null");
                return;
            }

            string indicatorType = GetIndicatorType();

            if (indicatorType == string.Empty) {
                interactable.ProcessHideQuestIndicator();
            } else {
                interactable.ProcessShowQuestIndicator(indicatorType, this);
            }
        }

        public string GetIndicatorType() {
            //Debug.Log(gameObject.name + ".QuestGiver.GetIndicatorType()");

            if (playerManager.MyCharacter == null) {
                //Debug.Log(gameObject.name + ".QuestGiver.GetIndicatorType(): playerManager.MyCharacter is null. returning empty");
                return string.Empty;
            }

            float relationValue = interactable.PerformFactionCheck(playerManager.MyCharacter);
            if (CanInteract(false, false, relationValue) == false) {
                //Debug.Log(gameObject.name + ".QuestGiver.GetIndicatorType(): Cannot interact.  Return empty string");
                return string.Empty;
            }

            string indicatorType = string.Empty;
            int completeCount = 0;
            int inProgressCount = 0;
            int availableCount = 0;
            //Debug.Log(gameObject.name + "QuestGiver.GetIndicatorType(): quests.length: " + quests.Length);
            foreach (QuestNode questNode in Props.Quests) {
                if (questNode != null && questNode.MyQuest != null) {
                    if (questLog.HasQuest(questNode.MyQuest.DisplayName)) {
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

        public void SetIndicatorText(string indicatorType, TextMeshProUGUI text) {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.SetIndicatorText(" + indicatorType + ")");
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
            //Debug.Log(gameObject.name + ".QuestGiverComponent.HasMiniMapText()");
            return true;
        }

        public override bool HasMainMapText() {
            //Debug.Log(gameObject.name + ".QuestGiverComponent.HasMiniMapText()");
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.SetMiniMapText()");
            if (!base.SetMiniMapText(text)) {
                //Debug.Log(interactable.gameObject.name + ".QuestGiver.SetMiniMapText(): hiding text");
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            SetIndicatorText(GetIndicatorType(), text);
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.GetCurrentOptionCount()");
            if (interactable.CombatOnly) {
                return 0;
            }
            return questLog.GetCompleteQuests(Props.Quests).Count + questLog.GetAvailableQuests(Props.Quests).Count;
        }

        public void HandleAcceptQuest() {
            // do nothing for now - used in questStartItem
        }

        public void HandleCompleteQuest() {
            // do nothing for now - used in questStartItem
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.HandlePrerequisiteUpdates()");
            // testing put this before the base since base calls minimap update
            UpdateQuestStatus();
            base.HandlePrerequisiteUpdates();
            //UpdateQuestStatus();
        }

        public bool EndsQuest(string questName) {
            foreach (QuestNode questNode in Props.Quests) {
                if (SystemDataFactory.MatchResource(questNode.MyQuest.DisplayName, questName)) {
                    if (questNode.MyEndQuest == true) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            return false;
        }

        public override void CleanupScriptableObjects() {
            base.CleanupScriptableObjects();
            foreach (QuestNode questNode in Props.Quests) {
                questNode.MyQuest.OnQuestStatusUpdated -= HandlePrerequisiteUpdates;
            }
        }
    }

}