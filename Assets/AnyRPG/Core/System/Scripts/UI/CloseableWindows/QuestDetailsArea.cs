using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class QuestDetailsArea : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI questDescription = null;

        [SerializeField]
        private TextMeshProUGUI experienceReward = null;

        [SerializeField]
        private GameObject currencyHeading = null;

        [SerializeField]
        private GameObject currencyArea = null;

        [SerializeField]
        private GameObject itemsHeading = null;

        [SerializeField]
        private GameObject itemIconsArea = null;

        [SerializeField]
        private UINavigationGrid itemGrid = null;

        [SerializeField]
        private GameObject factionsHeading = null;

        [SerializeField]
        private GameObject factionIconsArea = null;

        [SerializeField]
        private UINavigationGrid factionGrid = null;

        [SerializeField]
        private GameObject abilitiesHeading = null;

        [SerializeField]
        private GameObject abilityIconsArea = null;

        [SerializeField]
        private UINavigationGrid abilityGrid = null;

        [SerializeField]
        private GameObject skillHeading = null;

        [SerializeField]
        private GameObject skillIconsArea = null;

        [SerializeField]
        private UINavigationGrid skillGrid = null;

        [SerializeField]
        private LootButton currencyLootButton = null;

        [SerializeField]
        private GameObject rewardIconPrefab = null;

        [SerializeField]
        private GameObject factionRewardIconPrefab = null;

        protected CloseableWindowContents owner = null;

        private Quest quest = null;

        private List<RewardButton> itemRewardIcons = new List<RewardButton>();
        private List<RewardButton> abilityRewardIcons = new List<RewardButton>();
        private List<RewardButton> skillRewardIcons = new List<RewardButton>();
        private List<FactionRewardButton> factionRewardIcons = new List<FactionRewardButton>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private PlayerManager playerManager = null;
        private CurrencyConverter currencyConverter = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            currencyLootButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            objectPooler = systemGameManager.ObjectPooler;
            playerManager = systemGameManager.PlayerManager;
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        public void SetOwner(CloseableWindowContents closeableWindowContents) {
            //Debug.Log("QuestDetailsArea.SetOwner()");

            owner = closeableWindowContents;
        }


        public List<RewardButton> GetHighlightedItemRewardIcons() {
            //Debug.Log("QuestDetailsArea.GetHighlightedItemRewardIcons()");
            List<RewardButton> returnList = new List<RewardButton>();
            foreach (RewardButton rewardButton in itemRewardIcons) {
                //Debug.Log("QuestDetailsArea.GetHighlightedItemRewardIcons(): passing over rewardbutton");
                if (rewardButton.isActiveAndEnabled == true && (rewardButton.Chosen == true || quest.MaxItemRewards == 0)) {
                    //Debug.Log("QuestDetailsArea.GetHighlightedItemRewardIcons(): adding button to the list");
                    returnList.Add(rewardButton);
                }
            }
            return returnList;
        }

        public List<RewardButton> GetHighlightedAbilityRewardIcons() {
            //Debug.Log("QuestDetailsArea.GetHighlightedAbilityRewardIcons()");
            List<RewardButton> returnList = new List<RewardButton>();
            foreach (RewardButton rewardButton in abilityRewardIcons) {
                //Debug.Log("QuestDetailsArea.GetHighlightedAbilityRewardIcons(): passing over rewardbutton");
                if (rewardButton.isActiveAndEnabled == true && (rewardButton.Chosen == true || quest.MaxAbilityRewards == 0)) {
                    //Debug.Log("QuestDetailsArea.GetHighlightedAbilityRewardIcons(): adding button to the list");
                    returnList.Add(rewardButton);
                }
            }
            //Debug.Log("QuestDetailsArea.GetHighlightedAbilityRewardIcons(): listCount: " + returnList.Count);
            return returnList;
        }

        public List<RewardButton> GetHighlightedSkillRewardIcons() {
            //Debug.Log("QuestDetailsArea.GetHighlightedSkillRewardIcons()");
            List<RewardButton> returnList = new List<RewardButton>();
            foreach (RewardButton rewardButton in skillRewardIcons) {
                //Debug.Log("QuestDetailsArea.GetHighlightedSkillRewardIcons(): passing over rewardbutton");
                if (rewardButton.isActiveAndEnabled == true && (rewardButton.Chosen == true || quest.MaxSkillRewards == 0)) {
                    //Debug.Log("QuestDetailsArea.GetHighlightedSkillRewardIcons(): adding button to the list");
                    returnList.Add(rewardButton);
                }
            }
            //Debug.Log("QuestDetailsArea.GetHighlightedAbilityRewardIcons(): listCount: " + returnList.Count);
            return returnList;
        }

        public List<RewardButton> GetHighlightedFactionRewardIcons() {
            //Debug.Log("QuestDetailsArea.GetHighlightedFactionRewardIcons()");
            List<RewardButton> returnList = new List<RewardButton>();
            foreach (RewardButton rewardButton in factionRewardIcons) {
                //Debug.Log("QuestDetailsArea.GetHighlightedFactionRewardIcons(): passing over rewardbutton");
                if (rewardButton.isActiveAndEnabled == true && (rewardButton.Chosen == true || quest.MaxFactionRewards == 0)) {
                    //Debug.Log("QuestDetailsArea.GetHighlightedFactionRewardIcons(): adding button to the list");
                    returnList.Add(rewardButton);
                }
            }
            return returnList;
        }

        public void HandleAttemptSelect(RewardButton rewardButton) {
            //Debug.Log("QuestDetailsArea.HandleAttemptSelect()");
            if (GetHighlightedItemRewardIcons().Contains(rewardButton)) {
                //Debug.Log("QuestDetailsArea.HandleAttemptSelect(): it's an item reward; current count of highlighted icons: " + GetHighlightedItemRewardIcons().Count + "; max: " + quest.MyMaxItemRewards);
                if (quest.MaxItemRewards == 0
                    || (quest.MaxItemRewards > 0 && GetHighlightedItemRewardIcons().Count > quest.MaxItemRewards)) {
                    rewardButton.Unselect();
                }
            }
            if (GetHighlightedFactionRewardIcons().Contains(rewardButton)) {
                //Debug.Log("QuestDetailsArea.HandleAttemptSelect(): it's an faction reward; current count of highlighted icons: " + GetHighlightedFactionRewardIcons().Count + "; max: " + quest.MyMaxFactionRewards);
                if (quest.MaxFactionRewards == 0
                    || (quest.MaxFactionRewards > 0 && GetHighlightedFactionRewardIcons().Count > quest.MaxFactionRewards)) {
                    rewardButton.Unselect();
                }
            }

            if (GetHighlightedAbilityRewardIcons().Contains(rewardButton)) {
                //Debug.Log("QuestDetailsArea.HandleAttemptSelect(): it's an ability reward; current count of highlighted icons: " + GetHighlightedAbilityRewardIcons().Count + "; max: " + quest.MyMaxAbilityRewards);
                if (quest.MaxAbilityRewards == 0
                    || (quest.MaxAbilityRewards > 0 && GetHighlightedAbilityRewardIcons().Count > quest.MaxAbilityRewards)
                    || rewardButton.Rewardable.HasReward() == true) {
                    rewardButton.Unselect();
                }
            }

            if (GetHighlightedSkillRewardIcons().Contains(rewardButton)) {
                //Debug.Log("QuestDetailsArea.HandleAttemptSelect(): it's an ability reward; current count of highlighted icons: " + GetHighlightedAbilityRewardIcons().Count + "; max: " + quest.MyMaxAbilityRewards);
                if (quest.MaxSkillRewards == 0
                    || (quest.MaxSkillRewards > 0 && GetHighlightedSkillRewardIcons().Count > quest.MaxSkillRewards)
                    || rewardButton.Rewardable.HasReward() == true) {
                    rewardButton.Unselect();
                }
            }
        }


        public void ShowDescription(Quest quest) {
            //Debug.Log("QuestDetailsArea.ShowDescription(" + (quest == null ? "null" : quest.DisplayName) + ")");

            ClearDescription();

            if (quest == null) {
                return;
            }
            this.quest = quest;

            questDescription.text = quest.GetObjectiveDescription();

            experienceReward.text += LevelEquations.GetXPAmountForQuest(playerManager.MyCharacter.CharacterStats.Level, quest, systemConfigurationManager) + " XP";

            // display currency rewards

            List<CurrencyNode> currencyNodes = quest.GetCurrencyReward();

            // currencies could be different
            if (currencyNodes.Count > 0) {
                currencyHeading.gameObject.SetActive(true);
                currencyArea.gameObject.SetActive(true);
                if (currencyLootButton != null) {
                    KeyValuePair<Sprite, string> keyValuePair = currencyConverter.RecalculateValues(currencyNodes, true);
                    currencyLootButton.Icon.sprite = keyValuePair.Key;
                    currencyLootButton.Title.text = keyValuePair.Value;
                }
            } else {
                currencyHeading.gameObject.SetActive(false);
                currencyArea.gameObject.SetActive(false);
            }


            // show item rewards
            if (quest.ItemRewards.Count > 0) {
                itemsHeading.gameObject.SetActive(true);
                if (quest.MaxItemRewards > 0) {
                    itemsHeading.GetComponent<TextMeshProUGUI>().text = "Choose " + quest.MaxItemRewards + " Item Rewards:";
                } else {
                    itemsHeading.GetComponent<TextMeshProUGUI>().text = "Item Rewards:";
                }
            }
            for (int i = 0; i < quest.ItemRewards.Count; i++) {
                RewardButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, itemIconsArea.transform).GetComponent<RewardButton>();
                rewardIcon.Configure(systemGameManager);
                rewardIcon.SetOptions(owner.RectTransform, quest.MaxItemRewards > 0);
                rewardIcon.OnAttempSelect += HandleAttemptSelect;
                //Debug.Log("QuestDetailsArea.ShowDescription(): setting describable (and attemptselect) for: " + quest.MyItemRewards[i]);
                rewardIcon.SetReward(quest.ItemRewards[i]);
                itemRewardIcons.Add(rewardIcon);
                //if (quest.MaxItemRewards > 0) {
                    itemGrid.AddActiveButton(rewardIcon);
                //}
            }

            // show ability rewards
            if (quest.AbilityRewards.Count > 0) {
                abilitiesHeading.gameObject.SetActive(true);
                if (quest.MaxAbilityRewards > 0) {
                    abilitiesHeading.GetComponent<TextMeshProUGUI>().text = "Choose " + quest.MaxAbilityRewards + " Ability Rewards:";
                } else {
                    abilitiesHeading.GetComponent<TextMeshProUGUI>().text = "Ability Rewards:";
                }
            } else {
                abilitiesHeading.GetComponent<TextMeshProUGUI>().text = "";
            }
            for (int i = 0; i < quest.AbilityRewards.Count; i++) {
                RewardButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                rewardIcon.Configure(systemGameManager);
                rewardIcon.SetOptions(owner.RectTransform, quest.MaxAbilityRewards > 0);
                rewardIcon.OnAttempSelect += HandleAttemptSelect;
                //Debug.Log("QuestDetailsArea.ShowDescription(): setting describable (and attemptselect) for: " + quest.MyAbilityRewards[i]);
                rewardIcon.SetReward(quest.AbilityRewards[i].AbilityProperties);
                abilityRewardIcons.Add(rewardIcon);
                //if (quest.MaxAbilityRewards > 0) {
                    abilityGrid.AddActiveButton(rewardIcon);
                //}
            }

            // show faction rewards
            if (quest.FactionRewards.Count > 0) {
                factionsHeading.gameObject.SetActive(true);
                if (quest.MaxFactionRewards > 0) {
                    factionsHeading.GetComponent<TextMeshProUGUI>().text = "Choose " + quest.MaxFactionRewards + " Reputation Rewards:";
                } else {
                    factionsHeading.GetComponent<TextMeshProUGUI>().text = "Reputation Rewards:";
                }
            } else {
                factionsHeading.GetComponent<TextMeshProUGUI>().text = "";
            }
            for (int i = 0; i < quest.FactionRewards.Count; i++) {
                FactionRewardButton rewardIcon = objectPooler.GetPooledObject(factionRewardIconPrefab, factionIconsArea.transform).GetComponent<FactionRewardButton>();
                rewardIcon.Configure(systemGameManager);
                rewardIcon.SetOptions(owner.RectTransform, quest.MaxFactionRewards > 0);
                rewardIcon.OnAttempSelect += HandleAttemptSelect;
                //Debug.Log("QuestDetailsArea.ShowDescription(): setting describable (and attemptselect) for: " + quest.MyFactionRewards[i]);
                rewardIcon.SetReward(quest.FactionRewards[i]);
                factionRewardIcons.Add(rewardIcon);
                //if (quest.MaxFactionRewards > 0) {
                    factionGrid.AddActiveButton(rewardIcon);
                //}
            }

            // show Skill rewards
            if (quest.SkillRewards.Count > 0) {
                skillHeading.gameObject.SetActive(true);
                if (quest.MaxSkillRewards > 0) {
                    skillHeading.GetComponent<TextMeshProUGUI>().text = "Choose " + quest.MaxSkillRewards + " Skill Rewards:";
                } else {
                    skillHeading.GetComponent<TextMeshProUGUI>().text = "Skill Rewards:";
                }
            } else {
                skillHeading.GetComponent<TextMeshProUGUI>().text = "";
            }
            for (int i = 0; i < quest.SkillRewards.Count; i++) {
                RewardButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, skillIconsArea.transform).GetComponent<RewardButton>();
                rewardIcon.Configure(systemGameManager);
                rewardIcon.SetOptions(owner.RectTransform, quest.MaxSkillRewards > 0);
                rewardIcon.SetReward(quest.SkillRewards[i]);
                skillRewardIcons.Add(rewardIcon);
                //if (quest.MaxSkillRewards > 0) {
                    skillGrid.AddActiveButton(rewardIcon);
                //}
            }

        }

        public void ClearDescription() {
            //Debug.Log("QuestDetailsArea.ClearDescription()");

            questDescription.text = string.Empty;
            experienceReward.text = "Experience: ";
            itemsHeading.gameObject.SetActive(false);
            abilitiesHeading.gameObject.SetActive(false);
            skillHeading.gameObject.SetActive(false);
            factionsHeading.gameObject.SetActive(false);
            ClearRewardIcons();
        }

        private void ClearRewardIcons() {
            //Debug.Log("QuestDetailsArea.ClearRewardIcons()");

            // items
            foreach (RewardButton rewardIcon in itemRewardIcons) {
                rewardIcon.Unselect();
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            itemRewardIcons.Clear();
            itemGrid.ClearActiveButtons();

            // abilties
            foreach (RewardButton rewardIcon in abilityRewardIcons) {
                rewardIcon.Unselect();
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
            abilityGrid.ClearActiveButtons();

            // skills
            foreach (RewardButton rewardIcon in skillRewardIcons) {
                rewardIcon.Unselect();
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            skillRewardIcons.Clear();
            skillGrid.ClearActiveButtons();

            // factions
            foreach (RewardButton rewardIcon in factionRewardIcons) {
                rewardIcon.Unselect();
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            factionRewardIcons.Clear();
            factionGrid.ClearActiveButtons();
        }

    }

}