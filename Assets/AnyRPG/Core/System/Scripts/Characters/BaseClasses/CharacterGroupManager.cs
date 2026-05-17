using UnityEngine;

namespace AnyRPG {
    public class CharacterGroupManager : ConfiguredClass {

        private UnitController unitController = null;

        private int groupId = -1;

        public int GroupId { get => groupId; }

        public CharacterGroupManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }
        public void SetGroupId(int groupId) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterGroupManager.SetGroupId({groupId})");

            this.groupId = groupId;
            unitController.UnitEventController.NotifyOnSetGroupId(groupId);
        }

        public bool IsInGroup() {
            return groupId > 0;
        }

        public void LeaveGroup() {
            SetGroupId(-1);
        }

    }

}