using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Friend Command", menuName = "AnyRPG/Chat Commands/Friend Command")]
    public class FriendCommand : ChatCommand {

        [Header("Friend Command")]

        [Tooltip("The type of friend command")]
        [SerializeField]
        private FriendCommandType commandType = FriendCommandType.Add;

        // game manager references
        FriendServiceServer friendServiceServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            friendServiceServer = systemGameManager.FriendServiceServer;
        }

        public override void ExecuteCommand(string commandParameters, int accountId) {
            //Debug.Log($"ChatMessageCommand.ExecuteCommand() Executing command {DisplayName} with parameters ({commandParameters})");

            if (commandParameters.Length == 0) {
                // commands require a character name
                return;
            }

            if (commandType == FriendCommandType.Add) {
                friendServiceServer.RequestInviteCharacterToFriend(accountId, commandParameters);
                return;
            }

            if (commandType == FriendCommandType.Remove) {
                friendServiceServer.RequestRemoveCharacterFromFriendList(accountId, commandParameters);
                return;
            }

        }

    }

    public enum FriendCommandType { Add, Remove }

}