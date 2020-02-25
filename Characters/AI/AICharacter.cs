using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AICharacter : BaseCharacter {

        /// <summary>
        ///  the prefab for the actual character modeel
        /// </summary>
        [SerializeField]
        private GameObject characterModelPrefab;

        [SerializeField]
        private GameObject characterModelGameObject = null;

        [SerializeField]
        private LootableCharacter lootableCharacter = null;

        [SerializeField]
        private bool preventAutoDespawn = false;

        public GameObject MyCharacterModelPrefab { get => characterModelPrefab; set => characterModelPrefab = value; }

        public GameObject MyCharacterModelGameObject { get => characterModelGameObject; set => characterModelGameObject = value; }
        public LootableCharacter MyLootableCharacter { get => lootableCharacter; set => lootableCharacter = value; }

        protected override void Awake() {
            //Debug.Log(gameObject.name + ".AICharacter.Awake()");
            base.Awake();
            OrchestratorStart();
            OrchestratorFinish();
        }

        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".AICharacter.GetComponentReferences()");
            base.GetComponentReferences();
            characterController = GetComponent<AIController>();
            characterStats = GetComponent<AIStats>();
            characterCombat = GetComponent<AICombat>();
            lootableCharacter = GetComponent<LootableCharacter>();
            animatedUnit = GetComponent<AnimatedUnit>();
            if (animatedUnit == null) {
                animatedUnit = gameObject.AddComponent<AnimatedUnit>();
            }
        }

        public override void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".AICharacter.OrchestratorStart()");

            // if this is run, or the getcomponents part anyway before the below block, then character unit will be set properly
            base.OrchestratorStart();

            if (characterUnit != null) {
                characterUnit.OrchestrateStartup();
            }

        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".AICharacter.Start()");
            base.Start();
            if (characterModelGameObject == null && characterModelPrefab != null) {
                //Debug.Log(gameObject.name + ".AICharacter.Start(): Could not find character model gameobject, instantiating one");
                characterModelGameObject = Instantiate(characterModelPrefab, MyCharacterUnit.transform);
            }
            if (MyAnimatedUnit != null && MyAnimatedUnit.MyCharacterAnimator != null) {
                MyAnimatedUnit.MyCharacterAnimator.InitializeAnimator();
            } else {
                if (MyAnimatedUnit == null) {
                    //Debug.Log(gameObject.name + ".AICharacter.Start(): myanimatedunit is null");
                } else if (MyAnimatedUnit.MyCharacterAnimator == null) {
                    //Debug.Log(gameObject.name + ".AICharacter.Start() myanimatedunit.MyCharacterAnimator is null");
                }
            }
        }

        public void DespawnImmediate() {
            Debug.Log(gameObject.name + ".AICharacter.DespawnImmediate()");
            if (characterUnit != null) {
                characterUnit.Despawn(0, false, true);
            }
        }


        public void Despawn() {
            Debug.Log(gameObject.name + ".AICharacter.Despawn()");
            if (characterUnit != null) {
                characterUnit.Despawn();
            }
        }

        public void TryToDespawn() {
            Debug.Log(gameObject.name + ".AICharacter.TryToDespawn()");
            if (preventAutoDespawn == true) {
                return;
            }
            if (lootableCharacter != null) {
                // lootable character handles its own despawn logic
                return;
            }
            Despawn();
        }

    }

}