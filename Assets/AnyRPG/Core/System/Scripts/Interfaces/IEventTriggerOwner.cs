namespace AnyRPG {
    public interface IEventTriggerOwner {

        string DisplayName { get; }

        void HandleEventTriggered();
    }

}