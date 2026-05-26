using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Patrol Profile", menuName = "AnyRPG/PatrolProfile")]
    public class PatrolProfile : DescribableResource {

        [SerializeField]
        private PatrolProperties patrolProperties = new PatrolProperties();

        public PatrolProperties PatrolProperties { get => patrolProperties; set => patrolProperties = value; }

    }

}