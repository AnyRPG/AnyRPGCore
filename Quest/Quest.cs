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
    public class Quest : DescribableResource, IPrerequisiteOwner {

        public event System.Action OnQuestStatusUpdated = delegate { };

        [Header("Quest")]

        [Tooltip("Achievements are automatically tracked and automatically completed by the system without being in the player quest log")]
        [SerializeField]
        private bool isAchievement = false;

        [Tooltip("If true, this quest can this quest be completed more than once")]
        [SerializeField]
        private bool repeatableQuest = false;

        // a dialog that is not a requirement to interact with the questgiver or see the quest, but must be completed to start it
        //[SerializeField]
        //private Dialog openingDialog;

        // replaces the above setting to avoid issues with scriptableObjects
        [Tooltip("If true a dialog with the same name as the quest will be used (if found) and will be required to be completed before the quest can be accepted")]
        [SerializeField]
        private bool hasOpeningDialog;

        private Dialog openingDialog;

        private bool markedComplete = false;

        [Header("Quest Level")]

        [Tooltip("The level that is considered appropriate for the quest.  Used to calculate xp reduction")]
        [SerializeField]
        private int experienceLevel = 1;

        [Tooltip("If true, this quest is always the same level as the player")]
        [SerializeField]
        private bool dynamicLevel = true;

        [Tooltip("If dynamic level is true, this amount of extra levels will be added to the quest")]
        [SerializeField]
        private int extraLevels = 0;

        [Header("Experience Reward")]

        [Tooltip("The base experience for the quest, not scaled by level, and in addition to any automatic quest xp configured at the game level")]
        [SerializeField]
        private int baseExperienceReward = 0;

        [Tooltip("The experience for the quest, scaled by level, and in addition to any automatic quest xp configured at the game level")]
        [SerializeField]
        private int experienceRewardPerLevel = 0;

        [Header("Currency Reward")]

        [Tooltip("If true, the quest will reward currency based on the system quest currency reward settings")]
        [SerializeField]
        private bool automaticCurrencyReward = false;

        [Tooltip("If automatic currency is enabled for a quest, this currency will be rewarded")]
        [SerializeField]
        private string rewardCurrencyName = string.Empty;

        private Currency rewardCurrency;

        [Tooltip("The base currency reward for the quest, not scaled by level, and in addition to any automatic quest reward configured at the game level")]
        [SerializeField]
        private int baseCurrencyReward = 0;

        [Tooltip("The currency for the quest, scaled by level, and in addition to any automatic quest currency configured at the game level")]
        [SerializeField]
        private int currencyRewardPerLevel = 0;

        [Header("Item Rewards")]

        [Tooltip("The maximum number of item rewards that can be chosen if there are more than 1 reward")]
        [SerializeField]
        private int maxItemRewards = 0;

        [SerializeField]
        private List<string> itemRewardNames = new List<string>();

        private List<Item> itemRewardList = new List<Item>();

        [Header("Faction Rewards")]

        [SerializeField]
        private int maxFactionRewards = 0;

        [SerializeField]
        private List<FactionNode> factionRewards = new List<FactionNode>();

        [Header("Ability Rewards")]

        [SerializeField]
        private int maxAbilityRewards = 0;

        [SerializeField]
        private List<string> abilityRewardNames = new List<string>();

        private List<BaseAbility> abilityRewardList = new List<BaseAbility>();

        [Header("Skill Rewards")]

        [SerializeField]
        private int maxSkillRewards = 0;

        [SerializeField]
        private List<string> skillRewardNames = new List<string>();

        private List<Skill> skillRewardList = new List<Skill>();

        [Header("Objectives")]

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
        private VisitZoneObjective[] visitZoneObjectives;

        [Header("Prerequisites")]

        [SerializeField]
        private List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        [Header("Completion")]

        [Tooltip("Whether or not to give the items to the questgiver when you turn in a quest.  If false, you keep the items in your bag.")]
        [SerializeField]
        private bool turnInItems;

        [Tooltip("the player can complete the quest without having the quest in the questlog")]
        [SerializeField]
        private bool allowRawComplete = false;

        private Quest questTemplate = null;

        // Track whether this quest has been turned in
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
                // disabled because if a quest is raw completable (not required to be in log), it shouldn't have objectives anyway since there is no way to track them
                // therefore the default true at the bottom should return true anyway
                /*
                if (MyAllowRawComplete == true) {
                    return true;
                }
                */

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

                foreach (VisitZoneObjective o in visitZoneObjectives) {
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
        }

        public void SetTurnedIn(bool turnedIn, bool notify = true) {
            this.turnedIn = turnedIn;
            //Debug.Log(MyName + ".Quest.TurnedIn = " + value);
            if (notify) {
                SystemEventManager.MyInstance.NotifyOnQuestStatusUpdated();
                OnQuestStatusUpdated();
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
        public int MyExperienceLevel { get => ((dynamicLevel == true ? PlayerManager.MyInstance.MyCharacter.CharacterStats.Level : experienceLevel) + extraLevels); }

        public List<Item> MyItemRewards { get => itemRewardList; }
        public List<FactionNode> MyFactionRewards { get => factionRewards; }
        public List<BaseAbility> MyAbilityRewards { get => abilityRewardList; }
        public List<Skill> MySkillRewards { get => skillRewardList; }

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
        public VisitZoneObjective[] VisitZoneObjectives { get => visitZoneObjectives; set => visitZoneObjectives = value; }
        public int ExperienceRewardPerLevel { get => experienceRewardPerLevel; set => experienceRewardPerLevel = value; }
        public int BaseExperienceReward { get => baseExperienceReward; set => baseExperienceReward = value; }
        public bool AutomaticCurrencyReward { get => automaticCurrencyReward; set => automaticCurrencyReward = value; }
        public string RewardCurrencyName { get => rewardCurrencyName; set => rewardCurrencyName = value; }
        public Currency RewardCurrency { get => rewardCurrency; set => rewardCurrency = value; }
        public int BaseCurrencyReward { get => baseCurrencyReward; set => baseCurrencyReward = value; }
        public int CurrencyRewardPerLevel { get => currencyRewardPerLevel; set => currencyRewardPerLevel = value; }

        public void RemoveQuest() {
            //Debug.Log("Quest.RemoveQuest(): " + DisplayName + " calling OnQuestStatusUpdated()");
            OnAbandonQuest();
            SystemEventManager.MyInstance.NotifyOnQuestStatusUpdated();
            OnQuestStatusUpdated();
        }

        public List<CurrencyNode> GetCurrencyReward() {
            List<CurrencyNode> currencyNodes = new List<CurrencyNode>();

            if (AutomaticCurrencyReward == true) {
                if (SystemConfigurationManager.MyInstance.QuestCurrency != null) {
                    CurrencyNode currencyNode = new CurrencyNode();
                    currencyNode.currency = SystemConfigurationManager.MyInstance.QuestCurrency;
                    currencyNode.MyAmount = SystemConfigurationManager.MyInstance.QuestCurrencyAmountPerLevel * MyExperienceLevel;
                    currencyNodes.Add(currencyNode);
                }
            }
            if (RewardCurrency != null) {
                CurrencyNode currencyNode = new CurrencyNode();
                currencyNode.MyAmount = BaseCurrencyReward + (CurrencyRewardPerLevel * MyExperienceLevel);
                currencyNode.currency = RewardCurrency;
                currencyNodes.Add(currencyNode);
            }

            return currencyNodes;
        }

        public void CheckMarkComplete(bool notifyOnUpdate = true, bool printMessages = true) {
            if (markedComplete == true) {
                return;
            }
            if (isAchievement) {
                if (printMessages == true) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("Achievement: {0} Complete!", DisplayName));
                }
                PlayerManager.MyInstance.PlayLevelUpEffects(0);

                markedComplete = true;
                turnedIn = true;
            } else {
                if (printMessages == true) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("{0} Complete!", DisplayName));
                }
            }
            markedComplete = true;
            if (notifyOnUpdate == true) {
                SystemEventManager.MyInstance.NotifyOnQuestStatusUpdated();
                OnQuestStatusUpdated();
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
            foreach (VisitZoneObjective o in VisitZoneObjectives) {
                o.OnAbandonQuest();
            }
        }

        public string GetStatus() {
            //Debug.Log(MyName + ".Quest.GetStatus()");

            string returnString = string.Empty;

            if (TurnedIn && !repeatableQuest) {
                //Debug.Log(MyName + ".Quest.GetStatus(): returning completed");
                return "completed";
            }

            if (QuestLog.MyInstance.HasQuest(DisplayName) && IsComplete) {
                //Debug.Log(MyName + ".Quest.GetStatus(): returning complete");
                return "complete";
            }

            if (QuestLog.MyInstance.HasQuest(DisplayName)) {
                //Debug.Log(MyName + ".Quest.GetStatus(): returning inprogress");
                return "inprogress";
            }

            if (!QuestLog.MyInstance.HasQuest(DisplayName) && (TurnedIn == false || MyRepeatableQuest == true) && MyPrerequisitesMet == true) {
                //Debug.Log(MyName + ".Quest.GetStatus(): returning available");
                return "available";
            }

            // this quest prerequisites were not met
            //Debug.Log(MyName + ".Quest.GetStatus(): returning unavailable");
            return "unavailable";
        }

        public override string GetSummary() {
            //return string.Format("{0}\n{1} Points", description, baseExperienceReward);
            return string.Format("{0}", description);
        }

        public string GetObjectiveDescription() {

            Color titleColor = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level, MyExperienceLevel);
            return string.Format("<size=30><b><color=#{0}>{1}</color></b></size>\n\n<size=18>{2}</size>\n\n<b><size=24>Objectives:</size></b>\n\n<size=18>{3}</size>", ColorUtility.ToHtmlStringRGB(titleColor), DisplayName, MyDescription, GetUnformattedObjectiveList());

        }

        public string GetUnformattedObjectiveList() {
            string objectives = string.Empty;
            List<string> objectiveList = new List<string>();
            foreach (CollectObjective obj in MyCollectObjectives) {
                objectiveList.Add(obj.DisplayName + ": " + Mathf.Clamp(obj.CurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (KillObjective obj in MyKillObjectives) {
                objectiveList.Add(obj.DisplayName + ": " + Mathf.Clamp(obj.CurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (TradeSkillObjective obj in MyTradeSkillObjectives) {
                objectiveList.Add(obj.DisplayName + ": " + Mathf.Clamp(obj.CurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (QuestQuestObjective obj in MyQuestQuestObjectives) {
                //Debug.Log("questquestobjective display");
                objectiveList.Add(obj.DisplayName + ": " + Mathf.Clamp(obj.CurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (UseInteractableObjective obj in MyUseInteractableObjectives) {
                objectiveList.Add(obj.DisplayName + ": " + Mathf.Clamp(obj.CurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (AbilityObjective obj in MyAbilityObjectives) {
                string beginText = string.Empty;
                if (obj.MyRequireUse) {
                    beginText = "Use ";
                } else {
                    beginText = "Learn ";
                }
                objectiveList.Add(beginText + obj.DisplayName + ": " + Mathf.Clamp(obj.CurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (DialogObjective obj in MyDialogObjectives) {
                objectiveList.Add(obj.DisplayName + ": " + Mathf.Clamp(obj.CurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            foreach (VisitZoneObjective obj in VisitZoneObjectives) {
                objectiveList.Add(obj.DisplayName + ": " + Mathf.Clamp(obj.CurrentAmount, 0, obj.MyAmount) + "/" + obj.MyAmount);
            }
            objectives = string.Join("\n", objectiveList);
            if (objectives == string.Empty) {
                objectives = DisplayName;
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
            foreach (VisitZoneObjective o in VisitZoneObjectives) {
                o.OnAcceptQuest(this, printMessages);
            }
            foreach (QuestQuestObjective o in MyQuestQuestObjectives) {
                o.OnAcceptQuest(this, printMessages);
            }
            if (isAchievement == false && printMessages == true) {
                MessageFeedManager.MyInstance.WriteMessage("Quest Accepted: " + DisplayName);
            }
            if (!markedComplete) {
                // needs to be done here if quest wasn't auto-completed in checkcompletion
                SystemEventManager.MyInstance.NotifyOnQuestStatusUpdated();
                OnQuestStatusUpdated();
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
            foreach (VisitZoneObjective o in VisitZoneObjectives) {
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
                    if (questNode.MyQuest.GetStatus() == questStatusType && (requireInQuestLog == true ? QuestLog.MyInstance.HasQuest(questNode.MyQuest.DisplayName) : true) && (requireStartQuest == true ? questNode.MyStartQuest : true) && (requireEndQuest == true ? questNode.MyEndQuest : true)) {
                        //Debug.Log("Quest.GetQuestListByType(" + questStatusType + "): adding quest: " + questNode.MyQuest.MyName);
                        returnList.Add(questNode.MyQuest);
                    }
                }
            }
            return returnList;
        }

        // force prerequisite status update outside normal event notification
        public void UpdatePrerequisites(bool notify = true) {
            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.UpdatePrerequisites(notify);
            }
        }

        public override void SetupScriptableObjects() {
            //Debug.Log(MyName + ".Quest.SetupScriptableObjects(): ID: " + GetInstanceID());
        
            base.SetupScriptableObjects();

            if (rewardCurrencyName != null && rewardCurrencyName != string.Empty) {
                Currency tmpCurrency = SystemCurrencyManager.MyInstance.GetResource(rewardCurrencyName);
                if (tmpCurrency != null) {
                    rewardCurrency = tmpCurrency;
                    //currencyNode.MyAmount = gainCurrencyAmount;
                } else {
                    Debug.LogError("Quest.SetupScriptableObjects(): Could not find currency : " + rewardCurrencyName + ".  CHECK INSPECTOR");
                }
            }

            abilityRewardList = new List<BaseAbility>();
            if (abilityRewardNames != null) {
                foreach (string baseAbilityName in abilityRewardNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        abilityRewardList.Add(baseAbility);
                    } else {
                        Debug.LogError("Quest.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            skillRewardList = new List<Skill>();
            if (skillRewardNames != null) {
                foreach (string skillName in skillRewardNames) {
                    Skill skill = SystemSkillManager.MyInstance.GetResource(skillName);
                    if (skill != null) {
                        skillRewardList.Add(skill);
                    } else {
                        Debug.LogError("Quest.SetupScriptableObjects(): Could not find skill : " + skillName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            itemRewardList = new List<Item>();
            if (itemRewardNames != null) {
                foreach (string itemName in itemRewardNames) {
                    Item item = SystemItemManager.MyInstance.GetResource(itemName);
                    if (item != null) {
                        itemRewardList.Add(item);
                    } else {
                        Debug.LogError("Quest.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (factionRewards != null && factionRewards.Count > 0) {
                foreach (FactionNode factionNode in factionRewards) {
                    factionNode.SetupScriptableObjects();
                }
            }

            openingDialog = null;
            if (hasOpeningDialog) {
                Dialog dialog = SystemDialogManager.MyInstance.GetResource(DisplayName);
                if (dialog != null) {
                    openingDialog = dialog;
                } else {
                    Debug.LogError("Quest.SetupScriptableObjects(): Could not find dialog : " + DisplayName + " while inititalizing quest " + DisplayName + ".  CHECK INSPECTOR");
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
            foreach (QuestObjective objective in visitZoneObjectives) {
                objective.SetupScriptableObjects();
            }
            //Debug.Log("Quest.SetupScriptableObjects(): " + MyName + " about to initialize prerequisiteConditions");
            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.SetupScriptableObjects(this);
            }

        }

        public override void CleanupScriptableObjects() {
            base.CleanupScriptableObjects();
            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.CleanupScriptableObjects();
            }
        }

        public void HandlePrerequisiteUpdates() {
            OnQuestStatusUpdated();
        }
    }
}