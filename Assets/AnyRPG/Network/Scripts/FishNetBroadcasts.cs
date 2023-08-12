using FishNet.Broadcast;

namespace AnyRPG
{
    public struct PasswordBroadcast : IBroadcast
    {
        public string Username;
        public string Password;
    }

    public struct ResponseBroadcast : IBroadcast
    {
        public bool Passed;
    }

}