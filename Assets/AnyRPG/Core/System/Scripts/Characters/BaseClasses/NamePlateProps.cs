using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class NameplateProps {

        [Header("NAMEPLATE SETTINGS")]

        [Tooltip("This is what will be printed on the nameplate above the object.  It will also override whatever value is set for the Interactable mouseover display name.")]
        [SerializeField]
        private string displayName = string.Empty;

        [Tooltip("If true, the nameplate position will be set to this value.")]
        [SerializeField]
        private bool overrideNameplatePosition = false;

        [Tooltip("The position of the Nameplate anchor, relative to the unit pivot.")]
        [FormerlySerializedAs("namePlatePosition")]
        [SerializeField]
        private Vector3 nameplatePosition = Vector3.zero;

        public string DisplayName { get => displayName; set => displayName = value; }
        public bool OverrideNameplatePosition { get => overrideNameplatePosition; set => overrideNameplatePosition = value; }
        public Vector3 NameplatePosition { get => nameplatePosition; set => nameplatePosition = value; }
    }


}