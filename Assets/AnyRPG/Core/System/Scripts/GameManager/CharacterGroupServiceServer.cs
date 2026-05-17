using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        private CharacterManager characterManager = null;
        private MessageLogServer messageLogServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            characterManager = systemGameManager.CharacterManager;
            messageLogServer = systemGameManager.MessageLogServer;
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
            CharacterGroup characterGroup = new CharacterGroup(nextCharacterGroupId, new CharacterGroupMemberData(playerCharacterService.GetSummaryData(leaderCharacterId), CharacterGroupRank.Leader));
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
            CharacterGroupMemberData characterGroupMemberData = new CharacterGroupMemberData(playerCharacterService.GetSummaryData(characterId));
            characterGroup.AddPlayer(characterGroupMemberData);
            characterGroupMemberLookup.Add(characterId, characterGroupId);
            unitController?.CharacterGroupManager.SetGroupId(characterGroup.characterGroupId);
            int accountId = playerManagerServer.GetAccountIdFromUnitController(unitController);
            if (accountId != -1) {
                networkManagerServer.AdvertiseCharacterGroup(accountId, new CharacterGroupNetworkData(characterGroup));
            }
            foreach (CharacterGroupMemberData _characterGroupMemberData in characterGroup.MemberList[UnitControllerMode.Player].Values) {
                // get account id from player id, zero means the player is logged out
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(_characterGroupMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (_characterGroupMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseAddCharacterToGroup(memberAccountId, characterGroupId, new CharacterGroupMemberNetworkData(characterGroupMemberData));
            }
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
            foreach (CharacterGroupMemberData characterGroupMemberData in characterGroup.MemberList[UnitControllerMode.Player].Values) {
                // get account id from player id, zero means the player is logged out
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterGroupMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (characterGroupMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseRemoveCharacterFromGroup(memberAccountId, characterId, characterGroupId);
            }
            int removedAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterId);
            if (removedAccountId != -1) {
                networkManagerServer.AdvertiseRemoveCharacterFromGroup(removedAccountId, characterId, characterGroupId);
            }

            // if only one member remains, disband the group
            if (characterGroup.MemberList[UnitControllerMode.Player].Count == 1) {
                DisbandGroup(characterGroupId);
                return;
            }

            // if the leader left, promote a new leader
            if (characterGroup.leaderPlayerCharacterId == characterId) {
                PromoteCharacterToLeader(characterGroupId, characterGroup.MemberList[UnitControllerMode.Player].First().Key);
            }
        }

        public void DisbandGroupByAccountId(int accountId) {
            int leaderPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (leaderPlayerCharacterId == -1) {
                //Debug.Log($"CharacterGroupService.DisbandGroup: player character not found for accountId {accountId}");
                return;
            }
            if (characterGroupMemberLookup.ContainsKey(leaderPlayerCharacterId) == false) {
                return;
            }
            int characterGroupId = characterGroupMemberLookup[leaderPlayerCharacterId];
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning("CharacterGroupService.DisbandGroupByAccountId: character group not found");
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            if (leaderPlayerCharacterId != characterGroup.leaderPlayerCharacterId) {
                Debug.LogWarning("CharacterGroupService.DisbandGroupByAccountId: only the group leader can disband the group");
                return;
            }
            DisbandGroup(characterGroupId);
        }

        public void DisbandGroup(int accountId, int characterGroupId) {
            //Debug.Log($"CharacterGroupService.DisbandGroup({accountId}, {characterGroupId})");

            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                return;
            }
            int leaderPlayerCharacterid = playerManagerServer.GetPlayerCharacterId(accountId);
            if (leaderPlayerCharacterid == -1) {
                //Debug.Log($"CharacterGroupService.DisbandGroup: player character not found for accountId {accountId}");
                return;
            }
            if (leaderPlayerCharacterid != characterGroupDictionary[characterGroupId].leaderPlayerCharacterId) {
                Debug.LogWarning("CharacterGroupService.DisbandGroup: only the group leader can disband the group");
                return;
            }

            DisbandGroup(characterGroupId);
        }

        public void DisbandGroup(int characterGroupId) {

            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];

            foreach (CharacterGroupMemberData characterGroupMemberData in characterGroup.MemberList[UnitControllerMode.Player].Values) {
                characterGroupMemberLookup.Remove(characterGroupMemberData.CharacterSummaryData.CharacterId);

                // get account id from player id, zero means the player is logged out
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterGroupMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (characterGroupMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseDisbandCharacterGroup(memberAccountId, characterGroupId);
                UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, characterGroupMemberData.CharacterSummaryData.CharacterId);
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
                Debug.LogWarning($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}, {characterGroupId}) character group not found");
                return;
            }
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                Debug.LogWarning($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}, {characterGroupId}) player character not found for account");
                return;
            }
            if (characterGroupInvites.ContainsKey(playerCharacterId) == false || characterGroupInvites[playerCharacterId] != characterGroupId) {
                Debug.LogWarning($"CharacterGroupService.AcceptCharacterGroupInvite({accountId}, {characterGroupId}) character group invite not found");
                return;
            }
            characterGroupInvites.Remove(playerCharacterId);
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            
            // check if the leader is the only person in the group and notify 
            if (characterGroup.MemberList[UnitControllerMode.Player].Count == 1 &&
                characterGroup.leaderPlayerCharacterId == characterGroup.MemberList[UnitControllerMode.Player].First().Key) {
                int leaderAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterGroup.leaderPlayerCharacterId);
                networkManagerServer.AdvertiseCharacterGroup(leaderAccountId, new CharacterGroupNetworkData(characterGroup));
                networkManagerServer.AdvertiseAddCharacterToGroup(leaderAccountId, characterGroupId, new CharacterGroupMemberNetworkData(characterGroup.MemberList[UnitControllerMode.Player].First().Value));
            }

            // add the new member
            AddCharacterToGroup(playerCharacterId, characterGroupId);
        }

        public void DeclineCharacterGroupInvite(int accountId) {
            //Debug.Log($"CharacterGroupService.DeclineCharacterGroupInvite({accountId})");

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                Debug.LogWarning($"CharacterGroupService.DeclineCharacterGroupInvite({accountId}) player character not found for accountId");
                return;
            }
            if (characterGroupInvites.ContainsKey(playerCharacterId) == true) {
                // check if the leader is the only person in the group and silenty disband
                if (characterGroupDictionary.ContainsKey(characterGroupInvites[playerCharacterId])) {
                    CharacterGroup characterGroup = characterGroupDictionary[characterGroupInvites[playerCharacterId]];
                    if (characterGroup.MemberList[UnitControllerMode.Player].Count == 1 &&
                        characterGroup.leaderPlayerCharacterId == characterGroup.MemberList[UnitControllerMode.Player].First().Key) {
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
            if (playerCharacterId == -1) {
                //Debug.LogWarning($"CharacterGroupService.RequestLeaveCharacterGroup: player character not found for accountId {accountId}");
                return;
            }
            RemoveCharacterFromGroup(playerCharacterId);
        }

        public void RequestRemoveCharacterFromGroup(int leaderAccountId, string playerName) {
            int inviteCharacterId = playerCharacterService.GetPlayerIdFromName(playerName);
            if (inviteCharacterId != -1) {
                RequestRemoveCharacterFromGroup(leaderAccountId, inviteCharacterId);
            }
        }

        public void RequestRemoveCharacterFromGroup(int actingAccountId, int removedCharacterId) {
            int actingPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(actingAccountId);
            if (actingPlayerCharacterId == -1) {
                //Debug.LogWarning($"CharacterGroupServiceServer.RequestRemoveCharacterFromGroup: player character not found for accountId {accountId}");
                return;
            }
            if (characterGroupMemberLookup.ContainsKey(removedCharacterId) == false) {
                Debug.LogWarning("CharacterGroupServiceServer.RequestRemoveCharacterFromGroup: character group member not found");
                return;
            }
            int characterGroupId = characterGroupMemberLookup[removedCharacterId];
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning("CharacterGroupServiceServer.RequestRemoveCharacterFromGroup: character group not found");
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];

            CharacterGroupMemberData actingCharacterGroupMemberData = characterGroup.MemberList[UnitControllerMode.Player][actingPlayerCharacterId];
            CharacterGroupMemberData removedCharacterGroupMemberData = characterGroup.MemberList[UnitControllerMode.Player][removedCharacterId];
            if (actingCharacterGroupMemberData.Rank == CharacterGroupRank.Member) {
                Debug.LogWarning("CharacterGroupServiceServer.RequestRemoveCharacterFromGroup: only the leader and assistants can remove members from the group");
                return;
            }
            if (actingCharacterGroupMemberData.Rank == CharacterGroupRank.Assistant && removedCharacterGroupMemberData.Rank != CharacterGroupRank.Member) {
                Debug.LogWarning("CharacterGroupServiceServer.RequestRemoveCharacterFromGroup: assistants can only remove members from the group");
                return;
            }
            RemoveCharacterFromGroup(removedCharacterId);
        }

        public void RequestInviteCharacterToGroup(int leaderAccountId, string playerName) {
            int inviteCharacterId = playerCharacterService.GetPlayerIdFromName(playerName);
            if (inviteCharacterId != -1) {
                RequestInviteCharacterToGroup(leaderAccountId, inviteCharacterId);
            }
        }

        public void RequestInviteCharacterToGroup(int actingAccountId, int invitedCharacterId) {
            //Debug.Log($"CharacterGroupService.RequestInviteCharacterToGroup({leaderAccountId}, {invitedCharacterId})");

            int actingPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(actingAccountId);
            if (actingPlayerCharacterId == -1) {
                Debug.LogWarning($"CharacterGroupService.RequestInviteCharacterToGroup: player character not found for leader accountId {actingAccountId}");
                return;
            }

            if (actingPlayerCharacterId == invitedCharacterId) {
                return;
            }

            if (characterGroupMemberLookup.ContainsKey(actingPlayerCharacterId) == false) {
                CreateCharacterGroup(actingPlayerCharacterId);
            }

            int characterGroupId = characterGroupMemberLookup[actingPlayerCharacterId];
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning($"CharacterGroupService.RequestInviteCharacterToGroup({actingAccountId}, {invitedCharacterId}) character group ({characterGroupId}) not found");
                return;
            }

            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            CharacterGroupMemberData actingCharacterMemberData = characterGroup.MemberList[UnitControllerMode.Player][actingPlayerCharacterId];
            if (actingCharacterMemberData.Rank == CharacterGroupRank.Member) {
                Debug.LogWarning($"CharacterGroupService.RequestInviteCharacterToGroup({actingAccountId}, {invitedCharacterId}) only the group leader and assistants can invite members to the group");
                return;
            }

            characterGroupInvites[invitedCharacterId] = characterGroupId;
            string leaderName = playerManagerServer.GetPlayerName(actingAccountId);
            if (leaderName == string.Empty) {
                leaderName = "Unknown";
            }
            networkManagerServer.AdvertiseCharacterGroupInvite(invitedCharacterId, characterGroup.characterGroupId, leaderName);
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
            networkManagerServer.AdvertiseCharacterGroup(accountId, new CharacterGroupNetworkData(characterGroup));
        }

        /*
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
            string newLeaderName = playerCharacterService.GetPlayerNameFromId(newLeaderCharacterId);

            foreach (CharacterGroupMemberData characterSummaryData in characterGroup.CharacterIdList[UnitControllerMode.Player].Values) {
                // get account id from player id, zero means the player is logged out
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (characterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertisePromoteGroupLeader(memberAccountId, characterGroup.characterGroupId, newLeaderCharacterId);
                if (newLeaderCharacterId == characterSummaryData.CharacterId) {
                    messageLogServer.WriteSystemMessage(memberAccountId, "You are now the group leader.");
                } else {
                    messageLogServer.WriteSystemMessage(memberAccountId, $"{newLeaderName} is now the group leader.");
                }
            }
        }
        */

        public void RequestPromoteCharacter(int leaderAccountId, string playerName) {
            int inviteCharacterId = playerCharacterService.GetPlayerIdFromName(playerName);
            if (inviteCharacterId != -1) {
                RequestPromoteCharacter(leaderAccountId, inviteCharacterId);
            }
        }

        public void RequestPromoteCharacter(int requestingAccountId, int promotedCharacterId) {
            
            int requestingPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(requestingAccountId);
            if (requestingPlayerCharacterId == -1) {
                Debug.LogWarning($"CharacterGroupService.RequestInviteCharacterToGroup: player character not found for leader accountId {requestingAccountId}");
                return;
            }

            if (characterGroupMemberLookup.ContainsKey(requestingPlayerCharacterId) == false) {
                Debug.LogWarning("CharacterGroupService.RequestPromoteCharacter: character group member not found");
            }

            int characterGroupId = characterGroupMemberLookup[requestingPlayerCharacterId];
            PromoteCharacter(characterGroupId, requestingPlayerCharacterId, promotedCharacterId);
        }

        public void RequestDemoteCharacter(int actingAccountId, string playerName) {
            int demoteCharacterId = playerCharacterService.GetPlayerIdFromName(playerName);
            if (demoteCharacterId != -1) {
                RequestDemoteCharacter(actingAccountId, demoteCharacterId);
            }
        }

        public void RequestDemoteCharacter(int requestingAccountId, int demotedCharacterId) {

            int requestingPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(requestingAccountId);
            if (requestingPlayerCharacterId == -1) {
                Debug.LogWarning($"CharacterGroupService.RequestInviteCharacterToGroup: player character not found for leader accountId {requestingAccountId}");
                return;
            }

            if (characterGroupMemberLookup.ContainsKey(requestingPlayerCharacterId) == false) {
                Debug.LogWarning("CharacterGroupService.RequestDemoteCharacter: character group member not found");
            }

            int characterGroupId = characterGroupMemberLookup[requestingPlayerCharacterId];
            DemoteCharacter(characterGroupId, requestingPlayerCharacterId, demotedCharacterId);
        }

        public void ProcessRenameCharacter(int characterId, string newName, int groupId) {
            //Debug.Log($"CharacterGroupService.ProcessRenameCharacter({characterId}, {newName}, {groupId})");
            if (characterGroupDictionary.ContainsKey(groupId) == false) {
                Debug.LogWarning("CharacterGroupService.ProcessRenameCharacter: character group not found");
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[groupId];
            if (characterGroup.MemberList[UnitControllerMode.Player].ContainsKey(characterId) == false) {
                Debug.LogWarning("CharacterGroupService.ProcessRenameCharacter: character group member not found");
                return;
            }
            characterGroup.MemberList[UnitControllerMode.Player][characterId].CharacterSummaryData.CharacterName = newName;

            foreach (CharacterGroupMemberData characterGroupMemberData in characterGroup.MemberList[UnitControllerMode.Player].Values) {
                // get account id from player id, zero means the player is logged out
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterGroupMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (characterGroupMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseRenameCharacterInGroup(memberAccountId, groupId, characterId, newName);
            }

        }

        public void ProcessStatusChange(int playerCharacterId) {
            // check if character is in a group
            if (characterGroupMemberLookup.ContainsKey(playerCharacterId) == false) {
                return;
            }
            int characterGroupId = characterGroupMemberLookup[playerCharacterId];
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            if (characterGroup.MemberList[UnitControllerMode.Player].ContainsKey(playerCharacterId) == false) {
                return;
            }
            CharacterGroupMemberData targetCharacterGroupMemberData = characterGroup.MemberList[UnitControllerMode.Player][playerCharacterId];

            foreach (CharacterGroupMemberData characterGroupMemberData in characterGroup.MemberList[UnitControllerMode.Player].Values) {
                // get account id from player id, zero means the player is logged out
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterGroupMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (characterGroupMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseGroupMemberStatusChange(memberAccountId, characterGroupId, playerCharacterId, new CharacterGroupMemberNetworkData(targetCharacterGroupMemberData));
            }

        }

        public void PromoteCharacter(int characterGroupId, int actingCharacterId, int promotedCharacterId) {
            //Debug.Log($"CharacterGroupServiceServer.PromoteLeader({characterGroupId}, {actingCharacterId}, {promotedCharacterId})");
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning("CharacterGroupServiceServer.PromoteCharacter: character group not found");
                return;
            }

            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            if (characterGroup.MemberList[UnitControllerMode.Player].ContainsKey(promotedCharacterId) == false) {
                Debug.LogWarning("CharacterGroupServiceServer.PromoteCharacter: new leader must be a member of the group");
                return;
            }

            CharacterGroupMemberData actingCharacterData = characterGroup.MemberList[UnitControllerMode.Player][actingCharacterId];
            CharacterGroupMemberData promotedCharacterData = characterGroup.MemberList[UnitControllerMode.Player][promotedCharacterId];

            if (actingCharacterData.Rank == CharacterGroupRank.Member) {
                Debug.LogWarning("CharacterGroupServiceServer.PromoteCharacter: only assistants and leaders can promote members");
                return;
            } else if (actingCharacterData.Rank == CharacterGroupRank.Assistant) {
                if (promotedCharacterData.Rank != CharacterGroupRank.Member) {
                    Debug.LogWarning("CharacterGroupServiceServer.PromoteCharacter: assistants can only promote members to assistants");
                    return;
                }
                promotedCharacterData.Rank = CharacterGroupRank.Assistant;
            } else if (actingCharacterData.Rank == CharacterGroupRank.Leader) {
                if (promotedCharacterData.Rank == CharacterGroupRank.Member) {
                    promotedCharacterData.Rank = CharacterGroupRank.Assistant;
                } else if (promotedCharacterData.Rank == CharacterGroupRank.Assistant) {
                    promotedCharacterData.Rank = CharacterGroupRank.Leader;
                    actingCharacterData.Rank = CharacterGroupRank.Assistant;
                    characterGroup.leaderPlayerCharacterId = promotedCharacterId;
                }
            }

            ProcessPromoteCharacter(characterGroup, promotedCharacterData, actingCharacterData);

        }

        private void PromoteCharacterToLeader(int characterGroupId, int promotedCharacterId) {
            //Debug.Log($"CharacterGroupServiceServer.PromoteLeader({characterGroupId}, {actingCharacterId}, {promotedCharacterId})");
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning("CharacterGroupServiceServer.PromoteCharacter: character group not found");
                return;
            }

            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            if (characterGroup.MemberList[UnitControllerMode.Player].ContainsKey(promotedCharacterId) == false) {
                Debug.LogWarning("CharacterGroupServiceServer.PromoteCharacter: new leader must be a member of the group");
                return;
            }

            CharacterGroupMemberData promotedCharacterData = characterGroup.MemberList[UnitControllerMode.Player][promotedCharacterId];
            promotedCharacterData.Rank = CharacterGroupRank.Leader;
            ProcessPromoteCharacter(characterGroup, promotedCharacterData, null);
        }

        private void ProcessPromoteCharacter(CharacterGroup characterGroup, CharacterGroupMemberData promotedCharacterData, CharacterGroupMemberData actingCharacterData) {
            string promotedCharacterName = playerCharacterService.GetPlayerNameFromId(promotedCharacterData.CharacterSummaryData.CharacterId);

            CharacterGroupMemberNetworkData promotedCharacterNetworkData = new CharacterGroupMemberNetworkData(promotedCharacterData);
            CharacterGroupMemberNetworkData actingCharacterNetworkData = null;
            if (actingCharacterData != null) {
                actingCharacterNetworkData = new CharacterGroupMemberNetworkData(actingCharacterData);
            }
            foreach (CharacterGroupMemberData characterGroupMemberData in characterGroup.MemberList[UnitControllerMode.Player].Values) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterGroupMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (characterGroupMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseGroupMemberStatusChange(memberAccountId, characterGroup.characterGroupId, promotedCharacterData.CharacterSummaryData.CharacterId, promotedCharacterNetworkData);
                if (actingCharacterData != null) {
                    networkManagerServer.AdvertiseGroupMemberStatusChange(memberAccountId, characterGroup.characterGroupId, actingCharacterData.CharacterSummaryData.CharacterId, actingCharacterNetworkData);
                }

                if (promotedCharacterData.CharacterSummaryData.CharacterId == characterGroupMemberData.CharacterSummaryData.CharacterId) {
                    messageLogServer.WriteSystemMessage(memberAccountId, $"You have been promoted in the group to {promotedCharacterData.Rank.ToString()}.");
                } else {
                    messageLogServer.WriteSystemMessage(memberAccountId, $"{promotedCharacterName} has been promoted in the group to {promotedCharacterData.Rank.ToString()}.");
                }
            }
        }

        public void DemoteCharacter(int characterGroupId, int actingCharacterId, int demotedCharacterId) {
            //Debug.Log($"CharacterGroupServiceServer.DemoteCharacter({characterGroupId}, {actingCharacterId}, {demotedCharacterId})");
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                Debug.LogWarning("CharacterGroupServiceServer.DemoteCharacter: character group not found");
                return;
            }

            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            if (characterGroup.MemberList[UnitControllerMode.Player].ContainsKey(demotedCharacterId) == false) {
                Debug.LogWarning("CharacterGroupServiceServer.DemoteCharacter: new leader must be a member of the group");
                return;
            }

            CharacterGroupMemberData actingCharacterData = characterGroup.MemberList[UnitControllerMode.Player][actingCharacterId];
            CharacterGroupMemberData demotedCharacterData = characterGroup.MemberList[UnitControllerMode.Player][demotedCharacterId];

            if (actingCharacterData.Rank != CharacterGroupRank.Leader) {
                Debug.LogWarning("CharacterGroupServiceServer.DemoteCharacter() only leaders can demote assistants");
                return;
            }

            if (demotedCharacterData.Rank != CharacterGroupRank.Assistant) {
                Debug.LogWarning("CharacterGroupServiceServer.DemoteCharacter() only assistants can be demoted");
                return;
            }
            demotedCharacterData.Rank = CharacterGroupRank.Member;

            string demotedCharacterName = playerCharacterService.GetPlayerNameFromId(demotedCharacterId);

            foreach (CharacterGroupMemberData characterGroupMemberData in characterGroup.MemberList[UnitControllerMode.Player].Values) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterGroupMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (characterGroupMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseGroupMemberStatusChange(memberAccountId, characterGroupId, demotedCharacterId, new CharacterGroupMemberNetworkData(demotedCharacterData));

                if (demotedCharacterId == characterGroupMemberData.CharacterSummaryData.CharacterId) {
                    messageLogServer.WriteSystemMessage(memberAccountId, $"You have been demoted in the group to {demotedCharacterData.Rank.ToString()}.");
                } else {
                    messageLogServer.WriteSystemMessage(memberAccountId, $"{demotedCharacterName} has been demoted in the group to {demotedCharacterData.Rank.ToString()}.");
                }
            }

        }

        /*
        public void ProcessLevelChanged(UnitController unitController) {
            int playerCharacterId = unitController.CharacterId;
            // check if character is in a group
            if (characterGroupMemberLookup.ContainsKey(playerCharacterId) == false) {
                return;
            }
            int characterGroupId = characterGroupMemberLookup[playerCharacterId];
            if (characterGroupDictionary.ContainsKey(characterGroupId) == false) {
                return;
            }
            CharacterGroup characterGroup = characterGroupDictionary[characterGroupId];
            if (characterGroup.CharacterIdList[UnitControllerMode.Player].ContainsKey(playerCharacterId) == false) {
                return;
            }
            CharacterSummaryData targetCharacterSummaryData = characterGroup.CharacterIdList[UnitControllerMode.Player][playerCharacterId];

            foreach (CharacterSummaryData characterSummaryData in characterGroup.CharacterIdList[UnitControllerMode.Player].Values) {
                // get account id from player id, zero means the player is logged out
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (characterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseGroupMemberStatusChange(memberAccountId, characterGroupId, playerCharacterId, new CharacterSummaryNetworkData(targetCharacterSummaryData));
            }
        }
        */


    }

}