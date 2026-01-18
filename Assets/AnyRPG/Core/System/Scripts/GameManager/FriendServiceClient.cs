using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace AnyRPG {
    public class FriendServiceClient : ConfiguredClass {

        public event Action OnAddFriend = delegate { };
        public event Action OnRemoveFriend = delegate { };
        public event Action OnRenameFriend = delegate { };
        public event Action OnFriendStateChange = delegate { };

        private int inviteCharacterId = 0;
        private string inviteCharacterName = string.Empty;

        private FriendList friendList = new FriendList();

        // game manager references
        private UIManager uIManager = null;
        private CharacterManager characterManager = null;
        private PlayerManager playerManager = null;
        private MessageLogClient messageLogClient = null;

        public string InviteCharacterName { get => inviteCharacterName; }
        public int InviteFriendId { get => inviteCharacterId; }
        public FriendList FriendList { get => friendList; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            networkManagerClient.OnClientConnectionStopped += HandleClientConnectionStopped;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            characterManager = systemGameManager.CharacterManager;
            playerManager = systemGameManager.PlayerManager;
            messageLogClient = systemGameManager.MessageLogClient;
        }

        private void HandleClientConnectionStopped() {
            inviteCharacterId = 0;
            inviteCharacterName = string.Empty;
        }

        public void DisplayFriendInvite(int friendId, string characterName) {
            //Debug.Log($"FriendServiceClient.DisplayFriendInvite({friendId}, {leaderName})");

            inviteCharacterId = friendId;
            inviteCharacterName = characterName;
            uIManager.confirmAcceptFriendWindow.OpenWindow();
            messageLogClient.WriteSystemMessage($"You have a friend invite from {characterName}.");
        }

        public void DeclineFriendInvite() {
            //Debug.Log($"FriendServiceClient.DeclineFriendInvite()");

            networkManagerClient.DeclineFriendInvite(inviteCharacterId);
            inviteCharacterId = 0;
            inviteCharacterName = string.Empty;
        }

        public void AcceptFriendInvite() {
            //Debug.Log($"FriendServiceClient.AcceptFriendInvite()");

            networkManagerClient.AcceptFriendInvite(inviteCharacterId);
            inviteCharacterId = 0;
            inviteCharacterName = string.Empty;
        }

        public void ProcessLoadFriendList(FriendListNetworkData friendNetworkData) {
            //Debug.Log($"FriendServiceClient.ProcessLoadFriendList({friendNetworkData.MemberIdList.Count})");

            friendList = new FriendList(friendNetworkData, systemDataFactory);
        }

        public void RequestInviteCharacterToFriendList(int characterId) {
            //Debug.Log($"FriendServiceClient.RequestInviteCharacterToFriend({characterId})");

            networkManagerClient.RequestInviteCharacterToFriendList(characterId);
        }

        public void RequestInviteCharacterToFriendList(string characterName) {
            //Debug.Log($"FriendServiceClient.RequestInviteCharacterToFriend({characterId})");

            networkManagerClient.RequestInviteCharacterToFriendList(characterName);
        }

        public void AddCharacterToFriendList(CharacterSummaryNetworkData characterSummaryNetworkData) {
            //Debug.Log($"FriendServiceClient.AddCharacterToFriend({characterId}, {friendId})");

            if (friendList == null) {
                Debug.LogWarning("FriendService.AddCharacterToFriendList() friend list not found");
                return;
            }
            friendList.AddPlayer(new CharacterSummaryData(characterSummaryNetworkData, systemDataFactory));
            OnAddFriend();
            messageLogClient.WriteSystemMessage($"{characterSummaryNetworkData.CharacterName} has been added to your friend list.");
        }

        public void RequestRemoveCharacterFromFriendList(int characterId) {
            //Debug.Log($"FriendServiceClient.RequestRemoveCharacterFromFriendList({characterId})");

            networkManagerClient.RequestRemoveCharacterFromFriendList(characterId);
        }

        public void RemoveCharacterFromFriendList(int removedCharacterId) {
            //Debug.Log($"FriendServiceClient.RemoveCharacterFromFriendList({removedCharacterId})");

            if (friendList == null) {
                Debug.LogWarning("FriendService.RemoveCharacterFromFriend: character friend not found");
                return;
            }
            friendList.RemovePlayer(removedCharacterId);
            OnRemoveFriend();
        }

        public void ProcessRenameCharacterInFriendList(int characterId, string newName) {
            if (friendList == null) {
                Debug.LogWarning("FriendService.ProcessRenameCharacterInFriend: friend list not found");
                return;
            }
            if (friendList.MemberIdList.ContainsKey(characterId)) {
                friendList.MemberIdList[characterId].CharacterName = newName;
            }
            OnRenameFriend();
        }

        public void ProcessFriendStateChange(int playerCharacterId, CharacterSummaryNetworkData characterSummaryNetworkData) {
            if (friendList == null) {
                //Debug.Log("FriendService.ProcessFriendMemberOnline: friend list not found");
                return;
            }
            if (friendList.MemberIdList.ContainsKey(playerCharacterId)) {
                friendList.MemberIdList[playerCharacterId] = new CharacterSummaryData(characterSummaryNetworkData, systemDataFactory);
            }
            OnFriendStateChange();
        }

        public void ProcessDeclineFriendInvite(string decliningPlayerName) {
            messageLogClient.WriteSystemMessage($"{decliningPlayerName} has declined the friend invite.");
        }
    }

}