using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG {
    public class FriendServiceServer : ConfiguredClass {

        /// <summary>
        /// inviterCharacterId, invitedCharacterIds
        /// </summary>
        private Dictionary<int, List<int>> friendInvites = new Dictionary<int, List<int>>();

        /// <summary>
        /// characterId, FriendList
        /// </summary>
        private Dictionary<int, FriendList> friendListDictionary = new Dictionary<int, FriendList>();

        // game manager references
        private PlayerManagerServer playerManagerServer = null;
        private CharacterManager characterManager = null;
        private ServerDataService serverDataService = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            characterManager = systemGameManager.CharacterManager;
            serverDataService = systemGameManager.ServerDataService;
        }


        private void HandleStopServer() {
            ClearFriends();
        }

        public void LoadAllFriends() {
            //Debug.Log("FriendServiceServer.LoadAllFriends()");

            serverDataService.LoadAllFriends();
        }

        public void ProcessLoadAllFriendLists(List<FriendListSaveData> friendListSaveDatas) {
            foreach (FriendListSaveData friendListSaveData in friendListSaveDatas) {
                if (friendListSaveData.PlayerCharacterId == -1) {
                    Debug.LogWarning($"FriendServiceServer.LoadAllFriends(): Player has invalid id of -1.  This friend list will be skipped.");
                    continue;
                }
                if (friendListDictionary.ContainsKey(friendListSaveData.PlayerCharacterId)) {
                    Debug.LogWarning($"FriendServiceServer.LoadAllFriends(): Duplicate friend id {friendListSaveData.PlayerCharacterId} found.  This friend list will be skipped.");
                    continue;
                }
                FriendList friendList = new FriendList(friendListSaveData, playerCharacterService);

                //Debug.Log($"FriendServiceServer.LoadAllFriends(): Loaded friend list with id {friendList.playerCharacterId} and {friendList.MemberIdList.Count} members.");
                friendListDictionary.Add(friendList.playerCharacterId, friendList);
            }
        }

        public void ProcessLoadAllFriendLists(List<FriendListSerializedData> friendListSerializedDatas) {
            List<FriendListSaveData> friendListSaveDatas = new List<FriendListSaveData>();
            foreach (FriendListSerializedData friendListSerializedData in friendListSerializedDatas) {
                FriendListSaveData friendListSaveData = JsonUtility.FromJson<FriendListSaveData>(friendListSerializedData.saveData);
                // since network friend lists are created automatically with no content, check for null saveData
                if (friendListSaveData == null) {
                    friendListSaveData = new FriendListSaveData();
                }
                // since network friend list are created automatically with no content, the trusted field for player character id should be the database field
                friendListSaveData.PlayerCharacterId = friendListSerializedData.playerCharacterId;
                friendListSaveDatas.Add(friendListSaveData);
            }
            ProcessLoadAllFriendLists(friendListSaveDatas);
        }

        private void ClearFriends() {
            friendListDictionary.Clear();
        }

        public FriendList GetFriendListFromCharacterId(int characterId) {
            //Debug.Log($"FriendServiceServer.GetFriendFromCharacterId({characterId})");

            if (friendListDictionary.ContainsKey(characterId)) {
                return friendListDictionary[characterId];
            }
            return null;
        }

        private void SaveFriendList(FriendList friendList) {
            //Debug.Log($"FriendServiceServer.SaveFriendList(playerCharacterId: {friendList.playerCharacterId})");

            serverDataService.SaveFriendList(friendList);
        }

        public void AddCharacterToFriendList(int sourceCharacterId, int targetCharacterId) {
            //Debug.Log($"FriendServiceServer.AddCharacterToFriendList(sourceCharacterId: {sourceCharacterId}, targetCharacterId: {targetCharacterId})");

            if (friendListDictionary.ContainsKey(sourceCharacterId) == false) {
                friendListDictionary.Add(sourceCharacterId, new FriendList(sourceCharacterId));
            }
            if (friendListDictionary.ContainsKey(targetCharacterId) == false) {
                friendListDictionary.Add(targetCharacterId, new FriendList(targetCharacterId));
            }
            FriendList sourceFriendList = friendListDictionary[sourceCharacterId];
            FriendList targetFriendList = friendListDictionary[targetCharacterId];

            UnitController sourceUnitController = characterManager.GetUnitController(UnitControllerMode.Player, sourceCharacterId);
            if (sourceUnitController == null) {
                Debug.LogWarning($"FriendServiceServer.AddCharacterToFriendList() unit controller not found for characterId {sourceCharacterId}");
                return;
            }
            UnitController targetUnitController = characterManager.GetUnitController(UnitControllerMode.Player, targetCharacterId);
            if (targetUnitController == null) {
                Debug.LogWarning($"FriendServiceServer.AddCharacterToFriendList() unit controller not found for characterId {sourceCharacterId}");
                return;
            }

            int sourceCharacterAccountId = playerManagerServer.GetAccountIdFromUnitController(sourceUnitController);
            int targetCharacterAccountId = playerManagerServer.GetAccountIdFromUnitController(targetUnitController);

            CharacterSummaryData targetFriendInfo = playerCharacterService.GetSummaryData(targetCharacterId);
            CharacterSummaryData sourceFriendInfo = playerCharacterService.GetSummaryData(sourceCharacterId);

            if (sourceFriendInfo == null) {
                Debug.LogWarning($"FriendServiceServer.AddCharacterToFriendList() source CharacterSummaryData was not found");
                return;
            } else if (targetFriendInfo == null) {
                Debug.LogWarning($"FriendServiceServer.AddCharacterToFriendList() target CharacterSummaryData was not found");
                return;
            }

            sourceFriendList.AddPlayer(targetFriendInfo);
            targetFriendList.AddPlayer(sourceFriendInfo);
            SaveFriendList(sourceFriendList);
            SaveFriendList(targetFriendList);

            networkManagerServer.AdvertiseAddFriend(sourceCharacterAccountId, new CharacterSummaryNetworkData(targetFriendInfo));
            networkManagerServer.AdvertiseAddFriend(targetCharacterAccountId, new CharacterSummaryNetworkData(sourceFriendInfo));

        }

        public void RemoveCharacterFromFriendList(int sourceCharacterId, int targetCharacterId) {
            //Debug.Log($"FriendServiceServer.RemoveCharacterFromFriendList({sourceCharacterId}, {targetCharacterId})");

            if (friendListDictionary.ContainsKey(targetCharacterId) == true) {
                FriendList targetFriendList = friendListDictionary[targetCharacterId];
                targetFriendList.RemovePlayer(sourceCharacterId);
                SaveFriendList(targetFriendList);
                int targetCharacterAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(targetCharacterId);
                networkManagerServer.AdvertiseRemoveCharacterFromFriendList(targetCharacterAccountId, sourceCharacterId);
            }

            if (friendListDictionary.ContainsKey(sourceCharacterId) == true) {
                FriendList sourceFriendList = friendListDictionary[sourceCharacterId];
                sourceFriendList.RemovePlayer(targetCharacterId);
                SaveFriendList(sourceFriendList);
                int sourceCharacterAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(sourceCharacterId);
                networkManagerServer.AdvertiseRemoveCharacterFromFriendList(sourceCharacterAccountId, targetCharacterId);
            }
        }

        public void AcceptFriendInvite(int accountId, int friendId) {
            //Debug.Log($"FriendServiceServer.AcceptFriendInvite({accountId}, {friendId})");

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                Debug.LogWarning($"FriendServiceServer.AcceptFriendInvite({accountId}, {friendId}) player character not found for account");
                return;
            }
            if (friendInvites.ContainsKey(friendId) == false || friendInvites[friendId].Contains(playerCharacterId) == false) {
                Debug.LogWarning($"FriendServiceServer.AcceptFriendInvite({accountId}, {friendId}) character friend invite not found");
                return;
            }
            friendInvites[friendId].Remove(playerCharacterId);
            
            // add the new member
            AddCharacterToFriendList(playerCharacterId, friendId);
        }

        public void DeclineFriendInvite(int accountId, int friendId) {
            //Debug.Log($"FriendServiceServer.DeclineFriendInvite({accountId})");

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                Debug.LogWarning($"FriendServiceServer.DeclineFriendInvite({accountId}) player character not found for accountId");
                return;
            }
            int friendAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(friendId);
            if (friendAccountId == -1) {
                Debug.LogWarning($"FriendServiceServer.DeclineFriendInvite({accountId}) account not found for friendId {friendId}");
                return;
            }
            if (friendInvites.ContainsKey(friendId) == true) {
                friendInvites[friendId].Remove(playerCharacterId);
                networkManagerServer.AdvertiseDeclineFriendInvite(friendAccountId, playerCharacterService.GetPlayerNameFromId(playerCharacterId));
            }
        }

        public void RequestRemoveCharacterFromFriendList(int sourceAccountId, string removedCharacterName) {
            //Debug.Log($"FriendServiceServer.RequestRemoveCharacterFromFriendList({sourceAccountId}, {removedCharacterName})");

            int removedCharacterId = playerCharacterService.GetPlayerIdFromName(removedCharacterName);
            if (removedCharacterId == -1) {
                return;
            }

            RequestRemoveCharacterFromFriendList(sourceAccountId, removedCharacterId);
        }

        public void RequestRemoveCharacterFromFriendList(int sourceAccountId, int removedCharacterId) {
            //Debug.Log($"FriendServiceServer.RequestRemoveCharacterFromFriendList({sourceAccountId}, {removedCharacterId})");

            int sourceCharacterId = playerManagerServer.GetPlayerCharacterId(sourceAccountId);
            if (sourceCharacterId == -1) {
                //Debug.LogWarning($"FriendServiceServer.RequestInviteCharacterToFriend: player character not found for accountId {accountId}");
                return;
            }
            
            RemoveCharacterFromFriendList(sourceCharacterId, removedCharacterId);
        }

        public void RequestInviteCharacterToFriend(int sourceAccountId, string playerName) {
            int inviteCharacterId = playerCharacterService.GetPlayerIdFromName(playerName);
            if (inviteCharacterId != -1) {
                RequestInviteCharacterToFriend(sourceAccountId, inviteCharacterId);
            }
        }

        public void RequestInviteCharacterToFriend(int sourceAccountId, int invitedCharacterId) {
            //Debug.Log($"FriendServiceServer.RequestInviteCharacterToFriend({sourceAccountId}, {invitedCharacterId})");

            int sourceCharacterId = playerManagerServer.GetPlayerCharacterId(sourceAccountId);
            if (sourceCharacterId == -1) {
                Debug.LogWarning($"FriendServiceServer.RequestInviteCharacterToFriend: player character not found for leader accountId {sourceAccountId}");
                return;
            }

            if (sourceCharacterId == invitedCharacterId) {
                return;
            }

            if (friendInvites.ContainsKey(sourceCharacterId) == false) {
                friendInvites.Add(sourceCharacterId, new List<int>());
            }
            friendInvites[sourceCharacterId].Add(invitedCharacterId);
            string sourceCharacterName = playerManagerServer.GetPlayerName(sourceAccountId);
            if (sourceCharacterName == string.Empty) {
                sourceCharacterName = "Unknown";
            }
            int invitedAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(invitedCharacterId);
            networkManagerServer.AdvertiseFriendInvite(invitedAccountId, sourceCharacterId, sourceCharacterName);
        }

        public void SendFriendListInfo(int accountId) {
            //Debug.Log($"FriendServiceServer.SendFriendListInfo(accountId: {accountId})");

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (friendListDictionary.ContainsKey(playerCharacterId) == false) {
                friendListDictionary.Add(playerCharacterId, new FriendList(playerCharacterId));
            }

            FriendList friendList = friendListDictionary[playerCharacterId];
            //Debug.Log($"FriendServiceServer.SendFriendInfo: advertising friend list to accountId {accountId}");
            networkManagerServer.AdvertiseFriendList(accountId, new FriendListNetworkData(friendList));
        }

        /*
        public void ProcessRenameCharacter(int characterId, string newName) {
            //Debug.Log($"FriendServiceServer.ProcessRenameCharacter({characterId}, {newName}, {friendId})");
            if (friendListDictionary.ContainsKey(characterId) == false) {
                return;
            }
            FriendList friendList = friendListDictionary[characterId];
            foreach (CharacterSummaryData characterSummaryData in friendList.MemberIdList.Values) {
                int friendCharacterId = characterSummaryData.CharacterId;
                if (friendListDictionary.ContainsKey(friendCharacterId) == false) {
                    continue;
                }
                FriendList targetFriendList = friendListDictionary[friendCharacterId];
                if (targetFriendList.MemberIdList.ContainsKey(characterId) == false) {
                    continue;
                }
                targetFriendList.MemberIdList[characterId].CharacterName = newName;
                SaveFriendListFile(targetFriendList);

                // return zero if the friend is not logged in to avoid trying sent info to offline accounts
                int targetAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(friendCharacterId);
                if (targetAccountId == -1) {
                    continue;
                }
                if (characterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseRenameCharacterInFriendList(targetAccountId, characterId, newName);
            }
        }
        */

        public void ProcessStatusChange(int playerCharacterId) {
            //Debug.Log($"FriendServiceServer.SetCharacterOnline({playerCharacterId}, {isOnline})");

            if (friendListDictionary.ContainsKey(playerCharacterId) == false) {
                return;
            }
            FriendList friendList = friendListDictionary[playerCharacterId];
            foreach (CharacterSummaryData characterSummaryData in friendList.MemberIdList.Values) {
                int friendCharacterId = characterSummaryData.CharacterId;
                if (friendListDictionary.ContainsKey(friendCharacterId) == false) {
                    continue;
                }
                FriendList targetFriendList = friendListDictionary[friendCharacterId];
                if (targetFriendList.MemberIdList.ContainsKey(playerCharacterId) == false) {
                    continue;
                }
                CharacterSummaryData targetCharacterSummaryData = targetFriendList.MemberIdList[playerCharacterId];

                // return zero if the friend is not logged in to avoid trying sent info to offline accounts
                int targetAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(friendCharacterId);
                if (targetAccountId == -1) {
                    continue;
                }
                if (characterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseFriendStateChange(targetAccountId, playerCharacterId, new CharacterSummaryNetworkData(targetCharacterSummaryData));
            }
        }

        /*
        public void ProcessLevelChanged(UnitController unitController) {
            int playerCharacterId = unitController.CharacterId;
            if (friendListDictionary.ContainsKey(playerCharacterId) == false) {
                return;
            }
            FriendList friendList = friendListDictionary[playerCharacterId];
            foreach (CharacterSummaryData characterSummaryData in friendList.MemberIdList.Values) {
                int friendCharacterId = characterSummaryData.CharacterId;
                if (friendListDictionary.ContainsKey(friendCharacterId) == false) {
                    continue;
                }
                FriendList targetFriendList = friendListDictionary[friendCharacterId];
                if (targetFriendList.MemberIdList.ContainsKey(playerCharacterId) == false) {
                    continue;
                }
                CharacterSummaryData targetCharacterSummaryData = targetFriendList.MemberIdList[playerCharacterId];
                targetCharacterSummaryData.Level = unitController.CharacterStats.Level;

                // return zero if the friend is not logged in to avoid trying sent info to offline accounts
                int targetAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(friendCharacterId);
                if (targetAccountId == -1) {
                    continue;
                }
                if (characterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseFriendStateChange(targetAccountId, playerCharacterId, new CharacterSummaryNetworkData(targetCharacterSummaryData));
            }
        }
        */

    }

}