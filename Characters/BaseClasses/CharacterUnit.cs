using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

        [SerializeField]
        protected float despawnDelay = 20f;

        private NamePlateController namePlate;

        private Collider capsuleCollider;

        private float hitBoxSize = 1.5f;

        [Header("UNIT FRAME")]

        [Tooltip("a string that represents the name of the transform in the heirarchy that we will attach the portrait camera to when this character is displayed in a unit frame")]
        [SerializeField]
        private string unitFrameTarget = string.Empty;

        [SerializeField]
        private Vector3 unitFrameCameraLookOffset = Vector3.zero;

        [SerializeField]
        private Vector3 unitFrameCameraPositionOffset = Vector3.zero;

        [Header("PLAYER PREVIEW")]

        [Tooltip("a string that represents the name of the transform in the heirarchy that we will attach the camera to when this character is displayed in a player preview type of window")]
        [SerializeField]
        private string playerPreviewTarget = string.Empty;

        [SerializeField]
        private Vector3 unitPreviewCameraLookOffset = new Vector3(0f, 1f, 0f);

        [SerializeField]
        private Vector3 unitPreviewCameraPositionOffset = new Vector3(0f, 1f, 1f);

        [Header("NAMEPLATE")]

        [Tooltip("If true, the nameplate is not shown above this unit.")]
        [SerializeField]
        private bool suppressNamePlate = false;

        [Tooltip("If true, the nameplate will not show the faction of the unit.")]
        [SerializeField]
        private bool suppressFaction = false;

        // the transform to use for the nameplate anchor
        [Tooltip("Drag an object in the heirarchy here and the nameplate will show at its transform location")]
        [SerializeField]
        private Transform namePlateTransform = null;

        private Coroutine despawnCoroutine = null;

        private bool startHasRun = false;

        private BaseCharacter baseCharacter = null;
        private AnimatedUnit animatedUnit = null;

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
        public string MyDisplayName { get => (MyCharacter != null ? MyCharacter.CharacterName : interactionPanelTitle); }
        public string Title { get => (MyCharacter != null ? MyCharacter.Title : string.Empty); }
        public string MyUnitFrameTarget { get => unitFrameTarget; }
        public string MyPlayerPreviewTarget { get => playerPreviewTarget; }
        public Vector3 MyUnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
        public Vector3 MyUnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }
        protected float MyDespawnDelay { get => despawnDelay; set => despawnDelay = value; }
        public BaseCharacter MyBaseCharacter { get => MyCharacter; }
        public Transform MyNamePlateTransform {
            get {
                if (mounted) {
                    return baseCharacter.AnimatedUnit.transform;
                }
                if (namePlateTransform != null) {
                    return namePlateTransform;
                }
                return transform;
            }
        }

        public bool MyMounted { get => mounted; set => mounted = value; }
        public Collider MyCapsuleCollider { get => capsuleCollider; set => capsuleCollider = value; }
        public float HitBoxSize { get => hitBoxSize; set => hitBoxSize = value; }
        public Vector3 UnitPreviewCameraLookOffset { get => unitPreviewCameraLookOffset; set => unitPreviewCameraLookOffset = value; }
        public Vector3 UnitPreviewCameraPositionOffset { get => unitPreviewCameraPositionOffset; set => unitPreviewCameraPositionOffset = value; }
        public bool SuppressFaction { get => suppressFaction; set => suppressFaction = value; }

        public bool HasHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            return true;
        }

        public void SetUseRootMotion(bool useRootMotion) {
            if (MyCharacter != null && MyCharacter.AnimatedUnit != null && MyCharacter.AnimatedUnit.MyCharacterMotor != null) {
                MyCharacter.AnimatedUnit.MyCharacterMotor.MyUseRootMotion = false;
            }
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

        public virtual void CancelMountEffects() {
            if (MyMounted == true) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): canCast and character is mounted");

                foreach (StatusEffectNode statusEffectNode in baseCharacter.CharacterStats.MyStatusEffects.Values) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): looping through status effects");
                    if (statusEffectNode.MyStatusEffect is MountEffect) {
                        //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): looping through status effects: found a mount effect");
                        statusEffectNode.CancelStatusEffect();
                        break;
                    }
                }
            }
        }



        public int CurrentHealth() {
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                return baseCharacter.CharacterStats.currentHealth;
            }
            return 1;
        }

        public int MaxHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.MaxHealth()");
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                //Debug.Log(gameObject.name + ".CharacterUnit.MaxHealth(): we had character stats; returning " + baseCharacter.MyCharacterStats.MyMaxHealth);
                return baseCharacter.CharacterStats.MyMaxHealth;
            }
            return 1;
        }


        public void HandleReviveComplete() {
            InitializeNamePlate();
            animatedUnit.FreezeRotation();

            // give chance to update minimap and put character indicator back on it
            HandlePrerequisiteUpdates();
        }

        public void HandleDie(CharacterStats _characterStats) {
            HandleNamePlateNeedsRemoval(_characterStats);
            HandleFreezePosition();
            // give a chance to blank out minimap indicator
            // when the engine is upgraded to support multiplayer, this may need to be revisited.
            // some logic to still show minimap icons for dead players in your group so you can find and res them could be necessary
            HandlePrerequisiteUpdates();
        }

        public void HandleFreezePosition() {
            animatedUnit.FreezePositionXZ();
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

        public static bool IsInLayerMask(int layer, LayerMask layermask) {
            return layermask == (layermask | (1 << layer));
        }

        protected virtual void SetDefaultLayer() {
            if (SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer != null && SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer != string.Empty) {
                int defaultLayer = LayerMask.NameToLayer(SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer);
                int finalmask = (1 << defaultLayer) | (1 << UnitPreviewManager.MyInstance.PreviewLayer) | (1 << PetPreviewManager.MyInstance.PreviewLayer);
                if (!IsInLayerMask(gameObject.layer, finalmask)) {
                    //if (gameObject.layer != defaultLayer) {
                    gameObject.layer = defaultLayer;
                    Debug.Log(gameObject.name + ".CharacterUnit.SetDefaultLayer(): object was not set to correct layer: " + SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer + ". Setting automatically");
                }
            }
        }
        

        public override void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".CharacterUnit.OrchestratorStart()");
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
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                baseCharacter.CharacterStats.OnDie += HandleDie;
                //Debug.Log(gameObject.name + ".CharacterUnit.CreateEventSubscriptions(): subscribing to HEALTH BAR NEEDS UPDATE");
                baseCharacter.CharacterStats.OnHealthChanged += HandleHealthBarNeedsUpdate;
                baseCharacter.CharacterStats.OnReviveComplete += HandleReviveComplete;
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

            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                baseCharacter.CharacterStats.OnDie -= HandleDie;
                baseCharacter.CharacterStats.OnHealthChanged -= HandleHealthBarNeedsUpdate;
                baseCharacter.CharacterStats.OnReviveComplete -= HandleReviveComplete;
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
            if (capsuleCollider != null) {
                hitBoxSize = capsuleCollider.bounds.extents.y * 1.5f;
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): found collider");
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): DID NOT FIND collider");
            }
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
            if (Faction.RelationWith(PlayerManager.MyInstance.MyCharacter, MyBaseCharacter) <= -1 && baseCharacter.CharacterStats.IsAlive == true) {
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
                (source.MyCharacter.CharacterCombat as PlayerCombat).Attack(baseCharacter);
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

        public override bool SetMiniMapText(TextMeshProUGUI text) {
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
                yield return null;
                totalDelay -= Time.deltaTime;
            }

            if (baseCharacter.CharacterStats.IsAlive == false || forceDespawn == true) {
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
            return (MyCharacter.CharacterStats.IsAlive == true ? 1 : 0);
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

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


    }

}