using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Unit Profile", menuName = "AnyRPG/UnitProfile")]
    [System.Serializable]
    public class UnitProfile : DescribableResource {

        [SerializeField]
        private GameObject unitPrefab;

        [SerializeField]
        private string defaultToughness = string.Empty;

        protected UnitToughness unitToughness;

        [SerializeField]
        private string defaultAutoAttackAbilityName;

        /*
        [SerializeField]
        private string defaultAutoAttackAbility;
        */

        private BaseAbility realDefaultAutoAttackAbility = null;

        [SerializeField]
        private bool isUMAUnit;

        // this unit can be charmed and made into a pet
        [SerializeField]
        private bool isPet = false;

        public GameObject MyUnitPrefab { get => unitPrefab; set => unitPrefab = value; }
        public UnitToughness MyDefaultToughness { get => unitToughness; set => unitToughness = value; }
        public BaseAbility MyDefaultAutoAttackAbility { get => realDefaultAutoAttackAbility; set => realDefaultAutoAttackAbility = value; }
        public bool MyIsUMAUnit { get => isUMAUnit; set => isUMAUnit = value; }
        public bool MyIsPet { get => isPet; set => isPet = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            realDefaultAutoAttackAbility = null;
            if (defaultAutoAttackAbilityName != null && defaultAutoAttackAbilityName != string.Empty) {
                realDefaultAutoAttackAbility = SystemAbilityManager.MyInstance.GetResource(defaultAutoAttackAbilityName);
            }/* else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + defaultAutoAttackAbilityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
            }*/

            if (unitToughness == null && defaultToughness != null && defaultToughness != string.Empty) {
                UnitToughness tmpToughness = SystemUnitToughnessManager.MyInstance.GetResource(defaultToughness);
                if (tmpToughness != null) {
                    unitToughness = tmpToughness;
                } else {
                    Debug.LogError("Unit Toughness: " + defaultToughness + " not found while initializing character stats.  Check Inspector!");
                }
            }

        }
    }

}