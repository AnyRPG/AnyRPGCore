using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameFactionPanelController : WindowContentController {

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private GameObject buttonPrefab = null;

        [SerializeField]
        private GameObject buttonArea = null;

        [SerializeField]
        private GameObject rewardIconPrefab = null;

        [SerializeField]
        private GameObject abilityLabel = null;

        [SerializeField]
        private GameObject traitLabel = null;

        [SerializeField]
        private GameObject abilityButtonArea = null;

        [SerializeField]
        private CanvasGroup canvasGroup = null;

        private List<NewGameAbilityButton> abilityRewardIcons = new List<NewGameAbilityButton>();

        private List<NewGameAbilityButton> traitRewardIcons = new List<NewGameAbilityButton>();

        private NewGameFactionButton selectedFactionButton = null;

        private Faction faction;

        private List<NewGameFactionButton> optionButtons = new List<NewGameFactionButton>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private SystemDataFactory systemDataFactory = null;
        private NewGameManager newGameManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            objectPooler = systemGameManager.ObjectPooler;
            systemDataFactory = systemGameManager.SystemDataFactory;
            newGameManager = systemGameManager.NewGameManager;
        }

        public void ClearOptionButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (NewGameFactionButton optionButton in optionButtons) {
                if (optionButton != null) {
                    optionButton.DeSelect();
                    objectPooler.ReturnObjectToPool(optionButton.gameObject);
                }
            }
            optionButtons.Clear();
        }

        public void ShowOptionButtonsCommon() {
            //Debug.Log("NewGameFactionPanelController.ShowOptionButtonsCommon()");
            ClearOptionButtons();

            foreach (Faction faction in systemDataFactory.GetResourceList<Faction>()) {
                if (faction.NewGameOption == true) {
                    //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                    GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                    NewGameFactionButton optionButton = go.GetComponent<NewGameFactionButton>();
                    optionButton.Configure(systemGameManager);
                    optionButton.AddFaction(faction);
                    optionButtons.Add(optionButton);
                }
            }
            if (optionButtons.Count > 0) {
                optionButtons[0].Select();
            }
        }

        public void ShowFaction(NewGameFactionButton factionButton) {
            //Debug.Log("LoadGamePanel.ShowSavedGame()");
            if (selectedFactionButton != null && selectedFactionButton != this) {
                selectedFactionButton.DeSelect();
            }

            selectedFactionButton = factionButton;
            faction = factionButton.Faction;
            ShowAbilityRewards();
            ShowTraitRewards();
        }

        public void HidePanel() {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public void ShowPanel() {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }


        public void ShowTraitRewards() {
            //Debug.Log("ClassChangePanelController.ShowTraitRewards()");

            ClearTraitRewardIcons();
            // show trait rewards

            if (faction != null && faction.GetFilteredCapabilities(newGameManager).TraitList.Count > 0) {
                CapabilityProps capabilityProps = faction.GetFilteredCapabilities(newGameManager);
                traitLabel.SetActive(true);
                // move to bottom of list before putting traits below it
                traitLabel.transform.SetAsLastSibling();
                for (int i = 0; i < capabilityProps.TraitList.Count; i++) {
                    if (capabilityProps.TraitList[i] != null) {
                        NewGameAbilityButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, abilityButtonArea.transform).GetComponent<NewGameAbilityButton>();
                        rewardIcon.Configure(systemGameManager);
                        rewardIcon.AddAbility(capabilityProps.TraitList[i]);
                        traitRewardIcons.Add(rewardIcon);
                        /*
                        if ((characterClass.TraitList[i] as StatusEffect).MyRequiredLevel > 1) {
                            rewardIcon.StackSizeText.text = "Level\n" + (characterClass.TraitList[i] as StatusEffect).MyRequiredLevel;
                            rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                        }
                        */
                    }
                }
            } else {
                traitLabel.SetActive(false);
            }
        }

        public void ShowAbilityRewards() {
            //Debug.Log("ClassChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            if (faction != null && faction.GetFilteredCapabilities(newGameManager).AbilityList.Count > 0) {
                CapabilityProps capabilityProps = faction.GetFilteredCapabilities(newGameManager);
                abilityLabel.SetActive(true);
                abilityLabel.transform.SetAsFirstSibling();
                for (int i = 0; i < capabilityProps.AbilityList.Count; i++) {
                    if (capabilityProps.AbilityList[i] != null) {
                        NewGameAbilityButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, abilityButtonArea.transform).GetComponent<NewGameAbilityButton>();
                        rewardIcon.Configure(systemGameManager);
                        rewardIcon.AddAbility(capabilityProps.AbilityList[i]);
                        abilityRewardIcons.Add(rewardIcon);
                        /*
                        if (characterClass.AbilityList[i].MyRequiredLevel > 1) {
                            rewardIcon.StackSizeText.text = "Level\n" + characterClass.AbilityList[i].MyRequiredLevel;
                            rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                        }
                        */
                    }
                }
            } else {
                abilityLabel.SetActive(false);
            }
        }

        private void ClearTraitRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (NewGameAbilityButton rewardIcon in traitRewardIcons) {
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            traitRewardIcons.Clear();
        }

        private void ClearRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (NewGameAbilityButton rewardIcon in abilityRewardIcons) {
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("NewGameFactionPanelController.ReceiveOpenWindowNotification()");
            base.ReceiveOpenWindowNotification();
            abilityLabel.SetActive(false);
            traitLabel.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(abilityButtonArea.GetComponent<RectTransform>());

            ShowOptionButtonsCommon();

        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("ClassChangePanelController.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }
    }

}