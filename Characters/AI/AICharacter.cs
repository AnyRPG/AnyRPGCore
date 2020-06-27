using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AICharacter : BaseCharacter {

        [SerializeField]
        private GameObject characterModelGameObject = null;

        [SerializeField]
        private LootableCharacter lootableCharacter = null;

        [SerializeField]
        private bool preventAutoDespawn = false;

        private bool animationEnabled = false;

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
            if (characterStats == null) {
                //Debug.Log(gameObject.name + ".AICharacter.GetComponentReferences(): characterStats is null!");
            } else {
                //Debug.Log(gameObject.name + ".AICharacter.GetComponentReferences(): got reference to characterStats!");
            }
            characterCombat = GetComponent<AICombat>();
            lootableCharacter = GetComponent<LootableCharacter>();
            animatedUnit = GetComponent<AnimatedUnit>();
            /*
             * not necessary ? will mess up stationary targets
            if (animatedUnit == null) {
                animatedUnit = gameObject.AddComponent<AnimatedUnit>();
            }
            */
            interactable = GetComponent<Interactable>();
        }

        public override void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".AICharacter.OrchestratorStart()");

            // if this is run, or the getcomponents part anyway before the below block, then character unit will be set properly
            base.OrchestratorStart();

            if (interactable != null) {
                interactable.OrchestratorStart();
            }

            // now that interactable has initialized character unit, and therefore animatedunit we can turn on the navmeshagent
            /*
            if (previewCharacter == false) {
                animatedUnit.MyAgent.enabled = true;
            }
            */

            // commented because interactable will call characterUnit because it is an interactableOption
            /*
            if (characterUnit != null) {
                characterUnit.OrchestratorStart();
            }
            */
        }

        public override void OrchestratorFinish() {
            //Debug.Log(gameObject.name + ".AICharacter.OrchestratorStart()");

            // if this is run, or the getcomponents part anyway before the below block, then character unit will be set properly
            base.OrchestratorFinish();

            if (interactable != null) {
                interactable.OrchestratorFinish();
            }
        }

        public void EnableAnimation() {
            if (animationEnabled == true) {
                return;
            }
            /*
             * saved code for maybe using with unit profile in the future?
            if (characterModelGameObject == null && characterModelPrefab != null) {
                //Debug.Log(gameObject.name + ".AICharacter.Start(): Could not find character model gameobject, instantiating one");
                characterModelGameObject = Instantiate(characterModelPrefab, CharacterUnit.transform);
            }
            */
            if (previewCharacter == false && AnimatedUnit != null) {
                AnimatedUnit.EnableAgent();
            }
            if (AnimatedUnit != null && AnimatedUnit.MyCharacterAnimator != null) {
                AnimatedUnit.MyCharacterAnimator.InitializeAnimator();
            } else {
                if (AnimatedUnit == null) {
                    //Debug.Log(gameObject.name + ".AICharacter.Start(): myanimatedunit is null");
                } else {
                    if (AnimatedUnit.MyCharacterAnimator == null) {
                        //Debug.Log(gameObject.name + ".AICharacter.Start() myanimatedunit.MyCharacterAnimator is null");
                    }
                }
            }
            animationEnabled = true;
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".AICharacter.Start()");
            base.Start();

            EnableAnimation();
        }

        public void DespawnImmediate() {
            //Debug.Log(gameObject.name + ".AICharacter.DespawnImmediate()");
            if (characterUnit != null) {
                characterUnit.Despawn(0, false, true);
            }
        }


        public void Despawn() {
            //Debug.Log(gameObject.name + ".AICharacter.Despawn()");
            if (characterUnit != null) {
                characterUnit.Despawn();
            }
        }

        public void TryToDespawn() {
            //Debug.Log(gameObject.name + ".AICharacter.TryToDespawn()");
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