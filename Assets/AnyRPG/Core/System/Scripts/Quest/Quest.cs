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
        protected bool isAchievement = false;

        [Tooltip("If true, this quest can this quest be completed more than once")]
        [SerializeField]
        protected bool repeatableQuest = false;

        // a dialog that is not a requirement to interact with the questgiver or see the quest, but must be completed to start it
        //[SerializeField]
        //private Dialog openingDialog;

        // replaces the above setting to avoid issues with scriptableObjects
        [Tooltip("If true a dialog with the same name as the quest will be used (if found) and will be required to be completed before the quest can be accepted")]
        [SerializeField]
        protected bool hasOpeningDialog;

        protected Dialog openingDialog;

        [Header("Quest Level")]

        [Tooltip("The level that is considered appropriate for the quest.  Used to calculate xp reduction")]
        [SerializeField]
        protected int experienceLevel = 1;

        [Tooltip("If true, this quest is always the same level as the player")]
        [SerializeField]
        protected bool dynamicLevel = true;

        [Tooltip("If dynamic level is true, this amount of extra levels will be added to the quest")]
        [SerializeField]
        protected int extraLevels = 0;

        [Header("Experience Reward")]

        [Tooltip("The base experience for the quest, not scaled by level, and in addition to any automatic quest xp configured at the game level")]
        [SerializeField]
        protected int baseExperienceReward = 0;

        [Tooltip("The experience for the quest, scaled by level, and in addition to any automatic quest xp configured at the game level")]
        [SerializeField]
        protected int experienceRewardPerLevel = 0;

        [Header("Currency Reward")]

        [Tooltip("If true, the quest will reward currency based on the system quest currency reward settings")]
        [SerializeField]
        protected bool automaticCurrencyReward = false;

        [Tooltip("If automatic currency is enabled for a quest, this currency will be rewarded")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Currency))]
        protected string rewardCurrencyName = string.Empty;

        protected Currency rewardCurrency;

        [Tooltip("The base currency reward for the quest, not scaled by level, and in addition to any automatic quest reward configured at the game level")]
        [SerializeField]
        protected int baseCurrencyReward = 0;

        [Tooltip("The currency for the quest, scaled by level, and in addition to any automatic quest currency configured at the game level")]
        [SerializeField]
        protected int currencyRewardPerLevel = 0;

        [Header("Item Rewards")]

        [Tooltip("The maximum number of item rewards that can be chosen if there are more than 1 reward")]
        [SerializeField]
        protected int maxItemRewards = 0;

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        protected List<string> itemRewardNames = new List<string>();

        protected List<Item> itemRewardList = new List<Item>();

        [Header("Faction Rewards")]

        [SerializeField]
        protected int maxFactionRewards = 0;

        [SerializeField]
        protected List<FactionNode> factionRewards = new List<FactionNode>();

        [Header("Ability Rewards")]

        [SerializeField]
        protected int maxAbilityRewards = 0;

        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        protected List<string> abilityRewardNames = new List<string>();

        protected List<BaseAbility> abilityRewardList = new List<BaseAbility>();

        [Header("Skill Rewards")]

        [SerializeField]
        protected int maxSkillRewards = 0;

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Skill))]
        protected List<string> skillRewardNames = new List<string>();

        protected List<Skill> skillRewardList = new List<Skill>();

        [Header("Objectives")]

        [SerializeField]
        protected List<QuestStep> steps = new List<QuestStep>();

        [Header("Deprecated Objectives")]

        [SerializeField]
        [FormerlySerializedAs("collectObjectives")]
        protected CollectObjective[] deprecatedCollectObjectives;

        [SerializeField]
        [FormerlySerializedAs("killObjectives")]
        protected KillObjective[] deprecatedKillObjectives;

        [SerializeField]
        [FormerlySerializedAs("tradeSkillObjectives")]
        protected TradeSkillObjective[] deprecatedTradeSkillObjectives;

        [SerializeField]
        [FormerlySerializedAs("abilityObjectives")]
        protected AbilityObjective[] deprecatedAbilityObjectives;

        [SerializeField]
        [FormerlySerializedAs("useInteractableObjectives")]
        protected UseInteractableObjective[] deprecatedUseInteractableObjectives;

        [SerializeField]
        [FormerlySerializedAs("questQuestObjectives")]
        protected QuestQuestObjective[] deprecatedQuestQuestObjectives;

        [SerializeField]
        [FormerlySerializedAs("dialogObjectives")]
        protected DialogObjective[] deprecatedDialogObjectives;

        [SerializeField]
        [FormerlySerializedAs("visitZoneObjectives")]
        protected VisitZoneObjective[] deprecatedVisitZoneObjectives;

        [Header("Prerequisites")]

        [SerializeField]
        protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        [Header("Completion")]

        [Tooltip("Whether or not to give the items to the questgiver when you turn in a quest.  If false, you keep the items in your bag.")]
        [SerializeField]
        protected bool turnInItems;

        [Tooltip("the player can complete the quest without having the quest in the questlog")]
        [SerializeField]
        protected bool allowRawComplete = false;

        protected Quest questTemplate = null;

        // game manager references
        protected SaveManager saveManager = null;
        protected PlayerManager playerManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected QuestLog questLog = null;

        public virtual CollectObjective[] MyCollectObjectives { get => deprecatedCollectObjectives; set => deprecatedCollectObjectives = value; }
        public virtual KillObjective[] MyKillObjectives { get => deprecatedKillObjectives; set => deprecatedKillObjectives = value; }
        public virtual TradeSkillObjective[] MyTradeSkillObjectives { get => deprecatedTradeSkillObjectives; set => deprecatedTradeSkillObjectives = value; }
        public virtual AbilityObjective[] MyAbilityObjectives { get => deprecatedAbilityObjectives; set => deprecatedAbilityObjectives = value; }
        public virtual UseInteractableObjective[] MyUseInteractableObjectives { get => deprecatedUseInteractableObjectives; set => deprecatedUseInteractableObjectives = value; }
        public virtual QuestQuestObjective[] MyQuestQuestObjectives { get => deprecatedQuestQuestObjectives; set => deprecatedQuestQuestObjectives = value; }
        public virtual DialogObjective[] MyDialogObjectives { get => deprecatedDialogObjectives; set => deprecatedDialogObjectives = value; }

        public virtual bool IsComplete {
            get {
                //Debug.Log("Quest.IsComplete: " + MyTitle);
                // disabled because if a quest is raw completable (not required to be in log), it shouldn't have objectives anyway since there is no way to track them
                // therefore the default true at the bottom should return true anyway
                /*
                if (MyAllowRawComplete == true) {
                    return true;
                }
                */

                foreach (QuestObjective o in deprecatedCollectObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }
                foreach (QuestObjective o in deprecatedKillObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }
                foreach (QuestObjective o in deprecatedTradeSkillObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }
                foreach (QuestObjective o in deprecatedAbilityObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }
                foreach (QuestObjective o in deprecatedUseInteractableObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }

                foreach (QuestQuestObjective o in deprecatedQuestQuestObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }

                foreach (DialogObjective o in deprecatedDialogObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }

                foreach (VisitZoneObjective o in deprecatedVisitZoneObjectives) {
                    if (!o.IsComplete) {
                        return false;
                    }
                }

                //Debug.Log("Quest: " + MyTitle + ": no objectives for this quest:  were not complete, about to return true");
                return true;
            }
        }

        public virtual bool TurnedIn {
            get {
                return saveManager.GetQuestSaveData(this).turnedIn;
                //return false;
            }
            set {
                QuestSaveData saveData = saveManager.GetQuestSaveData(this);
                saveData.turnedIn = value;
                saveManager.QuestSaveDataDictionary[saveData.MyName] = saveData;
            }
        }

        public virtual void SetTurnedIn(bool turnedIn, bool notify = true) {
            this.TurnedIn = turnedIn;
            //Debug.Log(MyName + ".Quest.TurnedIn = " + value);
            if (notify) {
                if (playerManager.PlayerUnitSpawned == false) {
                    // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                    return;
                }
                SystemEventManager.TriggerEvent("OnQuestStatusUpdated", new EventParamProperties());
                SystemEventManager.TriggerEvent("OnAfterQuestStatusUpdated", new EventParamProperties());
                OnQuestStatusUpdated();
            }
        }

        public virtual bool MyPrerequisitesMet {
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

        public virtual Quest QuestTemplate { get => questTemplate; set => questTemplate = value; }
        public virtual int ExperienceLevel { get => ((dynamicLevel == true ? playerManager.MyCharacter.CharacterStats.Level : experienceLevel) + extraLevels); }

        public virtual List<Item> ItemRewards { get => itemRewardList; }
        public virtual List<FactionNode> FactionRewards { get => factionRewards; }
        public virtual List<BaseAbility> AbilityRewards { get => abilityRewardList; }
        public virtual List<Skill> SkillRewards { get => skillRewardList; }

        public virtual bool TurnInItems { get => turnInItems; set => turnInItems = value; }
        public virtual bool AllowRawComplete { get => allowRawComplete; set => allowRawComplete = value; }
        public virtual int MaxAbilityRewards { get => maxAbilityRewards; set => maxAbilityRewards = value; }
        public virtual int MaxSkillRewards { get => maxSkillRewards; set => maxSkillRewards = value; }
        public virtual int MaxItemRewards { get => maxItemRewards; set => maxItemRewards = value; }
        public virtual int MaxFactionRewards { get => maxFactionRewards; set => maxFactionRewards = value; }
        public virtual bool RepeatableQuest { get => repeatableQuest; set => repeatableQuest = value; }
        public virtual bool IsAchievement { get => isAchievement; set => isAchievement = value; }
        public virtual bool HasOpeningDialog { get => hasOpeningDialog; set => hasOpeningDialog = value; }
        public virtual Dialog OpeningDialog { get => openingDialog; set => openingDialog = value; }
        public virtual VisitZoneObjective[] VisitZoneObjectives { get => deprecatedVisitZoneObjectives; set => deprecatedVisitZoneObjectives = value; }
        public virtual int ExperienceRewardPerLevel { get => experienceRewardPerLevel; set => experienceRewardPerLevel = value; }
        public virtual int BaseExperienceReward { get => baseExperienceReward; set => baseExperienceReward = value; }
        public virtual bool AutomaticCurrencyReward { get => automaticCurrencyReward; set => automaticCurrencyReward = value; }
        public virtual string RewardCurrencyName { get => rewardCurrencyName; set => rewardCurrencyName = value; }
        public virtual Currency RewardCurrency { get => rewardCurrency; set => rewardCurrency = value; }
        public virtual int BaseCurrencyReward { get => baseCurrencyReward; set => baseCurrencyReward = value; }
        public virtual int CurrencyRewardPerLevel { get => currencyRewardPerLevel; set => currencyRewardPerLevel = value; }
        public virtual int CurrentStep {
            get {
                return saveManager.GetQuestSaveData(this).questStep;
                //return false;
            }
            set {
                QuestSaveData saveData = saveManager.GetQuestSaveData(this);
                saveData.questStep = value;
                saveManager.QuestSaveDataDictionary[saveData.MyName] = saveData;
            }
        }
        public virtual bool MarkedComplete {
            get {
                return saveManager.GetQuestSaveData(this).markedComplete;
                //return false;
            }
            set {
                QuestSaveData saveData = saveManager.GetQuestSaveData(this);
                saveData.markedComplete = value;
                saveManager.QuestSaveDataDictionary[saveData.MyName] = saveData;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            playerManager = systemGameManager.PlayerManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            questLog = systemGameManager.QuestLog;
        }

        public virtual void RemoveQuest() {
            //Debug.Log("Quest.RemoveQuest(): " + DisplayName + " calling OnQuestStatusUpdated()");
            OnAbandonQuest();
            if (playerManager != null && playerManager.PlayerUnitSpawned == false) {
                // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                return;
            }
            SystemEventManager.TriggerEvent("OnQuestStatusUpdated", new EventParamProperties());
            SystemEventManager.TriggerEvent("OnAfterQuestStatusUpdated", new EventParamProperties());
            OnQuestStatusUpdated();
        }

        public virtual List<CurrencyNode> GetCurrencyReward() {
            List<CurrencyNode> currencyNodes = new List<CurrencyNode>();

            if (AutomaticCurrencyReward == true) {
                if (systemConfigurationManager.QuestCurrency != null) {
                    CurrencyNode currencyNode = new CurrencyNode();
                    currencyNode.currency = systemConfigurationManager.QuestCurrency;
                    currencyNode.Amount = systemConfigurationManager.QuestCurrencyAmountPerLevel * ExperienceLevel;
                    currencyNodes.Add(currencyNode);
                }
            }
            if (RewardCurrency != null) {
                CurrencyNode currencyNode = new CurrencyNode();
                currencyNode.Amount = BaseCurrencyReward + (CurrencyRewardPerLevel * ExperienceLevel);
                currencyNode.currency = RewardCurrency;
                currencyNodes.Add(currencyNode);
            }

            return currencyNodes;
        }

        public virtual void CheckMarkComplete(bool notifyOnUpdate = true, bool printMessages = true) {
            if (MarkedComplete == true) {
                return;
            }
            if (isAchievement) {
                if (printMessages == true) {
                    messageFeedManager.WriteMessage(string.Format("Achievement: {0} Complete!", DisplayName));
                }
                playerManager.PlayLevelUpEffects(0);

                MarkedComplete = true;
                TurnedIn = true;
            } else {
                if (printMessages == true) {
                    messageFeedManager.WriteMessage(string.Format("{0} Complete!", DisplayName));
                }
            }
            MarkedComplete = true;
            if (notifyOnUpdate == true) {
                if (playerManager != null && playerManager.PlayerUnitSpawned == false) {
                    // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                    return;
                }
                SystemEventManager.TriggerEvent("OnQuestStatusUpdated", new EventParamProperties());
                SystemEventManager.TriggerEvent("OnAfterQuestStatusUpdated", new EventParamProperties());
                OnQuestStatusUpdated();
            }
        }

        public virtual void OnAbandonQuest() {
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

        public virtual string GetStatus() {
            //Debug.Log(MyName + ".Quest.GetStatus()");

            string returnString = string.Empty;

            if (TurnedIn && !repeatableQuest) {
                //Debug.Log(MyName + ".Quest.GetStatus(): returning completed");
                return "completed";
            }

            if (questLog.HasQuest(DisplayName) && IsComplete) {
                //Debug.Log(MyName + ".Quest.GetStatus(): returning complete");
                return "complete";
            }

            if (questLog.HasQuest(DisplayName)) {
                //Debug.Log(MyName + ".Quest.GetStatus(): returning inprogress");
                return "inprogress";
            }

            if (!questLog.HasQuest(DisplayName) && (TurnedIn == false || RepeatableQuest == true) && MyPrerequisitesMet == true) {
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

        public virtual string GetObjectiveDescription() {

            Color titleColor = LevelEquations.GetTargetColor(playerManager.MyCharacter.CharacterStats.Level, ExperienceLevel);
            return string.Format("<size=30><b><color=#{0}>{1}</color></b></size>\n\n<size=18>{2}</size>\n\n<b><size=24>Objectives:</size></b>\n\n<size=18>{3}</size>", ColorUtility.ToHtmlStringRGB(titleColor), DisplayName, MyDescription, GetUnformattedObjectiveList());

        }

        public virtual string GetUnformattedObjectiveList() {
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

        public virtual void AcceptQuest(bool printMessages = true) {
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
                messageFeedManager.WriteMessage("Quest Accepted: " + DisplayName);
            }
            if (!MarkedComplete) {
                // needs to be done here if quest wasn't auto-completed in checkcompletion
                if (playerManager != null && playerManager.PlayerUnitSpawned == false) {
                    // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                    return;
                }
                SystemEventManager.TriggerEvent("OnQuestStatusUpdated", new EventParamProperties());
                SystemEventManager.TriggerEvent("OnAfterQuestStatusUpdated", new EventParamProperties());
                OnQuestStatusUpdated();
            }
        }

        public virtual void CheckCompletion(bool notifyOnUpdate = true, bool printMessages = true) {
            //Debug.Log("QuestLog.CheckCompletion()");
            if (MarkedComplete) {
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
                SystemEventManager.TriggerEvent("OnQuestObjectiveStatusUpdated", new EventParamProperties());
            }
        }

        // force prerequisite status update outside normal event notification
        public virtual void UpdatePrerequisites(bool notify = true) {
            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.UpdatePrerequisites(notify);
            }
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            //Debug.Log(DisplayName + ".Quest.SetupScriptableObjects(" + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + "): ID: " + GetInstanceID());

            base.SetupScriptableObjects(systemGameManager);

            if (rewardCurrencyName != null && rewardCurrencyName != string.Empty) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(rewardCurrencyName);
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
                    BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(baseAbilityName);
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
                    Skill skill = systemDataFactory.GetResource<Skill>(skillName);
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
                    Item item = systemDataFactory.GetResource<Item>(itemName);
                    if (item != null) {
                        itemRewardList.Add(item);
                    } else {
                        Debug.LogError("Quest.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (factionRewards != null && factionRewards.Count > 0) {
                foreach (FactionNode factionNode in factionRewards) {
                    factionNode.SetupScriptableObjects(systemGameManager);
                }
            }

            openingDialog = null;
            if (hasOpeningDialog) {
                Dialog dialog = systemDataFactory.GetResource<Dialog>(DisplayName);
                if (dialog != null) {
                    openingDialog = dialog;
                } else {
                    Debug.LogError("Quest.SetupScriptableObjects(): Could not find dialog : " + DisplayName + " while inititalizing quest " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            foreach (QuestStep questStep in steps) {
                questStep.SetupScriptableObjects(this, systemGameManager);
            }

            foreach (QuestObjective objective in deprecatedCollectObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in deprecatedKillObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in deprecatedTradeSkillObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in deprecatedAbilityObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in deprecatedUseInteractableObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in deprecatedQuestQuestObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in deprecatedDialogObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in deprecatedVisitZoneObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            //Debug.Log("Quest.SetupScriptableObjects(): " + MyName + " about to initialize prerequisiteConditions");
            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.SetupScriptableObjects(systemGameManager, this);
            }

        }

        public override void CleanupScriptableObjects() {
            base.CleanupScriptableObjects();
            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.CleanupScriptableObjects(this);
            }
        }

        public virtual void HandlePrerequisiteUpdates() {
            OnQuestStatusUpdated();
        }
    }

    [System.Serializable]
    public class QuestStep : ConfiguredClass{
        [SerializeReference]
        [SerializeReferenceButton]
        private List<QuestObjective> questObjectives = new List<QuestObjective>();

        public List<QuestObjective> QuestObjectives { get => questObjectives; }

        public void SetupScriptableObjects(Quest quest, SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            foreach (QuestObjective objective in questObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(quest);
            }
        }

    }
}