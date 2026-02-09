using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ServerDataService : ConfiguredClass {

        private LocalGameServerClient localGameServerClient = null;
        private RemoteGameServerClient remoteGameServerClient = null;

        // game manager references
        private AuthenticationService authenticationService = null;
        private MailService mailService = null;
        private AuctionService auctionService = null;
        private GuildServiceServer guildServiceServer = null;
        private FriendServiceServer friendServiceServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            authenticationService = systemGameManager.AuthenticationService;
            mailService = systemGameManager.MailService;
            auctionService = systemGameManager.AuctionService;
            guildServiceServer = systemGameManager.GuildServiceServer;
            friendServiceServer = systemGameManager.FriendServiceServer;
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
            //Debug.Log($"NetworkManagerServer.ProcessServerStarted()");
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is loaded in Lobby mode
                return;
            }

            // load items and players first, as they have no dependencies on each other
            systemItemManager.LoadAllItems();
            playerCharacterService.LoadPlayerNameList();
        }

        public void ProcessItemsLoaded() {
            //Debug.Log($"NetworkManagerServer.ProcessItemsLoaded()");

            // auction has dependency on items for item displayName
            auctionService.LoadAuctionItemMap();
        }

        public void ProcessPlayerNameMapLoaded() {
            //Debug.Log($"NetworkManagerServer.ProcessPlayerNameMapLoaded()");

            // guild has dependency on player character service for summary data
            guildServiceServer.LoadAllGuilds();

            // friendlist has dependency on player character service for summary data
            friendServiceServer.LoadAllFriends();
        }


        public void LoadAllItems() {
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.LoadAllItems();
                ProcessItemsLoaded();
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.LoadItemInstanceList();
            }
        }

        public void LoadPlayerNameList() {
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.LoadPlayerNameList();
                ProcessPlayerNameMapLoaded();
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

        public void CreatePlayerCharacter(int accountId, CharacterSaveData characterSaveData) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            // create save data from parameters
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.CreatePlayerCharacter(accountId, characterSaveData);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                int characterId = localGameServerClient.AddPlayerCharacter(accountId, characterSaveData);
                playerCharacterService.ProcessCreatePlayerCharacterResponse(accountId, true, characterId, characterSaveData);
                playerCharacterService.LoadCharacterList(accountId);
            }
        }

        public void SavePlayerCharacter(PlayerCharacterMonitor playerCharacterMonitor) {
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
                localGameServerClient.SavePlayerCharacter(playerCharacterMonitor.accountId, playerCharacterMonitor.characterSaveData);
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
                localGameServerClient.LoadAllUserAccounts();
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
                localGameServerClient.SaveAccount(userAccount);
            }
        }

        public void LoadAuctionItemMap() {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.GetAuctionItemList();
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
                localGameServerClient.GetPlayerCharacters(accountId);
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
                localGameServerClient.SaveAuctionFile(playerCharacterId, auctionItem);
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
                localGameServerClient.SaveMailFile(playerCharacterId, mailMessage);
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
                localGameServerClient.SaveMailFile(playerCharacterId, mailMessage);
                localGameServerClient.GetMailMessageList(accountId, playerCharacterId);
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
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.LoadGuildList();
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
                localGameServerClient.SaveGuild(guild);
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
                localGameServerClient.GetMailMessageList(accountId, playerCharacterId);
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

        public void SaveItemInstance(InstantiatedItem instantiatedItem) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.SaveItemInstance(instantiatedItem);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.SaveItemInstanceDataFile(instantiatedItem);
            }
        }

        public void CreateItemInstance(InstantiatedItem instantiatedItem) {
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                // no persistent data is saved in Lobby mode
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.CreateItemInstance(instantiatedItem);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.SaveItemInstanceDataFile(instantiatedItem);
            }
        }

        public void LoadAllFriends() {
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                remoteGameServerClient.LoadAllFriendLists();
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                localGameServerClient.LoadAllFriends();
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
                localGameServerClient.SaveFriendList(friendList);
            }
        }
    }

}