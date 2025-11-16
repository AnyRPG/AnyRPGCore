using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CharacterGroupServiceServer : ConfiguredClass {

        int nextCharacterGroupId = 1;

        /// <summary>
        /// characterId, characterGroupId
        /// </summary>
        private Dictionary<int, int> characterGroupInvites = new Dictionary<int, int>();

        /// <summary>
        /// characterGroupId, CharacterGroup
        /// </summary>
        private Dictionary<int, CharacterGroup> characterGroupDictionary = new Dictionary<int, CharacterGroup>();

        /// <summary>
        /// playerCharacterId, characterGroupId
        /// </summary>
        private Dictionary<int, int> characterGroupMemberLookup = new Dictionary<int, int>();

        // game manager references
        private PlayerManagerServer playerManagerServer = null;
        private PlayerCharacterService playerCharacterService = null;
        private CharacterManager characterManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            playerCharacterService = systemGameManager.PlayerCharacterService;
            characterManager = systemGameManager.CharacterManager;
        }

        public int GetCharacterGroupIdFromCharacterId(int characterId) {
            if (characterGroupMemberLookup.ContainsKey(characterId)) {
                return characterGroupMemberLookup[characterId];
            }
            return -1;
        }

        public CharacterGroup GetCharacterGroupFromCharacterId(int characterId) {
            if (characterGroupMemberLookup.ContainsKey(characterId) && characterGroupDictionary.ContainsKey(characterGroupMemberLookup[characterId])) {
                return characterGroupDictionary[characterGroupMemberLookup[characterId]];
            }
            return null;
        }

        public void CreateCharacterGroup(int leaderCharacterId) {
            //Debug.Log($"CharacterGroupService.CreateCharacterGroup({leaderCharacterId})");

            UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, leaderCharacterId);
            if (unitController == null) {
                return;
            }
            CharacterGroup characterGroup = new CharacterGroup(nextCharacterGroupId, leaderCharacterId, unitController.DisplayName);
            nextCharacterGroupId++;
            characterGroupDictionary.Add(characterGroup.characterGroupId, characterGroup);
            characterGroupMemberLookup.Add(leaderCharacterId, characterGroup.characterGroupId);
            unitController?.CharacterGroupManager.SetGroupId(characterGroup.characterGroupId);
        }

        public void AddCharacterToGroup(int characterId, int characterGroupId) {
            //Debug.Log($"CharacterGroupService.AddCharacterToGroup({characterId}, {characterGroupId})");

            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning("CharacterGroupService.AddCharacterToGroup: character group not found");
                return;
            }

            UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, characterId);
            if (unitController == null) {
                Debug.LogWarning($"CharacterGroupService.AddCharacterToGroup: unit controller not found for characterId {characterId}");
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            characterGroup.AddPlayer(characterId, unitController.DisplayName);
            characterGroupMemberLookup.Add(characterId, characterGroupId);
            unitController?.CharacterGroupManager.SetGroupId(characterGroup.characterGroupId);
            networkManagerServer.AdvertiseAddCharacterToGroup(characterId, characterGroup);
        }

        public void RemoveCharacterFromGroup(int characterId) {
            //Debug.Log($"CharacterGroupService.RemoveCharacterFromGroup({characterId})");

            if (characterGroupMemberLookup.ContainsKey(characterId) == false) {
                //Debug.Log("CharacterGroupService.RemoveCharacterFromGroup: character group member not found");
                return;
            }
            int characterGroupId = characterGroupMemberLookup[characterId];
            RemoveCharacterFromGroup(characterId, characterGroupId);
        }

        public void RemoveCharacterFromGroup(int characterId, int characterGroupId) {
            //Debug.Log($"CharacterGroupService.RemoveCharacterFromGroup({characterId}, {characterGroupId})");

            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning("CharacterGroupService.RemoveCharacterFromGroup: character group not found");
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            characterGroup.RemovePlayer(characterId);
            characterGroupMemberLookup.Remove(characterId);
            UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, characterId);
            unitController?.CharacterGroupManager.LeaveGroup();
            networkManagerServer.AdvertiseRemoveCharacterFromGroup(characterId, characterGroup);

            // if only one member remains, disband the group
            if (characterGroup.CharacterIdList[UnitControllerMode.Player].Count == 1) {
                DisbandGroup(characterGroupId);
                return;
            }

            // if the leader left, promote a new leader
            if (characterGroup.leaderPlayerCharacterId == characterId) {
                PromoteLeader(characterGroupId, characterId, characterGroup.CharacterIdList[UnitControllerMode.Player].First().Key);
            }
        }

        public void DisbandGroup(int accountId, int characterGroupId) {
            //Debug.Log($"CharacterGroupService.DisbandGroup({accountId}, {characterGroupId})");

            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                return;
            }
            int leaderPlayerCharacterid = playerManagerServer.GetPlayerCharacterId(accountId);
            if (leaderPlayerCharacterid == 0) {
                //Debug.Log($"CharacterGroupService.DisbandGroup: player character not found for accountId {accountId}");
                return;
            }
            if (leaderPlayerCharacterid != characterGroupDictionary[characterGroupId].leaderPlayerCharacterId) {
                Debug.Log("CharacterGroupService.DisbandGroup: only the group leader can disband the group");
                return;
            }

            DisbandGroup(characterGroupId);
        }

        public void DisbandGroup(int characterGroupId) {

            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            networkManagerServer.AdvertiseDisbandCharacterGroup(characterGroup);

            foreach (int characterId in characterGroup.CharacterIdList[UnitControllerMode.Player].Keys) {
                characterGroupMemberLookup.Remove(characterId);
                UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, characterId);
                unitController?.CharacterGroupManager.LeaveGroup();
            }
            characterGroupDictionary.Remove(characterGroupId);

            // remove invites
            List<int> keysToRemove = new List<int>();
            foreach (KeyValuePair<int, int> invite in characterGroupInvites) {
                if (invite.Value == characterGroupId) {
                    keysToRemove.Add(invite.Key);
                }
            }
            foreach (int key in keysToRemove) {
                characterGroupInvites.Remove(key);
            }
        }



        public void AcceptCharacterGroupInvite(int accountId, int characterGroupId) {
            //Debug.Log($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}, {characterGroupId})");

            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.Log($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}, {characterGroupId}) character group not found");
                return;
            }
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == 0) {
                Debug.Log($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}, {characterGroupId}) player character not found for account");
                return;
            }
            if (characterGroupInvites.ContainsKey(playerCharacterId) == false || characterGroupInvites[playerCharacterId] != characterGroupId) {
                Debug.Log($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}, {characterGroupId}) character group invite not found");
                return;
            }
            characterGroupInvites.Remove(playerCharacterId);
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            
            // check if the leader is the only person in the group and notify 
            if (characterGroup.CharacterIdList[UnitControllerMode.Player].Count == 1 &&
                characterGroup.leaderPlayerCharacterId == characterGroup.CharacterIdList[UnitControllerMode.Player].First().Key) {
                networkManagerServer.AdvertiseAddCharacterToGroup(characterGroup.leaderPlayerCharacterId, characterGroup);
            }

            // add the new member
            AddCharacterToGroup(playerCharacterId, characterGroupId);
        }

        public void DeclineCharacterGroupInvite(int accountId) {
            //Debug.Log($"CharacterGroupService.DeclineCharacterGroupInvite({accountId})");

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == 0) {
                Debug.Log($"CharacterGroupService.DeclineCharacterGroupInvite({accountId}) player character not found for accountId");
                return;
            }
            if (characterGroupInvites.ContainsKey(playerCharacterId) == true) {
                // check if the leader is the only person in the group and silenty disband
                if (characterGroupDictionary.ContainsKey(characterGroupInvites[playerCharacterId])) {
                    CharacterGroup characterGroup = characterGroupDictionary[characterGroupInvites[playerCharacterId]];
                    if (characterGroup.CharacterIdList[UnitControllerMode.Player].Count == 1 &&
                        characterGroup.leaderPlayerCharacterId == characterGroup.CharacterIdList[UnitControllerMode.Player].First().Key) {
                        characterGroupDictionary.Remove(characterGroup.characterGroupId);
                        characterGroupMemberLookup.Remove(characterGroup.leaderPlayerCharacterId);
                        UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, characterGroup.leaderPlayerCharacterId);
                        unitController?.CharacterGroupManager.LeaveGroup();
                    }
                    int leaderAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterGroup.leaderPlayerCharacterId);
                    networkManagerServer.AdvertiseDeclineCharacterGroupInvite(leaderAccountId, playerCharacterService.GetPlayerNameFromId(playerCharacterId));
                }
                characterGroupInvites.Remove(playerCharacterId);
            }
        }

        public void RequestLeaveCharacterGroup(int accountId) {
            //Debug.Log($"CharacterGroupService.RequestLeaveCharacterGroup({accountId})");

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == 0) {
                //Debug.LogWarning($"CharacterGroupService.RequestLeaveCharacterGroup: player character not found for accountId {accountId}");
                return;
            }
            RemoveCharacterFromGroup(playerCharacterId);
        }

        public void RequestRemoveCharacterFromGroup(int leaderAccountId, int removedCharacterId) {
            int leaderPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(leaderAccountId);
            if (leaderPlayerCharacterId == 0) {
                //Debug.LogWarning($"CharacterGroupService.RequestInviteCharacterToGroup: player character not found for accountId {accountId}");
                return;
            }
            if (characterGroupMemberLookup.ContainsKey(removedCharacterId) == false) {
                Debug.LogWarning("CharacterGroupService.RequestRemoveCharacterFromGroup: character group member not found");
                return;
            }
            int characterGroupId = characterGroupMemberLookup[removedCharacterId];
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning("CharacterGroupService.RequestRemoveCharacterFromGroup: character group not found");
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            if (leaderPlayerCharacterId != characterGroup.leaderPlayerCharacterId) {
                Debug.Log("CharacterGroupService.RequestRemoveCharacterFromGroup: only the group leader can remove members from the group");
                return;
            }
            RemoveCharacterFromGroup(removedCharacterId);
        }

        public void RequestInviteCharacterToGroup(int leaderAccountId, int invitedCharacterId) {
            //Debug.Log($"CharacterGroupService.RequestInviteCharacterToGroup({leaderAccountId}, {invitedCharacterId})");

            int leaderPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(leaderAccountId);
            if (leaderPlayerCharacterId == 0) {
                Debug.Log($"CharacterGroupService.RequestInviteCharacterToGroup: player character not found for leader accountId {leaderAccountId}");
                return;
            }

            if (characterGroupMemberLookup.ContainsKey(leaderPlayerCharacterId) == false) {
                CreateCharacterGroup(leaderPlayerCharacterId);
            }

            int characterGroupId = characterGroupMemberLookup[leaderPlayerCharacterId];
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.Log($"CharacterGroupService.RequestInviteCharacterToGroup({leaderAccountId}, {invitedCharacterId}) character group ({characterGroupId}) not found");
                return;
            }

            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            if (leaderPlayerCharacterId != characterGroup.leaderPlayerCharacterId) {
                Debug.Log($"CharacterGroupService.RequestInviteCharacterToGroup({leaderAccountId}, {invitedCharacterId}) only the group leader can invite members to the group");
                return;
            }

            characterGroupInvites[invitedCharacterId] = characterGroupId;
            string leaderName = playerManagerServer.GetPlayerName(leaderAccountId);
            if (leaderName == string.Empty) {
                leaderName = "Unknown";
            }
            networkManagerServer.AdvertiseCharacterGroupInvite(invitedCharacterId, characterGroup, leaderName);
        }

        public void SendCharacterGroupInfo(int accountId, int playerCharacterId) {
            //Debug.Log($"CharacterGroupService.SendCharacterGroupInfo({accountId}, {playerCharacterId})");

            if (characterGroupMemberLookup.ContainsKey(playerCharacterId) == false) {
                //Debug.Log($"CharacterGroupService.SendCharacterGroupInfo: character group member not found for playerCharacterId {playerCharacterId}");
                return;
            }
            int characterGroupId = characterGroupMemberLookup[playerCharacterId];
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                //Debug.Log("CharacterGroupService.RequestInviteCharacterToGroup: character group not found");
                return;
            }

            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            networkManagerServer.AdvertiseCharacterGroup(accountId, characterGroup);
        }

        public void PromoteLeader(int groupId, int oldLeaderCharacterId, int newLeaderCharacterId) {
            //Debug.Log($"CharacterGroupService.PromoteLeader({groupId}, {oldLeaderCharacterId}, {newLeaderCharacterId})");
            if (characterGroupDictionary.ContainsKey(groupId) == false) {
                Debug.LogWarning("CharacterGroupService.PromoteLeader: character group not found");
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[groupId];
            if (characterGroup.leaderPlayerCharacterId != oldLeaderCharacterId) {
                Debug.LogWarning("CharacterGroupService.PromoteLeader: only the current leader can promote a new leader");
                return;
            }
            if (characterGroup.CharacterIdList[UnitControllerMode.Player].ContainsKey(newLeaderCharacterId) == false) {
                Debug.LogWarning("CharacterGroupService.PromoteLeader: new leader must be a member of the group");
                return;
            }
            characterGroup.leaderPlayerCharacterId = newLeaderCharacterId;
            networkManagerServer.AdvertisePromoteLeader(characterGroup, newLeaderCharacterId);
        }

        public void RequestPromoteCharacterToLeader(int requestingAccountId, int newLeaderCharacterId) {
            
            int requestingPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(requestingAccountId);
            if (requestingPlayerCharacterId == 0) {
                Debug.Log($"CharacterGroupService.RequestInviteCharacterToGroup: player character not found for leader accountId {requestingAccountId}");
                return;
            }

            if (characterGroupMemberLookup.ContainsKey(requestingPlayerCharacterId) == false) {
                Debug.LogWarning("CharacterGroupService.RequestPromoteCharacterToLeader: character group member not found");
            }

            int characterGroupId = characterGroupMemberLookup[requestingPlayerCharacterId];
            PromoteLeader(characterGroupId, requestingPlayerCharacterId, newLeaderCharacterId);
        }

        public void ProcessRenameCharacter(int characterId, string newName, int groupId) {
            //Debug.Log($"CharacterGroupService.ProcessRenameCharacter({characterId}, {newName}, {groupId})");
            if (characterGroupDictionary.ContainsKey(groupId) == false) {
                Debug.LogWarning("CharacterGroupService.ProcessRenameCharacter: character group not found");
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[groupId];
            if (characterGroup.CharacterIdList[UnitControllerMode.Player].ContainsKey(characterId) == false) {
                Debug.LogWarning("CharacterGroupService.ProcessRenameCharacter: character group member not found");
                return;
            }
            characterGroup.CharacterIdList[UnitControllerMode.Player][characterId] = newName;
            networkManagerServer.AdvertiseRenameCharacterInGroup(characterGroup, characterId, newName);
        }
    }

}