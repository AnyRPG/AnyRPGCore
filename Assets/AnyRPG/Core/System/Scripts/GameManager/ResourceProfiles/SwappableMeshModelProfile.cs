using UnityEngine;

namespace AnyRPG {
    
    [CreateAssetMenu(fileName = "New Swappable Mesh Model Profile", menuName = "AnyRPG/SwappableMeshModelProfile")]
    [System.Serializable]
    public class SwappableMeshModelProfile : DescribableResource {
        
        [SerializeField]
        private SwappableMeshModelOptions modelOptions = new SwappableMeshModelOptions();

        public SwappableMeshModelOptions ModelOptions { get => modelOptions; }
    }

}