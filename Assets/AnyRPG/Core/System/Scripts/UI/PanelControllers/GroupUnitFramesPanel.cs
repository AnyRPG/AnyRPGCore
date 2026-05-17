using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class GroupUnitFramesPanel : NavigableInterfaceElement {
        
        [Header("Group Unit Frames Panel")]

        [SerializeField]
        private List<GroupUnitFramePanel> unitFramePanels = null;

        private bool characterManagerSubscriptionsInitialized = false;

        // game manager references
        protected CharacterGroupServiceClient characterGroupServiceClient = null;
        protected CharacterManager characterManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (GroupUnitFramePanel unitFramePanelBase in unitFramePanels) {
                unitFramePanelBase.Configure(systemGameManager);
            }

            characterGroupServiceClient.OnJoinGroup += HandleJoinGroup;
            characterGroupServiceClient.OnLeaveGroup += HandleLeaveGroup;
            characterGroupServiceClient.OnAddMember += HandleAddMember;
            characterGroupServiceClient.OnRemoveMember += HandleRemoveMember;
            characterGroupServiceClient.OnDisbandGroup += HandleDisbandGroup;
            characterGroupServiceClient.OnPromoteGroupLeader += HandlePromoteGroupLeader;
            characterGroupServiceClient.OnRenameCharacterInGroup += HandleRenameCharacterInGroup;
            characterGroupServiceClient.OnCharacterGroupMemberStatusChange += HandleCharacterGroupMemberStatusChange;
            systemEventManager.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            characterGroupServiceClient = systemGameManager.CharacterGroupServiceClient;
            characterManager = systemGameManager.CharacterManager;
        }

        private void HandleCharacterGroupMemberStatusChange() {
            //Debug.Log($"GroupUnitFramesPanel.HandleCharacterGroupMemberStatusChange()");

            UpdateCharacterGroupDisplay();
        }

        private void HandleRenameCharacterInGroup() {
            UpdateCharacterGroupDisplay();
        }

        private void HandlePromoteGroupLeader() {
            UpdateCharacterGroupDisplay();
        }

        private void HandlePlayerUnitSpawn(UnitController unitController) {
            //Debug.Log($"GroupUnitFramesPanel.HandlePlayerUnitSpawn()");

            if (systemGameManager.GameMode == GameMode.Network) {
                UpdateCharacterGroupDisplay();
                CreateCharacterManagerEventSubscriptions();
            }
        }

        private void HandlePlayerUnitDespawn(UnitController controller) {
            if (systemGameManager.GameMode == GameMode.Network) {
                ClearCharacterManagerEventSubscriptions();
            }
        }


        private void HandleDisbandGroup() {
            //Debug.Log($"GroupUnitFramesPanel.HandleDisbandGroup()");

            UpdateCharacterGroupDisplay();
        }

        private void HandleRemoveMember() {
            //Debug.Log($"GroupUnitFramesPanel.HandleRemoveMember()");

            UpdateCharacterGroupDisplay();
        }

        private void HandleAddMember() {
            //Debug.Log($"GroupUnitFramesPanel.HandleAddMember()");

            UpdateCharacterGroupDisplay();
        }

        private void HandleLeaveGroup() {
            //Debug.Log($"GroupUnitFramesPanel.HandleLeaveGroup()");

            UpdateCharacterGroupDisplay();
        }

        private void HandleJoinGroup() {
            //Debug.Log($"GroupUnitFramesPanel.HandleJoinGroup()");

            UpdateCharacterGroupDisplay();
        }

        public void UpdateCharacterGroupDisplay() {
            //Debug.Log($"GroupUnitFramesPanel.UpdateCharacterGroupDisplay()");

            CharacterGroup characterGroup = characterGroupServiceClient.CurrentCharacterGroup;
            if (characterGroup == null) {
                foreach (GroupUnitFramePanel unitFrameController in unitFramePanels) {
                    unitFrameController.ClearTarget();
                }
                return;
            }
            Dictionary<int, UnitController> playerCharacters = characterGroupServiceClient.GetCurrentGroupMemberUnitControllers();
            int index = 0;
            foreach (KeyValuePair<int, UnitController> kvp in playerCharacters) {
                if (index < unitFramePanels.Count) {
                    if (kvp.Value == null) {
                        unitFramePanels[index].SetNullTarget(kvp.Key);
                    } else {
                        unitFramePanels[index].SetTarget(kvp.Value);
                    }
                }
                index++;
            }

            // if there were fewer group members than unit frames, clear the rest
            for (int i = index; i < unitFramePanels.Count; i++) {
                unitFramePanels[i].ClearTarget();
            }
        }

        private void CreateCharacterManagerEventSubscriptions() {
            //Debug.Log($"GroupUnitFramesPanel.CreateCharacterManagerEventSubscriptions()");

            if (characterManagerSubscriptionsInitialized == true) {
                //Debug.Log($"GroupUnitFramesPanel.CreateCharacterManagerEventSubscriptions(): subscriptions already initialized, returning");
                return;
            }
            characterManager.OnCompleteUnitControllerInit += HandleCompleteUnitControllerInit;
            characterManagerSubscriptionsInitialized = true;
        }

        private void ClearCharacterManagerEventSubscriptions() {
            //Debug.Log($"GroupUnitFramesPanel.ClearCharacterManagerEventSubscriptions()");

            if (characterManagerSubscriptionsInitialized == false) {
                //Debug.Log($"GroupUnitFramesPanel.ClearCharacterManagerEventSubscriptions(): subscriptions not initialized, returning");
                return;
            }
            characterManager.OnCompleteUnitControllerInit -= HandleCompleteUnitControllerInit;
            characterManagerSubscriptionsInitialized = false;
        }

        private void HandleCompleteUnitControllerInit(UnitController unitController) {
            //Debug.Log($"GroupUnitFramesPanel.HandleCompleteUnitControllerInit({unitController.gameObject.name})");

            if (characterGroupServiceClient.CurrentCharacterGroup == null || characterGroupServiceClient.CurrentCharacterGroup.MemberList[UnitControllerMode.Player].ContainsKey(unitController.CharacterId) == false) {
                return;
            }
            // this is a member of the current group, likely returning from disconnection, so the list needs to be refreshed
            UpdateCharacterGroupDisplay();
        }
    }

}