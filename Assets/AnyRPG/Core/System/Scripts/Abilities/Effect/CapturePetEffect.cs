using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Capture Pet Effect", menuName = "AnyRPG/Abilities/Effects/CapturePetEffect")]
    public class CapturePetEffect : InstantEffect {
        /*
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitType))]
        protected List<string> unitTypeRestrictions = new List<string>();

        protected List<UnitType> unitTypeRestrictionList = new List<UnitType>();

        public List<string> UnitTypeRestrictions { get => unitTypeRestrictions; set => unitTypeRestrictions = value; }
        */

        [SerializeField]
        private CapturePetEffectProperties capturePetEffectProperties = new CapturePetEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => capturePetEffectProperties; }


    }
}