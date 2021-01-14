using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface ITargetable {

        string DisplayName { get; }
        /*
        bool RequiresTarget { get; }
        bool RequiresGroundTarget { get; }
        bool RequiresLiveTarget { get; }
        bool RequireDeadTarget { get; }
        bool CanCastOnSelf { get; }
        bool CanCastOnOthers { get; }
        bool CanCastOnEnemy { get; }
        bool CanCastOnNeutral { get; }
        bool CanCastOnFriendly { get; }
        bool AutoSelfCast { get; }
        bool RequireLineOfSight { get; }
        bool UseMeleeRange { get; }
        int MaxRange { get; }
        LineOfSightSourceLocation LineOfSightSourceLocation { get; }
        TargetRangeSourceLocation TargetRangeSourceLocation { get; }
        */
        TargetProps GetTargetOptions(IAbilityCaster abilityCaster);
    }

}