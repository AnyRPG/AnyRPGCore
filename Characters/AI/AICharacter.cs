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

        public GameObject MyCharacterModelPrefab { get => characterModelPrefab; set => characterModelPrefab = value; }

        public GameObject MyCharacterModelGameObject { get => characterModelGameObject; set => characterModelGameObject = value; }

        protected override void Awake() {
            base.Awake();
            OrchestratorStart();

        }

        public override void GetComponentReferences() {
            base.GetComponentReferences();
            characterController = GetComponent<AIController>();
            characterStats = GetComponent<AIStats>();
            characterCombat = GetComponent<AICombat>();
            animatedUnit = GetComponent<AnimatedUnit>();
            if (animatedUnit == null) {
                animatedUnit = gameObject.AddComponent<AnimatedUnit>();
            }
        }

        public override void OrchestratorStart() {
            if (characterUnit != null) {
                characterUnit.OrchestrateStartup();
            }

            base.OrchestratorStart();

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
                    Debug.Log(gameObject.name + ".AICharacter.Start(): myanimatedunit is null");
                } else if (MyAnimatedUnit.MyCharacterAnimator == null) {
                    Debug.Log(gameObject.name + ".AICharacter.Start() myanimatedunit.MyCharacterAnimator is null");
                }
            }
        }

    }

}