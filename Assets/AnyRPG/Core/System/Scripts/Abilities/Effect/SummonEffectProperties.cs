using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class SummonEffectProperties : InstantEffectProperties {

        [Header("Summon")]

        [Tooltip("Unit Prefab Profile to use for the summon pet")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private string unitProfileName = string.Empty;

        // reference to actual unitProfile
        private UnitProfile unitProfile = null;

        // reference to spawned object UnitController
        private UnitController petUnitController;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }


        public override void SetupScriptableObjects(SystemGameManager systemGameManager, string displayName) {
            base.SetupScriptableObjects(systemGameManager, displayName);

            if (unitProfileName != null && unitProfileName != string.Empty) {
                UnitProfile tmpUnitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
                if (tmpUnitProfile != null) {
                    unitProfile = tmpUnitProfile;
                } else {
                    Debug.LogError("SummonEffect.SetupScriptableObjects(): Could not find unitProfile : " + unitProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }
        }


    }

}
