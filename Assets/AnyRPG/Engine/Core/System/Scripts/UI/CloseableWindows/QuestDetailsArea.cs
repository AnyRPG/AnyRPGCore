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
        private GameObject factionsHeading = null;

        [SerializeField]
        private GameObject factionIconsArea = null;

        [SerializeField]
        private GameObject abilitiesHeading = null;

        [SerializeField]
        private GameObject abilityIconsArea = null;

        [SerializeField]
        private GameObject skillHeading = null;

        [SerializeField]
        private GameObject skillIconsArea = null;

        [SerializeField]
        private LootButton currencyLootButton = null;

        [SerializeField]
        private GameObject rewardIconPrefab = null;

        [SerializeField]
        private GameObject factionRewardIconPrefab = null;

        private Quest quest = null;

        private List<RewardButton> itemRewardIcons = new List<RewardButton>();
        private List<RewardButton> abilityRewardIcons = new List<RewardButton>();
        private List<RewardButton> skillRewardIcons = new List<RewardButton>();
        private List<FactionRewardButton> factionRewardIcons = new List<FactionRewardButton>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private PlayerManager playerManager = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            objectPooler = systemGameManager.ObjectPooler;
            playerManager = systemGameManager.PlayerManager;

            currencyLootButton.Init(systemGameManager);
        }


        public List<RewardButton> GetHighlightedItemRewardIcons() {
            //Debug.Log("QuestDetailsArea.GetHighlightedItemRewardIcons()");
            List<RewardButton> returnList = new List<RewardButton>();
            foreach (RewardButton rewardButton in itemRewardIcons) {
                //Debug.Log("QuestDetailsArea.GetHighlightedItemRewardIcons(): passing over rewardbutton");
                if (rewardButton.isActiveAndEnabled == true && (rewardButton.MySelected == true || quest.MaxItemRewards == 0)) {
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
                if (rewardButton.isActiveAndEnabled == true && (rewardButton.MySelected == true || quest.MaxAbilityRewards == 0)) {
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
                if (rewardButton.isActiveAndEnabled == true && (rewardButton.MySelected == true || quest.MaxSkillRewards == 0)) {
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
                if (rewardButton.isActiveAndEnabled == true && (rewardButton.MySelected == true || quest.MaxFactionRewards == 0)) {
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
                if (quest.MaxItemRewards > 0 && GetHighlightedItemRewardIcons().Count > quest.MaxItemRewards) {
                    rewardButton.Unselect();
                }
            }
            if (GetHighlightedFactionRewardIcons().Contains(rewardButton)) {
                //Debug.Log("QuestDetailsArea.HandleAttemptSelect(): it's an faction reward; current count of highlighted icons: " + GetHighlightedFactionRewardIcons().Count + "; max: " + quest.MyMaxFactionRewards);
                if (quest.MaxFactionRewards > 0 && GetHighlightedFactionRewardIcons().Count > quest.MaxFactionRewards) {
                    rewardButton.Unselect();
                }
            }

            if (GetHighlightedAbilityRewardIcons().Contains(rewardButton)) {
                //Debug.Log("QuestDetailsArea.HandleAttemptSelect(): it's an ability reward; current count of highlighted icons: " + GetHighlightedAbilityRewardIcons().Count + "; max: " + quest.MyMaxAbilityRewards);
                if (quest.MaxAbilityRewards > 0 && GetHighlightedAbilityRewardIcons().Count > quest.MaxAbilityRewards || playerManager.MyCharacter.CharacterAbilityManager.HasAbility(rewardButton.Describable as BaseAbility)) {
                    rewardButton.Unselect();
                }
            }
        }


        public void ShowDescription(Quest quest) {
            //Debug.Log("QuestDetailsArea.ShowDescription()");

            ClearDescription();

            if (quest == null) {
                return;
            }
            this.quest = quest;

            questDescription.text = quest.GetObjectiveDescription();

            experienceReward.text += LevelEquations.GetXPAmountForQuest(playerManager.MyCharacter.CharacterStats.Level, quest) + " XP";

            // display currency rewards

            List<CurrencyNode> currencyNodes = quest.GetCurrencyReward();

            // currencies could be different
            if (currencyNodes.Count > 0) {
                currencyHeading.gameObject.SetActive(true);
                currencyArea.gameObject.SetActive(true);
                if (currencyLootButton != null) {
                    KeyValuePair<Sprite, string> keyValuePair = CurrencyConverter.RecalculateValues(currencyNodes, true);
                    currencyLootButton.MyIcon.sprite = keyValuePair.Key;
                    currencyLootButton.MyTitle.text = keyValuePair.Value;
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
                rewardIcon.Init(systemGameManager);
                rewardIcon.OnAttempSelect += HandleAttemptSelect;
                //Debug.Log("QuestDetailsArea.ShowDescription(): setting describable (and attemptselect) for: " + quest.MyItemRewards[i]);
                rewardIcon.SetDescribable(quest.ItemRewards[i]);
                itemRewardIcons.Add(rewardIcon);
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
                rewardIcon.Init(systemGameManager);
                rewardIcon.OnAttempSelect += HandleAttemptSelect;
                //Debug.Log("QuestDetailsArea.ShowDescription(): setting describable (and attemptselect) for: " + quest.MyAbilityRewards[i]);
                rewardIcon.SetDescribable(quest.AbilityRewards[i]);
                abilityRewardIcons.Add(rewardIcon);
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
                rewardIcon.Init(systemGameManager);
                rewardIcon.OnAttempSelect += HandleAttemptSelect;
                //Debug.Log("QuestDetailsArea.ShowDescription(): setting describable (and attemptselect) for: " + quest.MyFactionRewards[i]);
                rewardIcon.SetDescribable(quest.FactionRewards[i]);
                factionRewardIcons.Add(rewardIcon);
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
                rewardIcon.Init(systemGameManager);
                rewardIcon.SetDescribable(quest.SkillRewards[i]);
                skillRewardIcons.Add(rewardIcon);
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

            // abilties
            foreach (RewardButton rewardIcon in abilityRewardIcons) {
                rewardIcon.Unselect();
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();

            // skills
            foreach (RewardButton rewardIcon in skillRewardIcons) {
                rewardIcon.Unselect();
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            skillRewardIcons.Clear();

            // factions
            foreach (RewardButton rewardIcon in factionRewardIcons) {
                rewardIcon.Unselect();
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            factionRewardIcons.Clear();
        }

    }

}