using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class AIPatrol : MonoBehaviour {

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
        }

        void Start() {
            //Debug.Log(gameObject.name + ".AIPatrol.Start(): destinationList length: " + destinationList.Count);
            // testing moved these 2 from start in case they wake-up earlier than characterUnit and can't find the unit profile to get the patrol from
            SetupScriptableObjects();
            FindAutomaticPatrol();
        }

        public void BeginPatrolByIndex(int patrolIndex) {
            if (patrolIndex < 0 || patrolIndex >= patrolProfiles.Count) {
                Debug.Log(gameObject.name + ".AIPatrol.BeginPatrolByIndex(" + patrolIndex + "): invalid index");
                return;
            }
            string patrolName = patrolProfiles[patrolIndex].DisplayName;
            BeginPatrol(patrolName);
        }

        public void BeginPatrol(string patrolName) {
            //Debug.Log(gameObject.name + ".AIPatrol.BeginPatrol(" + (patrolName != null ? patrolName : "null" ) + ")");
            PatrolProfile tmpPatrolProfile = SystemPatrolProfileManager.MyInstance.GetNewResource(patrolName);
            if (tmpPatrolProfile != null) {
                tmpPatrolProfile.MyCharacterUnit = characterUnit;
                SetCurrentPatrol(tmpPatrolProfile);
                (characterUnit.MyCharacter.CharacterController as AIController).ChangeState(new PatrolState());
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
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.UnitProfile != null && characterUnit.MyCharacter.UnitProfile.PatrolNames != null) {
                foreach (string patrolName in characterUnit.MyCharacter.UnitProfile.PatrolNames) {
                    if (patrolName != null && patrolName != string.Empty) {
                        PatrolProfile _tmpPatrolProfile = SystemPatrolProfileManager.MyInstance.GetNewResource(patrolName);
                        if (_tmpPatrolProfile != null) {
                            patrolProfiles.Add(_tmpPatrolProfile);
                        } else {
                            Debug.LogError(gameObject.name + ".AIPatrol.SetupScriptableObjects: could not find patrol name: " + patrolName);
                        }
                    }
                }
            }
        }

    }

}