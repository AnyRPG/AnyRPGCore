namespace AnyRPG {
    public interface IMoveableOwner {

        IMoveable Moveable { get; }

        void CancelHandscriptMove();
    }
}