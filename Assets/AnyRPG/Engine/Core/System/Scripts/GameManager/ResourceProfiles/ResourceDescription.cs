using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Resource Description", menuName = "AnyRPG/ResourceDescription")]
    public class ResourceDescription : ResourceProfile {

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            // overwrite me
        }


    }

}