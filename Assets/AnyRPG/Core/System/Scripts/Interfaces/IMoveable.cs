using AnyRPG;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IMoveable {

        Sprite Icon { get; }
        string DisplayName { get; }

        void AssignToHandScript(Image backgroundImage);
    }
}