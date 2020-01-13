using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SpecializationChangePanelController : WindowContentController {

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private GameObject rewardIconPrefab;

        [SerializeField]
        private ClassSpecializationButton classSpecializationButton;

        [SerializeField]
        private GameObject abilitiesArea;

        [SerializeField]
        private GameObject abilityIconsArea;

        [SerializeField]
        private GameObject traitsArea;

        [SerializeField]
        private GameObject traitIconsArea;

        private List<RewardButton> abilityRewardIcons = new List<RewardButton>();

        private List<RewardButton> traitRewardIcons = new List<RewardButton>();

        //private string characterClassName;

        private ClassSpecialization classSpecialization;

        public void Setup(ClassSpecialization newClassSpecialization) {
            //Debug.Log("ClassChangePanelController.Setup(" + newClassName + ")");
            classSpecialization = newClassSpecialization;
            classSpecializationButton.AddClassSpecialization(classSpecialization);
            PopupWindowManager.MyInstance.specializationChangeWindow.SetWindowTitle(classSpecialization.MyName);
            ShowAbilityRewards();
            ShowTraitRewards();
            PopupWindowManager.MyInstance.specializationChangeWindow.OpenWindow();
        }

        public void ShowTraitRewards() {
            //Debug.Log("ClassChangePanelController.ShowTraitRewards()");

            ClearTraitRewardIcons();
            // show trait rewards
            if (classSpecialization.MyTraitList.Count > 0) {
                traitsArea.gameObject.SetActive(true);
            } else {
                traitsArea.gameObject.SetActive(false);
            }
            for (int i = 0; i < classSpecialization.MyTraitList.Count; i++) {
                if (classSpecialization.MyTraitList[i] != null) {
                    RewardButton rewardIcon = Instantiate(rewardIconPrefab, traitIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.SetDescribable(classSpecialization.MyTraitList[i]);
                    traitRewardIcons.Add(rewardIcon);
                    if ((classSpecialization.MyTraitList[i] as StatusEffect).MyRequiredLevel > PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel) {
                        rewardIcon.MyStackSizeText.text = "Level\n" + (classSpecialization.MyTraitList[i] as StatusEffect).MyRequiredLevel;
                        rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                }
            }
        }

        public void ShowAbilityRewards() {
            //Debug.Log("ClassChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            if (classSpecialization.MyAbilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
            }
            for (int i = 0; i < classSpecialization.MyAbilityList.Count; i++) {
                if (classSpecialization.MyAbilityList[i] != null) {
                    RewardButton rewardIcon = Instantiate(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.SetDescribable(classSpecialization.MyAbilityList[i]);
                    abilityRewardIcons.Add(rewardIcon);
                    if (classSpecialization.MyAbilityList[i].MyRequiredLevel > PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel) {
                        rewardIcon.MyStackSizeText.text = "Level\n" + classSpecialization.MyAbilityList[i].MyRequiredLevel;
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
            PopupWindowManager.MyInstance.specializationChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("ClassChangePanelController.ConfirmAction()");
            PlayerManager.MyInstance.SetPlayerCharacterSpecialization(classSpecialization);
            OnConfirmAction();
            PopupWindowManager.MyInstance.specializationChangeWindow.CloseWindow();
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