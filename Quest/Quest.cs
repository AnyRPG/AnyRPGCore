using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    //[System.Serializable]
    [CreateAssetMenu(fileName = "New Quest", menuName = "AnyRPG/Quests/Quest")]
    public class Quest : DescribableResource {

        [SerializeField]
        private bool isAchievement = false;

        [SerializeField]
        private bool repeatableQuest = false;

        // a dialog that is not a requirement to interact with the questgiver or see the quest, but must be completed to start it
        //[SerializeField]
        //private Dialog openingDialog;

        // replaces the above setting to avoid issues with scriptableObjects
        [SerializeField]
        private bool hasOpeningDialog;

        private Dialog openingDialog;

        private bool markedComplete = false;

        /// <summary>
        /// The level the quest becomes available from questgivers
        /// </summary>
        /// 
        /*
        [SerializeField]
        private int availableLevel = 1;
        */

        /// <summary>
        /// The level that is considered appropriate for the quest.  Used to calculate xp reduction
        /// </summary>
        [SerializeField]
        private int experienceLevel = 1;

        [SerializeField]
        private bool dynamicLevel = true;

        // levels above the normal level for this mob
        [SerializeField]
        private int extraLevels;


        /// <summary>
        /// The amount of experience gained for completing this quest at an appropriate level
        /// </summary>
        [SerializeField]
        private int experienceReward = 50;

        [SerializeField]
        private int maxItemRewards = 0;

        [SerializeField]
        private List<string> itemRewardNames = new List<string>();

        [SerializeField]
        private List<string> itemRewardList = new List<string>();

        private List<Item> realItemRewardList = new List<Item>();

        [SerializeField]
        private int maxFactionRewards = 0;

        [SerializeField]
        private List<FactionNode> factionRewards = new List<FactionNode>();

        [SerializeField]
        private int maxAbilityRewards = 0;

        [SerializeField]
        private List<string> abilityRewardNames = new List<string>();

        [SerializeField]
        private List<string> abilityRewardList = new List<string>();

        private List<BaseAbility> realAbilityRewardList = new List<BaseAbility>();

        [SerializeField]
        private int maxSkillRewards = 0;

        [SerializeField]
        private List<string> skillRewardNames = new List<string>();

        [SerializeField]
        private List<string> skillRewardList = new List<string>();

        private List<Skill> realSkillRewardList = new List<Skill>();

        /// <summary>
        /// The maximum number of copies of this quest you can have in your quest log at once.
        /// This is here because the quest system in the future should allow you to have multiple copies of a quest from different players
        /// </summary>
        //[SerializeField]
        //private int maxInstances = 1;

        // since we will have more than a few types in the future, this will be a generic array of objectives
        [SerializeField]
        private CollectObjective[] collectObjectives;

        [SerializeField]
        private KillObjective[] killObjectives;

        [SerializeField]
        private TradeSkillObjective[] tradeSkillObjectives;

        [SerializeField]
        private AbilityObjective[] abilityObjectives;

        [SerializeField]
        private UseInteractableObjective[] useInteractableObjectives;

        [SerializeField]
        private QuestQuestObjective[] questQuestObjectives;

        [SerializeField]
        private DialogObjective[] dialogObjectives;

        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        /// <summary>
        /// Whether or not to give the items to the questgiver when you turn in a quest.  If false, you keep the items in your bag.
        /// </summary>
        [SerializeField]
        private bool turnInItems;

        // the player can complete the quest without having the quest in the questlog
        [SerializeField]
        private bool allowRawComplete = false;

        private Quest questTemplate = null;

        /// <summary>
        /// Track whether this quest has been turned in
        /// </summary>
        private bool turnedIn = false;

        public CollectObjective[] MyCollectObjectives { get => collectObjectives; set => collectObjectives = value; }
        public KillObjective[] MyKillObjectives { get => killObjectives; set => killObjectives = value; }
        public TradeSkillObjective[] MyTradeSkillObjectives { get => tradeSkillObjectives; set => tradeSkillObjectives = value; }
        public AbilityObjective[] MyAbilityObjectives { get => abilityObjectives; set => abilityObjectives = value; }
        public UseInteractableObjective[] MyUseInteractableObjectives { get => useInteractableObjectives; set => useInteractableObjectives = value; }
        public QuestQuestObjective[] MyQuestQuestObjectives { get => questQuestObjectives; set => questQuestObjectives = value; }
        public DialogObjective[] MyDialogObjectives { get => dialogObjectives; set => dialogObjectives = value; }

        public bool IsComplete {
            get {
                //Debug.Log("Quest.IsComplete: " + MyTitle);
                if (MyAllowRawComplete == true) {
                    return true;
                }
                foreach (QuestObjective o in collectObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }
                foreach (QuestObjective o in killObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }
                foreach (QuestObjective o in tradeSkillObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }
                foreach (QuestObjective o in abilityObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }
                foreach (QuestObjective o in useInteractableObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }

                foreach (QuestQuestObjective o in questQuestObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }

                foreach (DialogObjective o in dialogObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }

                //Debug.Log("Quest: " + MyTitle + ": no objectives for this quest:  were not complete, about to return true");
                return true;
            }
        }

        public bool TurnedIn {
            get {
                return turnedIn;
            }

            set {
                turnedIn = value;
                SystemEventManager.MyInstance.NotifyOnQuestStatusUpdated();
            }
        }

        public bool MyPrerequisitesMet {
            get {
                //Debug.Log(MyName + ".Quest.MyPrerequisitesMet: ID: " + GetInstanceID());
                foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet()) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                return true;
            }
        }

        public Quest MyQuestTemplate { get => questTemplate; set => questTemplate = value; }
        public int MyExperienceLevel { get => ((dynamicLevel == true ? PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel : experienceLevel) + extraLevels); }

        public int MyExperienceReward { get => experienceReward; set => experienceReward = value; }
        public List<Item> MyItemRewards { get => realItemRewardList; }
        public List<FactionNode> MyFactionRewards { get => factionRewards; }
        public List<BaseAbility> MyAbilityRewards { get => realAbilityRewardList; }
        public List<Skill> MySkillRewards { get => realSkillRewardList; }

        public bool MyTurnInItems { get => turnInItems; set => turnInItems = value; }
        public bool MyAllowRawComplete { get => allowRawComplete; set => allowRawComplete = value; }
        public int MyMaxAbilityRewards { get => maxAbilityRewards; set => maxAbilityRewards = value; }
        public int MyMaxSkillRewards { get => maxSkillRewards; set => maxSkillRewards = value; }
        public int MyMaxItemRewards { get => maxItemRewards; set => maxItemRewards = value; }
        public int MyMaxFactionRewards { get => maxFactionRewards; set => maxFactionRewards = value; }
        public bool MyRepeatableQuest { get => repeatableQuest; set => repeatableQuest = value; }
        public bool MyIsAchievement { get => isAchievement; set => isAchievement = value; }
        //public Dialog MyOpeningDialog { get => openingDialog; set => openingDialog = value; }
        public bool MyHasOpeningDialog { get => hasOpeningDialog; set => hasOpeningDialog = value; }
        public Dialog MyOpeningDialog { get => openingDialog; set => openingDialog = value; }

        public void RemoveQuest() {
            //Debug.Log("Quest.RemoveQuest(): " + MyTitle + " calling OnQuestStatusUpdated()");
            SystemEventManager.MyInstance.NotifyOnQuestStatusUpdated();
        }

        public void CheckMarkComplete(bool notifyOnUpdate = true, bool printMessages = true) {
            if (markedComplete == true) {
                return;
            }
            if (isAchievement) {
                if (printMessages == true) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("Achievement: {0} Complete!", MyName));
                }
                PlayerManager.MyInstance.PlayLevelUpEffects(0);

                markedComplete = true;
                turnedIn = true;
            } else {
                if (printMessages == true) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("{0} Complete!", MyName));
                }
            }
            markedComplete = true;
            if (notifyOnUpdate == true) {
                SystemEventManager.MyInstance.NotifyOnQuestStatusUpdated();
            }
        }

        public void OnAbandonQuest() {
            foreach (CollectObjective o in MyCollectObjectives) {
                o.OnAbandonQuest();
            }
            foreach (KillObjective o in MyKillObjectives) {
                o.OnAbandonQuest();
            }
            foreach (TradeSkillObjective o in MyTradeSkillObjectives) {
                o.OnAbandonQuest();
            }
            foreach (AbilityObjective o in MyAbilityObjectives) {
                o.OnAbandonQuest();
            }
            foreach (UseInteractableObjective o in MyUseInteractableObjectives) {
                o.OnAbandonQuest();
            }
            foreach (QuestQuestObjective o in MyQuestQuestObjectives) {
                o.OnAbandonQuest();
            }
            foreach (DialogObjective o in MyDialogObjectives) {
                o.OnAbandonQuest();
            }
        }

        public string GetStatus() {
            //Debug.Log(MyName + ".Quest.GetStatus()");

            string returnString = string.Empty;

            if (TurnedIn && !repeatableQuest) {
                return "completed";
            }

            if (QuestLog.MyInstance.HasQuest(MyName) && IsComplete) {
                return "complete";
            }

            if (QuestLog.MyInstance.HasQuest(MyName)) {
                return "inprogress";
            }

            if (!QuestLog.MyInstance.HasQuest(MyName) && (TurnedIn == false || MyRepeatableQuest == true) && MyPrerequisitesMet == true) {
                return "available";
            }

            // this quest prerequisites were not met
            return "unavailable";
        }

        public override string GetSummary() {
            return string.Format("{0}\n{1} Points", description, experienceReward);
        }

        public string GetObjectiveDescription() {

            Color titleColor = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, MyExperienceLevel);
            return string.Format("<size=30><b><color=#{0}>{1}</color></b></size>\n\n<size=18>{2}</size>\n\n<b><size=24>Objectives:</size></b>\n\n<size=18>{3}</size>", ColorUtility.ToHtmlStringRGB(titleColor), MyName, MyDescription, GetUnformattedObjectiveList());

        }

        public string GetUnformattedObjectiveList() {
            string objectives = string.Empty;
            List<string> objectiveList = new List<string>();
            foreach (CollectObjective obj in MyCollectObjectives) {
                objectiveList.Add(obj.MyType + ": " + Mathf.Clamp(obj.MyCurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (KillObjective obj in MyKillObjectives) {
                objectiveList.Add(obj.MyType + ": " + Mathf.Clamp(obj.MyCurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (TradeSkillObjective obj in MyTradeSkillObjectives) {
                objectiveList.Add(obj.MyType + ": " + Mathf.Clamp(obj.MyCurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (QuestQuestObjective obj in MyQuestQuestObjectives) {
                objectiveList.Add(obj.MyType + ": " + Mathf.Clamp(obj.MyCurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (UseInteractableObjective obj in MyUseInteractableObjectives) {
                objectiveList.Add(obj.MyType + ": " + Mathf.Clamp(obj.MyCurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (AbilityObjective obj in MyAbilityObjectives) {
                string beginText = string.Empty;
                if (obj.MyRequireUse) {
                    beginText = "Use ";
                } else {
                    beginText = "Learn ";
                }
                objectiveList.Add(beginText + obj.MyType + ": " + Mathf.Clamp(obj.MyCurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (DialogObjective obj in MyDialogObjectives) {
                objectiveList.Add(obj.MyType + ": " + Mathf.Clamp(obj.MyCurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            objectives = string.Join("\n", objectiveList);
            if (objectives == string.Empty) {
                objectives = MyName;
            }
            return objectives;
        }

        public void AcceptQuest(bool printMessages = true) {
            //Debug.Log("Quest.AcceptQuest(" + MyName + ")");

            foreach (CollectObjective o in MyCollectObjectives) {
                o.OnAcceptQuest(this, printMessages);
            }
            foreach (TradeSkillObjective o in MyTradeSkillObjectives) {
                o.OnAcceptQuest(this, printMessages);
            }
            foreach (AbilityObjective o in MyAbilityObjectives) {
                o.OnAcceptQuest(this, printMessages);
            }
            foreach (KillObjective o in MyKillObjectives) {
                o.OnAcceptQuest(this, printMessages);
            }
            foreach (UseInteractableObjective o in MyUseInteractableObjectives) {
                o.OnAcceptQuest(this, printMessages);
            }
            foreach (DialogObjective o in MyDialogObjectives) {
                o.OnAcceptQuest(this, printMessages);
            }
            foreach (QuestQuestObjective o in MyQuestQuestObjectives) {
                o.OnAcceptQuest(this, printMessages);
            }
            if (isAchievement == false && printMessages == true) {
                MessageFeedManager.MyInstance.WriteMessage("Quest Accepted: " + MyName);
            }
            if (!markedComplete) {
                // needs to be done here if quest wasn't auto-completed in checkcompletion
                SystemEventManager.MyInstance.NotifyOnQuestStatusUpdated();
            }
        }

        public void CheckCompletion(bool notifyOnUpdate = true, bool printMessages = true) {
            //Debug.Log("QuestLog.CheckCompletion()");
            if (markedComplete) {
                // no need to waste cycles checking, we are already done
                return;
            }
            bool questComplete = true;

            foreach (CollectObjective o in MyCollectObjectives) {
                if (!o.IsComplete) {
                    questComplete = false;
                }
            }
            foreach (TradeSkillObjective o in MyTradeSkillObjectives) {
                if (!o.IsComplete) {
                    questComplete = false;
                }
            }
            foreach (AbilityObjective o in MyAbilityObjectives) {
                if (!o.IsComplete) {
                    questComplete = false;
                }
            }
            foreach (KillObjective o in MyKillObjectives) {
                if (!o.IsComplete) {
                    questComplete = false;
                }
            }
            foreach (UseInteractableObjective o in MyUseInteractableObjectives) {
                if (!o.IsComplete) {
                    questComplete = false;
                }
            }
            foreach (DialogObjective o in MyDialogObjectives) {
                if (!o.IsComplete) {
                    questComplete = false;
                }
            }
            foreach (QuestQuestObjective o in MyQuestQuestObjectives) {
                if (!o.IsComplete) {
                    questComplete = false;
                }
            }

            if (questComplete) {
                CheckMarkComplete(notifyOnUpdate, printMessages);
            } else {
                // since this method only gets called as a result of a quest objective status updating, we need to notify for that at minimum
                //Debug.Log("Quest.CheckCompletion(): about to notify for objective status updated");
                SystemEventManager.MyInstance.NotifyOnQuestObjectiveStatusUpdated();
            }
        }

        public static List<Quest> GetCompleteQuests(List<QuestNode> questNodeArray, bool requireInQuestLog = false) {
            return GetQuestListByType("complete", questNodeArray, requireInQuestLog, false, true);
        }

        public static List<Quest> GetInProgressQuests(List<QuestNode> questNodeArray, bool requireInQuestLog = true) {
            return GetQuestListByType("inprogress", questNodeArray, requireInQuestLog, false, true);
        }

        public static List<Quest> GetAvailableQuests(List<QuestNode> questNodeArray, bool requireInQuestLog = false) {
            return GetQuestListByType("available", questNodeArray, requireInQuestLog, true, false);
        }

        public static List<Quest> GetQuestListByType(string questStatusType, List<QuestNode> questNodeArray, bool requireInQuestLog = false, bool requireStartQuest = false, bool requireEndQuest = false) {
            List<Quest> returnList = new List<Quest>();
            foreach (QuestNode questNode in questNodeArray) {
                if (questNode.MyQuest != null) {
                    if (questNode.MyQuest.GetStatus() == questStatusType && (requireInQuestLog == true ? QuestLog.MyInstance.HasQuest(questNode.MyQuest.MyName) : true) && (requireStartQuest == true ? questNode.MyStartQuest : true) && (requireEndQuest == true ? questNode.MyEndQuest : true)) {
                        //Debug.Log("Quest.GetQuestListByType(" + questStatusType + "): adding quest: " + questNode.MyQuest.MyName);
                        returnList.Add(questNode.MyQuest);
                    }
                }
            }
            return returnList;
        }

        public override void SetupScriptableObjects() {
            //Debug.Log(MyName + ".Quest.SetupScriptableObjects(): ID: " + GetInstanceID());
        
            base.SetupScriptableObjects();

            realAbilityRewardList = new List<BaseAbility>();
            if (abilityRewardNames != null) {
                foreach (string baseAbilityName in abilityRewardNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        realAbilityRewardList.Add(baseAbility);
                    } else {
                        Debug.LogError("Quest.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            realSkillRewardList = new List<Skill>();
            if (skillRewardNames != null) {
                foreach (string skillName in skillRewardNames) {
                    Skill skill = SystemSkillManager.MyInstance.GetResource(skillName);
                    if (skill != null) {
                        realSkillRewardList.Add(skill);
                    } else {
                        Debug.LogError("Quest.SetupScriptableObjects(): Could not find skill : " + skillName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            realItemRewardList = new List<Item>();
            if (itemRewardNames != null) {
                foreach (string itemName in itemRewardNames) {
                    Item item = SystemItemManager.MyInstance.GetResource(itemName);
                    if (item != null) {
                        realItemRewardList.Add(item);
                    } else {
                        Debug.LogError("Quest.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            openingDialog = null;
            if (hasOpeningDialog) {
                Dialog dialog = SystemDialogManager.MyInstance.GetResource(MyName);
                if (dialog != null) {
                    openingDialog = dialog;
                } else {
                    Debug.LogError("Quest.SetupScriptableObjects(): Could not find dialog : " + MyName + " while inititalizing quest " + MyName + ".  CHECK INSPECTOR");
                }
            }

            foreach (QuestObjective objective in collectObjectives) {
                objective.SetupScriptableObjects();
            }
            foreach (QuestObjective objective in killObjectives) {
                objective.SetupScriptableObjects();
            }
            foreach (QuestObjective objective in tradeSkillObjectives) {
                objective.SetupScriptableObjects();
            }
            foreach (QuestObjective objective in abilityObjectives) {
                objective.SetupScriptableObjects();
            }
            foreach (QuestObjective objective in useInteractableObjectives) {
                objective.SetupScriptableObjects();
            }
            foreach (QuestObjective objective in questQuestObjectives) {
                objective.SetupScriptableObjects();
            }
            foreach (QuestObjective objective in dialogObjectives) {
                objective.SetupScriptableObjects();
            }
            //Debug.Log("Quest.SetupScriptableObjects(): " + MyName + " about to initialize prerequisiteConditions");
            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.SetupScriptableObjects();
            }

        }

    }
}