using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface ITargetable {

        string DisplayName { get; }
        bool CanCastOnEnemy { get; }
        bool CanCastOnFriendly { get; }
        bool RequireLineOfSight { get; }
        LineOfSightSourceLocation LineOfSightSourceLocation { get; }
        TargetRangeSourceLocation TargetRangeSourceLocation { get; }
    }

}