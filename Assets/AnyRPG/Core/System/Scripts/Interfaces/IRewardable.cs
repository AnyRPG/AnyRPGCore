namespace AnyRPG {
    public interface IRewardable : IDescribable {
        
        void GiveReward(UnitController sourceUnitController);
        bool HasReward(UnitController sourceUnitController);
    }

}