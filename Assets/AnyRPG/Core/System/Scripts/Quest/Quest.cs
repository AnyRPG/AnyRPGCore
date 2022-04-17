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
        public event System.Action OnQuestObjectiveStatusUpdated = delegate { };

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

        /*
        public virtual CollectObjective[] MyCollectObjectives { get => deprecatedCollectObjectives; set => deprecatedCollectObjectives = value; }
        public virtual KillObjective[] MyKillObjectives { get => deprecatedKillObjectives; set => deprecatedKillObjectives = value; }
        public virtual TradeSkillObjective[] MyTradeSkillObjectives { get => deprecatedTradeSkillObjectives; set => deprecatedTradeSkillObjectives = value; }
        public virtual AbilityObjective[] MyAbilityObjectives { get => deprecatedAbilityObjectives; set => deprecatedAbilityObjectives = value; }
        public virtual UseInteractableObjective[] MyUseInteractableObjectives { get => deprecatedUseInteractableObjectives; set => deprecatedUseInteractableObjectives = value; }
        public virtual QuestQuestObjective[] MyQuestQuestObjectives { get => deprecatedQuestQuestObjectives; set => deprecatedQuestQuestObjectives = value; }
        public virtual DialogObjective[] MyDialogObjectives { get => deprecatedDialogObjectives; set => deprecatedDialogObjectives = value; }
        public virtual VisitZoneObjective[] VisitZoneObjectives { get => deprecatedVisitZoneObjectives; set => deprecatedVisitZoneObjectives = value; }
        */

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

                if (steps.Count > 0) {
                    foreach (QuestObjective questObjective in steps[steps.Count -1].QuestObjectives) {
                        if (!questObjective.IsComplete) {
                            return false;
                        }
                    }
                }

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
                saveManager.QuestSaveDataDictionary[saveData.QuestName] = saveData;
            }
        }

        public virtual void SetTurnedIn(bool turnedIn, bool notify = true) {
            this.TurnedIn = turnedIn;
            //Debug.Log(DisplayName + ".Quest.TurnedIn = " + value);
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

        public virtual bool PrerequisitesMet {
            get {
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
                saveManager.SetQuestSaveData(saveData.QuestName, saveData);
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
                saveManager.SetQuestSaveData(saveData.QuestName, saveData);
            }
        }

        public List<QuestStep> Steps { get => steps; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            playerManager = systemGameManager.PlayerManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            questLog = systemGameManager.QuestLog;
        }

        public virtual void RemoveQuest(bool resetQuestStep = true) {
            //Debug.Log("Quest.RemoveQuest(): " + DisplayName + " calling OnQuestStatusUpdated()");


            OnAbandonQuest();

            // reset the quest objective save data so any completed portion is reset in case the quest is picked back up
            saveManager.ResetQuestObjectiveSaveData(DisplayName);

            // reset current step so the correct objective shows up in the quest giver window when the quest is picked back up
            if (resetQuestStep == true) {
                CurrentStep = 0;
            }
            MarkedComplete = false;

            if (playerManager != null && playerManager.PlayerUnitSpawned == false) {
                // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                return;
            }
            SystemEventManager.TriggerEvent("OnQuestStatusUpdated", new EventParamProperties());
            SystemEventManager.TriggerEvent("OnAfterQuestStatusUpdated", new EventParamProperties());
            OnQuestStatusUpdated();
        }

        public virtual void HandInItems() {
            if (turnInItems == true) {
                if (steps.Count > 0) {
                    foreach (QuestObjective questObjective in steps[saveManager.GetQuestSaveData(this).questStep].QuestObjectives) {
                        if ((questObjective as CollectObjective) is CollectObjective) {
                            (questObjective as CollectObjective).Complete();
                        }
                    }
                }
            }
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
            if (steps.Count > 0) {
                foreach (QuestObjective questObjective in steps[CurrentStep].QuestObjectives) {
                    questObjective.OnAbandonQuest();
                }
            }
           
        }

        public virtual string GetStatus() {
            //Debug.Log(DisplayName + ".Quest.GetStatus()");

            string returnString = string.Empty;

            if (TurnedIn && !repeatableQuest) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning completed");
                return "completed";
            }

            if (questLog.HasQuest(DisplayName) && IsComplete) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning complete");
                return "complete";
            }

            if (questLog.HasQuest(DisplayName)) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning inprogress");
                return "inprogress";
            }

            if (!questLog.HasQuest(DisplayName) && (TurnedIn == false || RepeatableQuest == true) && PrerequisitesMet == true) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning available");
                return "available";
            }

            // this quest prerequisites were not met
            //Debug.Log(DisplayName + ".Quest.GetStatus(): returning unavailable");
            return "unavailable";
        }

        public override string GetDescription() {
            //return string.Format("{0}\n{1} Points", description, baseExperienceReward);
            return string.Format("{0}", description);
        }

        public virtual string GetObjectiveDescription() {

            Color titleColor = LevelEquations.GetTargetColor(playerManager.MyCharacter.CharacterStats.Level, ExperienceLevel);
            return string.Format("<size=30><b><color=#{0}>{1}</color></b></size>\n\n<size=18>{2}</size>\n\n<b><size=24>Objectives:</size></b>\n\n<size=18>{3}</size>", ColorUtility.ToHtmlStringRGB(titleColor), DisplayName, Description, GetUnformattedObjectiveList());

        }

        public virtual string GetUnformattedObjectiveList() {
            string objectives = string.Empty;
            List<string> objectiveList = new List<string>();
            if (steps.Count > 0) {
                foreach (QuestObjective questObjective in steps[saveManager.GetQuestSaveData(this).questStep].QuestObjectives) {
                    objectiveList.Add(questObjective.GetUnformattedStatus());
                }
            }
            objectives = string.Join("\n", objectiveList);
            if (objectives == string.Empty) {
                objectives = DisplayName;
            }
            return objectives;
        }

        public virtual void AcceptQuest(bool printMessages = true, bool resetStep = true) {
            QuestSaveData questSaveData = saveManager.GetQuestSaveData(this);
            if (resetStep == true) {
                questSaveData.questStep = 0;
            }
            questSaveData.markedComplete = false;
            questSaveData.turnedIn = false;
            saveManager.SetQuestSaveData(DisplayName, questSaveData);
            if (steps.Count > 0) {
                foreach (QuestObjective questObjective in steps[CurrentStep].QuestObjectives) {
                    questObjective.OnAcceptQuest(this, printMessages);
                }
            }

            if (isAchievement == false && printMessages == true) {
                messageFeedManager.WriteMessage("Quest Accepted: " + DisplayName);
            }
            // this next statement seems unnecessary.  is it a holdover from when quests were cloned ?
            // disable for now and see if anything breaks
            //if (!MarkedComplete) {
                // needs to be done here if quest wasn't auto-completed in checkcompletion
                if (playerManager != null && playerManager.PlayerUnitSpawned == false) {
                    // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                    return;
                }
                SystemEventManager.TriggerEvent("OnQuestStatusUpdated", new EventParamProperties());
                SystemEventManager.TriggerEvent("OnAfterQuestStatusUpdated", new EventParamProperties());
                OnQuestStatusUpdated();
            //}
        }

        public virtual void CheckCompletion(bool notifyOnUpdate = true, bool printMessages = true) {
            //Debug.Log("QuestLog.CheckCompletion()");
            if (MarkedComplete) {
                // no need to waste cycles checking, we are already done
                return;
            }
            bool questComplete = true;

            if (steps.Count > 0) {
                for (int i = CurrentStep; i < steps.Count; i++) {
                    // advance current step to ensure quest tracker and log show proper objectives
                    if (CurrentStep != i) {
                        CurrentStep = i;

                        // unsubscribe the previous step objectives
                        foreach (QuestObjective questObjective in steps[i-1].QuestObjectives) {
                            questObjective.OnAbandonQuest();
                        }

                        // reset save data from this step in case the next step contains an objective of the same type, but different amount
                        saveManager.ResetQuestObjectiveSaveData(DisplayName);

                        // subscribe the current step objectives
                        foreach (QuestObjective questObjective in steps[i].QuestObjectives) {
                            questObjective.OnAcceptQuest(this, printMessages);
                        }
                    }
                    foreach (QuestObjective questObjective in steps[i].QuestObjectives) {
                        if (!questObjective.IsComplete) {
                            questComplete = false;
                            break;
                        }
                    }
                    if (questComplete == false) {
                        break;
                    }
                }
            }

            if (questComplete) {
                CheckMarkComplete(notifyOnUpdate, printMessages);
            } else {
                // since this method only gets called as a result of a quest objective status updating, we need to notify for that at minimum
                //Debug.Log(DisplayName + ".Quest.CheckCompletion(): about to notify for objective status updated");
                SystemEventManager.TriggerEvent("OnQuestObjectiveStatusUpdated", new EventParamProperties());
                OnQuestObjectiveStatusUpdated();
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