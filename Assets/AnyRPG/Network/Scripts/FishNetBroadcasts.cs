using FishNet.Broadcast;

namespace AnyRPG
{
    public struct PasswordBroadcast : IBroadcast
    {
        public string Username;
        public string Password;
        public string ClientVersion;
    }

    public struct ResponseBroadcast : IBroadcast
    {
        public bool AuthenticationPassed;
        public bool ClientPassed;
        public string RequiredClientVersion;
    }

}