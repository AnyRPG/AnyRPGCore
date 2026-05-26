namespace AnyRPG {
    public class AuthenticationRequest {
        public string UserName { get; set; }
        public string Password { get; set; }

        public AuthenticationRequest() {
            UserName = string.Empty;
            Password = string.Empty;
        }

        public AuthenticationRequest(string userName, string password) {
            UserName = userName;
            Password = password;
        }
    }
}