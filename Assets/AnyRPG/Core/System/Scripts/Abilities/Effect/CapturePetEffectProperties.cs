using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class CapturePetEffectProperties : InstantEffectProperties {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitType))]
        protected List<string> unitTypeRestrictions = new List<string>();

        protected List<UnitType> unitTypeRestrictionList = new List<UnitType>();

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, string displayName) {
            base.SetupScriptableObjects(systemGameManager, displayName);
            if (unitTypeRestrictions != null && unitTypeRestrictions.Count > 0) {
                foreach (string unitTypeRestriction in unitTypeRestrictions) {
                    //Debug.Log(DisplayName + ".CapturePetEffect.SetupScriptableObjects(): looping through restrictions: " + unitTypeRestriction);
                    UnitType tmpUnitType = systemDataFactory.GetResource<UnitType>(unitTypeRestriction);
                    if (tmpUnitType != null) {
                        unitTypeRestrictionList.Add(tmpUnitType);
                    } else {
                        Debug.LogError("CapturePetEffect.SetupScriptableObjects(): Could not find unitTypeRestriction: " + unitTypeRestriction + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }
        }

    }
}