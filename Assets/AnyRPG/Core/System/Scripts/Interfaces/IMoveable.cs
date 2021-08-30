using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public interface IMoveable {

        Sprite Icon { get; }
        string DisplayName { get; }
    }
}