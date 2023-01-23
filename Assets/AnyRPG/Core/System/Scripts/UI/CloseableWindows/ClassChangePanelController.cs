using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangePanelController : WindowContentController {

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

        [SerializeField]
        private HighlightButton confirmButton = null;

        [SerializeField]
        private ColoredUIElement coloredDivider = null;

        private List<RewardButton> abilityRewardIcons = new List<RewardButton>();

        private List<RewardButton> traitRewardIcons = new List<RewardButton>();

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;
        private ObjectPooler objectPooler = null;
        private ClassChangeManager classChangeManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            characterClassButton.Configure(systemGameManager);
            confirmButton.Configure(systemGameManager);
            coloredDivider.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
            objectPooler = systemGameManager.ObjectPooler;
            classChangeManager = systemGameManager.ClassChangeManager;
        }

        public void ShowTraitRewards() {
            //Debug.Log("ClassChangePanelController.ShowTraitRewards()");

            ClearTraitRewardIcons();
            // show trait rewards
            CapabilityProps capabilityProps = classChangeManager.CharacterClass.GetFilteredCapabilities(playerManager.ActiveCharacter);
            if (playerManager.MyCharacter.Faction != null) {
                CapabilityConsumerSnapshot capabilityConsumerSnapshot = new CapabilityConsumerSnapshot(playerManager.ActiveCharacter, systemGameManager);
                capabilityConsumerSnapshot.CharacterClass = classChangeManager.CharacterClass;
                CapabilityProps capabilityPropsFaction = playerManager.MyCharacter.Faction.GetFilteredCapabilities(capabilityConsumerSnapshot, false);
                capabilityProps = capabilityPropsFaction.Join(capabilityProps);
            }

            List<StatusEffect> traitList = capabilityProps.TraitList.Distinct().ToList();

            if (traitList.Count > 0) {
                traitsArea.gameObject.SetActive(true);
            } else {
                traitsArea.gameObject.SetActive(false);
                return;
            }
            for (int i = 0; i < traitList.Count; i++) {
                if (traitList[i] != null) {
                    RewardButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, traitIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.Configure(systemGameManager);
                    rewardIcon.SetOptions(rectTransform, false);
                    rewardIcon.SetDescribable(traitList[i]);
                    traitRewardIcons.Add(rewardIcon);
                    if ((traitList[i].AbilityEffectProperties as StatusEffectProperties).RequiredLevel > playerManager.MyCharacter.CharacterStats.Level) {
                        rewardIcon.StackSizeText.text = "Level\n" + (traitList[i].AbilityEffectProperties as StatusEffectProperties).RequiredLevel;
                        rewardIcon.HighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                    uINavigationControllers[1].AddActiveButton(rewardIcon);
                }
            }
        }

        public void ShowAbilityRewards() {
            //Debug.Log("ClassChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            CapabilityProps capabilityProps = classChangeManager.CharacterClass.GetFilteredCapabilities(playerManager.ActiveCharacter);
            if (playerManager.MyCharacter.Faction != null) {
                CapabilityConsumerSnapshot capabilityConsumerSnapshot = new CapabilityConsumerSnapshot(playerManager.ActiveCharacter, systemGameManager);
                capabilityConsumerSnapshot.CharacterClass = classChangeManager.CharacterClass;
                CapabilityProps capabilityPropsFaction = playerManager.MyCharacter.Faction.GetFilteredCapabilities(capabilityConsumerSnapshot, false);
                capabilityProps = capabilityPropsFaction.Join(capabilityProps);
            }
            List<BaseAbilityProperties> abilityList = capabilityProps.AbilityList.Distinct().ToList();
            if (abilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
                return;
            }
            for (int i = 0; i < abilityList.Count; i++) {
                if (abilityList[i] != null) {
                    RewardButton rewardIcon = objectPooler.GetPooledObject(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.Configure(systemGameManager);
                    rewardIcon.SetOptions(rectTransform, false);
                    rewardIcon.SetDescribable(abilityList[i]);
                    abilityRewardIcons.Add(rewardIcon);
                    if (abilityList[i].RequiredLevel > playerManager.MyCharacter.CharacterStats.Level) {
                        rewardIcon.StackSizeText.text = "Level\n" + abilityList[i].RequiredLevel;
                        rewardIcon.HighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                    uINavigationControllers[2].AddActiveButton(rewardIcon);
                }
            }
        }

        private void ClearTraitRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in traitRewardIcons) {
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            traitRewardIcons.Clear();
            uINavigationControllers[1].ClearActiveButtons();
        }

        private void ClearRewardIcons() {
            //Debug.Log("ClassChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in abilityRewardIcons) {
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
            uINavigationControllers[2].ClearActiveButtons();
        }

        public void CancelAction() {
            //Debug.Log("ClassChangePanelController.CancelAction()");
            uIManager.classChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("ClassChangePanelController.ConfirmAction()");
            classChangeManager.ChangeCharacterClass();
            uIManager.classChangeWindow.CloseWindow();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("ClassChangePanelController.OnOpenWindow()");
            base.ProcessOpenWindowNotification();
            
            uIManager.classChangeWindow.SetWindowTitle(classChangeManager.CharacterClass.DisplayName);
            characterClassButton.AddCharacterClass(classChangeManager.CharacterClass);
            ShowAbilityRewards();
            ShowTraitRewards();

            LayoutRebuilder.ForceRebuildLayoutImmediate(abilityIconsArea.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(traitIconsArea.GetComponent<RectTransform>());

        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("ClassChangePanelController.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            classChangeManager.EndInteraction();
        }
    }

}