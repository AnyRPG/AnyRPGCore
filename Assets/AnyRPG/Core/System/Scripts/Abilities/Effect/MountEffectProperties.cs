using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


namespace AnyRPG {

    [System.Serializable]
    public class MountEffectProperties : StatusEffectProperties {

        [Header("Mount")]

        [Tooltip("Unit Prefab Profile to use for the mount object")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private string unitProfileName = string.Empty;

        // reference to actual unitProfile
        private UnitProfile unitProfile = null;


        public override void SetupScriptableObjects(SystemGameManager systemGameManager, string displayName) {
            base.SetupScriptableObjects(systemGameManager, displayName);

            if (unitProfileName != null && unitProfileName != string.Empty) {
                UnitProfile tmpUnitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
                if (tmpUnitProfile != null) {
                    unitProfile = tmpUnitProfile;
                } else {
                    Debug.LogError("MountEffect.SetupScriptableObjects(): Could not find prefab Profile : " + unitProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("MountEffect.SetupScriptableObjects(): Mount effect requires a unit prefab profile but non was configured while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
            }

        }



    }
}
