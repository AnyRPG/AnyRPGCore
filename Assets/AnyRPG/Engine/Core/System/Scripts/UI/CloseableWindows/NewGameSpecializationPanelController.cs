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
            foreach (NewGameClassSpecializationButton optionButton in optionButtons) {
                if (optionButton != null) {
                    optionButton.DeSelect();
                    objectPooler.ReturnObjectToPool(optionButton.gameObject);
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

            foreach (ClassSpecialization classSpecialization in systemDataFactory.GetResourceList<ClassSpecialization>()) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                if (newGameManager.CharacterClass != null
                    && classSpecialization.CharacterClasses != null
                    && classSpecialization.CharacterClasses.Contains(newGameManager.CharacterClass)
                    && classSpecialization.NewGameOption == true) {
                    GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                    NewGameClassSpecializationButton optionButton = go.GetComponent<NewGameClassSpecializationButton>();
                    optionButton.Configure(systemGameManager);
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
                newGameManager.ShowClassSpecialization(null);
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
            if (classSpecialization != null && classSpecialization.GetFilteredCapabilities(newGameManager).TraitList.Count > 0) {
                CapabilityProps capabilityProps = classSpecialization.GetFilteredCapabilities(newGameManager);
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

            ClearAbilityRewardIcons();
            // show ability rewards
            if (classSpecialization != null && classSpecialization.GetFilteredCapabilities(newGameManager).AbilityList.Count > 0) {
                CapabilityProps capabilityProps = classSpecialization.GetFilteredCapabilities(newGameManager);
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

        private void ClearAbilityRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (NewGameAbilityButton rewardIcon in abilityRewardIcons) {
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
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