using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    [System.Serializable]
    public class PatrolProps  {

        [Header("Patrol")]

        [Tooltip("whether a unit that has this patrol should begin it automatically upon spawning or entering patrol state")]
        [SerializeField]
        private bool autoStart = false;

        [Tooltip("List of vectors to patrol to")]
        [SerializeField]
        private List<Vector3> destinationList = new List<Vector3>();

        [Tooltip("If true, use tags instead of vectors for patrol")]
        [SerializeField]
        private bool useTags = false;

        [Tooltip("List of tags to find and patrol to")]
        [SerializeField]
        private List<string> destinationTagList = new List<string>();

        [Tooltip("If randomDestinations is set to false, they are followed in order.  Otherwise, they are chosen randomly from the destinationList")]
        [SerializeField]
        private bool randomDestinations = false;

        [Tooltip("after travelling to all destinations, restart from the first and continue patrolling")]
        [SerializeField]
        private bool loopDestinations = true;

        [Tooltip("should the character despawn when the patrol is complete")]
        [SerializeField]
        private bool despawnOnCompletion = false;

        [Tooltip("0 for infinite, any other number and the patrol will end after that number is reached")]
        [SerializeField]
        private int maxDestinations = 0;

        [Tooltip("used when the destination list is empty and a random destination needs to be chosen")]
        [SerializeField]
        private float maxDistanceFromSpawnPoint = 1f;

        [Tooltip("how long to pause at each destination before going to the next one")]
        [SerializeField]
        private float destinationPauseTime = 0f;

        [Tooltip("zero will not override current movement speed, anything else will")]
        [SerializeField]
        private float movementSpeed = 0f;

        [Tooltip("If true, the unit will attempt to save it's position when it reaches the destination.")]
        [SerializeField]
        private bool savePositionAtDestination = false;


        public bool DespawnOnCompletion { get => despawnOnCompletion; }
        public float DestinationPauseTime { get => destinationPauseTime; set => destinationPauseTime = value; }
        public bool AutoStart { get => autoStart; set => autoStart = value; }
        public float MovementSpeed { get => movementSpeed; set => movementSpeed = value; }
        public bool SavePositionAtDestination { get => savePositionAtDestination; set => savePositionAtDestination = value; }
        public int DestinationCount {
            get {
                if (useTags == true) {
                    return destinationTagList.Count;
                }
                return destinationList.Count;
            }
        }

        public bool UseTags { get => useTags; set => useTags = value; }
        public List<string> DestinationTagList { get => destinationTagList; set => destinationTagList = value; }
        public List<Vector3> DestinationList { get => destinationList; set => destinationList = value; }
        public bool RandomDestinations { get => randomDestinations; set => randomDestinations = value; }
        public int MaxDestinations { get => maxDestinations; set => maxDestinations = value; }
        public bool LoopDestinations { get => loopDestinations; set => loopDestinations = value; }
        public float MaxDistanceFromSpawnPoint { get => maxDistanceFromSpawnPoint; set => maxDistanceFromSpawnPoint = value; }
    }

}