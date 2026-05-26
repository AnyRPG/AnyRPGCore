using UnityEngine;

namespace AnyRPG {
    public interface ILearnable {

        string DisplayName { get; }
        int RequiredLevel { get; }
        Sprite Icon { get; }

        string GetDescription();
        string GetShortDescription();
    }

}