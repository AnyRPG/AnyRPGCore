using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Guild Command", menuName = "AnyRPG/Chat Commands/Guild Command")]
    public class GuildCommand : ChatCommand {

        [Header("Guild Command")]

        [Tooltip("The type of guild command")]
        [SerializeField]
        private GuildCommandType commandType = GuildCommandType.Invite;

        // game manager references
        GuildServiceServer guildServiceServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            guildServiceServer = systemGameManager.GuildServiceServer;
        }

        public override void ExecuteCommand(string commandParameters, int accountId) {
            //Debug.Log($"ChatMessageCommand.ExecuteCommand() Executing command {DisplayName} with parameters ({commandParameters})");

            if (commandType == GuildCommandType.Leave) {
                guildServiceServer.RequestLeaveGuild(accountId);
            }

            if (commandParameters.Length == 0) {
                // remaining commands require a character name
                return;
            }

            if (commandType == GuildCommandType.Invite) {
                guildServiceServer.RequestInviteCharacterToGuild(accountId, commandParameters);
                return;
            }

            if (commandType == GuildCommandType.Kick) {
                guildServiceServer.RequestRemoveCharacterFromGuild(accountId, commandParameters);
                return;
            }

            if (commandType == GuildCommandType.Promote) {
                guildServiceServer.RequestPromoteCharacter(accountId, commandParameters);
                return;
            }

            if (commandType == GuildCommandType.Demote) {
                guildServiceServer.RequestDemoteCharacter(accountId, commandParameters);
                return;
            }

        }

    }

    public enum GuildCommandType { Invite, Kick, Leave, Promote, Demote }

}