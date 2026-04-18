using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class NamePlateProps {

        [Header("NAMEPLATE SETTINGS")]

        [Tooltip("This is what will be printed on the nameplate above the object.  It will also override whatever value is set for the Interactable mouseover display name.")]
        [SerializeField]
        private string displayName = string.Empty;

        [Tooltip("If true, the nameplate position will be set to this value")]
        [SerializeField]
        private bool overrideNameplatePosition = false;

        [Tooltip("The position of the NamePlate anchor, relative to the unit pivot")]
        [SerializeField]
        private Vector3 namePlatePosition = Vector3.zero;

        public string DisplayName { get => displayName; set => displayName = value; }
        public bool OverrideNameplatePosition { get => overrideNameplatePosition; set => overrideNameplatePosition = value; }
        public Vector3 NameplatePosition { get => namePlatePosition; set => namePlatePosition = value; }
    }


}