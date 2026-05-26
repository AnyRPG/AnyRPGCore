namespace AnyRPG {
    public interface IPrerequisiteOwner {

        string DisplayName { get; }

        void HandlePrerequisiteUpdates(UnitController sourceUnitController);
    }

}