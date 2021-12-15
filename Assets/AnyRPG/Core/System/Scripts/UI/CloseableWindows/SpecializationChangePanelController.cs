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
        private HighlightButton confirmButton = null;

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

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;
        private ObjectPooler objectPooler = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            confirmButton.Configure(systemGameManager);
            classSpecializationButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
            objectPooler = systemGameManager.ObjectPooler;
        }

        public void Setup(ClassSpecialization newClassSpecialization) {
            //Debug.Log("ClassChangePanelController.Setup(" + newClassName + ")");
            classSpecialization = newClassSpecialization;
            classSpecializationButton.AddClassSpecialization(classSpecialization);
            uIManager.specializationChangeWindow.SetWindowTitle(classSpecialization.DisplayName);
            ShowAbilityRewards();
            ShowTraitRewards();
            uIManager.specializationChangeWindow.OpenWindow();
        }

        public void ShowTraitRewards() {
            //Debug.Log("ClassChangePanelController.ShowTraitRewards()");

            ClearTraitRewardIcons();
            // show trait rewards
            CapabilityProps capabilityProps = classSpecialization.GetFilteredCapabilities(playerManager.ActiveCharacter);
            if (capabilityProps.TraitList.Count > 0) {
                traitsArea.gameObject.SetActive(true);
            } else {
                traitsArea.gameObject.SetActive(false);
                return;
            }
            for (int i = 0; i < capabilityProps.TraitList.Count; i++) {
                if (capabilityProps.TraitList[i] != null) {
                    RewardButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, traitIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.Configure(systemGameManager);
                    rewardIcon.SetDescribable(capabilityProps.TraitList[i]);
                    traitRewardIcons.Add(rewardIcon);
                    if ((capabilityProps.TraitList[i] as StatusEffect).RequiredLevel > playerManager.MyCharacter.CharacterStats.Level) {
                        rewardIcon.StackSizeText.text = "Level\n" + (capabilityProps.TraitList[i] as StatusEffect).RequiredLevel;
                        rewardIcon.HighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                }
            }
        }

        public void ShowAbilityRewards() {
            //Debug.Log("ClassChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            CapabilityProps capabilityProps = classSpecialization.GetFilteredCapabilities(playerManager.ActiveCharacter);
            if (capabilityProps.AbilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
                return;
            }
            for (int i = 0; i < capabilityProps.AbilityList.Count; i++) {
                if (capabilityProps.AbilityList[i] != null) {
                    RewardButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.Configure(systemGameManager);
                    rewardIcon.SetDescribable(capabilityProps.AbilityList[i]);
                    abilityRewardIcons.Add(rewardIcon);
                    if (capabilityProps.AbilityList[i].RequiredLevel > playerManager.MyCharacter.CharacterStats.Level) {
                        rewardIcon.StackSizeText.text = "Level\n" + capabilityProps.AbilityList[i].RequiredLevel;
                        rewardIcon.HighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                }
            }
        }

        private void ClearTraitRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in traitRewardIcons) {
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            traitRewardIcons.Clear();
        }

        private void ClearRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in abilityRewardIcons) {
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
        }

        public void CancelAction() {
            //Debug.Log("ClassChangePanelController.CancelAction()");
            uIManager.specializationChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("ClassChangePanelController.ConfirmAction()");
            playerManager.SetPlayerCharacterSpecialization(classSpecialization);
            OnConfirmAction();
            uIManager.specializationChangeWindow.CloseWindow();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("ClassChangePanelController.OnOpenWindow()");
            base.ProcessOpenWindowNotification();
            LayoutRebuilder.ForceRebuildLayoutImmediate(abilityIconsArea.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(traitIconsArea.GetComponent<RectTransform>());

        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("ClassChangePanelController.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }
    }

}