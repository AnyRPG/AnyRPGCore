using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AnyRPG {

    public class GameServerClient : ConfiguredClass {

        private const double clientTimeout = 30;
        private const string loginPath = "api/login";
        private const string createPlayerCharacterPath = "api/createplayercharacter";
        private const string deletePlayerCharacterPath = "api/deleteplayercharacter";
        private const string GetPlayerCharactersPath = "api/getplayercharacters";

        private string serverAddress = string.Empty;

        private NetworkManagerServer networkManagerServer = null;

        public GameServerClient(SystemGameManager systemGameManager, string serverAddress) {
            this.serverAddress = serverAddress;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public void Login(int clientId, string username, string password) {
            Debug.Log($"GameServerClient.Login({username}, {password})");
            LoginRequest loginRequest = new LoginRequest(username, password);

            networkManagerServer.StartCoroutine(GetLoginTokenEnumerator(clientId, loginRequest));
            /*
            Task<string> loginTokenResult = GetLoginToken(loginRequest);
            loginTokenResult.Wait();
            string token = loginTokenResult.Result;
            
            if (token == null) {
                return (false, string.Empty);
            }
            */
            //return (true, token);
            //return (true, string.Empty);
        }

        public async Task<string> GetLoginTokenAsync(LoginRequest loginRequest) {
            Debug.Log($"GameServerClient.GetLoginToken()");

            using (var httpClient = new HttpClient()) {
                string requestURL = $"{serverAddress}/{loginPath}";
                httpClient.BaseAddress = new Uri(requestURL);
                httpClient.Timeout = TimeSpan.FromSeconds(clientTimeout);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var payload = JsonUtility.ToJson(loginRequest);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(requestURL, content).ConfigureAwait(false);
                //var result = await httpClient.PostAsJsonAsync("Create", otherPerson);
                Debug.Log($"GameServerClient.GetLoginToken() url: {requestURL} payload: {content.ToString()} statusCode: {result.StatusCode}");
                if (result.StatusCode != HttpStatusCode.OK)
                    return null;
                var resourceJson = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                Debug.Log($"GameServerClient.GetLoginToken(): {resourceJson}");
                LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(resourceJson);
                return loginResponse.token;
            }
        }

        public IEnumerator GetLoginTokenEnumerator(int clientId, LoginRequest loginRequest) {
            Debug.Log($"GameServerClient.GetLoginTokenEnumerator({clientId})");

            string requestURL = $"{serverAddress}/{loginPath}";
            var payload = JsonUtility.ToJson(loginRequest);

            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();
            //UnityWebRequest www = UnityWebRequest.Post(requestURL, string.Empty);
            //using (UnityWebRequest www = UnityWebRequest.Post(requestURL, payload)) {
            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                //byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
                //www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                //www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                //www.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                Debug.Log($"GameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    networkManagerServer.ProcessLoginResponse(clientId, false, string.Empty);
                } else {
                    LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(webRequest.downloadHandler.text);
                    networkManagerServer.ProcessLoginResponse(clientId, true, loginResponse.token);
                }
            }
            //www.Dispose();

        }


        public void CreatePlayerCharacter(string token, AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"GameServerClient.CreatePlayerCharacter({token})");

            CreatePlayerCharacterRequest createPlayerCharacterRequest = new CreatePlayerCharacterRequest(anyRPGSaveData);

            networkManagerServer.StartCoroutine(CreatePlayerCharacterEnumerator(token, createPlayerCharacterRequest));
            /*
            Task<bool> createPlayerCharacterResult = CreatePlayerCharacterAsync(token, createPlayerCharacterRequest);
            createPlayerCharacterResult.Wait();
            bool result = createPlayerCharacterResult.Result;

            return result;
            */
        }

        public async Task<bool> CreatePlayerCharacterAsync(string token, CreatePlayerCharacterRequest createPlayerCharacterRequest) {
            Debug.Log($"GameServerClient.CreatePlayerCharacterAsync({token})");

            using (var httpClient = new HttpClient()) {
                string requestURL = $"{serverAddress}/{createPlayerCharacterPath}";
                httpClient.BaseAddress = new Uri(requestURL);
                httpClient.Timeout = TimeSpan.FromSeconds(clientTimeout);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Debug.Log(httpClient.DefaultRequestHeaders.ToString());
                var payload = JsonUtility.ToJson(createPlayerCharacterRequest);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(requestURL, content).ConfigureAwait(false);
                //var result = await httpClient.PostAsJsonAsync("Create", otherPerson);
                Debug.Log($"GameServerClient.CreatePlayerCharacterAsync() url: {requestURL} payload: {payload} statusCode: {result.StatusCode}");
                if (result.StatusCode != HttpStatusCode.OK)
                    return false;
                string resourceJson = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                //Debug.Log($"GameServerClient.CreatePlayerCharacterAsync(): {resourceJson}");
                return true;
            }
        }

        public IEnumerator CreatePlayerCharacterEnumerator(string token, CreatePlayerCharacterRequest createPlayerCharacterRequest) {
            Debug.Log($"GameServerClient.CreatePlayerCharacterEnumerator({token})");

            string requestURL = $"{serverAddress}/{createPlayerCharacterPath}";
            var payload = JsonUtility.ToJson(createPlayerCharacterRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                //byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
                //www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                //www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                //www.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                Debug.Log($"GameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                /*
                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    networkManagerServer.ProcessLoginResponse(clientId, false, string.Empty);
                } else {
                    LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(webRequest.downloadHandler.text);
                    networkManagerServer.ProcessLoginResponse(clientId, true, loginResponse.token);
                }
                */
            }

        }

        public void DeletePlayerCharacter(int clientId, string token, int playerCharacterId) {
            Debug.Log($"GameServerClient.DeletePlayerCharacter({clientId}, {token}, {playerCharacterId})");

            DeletePlayerCharacterRequest deletePlayerCharacterRequest = new DeletePlayerCharacterRequest(playerCharacterId);

            networkManagerServer.StartCoroutine(DeletePlayerCharacterEnumerator(clientId, token, deletePlayerCharacterRequest));
            /*
            Task<bool> deletePlayerCharacterResult = DeletePlayerCharacterAsync(token, deletePlayerCharacterRequest);
            deletePlayerCharacterResult.Wait();
            bool result = deletePlayerCharacterResult.Result;

            return result;
            */
        }

        public async Task<bool> DeletePlayerCharacterAsync(string token, DeletePlayerCharacterRequest deletePlayerCharacterRequest) {
            Debug.Log($"GameServerClient.CreatePlayerCharacterAsync({token}, {deletePlayerCharacterRequest.Id})");

            using (var httpClient = new HttpClient()) {
                string requestURL = $"{serverAddress}/{deletePlayerCharacterPath}";
                httpClient.BaseAddress = new Uri(requestURL);
                httpClient.Timeout = TimeSpan.FromSeconds(clientTimeout);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Debug.Log(httpClient.DefaultRequestHeaders.ToString());
                var payload = JsonUtility.ToJson(deletePlayerCharacterRequest);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(requestURL, content).ConfigureAwait(false);
                //var result = await httpClient.PostAsJsonAsync("Create", otherPerson);
                Debug.Log($"GameServerClient.CreatePlayerCharacterAsync() url: {requestURL} payload: {payload} statusCode: {result.StatusCode}");
                if (result.StatusCode != HttpStatusCode.OK)
                    return false;
                string resourceJson = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                //Debug.Log($"GameServerClient.CreatePlayerCharacterAsync(): {resourceJson}");
                return true;
            }
        }

        public IEnumerator DeletePlayerCharacterEnumerator(int clientId, string token, DeletePlayerCharacterRequest deletePlayerCharacterRequest) {
            Debug.Log($"GameServerClient.DeletePlayerCharacterEnumerator({token}, {deletePlayerCharacterRequest.Id})");

            string requestURL = $"{serverAddress}/{deletePlayerCharacterPath}";
            var payload = JsonUtility.ToJson(deletePlayerCharacterRequest);
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                //byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
                //www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                //www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                //www.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                Debug.Log($"GameServerClient.DeletePlayerCharacterEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                
                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    //networkManagerServer.ProcessLoginResponse(clientId, false, string.Empty);
                } else {
                    //LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(webRequest.downloadHandler.text);
                    networkManagerServer.ProcessDeletePlayerCharacterResponse(clientId);
                }
            }
        }

        public void LoadCharacterList(int clientId, string token) {
            Debug.Log($"GameServerClient.LoadCharacterList({token})");

            networkManagerServer.StartCoroutine(LoadCharacterListEnumerator(clientId, token));
            /*
            Task<List<PlayerCharacterData>> loadCharacterListResult = LoadCharacterListAsync(token);
            loadCharacterListResult.Wait();
            List<PlayerCharacterData> result = loadCharacterListResult.Result;

            Debug.Log($"GameServerClient.LoadCharacterList() list size: {result.Count}");
            return result;
            */
        }

        public async Task<List<PlayerCharacterData>> LoadCharacterListAsync(string token) {
            Debug.Log($"GameServerClient.LoadCharacterListAsync({token})");

            using (var httpClient = new HttpClient()) {
                string requestURL = $"{serverAddress}/{GetPlayerCharactersPath}";
                httpClient.BaseAddress = new Uri(requestURL);
                httpClient.Timeout = TimeSpan.FromSeconds(clientTimeout);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Debug.Log(httpClient.DefaultRequestHeaders.ToString());
                string payload = string.Empty;
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(requestURL, content).ConfigureAwait(false);
                //var result = await httpClient.PostAsJsonAsync("Create", otherPerson);
                Debug.Log($"GameServerClient.LoadCharacterListAsync() url: {requestURL} payload: {payload} statusCode: {result.StatusCode}");
                if (result.StatusCode != HttpStatusCode.OK)
                    return new List<PlayerCharacterData>();
                string resourceJson = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                Debug.Log($"GameServerClient.LoadCharacterListAsync(): {resourceJson}");
                PlayerCharacterListResponse playerCharacterListResponse = JsonUtility.FromJson<PlayerCharacterListResponse>(resourceJson);

                Debug.Log($"GameServerClient.LoadCharacterListAsync(): list size: {playerCharacterListResponse.playerCharacters.Count}");
                return playerCharacterListResponse.playerCharacters;
            }
        }

        public IEnumerator LoadCharacterListEnumerator(int clientId, string token) {
            Debug.Log($"GameServerClient.LoadCharacterListEnumerator({token})");

            string requestURL = $"{serverAddress}/{GetPlayerCharactersPath}";
            string payload = string.Empty;
            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler)) {
                //byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
                //www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                //www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                //www.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
                webRequest.uploadHandler.contentType = "application/json";
                yield return webRequest.SendWebRequest();
                Debug.Log($"GameServerClient.GetLoginTokenEnumerator() status code: {webRequest.responseCode} body: {webRequest.downloadHandler.text}");
                
                if (webRequest.responseCode != (long)HttpStatusCode.OK) {
                    //networkManagerServer.ProcessLoginResponse(clientId, false, string.Empty);
                } else {
                    PlayerCharacterListResponse playerCharacterListResponse = JsonUtility.FromJson<PlayerCharacterListResponse>(webRequest.downloadHandler.text);
                    networkManagerServer.ProcessLoadCharacterListResponse(clientId, playerCharacterListResponse.playerCharacters);
                }
                
            }
        }
    }

    [Serializable]
    public class PlayerCharacterListResponse {
        public List<PlayerCharacterData> playerCharacters;

        public PlayerCharacterListResponse() {
            playerCharacters = new List<PlayerCharacterData>();
        }
    }

    [Serializable]
    public class PlayerCharacterData {
        public int id;
        public int accountId;
        public string name;
        public string saveData;

        public PlayerCharacterData() {
            name = string.Empty;
            saveData = string.Empty;
        }
    }

    public class PlayerCharacterSaveData {
        public int PlayerCharacterId;
        public AnyRPGSaveData SaveData;
    }

    public class LoginResponse {
        public string token = string.Empty;
    }

    public class CreatePlayerCharacterRequest {
        public string Name = string.Empty;
        public string SaveData = string.Empty;
        
        public CreatePlayerCharacterRequest(AnyRPGSaveData anyRPGSaveData) {
            Name = anyRPGSaveData.playerName;
            SaveData = JsonUtility.ToJson(anyRPGSaveData);
        }
    }

    public class DeletePlayerCharacterRequest {
        public int Id;
        
        public DeletePlayerCharacterRequest(int playerCharacterId) {
            Id = playerCharacterId;
        }
    }

    public class LoginRequest {
        public string UserName = string.Empty;
        public string Password = string.Empty;

        public LoginRequest(string username, string password) {
            UserName = username;
            Password = password;
        }
    }

}

