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
            characterController = GetComponent<AIController>() as ICharacterController;
            characterStats = GetComponent<AIStats>() as ICharacterStats;
            characterAbilityManager = GetComponent<AIAbilityManager>() as ICharacterAbilityManager;
            characterCombat = GetComponent<AICombat>() as ICharacterCombat;
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".AICharacter.Start()");
            base.Start();
            if (characterModelGameObject == null && characterModelPrefab != null) {
                Debug.Log(gameObject.name + ".AICharacter.Start(): Could not find character model gameobject, instantiating one");
                characterModelGameObject = Instantiate(characterModelPrefab, MyCharacterUnit.transform);
            }
            if (MyCharacterUnit.MyCharacterAnimator != null) {
                MyCharacterUnit.MyCharacterAnimator.InitializeAnimator();
            } else {
                //Debug.Log(gameObject.name + ".AICharacter.Start(): Could not find character animator");
            }
        }

    }

}