namespace AnyRPG {
    public interface IEventTrigger {

        event System.Action OnEventTriggered;

        void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName);
        void CleanupScriptableObjects();
    }
}