using System;
using System.Security.Cryptography;
using UnityEngine;

namespace AnyRPG {
    public static class AuthenticationHelpers {
        public static void ProvideSaltAndHash(UserAccount userAccount) {
            var salt = GenerateSalt();
            userAccount.Salt = Convert.ToBase64String(salt);
            userAccount.PasswordHash = ComputeHash(userAccount.PasswordHash, userAccount.Salt);
        }

        private static byte[] GenerateSalt() {
            var rng = RandomNumberGenerator.Create();
            var salt = new byte[24];
            rng.GetBytes(salt);
            return salt;
        }

        public static string ComputeHash(string password, string saltString) {
            var salt = Convert.FromBase64String(saltString);

            using var hashGenerator = new Rfc2898DeriveBytes(password, salt);
            hashGenerator.IterationCount = 10101;
            var bytes = hashGenerator.GetBytes(24);
            return Convert.ToBase64String(bytes);
        }
    }
}