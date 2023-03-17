using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Quest", menuName = "AnyRPG/Quest")]
    public class Quest : QuestBase {

        [Header("Quest")]

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

        [Header("Completion")]

        [Tooltip("Whether or not to give the items to the questgiver when you turn in a quest.  If false, you keep the items in your bag.")]
        [SerializeField]
        protected bool turnInItems;

        [Tooltip("the player can complete the quest without having the quest in the questlog")]
        [SerializeField]
        protected bool allowRawComplete = false;

        // game manager references
        protected QuestLog questLog = null;

        public override bool PrintObjectiveCompletionMessages {
            get => true;
        }

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
        public virtual bool HasOpeningDialog { get => hasOpeningDialog; set => hasOpeningDialog = value; }
        public virtual Dialog OpeningDialog { get => openingDialog; set => openingDialog = value; }
        public virtual int ExperienceRewardPerLevel { get => experienceRewardPerLevel; set => experienceRewardPerLevel = value; }
        public virtual int BaseExperienceReward { get => baseExperienceReward; set => baseExperienceReward = value; }
        public virtual bool AutomaticCurrencyReward { get => automaticCurrencyReward; set => automaticCurrencyReward = value; }
        public virtual string RewardCurrencyName { get => rewardCurrencyName; set => rewardCurrencyName = value; }
        public virtual Currency RewardCurrency { get => rewardCurrency; set => rewardCurrency = value; }
        public virtual int BaseCurrencyReward { get => baseCurrencyReward; set => baseCurrencyReward = value; }
        public virtual int CurrencyRewardPerLevel { get => currencyRewardPerLevel; set => currencyRewardPerLevel = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            questLog = systemGameManager.QuestLog;
        }

        public virtual void HandInItems() {
            if (turnInItems == true) {
                if (steps.Count > 0) {
                    foreach (QuestObjective questObjective in steps[GetSaveData().questStep].QuestObjectives) {
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

        protected override void ProcessMarkComplete(bool printMessages) {
            base.ProcessMarkComplete(printMessages);
            if (printMessages == true) {
                messageFeedManager.WriteMessage(string.Format("{0} Complete!", DisplayName));
            }
        }

        protected override void ResetObjectiveSaveData() {
            saveManager.ResetQuestObjectiveSaveData(ResourceName);
        }

        protected override QuestSaveData GetSaveData() {
            return saveManager.GetQuestSaveData(this);
        }

        protected override void SetSaveData(string questName, QuestSaveData questSaveData) {
            saveManager.SetQuestSaveData(questName, questSaveData);
        }

        protected override bool HasQuest() {
            return questLog.HasQuest(ResourceName);
        }

        protected override bool StatusAvailable() {
            if (base.StatusAvailable() == false) {
                return false;
            }

            if (TurnedIn == false || RepeatableQuest == true) {
                return true;
            }

            return false;
        }

        protected override bool StatusCompleted() {
            if (repeatableQuest == true) {
                return false;
            }

            return base.StatusCompleted();
        }

        protected override Color GetTitleColor() {
            return LevelEquations.GetTargetColor(playerManager.MyCharacter.CharacterStats.Level, ExperienceLevel);
        }

        protected override void ProcessAcceptQuest() {
            base.ProcessAcceptQuest();
            messageFeedManager.WriteMessage("Quest Accepted: " + DisplayName);
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
                        Debug.LogError("Quest.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("Quest.SetupScriptableObjects(): Could not find skill : " + skillName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("Quest.SetupScriptableObjects(): Could not find item : " + itemName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
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
                Dialog dialog = systemDataFactory.GetResource<Dialog>(ResourceName);
                if (dialog != null) {
                    openingDialog = dialog;
                } else {
                    Debug.LogError("Quest.SetupScriptableObjects(): Could not find dialog : " + ResourceName + " while inititalizing quest " + ResourceName + ".  CHECK INSPECTOR");
                }
            }
        }

    }
   
}