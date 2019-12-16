using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Resource Description", menuName = "AnyRPG/ResourceDescription")]
    public class ResourceDescription : ResourceProfile {

        [SerializeField]
        protected string displayName = string.Empty;

        public string MyDisplayName { get => displayName; set => displayName = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            // overwrite me
        }


    }

}