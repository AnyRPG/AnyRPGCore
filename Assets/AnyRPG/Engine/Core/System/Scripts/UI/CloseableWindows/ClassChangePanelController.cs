using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            PopupWindowManager.Instance.classChangeWindow.SetWindowTitle(characterClass.DisplayName);
            ShowAbilityRewards();
            ShowTraitRewards();
            PopupWindowManager.Instance.classChangeWindow.OpenWindow();
        }

        public void ShowTraitRewards() {
            //Debug.Log("ClassChangePanelController.ShowTraitRewards()");

            ClearTraitRewardIcons();
            // show trait rewards
            CapabilityProps capabilityProps = characterClass.GetFilteredCapabilities(PlayerManager.MyInstance.ActiveCharacter);
            if (PlayerManager.MyInstance.MyCharacter.Faction != null) {
                CapabilityConsumerSnapshot capabilityConsumerSnapshot = new CapabilityConsumerSnapshot(PlayerManager.MyInstance.ActiveCharacter);
                capabilityConsumerSnapshot.CharacterClass = characterClass;
                CapabilityProps capabilityPropsFaction = PlayerManager.MyInstance.MyCharacter.Faction.GetFilteredCapabilities(capabilityConsumerSnapshot, false);
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
                    RewardButton rewardIcon = ObjectPooler.MyInstance.GetPooledObject(rewardIconPrefab, traitIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.SetDescribable(traitList[i]);
                    traitRewardIcons.Add(rewardIcon);
                    if (traitList[i].RequiredLevel > PlayerManager.MyInstance.MyCharacter.CharacterStats.Level) {
                        rewardIcon.StackSizeText.text = "Level\n" + traitList[i].RequiredLevel;
                        rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                    }
                }
            }
        }

        public void ShowAbilityRewards() {
            //Debug.Log("ClassChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            CapabilityProps capabilityProps = characterClass.GetFilteredCapabilities(PlayerManager.MyInstance.ActiveCharacter);
            if (PlayerManager.MyInstance.MyCharacter.Faction != null) {
                CapabilityConsumerSnapshot capabilityConsumerSnapshot = new CapabilityConsumerSnapshot(PlayerManager.MyInstance.ActiveCharacter);
                capabilityConsumerSnapshot.CharacterClass = characterClass;
                CapabilityProps capabilityPropsFaction = PlayerManager.MyInstance.MyCharacter.Faction.GetFilteredCapabilities(capabilityConsumerSnapshot, false);
                capabilityProps = capabilityPropsFaction.Join(capabilityProps);
            }
            List<BaseAbility> abilityList = capabilityProps.AbilityList.Distinct().ToList();
            if (abilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
                return;
            }
            for (int i = 0; i < abilityList.Count; i++) {
                if (abilityList[i] != null) {
                    RewardButton rewardIcon = ObjectPooler.MyInstance.GetPooledObject(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                    rewardIcon.SetDescribable(abilityList[i]);
                    abilityRewardIcons.Add(rewardIcon);
                    if (abilityList[i].RequiredLevel > PlayerManager.MyInstance.MyCharacter.CharacterStats.Level) {
                        rewardIcon.StackSizeText.text = "Level\n" + abilityList[i].RequiredLevel;
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
            PopupWindowManager.Instance.classChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("ClassChangePanelController.ConfirmAction()");
            PlayerManager.MyInstance.SetPlayerCharacterClass(characterClass);
            OnConfirmAction();
            PopupWindowManager.Instance.classChangeWindow.CloseWindow();
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