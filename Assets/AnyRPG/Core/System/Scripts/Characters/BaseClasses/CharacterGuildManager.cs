using UnityEngine;

namespace AnyRPG {
    public class CharacterGuildManager : ConfiguredClass {

        private UnitController unitController = null;

        private int guildId = -1;
        private string guildName = string.Empty;

        public int GuildId { get => guildId; }
        public string GuildName { get => guildName; }

        public CharacterGuildManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }
        public void SetGuildId(int guildId, string guildName) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterGuildManager.SetGuildId({guildId}, {guildName})");

            this.guildId = guildId;
            this.guildName = guildName;
            unitController.UnitEventController.NotifyOnSetGuildId(guildId, guildName);
        }

        public bool IsInGuild() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterGuildManager.IsInGuild() return {guildId > 0}");

            return guildId > 0;
        }

        public void LeaveGuild() {
            SetGuildId(-1, string.Empty);
        }

    }

}