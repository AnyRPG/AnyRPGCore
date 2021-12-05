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

        //private Faction faction;

        private List<NewGameFactionButton> optionButtons = new List<NewGameFactionButton>();

        private NewGamePanel newGamePanel = null;

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

        public void SetNewGamePanel(NewGamePanel newGamePanel) {
            this.newGamePanel = newGamePanel;
            //parentPanel = newGamePanel;
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
            uINavigationControllers[0].ClearActiveButtons();
            optionButtons.Clear();
        }

        public void ShowOptionButtons() {
            Debug.Log("NewGameFactionPanelController.ShowOptionButtons()");
            ClearOptionButtons();

            for (int i = 0; i < newGameManager.FactionList.Count; i++) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                NewGameFactionButton optionButton = go.GetComponent<NewGameFactionButton>();
                optionButton.Configure(systemGameManager);
                optionButton.AddFaction(newGameManager.FactionList[i]);
                optionButtons.Add(optionButton);
                uINavigationControllers[0].AddActiveButton(optionButton);
                if (newGameManager.FactionList[i] == newGameManager.Faction) {
                    uINavigationControllers[0].SetCurrentIndex(i);
                }
            }
            /*
            if (optionButtons.Count > 0) {
                SetNavigationController(uINavigationControllers[0]);
            }
            */
        }

        public void SetFaction(Faction newFaction) {
            Debug.Log("NewGameFactionPanelController.SetFaction(" + newFaction.DisplayName + ")");

            // deselect old button
            if (selectedFactionButton != null && selectedFactionButton.Faction != newFaction) {
                selectedFactionButton.DeSelect();
                selectedFactionButton.UnHighlightBackground();
            }

            // select new button
            for (int i = 0; i < optionButtons.Count; i++) {
                if (optionButtons[i].Faction == newFaction) {
                    selectedFactionButton = optionButtons[i];
                    uINavigationControllers[0].SetCurrentIndex(i);
                    optionButtons[uINavigationControllers[0].CurrentIndex].HighlightBackground();
                }
            }

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

            if (newGameManager.Faction != null && newGameManager.Faction.GetFilteredCapabilities(newGameManager).TraitList.Count > 0) {
                CapabilityProps capabilityProps = newGameManager.Faction.GetFilteredCapabilities(newGameManager);
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
            if (newGameManager.Faction != null && newGameManager.Faction.GetFilteredCapabilities(newGameManager).AbilityList.Count > 0) {
                CapabilityProps capabilityProps = newGameManager.Faction.GetFilteredCapabilities(newGameManager);
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

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("NewGameFactionPanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            abilityLabel.SetActive(false);
            traitLabel.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(abilityButtonArea.GetComponent<RectTransform>());

            //ShowOptionButtons();

        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("ClassChangePanelController.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }

        /*
        public override void Accept() {
            Debug.Log(gameObject.name + ".NewGameFactionPanelController.Accept()");
            base.Accept();
            if (currentNavigationController == uINavigationControllers[0]) {
                newGamePanel.OpenDetailsPanel();
            }
        }
            */

    }

}