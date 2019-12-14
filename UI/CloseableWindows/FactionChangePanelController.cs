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
        private GameObject rewardIconPrefab;

        [SerializeField]
        private FactionButton factionButton;

        [SerializeField]
        private GameObject abilitiesArea;

        [SerializeField]
        private GameObject abilityIconsArea;

        private List<RewardButton> abilityRewardIcons = new List<RewardButton>();

        private Faction faction;

        public void Setup(Faction newFaction) {
            //Debug.Log("FactionChangePanelController.Setup(" + newFactionName + ")");
            faction = newFaction;
            factionButton.AddFaction(faction);
            PopupWindowManager.MyInstance.factionChangeWindow.SetWindowTitle(faction.MyName);
            ShowAbilityRewards();
            PopupWindowManager.MyInstance.factionChangeWindow.OpenWindow();
        }

        public void ShowAbilityRewards() {
            //Debug.Log("FactionChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            if (faction.MyLearnedAbilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
            }
            for (int i = 0; i < faction.MyLearnedAbilityList.Count; i++) {
                RewardButton rewardIcon = Instantiate(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
                rewardIcon.SetDescribable(faction.MyLearnedAbilityList[i]);
                abilityRewardIcons.Add(rewardIcon);
                if (faction.MyLearnedAbilityList[i].MyRequiredLevel > PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel) {
                    rewardIcon.MyStackSizeText.text = "Level\n" + faction.MyLearnedAbilityList[i].MyRequiredLevel;
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