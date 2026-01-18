using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SocialPanel : PagedWindowContents {

        [Header("Social Panel")]

        [SerializeField]
        private TMP_Text panelTitle = null;

        [SerializeField]
        private Toggle showOfflineMembersToggle = null;

        [SerializeField]
        private List<CharacterGroupMemberButton> groupMemberButtons = new List<CharacterGroupMemberButton>();

        [SerializeField]
        private List<GuildMemberButton> guildMemberButtons = new List<GuildMemberButton>();

        [SerializeField]
        private List<FriendInfoButton> friendInfoButtons = new List<FriendInfoButton>();

        [SerializeField]
        private UINavigationController groupMemberButtonsNavigationController = null;

        [SerializeField]
        private UINavigationController guildMemberButtonsNavigationController = null;

        [SerializeField]
        private UINavigationController friendInfoButtonsNavigationController = null;

        [SerializeField]
        private GameObject noGroupMembersText = null;

        [SerializeField]
        private GameObject noGuildMembersText = null;

        [SerializeField]
        private GameObject groupPane = null;

        [SerializeField]
        private GameObject guildPane = null;

        [SerializeField]
        private GameObject friendsPane = null;

        [SerializeField]
        private TMP_InputField playerNameInput = null;

        [SerializeField]
        private HighlightButton inviteButton = null;

        [SerializeField]
        private TMP_Text inviteButtonText = null;

        [SerializeField]
        private GameObject leaveButton = null;

        [SerializeField]
        private GameObject disbandButton = null;

        [SerializeField]
        private HighlightButton guildButton = null;

        [SerializeField]
        private HighlightButton friendsButton = null;

        private SocialPanelSortType currentSortType = SocialPanelSortType.Level;
        private bool reverseSort = false;

        private bool windowSubscriptionsInitialized = false;

        private SocialPanelTab currentTab = SocialPanelTab.Group;

        // game manager references
        private PlayerManager playerManager = null;
        private CharacterGroupServiceClient characterGroupServiceClient = null;
        private GuildServiceClient guildServiceClient = null;
        private UIManager uiManager = null;
        private FriendServiceClient friendServiceClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            characterGroupServiceClient = systemGameManager.CharacterGroupServiceClient;
            guildServiceClient = systemGameManager.GuildServiceClient;
            uiManager = systemGameManager.UIManager;
            friendServiceClient = systemGameManager.FriendServiceClient;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (windowSubscriptionsInitialized == true) {
                return;
            }
            characterGroupServiceClient.OnAddMember += HandleAddGroupMember;
            characterGroupServiceClient.OnRemoveMember += HandleRemoveGroupMember;
            characterGroupServiceClient.OnLeaveGroup += HandleLeaveGroup;
            characterGroupServiceClient.OnDisbandGroup += HandleDisbandGroup;
            characterGroupServiceClient.OnCharacterGroupMemberStatusChange += HandleCharacterGroupMemberStatusChange;
            characterGroupServiceClient.OnRenameCharacterInGroup += HandleRenameCharacterInGroup;
            characterGroupServiceClient.OnPromoteGroupLeader += HandlePromoteGroupLeader;
            characterGroupServiceClient.OnJoinGroup += HandleJoinGroup;

            guildServiceClient.OnAddMember += HandleAddGuildMember;
            guildServiceClient.OnRemoveMember += HandleRemoveGuildMember;
            guildServiceClient.OnLeaveGuild += HandleLeaveGuild;
            guildServiceClient.OnDisbandGuild += HandleDisbandGuild;
            guildServiceClient.OnGuildMemberStatusChange += HandleGuildMemberStatusChange;
            guildServiceClient.OnRenameCharacterInGuild += HandleRenameCharacterInGuild;
            guildServiceClient.OnPromoteGuildLeader += HandlePromoteGuildLeader;
            guildServiceClient.OnJoinGuild += HandleJoinGuild;

            friendServiceClient.OnAddFriend += HandleAddFriend;
            friendServiceClient.OnRemoveFriend += HandleRemoveFriend;
            friendServiceClient.OnFriendStateChange += HandleFriendStateChange;
            friendServiceClient.OnRenameFriend += HandleRenameFriend;

            windowSubscriptionsInitialized = true;
            if (playerManager.UnitController == null) {
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) { 
                friendsButton.gameObject.SetActive(false);
                guildButton.gameObject.SetActive(false);
            } else {
                friendsButton.gameObject.SetActive(true);
                guildButton.gameObject.SetActive(true);
            }

            if (currentTab == SocialPanelTab.Guild) {
                SetupGuildTab();
            } else if (currentTab == SocialPanelTab.Friends) {
                SetupFriendsTab();
            } else {
                SetupGroupTab();
            }
            playerNameInput.text = string.Empty;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            if (windowSubscriptionsInitialized == false) {
                return;
            }
            characterGroupServiceClient.OnAddMember -= HandleAddGroupMember;
            characterGroupServiceClient.OnRemoveMember -= HandleRemoveGroupMember;
            characterGroupServiceClient.OnLeaveGroup -= HandleLeaveGroup;
            characterGroupServiceClient.OnDisbandGroup -= HandleDisbandGroup;
            characterGroupServiceClient.OnCharacterGroupMemberStatusChange -= HandleCharacterGroupMemberStatusChange;
            characterGroupServiceClient.OnRenameCharacterInGroup -= HandleRenameCharacterInGroup;
            characterGroupServiceClient.OnPromoteGroupLeader -= HandlePromoteGroupLeader;
            characterGroupServiceClient.OnJoinGroup -= HandleJoinGroup;

            guildServiceClient.OnAddMember -= HandleAddGuildMember;
            guildServiceClient.OnRemoveMember -= HandleRemoveGuildMember;
            guildServiceClient.OnLeaveGuild -= HandleLeaveGuild;
            guildServiceClient.OnDisbandGuild -= HandleDisbandGuild;
            guildServiceClient.OnGuildMemberStatusChange -= HandleGuildMemberStatusChange;
            guildServiceClient.OnRenameCharacterInGuild -= HandleRenameCharacterInGuild;
            guildServiceClient.OnPromoteGuildLeader -= HandlePromoteGuildLeader;
            guildServiceClient.OnJoinGuild -= HandleJoinGuild;

            friendServiceClient.OnAddFriend -= HandleAddFriend;
            friendServiceClient.OnRemoveFriend -= HandleRemoveFriend;
            friendServiceClient.OnFriendStateChange -= HandleFriendStateChange;
            friendServiceClient.OnRenameFriend -= HandleRenameFriend;

            windowSubscriptionsInitialized = false;
        }

        private void SetupGuildTab() {
            //Debug.Log("SocialPanel.SetupGuildTab()");

            groupPane.SetActive(false);
            guildPane.SetActive(true);
            friendsPane.SetActive(false);
            disbandButton.SetActive(false);

            inviteButtonText.text = "Invite to Guild";
            SetupGuildTabButtons();
            currentTab = SocialPanelTab.Guild;
            panelTitle.text = "Guild";
        }

        private void SetupGuildTabButtons() {
            if (guildServiceClient.CurrentGuild != null) {
                leaveButton.SetActive(true);
                if (guildServiceClient.CurrentGuild.MemberList[playerManager.UnitController.CharacterId].Rank != GuildRank.Member) {
                    inviteButton.Button.interactable = true;
                } else {
                    inviteButton.Button.interactable = false;
                }
            } else {
                leaveButton.SetActive(false);
            }
        }

        private void SetupGroupTab() {
            //Debug.Log("SocialPanel.SetupGroupTab()");

            groupPane.SetActive(true);
            guildPane.SetActive(false);
            friendsPane.SetActive(false);

            inviteButtonText.text = "Invite to Group";

            SetupGroupTabButtons();

            currentTab = SocialPanelTab.Group;
            panelTitle.text = "Group";

        }

        private void SetupGroupTabButtons() {
            if (characterGroupServiceClient.CurrentCharacterGroup != null) {
                leaveButton.SetActive(true);
                if (characterGroupServiceClient.CurrentCharacterGroup.leaderPlayerCharacterId == playerManager.UnitController.CharacterId) {
                    disbandButton.SetActive(true);
                    inviteButton.Button.interactable = true;
                } else if (characterGroupServiceClient.CurrentCharacterGroup.MemberList[UnitControllerMode.Player][playerManager.UnitController.CharacterId].Rank == CharacterGroupRank.Assistant) {
                    inviteButton.Button.interactable = true;
                    disbandButton.SetActive(false);
                } else {
                    inviteButton.Button.interactable = false;
                    disbandButton.SetActive(false);
                }
            } else {
                inviteButton.gameObject.SetActive(true);
                leaveButton.SetActive(false);
                disbandButton.SetActive(false);
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                inviteButton.Button.interactable = false;
            }
        }

        private void SetupFriendsTab() {
            //Debug.Log("SocialPanel.SetupFriendsTab()");

            groupPane.SetActive(false);
            guildPane.SetActive(false);
            friendsPane.SetActive(true);
            leaveButton.SetActive(false);
            disbandButton.SetActive(false);
            inviteButton.Button.interactable = true;
            inviteButtonText.text = "Add Friend";

            currentTab = SocialPanelTab.Friends;
            panelTitle.text = "Friends";
        }

        private void HandleJoinGroup() {
            //Debug.Log($"SocialPanel.HandleJoinGroup()");

            if (currentTab != SocialPanelTab.Group) {
                return;
            }
            SetupGroupTabButtons();
            CreatePages();
        }

        private void HandleJoinGuild() {
            //Debug.Log($"SocialPanel.HandleJoinGuild()");

            if (currentTab != SocialPanelTab.Guild) {
                return;
            }
            SetupGuildTabButtons();
            CreatePages();
        }

        private void HandlePromoteGroupLeader() {
            //Debug.Log($"SocialPanel.HandlePromoteGroupLeader()");

            if (currentTab != SocialPanelTab.Group) {
                return;
            }
            SetupGroupTabButtons();
            CreatePages();
        }

        private void HandlePromoteGuildLeader() {
            //Debug.Log($"SocialPanel.HandlePromoteGuildLeader()");

            if (currentTab != SocialPanelTab.Guild) {
                return;
            }
            SetupGuildTabButtons();
            CreatePages();
        }

        private void HandleRenameFriend() {
            if (currentTab != SocialPanelTab.Friends) {
                return;
            }
            CreatePages();
        }

        private void HandleFriendStateChange() {
            if (currentTab != SocialPanelTab.Friends) {
                return;
            }
            CreatePages();
        }

        private void HandleRemoveFriend() {
            if (currentTab != SocialPanelTab.Friends) {
                return;
            }
            CreatePages();
        }

        private void HandleAddFriend() {
            if (currentTab != SocialPanelTab.Friends) {
                return;
            }
            CreatePages();
        }

        private void HandleRenameCharacterInGuild() {
            if (currentTab != SocialPanelTab.Guild) {
                return;
            }
            CreatePages();
        }

        private void HandleRenameCharacterInGroup() {
            //Debug.Log($"SocialPanel.HandleRenameCharacterInGroup()");

            if (currentTab != SocialPanelTab.Group) {
                return;
            }
            CreatePages();
        }

        private void HandleGuildMemberStatusChange() {
            //Debug.Log($"SocialPanel.HandleGuildMemberOnline()");

            if (currentTab != SocialPanelTab.Guild) {
                return;
            }
            SetupGuildTabButtons();
            CreatePages();
        }

        private void HandleDisbandGuild() {
            //Debug.Log($"SocialPanel.HandleDisbandGuild()");

            if (currentTab != SocialPanelTab.Guild) {
                return;
            }
            SetupGuildTabButtons();
            CreatePages();
        }

        private void HandleLeaveGuild() {
            //Debug.Log($"SocialPanel.HandleLeaveGuild()");

            if (currentTab != SocialPanelTab.Guild) {
                return;
            }
            SetupGuildTabButtons();
            CreatePages();
        }

        private void HandleAddGuildMember() {
            //Debug.Log($"SocialPanel.HandleAddGuildMember()");

            if (currentTab != SocialPanelTab.Guild) {
                return;
            }
            CreatePages();
        }

        private void HandleRemoveGuildMember() {
            //Debug.Log($"SocialPanel.HandleRemoveGuildMember()");

            if (currentTab != SocialPanelTab.Guild) {
                return;
            }
            CreatePages();
        }

        private void HandleCharacterGroupMemberStatusChange() {
            //Debug.Log($"SocialPanel.HandleCharacterGroupMemberStatusChange()");

            if (currentTab != SocialPanelTab.Group) {
                return;
            }
            SetupGroupTabButtons();
            CreatePages();
        }

        private void HandleDisbandGroup() {
            //Debug.Log($"SocialPanel.HandleDisbandGroup()");

            if (currentTab != SocialPanelTab.Group) {
                return;
            }
            SetupGroupTabButtons();
            CreatePages();
        }

        private void HandleLeaveGroup() {
            //Debug.Log($"SocialPanel.HandleLeaveGroup()");

            if (currentTab != SocialPanelTab.Group) {
                return;
            }
            SetupGroupTabButtons();
            CreatePages();
        }

        private void HandleAddGroupMember() {
            //Debug.Log($"SocialPanel.HandleAddGroupMember()");

            if (currentTab != SocialPanelTab.Group) {
                return;
            }
            CreatePages();
        }

        private void HandleRemoveGroupMember() {
            //Debug.Log($"SocialPanel.HandleRemoveGroupMember()");

            if (currentTab != SocialPanelTab.Group) {
                return;
            }
            CreatePages();
        }

        protected override void PopulatePages() {
            //Debug.Log("SocialPanel.PopulatePages()");

            if (currentTab == SocialPanelTab.Group) {
                PopulateGroupPages();
            } else if (currentTab == SocialPanelTab.Friends) {
                PopulateFriendPages();
            } else if (currentTab == SocialPanelTab.Guild) {
                PopulateGuildPages();
            }
        }

        private void PopulateGuildPages() {
            if (guildServiceClient.CurrentGuild == null) {
                noGuildMembersText.SetActive(true);
                return;
            } else {
                noGuildMembersText.SetActive(false);
            }
            GuildContentList page = new GuildContentList();

            Dictionary<int, GuildMemberData> sortedDictionary = new Dictionary<int, GuildMemberData>();
            int sortOrder = 0;
            foreach (GuildMemberData guildMemberData in guildServiceClient.CurrentGuild.MemberList.Values) {
                sortedDictionary.Add(sortOrder, guildMemberData);
                sortOrder++;
            }
            SortGuildMemberDataDictionary(sortedDictionary, reverseSort);

            for (int i = 0; i < sortedDictionary.Count; i++) {
                GuildMemberData guildMemberData = sortedDictionary[i];
                if (showOfflineMembersToggle.isOn == false && guildMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                page.guildMembers.Add(guildMemberData);
                if (page.guildMembers.Count == pageSize) {
                    pages.Add(page);
                    page = new GuildContentList();
                }
            }
            if (page.guildMembers.Count > 0) {
                pages.Add(page);
            }

            /*
            // ensure there is at least one page, even if there are no guild members
            if (pages.Count == 0) {
                pages.Add(page);
            }
            */
            AddGuildMembers();
        }

        private void PopulateGroupPages() {
            if (characterGroupServiceClient.CurrentCharacterGroup == null) {
                noGroupMembersText.SetActive(true);
                return;
            } else {
                noGroupMembersText.SetActive(false);
            }
            CharacterGroupContentList page = new CharacterGroupContentList();

            Dictionary<int, CharacterGroupMemberData> sortedDictionary = new Dictionary<int, CharacterGroupMemberData>();
            int sortOrder = 0;
            foreach (CharacterGroupMemberData characterGroupMemberData in characterGroupServiceClient.CurrentCharacterGroup.MemberList[UnitControllerMode.Player].Values) {
                sortedDictionary.Add(sortOrder, characterGroupMemberData);
                sortOrder++;
            }
            SortCharacterGroupMemberDataDictionary(sortedDictionary, reverseSort);

            for (int i = 0; i < sortedDictionary.Count; i++) {
                CharacterGroupMemberData characterGroupMemberData = sortedDictionary[i];
                if (showOfflineMembersToggle.isOn == false && characterGroupMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                page.groupMembers.Add(characterGroupMemberData);
                if (page.groupMembers.Count == pageSize) {
                    pages.Add(page);
                    page = new CharacterGroupContentList();
                }
            }
            if (page.groupMembers.Count > 0) {
                pages.Add(page);
            }

            /*
            // ensure there is at least one page, even if there are no group members
            if (pages.Count == 0) {
                pages.Add(page);
            }
            */
            AddGroupMembers();
        }

        private void PopulateFriendPages() {
            FriendContentList page = new FriendContentList();

            Dictionary<int, CharacterSummaryData> sortedDictionary = new Dictionary<int, CharacterSummaryData>();
            int sortOrder = 0;
            foreach (CharacterSummaryData characterSummaryData in friendServiceClient.FriendList.MemberIdList.Values) {
                sortedDictionary.Add(sortOrder, characterSummaryData);
                sortOrder++;
            }
            SortCharacterSummaryDataDictionary(sortedDictionary, reverseSort);

            for (int i = 0; i < sortedDictionary.Count; i++) {
                CharacterSummaryData characterSummaryData = sortedDictionary[i];
                if (showOfflineMembersToggle.isOn == false && characterSummaryData.IsOnline == false) {
                    continue;
                }
                page.friends.Add(characterSummaryData);
                if (page.friends.Count == pageSize) {
                    pages.Add(page);
                    page = new FriendContentList();
                }
            }
            if (page.friends.Count > 0) {
                pages.Add(page);
            }

            /*
            // ensure there is at least one page, even if there are no friends
            if (pages.Count == 0) {
                pages.Add(page);
            }
            */
            AddFriends();
        }

        private void SortCharacterGroupMemberDataDictionary(Dictionary<int, CharacterGroupMemberData> sortedDictionary, bool reverseSort) {
            List<KeyValuePair<int, CharacterGroupMemberData>> sortedList = new List<KeyValuePair<int, CharacterGroupMemberData>>(sortedDictionary);
            if (reverseSort) {
                sortedList.Sort(
                    delegate (KeyValuePair<int, CharacterGroupMemberData> pair1,
                              KeyValuePair<int, CharacterGroupMemberData> pair2) {
                                  if (currentSortType == SocialPanelSortType.Level) {
                                      return pair2.Value.CharacterSummaryData.Level.CompareTo(pair1.Value.CharacterSummaryData.Level);
                                  } else if (currentSortType == SocialPanelSortType.Class) {
                                      if (pair2.Value.CharacterSummaryData.CharacterClass == null && pair1.Value.CharacterSummaryData.CharacterClass == null) {
                                          return 0;
                                      } else if (pair2.Value.CharacterSummaryData.CharacterClass == null) {
                                          return -1;
                                      } else if (pair1.Value.CharacterSummaryData.CharacterClass == null) {
                                          return 1;
                                      }
                                      return pair2.Value.CharacterSummaryData.CharacterClass.DisplayName.CompareTo(pair1.Value.CharacterSummaryData.CharacterClass.DisplayName);
                                  } else if (currentSortType == SocialPanelSortType.Zone) {
                                      return pair2.Value.CharacterSummaryData.CurrentZoneName.CompareTo(pair1.Value.CharacterSummaryData.CurrentZoneName);
                                  } else if (currentSortType == SocialPanelSortType.Rank) {
                                      return pair2.Value.Rank.CompareTo(pair1.Value.Rank);
                                  } else if (currentSortType == SocialPanelSortType.Status) {
                                      return pair2.Value.CharacterSummaryData.IsOnline.CompareTo(pair1.Value.CharacterSummaryData.IsOnline);
                                  }
                                  return pair2.Value.CharacterSummaryData.CharacterName.CompareTo(pair1.Value.CharacterSummaryData.CharacterName);
                              }
                );
            } else {
                sortedList.Sort(
                    delegate (KeyValuePair<int, CharacterGroupMemberData> pair1,
                              KeyValuePair<int, CharacterGroupMemberData> pair2) {
                                  if (currentSortType == SocialPanelSortType.Level) {
                                      return pair1.Value.CharacterSummaryData.Level.CompareTo(pair2.Value.CharacterSummaryData.Level);
                                  } else if (currentSortType == SocialPanelSortType.Class) {
                                      if (pair1.Value.CharacterSummaryData.CharacterClass != null && pair2.Value.CharacterSummaryData.CharacterClass != null) {
                                          return 0;
                                      } else if (pair1.Value.CharacterSummaryData.CharacterClass == null) {
                                          return -1;
                                      } else if (pair2.Value.CharacterSummaryData.CharacterClass == null) {
                                          return 1;
                                      }
                                      return pair1.Value.CharacterSummaryData.CharacterClass.DisplayName.CompareTo(pair2.Value.CharacterSummaryData.CharacterClass.DisplayName);
                                  } else if (currentSortType == SocialPanelSortType.Zone) {
                                      return pair1.Value.CharacterSummaryData.CurrentZoneName.CompareTo(pair2.Value.CharacterSummaryData.CurrentZoneName);
                                  } else if (currentSortType == SocialPanelSortType.Rank) {
                                      return pair1.Value.Rank.CompareTo(pair2.Value.Rank);
                                  } else if (currentSortType == SocialPanelSortType.Status) {
                                      return pair1.Value.CharacterSummaryData.IsOnline.CompareTo(pair2.Value.CharacterSummaryData.IsOnline);
                                  }
                                  return pair1.Value.CharacterSummaryData.CharacterName.CompareTo(pair2.Value.CharacterSummaryData.CharacterName);
                              }
                );
            }
            // Clear the original dictionary and repopulate it with the sorted values
            sortedDictionary.Clear();
            int sortOrder = 0;
            foreach (KeyValuePair<int, CharacterGroupMemberData> pair in sortedList) {
                sortedDictionary.Add(sortOrder, pair.Value);
                sortOrder++;
            }

        }

        private void SortGuildMemberDataDictionary(Dictionary<int, GuildMemberData> sortedDictionary, bool reverseSort) {
            List<KeyValuePair<int, GuildMemberData>> sortedList = new List<KeyValuePair<int, GuildMemberData>>(sortedDictionary);
            if (reverseSort) {
                sortedList.Sort(
                    delegate (KeyValuePair<int, GuildMemberData> pair1,
                              KeyValuePair<int, GuildMemberData> pair2) {
                                  if (currentSortType == SocialPanelSortType.Level) {
                                      return pair2.Value.CharacterSummaryData.Level.CompareTo(pair1.Value.CharacterSummaryData.Level);
                                  } else if (currentSortType == SocialPanelSortType.Class) {
                                      if (pair2.Value.CharacterSummaryData.CharacterClass == null && pair1.Value.CharacterSummaryData.CharacterClass == null) {
                                          return 0;
                                      } else if (pair2.Value.CharacterSummaryData.CharacterClass == null) {
                                          return -1;
                                      } else if (pair1.Value.CharacterSummaryData.CharacterClass == null) {
                                          return 1;
                                      }
                                      return pair2.Value.CharacterSummaryData.CharacterClass.DisplayName.CompareTo(pair1.Value.CharacterSummaryData.CharacterClass.DisplayName);
                                  } else if (currentSortType == SocialPanelSortType.Zone) {
                                      return pair2.Value.CharacterSummaryData.CurrentZoneName.CompareTo(pair1.Value.CharacterSummaryData.CurrentZoneName);
                                  } else if (currentSortType == SocialPanelSortType.Rank) {
                                      return pair2.Value.Rank.CompareTo(pair1.Value.Rank);
                                  } else if (currentSortType == SocialPanelSortType.Status) {
                                      return pair2.Value.CharacterSummaryData.IsOnline.CompareTo(pair1.Value.CharacterSummaryData.IsOnline);
                                  }
                                  return pair2.Value.CharacterSummaryData.CharacterName.CompareTo(pair1.Value.CharacterSummaryData.CharacterName);
                              }
                );
            } else {
                sortedList.Sort(
                    delegate (KeyValuePair<int, GuildMemberData> pair1,
                              KeyValuePair<int, GuildMemberData> pair2) {
                                  if (currentSortType == SocialPanelSortType.Level) {
                                      return pair1.Value.CharacterSummaryData.Level.CompareTo(pair2.Value.CharacterSummaryData.Level);
                                  } else if (currentSortType == SocialPanelSortType.Class) {
                                      if (pair1.Value.CharacterSummaryData.CharacterClass != null && pair2.Value.CharacterSummaryData.CharacterClass != null) {
                                          return 0;
                                      } else if (pair1.Value.CharacterSummaryData.CharacterClass == null) {
                                          return -1;
                                      } else if (pair2.Value.CharacterSummaryData.CharacterClass == null) {
                                          return 1;
                                      }
                                      return pair1.Value.CharacterSummaryData.CharacterClass.DisplayName.CompareTo(pair2.Value.CharacterSummaryData.CharacterClass.DisplayName);
                                  } else if (currentSortType == SocialPanelSortType.Zone) {
                                      return pair1.Value.CharacterSummaryData.CurrentZoneName.CompareTo(pair2.Value.CharacterSummaryData.CurrentZoneName);
                                  } else if (currentSortType == SocialPanelSortType.Rank) {
                                      return pair1.Value.Rank.CompareTo(pair2.Value.Rank);
                                  } else if (currentSortType == SocialPanelSortType.Status) {
                                      return pair1.Value.CharacterSummaryData.IsOnline.CompareTo(pair2.Value.CharacterSummaryData.IsOnline);
                                  }
                                  return pair1.Value.CharacterSummaryData.CharacterName.CompareTo(pair2.Value.CharacterSummaryData.CharacterName);
                              }
                );
            }
            // Clear the original dictionary and repopulate it with the sorted values
            sortedDictionary.Clear();
            int sortOrder = 0;
            foreach (KeyValuePair<int, GuildMemberData> pair in sortedList) {
                sortedDictionary.Add(sortOrder, pair.Value);
                sortOrder++;
            }

        }

        private void SortCharacterSummaryDataDictionary(Dictionary<int, CharacterSummaryData> sortedDictionary, bool reverseSort) {
            List<KeyValuePair<int, CharacterSummaryData>> sortedList = new List<KeyValuePair<int, CharacterSummaryData>>(sortedDictionary);
            if (reverseSort) {
                sortedList.Sort(
                    delegate (KeyValuePair<int, CharacterSummaryData> pair1,
                              KeyValuePair<int, CharacterSummaryData> pair2) {
                                  if (currentSortType == SocialPanelSortType.Level) {
                                      return pair2.Value.Level.CompareTo(pair1.Value.Level);
                                  } else if (currentSortType == SocialPanelSortType.Class) {
                                      if (pair2.Value.CharacterClass == null && pair1.Value.CharacterClass == null) {
                                          return 0;
                                      } else if (pair2.Value.CharacterClass == null) {
                                          return -1;
                                      } else if (pair1.Value.CharacterClass == null) {
                                          return 1;
                                      }
                                      return pair2.Value.CharacterClass.DisplayName.CompareTo(pair1.Value.CharacterClass.DisplayName);
                                  } else if (currentSortType == SocialPanelSortType.Zone) {
                                      return pair2.Value.CurrentZoneName.CompareTo(pair1.Value.CurrentZoneName);
                                  } else if (currentSortType == SocialPanelSortType.Status) {
                                      return pair2.Value.IsOnline.CompareTo(pair1.Value.IsOnline);
                                  }
                                  return pair2.Value.CharacterName.CompareTo(pair1.Value.CharacterName);
                              }
                );
            } else {
                sortedList.Sort(
                    delegate (KeyValuePair<int, CharacterSummaryData> pair1,
                              KeyValuePair<int, CharacterSummaryData> pair2) {
                                  if (currentSortType == SocialPanelSortType.Level) {
                                        return pair1.Value.Level.CompareTo(pair2.Value.Level);
                                    } else if (currentSortType == SocialPanelSortType.Class) {
                                      if (pair1.Value.CharacterClass != null && pair2.Value.CharacterClass != null) {
                                          return 0;
                                      } else if (pair1.Value.CharacterClass == null) {
                                          return -1;
                                      } else if (pair2.Value.CharacterClass == null) {
                                          return 1;
                                      }
                                      return pair1.Value.CharacterClass.DisplayName.CompareTo(pair2.Value.CharacterClass.DisplayName);
                                  } else if (currentSortType == SocialPanelSortType.Zone) {
                                        return pair1.Value.CurrentZoneName.CompareTo(pair2.Value.CurrentZoneName);
                                    } else if (currentSortType == SocialPanelSortType.Status) {
                                        return pair1.Value.IsOnline.CompareTo(pair2.Value.IsOnline);
                                  }
                                  return pair1.Value.CharacterName.CompareTo(pair2.Value.CharacterName);
                    }
                );
            }
            // Clear the original dictionary and repopulate it with the sorted values
            sortedDictionary.Clear();
            int sortOrder = 0;
            foreach (KeyValuePair<int, CharacterSummaryData> pair in sortedList) {
                sortedDictionary.Add(sortOrder, pair.Value);
                sortOrder++;
            }

        }

        public void AddGroupMembers() {
            //Debug.Log("SocialPanel.AddMessages()");

            bool foundButton = false;
            if (pages.Count > 0) {
                if (pageIndex >= pages.Count) {
                    pageIndex = pages.Count - 1;
                }
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex}");
                    if (i < (pages[pageIndex] as CharacterGroupContentList).groupMembers.Count) {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} adding button");
                        groupMemberButtons[i].gameObject.SetActive(true);
                        groupMemberButtons[i].AddGroupMember((pages[pageIndex] as CharacterGroupContentList).groupMembers[i]);
                        foundButton = true;
                    } else {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} clearing button");
                        groupMemberButtons[i].ClearButton();
                        groupMemberButtons[i].gameObject.SetActive(false);
                    }
                }
            }

            if (foundButton) {
                groupMemberButtonsNavigationController.FocusFirstButton();
                SetNavigationController(groupMemberButtonsNavigationController);
            }
        }

        public void AddGuildMembers() {
            //Debug.Log("SocialPanel.AddGuildMembers()");

            bool foundButton = false;
            if (pages.Count > 0) {
                if (pageIndex >= pages.Count) {
                    pageIndex = pages.Count - 1;
                }
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex}");
                    if (i < (pages[pageIndex] as GuildContentList).guildMembers.Count) {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} adding button");
                        guildMemberButtons[i].gameObject.SetActive(true);
                        guildMemberButtons[i].AddGuildMember((pages[pageIndex] as GuildContentList).guildMembers[i]);
                        foundButton = true;
                    } else {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} clearing button");
                        guildMemberButtons[i].ClearButton();
                        guildMemberButtons[i].gameObject.SetActive(false);
                    }
                }
            }

            if (foundButton) {
                guildMemberButtonsNavigationController.FocusFirstButton();
                SetNavigationController(guildMemberButtonsNavigationController);
            }
        }

        public void AddFriends() {
            //Debug.Log("SocialPanel.AddFriends()");
            bool foundButton = false;
            if (pages.Count > 0) {
                if (pageIndex >= pages.Count) {
                    pageIndex = pages.Count - 1;
                }
                for (int i = 0; i < pageSize; i++) {
                    if (i < (pages[pageIndex] as FriendContentList).friends.Count) {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} adding button");
                        friendInfoButtons[i].gameObject.SetActive(true);
                        friendInfoButtons[i].AddFriend((pages[pageIndex] as FriendContentList).friends[i]);
                        foundButton = true;
                    } else {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} clearing button");
                        friendInfoButtons[i].ClearButton();
                        friendInfoButtons[i].gameObject.SetActive(false);
                    }
                }
            }
            if (foundButton) {
                friendInfoButtonsNavigationController.FocusFirstButton();
                SetNavigationController(friendInfoButtonsNavigationController);
            }
        }

        public override void AddPageContent() {
            //Debug.Log("SocialPanel.AddPageContent()");

            base.AddPageContent();
            if (currentTab == SocialPanelTab.Group) {
                AddGroupMembers();
            } else if (currentTab == SocialPanelTab.Friends) {
                AddFriends();
            } else if (currentTab == SocialPanelTab.Guild) {
                AddGuildMembers();
            }
        }

        public override void ClearButtons() {
            //Debug.Log("SocialPanel.ClearButtons()");

            base.ClearButtons();
            foreach (CharacterGroupMemberButton btn in groupMemberButtons) {
                btn.ClearButton();
                btn.gameObject.SetActive(false);
            }
            foreach (GuildMemberButton btn in guildMemberButtons) {
                btn.ClearButton();
                btn.gameObject.SetActive(false);
            }
            foreach (FriendInfoButton btn in friendInfoButtons) {
                btn.ClearButton();
                btn.gameObject.SetActive(false);
            }
        }

        public void OpenGroupTab() {
            //Debug.Log("SocialPanel.OpenGroupTab()");
            
            SetupGroupTab();
            CreatePages();
        }

        public void OpenGuildTab() {
            //Debug.Log("SocialPanel.OpenGuildTab()");

            SetupGuildTab();
            CreatePages();
        }

        public void OpenFriendsTab() {
            //Debug.Log("SocialPanel.OpenFriendsTab()");

            SetupFriendsTab();
            CreatePages();
        }

        public void Invite() {
            if (systemGameManager.GameMode == GameMode.Local) {
                return;
            }
            if (currentTab == SocialPanelTab.Guild) {
                guildServiceClient.RequestInviteCharacterToGuild(playerNameInput.text);
            } else if (currentTab == SocialPanelTab.Group) {
                characterGroupServiceClient.RequestInviteCharacterToGroup(playerNameInput.text);
            } else {
                friendServiceClient.RequestInviteCharacterToFriendList(playerNameInput.text);
            }
            playerNameInput.text = string.Empty;
        }

        public void Leave() {
            if (currentTab == SocialPanelTab.Friends) {
                return;
            } else if (currentTab == SocialPanelTab.Guild) {
                guildServiceClient.RequestLeaveGuild();
            } else {
                characterGroupServiceClient.RequestLeaveGroup();
            }
        }

        public void Disband() {
            if (currentTab == SocialPanelTab.Friends) {
                return;
            } else if (currentTab == SocialPanelTab.Guild) {
                return;
            } else {
                characterGroupServiceClient.RequestDisbandGroup();
            }
        }

        public void ToggleShowOfflineMembers() {
            //Debug.Log("SocialPanel.ToggleShowOfflineMembers()");
            CreatePages();
        }

        public void SortByClass() {
            reverseSort = !reverseSort;
            currentSortType = SocialPanelSortType.Class;
            CreatePages();
        }

        public void SortByLevel() {
            reverseSort = !reverseSort;
            currentSortType = SocialPanelSortType.Level;
            CreatePages();
        }

        public void SortByName() {
            reverseSort = !reverseSort;
            currentSortType = SocialPanelSortType.Name;
            CreatePages();
        }

        public void SortByZone () {
            reverseSort = !reverseSort;
            currentSortType = SocialPanelSortType.Zone;
            CreatePages();
        }

        public void SortByRank() {
            reverseSort = !reverseSort;
            currentSortType = SocialPanelSortType.Rank;
            CreatePages();
        }

        public void SortByStatus() {
            reverseSort = !reverseSort;
            currentSortType = SocialPanelSortType.Status;
            CreatePages();
        }

    }

    public class CharacterGroupContentList : PagedContentList {
        public List<CharacterGroupMemberData> groupMembers = new List<CharacterGroupMemberData>();
    }

    public class GuildContentList : PagedContentList {
        public List<GuildMemberData> guildMembers = new List<GuildMemberData>();
    }

    public class FriendContentList : PagedContentList {
        public List<CharacterSummaryData> friends = new List<CharacterSummaryData>();
    }

    public enum SocialPanelTab {
        Group,
        Guild,
        Friends
    }

    public enum SocialPanelSortType {
        Name,
        Level,
        Class,
        Zone,
        Rank,
        Status
    }

}