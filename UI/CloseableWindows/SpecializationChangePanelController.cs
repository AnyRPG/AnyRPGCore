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
        private GameObject rewardIconPrefab = null;

        [SerializeField]
        private ClassSpecializationButton classSpecializationButton = null;

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

        private ClassSpecialization classSpecialization;

        public void Setup(ClassSpecialization newClassSpecialization) {
            //Debug.Log("ClassChangePanelController.Setup(" + newClassName + ")");
            classSpecialization = newClassSpecialization;
            classSpecializationButton.AddClassSpecialization(classSpecialization);
            PopupWindowManager.MyInstance.specializationChangeWindow.SetWindowTitle(classSpecialization.DisplayName);
            ShowAbilityRewards();
            ShowTraitRewards();
            PopupWindowManager.MyInstance.specializationChangeWindow.OpenWindow();
        }

        public void ShowTraitRewards() {
            //Debug.Log("ClassChangePanelController.ShowTraitRewards()");

            ClearTraitRewardIcons();
            // show trait rewards
            if (classSpecialization.TraitList.Count > 0) {
                traitsArea.gameObject.SetActive(true);
            } else {
                traitsArea.gameObject.SetActive(false);
            }
            for (int i = 0; i < classSpecialization.TraitList.Count; i++) {
                if (classSpecialization.TraitList[i] != null) {
                    RewardButton rewardIcon = Instantiate(rewardIconPrefab, traitIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.SetDescribable(classSpecialization.TraitList[i]);
                    traitRewardIcons.Add(rewardIcon);
                    if ((classSpecialization.TraitList[i] as StatusEffect).MyRequiredLevel > PlayerManager.MyInstance.MyCharacter.CharacterStats.Level) {
                        rewardIcon.StackSizeText.text = "Level\n" + (classSpecialization.TraitList[i] as StatusEffect).MyRequiredLevel;
                        rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                }
            }
        }

        public void ShowAbilityRewards() {
            //Debug.Log("ClassChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            if (classSpecialization.AbilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
            }
            for (int i = 0; i < classSpecialization.AbilityList.Count; i++) {
                if (classSpecialization.AbilityList[i] != null) {
                    RewardButton rewardIcon = Instantiate(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.SetDescribable(classSpecialization.AbilityList[i]);
                    abilityRewardIcons.Add(rewardIcon);
                    if (classSpecialization.AbilityList[i].MyRequiredLevel > PlayerManager.MyInstance.MyCharacter.CharacterStats.Level) {
                        rewardIcon.StackSizeText.text = "Level\n" + classSpecialization.AbilityList[i].MyRequiredLevel;
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