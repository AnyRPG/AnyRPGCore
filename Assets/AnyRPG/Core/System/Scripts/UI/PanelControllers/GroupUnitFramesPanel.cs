using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class GroupUnitFramesPanel : NavigableInterfaceElement {
        
        [Header("Group Unit Frames Panel")]

        [SerializeField]
        private List<UnitFramePanelBase> unitFramePanels = null;

        // game manager references
        protected CharacterGroupServiceClient characterGroupServiceClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (UnitFramePanelBase unitFramePanelBase in unitFramePanels) {
                unitFramePanelBase.Configure(systemGameManager);
            }

            characterGroupServiceClient.OnJoinGroup += HandleJoinGroup;
            characterGroupServiceClient.OnLeaveGroup += HandleLeaveGroup;
            characterGroupServiceClient.OnAddMember += HandleAddMember;
            characterGroupServiceClient.OnRemoveMember += HandleRemoveMember;
            characterGroupServiceClient.OnDisbandGroup += HandleDisbandGroup;
            systemEventManager.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            characterGroupServiceClient = systemGameManager.CharacterGroupServiceClient;
        }

        private void HandlePlayerUnitSpawn(UnitController unitController) {
            Debug.Log($"GroupUnitFramesPanel.HandlePlayerUnitSpawn()");

            if (systemGameManager.GameMode == GameMode.Network) {
                UpdateCharacterGroupDisplay();
            }
        }

        private void HandleDisbandGroup() {
            Debug.Log($"GroupUnitFramesPanel.HandleDisbandGroup()");

            UpdateCharacterGroupDisplay();
        }

        private void HandleRemoveMember() {
            Debug.Log($"GroupUnitFramesPanel.HandleRemoveMember()");

            UpdateCharacterGroupDisplay();
        }

        private void HandleAddMember() {
            Debug.Log($"GroupUnitFramesPanel.HandleAddMember()");

            UpdateCharacterGroupDisplay();
        }

        private void HandleLeaveGroup() {
            Debug.Log($"GroupUnitFramesPanel.HandleLeaveGroup()");

            UpdateCharacterGroupDisplay();
        }

        private void HandleJoinGroup() {
            Debug.Log($"GroupUnitFramesPanel.HandleJoinGroup()");

            UpdateCharacterGroupDisplay();
        }

        public void UpdateCharacterGroupDisplay() {
            Debug.Log($"GroupUnitFramesPanel.UpdateCharacterGroupDisplay()");

            CharacterGroup characterGroup = characterGroupServiceClient.CurrentCharacterGroup;
            if (characterGroup == null) {
                foreach (UnitFramePanelBase unitFrameController in unitFramePanels) {
                    unitFrameController.ClearTarget();
                }
                return;
            }
            List<UnitController> playerCharacters = characterGroupServiceClient.GetCurrentGroupMemberUnitControllers();
            for (int i = 0; i < unitFramePanels.Count; i++) {
                if (i < playerCharacters.Count) {
                    unitFramePanels[i].SetTarget(playerCharacters[i].NamePlateController);
                } else {
                    unitFramePanels[i].ClearTarget();
                }
            }
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log($"GroupUnitFramesPanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();
        }

        public override void ReceiveClosedWindowNotification() {
            Debug.Log($"GroupUnitFramesPanel.ReceiveClosedWindowNotification()");

            base.ReceiveClosedWindowNotification();
        }


    }

}