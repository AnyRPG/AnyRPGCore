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
        private int defaultToughness = 1;

        [SerializeField]
        private string defaultAutoAttackAbility;

        [SerializeField]
        private bool isUMAUnit;

        public GameObject MyUnitPrefab { get => unitPrefab; set => unitPrefab = value; }
        public int MyDefaultToughness { get => defaultToughness; set => defaultToughness = value; }
        public string MyDefaultAutoAttackAbility { get => defaultAutoAttackAbility; set => defaultAutoAttackAbility = value; }
        public bool MyIsUMAUnit { get => isUMAUnit; set => isUMAUnit = value; }
    }

}