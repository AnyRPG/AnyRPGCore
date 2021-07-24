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
            PopupWindowManager.Instance.specializationChangeWindow.SetWindowTitle(classSpecialization.DisplayName);
            ShowAbilityRewards();
            ShowTraitRewards();
            PopupWindowManager.Instance.specializationChangeWindow.OpenWindow();
        }

        public void ShowTraitRewards() {
            //Debug.Log("ClassChangePanelController.ShowTraitRewards()");

            ClearTraitRewardIcons();
            // show trait rewards
            CapabilityProps capabilityProps = classSpecialization.GetFilteredCapabilities(PlayerManager.MyInstance.ActiveCharacter);
            if (capabilityProps.TraitList.Count > 0) {
                traitsArea.gameObject.SetActive(true);
            } else {
                traitsArea.gameObject.SetActive(false);
                return;
            }
            for (int i = 0; i < capabilityProps.TraitList.Count; i++) {
                if (capabilityProps.TraitList[i] != null) {
                    RewardButton rewardIcon = ObjectPooler.MyInstance.GetPooledObject(rewardIconPrefab, traitIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.SetDescribable(capabilityProps.TraitList[i]);
                    traitRewardIcons.Add(rewardIcon);
                    if ((capabilityProps.TraitList[i] as StatusEffect).RequiredLevel > PlayerManager.MyInstance.MyCharacter.CharacterStats.Level) {
                        rewardIcon.StackSizeText.text = "Level\n" + (capabilityProps.TraitList[i] as StatusEffect).RequiredLevel;
                        rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                }
            }
        }

        public void ShowAbilityRewards() {
            //Debug.Log("ClassChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            CapabilityProps capabilityProps = classSpecialization.GetFilteredCapabilities(PlayerManager.MyInstance.ActiveCharacter);
            if (capabilityProps.AbilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
                return;
            }
            for (int i = 0; i < capabilityProps.AbilityList.Count; i++) {
                if (capabilityProps.AbilityList[i] != null) {
                    RewardButton rewardIcon = ObjectPooler.MyInstance.GetPooledObject(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.SetDescribable(capabilityProps.AbilityList[i]);
                    abilityRewardIcons.Add(rewardIcon);
                    if (capabilityProps.AbilityList[i].RequiredLevel > PlayerManager.MyInstance.MyCharacter.CharacterStats.Level) {
                        rewardIcon.StackSizeText.text = "Level\n" + capabilityProps.AbilityList[i].RequiredLevel;
                        rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                }
            }
        }

        private void ClearTraitRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in traitRewardIcons) {
                ObjectPooler.MyInstance.ReturnObjectToPool(rewardIcon.gameObject);
            }
            traitRewardIcons.Clear();
        }

        private void ClearRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in abilityRewardIcons) {
                ObjectPooler.MyInstance.ReturnObjectToPool(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
        }

        public void CancelAction() {
            //Debug.Log("ClassChangePanelController.CancelAction()");
            PopupWindowManager.Instance.specializationChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("ClassChangePanelController.ConfirmAction()");
            PlayerManager.MyInstance.SetPlayerCharacterSpecialization(classSpecialization);
            OnConfirmAction();
            PopupWindowManager.Instance.specializationChangeWindow.CloseWindow();
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