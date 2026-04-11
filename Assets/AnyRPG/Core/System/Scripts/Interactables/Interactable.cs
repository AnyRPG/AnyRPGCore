using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    public class Interactable : AutoConfiguredMonoBehaviour, IPersistentObjectOwner, IDescribable {

        public event System.Action OnPrerequisiteUpdates = delegate { };
        public event System.Action OnInteractableDisable = delegate { };
        public event System.Action OnInteractableResetSettings = delegate { };

        // this field does not do anything, but is needed to satisfy the IDescribable interface
        protected Sprite interactableIcon = null;

        [Header("Mouse Over")]

        [Tooltip("Show a tooltip when the mouse is over the object.")]
        [SerializeField]
        protected bool showTooltip = true;

        [Tooltip("This value will show in the mouseover tooltip.")]
        [SerializeField]
        protected string interactableName = string.Empty;

        [Tooltip("Should the object glow when the mouse is over it or its nameplate.")]
        [SerializeField]
        protected bool glowOnMouseOver = true;

        [Tooltip("The color of light to emit when glowing.")]
        [SerializeField]
        protected Color glowColor = Color.yellow;

        [Header("Interaction Options")]

        [Tooltip("Set this value to override the default 'Interact' option for the gamepad interaction tooltip.")]
        [SerializeField]
        protected string interactionTooltipText = string.Empty;

        [Tooltip("Set this value to prevent direct interaction from the player.  This can be useful for interactables that only need to be activated with control switches.")]
        [SerializeField]
        protected bool notInteractable = false;

        [Tooltip("Set this to true to allow triggering interaction with anything that has a collider, not just players.")]
        [SerializeField]
        protected bool interactWithAny = false;

        [Tooltip("Set this to true to cause the interaction to trigger on when something exits the collider.")]
        [SerializeField]
        protected bool interactOnExit = false;

        [Tooltip("If true, interaction is triggered by a collider, and not by clicking with the mouse.")]
        [SerializeField]
        protected bool isTrigger = false;

        [Tooltip("Set this to true to automatically activate the first interactable instead of opening the interaction window and presenting the player with interaction options.")]
        [SerializeField]
        protected bool suppressInteractionWindow = false;


        [Tooltip("If true, the InteractionRange collider will be set to the new extents in the Interaction Max Range field.")]
        [SerializeField]
        private bool overrideInteractionColliderSize = false;

        [Tooltip("For everything except character unit interactions, the interactor must be within this range of this objects collider. This does not apply to interactions triggered by switches.")]
        [SerializeField]
        private float interactionMaxRange = 2f;

        [Tooltip("The prefered locations that units wanting to interact will move to, based on NavMeshPath completeness.")]
        [FormerlySerializedAs("interactLocations")]
        [SerializeField]
        protected List<GameObject> interactionPoints = new List<GameObject>();

        [Header("Controller References")]

        [Tooltip("Reference to local component controller prefab with nameplate target, speakers, etc.")]
        [SerializeField]
        protected ComponentController componentController = null;

        [Header("Persistence")]

        [SerializeField]
        protected PersistentObjectComponent persistentObjectComponent = new PersistentObjectComponent();

        [Tooltip("Set this to true to save and load interactable data for this interactable.  This is only necessary if the interactable has interactable options that need to save data, such as loot or a door that can be opened/closed.")]
        [SerializeField]
        protected bool persistInteractableData = false;

        /*
        [Tooltip("Reference to local component controller prefab with nameplate target, speakers, etc.")]
        [SerializeField]
        protected UnitComponentController unitComponentController = null;
        */

        protected Dictionary<int, InteractableOptionComponent> interactables = new Dictionary<int, InteractableOptionComponent>();
        protected int interactableOptionCount = 0;

        // state
        protected InteractableSaveData _interactableSaveData = null;
        protected bool isInteracting = false;
        protected bool miniMapIndicatorReady = false;
        protected bool isMouseOverUnit = false;
        protected bool isMouseOverNameplate = false;
        protected bool isTargeted = false;

        protected bool isInitialized = false;
        protected bool startHasRun = false;
        protected bool componentReferencesInitialized = false;
        protected bool eventSubscriptionsInitialized = false;

        protected Dictionary<GameObject, UnitController> inRangeUnitControllers = new Dictionary<GameObject, UnitController>();

        // attached components
        protected Collider myCollider;
        //protected MiniMapIndicatorController miniMapIndicator = null;
        //protected MainMapIndicatorController mainMapIndicator = null;
        protected UUID uuid = null;


        // created components
        protected CharacterUnit characterUnit = null;
        protected DialogController dialogController = null;
        protected OutlineController outlineController = null;
        protected ObjectMaterialController objectMaterialController = null;
        protected InteractableEventController interactableEventController = new InteractableEventController();


        // game manager references
        protected PlayerManagerClient playerManagerClient = null;
        protected UIManager uIManager = null;
        protected NamePlateManager namePlateManager = null;
        protected MiniMapManager miniMapManager = null;
        protected MainMapManager mainMapManager = null;
        protected InteractionManagerClient interactionManagerClient = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected SystemItemManager systemItemManager = null;
        protected LevelManagerServer levelManagerServer = null;

        // properties
        public bool IsInteracting { get => isInteracting; }
        public Dictionary<int, InteractableOptionComponent> Interactables { get => interactables; set => interactables = value; }

        public Sprite Icon { get => interactableIcon; }

        //public UnitComponentController UnitComponentController { get => unitComponentController; set => unitComponentController = value; }

        public string ResourceName { get => DisplayName; }
        public virtual string DisplayName {
            get {
                if (interactableName != null && interactableName != string.Empty) {
                    return interactableName;
                }
                if (characterUnit != null) {
                    return characterUnit.UnitController.BaseCharacter.CharacterName;
                }
                return gameObject.name;
            }
            set {
                interactableName = value;
            }
        }

        // not used
        public virtual string Description {
            get => string.Empty;
        }

        public bool NotInteractable { get => notInteractable; set => notInteractable = value; }
        public Collider Collider { get => myCollider; }
        public virtual float InteractionMaxRange { get => interactionMaxRange; set => interactionMaxRange = value; }
        public bool IsTrigger { get => isTrigger; set => isTrigger = value; }
        public CharacterUnit CharacterUnit { get => characterUnit; set => characterUnit = value; }
        public DialogController DialogController { get => dialogController; }
        public InteractableEventController InteractableEventController { get => interactableEventController; }
        public virtual bool CombatOnly { get => false; }
        public virtual bool NonCombatOptionsAvailable { get => true; }

        /// <summary>
        /// this is the gameobject that should be targeted by abilities
        /// </summary>
        public virtual GameObject InteractableGameObject {
            get {
                return gameObject;
            }
        }

        /// <summary>
        /// this is the interactable that should be targeted by abilities
        /// </summary>
        public virtual Interactable InteractableTarget {
            get {
                return this;
            }
        }

        public virtual Interactable CharacterTarget {
            get {
                return this;
            }
        }

        public virtual Interactable PhysicalTarget {
            get {
                return this;
            }
        }

        public bool IsMouseOverUnit { get => isMouseOverUnit; set => isMouseOverUnit = value; }
        public bool IsMouseOverNameplate { get => isMouseOverNameplate; set => isMouseOverNameplate = value; }
        public string InteractionTooltipText { get => interactionTooltipText; set => interactionTooltipText = value; }
        public OutlineController OutlineController { get => outlineController; }
        public ObjectMaterialController ObjectMaterialController { get => objectMaterialController; }
        public bool SuppressInteractionWindow { get => suppressInteractionWindow; set => suppressInteractionWindow = value; }
        public bool IsTargeted { get => isTargeted; }
        public bool IsInitialized { get => isInitialized; }
        public List<GameObject> InteractionPoints { get => interactionPoints; set => interactionPoints = value; }
        public virtual bool OverrideInteractionColliderSize { get => overrideInteractionColliderSize; }
        public virtual IUUID UUID {
            get {
                return uuid;
            }
        }
        public PersistentObjectComponent PersistentObjectComponent { get => persistentObjectComponent; set => persistentObjectComponent = value; }


        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"{gameObject.name}.Interactable.Configure() instanceId: {GetInstanceID()}");

            base.Configure(systemGameManager);

            persistentObjectComponent.Setup(this, systemGameManager);
            GetComponentReferences();
            //CreateEventSubscriptions();
            ConfigureComponents();
            CreateComponents();
            LateConfigure();
        }

        protected override void PostConfigure() {
            //Debug.Log($"{gameObject.name}.Interactable.PostConfigure()");

            base.PostConfigure();
            // this is only called if by AutoConfigure() so we know this is a static scene object, and need to
            // register to the level manager
            levelManagerServer.RegisterInteractable(this);
            Init();
        }

        public virtual void Init() {
            //Debug.Log($"{gameObject.name}.Interactable.Init()");

            if (isInitialized == true) {
                Debug.LogWarning($"{gameObject.name}.Interactable.Init(): already initialized.  Returning.");
                return;
            }
            ProcessInit();

            // moved here from CreateEventSubscriptions.  Init should have time to occur before processing this
            if (playerManagerClient.PlayerUnitSpawned) {
                //Debug.Log($"{gameObject.name}.Interactable.CreateEventSubscriptions(): Player Unit is spawned.  Handling immediate spawn!");
                ProcessPlayerUnitSpawn(playerManagerClient.UnitController);
            } else {
                //Debug.Log($"{gameObject.name}.Interactable.CreateEventSubscriptions(): Player Unit is not spawned. Added Handle Spawn listener");
            }
            startHasRun = true;
            isInitialized = true;

            //Debug.Log($"{gameObject.name}.Interactable.Init() complete");
            PostInit();
        }

        protected virtual void PostInit() {
            //Debug.Log($"{gameObject.name}.Interactable.PostInit()");

            if (systemGameManager.GameMode != GameMode.Local) {
                return;
            }
            if (persistentObjectComponent.SaveOnGameSave == false && persistentObjectComponent.SaveOnLevelUnload == false) {
                return;
            }
            if (persistentObjectComponent.PersistObjectPosition == false && persistInteractableData == false) {
                return;
            }
            persistentObjectComponent.LoadPersistentState();
        }

        protected virtual void LateConfigure() {
            //DisableInteraction();
        }

        public virtual void ProcessPlayerUnitSpawn(UnitController sourceUnitController) {
            UpdateOnPlayerUnitSpawn(sourceUnitController);
        }

        protected virtual void ConfigureComponents() {
            //Debug.Log($"{gameObject.name}.Interactable.ConfigureComponents() instanceId: {GetInstanceID()}");

            if (componentController != null) {
                componentController.Configure(systemGameManager);
                componentController.SetInteractable(this);
            }
        }

        protected virtual void CreateComponents() {
            dialogController = new DialogController(this, systemGameManager);
            outlineController = new OutlineController(this, systemGameManager);
            interactableEventController.SetInteractable(this, systemGameManager);
            CreateMaterialController();
        }

        protected virtual void CreateMaterialController() {
            //Debug.Log($"{gameObject.name}.Interactable.CreateMaterialController()");

            objectMaterialController = new ObjectMaterialController(this, systemGameManager);
            objectMaterialController.PopulateOriginalMaterials();
        }

        public override void SetGameManagerReferences() {
            //Debug.Log($"{gameObject.name}.Interactable.SetGameManagerReferences()");

            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
            namePlateManager = uIManager.NamePlateManager;
            miniMapManager = uIManager.MiniMapManager;
            mainMapManager = uIManager.MainMapManager;
            interactionManagerClient = systemGameManager.InteractionManagerClient;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            playerManagerClient = systemGameManager.PlayerManagerClient;
            systemItemManager = systemGameManager.SystemItemManager;
            levelManagerServer = systemGameManager.LevelManagerServer;
        }

        public virtual void GetComponentReferences() {
            //Debug.Log($"{gameObject.name}.Interactable.InitializeComponents()");

            if (componentReferencesInitialized == true) {
                return;
            }
            componentReferencesInitialized = true;

            myCollider = GetComponent<Collider>();

            // get monobehavior interactables
            InteractableOption[] interactableOptionMonoList = GetComponents<InteractableOption>();
            foreach (InteractableOption interactableOption in interactableOptionMonoList) {
                if (interactableOption.InteractableOptionProps != null) {
                    interactableOption.SetupScriptableObjects(systemGameManager);
                    AddInteractableOption(interactableOption.InteractableOptionProps.GetInteractableOption(this, interactableOption));
                }
            }
            uuid = GetComponent<UUID>();
        }

        public virtual void CleanupEverything() {
            //Debug.Log($"{gameObject.name}.Interactable.CleanupEverything()");
            if (dialogController != null) {
                dialogController.Cleanup();
            }
        }

        public void RegisterDespawn(GameObject go) {
            RemoveInRangeCollider(go);
        }

        public virtual void ProcessInit() {
            PopulateOriginalMaterials();

            CheckEnableInteractableRange();

            //persistentObjectComponent.Init();
        }

        public void PopulateOriginalMaterials() {
            objectMaterialController.PopulateOriginalMaterials();
        }

        protected virtual void CheckEnableInteractableRange() {
            EnableInteractableRange();
        }

        public void EnableInteractableRange() {
            //Debug.Log($"{gameObject.name}.Interactable.EnableInteractableRange()");

            // meant to be overwritten in unitcontrollers, as they enable this during SetUnitControllerMode()
            interactableEventController.NotifyOnEnableInteractableRange();
        }

        /// <summary>
        /// get a list of interactable options by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Dictionary<int, InteractableOptionComponent> GetInteractableOptionList(Type type) {
            Dictionary<int, InteractableOptionComponent> returnList = new Dictionary<int, InteractableOptionComponent>();

            foreach (KeyValuePair<int, InteractableOptionComponent> row in interactables) {
                if (row.Value.GetType() == type) {
                    returnList.Add(row.Key, row.Value);
                }
            }

            return returnList;
        }

        /// <summary>
        /// get the first interactable option by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public InteractableOptionComponent GetFirstInteractableOption(Type type) {

            foreach (InteractableOptionComponent interactableOption in interactables.Values) {
                if (interactableOption.GetType() == type) {
                    return interactableOption;
                }
            }

            return null;
        }

        public void AddInteractableOption(InteractableOptionComponent interactableOption) {
            //Debug.Log($"{gameObject.name}.Interactable.AddInteractable()");
            interactables.Add(interactableOptionCount, interactableOption);
            interactableOptionCount++;
        }


        /*
        protected virtual void Update() {
        }
        */

        public virtual bool UpdateOnPlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Interactable.UpdateOnPlayerUnitSpawn()");

            foreach (InteractableOptionComponent _interactable in interactables.Values) {
                _interactable.HandlePlayerUnitSpawn(sourceUnitController);
            }
            bool preRequisitesUpdated = false;
            foreach (InteractableOptionComponent _interactable in interactables.Values) {
                if (_interactable.PrerequisitesMet(sourceUnitController) == true) {
                    preRequisitesUpdated = true;
                }
            }

            // calling this because our base will not have inititalized its prerequisites earlier
            if (preRequisitesUpdated) {
                HandlePrerequisiteUpdates();
                return true;
            }

            return false;
        }

        public virtual void HandlePrerequisiteUpdates() {
            //Debug.Log($"{gameObject.name}.Interactable.HandlePrerequisiteUpdates()");

            if (!playerManagerClient.PlayerUnitSpawned) {
                return;
            }

            // give interaction panel a chance to update or close
            OnPrerequisiteUpdates();

            InstantiateMiniMapIndicator();

            foreach (UnitController inRangeUnitController in inRangeUnitControllers.Values) {
                if (GetCurrentInteractables(inRangeUnitController).Count == 0) {
                    inRangeUnitController.UnitEventController.NotifyOnExitInteractableRange(this);
                } else {
                    inRangeUnitController.EnterInteractableRange(this);
                }
            }
        }

        // meant to be overwritten on characters
        public virtual void EnableInteraction() {
            //Debug.Log($"{gameObject.name}.Interactable.EnableInteraction()");

            EnableCollider();
        }

        // meant to be overwritten on characters
        public virtual void DisableInteraction() {
            //Debug.Log($"{gameObject.name}.Interactable.DisableInteraction()");

            DisableCollider();
        }

        public void EnableCollider() {
            //Debug.Log($"{gameObject.name}.UnitController.EnableCollider()");

            if (myCollider != null) {
                myCollider.enabled = true;
            }
        }

        public void DisableCollider() {
            //Debug.Log($"{gameObject.name}.UnitController.DisableCollider()");

            if (myCollider != null) {
                myCollider.enabled = false;
            }
        }

        public bool InstantiateMiniMapIndicator() {
            //Debug.Log($"{gameObject.name}.Interactable.InstantiateMiniMapIndicator()");

            if (networkManagerServer.ServerModeActive == true) {
                return false;
            }

            if (!playerManagerClient.PlayerUnitSpawned) {
                //Debug.Log($"{gameObject.name}.Interactable.InstantiateMiniMapIndicator(): player unit not spawned yet.  returning");
                return false;
            }

            Dictionary<int, InteractableOptionComponent> validInteractables = GetValidInteractables(playerManagerClient.UnitController);
            if (validInteractables.Count == 0) {
                //if (GetValidInteractables(playerManager.UnitController.MyCharacterUnit).Count == 0) {
                //Debug.Log($"{gameObject.name}.Interactable.InstantiateMiniMapIndicator(): No valid Interactables.  Not spawning indicator.");
                return false;
            }

            if (!miniMapIndicatorReady) {
                /*
                if (MiniMapController.Instance == null) {
                    //Debug.Log($"{gameObject.name}.Interactable.InstantiateMiniMapIndicator(): MiniMapController.Instance is null");
                    return false;
                }
                */
                if (interactables.Count > 0) {
                    //Debug.Log($"{gameObject.name}.Interactable.InstantiateMiniMapIndicator(): interactables.length > 0");
                    miniMapManager.AddIndicator(this);
                    mainMapManager.AddIndicator(this);
                    miniMapIndicatorReady = true;
                    return true;
                }
            }
            return false;
        }

        public virtual void UpdateMiniMapIndicator() {
            // nothing here for now - meant to be overwritten by unit controllers
        }

        public virtual void UpdateMainMapIndicator() {
            // nothing here for now - meant to be overwritten by unit controllers
        }

        public void CleanupMiniMapIndicator() {
            //Debug.Log($"{gameObject.name}.Interactable.CleanupMiniMapIndicator()");
            //if (miniMapIndicator != null) {
            //Debug.Log($"{gameObject.name}.Interactable.CleanupMiniMapIndicator(): " + miniMapIndicator.name);
            //MiniMapController.Instance.RemoveIndicator(this);
            miniMapManager.RemoveIndicator(this);

            // keeping this set to true so any other update can't respawn it
            // if there is a situation where we re-enable interactables, then we should set it to false in OnEnable instead
            // miniMapIndicatorReady = false;
            //}
            //if (mainMapIndicator != null) {
            //Debug.Log($"{gameObject.name}.Interactable.CleanupMiniMapIndicator(): " + miniMapIndicator.name);
            //mainMapManager.RemoveIndicator(this);
            mainMapManager.RemoveIndicator(this);
            //}
            // re added this since both places it's called should set it false
            miniMapIndicatorReady = false;
        }

        /*
        public Transform GetInteractionTransform() {
            return interactionTransform;
        }
        */


        public void CloseInteractionWindow() {
            interactionManagerClient.SetInteractable(null);
            uIManager.interactionWindow.CloseWindow();
        }

        public bool CanInteract(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Interactable.CanInteract()");

            if (notInteractable == true) {
                return false;
            }
            //Debug.Log($"{gameObject.name}.Interactable.CanInteract()");
            if (playerManagerClient == null || playerManagerClient.PlayerUnitSpawned == false) {
                return false;
            }
            Dictionary<int, InteractableOptionComponent> validInteractables = GetValidInteractables(sourceUnitController);
            //List<InteractableOptionComponent> validInteractables = GetValidInteractables(playerManager.UnitController.MyCharacterUnit);
            if (validInteractables.Count > 0) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// The entry method for interactions via trigger.  Character Interactions are handled via InteractionManager
        /// </summary>
        /// <returns></returns>
        public virtual bool Interact() {
            //Debug.Log($"{gameObject.name}.Interactable.Interact()");

            if (notInteractable == true) {
                return false;
            }

            // get a list of valid interactables to determine if there is an action we can treat as default
            Dictionary<int, InteractableOptionComponent> validInteractables = GetCurrentInteractables(null);
            if (validInteractables.Count > 0) {
                int key = validInteractables.Take(1).Select(d => d.Key).First();
                validInteractables[key].Interact(null, key, 0);
                return true;
            }

            return false;
        }

        /// <summary>
        /// give the interactable a chance to face the player
        /// </summary>
        /// <param name="unitController"></param>
        /// <param name="factionValue"></param>
        public virtual void InteractWithPlayer(UnitController unitController) {
        }

        public virtual void ProcessStartInteract() {
            // do something in inherited class
        }

        public virtual void ProcessStopInteract() {
            // do something in inherited class
        }

        public virtual void ProcessStartInteractWithOption(InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            // do something in inherited class
        }

        public virtual void ProcessStopInteractWithOption(InteractableOptionComponent interactableOptionComponent) {
            // do something in inherited class
        }

        public Dictionary<int, InteractableOptionComponent> GetValidInteractables(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Interactable.GetValidInteractables(" + processRangeCheck + ", " + passedRangeCheck + ")");

            Dictionary<int, InteractableOptionComponent> validInteractables = new Dictionary<int, InteractableOptionComponent>();

            if (notInteractable == true) {
                return validInteractables;
            }

            if (interactables == null) {
                //Debug.Log($"{gameObject.name}.Interactable.GetValidInteractables(): interactables is null.  returning null!");
                return validInteractables;
            }

            foreach (KeyValuePair<int, InteractableOptionComponent> interactableOption in interactables) {
                if (interactableOption.Value != null && !interactableOption.Equals(null)) {
                    if (interactableOption.Value.GetValidOptionCount(sourceUnitController) > 0
                        && interactableOption.Value.PrerequisitesMet(sourceUnitController)
                        //&& (processRangeCheck == false || _interactable.CanInteract())
                        ) {

                        // HAD TO REMOVE THE FIRST CONDITION BECAUSE IT WAS BREAKING MINIMAP UPDATES - MONITOR FOR WHAT REMOVING THAT BREAKS...
                        //if (_interactable.CanInteract(source) && _interactable.GetValidOptionCount() > 0 && _interactable.MyPrerequisitesMet) {

                        //Debug.Log($"{gameObject.name}.Interactable.GetValidInteractables(): Adding valid interactable: " + _interactable.ToString());
                        validInteractables.Add(interactableOption.Key, interactableOption.Value);
                    }
                }
            }
            return validInteractables;
        }

        public virtual float PerformFactionCheck(UnitController sourceUnitController) {
            // interactables allow everything to interact by default.
            // characters will override this
            return 0;
        }

        public Dictionary<int, InteractableOptionComponent> GetInRangeInteractables(UnitController sourceUnitController) {
            bool passedRangeCheck = IsInRange(sourceUnitController);

            Dictionary<int, InteractableOptionComponent> inRangeInteractables = new Dictionary<int, InteractableOptionComponent>();
            foreach (KeyValuePair<int, InteractableOptionComponent> interactableOption in interactables) {
                if (interactableOption.Value.CanInteract(sourceUnitController, true, passedRangeCheck, true)) {
                    //Debug.Log($"{gameObject.name}.Interactable.GetCurrentInteractables(): Adding interactable: {interactableOption.ToString()}");
                    inRangeInteractables.Add(interactableOption.Key, interactableOption.Value);
                } else {
                    //Debug.Log($"{gameObject.name}.Interactable.GetValidInteractables(): invalid interactable: {interactableOption.ToString()}");
                }
            }
            return inRangeInteractables;
        }

        public Dictionary<int, InteractableOptionComponent> GetCurrentInteractables(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Interactable.GetCurrentInteractables()");

            if (notInteractable == true) {
                return new Dictionary<int, InteractableOptionComponent>();
            }

            Dictionary<int, InteractableOptionComponent> currentInteractables = new Dictionary<int, InteractableOptionComponent>();
            foreach (KeyValuePair<int, InteractableOptionComponent> interactableOption in interactables) {
                if (interactableOption.Value.CanInteract(sourceUnitController, false, false, true)) {
                    //Debug.Log($"{gameObject.name}.Interactable.GetCurrentInteractables(): Adding interactable: {interactableOption.ToString()}");
                    currentInteractables.Add(interactableOption.Key, interactableOption.Value);
                } else {
                    //Debug.Log($"{gameObject.name}.Interactable.GetValidInteractables(): invalid interactable: {interactableOption.ToString()}");
                }
            }
            return currentInteractables;
        }

        public Dictionary<int, InteractableOptionComponent> GetSwitchInteractables(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Interactable.GetSwitchInteractables({sourceUnitController?.gameObject.name})");

            /*
            // switches will often not be interactable, so we can skip this check
            if (notInteractable == true) {
                return new Dictionary<int, InteractableOptionComponent>();
            }
            */

            Dictionary<int, InteractableOptionComponent> currentInteractables = new Dictionary<int, InteractableOptionComponent>();
            foreach (KeyValuePair<int, InteractableOptionComponent> interactableOption in interactables) {
                if (interactableOption.Value.CanInteract(sourceUnitController, false, false, true, true)) {
                    //Debug.Log($"{gameObject.name}.Interactable.GetCurrentInteractables(): Adding interactable: {interactableOption.ToString()}");
                    currentInteractables.Add(interactableOption.Key, interactableOption.Value);
                } else {
                    //Debug.Log($"{gameObject.name}.Interactable.GetValidInteractables(): invalid interactable: {interactableOption.ToString()}");
                }
            }
            return currentInteractables;
        }

        /*
        /// <summary>
        /// native unity mouse enter message
        /// </summary>
        public void OnMouseEnter() {
            if (playerManager.UnitController.gameObject == gameObject) {
                return;
            }
            isMouseOverUnit = true;
            OnMouseIn();
        }
        */

        public virtual bool IsBuilding() {
            return false;
        }

        public virtual bool IsMouseOverBlocked() {
            return false;
        }

        /// <summary>
        /// called manually after mouse enters nameplate or interactable
        /// </summary>
        public void OnMouseIn() {
            //Debug.Log($"{gameObject.name}.Interactable.OnMouseIn()");

            if (!isActiveAndEnabled) {
                // this interactable is inactive, there is no reason to do anything
                return;
            }

            if (networkManagerServer.ServerModeActive == true) {
                return;
            }

            if (playerManagerClient == null) {
                return;
            }
            if (playerManagerClient.PlayerUnitSpawned == false) {
                return;
            }

            if (playerManagerClient.ActiveUnitController.gameObject == gameObject) {
                return;
            }

            if (notInteractable == true) {
                //Debug.Log($"{gameObject.name}.Interactable.OnMouseEnter(): notInteractable is true, not showing tooltip or glow.");
                return;
            }

            if (IsMouseOverBlocked()) {
                //Debug.Log($"{gameObject.name}.Interactable.OnMouseEnter(): Mouse over is blocked, not showing tooltip or glow.");
                return;
            }

            if (showTooltip == false) {
                //Debug.Log($"{gameObject.name}.Interactable.OnMouseEnter(): showTooltip is false, not showing tooltip or glow.");
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject() == true && !namePlateManager.MouseOverNamePlate()) {
                // THIS CODE WILL STILL CAUSE THE GUY TO GLOW IF YOU MOUSE OVER HIS NAMEPLATE WHILE A WINDOW IS UP.  NOT A BIG DEAL FOR NOW
                // IT HAS TO BE THIS WAY BECAUSE THE MOUSEOVER WINDOW IS A GAMEOBJECT AND WE NEED TO BE ABLE TO GLOW WHEN A WINDOW IS NOT UP AND WE ARE OVER IT
                // THIS COULD BE POTENTIALLY FIXED BY BLOCKING MOUSEOVER THE SAME WAY WE BLOCK DRAG IN THE UIMANAGER BY RESTRICTING ON MOUSEENTER ON ANY CLOSEABLEWINDOW IF IT'S TOO DISTRACTING
                // ANOTHER WAY WOULD BE DETECT NAMEPLATE UNDER MOUSE IN PLAYERCONTROLLER AND REMOVE THE AUTOMATIC MOUSEOVER RECEIVERS FROM ALL INTERACTABLES AND NAMEPLATES
                Debug.Log($"{gameObject.name}.Interactable.OnMouseEnter(): should not activate mouseover when windows are in front of things");
                return;
            }

            foreach (InteractableOptionComponent interactableOption in interactables.Values) {
                if (interactableOption.BlockTooltip == true) {
                    //Debug.Log($"{gameObject.name}.Interactable.OnMouseEnter(): {interactableOption.GetType()} is blocking tooltip.  Not showing tooltip or glow.");
                    return;
                }
            }

            // moved to before the return statement.  This is because we still want a tooltip even if there are no current interactions to perform
            // added pivot so the tooltip doesn't bounce around
            uIManager.ShowToolTip(new Vector2(0, 1), uIManager.MouseOverWindow.transform.position, CharacterTarget);

            // this function will not be triggered on the server, so sending the client player is ok
            if (CharacterTarget.GetCurrentInteractables(playerManagerClient.ActiveUnitController).Count == 0) {
                //if (GetValidInteractables(playerManager.UnitController.MyCharacterUnit).Count == 0) {
                //Debug.Log($"{gameObject.name}.Interactable.OnMouseEnter(): No current Interactables.  Not glowing.");
                return;
            }

            if (glowOnMouseOver) {
                //Debug.Log($"{gameObject.name}.Interactable.OnMouseEnter(): Turning on outline.");
                outlineController.TurnOnOutline();
            }// else {
                //Debug.Log($"{gameObject.name}.Interactable.OnMouseEnter(): glowOnMouseOver is false, not turning on outline.");
            //}

        }



        /*
        public void OnMouseExit() {
            if (playerManager?.UnitController?.gameObject == gameObject) {
                return;
            }

            isMouseOverUnit = false;
            OnMouseOut();
        }
        */

        // renamed from OnMouseOver to OnMouseOut to stop automatic events from being received
        public void OnMouseOut() {
            //Debug.Log($"{gameObject.name}.Interactable.OnMouseOut()");

            if (playerManagerClient == null) {
                return;
            }
            if (playerManagerClient.PlayerUnitSpawned == false) {
                return;
            }

            if (playerManagerClient.ActiveUnitController.gameObject == gameObject) {
                return;
            }

            if (notInteractable == true) {
                return;
            }

            if (isInitialized == false) {
                // if the unit despawns while the mouse is over it, we don't want to do anything
                return;
            }

            // prevent moving mouse from unit to namePlate from stopping glow or hiding tooltip
            if (isMouseOverNameplate || isMouseOverUnit) {
                return;
            }

            if (IsMouseOverBlocked()) {
                return;
            }

            //playerManager.PlayerController.HandleMouseOut(this);

            if (showTooltip == false) {
                return;
            }

            // new mouseover code
            uIManager.HideToolTip();

            outlineController.TurnOffOutline();
        }

        protected void OnMouseDown() {
            //Debug.Log($"{gameObject.name}: OnMouseDown()");
        }

        /// <summary>
        /// putting this in InteractableOptionComponent for now also
        /// </summary>
        public virtual void StopInteract() {
            // this is currently unused?  no references to it.
            //Debug.Log($"{gameObject.name}.Interactable.StopInteract()");
            foreach (InteractableOptionComponent interactable in interactables.Values) {
                interactable.StopInteract();
            }
            CloseInteractionWindow();
            isInteracting = false;
            return;
        }

        public void OnTriggerEnter(Collider other) {
            //Debug.Log($"{gameObject.name}.Interactable.OnTriggerEnter({other.gameObject.name})");

            if (notInteractable == true) {
                return;
            }

            if (isTrigger) {
                if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false) {
                    // triggers are server authoritative
                    return;
                }

                UnitController unitController = other.gameObject.GetComponent<UnitController>();
                if (unitController != null) {
                    if (unitController.RiderUnitController != null) {
                        unitController.RiderUnitController.UnitEventController.NotifyOnEnterInteractableTrigger(this);
                    } else if (unitController.UnitEventController != null) {
                        unitController.UnitEventController.NotifyOnEnterInteractableTrigger(this);
                    }
                } else if (interactWithAny) {
                    Interact();
                }
            }
        }

        public void OnTriggerExit(Collider other) {
            //Debug.Log($"{gameObject.name}.Interactable.OnTriggerExit({other.gameObject.name})");

            if (notInteractable == true) {
                return;
            }

            if (isTrigger == true && interactOnExit == true) {
                if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false) {
                    // triggers are server authoritative
                    return;
                }
                UnitController unitController = other.gameObject.GetComponent<UnitController>();
                // ensure ai don't accidentally trigger interactions
                if (unitController != null) {
                    unitController.UnitEventController.NotifyOnEnterInteractableTrigger(this);
                } else if (interactWithAny) {
                    Interact();
                }
            }

        }

        public virtual string GetSummary() {
            //Debug.Log($"{gameObject.name}.Interactable.GetDescription()");

            string nameString = DisplayName;
            if (DisplayName == string.Empty) {
                CharacterUnit baseCharacter = CharacterUnit.GetCharacterUnit(this);
                if (baseCharacter != null) {
                    nameString = baseCharacter.DisplayName;
                }
            }
            Color textColor = GetDescriptionColor();
            string titleString = GetTitleString();
            return string.Format("<color=#{0}>{1}{2}</color>\n{3}", ColorUtility.ToHtmlStringRGB(textColor), nameString, titleString, GetDescription());
            // this would be where quest tracker info goes if we want to add that in the future -eg: Kill 5 skeletons : 1/5
        }

        public virtual Color GetDescriptionColor() {
            return Color.white;
        }

        public virtual string GetTitleString() {
            return string.Empty;
        }

        public virtual string GetDescription() {
            //Debug.Log($"{gameObject.name}.Interactable.GetSummary()");

            string returnString = string.Empty;


            // switched this to current interactables so that we don't see mouseover options that we can't current interact with
            //List<InteractableOptionComponent> validInteractables = GetValidInteractables(playerManager.UnitController.MyCharacterUnit);
            Dictionary<int, InteractableOptionComponent> currentInteractables = GetCurrentInteractables(playerManagerClient.UnitController);

            // perform default interaction or open a window if there are multiple valid interactions
            List<string> returnStrings = new List<string>();
            foreach (InteractableOptionComponent interactableOptionComponent in currentInteractables.Values) {
                //if (!(_interactable is INamePlateUnit)) {
                // we already put the character name in the description so skip it here
                returnStrings.Add(interactableOptionComponent.GetSummary(playerManagerClient.UnitController));
                //}
            }
            returnString = string.Join("\n", returnStrings);
            return string.Format("{0}", returnString);
        }

        public virtual void ProcessBeginDialog() {
        }

        public virtual void ProcessEndDialog() {
        }

        public virtual void ProcessDialogTextUpdate(string newText) {
        }

        public virtual void ProcessShowQuestIndicator(string indicatorText, QuestGiverComponent questGiverComponent) {
        }

        public virtual void ProcessHideQuestIndicator() {
        }

        public virtual void ProcessStatusIndicatorSourceInit() {
            Dictionary<int, InteractableOptionComponent> currentInteractables = GetCurrentInteractables(playerManagerClient.UnitController);
            foreach (InteractableOptionComponent _interactable in currentInteractables.Values) {
                //if (!(_interactable is INamePlateUnit)) {
                // we already put the character name in the description so skip it here
                _interactable.ProcessStatusIndicatorSourceInit();
                //}
            }
        }

        #region MaterialChange

        public virtual Color GetGlowColor() {
            return Color.yellow;
        }

        #endregion

        public void HandleMiniMapStatusUpdate(InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log($"{gameObject.name}.Interactable.HandleMiniMapStatusUpdate({interactableOptionComponent.GetType()})");

            if (networkManagerServer.ServerModeActive == false) {
                miniMapManager.InteractableStatusUpdate(this, interactableOptionComponent);
                mainMapManager.InteractableStatusUpdate(this, interactableOptionComponent);
            }
            interactableEventController.NotifyOnMiniMapStatusUpdate(interactableOptionComponent);
        }

        public virtual void ConfigureDialogPanel(DialogPanel dialogPanelController) {
            // only needed in namePlateUnit and above
        }

        public virtual bool IsInRange(UnitController sourceUnitController) {
            GetValidInteractables(sourceUnitController);
            return IsInInteractableRange(sourceUnitController.gameObject);
        }

        public void NotifyOnInteractableDisable() {
            OnInteractableDisable();
        }

        public void NotifyOnInteractableResetSettings() {
            //Debug.Log($"{gameObject.name}.Interactable.NotifyOnInteractableResetSettings() {GetInstanceID()}");

            OnInteractableResetSettings();
        }

        public virtual void ResetSettings() {
            //Debug.Log($"{gameObject.name}.Interactable.ResetSettings() {GetInstanceID()}");

            if (glowOnMouseOver) {
                outlineController.TurnOffOutline();
            }

            foreach (InteractableOptionComponent interactableOptionComponent in interactables.Values) {
                //Debug.Log($"{gameObject.name}.Interactable.Awake(): Found InteractableOptionComponent: " + interactable.ToString());
                if (interactableOptionComponent != null) {
                    // in rare cases where a script is missing or has been made abstract, but not updated, this can return a null interactable option
                    interactableOptionComponent.Cleanup();
                }
            }
            CleanupMiniMapIndicator();
            NotifyOnInteractableResetSettings();

            foreach (UnitController inRangeUnitController in inRangeUnitControllers.Values) {
                inRangeUnitController.UnitEventController.NotifyOnExitInteractableRange(this);
            }
            inRangeUnitControllers.Clear();

            interactables = new Dictionary<int, InteractableOptionComponent>();
            interactableOptionCount = 0;
            isInteracting = false;
            miniMapIndicatorReady = false;
            isMouseOverUnit = false;
            isMouseOverNameplate = false;

            //miniMapIndicator = null;
            //mainMapIndicator = null;

            //CleanupEventSubscriptions();
            CleanupEverything();

            uuid = null;
            characterUnit = null;
            outlineController = null;
            objectMaterialController = null;
            interactableEventController = new InteractableEventController();
            dialogController = null;

            startHasRun = false;
            componentReferencesInitialized = false;
            isInitialized = false;
            //eventSubscriptionsInitialized = false;
            isTargeted = false;
            _interactableSaveData = null;

            UnregisterWithLevelManager();
        }

        protected virtual void UnregisterWithLevelManager() {
            //Debug.Log($"{gameObject.name}.Interactable.UnregisterWithLevelManager()");
            levelManagerServer.UnregisterInteractable(this);
        }

        public virtual void OnSendObjectToPool() {
        }

        /*
        public void CreateEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.Interactable.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            ProcessCreateEventSubscriptions();
            eventSubscriptionsInitialized = true;
        }

        public virtual void ProcessCreateEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.Interactable.ProcessCreateEventSubscriptions() Interactable instance: {GetInstanceID()}");
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.Interactable.CleanupEventSubscriptions(): {GetInstanceID()}");

            if (!eventSubscriptionsInitialized) {
                return;
            }
            ProcessCleanupEventSubscriptions();
            eventSubscriptionsInitialized = false;
        }

        public virtual void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.Interactable.ProcessCleanupEventSubscriptions() Interactable Instance: {GetInstanceID()}");
        }
        */


        public void SetTargeted() {
            //Debug.Log($"{gameObject.name}.Interactable.SetTargeted()");

            isTargeted = true;
            HandleTargeted();
        }

        public virtual void HandleTargeted() {
            interactableEventController.NotifyOnTargeted();
        }

        public void SetUnTargeted() {
            //Debug.Log($"{gameObject.name}.Interactable.SetUnTargeted()");

            isTargeted = false;
            HandleUnTargeted();
        }

        public void HandleUnTargeted() {
            interactableEventController.NotifyOnUnTargeted();
        }

        public void InteractableTriggerEnter(Collider collider) {
            //Debug.Log($"{gameObject.name}.Interactable.InteractableTriggerEnter({collider.gameObject.name})");

            if (inRangeUnitControllers.ContainsKey(collider.gameObject) == false) {
                UnitController unitController = collider.gameObject.GetComponent<UnitController>();
                if (unitController != null
                    && unitController.isInitialized == false) {
                    //Debug.LogWarning($"{gameObject.name}.Interactable.InteractableTriggerEnter({collider.gameObject.name}): unit controller is not initialized.  ignoring trigger enter.");
                }
                if (unitController == null
                    || unitController.isInitialized == false
                    || ((unitController.UnitControllerMode == UnitControllerMode.Player || unitController.UnitControllerMode == UnitControllerMode.Mount) == false)) {
                    return;
                }
                if (unitController.UnitControllerMode == UnitControllerMode.Player) {
                    inRangeUnitControllers.Add(collider.gameObject, unitController);
                }/* else if (unitController.UnitControllerMode == UnitControllerMode.Mount && unitController.RiderUnitController != null) {
                    inRangeUnitControllers.Add(unitController.RiderUnitController.gameObject, unitController.RiderUnitController);
                }*/
                if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false) {
                    // events from triggers are server authoritative
                    return;
                }
                if (unitController.UnitControllerMode == UnitControllerMode.Player) {
                    if (GetCurrentInteractables(unitController).Count == 0) {
                        return;
                    }
                }
                unitController.EnterInteractableRange(this);
            }
        }

        public void InteractableTriggerExit(Collider collider) {
            if (inRangeUnitControllers.ContainsKey(collider.gameObject) == false) {
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                // events from triggers are server authoritative
                inRangeUnitControllers[collider.gameObject].UnitEventController.NotifyOnExitInteractableRange(this);
            }
            RemoveInRangeCollider(collider.gameObject);
        }

        private void RemoveInRangeCollider(GameObject go) {
            //Debug.Log("InteractableRange.RemoveInRangeCollider(" + go.name + ") count: " + inRangeColliders.Count);
            if (inRangeUnitControllers.ContainsKey(go)) {
                inRangeUnitControllers.Remove(go);
            }
        }

        public bool IsInInteractableRange(GameObject go) {
            //Debug.Log($"{gameObject.name}.Interactable.IsInInteractableRange({go.name}) count: {inRangeUnitControllers.Count}"); 
            //Debug.Log($"InteractableRange.IsInRange({go.name}) count: {inRangeUnitControllers.Count} instanceId: {GetInstanceID()}");

            if (inRangeUnitControllers.ContainsKey(go)) {
                //Debug.Log($"{gameObject.name}.Interactable.IsInInteractableRange({go.name}): in range");
                return true;
            }
            //Debug.Log($"{gameObject.name}.Interactable.IsInInteractableRange({go.name}): not in range");
            return false;
        }

        public InteractableSaveData GetInteractableSaveData() {
            //Debug.Log($"{gameObject.name}.Interactable.GetInteractableSaveData()");

            InteractableSaveData interactableSaveData = new InteractableSaveData();
            foreach (InteractableOptionComponent interactableOptionComponent in interactables.Values) {
                interactableOptionComponent.SetSaveData(interactableSaveData);
            }
            return interactableSaveData;
        }

        public void LoadInteractableSaveData(InteractableSaveData interactableSaveData) {
            //Debug.Log($"{gameObject.name}.Interactable.LoadInteractableSaveData()");

            _interactableSaveData = interactableSaveData;
            systemItemManager.LoadItemInstanceListSaveData(interactableSaveData.ItemInstanceListSaveData);
            foreach (InteractableOptionComponent interactableOptionComponent in interactables.Values) {
                interactableOptionComponent.LoadFromSaveData(interactableSaveData);
            }
        }

        public virtual void PopulatePersistentObjectSaveData(PersistentObjectSaveData persistentObjectSaveData) {
            //Debug.Log($"{gameObject.name}.Interactable.PopulatePersistentObjectSaveData()");

            if (persistInteractableData == false) {
                return;
            }
            persistentObjectSaveData.InteractableSaveData = GetInteractableSaveData();
            persistentObjectSaveData.InteractableSaveData.BundleItems(systemItemManager);
        }

        public virtual void LoadPersistentObjectSaveData(PersistentObjectSaveData persistentObjectSaveData) {
            //Debug.Log($"{gameObject.name}.Interactable.LoadPersistentObjectSaveData()");

            if (persistInteractableData == false) {
                return;
            }
            if (persistentObjectSaveData.InteractableSaveData == null) {
                //Debug.LogWarning($"{gameObject.name}.Interactable.LoadPersistentObjectSaveData(): no interactable save data found.  skipping.");
                return;
            }
            LoadInteractableSaveData(persistentObjectSaveData.InteractableSaveData);
        }

    }

}