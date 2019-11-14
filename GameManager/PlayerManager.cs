using AnyRPG;
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
        private GameObject playerConnectionParent;

        [SerializeField]
        private GameObject playerConnectionPrefab;

        [SerializeField]
        private GameObject playerUnitParent;

        [SerializeField]
        private GameObject aiUnitParent;

        // the default non UMA player unit prefab
        [SerializeField]
        private GameObject defaultNonUMAPlayerUnitPrefab;

        // the default UMA player unit prefab
        [SerializeField]
        private GameObject defaultUMAPlayerUnitPrefab;

        // should the non UMA unit, or the UMA unit be used as the default player unit
        [SerializeField]
        private bool defaultIsNonUMAUnit = true;

        [SerializeField]
        private string defaultPlayerName = "Player";
        private string currentPlayerName = string.Empty;

        // players with no faction will get this one by default
        [SerializeField]
        private Faction defaultFaction;

        [SerializeField]
        private bool autoDetectNavMeshes = false;

        [SerializeField]
        private bool autoSpawnPlayerOnLevelLoad = false;

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

        private GameObject currentPlayerUnitPrefab = null;

        protected bool startHasRun = false;
        protected bool eventSubscriptionsInitialized = false;

        public PlayerCharacter MyCharacter { get => character; set => character = value; }
        public GameObject MyPlayerConnectionObject { get => playerConnectionObject; set => playerConnectionObject = value; }
        public GameObject MyPlayerUnitObject { get => playerUnitObject; set => playerUnitObject = value; }
        public float MyMaxMovementSpeed { get => maxMovementSpeed; set => maxMovementSpeed = value; }
        public bool MyPlayerUnitSpawned { get => playerUnitSpawned; }
        public bool MyPlayerConnectionSpawned { get => playerConnectionSpawned; }
        public DynamicCharacterAvatar MyAvatar { get => avatar; set => avatar = value; }
        public int MyInitialLevel { get => initialLevel; set => initialLevel = value; }
        public GameObject MyCurrentPlayerUnitPrefab { get => currentPlayerUnitPrefab; set => currentPlayerUnitPrefab = value; }
        public GameObject MyDefaultNonUMAPlayerUnitPrefab { get => defaultNonUMAPlayerUnitPrefab; set => defaultNonUMAPlayerUnitPrefab = value; }
        public GameObject MyDefaultUMAPlayerUnitPrefab { get => defaultUMAPlayerUnitPrefab; set => defaultUMAPlayerUnitPrefab = value; }
        public bool MyDefaultIsNonUMAUnit { get => defaultIsNonUMAUnit; set => defaultIsNonUMAUnit = value; }
        public Faction MyDefaultFaction { get => defaultFaction; set => defaultFaction = value; }
        public GameObject MyAIUnitParent { get => aiUnitParent; set => aiUnitParent = value; }

        private void Awake() {
            //Debug.Log("PlayerManager.Awake()");
            if (defaultIsNonUMAUnit == true) {
                currentPlayerUnitPrefab = defaultNonUMAPlayerUnitPrefab;
            } else {
                currentPlayerUnitPrefab = defaultUMAPlayerUnitPrefab;
            }
            if (currentPlayerUnitPrefab == null) {
                Debug.LogError("PlayerManager.Awake(): the default player unit prefab is null.  Please set it in the inspector");
            }
            if (aiUnitParent == null) {
                Debug.LogError("PlayerManager.Awake(): the ai unit parent is null.  Please set it in the inspector");
            }
        }

        private void Start() {
            //Debug.Log("PlayerManager.Start()");
            startHasRun = true;
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized || !startHasRun) {
                return;
            }
            SystemEventManager.MyInstance.OnLevelUnload += DespawnPlayerUnit;
            SystemEventManager.MyInstance.OnLevelLoad += OnLevelLoad;
            SystemEventManager.MyInstance.OnExitGame += ExitGameHandler;
            SystemEventManager.MyInstance.OnLevelChanged += PlayLevelUpEffects;
            SystemEventManager.MyInstance.OnPlayerDeath += HandlePlayerDeath;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnLevelUnload -= DespawnPlayerUnit;
                SystemEventManager.MyInstance.OnLevelLoad -= OnLevelLoad;
                SystemEventManager.MyInstance.OnExitGame -= ExitGameHandler;
                SystemEventManager.MyInstance.OnLevelChanged -= PlayLevelUpEffects;
                SystemEventManager.MyInstance.OnPlayerDeath -= HandlePlayerDeath;
            }
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
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

        public void SetPlayerFaction(string factionName) {
            //Debug.Log("PlayerManager.SetPlayerFaction(" + factionName + ")");
            if (factionName != null && factionName != string.Empty) {
                MyCharacter.JoinFaction(factionName);
            }
            SystemEventManager.MyInstance.NotifyOnPrerequisiteUpdated();
        }

        public void SetUMAPrefab() {
            // if an UMA prefab exists, set it as the current default for spawning
            //Debug.Log("Playermanager.SetUMAPrefab()");
            if (defaultUMAPlayerUnitPrefab != null) {
                //Debug.Log("Playermanager.SetUMAPrefab(): UMA prefab set successfully");
                currentPlayerUnitPrefab = defaultUMAPlayerUnitPrefab;
            } else {
                //Debug.Log("Playermanager.SetUMAPrefab(): no player unit UMA prefab found!!");
            }
        }

        public void SetDefaultPrefab() {
            //Debug.Log("PlayerManager.SetDefaultPrefab()");
            // if an UMA prefab exists, set it as the current default for spawning
            if (defaultNonUMAPlayerUnitPrefab != null) {
                //Debug.Log("PlayerManager.SetDefaultPrefab(): setting default to non uma prefab");
                currentPlayerUnitPrefab = defaultNonUMAPlayerUnitPrefab;
            } else {
                //Debug.Log("PlayerManager.SetDefaultPrefab(): no player unit Non UMA prefab found!!");
            }
        }

        public void OnLevelLoad() {
            //Debug.Log("PlayerManager.OnLevelLoad()");
            bool loadCharacter = true;
            SceneNode activeSceneNode = LevelManager.MyInstance.GetActiveSceneNode();
            if (activeSceneNode != null) {
                //Debug.Log("PlayerManager.OnLevelLoad(): we have a scene node");
                // fix to allow character to spawn after cutscene is viewed on next level load - and another fix to prevent character from spawning on a pure cutscene
                if (activeSceneNode.MyIsCutScene || (activeSceneNode.MySuppressCharacterSpawn && !activeSceneNode.MyCutsceneViewed)) {
                    //Debug.Log("PlayerManager.OnLevelLoad(): character spawn is suppressed");
                    loadCharacter = false;
                    CameraManager.MyInstance.MyMainCamera.gameObject.SetActive(false);
                    //CameraManager.MyInstance.MyCharacterCreatorCamera.gameObject.SetActive(true);
                }
            }
            if (autoSpawnPlayerOnLevelLoad == true && loadCharacter) {
                //CameraManager.MyInstance.MyCharacterCreatorCamera.gameObject.SetActive(false);
                Vector3 spawnLocation = SpawnPlayerUnit();
                CameraManager.MyInstance.MyMainCamera.gameObject.SetActive(true);
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
                MyCharacter.MyCharacterAbilityManager.BeginAbility((SystemConfigurationManager.MyInstance.MyLevelUpAbility as IAbility), MyCharacter.MyCharacterUnit.gameObject);
            }
        }

        public void PlayDeathEffect() {
            //Debug.Log("PlayerManager.PlayDeathEffect()");
            if (MyPlayerUnitSpawned == false) {
                return;
            }
            //PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.PerformAbilityCast((levelUpAbility as IAbility), null);
            MyCharacter.MyCharacterAbilityManager.BeginAbility((SystemConfigurationManager.MyInstance.MyDeathAbility as IAbility), MyCharacter.MyCharacterUnit.gameObject);
        }

        public void Initialize() {
            //Debug.Log("PlayerManager.Initialize()");
            SpawnPlayerConnection();
            SpawnPlayerUnit();
        }

        public void DespawnPlayerUnit() {

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

        public void HandlePlayerDeath() {
            //Debug.Log("PlayerManager.KillPlayer()");
            PlayDeathEffect();
        }

        public void RespawnPlayer() {
            //Debug.Log("PlayerManager.RespawnPlayer()");
            DespawnPlayerUnit();
            MyCharacter.MyCharacterStats.ReviveRaw();
            SpawnPlayerUnit();
        }

        public void RevivePlayerUnit() {
            //Debug.Log("PlayerManager.RevivePlayerUnit()");
            MyCharacter.MyCharacterStats.Revive();
        }

        public void SpawnPlayerUnit(Vector3 spawnLocation) {
            // Debug.Log("PlayerManager.SpawnPlayerUnit()");

            if (playerUnitObject != null) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Player Unit already exists");
                return;
            }

            if (playerConnectionObject == null) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): playerConnectionObject is null, instantiating connection!");
                SpawnPlayerConnection();
            }

            // spawn the player unit
            //playerUnitObject = Instantiate(currentPlayerUnitPrefab, spawnLocation, Quaternion.LookRotation(Vector3.forward), playerUnitParent.transform);
            Vector3 spawnRotation = LevelManager.MyInstance.GetSpawnRotation();
            //Debug.Log("PlayerManager.SpawnPlayerUnit(): spawning player unit at location: " + playerUnitParent.transform.position + " with rotation: " + spawnRotation);
            playerUnitObject = Instantiate(currentPlayerUnitPrefab, spawnLocation, Quaternion.LookRotation(spawnRotation), playerUnitParent.transform);

            // create a reference from the character (connection) to the character unit, and from the character unit to the character (connection)
            MyCharacter.MyCharacterUnit = playerUnitObject.GetComponent<PlayerUnit>();
            MyCharacter.MyCharacterUnit.MyCharacter = MyCharacter;

            if (LevelManager.MyInstance.MyNavMeshAvailable == true && autoDetectNavMeshes) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Enabling NavMeshAgent()");
                playerUnitObject.GetComponent<NavMeshAgent>().enabled = true;
                playerUnitObject.GetComponent<PlayerUnitMovementController>().useMeshNav = true;
            } else {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Disabling NavMeshAgent()");
                playerUnitObject.GetComponent<NavMeshAgent>().enabled = false;
                playerUnitObject.GetComponent<PlayerUnitMovementController>().useMeshNav = false;
            }

            if (currentPlayerUnitPrefab == defaultUMAPlayerUnitPrefab) {
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
            SaveManager.MyInstance.LoadUMASettings();
            HandlePlayerUnitSpawn();
        }

        private void HandlePlayerUnitSpawn() {
            // inform any subscribers that we just spawned a player unit
            //Debug.Log("PlayerManager.HandlePlayerUnitSpawn(): calling SystemEventManager.MyInstance.NotifyOnPlayerUnitSpawn()");
            playerUnitSpawned = true;
            SystemEventManager.MyInstance.NotifyOnPlayerUnitSpawn();

            // do this just in case things that would not update before the player unit spawned that are now initialized due to that last call have a chance to react : EDIT BELOW
            // this should no longer be necessary and it is causing double calls every time the player unit spawns.  if it is needed, then whatever is supposed to use it, should instead react to
            // notifyonplayerunitspawn
            //SystemEventManager.MyInstance.NotifyOnPrerequisiteUpdated();
        }

        public void InitializeUMA() {
            //Debug.Log("PlayerManager.InitializeUMA()");

            // ensure the character unit has its references before we try to access them
            MyCharacter.MyCharacterUnit.GetComponentReferences();

            avatar = MyPlayerUnitObject.GetComponent<DynamicCharacterAvatar>();
            if (avatar == null) {
                Debug.Log("PlayerManager.InitializeUMA(): avatar is null!!! returning");
                return;
            }


            // initialize the animator so our avatar initialization has an animator.
            MyCharacter.MyCharacterUnit.MyCharacterAnimator.InitializeAnimator();
            avatar.Initialize();
            UMAData umaData = avatar.umaData;
            umaData.OnCharacterBeforeDnaUpdated += OnCharacterBeforeDnaUpdated;
            umaData.OnCharacterBeforeUpdated += OnCharacterBeforeUpdated;
            umaData.OnCharacterCreated += OnCharacterCreated;
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
        public void OnCharacterCreated(UMAData umaData) {
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
            // try moving before initialize so initialize has a
            MyCharacter.Initialize(defaultPlayerName, initialLevel);
            playerConnectionSpawned = true;
            SystemEventManager.MyInstance.NotifyOnPlayerConnectionSpawn();
        }

        public void DespawnPlayerConnection() {
            if (playerConnectionObject == null) {
                //Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is not null.  exiting.");
                return;
            }
            SystemEventManager.MyInstance.NotifyOnPlayerConnectionDespawn();
            Destroy(playerConnectionObject.gameObject);
            MyCharacter = null;
            playerConnectionSpawned = false;
        }

    }

}