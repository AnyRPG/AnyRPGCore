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
        [ResourceSelector(resourceType = typeof(Currency))]
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
        [ResourceSelector(resourceType = typeof(Item))]
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
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        private List<string> abilityRewardNames = new List<string>();

        private List<BaseAbility> abilityRewardList = new List<BaseAbility>();

        [Header("Skill Rewards")]

        [SerializeField]
        private int maxSkillRewards = 0;

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Skill))]
        private List<string> skillRewardNames = new List<string>();

        private List<Skill> skillRewardList = new List<Skill>();

        [Header("Objectives")]

        [SerializeReference]
        [SerializeReferenceButton]
        private List<QuestObjective> baseObjectives = new List<QuestObjective>();

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

        // game manager references
        protected SaveManager saveManager = null;
        protected PlayerManager playerManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected QuestLog questLog = null;

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
                return saveManager.GetQuestSaveData(this).turnedIn;
                //return false;
            }
            set {
                QuestSaveData saveData = saveManager.GetQuestSaveData(this);
                saveData.turnedIn = value;
                saveManager.QuestSaveDataDictionary[saveData.MyName] = saveData;
            }
        }

        public void SetTurnedIn(bool turnedIn, bool notify = true) {
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

        public Quest QuestTemplate { get => questTemplate; set => questTemplate = value; }
        public int ExperienceLevel { get => ((dynamicLevel == true ? playerManager.MyCharacter.CharacterStats.Level : experienceLevel) + extraLevels); }

        public List<Item> ItemRewards { get => itemRewardList; }
        public List<FactionNode> FactionRewards { get => factionRewards; }
        public List<BaseAbility> AbilityRewards { get => abilityRewardList; }
        public List<Skill> SkillRewards { get => skillRewardList; }

        public bool TurnInItems { get => turnInItems; set => turnInItems = value; }
        public bool AllowRawComplete { get => allowRawComplete; set => allowRawComplete = value; }
        public int MaxAbilityRewards { get => maxAbilityRewards; set => maxAbilityRewards = value; }
        public int MaxSkillRewards { get => maxSkillRewards; set => maxSkillRewards = value; }
        public int MaxItemRewards { get => maxItemRewards; set => maxItemRewards = value; }
        public int MaxFactionRewards { get => maxFactionRewards; set => maxFactionRewards = value; }
        public bool RepeatableQuest { get => repeatableQuest; set => repeatableQuest = value; }
        public bool IsAchievement { get => isAchievement; set => isAchievement = value; }
        public bool HasOpeningDialog { get => hasOpeningDialog; set => hasOpeningDialog = value; }
        public Dialog OpeningDialog { get => openingDialog; set => openingDialog = value; }
        public VisitZoneObjective[] VisitZoneObjectives { get => visitZoneObjectives; set => visitZoneObjectives = value; }
        public int ExperienceRewardPerLevel { get => experienceRewardPerLevel; set => experienceRewardPerLevel = value; }
        public int BaseExperienceReward { get => baseExperienceReward; set => baseExperienceReward = value; }
        public bool AutomaticCurrencyReward { get => automaticCurrencyReward; set => automaticCurrencyReward = value; }
        public string RewardCurrencyName { get => rewardCurrencyName; set => rewardCurrencyName = value; }
        public Currency RewardCurrency { get => rewardCurrency; set => rewardCurrency = value; }
        public int BaseCurrencyReward { get => baseCurrencyReward; set => baseCurrencyReward = value; }
        public int CurrencyRewardPerLevel { get => currencyRewardPerLevel; set => currencyRewardPerLevel = value; }
        public bool MarkedComplete {
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

        public void RemoveQuest() {
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

        public List<CurrencyNode> GetCurrencyReward() {
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

        public void CheckMarkComplete(bool notifyOnUpdate = true, bool printMessages = true) {
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

        public string GetObjectiveDescription() {

            Color titleColor = LevelEquations.GetTargetColor(playerManager.MyCharacter.CharacterStats.Level, ExperienceLevel);
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

        public void CheckCompletion(bool notifyOnUpdate = true, bool printMessages = true) {
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
        public void UpdatePrerequisites(bool notify = true) {
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

            foreach (QuestObjective objective in collectObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in killObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in tradeSkillObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in abilityObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in useInteractableObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in questQuestObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in dialogObjectives) {
                objective.SetupScriptableObjects(systemGameManager);
                objective.SetQuest(this);
            }
            foreach (QuestObjective objective in visitZoneObjectives) {
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

        public void HandlePrerequisiteUpdates() {
            OnQuestStatusUpdated();
        }
    }
}