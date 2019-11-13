using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterUnit : InteractableOption, ICharacterUnit, INamePlateUnit {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public event System.Action OnInitializeNamePlate = delegate { };
        public event Action<INamePlateUnit> NamePlateNeedsRemoval = delegate { };
        public event Action<int, int> HealthBarNeedsUpdate = delegate { };
        public event System.Action<GameObject> OnDespawn = delegate { };

        //[SerializeField]
        private BaseCharacter baseCharacter = null;

        [SerializeField]
        protected float despawnDelay = 20f;

        private NamePlateController namePlate;

        private NavMeshAgent agent;

        protected Rigidbody rigidBody;

        private CharacterMotor characterMotor;

        private CharacterAnimator characterAnimator;

        /// <summary>
        /// a string that represents the location of the transform in the heirarchy that we will attach the portrait camera to when this character is displayed in a unit frame
        /// </summary>
        [SerializeField]
        private string unitFrameTarget = string.Empty;

        [SerializeField]
        private Vector3 unitFrameCameraLookOffset;

        [SerializeField]
        private Vector3 unitFrameCameraPositionOffset;

        [SerializeField]
        private string playerPreviewTarget = string.Empty;

        [SerializeField]
        private Vector3 playerPreviewInitialOffset;

        private Coroutine despawnCoroutine;

        public BaseCharacter MyCharacter {
            get => baseCharacter;
            set {
                baseCharacter = value;
                InitializeNamePlate();
            }
        }

        public string MyFactionName { get => MyCharacter.MyFactionName; }
        public NamePlateController MyNamePlate { get => namePlate; set => namePlate = value; }
        public NavMeshAgent MyAgent { get => agent; set => agent = value; }
        public Rigidbody MyRigidBody { get => rigidBody; set => rigidBody = value; }
        public CharacterMotor MyCharacterMotor { get => characterMotor; set => characterMotor = value; }
        public CharacterAnimator MyCharacterAnimator { get => characterAnimator; set => characterAnimator = value; }
        public string MyDisplayName { get => (MyCharacter != null ? MyCharacter.MyCharacterName : interactionPanelTitle); }
        public string MyUnitFrameTarget { get => unitFrameTarget; }
        public string MyPlayerPreviewTarget { get => playerPreviewTarget; }
        public Vector3 MyPlayerPreviewInitialOffset { get => playerPreviewInitialOffset; }
        public Vector3 MyUnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
        public Vector3 MyUnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }
        protected float MyDespawnDelay { get => despawnDelay; set => despawnDelay = value; }
        public BaseCharacter MyBaseCharacter { get => MyCharacter; }

        public bool HasHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            return true;
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

        protected override void Awake() {
            //Debug.Log(gameObject.name + ".CharacterUnit.Awake() about to get references to all local components");
            base.Awake();
            // already handled in base.awake
            //GetComponentReferences();
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ": running Start()");
            base.Start();
            InitializeNamePlate();
            CreateEventSubscriptions();
            if (characterAnimator != null) {
                characterAnimator.OrchestratorStart();
            }
        }

        public void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized || !startHasRun) {
                return;
            }
            if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
                baseCharacter.MyCharacterStats.OnDie += HandleDie;
                baseCharacter.MyCharacterStats.OnHealthChanged += HandleHealthBarNeedsUpdate;
                baseCharacter.MyCharacterStats.OnReviveComplete += HandleReviveComplete;
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.Start(): baseCharacter is null");
            }
            eventSubscriptionsInitialized = true;
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
            InitializeNamePlate();
            CreateEventSubscriptions();
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
            agent = GetComponent<NavMeshAgent>();
            rigidBody = GetComponent<Rigidbody>();
            characterMotor = GetComponent<CharacterMotor>();
            characterAnimator = GetComponent<CharacterAnimator>();
            if (baseCharacter == null) {
                baseCharacter = GetComponent<BaseCharacter>();
                if (baseCharacter == null) {
                    //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): baseCharacter was null and is still null");
                } else {
                    //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): baseCharacter was null but is now initialized to: " + baseCharacter.MyCharacterName);
                }
            }
        }

        public void InitializeNamePlate() {
            //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate()");
            if (baseCharacter != null && startHasRun) {
                NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(this);
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
        public override bool CanInteract(CharacterUnit targetCharacter) {
            //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract(" + targetCharacter.MyName + ")");
            if (targetCharacter == null) {
                //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract(): source is null!!");
                // we must have moused over a healthbar before the player spawned
                return false;
            }
            if (Faction.RelationWith(targetCharacter.MyCharacter, MyBaseCharacter) <= -1 && baseCharacter.MyCharacterStats.IsAlive == true) {
                //Debug.Log(source.name + " can interact with us!");
                return true;
            }
            //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract: " + source.name + " was unable to interact with (attack) us!");
            return false;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".CharacterUnit.Interact(" + source.name + ")");
            if (CanInteract(source)) {
                source.MyCharacter.MyCharacterCombat.Attack(baseCharacter);
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
            if (baseCharacter != null && baseCharacter.MyFactionName != null && baseCharacter.MyFactionName != string.Empty) {
                text.color = Faction.GetFactionColor(PlayerManager.MyInstance.MyCharacter, baseCharacter);
            }
            return true;
        }

        public void Despawn(float despawnDelay = 0f, bool addSystemDefaultTime = true, bool forceDespawn = false) {
            //Debug.Log(gameObject.name + ".CharacterUnit.Despawn(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + ")");
            //gameObject.SetActive(false);
            // TEST ADDING A MANDATORY DELAY
            if (despawnCoroutine == null) {
                StartCoroutine(PerformDespawnDelay(despawnDelay, addSystemDefaultTime, forceDespawn));
            }
            // we are going to send this ondespawn call now to allow another unit to respawn from a spawn node without a long wait during events that require rapid mob spawning
            OnDespawn(gameObject);
        }

        public IEnumerator PerformDespawnDelay(float despawnDelay, bool addSystemDefaultTime = true, bool forceDespawn = false) {
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
                // this character could have been ressed while waiting to despawn.  don't let it despawn if that happened unless forceDesapwn is true (such as at the end of a patrol)
                Destroy(gameObject);
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