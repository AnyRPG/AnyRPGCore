using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IRewardable : IDescribable {
        
        void GiveReward(UnitController sourceUnitController);
        bool HasReward(UnitController sourceUnitController);
    }

}