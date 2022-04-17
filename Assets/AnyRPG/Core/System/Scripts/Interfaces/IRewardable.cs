using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IRewardable : IDescribable {
        
        void GiveReward();
        bool HasReward();
    }

}