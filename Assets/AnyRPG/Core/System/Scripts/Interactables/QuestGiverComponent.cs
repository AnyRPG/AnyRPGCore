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
        private DialogManagerClient dialogManagerClient = null;
        private MessageLogClient logManager = null;
        private CurrencyConverter currencyConverter = null;
        private QuestGiverManagerClient questGiverManagerClient = null;

        public QuestGiverProps QuestGiverProps { get => interactableOptionProps as QuestGiverProps; }
        public override int PriorityValue { get => 1; }
        public InteractableOptionComponent InteractableOptionComponent { get => this; }

        public QuestGiverComponent(Interactable interactable, QuestGiverProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) {
                foreach (QuestNode questNode in QuestGiverProps.Quests) {
                    questNode.Quest.OnQuestBaseStatusUpdated += HandlePrerequisiteUpdates;
                }
            }

            // moved here from Init() monitor for breakage
            InitializeQuestGiver();
            //UpdateQuestStatus();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            
            dialogManagerClient = systemGameManager.DialogManagerClient;
            logManager = systemGameManager.MessageLogClient;
            currencyConverter = systemGameManager.CurrencyConverter;
            questGiverManagerClient = systemGameManager.QuestGiverManagerClient;
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
            HandlePrerequisiteUpdates(playerManager.UnitController);
        }

        public override bool CanInteract(UnitController sourceUnitController, bool processRangeCheck, bool passedRangeCheck, bool processNonCombatCheck, bool viaSwitch = false) {
            //Debug.Log($"{gameObject.name}.QuestGiver.CanInteract()");
            if (sourceUnitController.CharacterQuestLog.GetCompleteQuests(QuestGiverProps.Quests).Count + sourceUnitController.CharacterQuestLog.GetAvailableQuests(QuestGiverProps.Quests).Count == 0) {
                return false;
            }
            return base.CanInteract(sourceUnitController, processRangeCheck, passedRangeCheck, processNonCombatCheck);

        }

        public void InitializeQuestGiver() {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.InitializeQuestGiver()");
            if (questGiverInitialized == true) {
                return;
            }

            interactionPanelTitle = "Quests";
            foreach (QuestNode questNode in QuestGiverProps.Quests) {
                //Type questType = questNode.MyQuestTemplate.GetType();
                if (questNode.Quest == null) {
                    //Debug.Log($"{gameObject.name}.InitializeQuestGiver(): questnode.MyQuestTemplate is null!!!!");
                    return;
                }
                if (questNode.Quest.ResourceName == null) {
                    //Debug.Log($"{gameObject.name}.InitializeQuestGiver(): questnode.MyQuestTemplate.MyTitle is null!!!!");
                    return;
                }
                questNode.Quest = systemDataFactory.GetResource<Quest>(questNode.Quest.ResourceName);
            }
            questGiverInitialized = true;
        }

        public override void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.HandleCharacterSpawn()");

            base.HandlePlayerUnitSpawn(sourceUnitController);
            InitializeQuestGiver();
            foreach (QuestNode questNode in QuestGiverProps.Quests) {
                //if (questNode.MyQuest.TurnedIn != true) {
                    questNode.Quest.UpdatePrerequisites(sourceUnitController, false);
                //}
            }

            UpdateQuestStatus(sourceUnitController);
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

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.Interact()");
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.QuestGiverComponent.ClientInteraction({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            // this is running locally
            questGiverManagerClient.SetQuestGiver(this, componentIndex, choiceIndex, false);
            if (sourceUnitController.CharacterQuestLog.GetCompleteQuests(QuestGiverProps.Quests, true).Count + sourceUnitController.CharacterQuestLog.GetAvailableQuests(QuestGiverProps.Quests).Count > 1) {
                // there are multiple available or complete quests to choose from, so open the questgiver window to let the player choose
                interactionManager.OpenInteractionWindow(Interactable);
                return;
            } else if (sourceUnitController.CharacterQuestLog.GetAvailableQuests(QuestGiverProps.Quests).Count == 1 && sourceUnitController.CharacterQuestLog.GetCompleteQuests(QuestGiverProps.Quests).Count == 0) {
                // there is only one available quest and no complete quests
                if (sourceUnitController.CharacterQuestLog.GetAvailableQuests(QuestGiverProps.Quests)[0].HasOpeningDialog == true && sourceUnitController.CharacterQuestLog.GetAvailableQuests(QuestGiverProps.Quests)[0].OpeningDialog.TurnedIn(sourceUnitController) == false) {
                    dialogManagerClient.SetQuestDialog(sourceUnitController.CharacterQuestLog.GetAvailableQuests(QuestGiverProps.Quests)[0], Interactable, this, componentIndex, choiceIndex);
                    uIManager.dialogWindow.OpenWindow();
                    return;
                } else {
                    // do nothing will skip to below and open questlog to the available quest
                }
            }
            // we got here: we only have a single complete quest, or a single available quest with the opening dialog competed already
            if (!uIManager.questGiverWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                sourceUnitController.CharacterQuestLog.ShowQuestGiverDescription(sourceUnitController.CharacterQuestLog.GetAvailableQuests(QuestGiverProps.Quests).Union(sourceUnitController.CharacterQuestLog.GetCompleteQuests(QuestGiverProps.Quests)).ToList()[0], this);
                return;
            }

        }

        public override void StopInteract() {
            //Debug.Log($"{gameObject.name}.QuestGiver.StopInteract()");
            base.StopInteract();
            //vendorUI.ClearPages();
            uIManager.questGiverWindow.CloseWindow();
        }

        public void UpdateQuestStatus(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.UpdateQuestStatus()");
            if (playerManager.UnitController == null) {
                //Debug.Log($"{gameObject.name}.QuestGiver.UpdateQuestStatus(): player has no character");
                return;
            }
            if (interactable == null) {
                //Debug.Log($"{gameObject.name}:QuestGiver.UpdateQuestStatus() Nameplate is null");
                return;
            }

            string indicatorType = GetIndicatorType(sourceUnitController);

            if (indicatorType == string.Empty) {
                interactable.ProcessHideQuestIndicator();
            } else {
                interactable.ProcessShowQuestIndicator(indicatorType, this);
            }
        }

        public string GetIndicatorType(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.QuestGiver.GetIndicatorType()");

            if (playerManager.UnitController == null) {
                //Debug.Log($"{gameObject.name}.QuestGiver.GetIndicatorType(): playerManager.UnitController is null. returning empty");
                return string.Empty;
            }

            float relationValue = interactable.PerformFactionCheck(playerManager.UnitController);
            if (CanInteract(playerManager.UnitController, false, false, true) == false) {
                //Debug.Log($"{gameObject.name}.QuestGiver.GetIndicatorType(): Cannot interact.  Return empty string");
                return string.Empty;
            }

            string indicatorType = string.Empty;
            int completeCount = 0;
            int inProgressCount = 0;
            int availableCount = 0;
            //Debug.Log($"{gameObject.name}QuestGiver.GetIndicatorType(): quests.length: " + quests.Length);
            foreach (QuestNode questNode in QuestGiverProps.Quests) {
                if (questNode != null && questNode.Quest != null) {
                    if (playerManager.UnitController.CharacterQuestLog.HasQuest(questNode.Quest.ResourceName)) {
                        if (questNode.Quest.IsComplete(sourceUnitController) && !questNode.Quest.TurnedIn(sourceUnitController) && questNode.EndQuest) {
                            //Debug.Log($"{gameObject.name}: There is a complete quest to turn in.  Incrementing inProgressCount.");
                            completeCount++;
                        } else if (!questNode.Quest.IsComplete(sourceUnitController) && questNode.EndQuest) {
                            //Debug.Log($"{gameObject.name}: A quest is in progress.  Incrementing inProgressCount.");
                            inProgressCount++;
                        } else {
                            //Debug.Log($"{gameObject.name}: This quest must have been turned in already or we are not responsible for ending it.  doing nothing.");
                        }
                    } else if ((questNode.Quest.TurnedIn(sourceUnitController) == false || (questNode.Quest.RepeatableQuest == true && playerManager.UnitController.CharacterQuestLog.HasQuest(questNode.Quest.ResourceName) == false)) && questNode.StartQuest && questNode.Quest.PrerequisitesMet(playerManager.UnitController) == true) {
                        availableCount++;
                        //Debug.Log($"{gameObject.name}: The quest is not in the log and hasn't been turned in yet.  Incrementing available count");
                    }
                } else {
                    if (questNode == null) {
                        //Debug.Log($"{gameObject.name}: The quest node was null");
                    }
                    if (questNode.Quest == null) {
                        //Debug.Log($"{gameObject.name}: The questNode.MyQuest was null");
                    }
                }
            }
            //Debug.Log($"{gameObject.name}: complete: " + completeCount.ToString() + "; available: " + availableCount.ToString() + "; inProgress: " + inProgressCount.ToString() + ";");
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
            //Debug.Log($"{interactable.gameObject.name}.QuestGiver.SetIndicatorText({indicatorType})");

            if (indicatorType == "complete") {
                text.text = "?";
                text.color = Color.yellow;
            }/* else if (indicatorType == "inProgress") {
                text.text = "?";
                text.color = Color.gray;
            }*/ else if (indicatorType == "available") {
                text.text = "!";
                text.color = Color.yellow;
            } else {
                text.text = string.Empty;
                text.color = new Color32(0, 0, 0, 0);
            }
        }

        public override bool HasMiniMapText() {
            //Debug.Log($"{interactable.gameObject.name}.QuestGiverComponent.HasMiniMapText()");

            return true;
        }

        public override bool HasMainMapText() {
            //Debug.Log($"{gameObject.name}.QuestGiverComponent.HasMiniMapText()");
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
            SetIndicatorText(GetIndicatorType(playerManager.UnitController), text);
            return true;
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.GetCurrentOptionCount()");
            if (interactable.CombatOnly) {
                return 0;
            }
            return sourceUnitController.CharacterQuestLog.GetCompleteQuests(QuestGiverProps.Quests).Count + sourceUnitController.CharacterQuestLog.GetAvailableQuests(QuestGiverProps.Quests).Count;
        }

        public void HandleAcceptQuest() {
            // do nothing for now - used in questStartItem
        }

        public void HandleCompleteQuest() {
            // do nothing for now - used in questStartItem
        }

        public override void HandlePrerequisiteUpdates(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".QuestGiver.HandlePrerequisiteUpdates()");
            // testing put this before the base since base calls minimap update
            UpdateQuestStatus(sourceUnitController);
            base.HandlePrerequisiteUpdates(sourceUnitController);
            //UpdateQuestStatus();
        }

        public bool EndsQuest(string questName) {
            foreach (QuestNode questNode in QuestGiverProps.Quests) {
                if (SystemDataUtility.MatchResource(questNode.Quest.ResourceName, questName)) {
                    if (questNode.EndQuest == true) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            return false;
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}

        public void AcceptQuest(UnitController sourceUnitController, Quest quest) {
            sourceUnitController.CharacterQuestLog.AcceptQuest(quest);

            NotifyOnConfirmAction(sourceUnitController);
        }

        public override void CleanupScriptableObjects() {
            base.CleanupScriptableObjects();

            foreach (QuestNode questNode in QuestGiverProps.Quests) {
                questNode.Quest.OnQuestBaseStatusUpdated -= HandlePrerequisiteUpdates;
            }
        }

        public void CompleteQuest(UnitController sourceUnitController, Quest quest, QuestRewardChoices questRewardChoices) {
            //Debug.Log("QuestGiverUI.CompleteQuest()");
            if (!quest.IsComplete(sourceUnitController)) {
                Debug.LogWarning("QuestGiverManager.CompleteQuest(): currentQuest is not complete, exiting!");
                return;
            }

            quest.CompleteQuest(sourceUnitController, questRewardChoices);
        }

        public void RequestAcceptQuest(UnitController unitController, Quest currentQuest) {
            questGiverManagerClient.RequestAcceptQuest(playerManager.UnitController, currentQuest);
        }

        public void RequestCompleteQuest(UnitController unitController, Quest currentQuest, QuestRewardChoices questRewardChoices) {
            questGiverManagerClient.RequestCompleteQuest(playerManager.UnitController, currentQuest, questRewardChoices);
        }
    }

}