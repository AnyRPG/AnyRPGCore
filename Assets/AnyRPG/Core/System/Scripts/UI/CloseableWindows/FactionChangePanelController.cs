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
        private HighlightButton confirmButton = null;

        [SerializeField]
        private GameObject abilitiesArea = null;

        [SerializeField]
        private GameObject abilityIconsArea = null;

        private List<RewardButton> abilityRewardIcons = new List<RewardButton>();

        private Faction faction = null;

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;
        private ObjectPooler objectPooler = null;
        private NewGameManager newGameManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            factionButton.Configure(systemGameManager);
            confirmButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
            objectPooler = systemGameManager.ObjectPooler;
            newGameManager = systemGameManager.NewGameManager;
        }

        public void Setup(Faction newFaction) {
            //Debug.Log("FactionChangePanelController.Setup(" + newFaction.DisplayName + ")");
            faction = newFaction;
            factionButton.AddFaction(faction);
            uIManager.factionChangeWindow.SetWindowTitle(faction.DisplayName);
            ShowAbilityRewards();
            uIManager.factionChangeWindow.OpenWindow();
        }

        public void ShowAbilityRewards() {
            //Debug.Log("FactionChangePanelController.ShowAbilityRewards()");

            ClearRewardIcons();
            // show ability rewards
            // new game manager ? isn't this only in-game ?
            CapabilityProps capabilityProps = faction.GetFilteredCapabilities(newGameManager);
            if (capabilityProps.AbilityList.Count > 0) {
                abilitiesArea.gameObject.SetActive(true);
            } else {
                abilitiesArea.gameObject.SetActive(false);
                return;
            }
            for (int i = 0; i < capabilityProps.AbilityList.Count; i++) {
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

        private void ClearRewardIcons() {
            //Debug.Log("FactionChangePanelController.ClearRewardIcons()");

            foreach (RewardButton rewardIcon in abilityRewardIcons) {
                objectPooler.ReturnObjectToPool(rewardIcon.gameObject);
            }
            abilityRewardIcons.Clear();
        }

        public void CancelAction() {
            //Debug.Log("FactionChangePanelController.CancelAction()");
            uIManager.factionChangeWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("FactionChangePanelController.ConfirmAction()");
            playerManager.SetPlayerFaction(faction);
            OnConfirmAction();
            uIManager.factionChangeWindow.CloseWindow();
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("FactionChangePanelController.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }
    }

}