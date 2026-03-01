namespace AnyRPG {
    public interface IMovementState {
        void Enter(bool isReplay);

        void Update(bool isReplay, double timeInterval);

        void Exit(bool isReplay);
    }
}