using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface ITargetable {

        string MyName { get; }
        bool CanCastOnEnemy { get; }
        bool CanCastOnFriendly { get; }
        bool RequireLineOfSight { get; }
    }

}