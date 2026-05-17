using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IEventTrigger {

        event System.Action OnEventTriggered;

        void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName);
        void CleanupScriptableObjects();
    }
}