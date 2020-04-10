using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
public class QuestDetailsArea : MonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI questDescription = null;

    [SerializeField]
    private TextMeshProUGUI experienceReward = null;

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
    private GameObject rewardIconPrefab = null;

    private Quest quest = null;

    private List<RewardButton> itemRewardIcons = new List<RewardButton>();
    private List<RewardButton> abilityRewardIcons = new List<RewardButton>();
    private List<RewardButton> skillRewardIcons = new List<RewardButton>();
    private List<RewardButton> factionRewardIcons = new List<RewardButton>();


    private void Start() {
    }

    public List<RewardButton> GetHighlightedItemRewardIcons() {
        //Debug.Log("QuestDetailsArea.GetHighlightedItemRewardIcons()");
        List<RewardButton> returnList = new List<RewardButton>();
        foreach (RewardButton rewardButton in itemRewardIcons) {
            //Debug.Log("QuestDetailsArea.GetHighlightedItemRewardIcons(): passing over rewardbutton");
            if (rewardButton.isActiveAndEnabled == true && (rewardButton.MySelected == true || quest.MyMaxItemRewards == 0)) {
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
            if (rewardButton.isActiveAndEnabled == true && (rewardButton.MySelected == true || quest.MyMaxAbilityRewards == 0)) {
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
            if (rewardButton.isActiveAndEnabled == true && (rewardButton.MySelected == true || quest.MyMaxSkillRewards == 0)) {
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
            if (rewardButton.isActiveAndEnabled == true && (rewardButton.MySelected == true || quest.MyMaxFactionRewards == 0)) {
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
            if (quest.MyMaxItemRewards > 0 && GetHighlightedItemRewardIcons().Count > quest.MyMaxItemRewards) {
                rewardButton.Unselect();
            }
        }
        if (GetHighlightedFactionRewardIcons().Contains(rewardButton)) {
            //Debug.Log("QuestDetailsArea.HandleAttemptSelect(): it's an faction reward; current count of highlighted icons: " + GetHighlightedFactionRewardIcons().Count + "; max: " + quest.MyMaxFactionRewards);
            if (quest.MyMaxFactionRewards > 0 && GetHighlightedFactionRewardIcons().Count > quest.MyMaxFactionRewards) {
                rewardButton.Unselect();
            }
        }

        if (GetHighlightedAbilityRewardIcons().Contains(rewardButton)) {
            //Debug.Log("QuestDetailsArea.HandleAttemptSelect(): it's an ability reward; current count of highlighted icons: " + GetHighlightedAbilityRewardIcons().Count + "; max: " + quest.MyMaxAbilityRewards);
            if (quest.MyMaxAbilityRewards > 0 && GetHighlightedAbilityRewardIcons().Count > quest.MyMaxAbilityRewards || PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(rewardButton.MyDescribable as BaseAbility)) {
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

        experienceReward.text += quest.MyExperienceReward + " XP";

        // show item rewards
        if (quest.MyItemRewards.Count > 0) {
            itemsHeading.gameObject.SetActive(true);
            if (quest.MyMaxItemRewards > 0) {
                itemsHeading.GetComponent<Text>().text = "Choose " + quest.MyMaxItemRewards + " Item Rewards:";
            } else {
                itemsHeading.GetComponent<Text>().text = "Item Rewards:";
            }
        }
        for (int i = 0; i < quest.MyItemRewards.Count; i++) {
            RewardButton rewardIcon = Instantiate(rewardIconPrefab, itemIconsArea.transform).GetComponent<RewardButton>();
            rewardIcon.OnAttempSelect += HandleAttemptSelect;
            //Debug.Log("QuestDetailsArea.ShowDescription(): setting describable (and attemptselect) for: " + quest.MyItemRewards[i]);
            rewardIcon.SetDescribable(quest.MyItemRewards[i]);
            itemRewardIcons.Add(rewardIcon);
        }

        // show ability rewards
        if (quest.MyAbilityRewards.Count > 0) {
            abilitiesHeading.gameObject.SetActive(true);
            if (quest.MyMaxAbilityRewards > 0) {
                abilitiesHeading.GetComponent<Text>().text = "Choose " + quest.MyMaxAbilityRewards + " Ability Rewards:";
            } else {
                abilitiesHeading.GetComponent<Text>().text = "Ability Rewards:";
            }
        } else {
            abilitiesHeading.GetComponent<Text>().text = "";
        }
        for (int i = 0; i < quest.MyAbilityRewards.Count; i++) {
            RewardButton rewardIcon = Instantiate(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
            rewardIcon.OnAttempSelect += HandleAttemptSelect;
            //Debug.Log("QuestDetailsArea.ShowDescription(): setting describable (and attemptselect) for: " + quest.MyAbilityRewards[i]);
            rewardIcon.SetDescribable(quest.MyAbilityRewards[i]);
            abilityRewardIcons.Add(rewardIcon);
        }

        // show faction rewards
        if (quest.MyFactionRewards.Count > 0) {
            factionsHeading.gameObject.SetActive(true);
            if (quest.MyMaxFactionRewards > 0) {
                factionsHeading.GetComponent<Text>().text = "Choose " + quest.MyMaxFactionRewards + " Reputation Rewards:";
            } else {
                factionsHeading.GetComponent<Text>().text = "Reputation Rewards:";
            }
        } else {
            factionsHeading.GetComponent<Text>().text = "";
        }
        for (int i = 0; i < quest.MyFactionRewards.Count; i++) {
            RewardButton rewardIcon = Instantiate(rewardIconPrefab, factionIconsArea.transform).GetComponent<RewardButton>();
            rewardIcon.OnAttempSelect += HandleAttemptSelect;
            //Debug.Log("QuestDetailsArea.ShowDescription(): setting describable (and attemptselect) for: " + quest.MyFactionRewards[i]);
            rewardIcon.SetDescribable(quest.MyFactionRewards[i]);
            factionRewardIcons.Add(rewardIcon);
        }

        // show Skill rewards
        if (quest.MySkillRewards.Count > 0) {
            skillHeading.gameObject.SetActive(true);
            if (quest.MyMaxSkillRewards > 0) {
                skillHeading.GetComponent<Text>().text = "Choose " + quest.MyMaxSkillRewards + " Skill Rewards:";
            } else {
                skillHeading.GetComponent<Text>().text = "Skill Rewards:";
            }
        } else {
            skillHeading.GetComponent<Text>().text = "";
        }
        for (int i = 0; i < quest.MySkillRewards.Count; i++) {
            RewardButton rewardIcon = Instantiate(rewardIconPrefab, skillIconsArea.transform).GetComponent<RewardButton>();
            rewardIcon.SetDescribable(quest.MySkillRewards[i]);
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
            Destroy(rewardIcon.gameObject);
        }
        itemRewardIcons.Clear();

        // abilties
        foreach (RewardButton rewardIcon in abilityRewardIcons) {
            Destroy(rewardIcon.gameObject);
        }
        abilityRewardIcons.Clear();

        // skills
        foreach (RewardButton rewardIcon in skillRewardIcons) {
            Destroy(rewardIcon.gameObject);
        }
        skillRewardIcons.Clear();

        // factions
        foreach (RewardButton rewardIcon in factionRewardIcons) {
            Destroy(rewardIcon.gameObject);
        }
        factionRewardIcons.Clear();
    }

}

}