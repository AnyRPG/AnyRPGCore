using System;
using System.Collections;
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
            
            //string token = "1234";

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
                return resourceJson;
            }
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

