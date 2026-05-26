using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ServerDataService : ConfiguredClass {

        public Action OnBeforeLoadItems = delegate { };
        public Action <int> OnLoadItem = delegate { };
        public Action<int> OnLoadItems = delegate { };
        public Action OnBeforeLoadPlayerNameMap = delegate { };
        public Action<int> OnLoadPlayerName = delegate { };
        public Action<int> OnLoadPlayerNameMap = delegate { };
        public Action OnBeforeLoadGuilds = delegate { };
        public Action<int> OnLoadGuild = delegate { };
        public Action<int> OnLoadGuilds = delegate { };
        public Action OnBeforeLoadFriends = delegate { };
        public Action<int> OnLoadFriend = delegate { };
        public Action<int> OnLoadFriends = delegate { };
        public Action OnBeforeLoadAuctionItems = delegate { };
        public Action<int> OnLoadAuctionItem = delegate { };
        public Action<int> OnLoadAuctionItems = delegate { };

        private LocalGameServerClient localGameServerClient = null;
        private RemoteGameServerClient remoteGameServerClient = null;

        private bool itemsLoaded = false;
        private bool playerNameMapLoaded = false;
        private bool guildsLoaded = false;
        private bool friendsLoaded = false;
        private bool auctionItemsLoaded = false;

        // game manager references
        private MailService mailService = null;
        private AuctionService auctionService = null;
        private GuildServiceServer guildServiceServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerServer.OnBeforeStartServer += HandleBeforeStartServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            mailService = systemGameManager.MailService;
            auctionService = systemGameManager.AuctionService;
            guildServiceServer = systemGameManager.GuildServiceServer;
        }

        private void HandleBeforeStartServer() {
            itemsLoaded = false;
            playerNameMapLoaded = false;
            guildsLoaded = false;
            friendsLoaded = false;
            auctionItemsLoaded = false;
        }

        public bool IsServerDataLoaded() {
            return itemsLoaded && playerNameMapLoaded && guildsLoaded && friendsLoaded && auctionItemsLoaded;
        }

        public void LoadServerData() {
            // create instance of GameServerClient
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient = new RemoteGameServerClient(systemGameManager, systemConfigurationManager.ApiServerAddress);
                // get jwt for api server communication
                remoteGameServerClient.ServerLogin(systemConfigurationManager.ApiServerSharedSecret);
            } else {
                localGameServerClient = new LocalGameServerClient(systemGameManager);
                // load local counter ids before anything else
                localGameServerClient.ProcessStartServer();
                // load account list in local mode only. In server mode, this is done by database queries
                userAccountService.ProcessStartServer();
                ProcessServerStarted();
            }
        }

        public void ProcessServerStarted() {
            //Debug.Log($"ServerDataService.ProcessServerStarted()");
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is loaded in Lobby mode
                return;
            }

            // load items and players first, as they have no dependencies on each other
            LoadAllItems();
            LoadPlayerNameList();
        }

        public void NotifyOnLoadItem(int count) {
            //Debug.Log($"ServerDataService.NotifyOnLoadItem(count: {count})");

            OnLoadItem(count);
        }

        public void ProcessItemsLoaded() {
            //Debug.Log($"ServerDataService.ProcessItemsLoaded()");

            itemsLoaded = true;
            OnLoadItems(systemItemManager.InstantiatedItems.Count);

            // auction has dependency on items for item displayName
            LoadAuctionItemMap();
        }

        public void ProcessPlayerNameMapLoaded() {
            //Debug.Log($"ServerDataService.ProcessPlayerNameMapLoaded()");

            playerNameMapLoaded = true;
            OnLoadPlayerNameMap(playerCharacterService.GetPlayerNameMapCount());

            // guild has dependency on player character service for summary data
            //guildServiceServer.LoadAllGuilds();
            LoadAllGuilds();

            // friendlist has dependency on player character service for summary data
            //friendServiceServer.LoadAllFriends();
            LoadAllFriends();
        }


        public void LoadAllItems() {
            OnBeforeLoadItems();
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.LoadAllItemsAsync();
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.LoadItemInstanceList();
            }
        }

        public void LoadPlayerNameList() {
            OnBeforeLoadPlayerNameMap();
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.LoadPlayerNameListAsync();
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.LoadAllPlayerCharacters();
            }
        }

        public void GetNewMailMessageId(MailMessage mailMessage, MailMessageRequest mailMessageRequest, int recipientPlayerCharacterId) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            // get mail message Id
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.GetNewMailMessageId(mailMessage);
                mailService.ProcessMailMessageIdAssigned(mailMessage, mailMessageRequest, recipientPlayerCharacterId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.CreateMailMessage(mailMessage, mailMessageRequest, recipientPlayerCharacterId);
            }
        }

        public CharacterSaveData GetPlayerCharacterSaveData(int accountId, int playerCharacterId) {
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                return localGameServerClient.GetPlayerCharacterSaveData(accountId, playerCharacterId);
            }
            return null;
        }

        public async void CreatePlayerCharacterAsync(int accountId, CharacterSaveData characterSaveData) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }

            SaveNewItemList(characterSaveData);

            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.CreatePlayerCharacter(accountId, characterSaveData);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                // 1. Wait for the file to actually be written to disk
                int characterId = await localGameServerClient.AddPlayerCharacterAsync(accountId, characterSaveData);

                // 2. Now these are safe to call because the file exists
                playerCharacterService.ProcessCreatePlayerCharacterResponse(accountId, true, characterId, characterSaveData);
                playerCharacterService.LoadCharacterList(accountId);
            }
        }

        private void SaveNewItemList(CharacterSaveData characterSaveData) {
            List<InstantiatedItem> newInstantiatedItems = new List<InstantiatedItem>();
            foreach (InventorySlotSaveData inventorySlotSaveData in characterSaveData.InventorySlotSaveData) {
                foreach (long itemInstanceId in inventorySlotSaveData.ItemInstanceIds) {
                    InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                    if (instantiatedItem == null) {
                        Debug.LogWarning($"PlayerCharacterSaveData.PlayerCharacterSaveData(): Could not find instantiated item with id {itemInstanceId} in inventory for character {characterSaveData.CharacterName}");
                        continue;
                    }
                    newInstantiatedItems.Add(instantiatedItem);
                }
            }
            foreach (InventorySlotSaveData inventorySlotSaveData in characterSaveData.BankSlotSaveData) {
                foreach (long itemInstanceId in inventorySlotSaveData.ItemInstanceIds) {
                    InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                    if (instantiatedItem == null) {
                        Debug.LogWarning($"PlayerCharacterSaveData.PlayerCharacterSaveData(): Could not find instantiated item with id {itemInstanceId} in bank for character {characterSaveData.CharacterName}");
                        continue;
                    }
                    newInstantiatedItems.Add(instantiatedItem);
                }
            }
            foreach (EquipmentInventorySlotSaveData equipmentInventorySlotSaveData in characterSaveData.EquipmentSaveData) {
                //Debug.Log($"PlayerCharacterSaveData.Constructor() equipmentId: {equipmentSaveData.ItemInstanceId}");
                if (equipmentInventorySlotSaveData.HasItem == false) {
                    continue;
                }
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(equipmentInventorySlotSaveData.ItemInstanceId);
                if (instantiatedItem == null) {
                    Debug.LogWarning($"PlayerCharacterSaveData.PlayerCharacterSaveData(): Could not find instantiated item with id {equipmentInventorySlotSaveData.ItemInstanceId} in equipment for character {characterSaveData.CharacterName}");
                    continue;
                }
                newInstantiatedItems.Add(instantiatedItem);
            }
            foreach (EquippedBagSaveData equippedBagSaveData in characterSaveData.EquippedBagSaveData) {
                if (equippedBagSaveData.HasItem == false) {
                    continue;
                }
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(equippedBagSaveData.ItemInstanceId);
                if (instantiatedItem == null) {
                    Debug.LogWarning($"PlayerCharacterSaveData.PlayerCharacterSaveData(): Could not find instantiated item with id {equippedBagSaveData.ItemInstanceId} in equipped bags for character {characterSaveData.CharacterName}");
                    continue;
                }
                newInstantiatedItems.Add(instantiatedItem);
            }
            foreach (EquippedBagSaveData equippedBagSaveData in characterSaveData.EquippedBankBagSaveData) {
                if (equippedBagSaveData.HasItem == false) {
                    continue;
                }
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(equippedBagSaveData.ItemInstanceId);
                if (instantiatedItem == null) {
                    Debug.LogWarning($"PlayerCharacterSaveData.PlayerCharacterSaveData(): Could not find instantiated item with id {equippedBagSaveData.ItemInstanceId} in equipped bank bags for character {characterSaveData.CharacterName}");
                    continue;
                }
                newInstantiatedItems.Add(instantiatedItem);
            }
            foreach (InstantiatedItem instantiatedItem in newInstantiatedItems) {
                CreateItemInstance(instantiatedItem);
            }
        }

        public async void SavePlayerCharacter(PlayerCharacterMonitor playerCharacterMonitor) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.SavePlayerCharacter(
                    playerCharacterMonitor.accountId,
                    playerCharacterMonitor.characterSaveData.CharacterId,
                    playerCharacterMonitor.characterSaveData);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                await localGameServerClient.SavePlayerCharacterDataFileAsync(playerCharacterMonitor.accountId, playerCharacterMonitor.characterSaveData);
            }
        }

        public void DeletePlayerCharacter(int accountId, int playerCharacterId) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.DeletePlayerCharacter(accountId, playerCharacterId);
                playerCharacterService.ProcessDeletePlayerCharacterResponse(accountId, playerCharacterId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.DeletePlayerCharacter(accountId, playerCharacterId);
            }
        }

        public void LoadAllUserAccounts() {
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.LoadAllUserAccountsAsync();
            }
            
            // user accounts are not cached when using the APIServer
        }

        public int GetNewAccountId() {
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                return localGameServerClient.GetNewAccountId();
            }
            return -1;
        }

        public void SaveAccount(UserAccount userAccount) {
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.SaveAccountAsync(userAccount);
            }
        }

        public void LoadAuctionItemMap() {
            OnBeforeLoadAuctionItems();
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                OnLoadAuctionItems(0);
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.GetAuctionItemListAsync();
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.LoadAuctionItemList();
            }
        }

        public void GetNewAuctionItemId(AuctionItem auctionItem) {
            //Debug.Log($"ServerDataService.GetNewAuctionItemId()");
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }

            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.GetNewAuctionItemId(auctionItem);
                auctionService.ProcessAuctionItemIdAssigned(auctionItem);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.CreateAuctionItem(auctionItem);
            }
        }

        public void LoadCharacterList(int accountId) {
            //Debug.Log($"ServerDataService.LoadCharacterList(accountId: {accountId})");
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }

            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.GetPlayerCharactersAsync(accountId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.LoadCharacterList(accountId);
            }
        }

        public void SaveAuction(int playerCharacterId, AuctionItem auctionItem) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.SaveAuctionFileAsync(playerCharacterId, auctionItem);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.SaveAuctionItem(auctionItem);
            }
        }

        public void Login(int clientId, string username, string password) {
            remoteGameServerClient.Login(clientId, username, password);
        }

        public void DeleteAuctionItem(AuctionItem auctionItem) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.DeleteAuctionItem(auctionItem);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.DeleteAuctionItem(auctionItem.AuctionItemId);
            }
        }

        public void SaveMailMessage(int playerCharacterId, MailMessage mailMessage) {
            //Debug.Log($"ServerDataService.SaveMailMessage(playerCharacterId: {playerCharacterId}, mailMessageId: {mailMessage.MessageId})");
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }

            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.SaveMailFileAsync(playerCharacterId, mailMessage);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.SaveMailMessage(mailMessage);
            }
        }

        public void SaveMailAndRefreshMessages(int accountId, int playerCharacterId, MailMessage mailMessage) {
            //Debug.Log($"ServerDataService.SaveMailMessage(playerCharacterId: {playerCharacterId}, mailMessageId: {mailMessage.MessageId})");
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }

            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.SaveMailFileAsync(playerCharacterId, mailMessage);
                localGameServerClient.GetMailMessageListAsync(accountId, playerCharacterId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.SaveMailAndRefreshMessages(accountId, playerCharacterId, mailMessage);
            }
        }

        public void DeleteMailMessage(int accountId, int playerCharacterId, int messageId) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.DeleteMessage(playerCharacterId, messageId);
                mailService.ProcessDeleteMessage(accountId, messageId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.DeleteMailMessage(accountId, messageId);
            }
        }

        public void LoadAllGuilds() {
            OnBeforeLoadGuilds();
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                OnLoadGuilds(0);
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.LoadGuildListAsync();
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.LoadGuildList();
            }
        }

        public void GetGuildId(Guild guild) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.GetGuildId(guild);
                guildServiceServer.ProcessGuildIdAssigned(guild);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.CreateGuild(guild);
            }
        }

        public void SaveGuild(Guild guild) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.SaveGuildAsync(guild);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.SaveGuild(guild);
            }
        }

        public void DeleteGuild(int guildId) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.DeleteGuild(guildId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.DeleteGuild(guildId);
            }
        }

        public void GetMailMessages(int accountId, int playerCharacterId) {
            //Debug.Log($"ServerDataService.GetMailMessages(accountId: {accountId}, playerCharacterId: {playerCharacterId}");
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }

            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.GetMailMessageListAsync(accountId, playerCharacterId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.LoadMailMessageList(accountId, playerCharacterId);
            }
        }

        public void TakeAttachment(int playerCharacterId, int messageId, int attachmentSlotId) {
            //Debug.Log($"ServerDataService.SaveMailMessage(playerCharacterId: {playerCharacterId}, messageId: {messageId}, attachmentSlotId: {attachmentSlotId})");
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }

            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                MailMessage mailMessage = localGameServerClient.GetMailMessage(messageId, playerCharacterId);
                mailService.ProcessTakeAttachment(mailMessage, playerCharacterId, attachmentSlotId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.RequestTakeAttachment(messageId, playerCharacterId, attachmentSlotId);
            }
        }

        public void TakeAttachments(int accountId, int playerCharacterId, int messageId) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                MailMessage mailMessage = localGameServerClient.GetMailMessage(messageId, playerCharacterId);
                mailService.ProcessTakeAttachments(mailMessage, playerCharacterId, accountId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.RequestTakeAttachments(messageId, playerCharacterId, accountId);
            }
        }

        public void MarkMailMessageAsRead(int messageId, int playerCharacterId) {
            //Debug.Log($"ServerDataService.MarkMailMessageAsRead(messageId: {messageId}, playerCharacterId: {playerCharacterId})");
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }

            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                MailMessage mailMessage = localGameServerClient.GetMailMessage(messageId, playerCharacterId);
                mailService.ProcessMarkMessageAsRead(mailMessage, playerCharacterId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.RequestMarkMailMessageAsRead(messageId, playerCharacterId);
            }
        }

        /*
        public void SaveItemInstance(InstantiatedItem instantiatedItem) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.SaveItemInstance(instantiatedItem);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.SaveItemInstanceDataFileAsync(instantiatedItem);
            }
        }
        */

        public void CreateItemInstance(InstantiatedItem instantiatedItem) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.CreateItemInstance(instantiatedItem);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.SaveItemInstanceDataFileAsync(instantiatedItem);
            }
        }

        public void LoadAllFriends() {
            OnBeforeLoadFriends();
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.LoadAllFriendLists();
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.LoadAllFriendsAsync();
            }
        }

        public void SaveFriendList(FriendList friendList) {
            //Debug.Log($"ServerDataService.SaveFriendList(playerCharacterId: {friendList.playerCharacterId})");
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.SaveFriendList(friendList);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.SaveFriendListAsync(friendList);
            }
        }

        public void NotifyOnLoadGuilds(int count) {
            guildsLoaded = true;
            OnLoadGuilds(count);
        }

        public void NotifyOnLoadFriends(int count) {
            friendsLoaded = true;
            OnLoadFriends(count);
        }

        public void NotifyOnLoadAuctionItems(int count) {
            auctionItemsLoaded = true;
            OnLoadAuctionItems(count);
        }

        public void NotifyOnLoadGuild(int count) {
            OnLoadGuild(count);
        }

        public void NotifyOnLoadAuctionItem(int count) {
            OnLoadAuctionItem(count);
        }

        public void NotifyOnLoadPlayerName(int count) {
            OnLoadPlayerName(count);
        }

        public void NotifyOnLoadFriend(int count) {
            OnLoadFriend(count);
        }

        public int GetActiveSaveTasks() {
            if (networkManagerServer.ServerModeActive == false) {
                return 0;
            }

            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                return 0;
            } else {
                return localGameServerClient.ActiveSaveTasks;
            }
        }

        public void DeleteItemInstance(InstantiatedItem itemToRemove) {
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode or Local mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.DeleteItemInstance(itemToRemove.InstanceId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.DeleteItemInstance(itemToRemove);
            }
        }

        public void ResetSettings() {
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.ResetSettings();
                remoteGameServerClient = null;
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.ResetSettings();
                localGameServerClient = null;
            }
        }
    }

}