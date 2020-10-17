using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameSpecializationPanelController : WindowContentController {

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

        private NewGameClassSpecializationButton selectedClassSpecializationButton = null;

        private ClassSpecialization classSpecialization;

        private List<NewGameClassSpecializationButton> optionButtons = new List<NewGameClassSpecializationButton>();


        public void ClearOptionButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (NewGameClassSpecializationButton optionButton in optionButtons) {
                if (optionButton != null) {
                    Destroy(optionButton.gameObject);
                }
            }
            optionButtons.Clear();
        }

        public void ShowOptionButtonsCommon() {
            //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon()");
            ClearOptionButtons();

            foreach (ClassSpecialization classSpecialization in SystemClassSpecializationManager.MyInstance.GetResourceList()) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                if (NewGamePanel.MyInstance.CharacterClass != null
                    && classSpecialization.CharacterClasses != null
                    && classSpecialization.CharacterClasses.Contains(NewGamePanel.MyInstance.CharacterClass)
                    && classSpecialization.NewGameOption == true) {
                    GameObject go = Instantiate(buttonPrefab, buttonArea.transform);
                    NewGameClassSpecializationButton optionButton = go.GetComponent<NewGameClassSpecializationButton>();
                    optionButton.AddClassSpecialization(classSpecialization);
                    optionButtons.Add(optionButton);
                }
            }
            if (optionButtons.Count > 0) {
                optionButtons[0].Select();
            } else {
                NewGamePanel.MyInstance.ShowClassSpecialization(null);
            }
        }



        public void ShowClassSpecialization(NewGameClassSpecializationButton newGameClassSpecializationButton) {
            //Debug.Log("LoadGamePanel.ShowSavedGame()");
            if (selectedClassSpecializationButton != null && selectedClassSpecializationButton != newGameClassSpecializationButton) {
                selectedClassSpecializationButton.DeSelect();
            }

            selectedClassSpecializationButton = newGameClassSpecializationButton;
            if (newGameClassSpecializationButton == null) {
                classSpecialization = null;
            } else {
                classSpecialization = newGameClassSpecializationButton.ClassSpecialization;
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
            if (classSpecialization != null && classSpecialization.TraitList.Count > 0) {
                traitLabel.gameObject.SetActive(true);
                // move to bottom of list before putting traits below it
                traitLabel.transform.SetAsLastSibling();
                for (int i = 0; i < classSpecialization.TraitList.Count; i++) {
                    if (classSpecialization.TraitList[i] != null) {
                        NewGameAbilityButton rewardIcon = Instantiate(rewardIconPrefab, abilityButtonArea.transform).GetComponent<NewGameAbilityButton>();
                        rewardIcon.AddAbility(classSpecialization.TraitList[i]);
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
                traitLabel.gameObject.SetActive(false);
            }
        }

        public void ShowAbilityRewards() {
            //Debug.Log("ClassChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            if (classSpecialization != null && classSpecialization.AbilityList.Count > 0) {
                abilityLabel.gameObject.SetActive(true);
                abilityLabel.transform.SetAsFirstSibling();
                for (int i = 0; i < classSpecialization.AbilityList.Count; i++) {
                    if (classSpecialization.AbilityList[i] != null) {
                        NewGameAbilityButton rewardIcon = Instantiate(rewardIconPrefab, abilityButtonArea.transform).GetComponent<NewGameAbilityButton>();
                        rewardIcon.AddAbility(classSpecialization.AbilityList[i]);
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
                abilityLabel.gameObject.SetActive(false);
            }
        }

        private void ClearTraitRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (NewGameAbilityButton rewardIcon in traitRewardIcons) {
                Destroy(rewardIcon.gameObject);
            }
            traitRewardIcons.Clear();
        }

        private void ClearRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (NewGameAbilityButton rewardIcon in abilityRewardIcons) {
                Destroy(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("ClassChangePanelController.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
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