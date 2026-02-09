using System.Collections;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AnyRPG {

    public class RemoteGameServerClient : ConfiguredClass {

        private const double clientTimeout = 30;
        private const string loginPath = "api/login";
        private const string serverLoginPath = "api/serverlogin";

        private const string createPlayerCharacterPath = "api/createplayercharacter";
        private const string savePlayerCharacterPath = "api/saveplayercharacter";
        private const string deletePlayerCharacterPath = "api/deleteplayercharacter";
        private const string getPlayerCharactersPath = "api/getplayercharacters";
        private const string getAllPlayerCharactersPath = "api/getallplayercharacters";

        private const string createGuildPath = "api/createguild";
        private const string saveGuildPath = "api/saveguild";
        private const string deleteGuildPath = "api/deleteguild";
        private const string getGuildListPath = "api/getguilds";
        
        private const string createAuctionItemPath = "api/createauctionitem";
        private const string saveAuctionItemPath = "api/saveauctionitem";
        private const string deleteAuctionItemPath = "api/deleteauctionitem";
        private const string getAuctionItemListPath = "api/getauctionitems";
        
        private const string createMailMessagePath = "api/createmailmessage";
        private const string saveMailMessagePath = "api/savemailmessage";
        private const string deleteMailMessagePath = "api/deletemailmessage";
        private const string getMailMessageListPath = "api/getmailmessages";
        private const string getMailMessagePath = "api/getmailmessage";

        private const string createItemInstancePath = "api/createiteminstance";
        private const string saveItemInstancePath = "api/saveiteminstance";
        private const string deleteItemInstancePath = "api/deleteiteminstance";
        private const string getItemInstanceListPath = "api/getiteminstances";

        private const string saveFriendListPath = "api/savefriendlist";
        private const string getFriendListsPath = "api/getfriendlists";

        // the token the server uses for server based requests to the api server
        private string token = string.Empty;

        private string serverAddress = string.Empty;

        // game manager references
        private AuthenticationService authenticationService = null;
        private GuildServiceServer guildServiceServer = null;
        private AuctionService auctionService = null;
        private MailService mailService = null;
        private ServerDataService serverDataService = null;
        private FriendServiceServer friendServiceServer = null;

        public RemoteGameServerClient(SystemGameManager systemGameManager, string serverAddress) {
            this.serverAddress = serverAddress;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            authenticationService = systemGameManager.AuthenticationService;
            guildServiceServer = systemGameManager.GuildServiceServer;
            auctionService = systemGameManager.AuctionService;
            mailService = systemGameManager.MailService;
            serverDataService = systemGameManager.ServerDataService;
            friendServiceServer = systemGameManager.FriendServiceServer;
        }

        public void Login(int clientId, string username, string password) {
            //Debug.Log($"RemoteGameServerClient.Login({clientId}, {username}, {password})");
            LoginRequest loginRequest = new LoginRequest(username, password);

            networkManagerServer.StartCoroutine(GetLoginTokenEnumerator(clientId, loginRequest));
        }

        public IEnumerator GetLoginTokenEnumerator(int clientId, LoginRequest loginRequest) {
            //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator(clientId: {clientId}, {loginRequest.UserName} {loginRequest.Password})");

            string requestURL = $"{serverAddress}/{loginPath}";
            var payload = JsonUtility.ToJson(loginRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    authenticationService.ProcessLoginResponse(clientId, -1, false, string.Empty);
                } else {
                    LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(webRequest.downloadHandler.text);
                    authenticationService.ProcessLoginResponse(clientId, loginResponse.accountId, true, loginResponse.token);
                }
            }
        }

        public void ServerLogin(string sharedSecret) {
            //Debug.Log($"RemoteGameServerClient.ServerLogin({sharedSecret})");

            ServerLoginRequest serverLoginRequest = new ServerLoginRequest(sharedSecret);
            networkManagerServer.StartCoroutine(GetServerLoginTokenEnumerator(serverLoginRequest));
        }

        public IEnumerator GetServerLoginTokenEnumerator(ServerLoginRequest serverLoginRequest) {
            //Debug.Log($"RemoteGameServerClient.GetServerLoginTokenEnumerator()");

            string requestURL = $"{serverAddress}/{serverLoginPath}";
            var payload = JsonUtility.ToJson(serverLoginRequest);
            //Debug.Log($"payload: {payload}");
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogError($"RemoteGameServerClient.GetSErverLoginTokenEnumerator() failed login to GameServer. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    //Debug.Log($"RemoteGameServerClient.GetSErverLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                    LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(webRequest.downloadHandler.text);
                    token = loginResponse.token;
                    //Debug.Log($"RemoteGameServerClient.GetServerLoginTokenEnumerator() token: {token}");
                    serverDataService.ProcessServerStarted();
                }
            }
        }

        // ****************************************
        // PLAYER CHARACTER
        // ****************************************

        public void CreatePlayerCharacter(int accountId, CharacterSaveData characterSaveData) {
            //Debug.Log($"RemoteGameServerClient.CreatePlayerCharacter(accountId: {accountId})");

            CreatePlayerCharacterRequest createPlayerCharacterRequest = new CreatePlayerCharacterRequest(accountId, characterSaveData);
            networkManagerServer.StartCoroutine(CreatePlayerCharacterEnumerator(accountId, characterSaveData, createPlayerCharacterRequest));
        }

        public IEnumerator CreatePlayerCharacterEnumerator(int accountId, CharacterSaveData characterSaveData, CreatePlayerCharacterRequest createPlayerCharacterRequest) {
            //Debug.Log($"RemoteGameServerClient.CreatePlayerCharacterEnumerator(accountId: {accountId})");

            string requestURL = $"{serverAddress}/{createPlayerCharacterPath}";
            var payload = JsonUtility.ToJson(createPlayerCharacterRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                
                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.CreatePlayerCharacterEnumerator({accountId}, {characterSaveData.CharacterId}) status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                    playerCharacterService.ProcessCreatePlayerCharacterResponse(accountId, false, 0, characterSaveData);
                } else {
                    CreatePlayerCharacterResponse createPlayerCharacterResponse = JsonUtility.FromJson<CreatePlayerCharacterResponse>(webRequest.downloadHandler.text);
                    // give a chance for id to be assigned to saveData
                    playerCharacterService.ProcessCreatePlayerCharacterResponse(accountId, true, createPlayerCharacterResponse.id, characterSaveData);
                    // save with id, since id was only inserted in table but not in serialized data on the first save.
                    SavePlayerCharacter(accountId, createPlayerCharacterResponse.id, characterSaveData, true);
                }
            }
        }

        public void SavePlayerCharacter(int accountId, int playerCharacterId, CharacterSaveData saveData, bool loadListOnComplete = false) {
            //Debug.Log($"RemoteGameServerClient.SavePlayerCharacter(accountId: {accountId}, playerCharacterId: {playerCharacterId})");

            SavePlayerCharacterRequest savePlayerCharacterRequest = new SavePlayerCharacterRequest(playerCharacterId, saveData.CharacterName, saveData);

            networkManagerServer.StartCoroutine(SavePlayerCharacterEnumerator(accountId, savePlayerCharacterRequest, loadListOnComplete));
        }

        public IEnumerator SavePlayerCharacterEnumerator(int accountId, SavePlayerCharacterRequest savePlayerCharacterRequest, bool loadListOnComplete) {
            //Debug.Log($"RemoteGameServerClient.SavePlayerCharacterEnumerator(accountId: {accountId}, characterId: {savePlayerCharacterRequest.Id})");

            string requestURL = $"{serverAddress}/{savePlayerCharacterPath}";
            var payload = JsonUtility.ToJson(savePlayerCharacterRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.SavePlayerCharacterEnumerator({accountId}, {savePlayerCharacterRequest.Id}) status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    if (loadListOnComplete) {
                        playerCharacterService.LoadCharacterList(accountId);
                    }
                }

            }
        }

        public void DeletePlayerCharacter(int accountId, int playerCharacterId) {
            //Debug.Log($"RemoteGameServerClient.DeletePlayerCharacter({accountId}, {token}, {playerCharacterId})");

            DeletePlayerCharacterRequest deletePlayerCharacterRequest = new DeletePlayerCharacterRequest(playerCharacterId);

            networkManagerServer.StartCoroutine(DeletePlayerCharacterEnumerator(accountId, deletePlayerCharacterRequest));
        }

        public IEnumerator DeletePlayerCharacterEnumerator(int accountId, DeletePlayerCharacterRequest deletePlayerCharacterRequest) {
            //Debug.Log($"RemoteGameServerClient.DeletePlayerCharacterEnumerator({token}, {deletePlayerCharacterRequest.Id})");

            string requestURL = $"{serverAddress}/{deletePlayerCharacterPath}";
            var payload = JsonUtility.ToJson(deletePlayerCharacterRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.DeletePlayerCharacterEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                
                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.DeletePlayerCharacterEnumerator({deletePlayerCharacterRequest.Id}) status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    //LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(webRequest.downloadHandler.text);
                    playerCharacterService.ProcessDeletePlayerCharacterResponse(accountId, deletePlayerCharacterRequest.Id);
                }
            }
        }

        public void LoadCharacterList(int accountId) {
            //Debug.Log($"RemoteGameServerClient.LoadCharacterList(accountId: {accountId})");

            networkManagerServer.StartCoroutine(LoadCharacterListEnumerator(accountId));
        }

        public IEnumerator LoadCharacterListEnumerator(int accountId) {
            //Debug.Log($"RemoteGameServerClient.LoadCharacterListEnumerator(accountId: {accountId})");

            string requestURL = $"{serverAddress}/{getPlayerCharactersPath}";
            var payload = JsonUtility.ToJson(new LoadPlayerCharacterListRequest(accountId));
            //Debug.Log($"payload: {payload}");
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                
                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.LoadCharacterListEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    PlayerCharacterListResponse playerCharacterListResponse = JsonUtility.FromJson<PlayerCharacterListResponse>(webRequest.downloadHandler.text);
                    playerCharacterService.ProcessLoadCharacterListResponse(accountId, playerCharacterListResponse.playerCharacters);
                }
            }
        }

        public void LoadAllPlayerCharacters() {
            //Debug.Log($"RemoteGameServerClient.LoadCharacterList()");

            networkManagerServer.StartCoroutine(LoadAllPlayerCharactersEnumerator());
        }

        public IEnumerator LoadAllPlayerCharactersEnumerator() {
            //Debug.Log($"RemoteGameServerClient.LoadCharacterListEnumerator()");

            string requestURL = $"{serverAddress}/{getAllPlayerCharactersPath}";
            string payload = string.Empty;
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                //Debug.Log($"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.LoadAllPlayerCharactersEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.LoadAllPlayerCharactersEnumerator() failed to load character list. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    PlayerCharacterListResponse playerCharacterListResponse = JsonUtility.FromJson<PlayerCharacterListResponse>(webRequest.downloadHandler.text);
                    playerCharacterService.ProcessLoadPlayerNameList(playerCharacterListResponse.playerCharacters);
                    serverDataService.ProcessPlayerNameMapLoaded();
                }
            }
        }

        // ****************************************
        // GUILD
        // ****************************************

        public void CreateGuild(Guild guild) {
            //Debug.Log($"RemoteGameServerClient.CreateGuild()");

            networkManagerServer.StartCoroutine(CreateGuildEnumerator(guild));
        }

        public IEnumerator CreateGuildEnumerator(Guild guild) {
            //Debug.Log($"RemoteGameServerClient.CreateGuildEnumerator()");
            GuildSaveData guildSaveData = new GuildSaveData(guild);
            CreateGuildRequest createGuildRequest = new CreateGuildRequest(guildSaveData);

            string requestURL = $"{serverAddress}/{createGuildPath}";
            var payload = JsonUtility.ToJson(createGuildRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.CreateGuildEnumerator() There was an error assigning a guild Id.  status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    CreateGuildResponse createGuildResponse = JsonUtility.FromJson<CreateGuildResponse>(webRequest.downloadHandler.text);
                    guild.GuildId = createGuildResponse.id;
                    guildServiceServer.ProcessGuildIdAssigned(guild);
                }
            }
        }

        public void SaveGuild(Guild guild) {
            //Debug.Log($"RemoteGameServerClient.SaveGuild({accountId}, {token}, {guildId})");

            GuildSaveData guildSaveData = new GuildSaveData(guild);
            SaveGuildRequest saveGuildRequest = new SaveGuildRequest(guild.GuildId, guildSaveData);

            networkManagerServer.StartCoroutine(SaveGuildEnumerator(saveGuildRequest));
        }

        public IEnumerator SaveGuildEnumerator(SaveGuildRequest saveGuildRequest) {
            //Debug.Log($"RemoteGameServerClient.SaveGuildEnumerator(guildId: {saveGuildRequest.Id})");

            string requestURL = $"{serverAddress}/{saveGuildPath}";
            var payload = JsonUtility.ToJson(saveGuildRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.SaveGuildEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                
                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.SaveGuildEnumerator(guildId: {saveGuildRequest.Id}) error saving guild. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                }

            }
        }

        public void DeleteGuild(int guildId) {
            //Debug.Log($"RemoteGameServerClient.DeleteGuild({guildId})");

            DeleteGuildRequest deleteGuildRequest = new DeleteGuildRequest(guildId);

            networkManagerServer.StartCoroutine(DeleteGuildEnumerator(deleteGuildRequest));
        }

        public IEnumerator DeleteGuildEnumerator(DeleteGuildRequest deleteGuildRequest) {
            //Debug.Log($"RemoteGameServerClient.DeleteGuildEnumerator({token}, {deleteGuildRequest.Id})");

            string requestURL = $"{serverAddress}/{deleteGuildPath}";
            var payload = JsonUtility.ToJson(deleteGuildRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.DeleteGuildEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.DeleteGuildEnumerator() delete guild failed. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                }
            }
        }

        public void LoadGuildList() {
            //Debug.Log($"RemoteGameServerClient.LoadGuildList()");

            networkManagerServer.StartCoroutine(LoadGuildListEnumerator());
        }

        public IEnumerator LoadGuildListEnumerator() {
            //Debug.Log($"RemoteGameServerClient.LoadGuildListEnumerator()");

            string requestURL = $"{serverAddress}/{getGuildListPath}";
            string payload = string.Empty;
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.LoadGuildListEnumerator() unable to load guild list. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    GuildListResponse guildListResponse = JsonUtility.FromJson<GuildListResponse>(webRequest.downloadHandler.text);
                    guildServiceServer.ProcessLoadGuildListResponse(guildListResponse.guilds);
                }
            }
        }

        // ****************************************
        // AUCTION ITEM
        // ****************************************

        public void CreateAuctionItem(AuctionItem auctionItem) {
            //Debug.Log($"RemoteGameServerClient.CreateAuctionItem()");

            networkManagerServer.StartCoroutine(CreateAuctionItemEnumerator(auctionItem));
        }

        public IEnumerator CreateAuctionItemEnumerator(AuctionItem auctionItem) {
            //Debug.Log($"RemoteGameServerClient.CreateAuctionItemEnumerator()");

            CreateAuctionItemRequest createAuctionItemRequest = new CreateAuctionItemRequest(auctionItem);

            string requestURL = $"{serverAddress}/{createAuctionItemPath}";
            var payload = JsonUtility.ToJson(createAuctionItemRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.CreateAuctionItemEnumerator() There was an error assigning a auctionItem Id. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    CreateAuctionItemResponse createAuctionItemResponse = JsonUtility.FromJson<CreateAuctionItemResponse>(webRequest.downloadHandler.text);
                    auctionItem.AuctionItemId = createAuctionItemResponse.id;
                    auctionService.ProcessAuctionItemIdAssigned(auctionItem);
                }
            }
        }

        public void SaveAuctionItem(AuctionItem auctionItem) {
            //Debug.Log($"RemoteGameServerClient.SaveAuctionItem({accountId}, {token}, {auctionItemId})");

            SaveAuctionItemRequest saveAuctionItemRequest = new SaveAuctionItemRequest(auctionItem.AuctionItemId, auctionItem);

            networkManagerServer.StartCoroutine(SaveAuctionItemEnumerator(saveAuctionItemRequest));
        }

        public IEnumerator SaveAuctionItemEnumerator(SaveAuctionItemRequest saveAuctionItemRequest) {
            //Debug.Log($"RemoteGameServerClient.SaveAuctionItemEnumerator(auctionItemId: {saveAuctionItemRequest.Id})");

            string requestURL = $"{serverAddress}/{saveAuctionItemPath}";
            var payload = JsonUtility.ToJson(saveAuctionItemRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.SaveAuctionItemEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");


                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.SaveAuctionItemEnumerator(auctionItemId: {saveAuctionItemRequest.Id}) error saving auctionItem. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                }

            }
        }

        public void DeleteAuctionItem(int auctionItemId) {
            //Debug.Log($"RemoteGameServerClient.DeleteAuctionItem({auctionItemId})");

            DeleteAuctionItemRequest deleteAuctionItemRequest = new DeleteAuctionItemRequest(auctionItemId);

            networkManagerServer.StartCoroutine(DeleteAuctionItemEnumerator(deleteAuctionItemRequest));
        }

        public IEnumerator DeleteAuctionItemEnumerator(DeleteAuctionItemRequest deleteAuctionItemRequest) {
            //Debug.Log($"RemoteGameServerClient.DeleteAuctionItemEnumerator({token}, {deleteAuctionItemRequest.Id})");

            string requestURL = $"{serverAddress}/{deleteAuctionItemPath}";
            var payload = JsonUtility.ToJson(deleteAuctionItemRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.DeleteAuctionItemEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.DeleteAuctionItemEnumerator() delete auctionItem failed. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                }
            }
        }

        public void LoadAuctionItemList() {
            //Debug.Log($"RemoteGameServerClient.LoadAuctionItemList()");

            networkManagerServer.StartCoroutine(LoadAuctionItemListEnumerator());
        }

        public IEnumerator LoadAuctionItemListEnumerator() {
            //Debug.Log($"RemoteGameServerClient.LoadAuctionItemListEnumerator()");

            string requestURL = $"{serverAddress}/{getAuctionItemListPath}";
            string payload = string.Empty;
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.LoadAuctionItemListEnumerator() unable to load auctionItem list. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    AuctionItemListResponse auctionItemListResponse = JsonUtility.FromJson<AuctionItemListResponse>(webRequest.downloadHandler.text);
                    auctionService.ProcessLoadAuctionItemListResponse(auctionItemListResponse.auctionItems);
                }
            }
        }

        // ****************************************
        // MAIL MESSAGE
        // ****************************************

        public void CreateMailMessage(MailMessage mailMessage, MailMessageRequest mailMessageRequest, int recipientPlayerCharacterId) {
            //Debug.Log($"RemoteGameServerClient.CreateMailMessage()");

            networkManagerServer.StartCoroutine(CreateMailMessageEnumerator(mailMessage, mailMessageRequest, recipientPlayerCharacterId));
        }

        public IEnumerator CreateMailMessageEnumerator(MailMessage mailMessage, MailMessageRequest mailMessageRequest, int recipientPlayerCharacterId) {
            //Debug.Log($"RemoteGameServerClient.CreateMailMessageEnumerator()");

            CreateMailMessageRequest createMailMessageRequest = new CreateMailMessageRequest(mailMessage, recipientPlayerCharacterId);

            string requestURL = $"{serverAddress}/{createMailMessagePath}";
            var payload = JsonUtility.ToJson(createMailMessageRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.CreateMailMessageEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.CreateMailMessageEnumerator() There was an error assigning a mailMessage Id. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    CreateMailMessageResponse createMailMessageResponse = JsonUtility.FromJson<CreateMailMessageResponse>(webRequest.downloadHandler.text);
                    mailMessage.MessageId = createMailMessageResponse.id;
                    mailService.ProcessMailMessageIdAssigned(mailMessage, mailMessageRequest, recipientPlayerCharacterId);
                }
            }
        }

        public void SaveMailMessage(MailMessage mailMessage) {
            //Debug.Log($"RemoteGameServerClient.SaveMailMessage(mailMessageId: {mailMessage.MessageId})");

            SaveMailMessageRequest saveMailMessageRequest = new SaveMailMessageRequest(mailMessage.MessageId, mailMessage);

            networkManagerServer.StartCoroutine(SaveMailMessageEnumerator(saveMailMessageRequest));
        }

        public void SaveMailAndRefreshMessages(int accountId, int playerCharacterId, MailMessage mailMessage) {
            //Debug.Log($"RemoteGameServerClient.SaveMailMessage(mailMessageId: {mailMessage.MessageId})");

            SaveMailMessageRequest saveMailMessageRequest = new SaveMailMessageRequest(mailMessage.MessageId, mailMessage);

            networkManagerServer.StartCoroutine(SaveMailAndRefreshMessagesEnumerator(accountId, playerCharacterId, saveMailMessageRequest));
        }

        public IEnumerator SaveMailMessageEnumerator(SaveMailMessageRequest saveMailMessageRequest) {
            //Debug.Log($"RemoteGameServerClient.SaveMailMessageEnumerator(mailMessageId: {saveMailMessageRequest.Id})");

            string requestURL = $"{serverAddress}/{saveMailMessagePath}";
            var payload = JsonUtility.ToJson(saveMailMessageRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.SaveMailMessageEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");


                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.SaveMailMessageEnumerator(mailMessageId: {saveMailMessageRequest.Id}) error saving mailMessage. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                }

            }
        }

        public IEnumerator SaveMailAndRefreshMessagesEnumerator(int accountId, int playerCharacterId, SaveMailMessageRequest saveMailMessageRequest) {
            //Debug.Log($"RemoteGameServerClient.SaveMailAndRefreshMessagesEnumerator(mailMessageId: {saveMailMessageRequest.Id})");

            string requestURL = $"{serverAddress}/{saveMailMessagePath}";
            var payload = JsonUtility.ToJson(saveMailMessageRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.SaveMailAndRefreshMessagesEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");


                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.SaveMailAndRefreshMessagesEnumerator(mailMessageId: {saveMailMessageRequest.Id}) error saving mailMessage. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    LoadMailMessageList(accountId, playerCharacterId);
                }

            }
        }

        public void DeleteMailMessage(int accountId, int mailMessageId) {
            //Debug.Log($"RemoteGameServerClient.DeleteMailMessage({mailMessageId})");

            DeleteMailMessageRequest deleteMailMessageRequest = new DeleteMailMessageRequest(mailMessageId);

            networkManagerServer.StartCoroutine(DeleteMailMessageEnumerator(accountId, deleteMailMessageRequest));
        }

        public IEnumerator DeleteMailMessageEnumerator(int accountId, DeleteMailMessageRequest deleteMailMessageRequest) {
            //Debug.Log($"RemoteGameServerClient.DeleteMailMessageEnumerator({token}, {deleteMailMessageRequest.Id})");

            string requestURL = $"{serverAddress}/{deleteMailMessagePath}";
            var payload = JsonUtility.ToJson(deleteMailMessageRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.DeleteMailMessageEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.DeleteMailMessageEnumerator() delete mailMessage failed. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    mailService.ProcessDeleteMessage(accountId, deleteMailMessageRequest.Id);
                }
            }
        }

        public void LoadMailMessageList(int accountId, int playerCharacterId) {
            //Debug.Log($"RemoteGameServerClient.LoadMailMessageList(accountId: {accountId}, playerCharacterId: {playerCharacterId})");

            networkManagerServer.StartCoroutine(LoadMailMessageListEnumerator(accountId, playerCharacterId));
        }

        public IEnumerator LoadMailMessageListEnumerator(int accountId, int playerCharacterId) {
            //Debug.Log($"RemoteGameServerClient.LoadMailMessageListEnumerator(accountId: {accountId}, playerCharacterId: {playerCharacterId})");

            LoadMailMessageListRequest loadMailMessageListRequest = new LoadMailMessageListRequest(playerCharacterId);

            string requestURL = $"{serverAddress}/{getMailMessageListPath}";
            string payload = JsonUtility.ToJson(loadMailMessageListRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.LoadMailMessageListEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.LoadMailMessageListEnumerator(accountId: {accountId}, playerCharacterId: {playerCharacterId}) getting list failed. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    MailMessageListResponse mailMessageListResponse = JsonUtility.FromJson<MailMessageListResponse>(webRequest.downloadHandler.text);
                    mailService.ProcessMailMessageListResponse(accountId, mailMessageListResponse.mailMessages);
                }
            }
        }

        
        public void RequestMarkMailMessageAsRead(int messageId, int playerCharacterId) {
            //Debug.Log($"RemoteGameServerClient.RequestMarkMailMessageAsRead(messageid: {messageId}, playerCharacterId: {playerCharacterId})");

            networkManagerServer.StartCoroutine(MarkMailAsReadEnumerator(messageId, playerCharacterId));
        }

        public IEnumerator MarkMailAsReadEnumerator(int messageId, int playerCharacterId) {
            //Debug.Log($"RemoteGameServerClient.MarkMailAsReadEnumerator(messageid: {messageId}, playerCharacterId: {playerCharacterId})");

            LoadMailMessageRequest loadMailMessageListRequest = new LoadMailMessageRequest(messageId);

            string requestURL = $"{serverAddress}/{getMailMessagePath}";
            string payload = JsonUtility.ToJson(loadMailMessageListRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.LoadMailMessageEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.MarkMailAsReadEnumerator(messageId: {messageId}, playerCharacterId: {playerCharacterId}) marking message read failed. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    MailMessageSerializedData mailMessageSerializedData = JsonUtility.FromJson<MailMessageSerializedData>(webRequest.downloadHandler.text);
                    MailMessage mailMessage = JsonUtility.FromJson<MailMessage>(mailMessageSerializedData.saveData);
                    if (mailMessage != null) {
                        mailService.ProcessMarkMessageAsRead(mailMessage, playerCharacterId);
                    }
                }
            }
        }
        

        public void RequestTakeAttachments(int messageid, int playerCharacterId, int accountId) {
            networkManagerServer.StartCoroutine(TakeAttachmentsEnumerator(messageid, playerCharacterId, accountId));
        }

        public IEnumerator TakeAttachmentsEnumerator(int messageId, int playerCharacterId, int accountId) {
            //Debug.Log($"RemoteGameServerClient.TakeAttachmentsEnumerator(accountId: {accountId}, playerCharacterId: {playerCharacterId})");

            LoadMailMessageRequest loadMailMessageRequest = new LoadMailMessageRequest(messageId);

            string requestURL = $"{serverAddress}/{getMailMessagePath}";
            string payload = JsonUtility.ToJson(loadMailMessageRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.TakeAttachmentsEnumerator(messageId: {messageId}, playerCharacterId: {playerCharacterId}) take attachments failed. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    MailMessageSerializedData mailMessageSerializedData = JsonUtility.FromJson<MailMessageSerializedData>(webRequest.downloadHandler.text);
                    MailMessage mailMessage = JsonUtility.FromJson<MailMessage>(mailMessageSerializedData.saveData);
                    if (mailMessage != null) {
                        mailService.ProcessTakeAttachments(mailMessage, playerCharacterId, accountId);
                    }
                }
            }
        }

        public void RequestTakeAttachment(int messageid, int playerCharacterId, int attachmentSlotId) {
            networkManagerServer.StartCoroutine(TakeAttachmentEnumerator(messageid, playerCharacterId, attachmentSlotId));
        }

        public IEnumerator TakeAttachmentEnumerator(int messageId, int playerCharacterId, int attachmentslotId) {
            //Debug.Log($"RemoteGameServerClient.TakeAttachmentEnumerator(messageId: {messageId}, playerCharacterId: {playerCharacterId}, attachmentslotId: {attachmentslotId})");

            LoadMailMessageRequest loadMailMessageListRequest = new LoadMailMessageRequest(messageId);

            string requestURL = $"{serverAddress}/{getMailMessagePath}";
            string payload = JsonUtility.ToJson(loadMailMessageListRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.TakeAttachmentEnumerator(messageId: {messageId}, playerCharacterId: {playerCharacterId}) take attachment failed. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    MailMessageSerializedData mailMessageSerializedData = JsonUtility.FromJson<MailMessageSerializedData>(webRequest.downloadHandler.text);
                    MailMessage mailMessage = JsonUtility.FromJson<MailMessage>(mailMessageSerializedData.saveData);
                    if (mailMessage != null) {
                        mailService.ProcessTakeAttachment(mailMessage, playerCharacterId, attachmentslotId);
                    }
                }
            }
        }

        // ****************************************
        // ITEM INSTANCE
        // ****************************************

        public void CreateItemInstance(InstantiatedItem itemInstance) {
            //Debug.Log($"RemoteGameServerClient.CreateItemInstance(itemInstanceId: {itemInstance.InstanceId})");

            networkManagerServer.StartCoroutine(CreateItemInstanceEnumerator(itemInstance));
        }

        public IEnumerator CreateItemInstanceEnumerator(InstantiatedItem itemInstance) {
            //Debug.Log($"RemoteGameServerClient.CreateItemInstanceEnumerator(itemInstanceId: {itemInstance.InstanceId})");

            ItemInstanceSaveData itemInstanceSaveData = itemInstance.GetItemSaveData();
            CreateItemInstanceRequest createItemInstanceRequest = new CreateItemInstanceRequest(itemInstanceSaveData);

            string requestURL = $"{serverAddress}/{createItemInstancePath}";
            var payload = JsonUtility.ToJson(createItemInstanceRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.CreateItemInstanceEnumerator() There was an error creating an item. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    //CreateItemInstanceResponse createItemInstanceResponse = JsonUtility.FromJson<CreateItemInstanceResponse>(webRequest.downloadHandler.text);
                    //itemInstance.InstanceId = createItemInstanceResponse.Id;
                    //systemItemManager.ProcessItemInstanceIdAssigned(itemInstance);
                }
            }
        }

        public void SaveItemInstance(InstantiatedItem itemInstance) {
            //Debug.Log($"RemoteGameServerClient.SaveItemInstance({accountId}, {token}, {itemInstanceId})");

            ItemInstanceSaveData itemInstanceSaveData = itemInstance.GetItemSaveData();
            SaveItemInstanceRequest saveItemInstanceRequest = new SaveItemInstanceRequest(itemInstance.InstanceId, itemInstanceSaveData);

            networkManagerServer.StartCoroutine(SaveItemInstanceEnumerator(saveItemInstanceRequest));
        }

        public IEnumerator SaveItemInstanceEnumerator(SaveItemInstanceRequest saveItemInstanceRequest) {
            //Debug.Log($"RemoteGameServerClient.SaveItemInstanceEnumerator(itemInstanceId: {saveItemInstanceRequest.Id})");

            string requestURL = $"{serverAddress}/{saveItemInstancePath}";
            var payload = JsonUtility.ToJson(saveItemInstanceRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.SaveItemInstanceEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");


                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.SaveItemInstanceEnumerator(itemInstanceId: {saveItemInstanceRequest.ItemInstanceId}) error saving itemInstance. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                }

            }
        }

        public void DeleteItemInstance(long itemInstanceId) {
            //Debug.Log($"RemoteGameServerClient.DeleteItemInstance({itemInstanceId})");

            DeleteItemInstanceRequest deleteItemInstanceRequest = new DeleteItemInstanceRequest(itemInstanceId);

            networkManagerServer.StartCoroutine(DeleteItemInstanceEnumerator(deleteItemInstanceRequest));
        }

        public IEnumerator DeleteItemInstanceEnumerator(DeleteItemInstanceRequest deleteItemInstanceRequest) {
            //Debug.Log($"RemoteGameServerClient.DeleteItemInstanceEnumerator({token}, {deleteItemInstanceRequest.Id})");

            string requestURL = $"{serverAddress}/{deleteItemInstancePath}";
            var payload = JsonUtility.ToJson(deleteItemInstanceRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.DeleteItemInstanceEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.DeleteItemInstanceEnumerator() delete itemInstance failed. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                }
            }
        }

        public void LoadItemInstanceList() {
            //Debug.Log($"RemoteGameServerClient.LoadItemInstanceList()");

            networkManagerServer.StartCoroutine(LoadItemInstanceListEnumerator());
        }

        public IEnumerator LoadItemInstanceListEnumerator() {
            //Debug.Log($"RemoteGameServerClient.LoadItemInstanceListEnumerator()");

            string requestURL = $"{serverAddress}/{getItemInstanceListPath}";
            string payload = string.Empty;
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                //Debug.Log($"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.LoadItemInstanceListEnumerator() unable to load itemInstance list. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    ItemInstanceListResponse itemInstanceListResponse = JsonUtility.FromJson<ItemInstanceListResponse>(webRequest.downloadHandler.text);
                    systemItemManager.ProcessLoadAllItemInstances(itemInstanceListResponse.itemInstances);
                    serverDataService.ProcessItemsLoaded();
                }
            }
        }

        // ****************************************
        // FRIENDS
        // ****************************************

        public void LoadAllFriendLists() {
            //Debug.Log($"RemoteGameServerClient.LoadAllFriendLists()");

            networkManagerServer.StartCoroutine(LoadAllFriendListsEnumerator());
        }

        public IEnumerator LoadAllFriendListsEnumerator() {
            //Debug.Log($"RemoteGameServerClient.LoadAllFriendListsEnumerator()");

            string requestURL = $"{serverAddress}/{getFriendListsPath}";
            string payload = string.Empty;
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                //Debug.Log($"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.LoadAllFriendListsEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.LoadAllFriendListsEnumerator() unable to load itemInstance list. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                } else {
                    FriendListResponse friendListResponse = JsonUtility.FromJson<FriendListResponse>(webRequest.downloadHandler.text);
                    friendServiceServer.ProcessLoadAllFriendLists(friendListResponse.friendLists);
                }
            }
        }

        public void SaveFriendList(FriendList friendList) {

            FriendListSaveData friendListSaveData = new FriendListSaveData(friendList);
            networkManagerServer.StartCoroutine(SaveFriendListEnumerator(new SaveFriendListRequest(friendList.playerCharacterId, friendListSaveData)));
        }

        public IEnumerator SaveFriendListEnumerator(SaveFriendListRequest saveFriendListRequest) {
            //Debug.Log($"RemoteGameServerClient.SaveFriendListEnumerator(playerCharacterId: {saveFriendListRequest.PlayerCharacterId})");

            string requestURL = $"{serverAddress}/{saveFriendListPath}";
            var payload = JsonUtility.ToJson(saveFriendListRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                if (systemConfigurationManager.ValidateAPIServerCert == false) {
                    // Assign the bypass handler
                    webRequest.certificateHandler = new BypassCertificateHandler();
                }
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                //Debug.Log($"RemoteGameServerClient.SaveItemInstanceEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");

                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    Debug.LogWarning($"RemoteGameServerClient.SaveFriendListEnumerator(playerCharacterId: {saveFriendListRequest.PlayerCharacterId}) error saving friend list. status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                }

            }
        }

    }

}

