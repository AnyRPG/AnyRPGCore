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

        [SerializeField]
        private List<ProjectorColorMapNode> focusProjectorOverrideMap = new List<ProjectorColorMapNode>();


        [SerializeField]
        private float healthMultiplier = 1f;

        [SerializeField]
        private float manaMultiplier = 1f;

        [SerializeField]
        private float intellectMultiplier = 1f;

        [SerializeField]
        private float staminaMultiplier = 1f;

        [SerializeField]
        private float strengthMultiplier = 1f;

        [SerializeField]
        private float agilityMultiplier = 1f;

        public float MyHealthMultiplier { get => healthMultiplier; set => healthMultiplier = value; }
        public float MyManaMultiplier { get => manaMultiplier; set => manaMultiplier = value; }
        public float MyIntellectMultiplier { get => intellectMultiplier; set => intellectMultiplier = value; }
        public float MyStaminaMultiplier { get => staminaMultiplier; set => staminaMultiplier = value; }
        public float MyStrengthMultiplier { get => strengthMultiplier; set => strengthMultiplier = value; }
        public float MyAgilityMultiplier { get => agilityMultiplier; set => agilityMultiplier = value; }
        public List<ProjectorColorMapNode> MyFocusProjectorOverrideMap { get => focusProjectorOverrideMap; set => focusProjectorOverrideMap = value; }
    }

}