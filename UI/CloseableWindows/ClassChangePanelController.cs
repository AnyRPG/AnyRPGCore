using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangePanelController : WindowContentController {

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private GameObject rewardIconPrefab = null;

        [SerializeField]
        private CharacterClassButton characterClassButton = null;

        [SerializeField]
        private GameObject abilitiesArea = null;

        [SerializeField]
        private GameObject abilityIconsArea = null;

        [SerializeField]
        private GameObject traitsArea = null;

        [SerializeField]
        private GameObject traitIconsArea = null;

        private List<RewardButton> abilityRewardIcons = new List<RewardButton>();

        private List<RewardButton> traitRewardIcons = new List<RewardButton>();

        //private string characterClassName;

        private CharacterClass characterClass;

        public void Setup(CharacterClass newCharacterClass) {
            //Debug.Log("ClassChangePanelController.Setup(" + newClassName + ")");
            characterClass = newCharacterClass;
            characterClassButton.AddCharacterClass(characterClass);
            PopupWindowManager.MyInstance.classChangeWindow.SetWindowTitle(characterClass.MyName);
            ShowAbilityRewards();
            ShowTraitRewards();
            PopupWindowManager.MyInstance.classChangeWindow.OpenWindow();
        }

        public void ShowTraitRewards() {
            //Debug.Log("ClassChangePanelController.ShowTraitRewards()");

            ClearTraitRewardIcons();
            // show trait rewards
            if (characterClass.MyTraitList.Count > 0) {
                traitsArea.gameObject.SetActive(true);
            } else {
                traitsArea.gameObject.SetActive(false);
            }
            for (int i = 0; i < characterClass.MyTraitList.Count; i++) {
                if (characterClass.MyTraitList[i] != null) {
                    RewardButton rewardIcon = Instantiate(rewardIconPrefab, traitIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.SetDescribable(characterClass.MyTraitList[i]);
                    traitRewardIcons.Add(rewardIcon);
                    if ((characterClass.MyTraitList[i] as StatusEffect).MyRequiredLevel > PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel) {
                        rewardIcon.MyStackSizeText.text = "Level\n" + (characterClass.MyTraitList[i] as StatusEffect).MyRequiredLevel;
                        rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                }
            }
        }

        public void ShowAbilityRewards() {
            //Debug.Log("ClassChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            if (characterClass.MyAbilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
            }
            for (int i = 0; i < characterClass.MyAbilityList.Count; i++) {
                if (characterClass.MyAbilityList[i] != null) {
                    RewardButton rewardIcon = Instantiate(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.SetDescribable(characterClass.MyAbilityList[i]);
                    abilityRewardIcons.Add(rewardIcon);
                    if (characterClass.MyAbilityList[i].MyRequiredLevel > PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel) {
                        rewardIcon.MyStackSizeText.text = "Level\n" + characterClass.MyAbilityList[i].MyRequiredLevel;
                        rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                }
            }
        }

        private void ClearTraitRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in traitRewardIcons) {
                Destroy(rewardIcon.gameObject);
            }
            traitRewardIcons.Clear();
        }

        private void ClearRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in abilityRewardIcons) {
                Destroy(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
        }

        public void CancelAction() {
            //Debug.Log("ClassChangePanelController.CancelAction()");
            PopupWindowManager.MyInstance.classChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("ClassChangePanelController.ConfirmAction()");
            PlayerManager.MyInstance.SetPlayerCharacterClass(characterClass);
            OnConfirmAction();
            PopupWindowManager.MyInstance.classChangeWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("ClassChangePanelController.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            LayoutRebuilder.ForceRebuildLayoutImmediate(abilityIconsArea.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(traitIconsArea.GetComponent<RectTransform>());

        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("ClassChangePanelController.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }
    }

}