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
        [ResourceSelector(resourceType = typeof(Ability))]
        protected List<string> abilityRewardNames = new List<string>();

        protected List<Ability> abilityRewardList = new List<Ability>();

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
        protected MessageLogServer messageLogServer = null;
        protected CurrencyConverter currencyConverter = null;

        public override bool PrintObjectiveCompletionMessages {
            get => true;
        }

        public virtual List<Item> ItemRewards { get => itemRewardList; }
        public virtual List<FactionNode> FactionRewards { get => factionRewards; }
        public virtual List<Ability> AbilityRewards { get => abilityRewardList; }
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
            messageLogServer = systemGameManager.MessageLogServer;
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        public virtual int ExperienceLevel(UnitController sourceUnitController) { 
            return (dynamicLevel == true ? sourceUnitController.CharacterStats.Level : experienceLevel) + extraLevels;
        }

        public virtual void HandInItems(UnitController sourceUnitController) {
            if (turnInItems == true) {
                if (steps.Count > 0) {
                    foreach (QuestObjective questObjective in steps[GetSaveData(sourceUnitController).QuestStep].QuestObjectives) {
                        if ((questObjective as CollectObjective) is CollectObjective) {
                            (questObjective as CollectObjective).Complete(sourceUnitController);
                        }
                    }
                }
            }
        }

        public virtual List<CurrencyNode> GetCurrencyReward(UnitController sourceUnitController) {
            List<CurrencyNode> currencyNodes = new List<CurrencyNode>();

            if (AutomaticCurrencyReward == true) {
                if (systemConfigurationManager.QuestCurrency != null) {
                    CurrencyNode currencyNode = new CurrencyNode();
                    currencyNode.currency = systemConfigurationManager.QuestCurrency;
                    currencyNode.Amount = systemConfigurationManager.QuestCurrencyAmountPerLevel * ExperienceLevel(sourceUnitController);
                    currencyNodes.Add(currencyNode);
                }
            }
            if (RewardCurrency != null) {
                CurrencyNode currencyNode = new CurrencyNode();
                currencyNode.Amount = BaseCurrencyReward + (CurrencyRewardPerLevel * ExperienceLevel(sourceUnitController));
                currencyNode.currency = RewardCurrency;
                currencyNodes.Add(currencyNode);
            }

            return currencyNodes;
        }

        protected override void ProcessMarkComplete(UnitController sourceUnitController, bool printMessages) {
            base.ProcessMarkComplete(sourceUnitController, printMessages);
            if (printMessages == true) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("{0} Complete!", DisplayName));
            }
        }

        protected override QuestSaveData GetSaveData(UnitController sourceUnitController) {
            return sourceUnitController.CharacterQuestLog.GetQuestSaveData(this);
        }

        protected override void SetSaveData(UnitController sourceUnitController, string questName, QuestSaveData questSaveData) {
            sourceUnitController.CharacterQuestLog.SetQuestSaveData(questName, questSaveData);
        }

        protected override bool HasQuest(UnitController sourceUnitController) {
            return sourceUnitController.CharacterQuestLog.HasQuest(ResourceName);
        }

        protected override bool StatusAvailable(UnitController sourceUnitController) {
            if (base.StatusAvailable(sourceUnitController) == false) {
                return false;
            }

            if (TurnedIn(sourceUnitController) == false || RepeatableQuest == true) {
                return true;
            }

            return false;
        }

        protected override bool StatusCompleted(UnitController sourceUnitController) {
            if (repeatableQuest == true) {
                return false;
            }

            return base.StatusCompleted(sourceUnitController);
        }

        protected override Color GetTitleColor() {
            return LevelEquations.GetTargetColor(playerManager.UnitController.CharacterStats.Level, ExperienceLevel(playerManager.UnitController));
        }

        protected override void ProcessAcceptQuest(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.Quest.ProcessAcceptQuest({sourceUnitController.gameObject.name})");

            base.ProcessAcceptQuest(sourceUnitController);
            sourceUnitController.WriteMessageFeedMessage($"Quest Accepted: {DisplayName}");
        }

        public override int GetObjectiveCurrentAmount(UnitController sourceUnitController, string objectiveTypeName, string objectiveName) {
            return sourceUnitController.CharacterQuestLog.GetQuestObjectiveSaveData(ResourceName, objectiveTypeName, objectiveName).Amount;
        }

        public override void SetObjectiveCurrentAmount(UnitController sourceUnitController, string objectiveTypeName, string objectiveName, int amount) {
            sourceUnitController.CharacterQuestLog.SetQuestObjectiveCurrentAmount(ResourceName, objectiveTypeName, objectiveName, amount);
        }

        public override void ResetObjectiveSaveData(UnitController sourceUnitController) {
            sourceUnitController.CharacterQuestLog.ResetQuestObjectiveSaveData(ResourceName);
        }

        public override void NotifyOnObjectiveStatusUpdated(UnitController sourceUnitController) {
            sourceUnitController.UnitEventController.NotifyOnQuestObjectiveStatusUpdated(this);
        }

        public override void NotifyOnMarkComplete(UnitController sourceUnitController) {
            sourceUnitController.UnitEventController.NotifyOnMarkQuestComplete(this);
        }

        public override void NotifyOnAcceptQuest(UnitController sourceUnitController) {
            sourceUnitController.UnitEventController.NotifyOnAcceptQuest(this);
        }

        public override void RemoveQuest(UnitController sourceUnitController, bool resetQuestStep = true) {
            base.RemoveQuest(sourceUnitController, resetQuestStep);

            SetMarkedComplete(sourceUnitController, false);

            NotifyOnQuestBaseStatusUpdated(sourceUnitController);

        }

        public override void SetCurrentStep(UnitController sourceUnitController, int value) {
            base.SetCurrentStep(sourceUnitController, value);
            sourceUnitController.UnitEventController.NotifyOnQuestObjectiveStatusUpdated(this);
        }

        public void CompleteQuest(UnitController sourceUnitController, QuestRewardChoices questRewardChoices) {
            //Debug.Log("QuestGiverUI.CompleteQuest()");
            if (!IsComplete(sourceUnitController)) {
                Debug.LogWarning("QuestGiverManager.CompleteQuest(): currentQuest is not complete, exiting!");
                return;
            }

            bool itemCountMatches = false;
            bool abilityCountMatches = false;
            bool factionCountMatches = false;
            bool skillCountMatches = false;
            if (ItemRewards.Count == 0 || MaxItemRewards == 0 || MaxItemRewards == questRewardChoices.itemRewardIndexes.Count) {
                itemCountMatches = true;
            }
            if (FactionRewards.Count == 0 || MaxFactionRewards == 0 || MaxFactionRewards == questRewardChoices.factionRewardIndexes.Count) {
                factionCountMatches = true;
            }
            if (AbilityRewards.Count == 0 || MaxAbilityRewards == 0 || MaxAbilityRewards == questRewardChoices.abilityRewardIndexes.Count) {
                abilityCountMatches = true;
            }
            if (SkillRewards.Count == 0 || MaxSkillRewards == 0 || MaxSkillRewards == questRewardChoices.skillRewardIndexes.Count) {
                skillCountMatches = true;
            }

            if (!itemCountMatches || !abilityCountMatches || !factionCountMatches || !skillCountMatches) {
                sourceUnitController.WriteMessageFeedMessage("You must choose rewards before turning in this quest");
                return;
            }

            // currency rewards
            List<CurrencyNode> currencyNodes = GetCurrencyReward(sourceUnitController);
            foreach (CurrencyNode currencyNode in currencyNodes) {
                sourceUnitController.CharacterCurrencyManager.AddCurrency(currencyNode.currency, currencyNode.Amount);
                List<CurrencyNode> tmpCurrencyNode = new List<CurrencyNode>();
                tmpCurrencyNode.Add(currencyNode);
                messageLogServer.WriteSystemMessage(sourceUnitController, $"Gained {currencyConverter.RecalculateValues(tmpCurrencyNode, false).Value.Replace("\n", ", ")}");
            }

            // item rewards first in case not enough space in inventory
            // TO FIX: THIS CODE DOES NOT DEAL WITH PARTIAL STACKS AND WILL REQUEST ONE FULL SLOT FOR EVERY REWARD
            if (questRewardChoices.itemRewardIndexes.Count > 0) {
                if (sourceUnitController.CharacterInventoryManager.EmptySlotCount() < questRewardChoices.itemRewardIndexes.Count) {
                    sourceUnitController.WriteMessageFeedMessage("Not enough room in inventory!");
                    return;
                }
                foreach (int rewardIndex in questRewardChoices.itemRewardIndexes) {
                    ItemRewards[rewardIndex].GiveReward(sourceUnitController);
                }
                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedItemRewardIcons()) {
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(sourceUnitController);
                    }
                }
                */
            }

            HandInItems(sourceUnitController);

            // faction rewards
            if (questRewardChoices.factionRewardIndexes.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Faction Rewards");
                foreach (int rewardIndex in questRewardChoices.factionRewardIndexes) {
                    FactionRewards[rewardIndex].GiveReward(sourceUnitController);
                }
                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedFactionRewardIcons()) {
                    //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Faction Rewards: got a reward button!");
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(sourceUnitController);
                    }
                }
                */
            }

            // ability rewards
            if (questRewardChoices.abilityRewardIndexes.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Ability Rewards");
                foreach (int rewardIndex in questRewardChoices.abilityRewardIndexes) {
                    AbilityRewards[rewardIndex].AbilityProperties.GiveReward(sourceUnitController);
                }
                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedAbilityRewardIcons()) {
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(sourceUnitController);
                    }
                }
                */
            }

            // skill rewards
            if (questRewardChoices.skillRewardIndexes.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Skill Rewards");
                foreach (int rewardIndex in questRewardChoices.skillRewardIndexes) {
                    SkillRewards[rewardIndex].GiveReward(sourceUnitController);
                }

                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedSkillRewardIcons()) {
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(sourceUnitController);
                    }
                }
                */
            }

            // xp reward
            sourceUnitController.CharacterStats.GainXP(LevelEquations.GetXPAmountForQuest(sourceUnitController, this, systemConfigurationManager));

            //UpdateButtons(currentQuest);

            // DO THIS HERE OR TURNING THE QUEST RESULTING IN THIS WINDOW RE-OPENING WOULD JUST INSTA-CLOSE IT INSTEAD
            //uIManager.questGiverWindow.CloseWindow();

            sourceUnitController.CharacterQuestLog.TurnInQuest(this);

            /*
            // this is client side stuff, and mostly should be handled by other subscriptions
            // do this last
            // DO THIS AT THE END OR THERE WILL BE NO SELECTED QUESTGIVERQUESTSCRIPT
            if (questGiver != null) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): questGiver is not null");
                // MUST BE DONE IN CASE WINDOW WAS OPEN INBETWEEN SCENES BY ACCIDENT
                //Debug.Log("QuestGiverUI.CompleteQuest() Updating questGiver queststatus");
                questGiver.UpdateQuestStatus(sourceUnitController);
                questGiver.HandleCompleteQuest();
            } else {
                //Debug.Log("QuestGiverUI.CompleteQuest(): questGiver is null!");
            }
            */

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

            abilityRewardList = new List<Ability>();
            if (abilityRewardNames != null) {
                foreach (string baseAbilityName in abilityRewardNames) {
                    Ability baseAbility = systemDataFactory.GetResource<Ability>(baseAbilityName);
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