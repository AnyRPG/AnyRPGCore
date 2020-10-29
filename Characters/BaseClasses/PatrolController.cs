using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class PatrolController {

        // references
        private UnitController unitController;

        private List<PatrolProfile> patrolProfiles = new List<PatrolProfile>();

        public List<PatrolProfile> PatrolProfiles { get => patrolProfiles; set => patrolProfiles = value; }


        // state
        private PatrolProfile currentPatrol = null;

        public PatrolProfile MyCurrentPatrol {
            get {
                return currentPatrol;
            }
            set => currentPatrol = value;
        }

        public PatrolController(UnitController unitController) {
            this.unitController = unitController;
            SetupScriptableObjects();
        }

        public void Init() {
            // this should be run after the unit profile is set
            FindAutomaticPatrol();
        }

        public void BeginPatrolByIndex(int patrolIndex) {
            if (patrolIndex < 0 || patrolIndex >= patrolProfiles.Count) {
                Debug.Log("PatrolController.BeginPatrolByIndex(" + patrolIndex + "): invalid index");
                return;
            }
            string patrolName = patrolProfiles[patrolIndex].DisplayName;
            BeginPatrol(patrolName);
        }

        public void BeginPatrol(string patrolName) {
            //Debug.Log(gameObject.name + ".AIPatrol.BeginPatrol(" + (patrolName != null ? patrolName : "null" ) + ")");
            PatrolProfile tmpPatrolProfile = SystemPatrolProfileManager.MyInstance.GetNewResource(patrolName);
            if (tmpPatrolProfile != null) {
                tmpPatrolProfile.CurrentUnitController = unitController;
                SetCurrentPatrol(tmpPatrolProfile);
                unitController.ChangeState(new PatrolState());
            }
        }

        public void SetCurrentPatrol(PatrolProfile newPatrolProfile) {
            currentPatrol = newPatrolProfile;
        }

        private void FindAutomaticPatrol() {
            foreach (PatrolProfile patrolProfile in patrolProfiles) {
                if (patrolProfile.AutoStart == true) {
                    currentPatrol = patrolProfile;
                    currentPatrol.CurrentUnitController = unitController;
                    break;
                }
            }
        }

        private void SetupScriptableObjects() {


            // local patrols
            if (unitController.PatrolNames != null) {
                foreach (string patrolName in unitController.PatrolNames) {
                    if (patrolName != null && patrolName != string.Empty) {
                        PatrolProfile _tmpPatrolProfile = SystemPatrolProfileManager.MyInstance.GetNewResource(patrolName);
                        if (_tmpPatrolProfile != null) {
                            patrolProfiles.Add(_tmpPatrolProfile);
                        } else {
                            Debug.LogError("PatrolController.SetupScriptableObjects: could not find patrol name: " + patrolName);
                        }
                    }
                }
            }

            // patrols from unit profile
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.UnitProfile != null && unitController.BaseCharacter.UnitProfile.PatrolNames != null) {
                foreach (string patrolName in unitController.BaseCharacter.UnitProfile.PatrolNames) {
                    if (patrolName != null && patrolName != string.Empty) {
                        PatrolProfile _tmpPatrolProfile = SystemPatrolProfileManager.MyInstance.GetNewResource(patrolName);
                        if (_tmpPatrolProfile != null) {
                            patrolProfiles.Add(_tmpPatrolProfile);
                        } else {
                            Debug.LogError("PatrolController.SetupScriptableObjects: could not find patrol name: " + patrolName);
                        }
                    }
                }
            }

        }

    }

}