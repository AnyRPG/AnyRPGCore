using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IPrerequisite {

        event System.Action OnStatusUpdated;

        // whether or not this is a match if the condition is true.  set to false to make a negative match
        bool IsMet(BaseCharacter baseCharacter);

        void SetupScriptableObjects();
        void CleanupScriptableObjects();
        void UpdateStatus();
    }
}