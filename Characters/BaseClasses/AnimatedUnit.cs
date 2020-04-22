using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class AnimatedUnit : MonoBehaviour  {

        /*
        //[SerializeField]
        protected BaseCharacter baseCharacter = null;

    */
        protected NavMeshAgent agent;

        protected Rigidbody rigidBody;

        protected CharacterMotor characterMotor;

        protected CharacterAnimator characterAnimator;

        protected bool componentReferencesInitialized = false;

        protected bool eventSubscriptionsInitialized = false;

        protected bool orchestratorStartupComplete = false;
        protected bool orchestratorFinishComplete = false;

        private CharacterUnit characterUnit;

        public CharacterUnit MyCharacterUnit { get => characterUnit; set => characterUnit = value; }

        /*
        public BaseCharacter MyCharacter {
            get => baseCharacter;
            set {
                baseCharacter = value;
            }
        }
        */

        public NavMeshAgent MyAgent { get => agent; set => agent = value; }
        public Rigidbody MyRigidBody { get => rigidBody; set => rigidBody = value; }
        public CharacterMotor MyCharacterMotor { get => characterMotor; set => characterMotor = value; }
        public CharacterAnimator MyCharacterAnimator { get => characterAnimator; set => characterAnimator = value; }
        //public BaseCharacter MyBaseCharacter { get => MyCharacter; }

        protected virtual void Awake() {
            //Debug.Log(gameObject.name + ".AnimatedUnit.Awake()");
        }

        protected virtual void Start() {
            //Debug.Log(gameObject.name + ".AnimatedUnit.Start()");
            if (GetComponent<CharacterUnit>() != null) {
                OrchestratorStart();
                OrchestratorFinish();
            }
        }

        public virtual void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".AnimatedUnit.OrchestrateStart()");
            if (orchestratorStartupComplete == true) {
                return;
            }
            GetComponentReferences();
            if (characterMotor != null) {
                characterMotor.OrchestratorStart();
            }
            if (characterAnimator != null) {
                characterAnimator.OrchestratorStart();
            }
            orchestratorStartupComplete = true;
        }

        public virtual void OrchestratorFinish() {
            if (orchestratorFinishComplete) {
                return;
            }
            //Debug.Log(gameObject.name + ".AnimatedUnit.OrchestrateStartup()");
            CreateEventSubscriptions();
            if (characterMotor != null) {
                characterMotor.OrchestratorFinish();
            }
            if (characterAnimator != null) {
                characterAnimator.OrchestratorFinish();
            }
            orchestratorFinishComplete = true;
        }

        public virtual void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            //Debug.Log("CharacterUnit.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
        }

        protected virtual void OnEnable() {
            //Debug.Log(gameObject.name + ".CharacterUnit.OnEnable()");
            CreateEventSubscriptions();
        }

        protected virtual void OnDisable() {
            //Debug.Log(gameObject.name + ".CharacterUnit.OnDisable()");
            CleanupEventSubscriptions();
        }

        public virtual void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".AnimatedUnit.GetComponentReferences()");
            if (componentReferencesInitialized) {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): already initialized. exiting!");
                return;
            }
            agent = GetComponent<NavMeshAgent>();
            rigidBody = GetComponent<Rigidbody>();
            characterMotor = GetComponent<CharacterMotor>();
            characterAnimator = GetComponent<CharacterAnimator>();
            characterUnit = GetComponent<CharacterUnit>();
            /*
            if (baseCharacter == null) {
                baseCharacter = GetComponent<BaseCharacter>();
                if (baseCharacter == null) {
                    //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): baseCharacter was null and is still null");
                } else {
                    //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): baseCharacter was null but is now initialized to: " + baseCharacter.MyCharacterName);
                }
            }
            */
        }


    }

}