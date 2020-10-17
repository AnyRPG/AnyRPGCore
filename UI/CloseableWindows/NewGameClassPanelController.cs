using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameClassPanelController : WindowContentController {

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

        private NewGameCharacterClassButton selectedClassButton = null;

        private CharacterClass characterClass;

        private List<NewGameCharacterClassButton> optionButtons = new List<NewGameCharacterClassButton>();


        public void ClearOptionButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (NewGameCharacterClassButton optionButton in optionButtons) {
                if (optionButton != null) {
                    Destroy(optionButton.gameObject);
                }
            }
            optionButtons.Clear();
        }

        public void ShowOptionButtonsCommon() {
            //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon()");
            ClearOptionButtons();

            foreach (CharacterClass characterClass in SystemCharacterClassManager.MyInstance.GetResourceList()) {
                if (characterClass.NewGameOption == true) {
                    //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                    GameObject go = Instantiate(buttonPrefab, buttonArea.transform);
                    NewGameCharacterClassButton optionButton = go.GetComponent<NewGameCharacterClassButton>();
                    optionButton.AddCharacterClass(characterClass);
                    optionButtons.Add(optionButton);
                }
            }
            if (optionButtons.Count > 0) {
                optionButtons[0].Select();
            }
        }

        public void ShowCharacterClass(NewGameCharacterClassButton classButton) {
            //Debug.Log("LoadGamePanel.ShowSavedGame()");

            if (selectedClassButton != null && selectedClassButton != this) {
                selectedClassButton.DeSelect();
            }

            selectedClassButton = classButton;
            characterClass = classButton.CharacterClass;
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
            if (characterClass != null && characterClass.TraitList.Count > 0) {
                traitLabel.gameObject.SetActive(true);
                // move to bottom of list before putting traits below it
                traitLabel.transform.SetAsLastSibling();
                for (int i = 0; i < characterClass.TraitList.Count; i++) {
                    if (characterClass.TraitList[i] != null) {
                        NewGameAbilityButton rewardIcon = Instantiate(rewardIconPrefab, abilityButtonArea.transform).GetComponent<NewGameAbilityButton>();
                        rewardIcon.AddAbility(characterClass.TraitList[i]);
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
            if (characterClass != null && characterClass.AbilityList.Count > 0) {
                abilityLabel.gameObject.SetActive(true);
                abilityLabel.transform.SetAsFirstSibling();
                for (int i = 0; i < characterClass.AbilityList.Count; i++) {
                    if (characterClass.AbilityList[i] != null) {
                        NewGameAbilityButton rewardIcon = Instantiate(rewardIconPrefab, abilityButtonArea.transform).GetComponent<NewGameAbilityButton>();
                        rewardIcon.AddAbility(characterClass.AbilityList[i]);
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