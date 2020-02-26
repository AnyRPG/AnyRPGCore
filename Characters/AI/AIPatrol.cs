using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class AIPatrol : MonoBehaviour {

        [SerializeField]
        private List<string> patrolNames = new List<string>();

        private List<PatrolProfile> patrolProfiles = new List<PatrolProfile>();

        //private PatrolProfile automaticPatrol = null;

        private PatrolProfile currentPatrol = null;

        private CharacterUnit characterUnit;

        //public PatrolProfile MyAutomaticPatrol { get => automaticPatrol; set => automaticPatrol = value; }

        public PatrolProfile MyCurrentPatrol {
            get {
                return currentPatrol;
            }
            set => currentPatrol = value;
        }

        protected void Awake() {
            //Debug.Log(gameObject.name + ".AIPatrol.Awake()");
            characterUnit = GetComponent<CharacterUnit>();
            SetupScriptableObjects();
            FindAutomaticPatrol();
        }

        void Start() {
            //Debug.Log(gameObject.name + ".AIPatrol.Start(): destinationList length: " + destinationList.Count);
        }

        public void BeginPatrolByIndex(int patrolIndex) {
            if (patrolIndex < 0 || patrolIndex >= patrolNames.Count) {
                Debug.Log(gameObject.name + ".AIPatrol.BeginPatrolByIndex(" + patrolIndex + "): invalid index");
                return;
            }
            string patrolName = patrolNames[patrolIndex];
            BeginPatrol(patrolName);
        }

        public void BeginPatrol(string patrolName) {
            //Debug.Log(gameObject.name + ".AIPatrol.BeginPatrol(" + (patrolName != null ? patrolName : "null" ) + ")");
            PatrolProfile tmpPatrolProfile = SystemPatrolProfileManager.MyInstance.GetNewResource(patrolName);
            if (tmpPatrolProfile != null) {
                tmpPatrolProfile.MyCharacterUnit = characterUnit;
                SetCurrentPatrol(tmpPatrolProfile);
                (characterUnit.MyCharacter.MyCharacterController as AIController).ChangeState(new PatrolState());
            }
        }

        public void SetCurrentPatrol(PatrolProfile newPatrolProfile) {
            currentPatrol = newPatrolProfile;
        }

        private void FindAutomaticPatrol() {
            foreach (PatrolProfile patrolProfile in patrolProfiles) {
                if (patrolProfile.MyAutoStart == true) {
                    currentPatrol = patrolProfile;
                    currentPatrol.MyCharacterUnit = characterUnit;
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