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
            PopupWindowManager.MyInstance.factionChangeWindow.SetWindowTitle(faction.DisplayName);
            ShowAbilityRewards();
            PopupWindowManager.MyInstance.factionChangeWindow.OpenWindow();
        }

        public void ShowAbilityRewards() {
            //Debug.Log("FactionChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            if (faction.LearnedAbilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
            }
            for (int i = 0; i < faction.LearnedAbilityList.Count; i++) {
                RewardButton rewardIcon = Instantiate(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                rewardIcon.SetDescribable(faction.LearnedAbilityList[i]);
                abilityRewardIcons.Add(rewardIcon);
                if (faction.LearnedAbilityList[i].MyRequiredLevel > PlayerManager.MyInstance.MyCharacter.CharacterStats.Level) {
                    rewardIcon.StackSizeText.text = "Level\n" + faction.LearnedAbilityList[i].MyRequiredLevel;
                    rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
                }
            }
        }

        private void ClearRewardIcons() {
            //Debug.Log("FactionChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in abilityRewardIcons) {
                Destroy(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
        }

        public void CancelAction() {
            //Debug.Log("FactionChangePanelController.CancelAction()");
            PopupWindowManager.MyInstance.factionChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("FactionChangePanelController.ConfirmAction()");
            PlayerManager.MyInstance.SetPlayerFaction(faction);
            OnConfirmAction();
            PopupWindowManager.MyInstance.factionChangeWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("FactionChangePanelController.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("FactionChangePanelController.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }
    }

}