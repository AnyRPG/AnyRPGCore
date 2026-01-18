using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class GroupUnitFramePanel : UnitFramePanelBase {

        [Header("Group Unit Frame")]

        [SerializeField]
        protected Sprite disconnectedImage = null;

        protected override void HandleLeftClick(Vector2 mousePosition) {
            //Debug.Log($"GroupUnitFramePanel.HandleLeftClick({mousePosition})");

            base.HandleLeftClick(mousePosition);
            playerManager.UnitController.SetTarget(unitController);
        }

        public override void CreateSubscriptions() {
            //Debug.Log($"{gameObject.name}.GroupUnitFramePanel.CreateSubscriptions()");

            base.CreateSubscriptions();
            unitController.UnitEventController.OnDespawn += HandleDespawn;
        }

        public override void ClearSubscriptions() {
            //Debug.Log($"{gameObject.name}.GroupUnitFramePanel.ClearSubscriptions()");

            base.ClearSubscriptions();
            if (unitController != null) {
                unitController.UnitEventController.OnDespawn -= HandleDespawn;
            }
        }

        private void HandleDespawn(UnitController controller) {
            //Debug.Log($"GroupUnitFramePanel.HandleDespawn({controller.DisplayName})");

            SetNullTarget(unitController.CharacterId);
        }

        public override void SetTarget(UnitController unitController) {
            //Debug.Log($"GroupUnitFramePanel.SetTarget({(unitController == null ? "null" : unitController.DisplayName)})");

            /*
            if (unitController == null) { 
                SetNullTarget();
                return;
            }
            */
            base.SetTarget(unitController);
        }

        public void SetNullTarget(int characterId) {
            //Debug.Log($"GroupUnitFramePanel.SetNullTarget({userName}, {isLeader})");

            ClearTarget(false);

            if (!isActiveAndEnabled) {
                //Debug.Log($"{gameObject.name}.UnitFrameController.SetTarget(" + target.name + "): controller is not active and enabled.  Activating");
                gameObject.SetActive(true);
            }

            CharacterGroupMemberData characterGroupMemberData = characterGroupServiceClient.GetCharacterGroupMemberData(characterId);

            ClearResourceBars();
            primaryResourceText.text = $"?? / ?? (??%)";
            secondaryResourceText.text = $"?? / ?? (??%)";
            unitNameText.text = characterGroupMemberData.CharacterSummaryData.CharacterName;
            unitLevelText.text = characterGroupMemberData.CharacterSummaryData.Level.ToString();
            if (characterGroupMemberData.CharacterSummaryData.IsOnline == false) {
                ConfigurePortrait(disconnectedImage);
            } else {
                ConfigurePortrait(characterGroupMemberData.CharacterSummaryData.UnitProfile.Icon);
            }
            if (characterGroupMemberData.Rank == CharacterGroupRank.Leader) {
                leaderIcon.gameObject.SetActive(true);
            } else {
                leaderIcon.gameObject.SetActive(false);
            }
            Color tmp = Color.gray;
            tmp.a = 0.5f;
            unitNameBackground.color = tmp;
        }
    }

}