namespace AnyRPG {
    public interface IMovementState {
        void Enter(bool isReplay, bool isSilent);

        void Update(bool isReplay, double timeInterval);

        void Exit(bool isReplay, bool isSilent);
    }
}