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

        public PatrolProfile CurrentPatrol {
            get {
                return currentPatrol;
            }
            set => currentPatrol = value;
        }

        public PatrolController(UnitController unitController) {
            this.unitController = unitController;
        }

        // this should be run after the unit profile is set
        public void Init() {
            //Debug.Log("PatrolController.Init()");

            SetupScriptableObjects();

            FindAutomaticPatrol();
        }

        public void BeginPatrolByIndex(int patrolIndex) {
            if (patrolIndex < 0 || patrolIndex >= patrolProfiles.Count) {
                //Debug.Log("PatrolController.BeginPatrolByIndex(" + patrolIndex + "): invalid index");
                return;
            }
            string patrolName = patrolProfiles[patrolIndex].DisplayName;
            BeginPatrol(patrolName);
        }

        public void BeginPatrol(string patrolName) {
            //Debug.Log(unitController.gameObject.name + ".PatrolController.BeginPatrol(" + (patrolName != null ? patrolName : "null" ) + ")");
            PatrolProfile tmpPatrolProfile = SystemPatrolProfileManager.MyInstance.GetNewResource(patrolName);
            if (tmpPatrolProfile != null) {
                tmpPatrolProfile.CurrentUnitController = unitController;
                SetCurrentPatrol(tmpPatrolProfile);
                unitController.ChangeState(new PatrolState());
                return;
            } else {
                Debug.LogError(unitController.gameObject.name + ".PatrolController.BeginPatrol() could not find patrol: " + (patrolName != null ? patrolName : "null") + ")");
            }
        }

        public void SetCurrentPatrol(PatrolProfile newPatrolProfile) {
            currentPatrol = newPatrolProfile;
        }

        private void FindAutomaticPatrol() {
            //Debug.Log(unitController.gameObject.name + ".patrolController.FindAutomaticPatrol()");
            if (unitController.UnitControllerMode != UnitControllerMode.AI) {
                return;
            }

            foreach (PatrolProfile patrolProfile in patrolProfiles) {
                //Debug.Log(unitController.gameObject.name + ".patrolController.FindAutomaticPatrol(): found patrol profile: " + patrolProfile.DisplayName);
                if (patrolProfile.PatrolProperties.AutoStart == true) {
                    //Debug.Log(unitController.gameObject.name + ".patrolController.FindAutomaticPatrol(): found autostart profile");
                    currentPatrol = patrolProfile;
                    currentPatrol.CurrentUnitController = unitController;
                    break;
                }
            }
        }

        private void SetupScriptableObjects() {
            //Debug.Log(unitController.gameObject.name + ".patrolController.SetupScriptableObjects()");


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
            if (unitController != null && unitController.UnitProfile != null && unitController.UnitProfile.PatrolNames != null) {
                foreach (string patrolName in unitController.UnitProfile.PatrolNames) {
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