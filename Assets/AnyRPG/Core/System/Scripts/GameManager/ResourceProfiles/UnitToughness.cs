using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Unit Toughness", menuName = "AnyRPG/UnitToughness")]
    [System.Serializable]
    public class UnitToughness : DescribableResource {

        [Header("Unit Toughness")]

        [Tooltip("List of special projector cookies to use for units with this toughness")]
        [SerializeField]
        private List<ProjectorColorMapNode> focusProjectorOverrideMap = new List<ProjectorColorMapNode>();

        [Header("Currency Multiplier")]

        [Tooltip("Multiply the total currency gain from a kill by this amount")]
        [SerializeField]
        private float currencyMultiplier = 1f;

        [Header("Experience Multiplier")]

        [Tooltip("Multiply the total experience gain from a kill by this amount")]
        [SerializeField]
        private float experienceMultiplier = 1f;

        [Header("Resource Multiplier")]

        [Tooltip("Multiply all resources")]
        [SerializeField]
        private float defaultResourceMultiplier = 1f;

        [Tooltip("Multiply specific resources")]
        [SerializeField]
        private List<ResourceMultiplierNode> resourceMultipliers = new List<ResourceMultiplierNode>();

        [Header("Stat Multiplier")]

        [Tooltip("Multiply all primary stats")]
        [SerializeField]
        private float defaultPrimaryStatMultiplier = 1f;

        [Tooltip("Multiply primary stats")]
        [SerializeField]
        private List<PrimaryStatMultiplierNode> primaryStatMultipliers = new List<PrimaryStatMultiplierNode>();

        public List<ProjectorColorMapNode> FocusProjectorOverrideMap { get => focusProjectorOverrideMap; set => focusProjectorOverrideMap = value; }
        public float ExperienceMultiplier { get => experienceMultiplier; set => experienceMultiplier = value; }
        public List<PrimaryStatMultiplierNode> PrimaryStatMultipliers { get => primaryStatMultipliers; set => primaryStatMultipliers = value; }
        public List<ResourceMultiplierNode> ResourceMultipliers { get => resourceMultipliers; set => resourceMultipliers = value; }
        public float CurrencyMultiplier { get => currencyMultiplier; set => currencyMultiplier = value; }
        public float DefaultResourceMultiplier { get => defaultResourceMultiplier; set => defaultResourceMultiplier = value; }
        public float DefaultPrimaryStatMultiplier { get => defaultPrimaryStatMultiplier; set => defaultPrimaryStatMultiplier = value; }
    }

    [System.Serializable]
    public class PrimaryStatMultiplierNode {

        [Tooltip("The name of the stat to multiply")]
        [SerializeField]
        private string statName = string.Empty;

        [Tooltip("The value to multiply the stat by.  A value of 1 will result in the stat staying the same.")]
        [SerializeField]
        private float statMultiplier = 1f;

        public string StatName { get => statName; set => statName = value; }
        public float StatMultiplier { get => statMultiplier; set => statMultiplier = value; }
    }

    [System.Serializable]
    public class ResourceMultiplierNode {

        [Tooltip("The name of the resource to multiply")]
        [SerializeField]
        private string resourceName = string.Empty;

        [Tooltip("The value to multiply the resource by.  A value of 1 will result in the stat staying the same.")]
        [FormerlySerializedAs("statMultiplier")]
        [SerializeField]
        private float valueMultiplier = 1f;

        public float ValueMultiplier { get => valueMultiplier; set => valueMultiplier = value; }
        public string ResourceName { get => resourceName; set => resourceName = value; }
    }


}