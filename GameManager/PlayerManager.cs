using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UMA;
using UMA.CharacterSystem;

namespace AnyRPG {
    public class PlayerManager : MonoBehaviour {

        #region Singleton
        private static PlayerManager instance;

        public static PlayerManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<PlayerManager>();
                }

                return instance;
            }
        }
        #endregion

        [SerializeField]
        private int initialLevel = 1;

        [SerializeField]
        private float maxMovementSpeed = 20f;

        [SerializeField]
        private LayerMask defaultGroundMask;

        [SerializeField]
        private GameObject playerConnectionParent = null;

        [SerializeField]
        private GameObject playerConnectionPrefab = null;

        [SerializeField]
        private GameObject playerUnitParent = null;

        [SerializeField]
        private GameObject aiUnitParent = null;

        [SerializeField]
        private GameObject effectPrefabParent = null;

        // the default non UMA player unit prefab
        [SerializeField]
        private string defaultPlayerUnitProfileName = string.Empty;

        [SerializeField]
        private string defaultCharacterCreatorUnitProfileName = string.Empty;

        [SerializeField]
        private List<string> defaultUMARaceProfiles = new List<string>();

        [SerializeField]
        private string defaultPlayerName = "Player";
        private string currentPlayerName = string.Empty;

        // players with no faction will get this one by default
        [SerializeField]
        private Faction defaultFaction = null;

        [SerializeField]
        private bool autoDetectNavMeshes = false;

        [SerializeField]
        private bool autoSpawnPlayerOnLevelLoad = false;

        // reference to the default profile
        private UnitProfile defaultPlayerUnitProfile;

        // reference to the default profile
        private UnitProfile defaultCharacterCreatorUnitProfile;

        /// <summary>
        /// The invisible gameobject that stores all the player scripts. A reference to an instantiated playerPrefab
        /// </summary>
        private GameObject playerConnectionObject = null;

        /// <summary>
        /// The actual movable rendered unit in the game world that we will be moving around
        /// </summary>
        private GameObject playerUnitObject = null;

        private bool playerUnitSpawned = false;

        private bool playerConnectionSpawned = false;

        private PlayerCharacter character = null;

        private DynamicCharacterAvatar avatar = null;

        protected bool eventSubscriptionsInitialized = false;

        public PlayerCharacter MyCharacter { get => character; set => character = value; }
        public GameObject MyPlayerConnectionObject { get => playerConnectionObject; set => playerConnectionObject = value; }
        public GameObject MyPlayerUnitObject { get => playerUnitObject; set => playerUnitObject = value; }
        public float MyMaxMovementSpeed { get => maxMovementSpeed; set => maxMovementSpeed = value; }
        public bool MyPlayerUnitSpawned { get => playerUnitSpawned; }
        public bool MyPlayerConnectionSpawned { get => playerConnectionSpawned; }
        public DynamicCharacterAvatar MyAvatar { get => avatar; set => avatar = value; }
        public int MyInitialLevel { get => initialLevel; set => initialLevel = value; }
        public Faction MyDefaultFaction { get => defaultFaction; set => defaultFaction = value; }
        public GameObject MyAIUnitParent { get => aiUnitParent; set => aiUnitParent = value; }
        public GameObject MyEffectPrefabParent { get => effectPrefabParent; set => effectPrefabParent = value; }
        public GameObject MyPlayerUnitParent { get => playerUnitParent; set => playerUnitParent = value; }
        public LayerMask MyDefaultGroundMask { get => defaultGroundMask; set => defaultGroundMask = value; }
        public string MyDefaultPlayerUnitProfileName { get => defaultPlayerUnitProfileName; set => defaultPlayerUnitProfileName = value; }
        public UnitProfile MyDefaultPlayerUnitProfile { get => defaultPlayerUnitProfile; set => defaultPlayerUnitProfile = value; }
        public string MyDefaultCharacterCreatorUnitProfileName { get => defaultCharacterCreatorUnitProfileName; set => defaultCharacterCreatorUnitProfileName = value; }
        public UnitProfile MyDefaultCharacterCreatorUnitProfile { get => defaultCharacterCreatorUnitProfile; set => defaultCharacterCreatorUnitProfile = value; }

        private void Awake() {
            //Debug.Log("PlayerManager.Awake()");
            /*
            if (defaultIsNonUMAUnit == true) {
                currentPlayerUnitPrefab = defaultNonUMAPlayerUnitPrefab;
            } else {
                currentPlayerUnitPrefab = defaultUMAPlayerUnitPrefab;
            }
            */
        }

        public void PerformRequiredPropertyChecks() {
            if (defaultPlayerUnitProfileName == null || defaultPlayerUnitProfileName == string.Empty) {
                Debug.LogError("PlayerManager.Awake(): the default player unit profile name is null.  Please set it in the inspector");
            }
            if (aiUnitParent == null) {
                Debug.LogError("PlayerManager.Awake(): the ai unit parent is null.  Please set it in the inspector");
            }
            if (effectPrefabParent == null) {
                Debug.LogError("PlayerManager.Awake(): the effect prefab parent is null.  Please set it in the inspector");
            }
        }

        public void OrchestratorStart() {
            PerformRequiredPropertyChecks();
            SetupScriptableObjects();
            CreateEventSubscriptions();
        }

        public void SetupScriptableObjects() {

            // get default player unit profile
            if (defaultPlayerUnitProfileName != null && defaultPlayerUnitProfileName != string.Empty) {
                UnitProfile tmpUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(defaultPlayerUnitProfileName);
                if (tmpUnitProfile != null) {
                    defaultPlayerUnitProfile = tmpUnitProfile;
                } else {
                    Debug.LogError("PlayerManager.SetupScriptableObjects(): could not find unit profile " + defaultPlayerUnitProfileName + ".  Check Inspector");
                }
            } else {
                Debug.LogError("PlayerManager.SetupScriptableObjects(): defaultPlayerUnitProfileName field is required, but not value was set.  Check Inspector");
            }

            // get default player unit profile
            if (defaultCharacterCreatorUnitProfileName != null && defaultCharacterCreatorUnitProfileName != string.Empty) {
                UnitProfile tmpUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(defaultCharacterCreatorUnitProfileName);
                if (tmpUnitProfile != null) {
                    defaultCharacterCreatorUnitProfile = tmpUnitProfile;
                } else {
                    Debug.LogError("PlayerManager.SetupScriptableObjects(): could not find unit profile " + defaultPlayerUnitProfileName + ".  Check Inspector");
                }
            } else {
                Debug.LogError("PlayerManager.SetupScriptableObjects(): defaultPlayerUnitProfileName field is required, but not value was set.  Check Inspector");
            }

            //defaultCharacterCreatorUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(defaultCharacterCreatorUnitProfileName);
        }

        private void Start() {
            //Debug.Log("PlayerManager.Start()");
            //CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
            SystemEventManager.MyInstance.OnExitGame += ExitGameHandler;
            SystemEventManager.MyInstance.OnLevelChanged += PlayLevelUpEffects;
            SystemEventManager.StartListening("OnPlayerDeath", HandlePlayerDeath);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
                SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
                SystemEventManager.MyInstance.OnExitGame -= ExitGameHandler;
                SystemEventManager.MyInstance.OnLevelChanged -= PlayLevelUpEffects;
                SystemEventManager.StopListening("OnPlayerDeath", HandlePlayerDeath);
            }
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            ProcessLevelUnload();
        }


        public void ResetInitialLevel() {
            initialLevel = 1;
        }

        public void ExitGameHandler() {
            //Debug.Log("PlayerManager.ExitGameHandler()");
            DespawnPlayerUnit();
            DespawnPlayerConnection();
            SaveManager.MyInstance.ClearSystemManagedCharacterData();
        }

        public void SetPlayerName(string newName) {
            //Debug.Log("PlayerManager.SetPlayerName()");
            if (newName != null && newName != string.Empty) {
                MyCharacter.SetCharacterName(newName);
            }

            SystemEventManager.MyInstance.NotifyOnPlayerNameChanged();
            if (playerUnitSpawned) {
                UIManager.MyInstance.MyPlayerUnitFrameController.SetTarget(MyPlayerUnitObject);
            }
        }

        public void SetPlayerCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log("PlayerManager.SetPlayerCharacterClass(" + characterClassName + ")");
            if (newCharacterClass != null) {
                MyCharacter.ChangeCharacterClass(newCharacterClass);
            }
        }

        public void SetPlayerCharacterSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log("PlayerManager.SetPlayerCharacterClass(" + characterClassName + ")");
            if (newClassSpecialization != null) {
                MyCharacter.ChangeClassSpecialization(newClassSpecialization);
            }
        }

        public void SetPlayerFaction(Faction newFaction) {
            //Debug.Log("PlayerManager.SetPlayerFaction(" + factionName + ")");
            if (newFaction != null) {
                MyCharacter.JoinFaction(newFaction);
            }
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void SetUMAPrefab() {
            // if an UMA prefab exists, set it as the current default for spawning
            //Debug.Log("Playermanager.SetUMAPrefab()");
            /*
            if (defaultUMAPlayerUnitPrefab != null) {
                //Debug.Log("Playermanager.SetUMAPrefab(): UMA prefab set successfully");
                currentPlayerUnitPrefab = defaultUMAPlayerUnitPrefab;
            } else {
                //Debug.Log("Playermanager.SetUMAPrefab(): no player unit UMA prefab found!!");
            }
            */
        }

        public void SetDefaultPrefab() {
            //Debug.Log("PlayerManager.SetDefaultPrefab()");
            // if an UMA prefab exists, set it as the current default for spawning
            /*
            if (defaultNonUMAPlayerUnitPrefab != null) {
                //Debug.Log("PlayerManager.SetDefaultPrefab(): setting default to non uma prefab");
                currentPlayerUnitPrefab = defaultNonUMAPlayerUnitPrefab;
            } else {
                //Debug.Log("PlayerManager.SetDefaultPrefab(): no player unit Non UMA prefab found!!");
            }
            */
        }

        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("PlayerManager.OnLevelLoad()");
            bool loadCharacter = true;
            SceneNode activeSceneNode = LevelManager.MyInstance.GetActiveSceneNode();
            if (activeSceneNode != null) {
                //Debug.Log("PlayerManager.OnLevelLoad(): we have a scene node");
                // fix to allow character to spawn after cutscene is viewed on next level load - and another fix to prevent character from spawning on a pure cutscene
                if ((activeSceneNode.MyAutoPlayCutscene != null && (activeSceneNode.MyAutoPlayCutscene.Viewed == false || activeSceneNode.MyAutoPlayCutscene.Repeatable == true)) || activeSceneNode.MySuppressCharacterSpawn) {
                    //Debug.Log("PlayerManager.OnLevelLoad(): character spawn is suppressed");
                    loadCharacter = false;
                    CameraManager.MyInstance.DeactivateMainCamera();
                    //CameraManager.MyInstance.MyCharacterCreatorCamera.gameObject.SetActive(true);
                }
            }
            if (autoSpawnPlayerOnLevelLoad == true && loadCharacter) {
                //CameraManager.MyInstance.MyCharacterCreatorCamera.gameObject.SetActive(false);
                Vector3 spawnLocation = SpawnPlayerUnit();
                CameraManager.MyInstance.ActivateMainCamera();
                CameraManager.MyInstance.MyMainCameraController.SetTargetPositionRaw(spawnLocation, MyPlayerUnitObject.transform.forward);
            }
        }

        public void PlayLevelUpEffects(int newLevel) {
            //Debug.Log("PlayerManager.PlayLevelUpEffect()");
            if (MyPlayerUnitSpawned == false) {
                return;
            }
            //PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.PerformAbilityCast((levelUpAbility as IAbility), null);
            // 0 to allow playing this effect for different reasons than levelup
            if (newLevel == 0 || newLevel != 1) {
                //MyCharacter.CharacterAbilityManager.BeginAbility((SystemConfigurationManager.MyInstance.MyLevelUpAbility as IAbility), MyCharacter.CharacterUnit.gameObject);
                AbilityEffectContext abilityEffectContext = new AbilityEffectContext();
                abilityEffectContext.baseAbility = SystemConfigurationManager.MyInstance.MyLevelUpAbility;

                SystemConfigurationManager.MyInstance.MyLevelUpAbility.Cast(SystemAbilityController.MyInstance, MyCharacter.CharacterUnit.gameObject, abilityEffectContext);
            }
        }

        public void PlayDeathEffect() {
            //Debug.Log("PlayerManager.PlayDeathEffect()");
            if (MyPlayerUnitSpawned == false) {
                return;
            }
            //PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.PerformAbilityCast((levelUpAbility as IAbility), null);
            //MyCharacter.CharacterAbilityManager.BeginAbility((SystemConfigurationManager.MyInstance.DeathAbility as IAbility), MyCharacter.CharacterUnit.gameObject);
            AbilityEffectContext abilityEffectContext = new AbilityEffectContext();
            abilityEffectContext.baseAbility = SystemConfigurationManager.MyInstance.DeathAbility;
            SystemConfigurationManager.MyInstance.DeathAbility.Cast(SystemAbilityController.MyInstance, MyCharacter.CharacterUnit.gameObject, abilityEffectContext);
        }

        public void Initialize() {
            //Debug.Log("PlayerManager.Initialize()");
            SpawnPlayerConnection();
            SpawnPlayerUnit();
        }

        public void ProcessLevelUnload() {
            DespawnPlayerUnit();
        }

        public void DespawnPlayerUnit() {
            //Debug.Log("PlayerManager.DespawnPlayerUnit()");
            if (!playerUnitSpawned) {
                //Debug.Log("Player Unit is not spawned.  Nothing to despawn.  returning");
                return;
            }
            // trying this at top so subscribers can remove their methods before the object is destroyed
            SystemEventManager.MyInstance.NotifyOnPlayerUnitDespawn();

            Destroy(playerUnitObject);
            playerUnitObject = null;
            playerUnitSpawned = false;
        }

        public void HandlePlayerDeath(string eventName, EventParamProperties eventParam) {
            //Debug.Log("PlayerManager.KillPlayer()");
            PlayDeathEffect();
        }

        public void RespawnPlayer() {
            //Debug.Log("PlayerManager.RespawnPlayer()");
            DespawnPlayerUnit();
            MyCharacter.CharacterStats.ReviveRaw();
            SpawnPlayerUnit();
        }

        public void RevivePlayerUnit() {
            //Debug.Log("PlayerManager.RevivePlayerUnit()");
            MyCharacter.CharacterStats.Revive();
        }

        public void SpawnPlayerUnit(Vector3 spawnLocation) {
            //Debug.Log("PlayerManager.SpawnPlayerUnit(" + spawnLocation + ")");

            if (playerUnitObject != null) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Player Unit already exists");
                return;
            }

            if (playerConnectionObject == null) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): playerConnectionObject is null, instantiating connection!");
                SpawnPlayerConnection();
            }
            if (MyCharacter.UnitProfile == null) {
                MyCharacter.SetUnitProfile(defaultPlayerUnitProfileName);
            }

            // spawn the player unit
            //playerUnitObject = Instantiate(currentPlayerUnitPrefab, spawnLocation, Quaternion.LookRotation(Vector3.forward), playerUnitParent.transform);
            Vector3 spawnRotation = LevelManager.MyInstance.GetSpawnRotation();
            //Debug.Log("PlayerManager.SpawnPlayerUnit(): spawning player unit at location: " + playerUnitParent.transform.position + " with rotation: " + spawnRotation);
            playerUnitObject = Instantiate(MyCharacter.UnitProfile.UnitPrefab, spawnLocation, Quaternion.LookRotation(spawnRotation), playerUnitParent.transform);

            // create a reference from the character (connection) to the character unit, and from the character unit to the character (connection)
            MyCharacter.CharacterUnit = playerUnitObject.GetComponent<PlayerUnit>();
            MyCharacter.CharacterUnit.MyCharacter = MyCharacter;
            MyCharacter.CharacterUnit.GetComponentReferences();

            MyCharacter.AnimatedUnit = playerUnitObject.GetComponent<AnimatedUnit>();
            MyCharacter.CharacterUnit.OrchestratorStart();
            MyCharacter.CharacterUnit.OrchestratorFinish();

            NavMeshAgent navMeshAgent = playerUnitObject.GetComponent<NavMeshAgent>();

            if (LevelManager.MyInstance.MyNavMeshAvailable == true && autoDetectNavMeshes) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Enabling NavMeshAgent()");
                if (navMeshAgent != null) {
                    navMeshAgent.enabled = true;
                }
                if (MyCharacter.AnimatedUnit is AnimatedPlayerUnit && (MyCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController != null) {
                    (MyCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController.useMeshNav = true;
                }
            } else {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Disabling NavMeshAgent()");
                if (navMeshAgent != null) {
                    navMeshAgent.enabled = false;
                }
                if (MyCharacter.AnimatedUnit is AnimatedPlayerUnit && (MyCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController != null) {
                    (MyCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController.useMeshNav = false;
                }
            }

            if (MyCharacter.UnitProfile.IsUMAUnit == true) {
                // do UMA spawn stuff to wait for UMA to spawn
                InitializeUMA();
            } else {
                // handle spawn immediately since this is a non UMA unit and waiting should not be necessary
                HandlePlayerUnitSpawn();
            }

        }

        public Vector3 SpawnPlayerUnit() {
            //Debug.Log("PlayerManager.SpawnPlayerUnit()");
            Vector3 spawnLocation = LevelManager.MyInstance.GetSpawnLocation();
            SpawnPlayerUnit(spawnLocation);
            return spawnLocation;
        }

        public void HandleUMACreated() {
            //Debug.Log("PlayerManager.HandleUMACreated()");

            // update the UMA armor models and stuff
            // testing do this earlier
            //SaveManager.MyInstance.LoadUMASettings();

            HandlePlayerUnitSpawn();
        }

        private void HandlePlayerUnitSpawn() {
            // inform any subscribers that we just spawned a player unit
            //Debug.Log("PlayerManager.HandlePlayerUnitSpawn(): calling SystemEventManager.MyInstance.NotifyOnPlayerUnitSpawn()");
            playerUnitSpawned = true;
            SystemEventManager.TriggerEvent("OnPlayerUnitSpawn", new EventParamProperties());

            // do this just in case things that would not update before the player unit spawned that are now initialized due to that last call have a chance to react : EDIT BELOW
            // this should no longer be necessary and it is causing double calls every time the player unit spawns.  if it is needed, then whatever is supposed to use it, should instead react to
            // notifyonplayerunitspawn
            //SystemEventManager.MyInstance.NotifyOnPrerequisiteUpdated();
        }

        public void InitializeUMA() {
            //Debug.Log("PlayerManager.InitializeUMA()");

            // ensure the character unit has its references before we try to access them
            //MyCharacter.MyCharacterUnit.GetComponentReferences();

            avatar = MyPlayerUnitObject.GetComponent<DynamicCharacterAvatar>();
            if (avatar == null) {
                Debug.Log("PlayerManager.InitializeUMA(): avatar is null!!! returning");
                return;
            }

            // try this earlier
            SaveManager.MyInstance.LoadUMASettings(false);

            // initialize the animator so our avatar initialization has an animator.
            MyCharacter.AnimatedUnit.MyCharacterAnimator.InitializeAnimator();
            avatar.Initialize();
            UMAData umaData = avatar.umaData;
            umaData.OnCharacterBeforeDnaUpdated += OnCharacterBeforeDnaUpdated;
            umaData.OnCharacterBeforeUpdated += OnCharacterBeforeUpdated;
            umaData.OnCharacterCreated += HandleCharacterCreated;
            umaData.OnCharacterDnaUpdated += OnCharacterDnaUpdated;
            umaData.OnCharacterDestroyed += OnCharacterDestroyed;
            umaData.OnCharacterUpdated += OnCharacterUpdatedCallback;
        }

        public void OnCharacterBeforeDnaUpdated(UMAData umaData) {
            //Debug.Log("PlayerManager.BeforeDnaUpdated(): " + umaData);
        }
        public void OnCharacterBeforeUpdated(UMAData umaData) {
            //Debug.Log("PlayerManager.OnCharacterBeforeUpdated(): " + umaData);
        }
        public void HandleCharacterCreated(UMAData umaData) {
            //Debug.Log("PlayerManager.CharacterCreatedCallback(): " + umaData);
            HandleUMACreated();
        }
        public void OnCharacterDnaUpdated(UMAData umaData) {
            //Debug.Log("PlayerManager.OnCharacterDnaUpdated(): " + umaData);
        }
        public void OnCharacterDestroyed(UMAData umaData) {
            //Debug.Log("PlayerManager.OnCharacterDestroyed(): " + umaData);
        }
        public void OnCharacterUpdatedCallback(UMAData umaData) {
            //Debug.Log("PlayerManager.OnCharacterUpdated(): " + umaData);
        }

        public void SpawnPlayerConnection() {
            //Debug.Log("PlayerManager.SpawnPlayerConnection()");
            if (playerConnectionObject != null) {
                //Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is not null.  exiting.");
                return;
            }
            playerConnectionObject = Instantiate(playerConnectionPrefab, playerConnectionParent.transform);
            MyCharacter = playerConnectionObject.GetComponent<PlayerCharacter>() as PlayerCharacter;

            SystemEventManager.MyInstance.NotifyBeforePlayerConnectionSpawn();

            MyCharacter.OrchestratorStart();
            MyCharacter.OrchestratorFinish();

            MyCharacter.Initialize(defaultPlayerName, initialLevel);
            playerConnectionSpawned = true;
            SystemEventManager.MyInstance.NotifyOnPlayerConnectionSpawn();
        }

        public void DespawnPlayerConnection() {
            if (playerConnectionObject == null) {
                Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is null.  exiting.");
                return;
            }
            SystemEventManager.MyInstance.NotifyOnPlayerConnectionDespawn();
            Destroy(playerConnectionObject.gameObject);
            MyCharacter = null;
            playerConnectionSpawned = false;
        }

    }

}