using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterUnit : InteractableOption, INamePlateUnit {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public event System.Action OnInitializeNamePlate = delegate { };
        public event Action<INamePlateUnit> NamePlateNeedsRemoval = delegate { };
        public event Action<int, int> HealthBarNeedsUpdate = delegate { };
        public event System.Action<GameObject> OnDespawn = delegate { };

        //[SerializeField]
        private BaseCharacter baseCharacter = null;
        private AnimatedUnit animatedUnit = null;


        [SerializeField]
        protected float despawnDelay = 20f;

        private NamePlateController namePlate;

        private Collider capsuleCollider;

        /// <summary>
        /// a string that represents the location of the transform in the heirarchy that we will attach the portrait camera to when this character is displayed in a unit frame
        /// </summary>
        [SerializeField]
        private string unitFrameTarget = string.Empty;

        [SerializeField]
        private Vector3 unitFrameCameraLookOffset = Vector3.zero;

        [SerializeField]
        private Vector3 unitFrameCameraPositionOffset = Vector3.zero;

        [SerializeField]
        private string playerPreviewTarget = string.Empty;

        [SerializeField]
        private Vector3 playerPreviewInitialOffset = Vector3.zero;

        [SerializeField]
        private bool suppressNamePlate = false;

        // the transform to use for the nameplate anchor
        [SerializeField]
        private Transform namePlateTransform = null;

        private Coroutine despawnCoroutine = null;

        private bool startHasRun = false;

        // keep track of mounted state
        private bool mounted = false;

        public BaseCharacter MyCharacter {
            get => baseCharacter;
            set {
                baseCharacter = value;
                InitializeNamePlate();
            }
        }

        public Faction MyFaction { get => MyCharacter.MyFaction; }
        public NamePlateController MyNamePlate { get => namePlate; set => namePlate = value; }
        public string MyDisplayName { get => (MyCharacter != null ? MyCharacter.MyCharacterName : interactionPanelTitle); }
        public string MyUnitFrameTarget { get => unitFrameTarget; }
        public string MyPlayerPreviewTarget { get => playerPreviewTarget; }
        public Vector3 MyPlayerPreviewInitialOffset { get => playerPreviewInitialOffset; }
        public Vector3 MyUnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
        public Vector3 MyUnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }
        protected float MyDespawnDelay { get => despawnDelay; set => despawnDelay = value; }
        public BaseCharacter MyBaseCharacter { get => MyCharacter; }
        public Transform MyNamePlateTransform {
            get {
                if (mounted) {
                    return baseCharacter.MyAnimatedUnit.transform;
                }
                if (namePlateTransform != null) {
                    return namePlateTransform;
                }
                return transform;
            }
        }

        public bool MyMounted { get => mounted; set => mounted = value; }
        public Collider MyCapsuleCollider { get => capsuleCollider; set => capsuleCollider = value; }

        public bool HasHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            return true;
        }

        public void EnableCollider() {
            if (capsuleCollider != null) {
                capsuleCollider.enabled = true;
            }
        }

        public void DisableCollider() {
            if (capsuleCollider != null) {
                capsuleCollider.enabled = false;
            }

        }

        public int CurrentHealth() {
            if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
                return baseCharacter.MyCharacterStats.currentHealth;
            }
            return 1;
        }

        public int MaxHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.MaxHealth()");
            if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
                //Debug.Log(gameObject.name + ".CharacterUnit.MaxHealth(): we had character stats; returning " + baseCharacter.MyCharacterStats.MyMaxHealth);
                return baseCharacter.MyCharacterStats.MyMaxHealth;
            }
            return 1;
        }


        public void HandleReviveComplete() {
            InitializeNamePlate();

            // give chance to update minimap and put character indicator back on it
            HandlePrerequisiteUpdates();
        }

        public void HandleDie(CharacterStats _characterStats) {
            HandleNamePlateNeedsRemoval(_characterStats);

            // give a chance to blank out minimap indicator
            // when the engine is upgraded to support multiplayer, this may need to be revisited.
            // some logic to still show minimap icons for dead players in your group so you can find and res them could be necessary
            HandlePrerequisiteUpdates();
        }

        public void HandleNamePlateNeedsRemoval(CharacterStats _characterStats) {
            //Debug.Log(gameObject.name + ".CharacterUnit.HandleNamePlateNeedsRemoval()");
            if (gameObject != null && _characterStats != null) {
                //Debug.Log(gameObject.name + ".CharacterUnit.HandleNamePlateNeedsRemoval(" + _characterStats + ")");
                NamePlateNeedsRemoval(this as INamePlateUnit);
            }
            //baseCharacter.MyCharacterStats.OnHealthChanged -= HealthBarNeedsUpdate;
        }

        public void HandleHealthBarNeedsUpdate(int currentHealth, int maxHealth) {
            //Debug.Log(gameObject.name + ".CharacterUnit.HandleHealthBarNeedsUpdate(" + currentHealth + ", " + maxHealth + ")");
            HealthBarNeedsUpdate(currentHealth, maxHealth);
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".CharacterUnit.Start()");
            base.Start();
            InitializeNamePlate();
            CreateEventSubscriptions();
            SetDefaultLayer();
            startHasRun = true;
        }

        protected virtual void SetDefaultLayer() {
            if (SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer != null && SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer != string.Empty) {
                int defaultLayer = LayerMask.NameToLayer(SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer);
                if (gameObject.layer != defaultLayer) {
                    gameObject.layer = defaultLayer;
                    Debug.Log(gameObject.name + ".CharacterUnit.SetDefaultLayer(): object was not set to correct layer: " + SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer + ". Setting automatically");
                }
            }
        }

        public override void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".CharacterUnit.OrchestrateStart()");
            base.OrchestratorStart();
            if (animatedUnit != null) {
                animatedUnit.OrchestratorStart();
            }
            // commented because this is handled when aicharacter calls interactable start
            /*
            if (interactable != null) {
                interactable.OrchestratorStart();
            }
            */
        }

        public override void OrchestratorFinish() {
            //Debug.Log(gameObject.name + ".CharacterUnit.OrchestratorFinish()");
            base.OrchestratorFinish();
            if (animatedUnit != null) {
                animatedUnit.OrchestratorFinish();
            }
            /*
            if (interactable != null) {
                interactable.OrchestratorFinish();
            }
            */
            InitializeNamePlate();

        }


        public override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".CharacterUnit.CreateEventSubscriptions(): CREATE EVENT SUBSCRIPTIONS");

            if (eventSubscriptionsInitialized) {
                //Debug.Log(gameObject.name + ".CharacterUnit.CreateEventSubscriptions(): ALREADY SUBSCRIBED, EXIT");
                return;
            }
            base.CreateEventSubscriptions();
            if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
                baseCharacter.MyCharacterStats.OnDie += HandleDie;
                //Debug.Log(gameObject.name + ".CharacterUnit.CreateEventSubscriptions(): subscribing to HEALTH BAR NEEDS UPDATE");
                baseCharacter.MyCharacterStats.OnHealthChanged += HandleHealthBarNeedsUpdate;
                baseCharacter.MyCharacterStats.OnReviveComplete += HandleReviveComplete;
                eventSubscriptionsInitialized = true;
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.Start(): baseCharacter is null");
            }
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("CharacterUnit.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();

            if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
                baseCharacter.MyCharacterStats.OnDie -= HandleDie;
                baseCharacter.MyCharacterStats.OnHealthChanged -= HandleHealthBarNeedsUpdate;
                baseCharacter.MyCharacterStats.OnReviveComplete -= HandleReviveComplete;
            }
            eventSubscriptionsInitialized = false;
        }

        private void OnEnable() {
            //Debug.Log(gameObject.name + ".CharacterUnit.OnEnable()");
            if (startHasRun) {
                // this is the result of re-enabling a disabled character?
                InitializeNamePlate();
            }
            //CreateEventSubscriptions();
        }

        public override void OnDisable() {
            //Debug.Log(gameObject.name + ".CharacterUnit.OnDisable()");
            base.OnDisable();
            CleanupEventSubscriptions();
            if (NamePlateManager.MyInstance != null) {
                NamePlateManager.MyInstance.RemoveNamePlate(this as INamePlateUnit);
            }
            if (despawnCoroutine != null) {
                StopCoroutine(despawnCoroutine);
            }
        }

        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences()");
            if (componentReferencesInitialized) {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): already initialized. exiting!");
                return;
            }
            base.GetComponentReferences();
            if (baseCharacter == null) {
                baseCharacter = GetComponent<BaseCharacter>();
                if (baseCharacter == null) {
                    //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): baseCharacter was null and is still null");
                } else {
                    //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): baseCharacter was null but is now initialized to: " + baseCharacter.MyCharacterName);
                }
            }

            animatedUnit = GetComponent<AnimatedUnit>();

            if (unitAudio == null) {
                Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): AUDIOSOURCE WAS NULL. ADDING ONE, BUT AN AUDIO SOURCE SHOULD BE MANUALLY ADDED.  CHECK INSPECTOR.");
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.minDistance = 1f;
                audioSource.maxDistance = 50f;
                audioSource.outputAudioMixerGroup = AudioManager.MyInstance.MyEffectsAudioSource.outputAudioMixerGroup;
            }
            // ADD SOME CODE IN THE FUTURE TO AUTO-CONFIGURE THIS AUDIO SOURCE IN CASE IT HAS NOT BEEN ADDED TO THE UNIT PREFAB

            capsuleCollider = GetComponent<Collider>();
            /*
            if (capsuleCollider != null) {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): found collider");
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): DID NOT FIND collider");
            }
            */
        }

        public void InitializeNamePlate() {
            //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate()");
            if (suppressNamePlate == true) {
                return;
            }
            if (baseCharacter != null) {
                NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(this, (namePlateTransform == null ? true : false));
                if (_namePlate != null) {
                    namePlate = _namePlate;
                }
                OnInitializeNamePlate();
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate(): Character is null or start has not been run yet. exiting.");
                return;
            }
        }

        /// <summary>
        /// The default interaction on any character is to be attacked.  Return true if the relationship is less than 0.
        /// </summary>
        /// <param name="targetCharacter"></param>
        /// <returns></returns>
        public override bool CanInteract() {
            //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract(" + targetCharacter.MyName + ")");
            /*
            if (targetCharacter == null) {
                //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract(): source is null!!");
                // we must have moused over a healthbar before the player spawned
                return false;
            }
            */
            if (Faction.RelationWith(PlayerManager.MyInstance.MyCharacter, MyBaseCharacter) <= -1 && baseCharacter.MyCharacterStats.IsAlive == true) {
                //Debug.Log(source.name + " can interact with us!");
                return true;
            }
            //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract: " + source.name + " was unable to interact with (attack) us!");
            return false;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".CharacterUnit.Interact(" + source.name + ")");
            if (CanInteract()) {
                base.Interact(source);

                //source.MyCharacter.MyCharacterCombat.Attack(baseCharacter);
                (source.MyCharacter.MyCharacterCombat as PlayerCombat).Attack(baseCharacter);
                PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
                return true;
            }
            //return true;
            return false;
        }

        public override void StopInteract() {
            //Debug.Log(gameObject.name + ".CharacterUnit.StopInteract()");
            base.StopInteract();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(Text text) {
            //Debug.Log(gameObject.name + ".CharacterUnit.SetMiniMapText()");
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.fontSize = 50;
            if (baseCharacter != null && baseCharacter.MyFaction != null) {
                text.color = Faction.GetFactionColor(PlayerManager.MyInstance.MyCharacter, baseCharacter);
            }
            return true;
        }

        public void Despawn(float despawnDelay = 0f, bool addSystemDefaultTime = true, bool forceDespawn = false) {
            //Debug.Log(gameObject.name + ".CharacterUnit.Despawn(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + ")");
            //gameObject.SetActive(false);
            // TEST ADDING A MANDATORY DELAY
            if (despawnCoroutine == null && gameObject.activeSelf == true && isActiveAndEnabled) {
                despawnCoroutine = StartCoroutine(PerformDespawnDelay(despawnDelay, addSystemDefaultTime, forceDespawn));
            }
            // we are going to send this ondespawn call now to allow another unit to respawn from a spawn node without a long wait during events that require rapid mob spawning
            OnDespawn(gameObject);
        }

        public IEnumerator PerformDespawnDelay(float despawnDelay, bool addSystemDefaultTime = true, bool forceDespawn = false) {
            //Debug.Log(gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + ")");
            // add all possible delays together
            float extraTime = 0f;
            if (addSystemDefaultTime) {
                extraTime = SystemConfigurationManager.MyInstance.MyDefaultDespawnTimer;
            }
            float totalDelay = despawnDelay + this.despawnDelay + extraTime;
            while (totalDelay > 0f) {
                totalDelay -= Time.deltaTime;
                yield return null;
            }

            if (baseCharacter.MyCharacterStats.IsAlive == false || forceDespawn == true) {
                //Debug.Log(gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + "): despawning");
                // this character could have been ressed while waiting to despawn.  don't let it despawn if that happened unless forceDesapwn is true (such as at the end of a patrol)
                Destroy(gameObject);
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + "): unit is alive!! NOT DESPAWNING");
            }
        }

        public override string GetDescription() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetDescription()");
            if (interactionPanelTitle == null || interactionPanelTitle == string.Empty) {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetDescription(): returning " + MyDisplayName);
                return MyDisplayName;
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetDescription(): returning " + interactionPanelTitle);
                return interactionPanelTitle;
            }
        }

        // CHARACTER UNIT ALIVE IS ALWAYS VALID AND CURRENT TO ALLOW ATTACKS
        public override int GetValidOptionCount() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetValidOptionCount()");
            return (MyCharacter.MyCharacterStats.IsAlive == true ? 1 : 0);
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

    }

}