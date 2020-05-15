using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface ITargetable {

        bool CanCastOnEnemy { get; }
        bool CanCastOnFriendly { get; }
    }

}