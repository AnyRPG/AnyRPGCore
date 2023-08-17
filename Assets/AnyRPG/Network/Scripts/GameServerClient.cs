using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AnyRPG {

    public class GameServerClient {

        private const double clientTimeout = 30;
        private const string loginPath = "api/login";
        private const string createPlayerCharacterPath = "api/createplayercharacter";
        private const string GetPlayerCharactersPath = "api/getplayercharacters";

        private string serverAddress = string.Empty;

        public GameServerClient(string serverAddress) {
            this.serverAddress = serverAddress;
        }

        /*
        // for reference
        public static async Task<Resource> GetResource() {
            using (var httpClient = new HttpClient()) {
                httpClient.BaseAddress = new Uri(URL);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync("api/session");
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;
                var resourceJson = await response.Content.ReadAsStringAsync();
                return JsonUtility.FromJson<Resource>(resourceJson);
            }
        }
        */

        public (bool, string) Login(string username, string password) {
            Debug.Log($"GameServerClient.Login({username}, {password})");
            LoginRequest loginRequest = new LoginRequest(username, password);

            
            Task<string> loginTokenResult = GetLoginToken(loginRequest);
            loginTokenResult.Wait();
            string token = loginTokenResult.Result;
            
            if (token == null) {
                return (false, string.Empty);
            }
            return (true, token);
        }

        public async Task<string> GetLoginToken(LoginRequest loginRequest) {
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

        public bool CreatePlayerCharacter(string token, AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"GameServerClient.CreatePlayerCharacter({token})");

            CreatePlayerCharacterRequest createPlayerCharacterRequest = new CreatePlayerCharacterRequest(anyRPGSaveData);

            Task<bool> createPlayerCharacterResult = CreatePlayerCharacterAsync(token, createPlayerCharacterRequest);
            createPlayerCharacterResult.Wait();
            bool result = createPlayerCharacterResult.Result;

            return result;
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

        public List<PlayerCharacterData> LoadCharacterList(string token) {
            Debug.Log($"GameServerClient.LoadCharacterList({token})");

            //CreatePlayerCharacterRequest createPlayerCharacterRequest = new CreatePlayerCharacterRequest(anyRPGSaveData);

            Task<List<PlayerCharacterData>> loadCharacterListResult = LoadCharacterListAsync(token);
            loadCharacterListResult.Wait();
            List<PlayerCharacterData> result = loadCharacterListResult.Result;

            Debug.Log($"GameServerClient.LoadCharacterList() list size: {result.Count}");
            return result;
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

    public class LoginRequest {
        public string UserName = string.Empty;
        public string Password = string.Empty;

        public LoginRequest(string username, string password) {
            UserName = username;
            Password = password;
        }
    }

}

