using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameRacePanelController : WindowContentController {

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

        private NewGameRaceButton selectedRaceButton = null;

        private List<NewGameRaceButton> optionButtons = new List<NewGameRaceButton>();

        private NewGamePanel newGamePanel = null;

        // game manager references
        private ObjectPooler objectPooler = null;
        private NewGameManager newGameManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            objectPooler = systemGameManager.ObjectPooler;
            newGameManager = systemGameManager.NewGameManager;
        }

        public void SetNewGamePanel(NewGamePanel newGamePanel) {
            this.newGamePanel = newGamePanel;
            //parentPanel = newGamePanel;
        }

        public void ClearOptionButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (NewGameRaceButton optionButton in optionButtons) {
                if (optionButton != null) {
                    optionButton.DeSelect();
                    objectPooler.ReturnObjectToPool(optionButton.gameObject);
                }
            }
            uINavigationControllers[0].ClearActiveButtons();
            optionButtons.Clear();
        }

        public void ShowOptionButtons() {
            Debug.Log("NewGameRacePanelController.ShowOptionButtons()");
            ClearOptionButtons();

            for (int i = 0; i < newGameManager.CharacterRaceList.Count; i++) {
                GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                NewGameRaceButton optionButton = go.GetComponent<NewGameRaceButton>();
                optionButton.Configure(systemGameManager);
                optionButton.AddCharacterRace(newGameManager.CharacterRaceList[i]);
                optionButtons.Add(optionButton);
                uINavigationControllers[0].AddActiveButton(optionButton);
                if (newGameManager.CharacterRaceList[i] == newGameManager.CharacterRace) {
                    uINavigationControllers[0].SetCurrentIndex(i);
                }
            }
            /*
            if (optionButtons.Count > 0) {
                SetNavigationController(uINavigationControllers[0]);
            }
            */
        }

        public void SetCharacterRace(CharacterRace newCharacterRace) {
            //Debug.Log("NewGameRacePanelController.SetCharacterRace()");

            // deselect old button
            if (selectedRaceButton != null && newCharacterRace != selectedRaceButton.CharacterRace) {
                selectedRaceButton.DeSelect();
                selectedRaceButton.UnHighlightBackground();
            }

            // select new button
            for (int i = 0; i < optionButtons.Count; i++) {
                if (optionButtons[i].CharacterRace == newCharacterRace) {
                    selectedRaceButton = optionButtons[i];
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
            //Debug.Log("NewGameRacePanelController.ShowTraitRewards()");

            ClearTraitRewardIcons();
            // show trait rewards
            if (newGameManager.CharacterRace != null && newGameManager.CharacterRace.GetFilteredCapabilities(newGameManager).TraitList.Count > 0) {
                CapabilityProps capabilityProps = newGameManager.CharacterRace.GetFilteredCapabilities(newGameManager);
                traitLabel.SetActive(true);
                // move to bottom of list before putting traits below it
                traitLabel.transform.SetAsLastSibling();
                for (int i = 0; i < capabilityProps.TraitList.Count; i++) {
                    if (capabilityProps.TraitList[i] != null) {
                        NewGameAbilityButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, abilityButtonArea.transform).GetComponent<NewGameAbilityButton>();
                        rewardIcon.Configure(systemGameManager);
                        rewardIcon.AddAbility(capabilityProps.TraitList[i].AbilityEffectProperties as StatusEffectProperties);
                        traitRewardIcons.Add(rewardIcon);
                    }
                }
            } else {
                traitLabel.SetActive(false);
            }
        }

        public void ShowAbilityRewards() {
            //Debug.Log("NewGameRacePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            if (newGameManager.CharacterRace != null && newGameManager.CharacterRace.GetFilteredCapabilities(newGameManager).AbilityList.Count > 0) {
                CapabilityProps capabilityProps = newGameManager.CharacterRace.GetFilteredCapabilities(newGameManager);
                abilityLabel.SetActive(true);
                abilityLabel.transform.SetAsFirstSibling();
                for (int i = 0; i < capabilityProps.AbilityList.Count; i++) {
                    if (capabilityProps.AbilityList[i] != null) {
                        NewGameAbilityButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, abilityButtonArea.transform).GetComponent<NewGameAbilityButton>();
                        rewardIcon.Configure(systemGameManager);
                        rewardIcon.AddAbility(capabilityProps.AbilityList[i]);
                        abilityRewardIcons.Add(rewardIcon);
                    }
                }
            } else {
                abilityLabel.SetActive(false);
            }
        }

        private void ClearTraitRewardIcons() {
            //Debug.Log("NewGameRacePanelController.ClearRewardIcons()");

            foreach (NewGameAbilityButton rewardIcon in traitRewardIcons) {
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            traitRewardIcons.Clear();
        }

        private void ClearRewardIcons() {
            //Debug.Log("NewGameRacePanelController.ClearRewardIcons()");

            foreach (NewGameAbilityButton rewardIcon in abilityRewardIcons) {
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("NewGameRacePanelController.OnOpenWindow()");
            base.ProcessOpenWindowNotification();
            abilityLabel.SetActive(false);
            traitLabel.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(abilityButtonArea.GetComponent<RectTransform>());

            ShowOptionButtons();

        }

        /*
        public override void Accept() {
            base.Accept();
            if (currentNavigationController == uINavigationControllers[0]) {
                newGamePanel.OpenDetailsPanel();
            }
        }
        */

    }

}