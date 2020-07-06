using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Environment State", menuName = "AnyRPG/EnvironmentState")]
    [System.Serializable]
    public class EnvironmentStateProfile : DescribableResource {

        [Header("Environment State")]
        
        [Tooltip("The skybox that should be used when this environment state is active")]
        [SerializeField]
        private Material skyBoxMaterial;

        public Material MySkyBoxMaterial { get => skyBoxMaterial; set => skyBoxMaterial = value; }
    }

}