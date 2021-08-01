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

        public List<NewGameClassSpecializationButton> OptionButtons { get => optionButtons; }

        public void ClearOptionButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (NewGameClassSpecializationButton optionButton in optionButtons) {
                if (optionButton != null) {
                    optionButton.DeSelect();
                    ObjectPooler.Instance.ReturnObjectToPool(optionButton.gameObject);
                }
            }
            optionButtons.Clear();
        }

        private void HideInfoArea() {
            traitLabel.SetActive(false);
            abilityLabel.SetActive(false);
            ClearTraitRewardIcons();
            ClearAbilityRewardIcons();
        }

        public void ShowOptionButtonsCommon() {
            //Debug.Log("LoadGamePanel.ShowOptionButtonsCommon()");
            ClearOptionButtons();
            HideInfoArea();
            classSpecialization = null;

            foreach (ClassSpecialization classSpecialization in SystemDataFactory.Instance.GetResourceList<ClassSpecialization>()) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                if (NewGamePanel.Instance.CharacterClass != null
                    && classSpecialization.CharacterClasses != null
                    && classSpecialization.CharacterClasses.Contains(NewGamePanel.Instance.CharacterClass)
                    && classSpecialization.NewGameOption == true) {
                    GameObject go = ObjectPooler.Instance.GetPooledObject(buttonPrefab, buttonArea.transform);
                    NewGameClassSpecializationButton optionButton = go.GetComponent<NewGameClassSpecializationButton>();
                    optionButton.AddClassSpecialization(classSpecialization);
                    optionButtons.Add(optionButton);
                }
            }
            if (optionButtons.Count > 0) {
                optionButtons[0].Select();
            }
            // that should not be needed
            /*
            else {
                NewGamePanel.Instance.ShowClassSpecialization(null);
            }
            */
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
            if (classSpecialization != null && classSpecialization.GetFilteredCapabilities(NewGamePanel.Instance).TraitList.Count > 0) {
                CapabilityProps capabilityProps = classSpecialization.GetFilteredCapabilities(NewGamePanel.Instance);
                traitLabel.SetActive(true);
                // move to bottom of list before putting traits below it
                traitLabel.transform.SetAsLastSibling();
                for (int i = 0; i < capabilityProps.TraitList.Count; i++) {
                    if (capabilityProps.TraitList[i] != null) {
                        NewGameAbilityButton rewardIcon = ObjectPooler.Instance.GetPooledObject(rewardIconPrefab, abilityButtonArea.transform).GetComponent<NewGameAbilityButton>();
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

            ClearAbilityRewardIcons();
            // show ability rewards
            if (classSpecialization != null && classSpecialization.GetFilteredCapabilities(NewGamePanel.Instance).AbilityList.Count > 0) {
                CapabilityProps capabilityProps = classSpecialization.GetFilteredCapabilities(NewGamePanel.Instance);
                abilityLabel.SetActive(true);
                abilityLabel.transform.SetAsFirstSibling();
                for (int i = 0; i < capabilityProps.AbilityList.Count; i++) {
                    if (capabilityProps.AbilityList[i] != null) {
                        NewGameAbilityButton rewardIcon = ObjectPooler.Instance.GetPooledObject(rewardIconPrefab, abilityButtonArea.transform).GetComponent<NewGameAbilityButton>();
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
                ObjectPooler.Instance.ReturnObjectToPool(rewardIcon.gameObject);
            }
            traitRewardIcons.Clear();
        }

        private void ClearAbilityRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (NewGameAbilityButton rewardIcon in abilityRewardIcons) {
                ObjectPooler.Instance.ReturnObjectToPool(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("ClassChangePanelController.OnOpenWindow()");
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