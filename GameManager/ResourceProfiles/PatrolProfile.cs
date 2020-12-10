using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Patrol Profile", menuName = "AnyRPG/PatrolProfile")]
    public class PatrolProfile : DescribableResource {

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

        // the current count of destinations reached
        private int destinationRetrievedCount = 0;

        // the current count of destinations reached
        private int destinationReachedCount = 0;

        // track the current position in the list of destinations to travel to on the patrol path
        private int destinationIndex = 0;

        // keep track of the current destination
        private Vector3 currentDestination = Vector3.zero;

        private UnitController unitController;

        public bool DespawnOnCompletion { get => despawnOnCompletion; }
        public float DestinationPauseTime { get => destinationPauseTime; set => destinationPauseTime = value; }
        public UnitController CurrentUnitController { get => unitController; set => unitController = value; }
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

        public Vector3 GetDestination(bool destinationReached) {
            //Debug.Log("AIPatrol.GetDestination(" + destinationReached + ")");
            Vector3 returnValue = Vector3.zero;

            if (destinationReached || destinationRetrievedCount == 0) {
                // choose next correct destination from list
                if (randomDestinations) {
                    returnValue = GetRandomDestination();
                } else {
                    returnValue = GetLinearDestination();
                }
            } else {
                // return current destination since it has not yet been reached and this is not the first retrieval
                returnValue = currentDestination;
            }

            if (destinationRetrievedCount == 0) {
                // not allowed to reach destination on first retrieve
                destinationRetrievedCount++;
            } else {
                if (destinationReached) {
                    // if destination was not reached, we do not increment the retrieval because it is the current destination
                    destinationRetrievedCount++;
                    destinationReachedCount++;
                }
            }

            // check if patrol is complete
            if (PatrolComplete()) {
                returnValue = Vector3.zero;
            }

            currentDestination = returnValue;
            return returnValue;
        }

        public bool PatrolComplete() {
            //Debug.Log("PatrolProfile.PatrolComplete(): loopDestination: " + loopDestinations + "; destinationReachedCount: " + destinationReachedCount + "; maxDestinations: " + maxDestinations + "; destinationListCount: " + destinationList.Count);

            if (randomDestinations && (maxDestinations == 0 || destinationReachedCount < maxDestinations)) {
                //Debug.Log("AIPatrol.PatrolComplete() randomDestinations && (maxDestinations == 0 || destinationReachedCount < maxDestinations); return false");
                return false;
            }

            if (!loopDestinations && destinationReachedCount >= DestinationCount) {
                return true;
            }

            // apply destination amount cap
            if (maxDestinations > 0 && destinationReachedCount >= maxDestinations) {
                return true;
            }
            //Debug.Log("AIPatrol.PatrolComplete(): returning false");
            return false;
        }

        /// <summary>
        /// get a random destination from the list, or a random destination near the spawn point if no list exists
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRandomDestination() {
            //Debug.Log(MyName + ".AIPatrol.GetRandomDestination()");
            if (DestinationCount > 0) {
                // get destination from list
                int randomNumber = Random.Range(0, DestinationCount);
                return GetDestinationByIndex(randomNumber);
            } else {
                // choose nearby random destination
                float randomXNumber = Random.Range(0, maxDistanceFromSpawnPoint * 2) - maxDistanceFromSpawnPoint;
                float randomZNumber = Random.Range(0, maxDistanceFromSpawnPoint * 2) - maxDistanceFromSpawnPoint;
                if (unitController == null) {
                    //Debug.Log("AIPatrol.GetRandomDestination(): CharacterUnit is null!");
                    return Vector3.zero;
                }

                // get a random point that's on the navmesh
                Vector3 randomPoint = unitController.MyStartPosition + new Vector3(randomXNumber, 0, randomZNumber);
                randomPoint = unitController.UnitMotor.CorrectedNavmeshPosition(randomPoint);
                /*
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas)) {
                    //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): destinationPosition " + destinationPosition + " on NavMesh found closest point: " + hit.position + ")");
                    randomPoint = hit.position;
                } else {
                    //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): destinationPosition " + randomPoint + " was not on NavMesh! return start position instead");
                    return (characterUnit.MyCharacter.MyCharacterController as AIController).MyStartPosition;
                }
                */
                return randomPoint;
            }
        }

        /// <summary>
        /// return a vector3 location for a destination in the current destination list
        /// </summary>
        /// <param name="listIndex"></param>
        /// <returns></returns>
        public Vector3 GetDestinationByIndex(int listIndex) {
            Vector3 returnValue = Vector3.zero;
            if (useTags == false) {
                returnValue = destinationList[listIndex];
            } else {
                GameObject tagObject = GameObject.FindGameObjectWithTag(destinationTagList[listIndex]);
                if (tagObject != null) {
                    returnValue = tagObject.transform.position;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// get the next destination based on the current index
        /// </summary>
        /// <returns></returns>
        public Vector3 GetLinearDestination() {
            //Debug.Log("AIPatrol.GetLinearDestination(): destinationIndex: " + destinationIndex);
            Vector3 returnValue = GetDestinationByIndex(destinationIndex);
            destinationIndex++;
            if (destinationIndex >= DestinationCount) {
                destinationIndex = 0;
            }
            return returnValue;
        }

    }

}