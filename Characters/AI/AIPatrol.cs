using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class AIPatrol : MonoBehaviour {

        [SerializeField]
        private List<Vector3> destinationList = new List<Vector3>();

        // If randomDestinations is set to false, they are followed in order.  Otherwise, they are chosen randomly from the destinationList
        [SerializeField]
        private bool randomDestinations = false;

        // after travelling to all destinations, restart from the first and continue patrolling
        [SerializeField]
        private bool loopDestinations = true;

        // should the character despawn when the patrol is complete
        [SerializeField]
        private bool despawnOnCompletion = false;

        // 0 for infinite, any other number and the patrol will end after that number is reached
        [SerializeField]
        private int maxDestinations = 0;

        // used when the destination list is empty and a random destination needs to be chosen
        [SerializeField]
        private float maxDistanceFromSpawnPoint;

        // how long to pause at each destination before going to the next one
        [SerializeField]
        private float destinationPauseTime;

        // the current count of destinations reached
        private int destinationRetrievedCount = 0;

        // the current count of destinations reached
        private int destinationReachedCount = 0;

        // track the current position in the list of destinations to travel to on the patrol path
        private int destinationIndex = 0;

        // keep track of the current destination
        private Vector3 currentDestination = Vector3.zero;

        private CharacterUnit characterUnit;

        public bool MyDespawnOnCompletion { get => despawnOnCompletion; }
        public float MyDestinationPauseTime { get => destinationPauseTime; set => destinationPauseTime = value; }

        protected void Awake() {
            //Debug.Log(gameObject.name + ".AIPatrol.Awake()");
            characterUnit = GetComponent<CharacterUnit>();
        }

        void Start() {
            //Debug.Log(gameObject.name + ".AIPatrol.Start(): destinationList length: " + destinationList.Count);
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
            //Debug.Log("AIPatrol.PatrolComplete(): loopDestination: " + loopDestinations + "; destinationReachedCount: " + destinationReachedCount + "; maxDestinations: " + maxDestinations + "; destinationListCount: " + destinationList.Count);

            if (randomDestinations && maxDestinations == 0) {
                //Debug.Log("AIPatrol.PatrolComplete() randomDestinations && maxDestinations == 0; return false");
                return false;
            }

            if (!loopDestinations && destinationReachedCount >= destinationList.Count) {
                return true;
            }

            // apply destination amount cap
            if (maxDestinations > 0 && destinationReachedCount >= maxDestinations) {
                return true;
            }
            //Debug.Log("AIPatrol.PatrolComplete(): returning false");
            return false;
        }

        public Vector3 GetRandomDestination() {
            //Debug.Log(gameObject.name + ".AIPatrol.GetRandomDestination()");
            if (destinationList.Count > 0) {
                // get destination from list
                int randomNumber = Random.Range(0, destinationList.Count);
                return destinationList[randomNumber];
            } else {
                // choose nearby random destination
                float randomXNumber = Random.Range(0, maxDistanceFromSpawnPoint * 2) - maxDistanceFromSpawnPoint;
                float randomZNumber = Random.Range(0, maxDistanceFromSpawnPoint * 2) - maxDistanceFromSpawnPoint;
                if (characterUnit == null) {
                    //Debug.Log("AIPatrol.GetRandomDestination(): CharacterUnit is null!");
                    return Vector3.zero;
                }

                // get a random point that's on the navmesh
                Vector3 randomPoint = (characterUnit.MyCharacter.MyCharacterController as AIController).MyStartPosition + new Vector3(randomXNumber, 0, randomZNumber);
                randomPoint = characterUnit.MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.CorrectedNavmeshPosition(randomPoint);
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

        public Vector3 GetLinearDestination() {
            //Debug.Log("AIPatrol.GetLinearDestination(): destinationIndex: " + destinationIndex);
            Vector3 returnValue = destinationList[destinationIndex];
            destinationIndex++;
            if (destinationIndex >= destinationList.Count) {
                destinationIndex = 0;
            }
            return returnValue;
        }

    }

}