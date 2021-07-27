using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionChangePanelController : WindowContentController {

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private GameObject rewardIconPrefab = null;

        [SerializeField]
        private FactionButton factionButton = null;

        [SerializeField]
        private GameObject abilitiesArea = null;

        [SerializeField]
        private GameObject abilityIconsArea = null;

        private List<RewardButton> abilityRewardIcons = new List<RewardButton>();

        private Faction faction = null;

        public void Setup(Faction newFaction) {
            //Debug.Log("FactionChangePanelController.Setup(" + newFactionName + ")");
            faction = newFaction;
            factionButton.AddFaction(faction);
            PopupWindowManager.Instance.factionChangeWindow.SetWindowTitle(faction.DisplayName);
            ShowAbilityRewards();
            PopupWindowManager.Instance.factionChangeWindow.OpenWindow();
        }

        public void ShowAbilityRewards() {
            //Debug.Log("FactionChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            CapabilityProps capabilityProps = faction.GetFilteredCapabilities(NewGamePanel.Instance);
            if (capabilityProps.AbilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
                return;
            }
            for (int i = 0; i < capabilityProps.AbilityList.Count; i++) {
                RewardButton rewardIcon = ObjectPooler.Instance.GetPooledObject(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                rewardIcon.SetDescribable(capabilityProps.AbilityList[i]);
                abilityRewardIcons.Add(rewardIcon);
                if (capabilityProps.AbilityList[i].RequiredLevel > PlayerManager.Instance.MyCharacter.CharacterStats.Level) {
                    rewardIcon.StackSizeText.text = "Level\n" + capabilityProps.AbilityList[i].RequiredLevel;
                    rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                }
            }
        }

        private void ClearRewardIcons() {
            //Debug.Log("FactionChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in abilityRewardIcons) {
                ObjectPooler.Instance.ReturnObjectToPool(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
        }

        public void CancelAction() {
            //Debug.Log("FactionChangePanelController.CancelAction()");
            PopupWindowManager.Instance.factionChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("FactionChangePanelController.ConfirmAction()");
            PlayerManager.Instance.SetPlayerFaction(faction);
            OnConfirmAction();
            PopupWindowManager.Instance.factionChangeWindow.CloseWindow();
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("FactionChangePanelController.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }
    }

}