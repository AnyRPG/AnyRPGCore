using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    
    [CreateAssetMenu(fileName = "New UMA Model Profile", menuName = "AnyRPG/UMA Model Profile")]
    [System.Serializable]
    public class UMAModelProfile : DescribableResource {
        
        [SerializeField]
        private UMAModelOptions modelOptions = new UMAModelOptions();

        public UMAModelOptions ModelOptions { get => modelOptions; }
    }

}