using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    
    [CreateAssetMenu(fileName = "New Swappable Mesh Model Profile", menuName = "AnyRPG/SwappableMeshModelProfile")]
    [System.Serializable]
    public class SwappableMeshModelProfile : DescribableResource {
        
        [SerializeField]
        private SwappableMeshModelOptions modelOptions = new SwappableMeshModelOptions();

        public SwappableMeshModelOptions ModelOptions { get => modelOptions; }
    }

}