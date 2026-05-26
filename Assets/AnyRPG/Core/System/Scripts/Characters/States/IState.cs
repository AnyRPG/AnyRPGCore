namespace AnyRPG {
    public interface IState {
        // prepare the state
        void Enter(UnitController parent);

        void Update();

        void Exit();

    }
}