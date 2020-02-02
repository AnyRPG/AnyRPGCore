using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class AIPatrol : MonoBehaviour {

        [SerializeField]
        private List<string> patrolNames = new List<string>();

        private List<PatrolProfile> patrolProfiles = new List<PatrolProfile>();

        private PatrolProfile automaticPatrol = null;

        private CharacterUnit characterUnit;

        public PatrolProfile MyAutomaticPatrol { get => automaticPatrol; set => automaticPatrol = value; }

        protected void Awake() {
            //Debug.Log(gameObject.name + ".AIPatrol.Awake()");
            characterUnit = GetComponent<CharacterUnit>();
            SetupScriptableObjects();
            FindAutomaticPatrol();
        }

        void Start() {
            //Debug.Log(gameObject.name + ".AIPatrol.Start(): destinationList length: " + destinationList.Count);

        }

        private void FindAutomaticPatrol() {
            foreach (PatrolProfile patrolProfile in patrolProfiles) {
                if (patrolProfile.MyAutoStart == true) {
                    automaticPatrol = patrolProfile;
                    automaticPatrol.MyCharacterUnit = characterUnit;
                    break;
                }
            }
        }



        private void SetupScriptableObjects() {
            foreach (string patrolName in patrolNames) {
                if (patrolName != null && patrolName != string.Empty) {
                    PatrolProfile _tmpPatrolProfile = SystemPatrolProfileManager.MyInstance.GetNewResource(patrolName);
                    if (_tmpPatrolProfile != null) {
                        patrolProfiles.Add(_tmpPatrolProfile);
                    } else {
                        Debug.LogError("AIPatrol.SetupScriptableObjects: could not find patrol name: " + patrolName);
                    }
                }
            }
        }

    }

}