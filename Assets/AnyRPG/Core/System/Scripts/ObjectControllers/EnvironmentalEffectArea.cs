using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class EnvironmentalEffectArea : AutoConfiguredMonoBehaviour, IAbilityCaster {

        [Tooltip("Every x seconds, the effect will be applied to everyone within the effect radius")]
        [SerializeField]
        private float tickRate = 1f;

        [Tooltip("The name of the ability effect to cast on valid targets every tick")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private List<string> abilityEffectNames = new List<string>();

        // a reference to the ability effect to apply to targets on tick
        private List<AbilityEffect> abilityEffects = new List<AbilityEffect>();

        // a counter to keep track of the amount of time passed since the last tick
        private float elapsedTime = 0f;

        private BoxCollider boxCollider = null;

        private AbilityManager abilityManager = null;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private NetworkManagerServer networkManagerServer = null;

        public IAbilityManager AbilityManager { get => abilityManager; }
        public MonoBehaviour MonoBehaviour { get => this; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.Configure() scene: {gameObject.scene.name} default physics scene: {(gameObject.scene.GetPhysicsScene() == Physics.defaultPhysicsScene)}");
            base.Configure(systemGameManager);

            GetComponentReferences();
            SetupScriptableObjects();
            abilityManager = new AbilityManager(this, systemGameManager);
        }

        public override void SetGameManagerReferences() {
            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.SetGameManagerReferences()");

            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public void GetComponentReferences() {
            boxCollider = GetComponent<BoxCollider>();
        }


        private void FixedUpdate() {
            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.FixedUpdate()");

            if (configureCount == 0) {
                return;
            }

            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false) {
                // ticks should only happen on the server
                return;
            }

            elapsedTime += Time.fixedDeltaTime;
            if (elapsedTime > tickRate) {
                //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.FixedUpdate()");
                elapsedTime -= tickRate;
                PerformAbilityEffects();
            }
        }

        private void PerformAbilityEffects() {
            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.PerformAbilityEffects()");

            List<AOETargetNode> validTargets = GetValidTargets();
            foreach (AOETargetNode validTarget in validTargets) {
                foreach (AbilityEffect abilityEffect in abilityEffects) {
                    abilityEffect.AbilityEffectProperties.Cast(this, validTarget.targetGameObject, null, new AbilityEffectContext(abilityEffect.AbilityEffectProperties));
                }
            }
        }

        protected virtual List<AOETargetNode> GetValidTargets() {
            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.GetValidTargets() scene: {gameObject.scene.name} default physics scene: {(gameObject.scene.GetPhysicsScene() == Physics.defaultPhysicsScene)}");

            Vector3 aoeSpawnCenter = transform.position;

            Collider[] colliders = new Collider[200];
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            int validMask = (playerMask | characterMask);

            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.GetValidTargets(): using aoeSpawnCenter: {aoeSpawnCenter} extents: {boxCollider.bounds.extents}");
            int hitCount = gameObject.scene.GetPhysicsScene().OverlapBox(aoeSpawnCenter, boxCollider.bounds.extents, colliders, Quaternion.identity, validMask, QueryTriggerInteraction.Collide);
            //int hitCount = gameObject.scene.GetPhysicsScene().OverlapBox(Vector3.zero, new Vector3(200, 200, 200), colliders, Quaternion.identity, Physics.AllLayers, QueryTriggerInteraction.Collide);

            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.GetValidTargets(): hitCount: {hitCount}");

            List<AOETargetNode> validTargets = new List<AOETargetNode>();
            foreach (Collider collider in colliders) {
                if (collider == null) {
                    continue;
                }
                //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.GetValidTargets() hit: " + collider.gameObject.name + "; layer: " + collider.gameObject.layer);

                bool canAdd = true;
                Interactable interactable = collider.gameObject.GetComponent<Interactable>();
                if (interactable == null) {
                    canAdd = false;
                } else {
                    if (CharacterUnit.GetCharacterUnit(interactable) == null) {
                        canAdd = false;
                    }
                }
                if (canAdd) {
                    AOETargetNode validTargetNode = new AOETargetNode();
                    validTargetNode.targetGameObject = interactable;
                    validTargets.Add(validTargetNode);
                }
            }
            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.GetValidTargets(). Valid targets count: " + validTargets.Count);
            return validTargets;
        }

        private void SetupScriptableObjects() {
            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.SetupScriptableObjects()");
            if (systemGameManager == null) {
                Debug.LogError(gameObject.name + ": SystemAbilityEffectManager not found.  Is the GameManager in the scene?");
                return;
            }

            if (abilityEffectNames != null) {
                foreach (string abilityEffectName in abilityEffectNames) {
                    if (abilityEffectName != string.Empty) {
                        AbilityEffect tmpAbilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                        if (tmpAbilityEffect != null) {
                            abilityEffects.Add(tmpAbilityEffect);
                        } else {
                            Debug.LogError(gameObject.name + ".EnvironmentalEffectArea.SetupScriptableObjects(): Could not find ability effect " + abilityEffectName + " while initializing " + gameObject.name + ". Check inspector.");
                        }
                    } else {
                        Debug.LogError(gameObject.name + ".EnvironmentalEffectArea.SetupScriptableObjects(): Ability Effect name was empty while initializing " + gameObject.name + ". Check inspector.");
                    }
                }
            }
        }

    }

}
