using System;
using UnityEngine;

namespace AnyRPG {

    [Serializable]
    public class UserAccount {
        public int Id;
        public string UserName;
        public string PasswordHash;
        public string Salt;
        public string Email;
        public string Phone;

        public UserAccount() {
            UserName = string.Empty;
            PasswordHash = string.Empty;
            Salt = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
        }
    }
}

