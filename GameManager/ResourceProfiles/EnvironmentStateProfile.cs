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

        
        [SerializeField]
        private Material skyBoxMaterial;

        public Material MySkyBoxMaterial { get => skyBoxMaterial; set => skyBoxMaterial = value; }
    }

}