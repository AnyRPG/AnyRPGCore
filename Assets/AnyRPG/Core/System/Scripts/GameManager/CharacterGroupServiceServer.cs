using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

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

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            playerCharacterService = systemGameManager.PlayerCharacterService;
        }

        public void CreateCharacterGroup(int leaderCharacterId) {
            Debug.Log($"CharacterGroupService.CreateCharacterGroup({leaderCharacterId})");

            CharacterGroup characterGroup = new CharacterGroup(nextCharacterGroupId, leaderCharacterId);
            nextCharacterGroupId++;
            characterGroupDictionary.Add(characterGroup.characterGroupId, characterGroup);
            characterGroupMemberLookup.Add(leaderCharacterId, characterGroup.characterGroupId);
        }

        public void AddCharacterToGroup(int characterId, int characterGroupId) {
            Debug.Log($"CharacterGroupService.AddCharacterToGroup({characterId}, {characterGroupId})");

            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning("CharacterGroupService.AddCharacterToGroup: character group not found");
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            characterGroup.AddPlayer(characterId);
            characterGroupMemberLookup.Add(characterId, characterGroupId);
            networkManagerServer.AdvertiseAddCharacterToGroup(characterId, characterGroup);
        }

        public void RemoveCharacterFromGroup(int characterId) {
            Debug.Log($"CharacterGroupService.RemoveCharacterFromGroup({characterId})");

            if (characterGroupMemberLookup.ContainsKey(characterId) == false) {
                Debug.LogWarning("CharacterGroupService.RemoveCharacterFromGroup: character group member not found");
                return;
            }
            int characterGroupId = characterGroupMemberLookup[characterId];
            RemoveCharacterFromGroup(characterId, characterGroupId);
        }

        public void RemoveCharacterFromGroup(int characterId, int characterGroupId) {
            Debug.Log($"CharacterGroupService.RemoveCharacterFromGroup({characterId}, {characterGroupId})");

            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning("CharacterGroupService.RemoveCharacterFromGroup: character group not found");
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            characterGroup.RemovePlayer(characterId);
            characterGroupMemberLookup.Remove(characterId);
            networkManagerServer.AdvertiseRemoveCharacterFromGroup(characterId, characterGroup);
        }

        public void DisbandGroup(int accountId, int characterGroupId) {
            Debug.Log($"CharacterGroupService.DisbandGroup({accountId}, {characterGroupId})");

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

            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            networkManagerServer.AdvertiseDisbandCharacterGroup(characterGroup);

            foreach (int characterId in characterGroup.CharacterIdList[UnitControllerMode.Player]) {
                characterGroupMemberLookup.Remove(characterId);
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
            Debug.Log($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}, {characterGroupId})");

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
                characterGroup.leaderPlayerCharacterId == characterGroup.CharacterIdList[UnitControllerMode.Player][0]) {
                networkManagerServer.AdvertiseAddCharacterToGroup(characterGroup.leaderPlayerCharacterId, characterGroup);
            }

            // add the new member
            AddCharacterToGroup(playerCharacterId, characterGroupId);
        }

        public void DeclineCharacterGroupInvite(int accountId) {
            Debug.Log($"CharacterGroupService.DeclineCharacterGroupInvite({accountId})");

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
                        characterGroup.leaderPlayerCharacterId == characterGroup.CharacterIdList[UnitControllerMode.Player][0]) {
                        characterGroupDictionary.Remove(characterGroup.characterGroupId);
                        characterGroupMemberLookup.Remove(characterGroup.leaderPlayerCharacterId);
                    }
                    int leaderAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterGroup.leaderPlayerCharacterId);
                    networkManagerServer.AdvertiseDeclineCharacterGroupInvite(leaderAccountId, playerCharacterService.GetPlayerNameFromId(playerCharacterId));
                }
                characterGroupInvites.Remove(playerCharacterId);
            }
        }

        public void RequestLeaveCharacterGroup(int accountId) {
            Debug.Log($"CharacterGroupService.RequestLeaveCharacterGroup({accountId})");

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
            Debug.Log($"CharacterGroupService.RequestInviteCharacterToGroup({leaderAccountId}, {invitedCharacterId})");

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
                Debug.Log("CharacterGroupService.RequestInviteCharacterToGroup({leaderAccountId}, {invitedCharacterId}) only the group leader can invite members to the group");
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
            Debug.Log($"CharacterGroupService.SendCharacterGroupInfo({accountId}, {playerCharacterId})");

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
    }

}