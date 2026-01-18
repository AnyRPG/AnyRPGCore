using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Character Group Command", menuName = "AnyRPG/Chat Commands/Character Group Command")]
    public class CharacterGroupCommand : ChatCommand {

        [Header("Character Group Command")]

        [Tooltip("The type of group command")]
        [SerializeField]
        private CharacterGroupCommandType commandType = CharacterGroupCommandType.Invite;

        // game manager references
        MessageLogServer messageLogServer = null;
        CharacterGroupServiceServer characterGroupServiceServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            messageLogServer = systemGameManager.MessageLogServer;
            characterGroupServiceServer = systemGameManager.CharacterGroupServiceServer;
        }

        public override void ExecuteCommand(string commandParameters, int accountId) {
            //Debug.Log($"ChatMessageCommand.ExecuteCommand() Executing command {DisplayName} with parameters ({commandParameters})");

            if (commandType == CharacterGroupCommandType.Disband) {
                characterGroupServiceServer.DisbandGroupByAccountId(accountId);
                return;
            }

            if (commandType == CharacterGroupCommandType.Leave) {
                characterGroupServiceServer.RequestLeaveCharacterGroup(accountId);
            }

            if (commandParameters.Length == 0) {
                // remaining commands require a character name
                return;
            }

            if (commandType == CharacterGroupCommandType.Invite) {
                characterGroupServiceServer.RequestInviteCharacterToGroup(accountId, commandParameters);
                return;
            }

            if (commandType == CharacterGroupCommandType.Kick) {
                characterGroupServiceServer.RequestRemoveCharacterFromGroup(accountId, commandParameters);
                return;
            }

            if (commandType == CharacterGroupCommandType.Promote) {
                characterGroupServiceServer.RequestPromoteCharacter(accountId, commandParameters);
                return;
            }

            if (commandType == CharacterGroupCommandType.Demote) {
                characterGroupServiceServer.RequestDemoteCharacter(accountId, commandParameters);
                return;
            }

        }

    }

    public enum CharacterGroupCommandType { Invite, Kick, Leave, Disband, Promote, Demote }

}