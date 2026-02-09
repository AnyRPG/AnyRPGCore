using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class SystemGameManager : MonoBehaviour {

        // private fields
        private GameMode gameMode = GameMode.Local;

        // serialized fields
        [Header("Configuration")]

        // configuration monobehavior
        [SerializeField]
        private SystemConfigurationManager systemConfigurationManager = null;

        [Header("Objects and Data")]

        [SerializeField]
        private ObjectPooler objectPooler = null;

        [Header("Monobehavior Managers")]

        // sub manager monobehaviors

        [SerializeField]
        private CameraManager cameraManager = null;

        [SerializeField]
        private AudioManager audioManager = null;

        [SerializeField]
        private PetPreviewManager petPreviewManager = null;

        [SerializeField]
        private UnitPreviewManager unitPreviewManager = null;

        [SerializeField]
        private CharacterPanelManager characterPanelManager = null;

        [SerializeField]
        private CharacterCreatorManager characterCreatorManager = null;

        [SerializeField]
        private SystemAbilityController systemAbilityController = null;

        [SerializeField]
        private CastTargettingManager castTargettingManager = null;

        [SerializeField]
        private InputManager inputManager = null;

        [SerializeField]
        private PlayerManager playerManager = null;

        [SerializeField]
        private UIManager uIManager = null;

        [SerializeField]
        private NetworkManagerClient networkManagerClient = null;

        [SerializeField]
        private NetworkManagerServer networkManagerServer = null;

        // system scripts
        private AuthenticationService authenticationService = new AuthenticationService();
        private AuctionManagerClient auctionManagerClient = new AuctionManagerClient();
        private AuctionManagerServer auctionManagerServer = new AuctionManagerServer();
        private AuctionService auctionService = new AuctionService();
        private CharacterAppearanceManagerClient characterAppearanceManagerClient = new CharacterAppearanceManagerClient();
        private CharacterAppearanceManagerServer characterAppearanceManagerServer = new CharacterAppearanceManagerServer();
        private CharacterGroupServiceClient characterGroupServiceClient = new CharacterGroupServiceClient();
        private CharacterGroupServiceServer characterGroupServiceServer = new CharacterGroupServiceServer();
        private CharacterManager characterManager = new CharacterManager();
        private ChatCommandManager chatCommandManager = new ChatCommandManager();
        private ClassChangeManagerClient classChangeManagerClient = new ClassChangeManagerClient();
        private ClassChangeManagerServer classChangeManagerServer = new ClassChangeManagerServer();
        private ContextMenuService contextMenuService = new ContextMenuService();
        private ControlsManager controlsManager = new ControlsManager();
        private CraftingManager craftingManager = new CraftingManager();
        private CurrencyConverter currencyConverter = new CurrencyConverter();
        private DialogManagerClient dialogManagerClient = new DialogManagerClient();
        private DialogManagerServer dialogManagerServer = new DialogManagerServer();
        private FactionChangeManagerClient factionChangeManagerClient = new FactionChangeManagerClient();
        private FactionChangeManagerServer factionChangeManagerServer = new FactionChangeManagerServer();
        private FriendServiceClient friendServiceClient = new FriendServiceClient();
        private FriendServiceServer friendServiceServer = new FriendServiceServer();
        private ServerDataService serverDataService = new ServerDataService();
        private GuildmasterManagerClient guildmasterManagerClient = new GuildmasterManagerClient();
        private GuildmasterManagerServer guildmasterManagerServer = new GuildmasterManagerServer();
        private GuildServiceClient guildServiceClient = new GuildServiceClient();
        private GuildServiceServer guildServiceServer = new GuildServiceServer();
        private InspectCharacterService inspectCharacterService = new InspectCharacterService();
        private InteractionManager interactionManager = new InteractionManager();
        private KeyBindManager keyBindManager = new KeyBindManager();
        private LevelManager levelManager = new LevelManager();
        private LevelManagerServer levelManagerServer = new LevelManagerServer();
        private LoadGameManager loadGameManager = new LoadGameManager();
        private MessageLogClient messageLogClient = new MessageLogClient();
        private MessageLogServer messageLogServer = new MessageLogServer();
        private LootManager lootManager = new LootManager();
        private MusicPlayerManager musicPlayerManager = new MusicPlayerManager();
        private MailboxManagerClient mailboxManagerClient = new MailboxManagerClient();
        private MailboxManagerServer mailboxManagerServer = new MailboxManagerServer();
        private NameChangeManagerClient nameChangeManagerClient = new NameChangeManagerClient();
        private NameChangeManagerServer nameChangeManagerServer = new NameChangeManagerServer();
        private NewGameManager newGameManager = new NewGameManager();
        private PlayerCharacterService playerCharacterService = new PlayerCharacterService();
        private MailService mailService = new MailService();
        private PlayerManagerServer playerManagerServer = new PlayerManagerServer();
        private QuestGiverManagerClient questGiverManagerClient = new QuestGiverManagerClient();
        private QuestGiverManagerServer questGiverManagerServer = new QuestGiverManagerServer();
        private SaveManager saveManager = new SaveManager();
        //private LocalGameServerClient localGameServerClient = new LocalGameServerClient();
        private SkillTrainerManagerClient skillTrainerManagerClient = new SkillTrainerManagerClient();
        private SkillTrainerManagerServer skillTrainerManagerServer = new SkillTrainerManagerServer();
        private SpecializationChangeManagerClient specializationChangeManagerClient = new SpecializationChangeManagerClient();
        private SpecializationChangeManagerServer specializationChangeManagerServer = new SpecializationChangeManagerServer();
        private SystemAchievementManager systemAchievementManager = new SystemAchievementManager();
        private SystemDataFactory systemDataFactory = new SystemDataFactory();
        private SystemEnvironmentManager systemEnvironmentManager = new SystemEnvironmentManager();
        private SystemEventManager systemEventManager = new SystemEventManager();
        private SystemItemManager systemItemManager = new SystemItemManager();
        private SystemPlayableDirectorManager systemPlayableDirectorManager = new SystemPlayableDirectorManager();
        private TimeOfDayManagerClient timeOfDayManagerClient = new TimeOfDayManagerClient();
        private TimeOfDayManagerServer timeOfDayManagerServer = new TimeOfDayManagerServer();
        private TradeServiceClient tradeServiceClient = new TradeServiceClient();
        private TradeServiceServer tradeServiceServer = new TradeServiceServer();
        private UnitSpawnManager unitSpawnManager = new UnitSpawnManager();
        private UserAccountService userAccountService = new UserAccountService();
        private VendorManagerClient vendorManagerClient = new VendorManagerClient();
        private VendorManagerServer vendorManagerServer = new VendorManagerServer();
        private WeatherManagerClient weatherManagerClient = new WeatherManagerClient();
        private WeatherManagerServer weatherManagerServer = new WeatherManagerServer();
        private WindowManager windowManager = new WindowManager();

        // application state
        private int spawnCount = 0;
        private bool disconnectingNetworkForShutdown = false;
        private static bool isShuttingDown = false;

        public static bool IsShuttingDown { get => isShuttingDown; }
        public bool DisconnectingNetworkForShutdown { get => disconnectingNetworkForShutdown; set => disconnectingNetworkForShutdown = value; }

        public SystemEventManager SystemEventManager { get => systemEventManager; }
        public AuthenticationService AuthenticationService { get => authenticationService; }
        public UserAccountService UserAccountService { get => userAccountService; }
        public PlayerCharacterService PlayerCharacterService { get => playerCharacterService; }
        public MailService MailService { get => mailService; }
        public SystemEnvironmentManager SystemEnvironmentManager { get => systemEnvironmentManager; set => systemEnvironmentManager = value; }
        public CraftingManager CraftingManager { get => craftingManager; set => craftingManager = value; }
        public InteractionManager InteractionManager { get => interactionManager; set => interactionManager = value; }
        public LootManager LootManager { get => lootManager; set => lootManager = value; }
        public SystemPlayableDirectorManager SystemPlayableDirectorManager { get => systemPlayableDirectorManager; set => systemPlayableDirectorManager = value; }
        public SaveManager SaveManager { get => saveManager; }
        public KeyBindManager KeyBindManager { get => keyBindManager; set => keyBindManager = value; }

        public SystemConfigurationManager SystemConfigurationManager { get => systemConfigurationManager; set => systemConfigurationManager = value; }
        public AuctionManagerClient AuctionManagerClient { get => auctionManagerClient; set => auctionManagerClient = value; }
        public AuctionManagerServer AuctionManagerServer { get => auctionManagerServer; set => auctionManagerServer = value; }
        public AuctionService AuctionService { get => auctionService; }
        public ControlsManager ControlsManager { get => controlsManager; }
        public WindowManager WindowManager { get => windowManager; set => windowManager = value; }
        public CameraManager CameraManager { get => cameraManager; set => cameraManager = value; }
        public AudioManager AudioManager { get => audioManager; set => audioManager = value; }
        public PetPreviewManager PetPreviewManager { get => petPreviewManager; set => petPreviewManager = value; }
        public UnitPreviewManager UnitPreviewManager { get => unitPreviewManager; set => unitPreviewManager = value; }
        public CharacterPanelManager CharacterPanelManager { get => characterPanelManager; set => characterPanelManager = value; }
        public CharacterCreatorManager CharacterCreatorManager { get => characterCreatorManager; set => characterCreatorManager = value; }
        public SystemAchievementManager SystemAchievementManager { get => systemAchievementManager; set => systemAchievementManager = value; }
        public UIManager UIManager { get => uIManager; set => uIManager = value; }
        public SystemAbilityController SystemAbilityController { get => systemAbilityController; set => systemAbilityController = value; }
        public CastTargettingManager CastTargettingManager { get => castTargettingManager; set => castTargettingManager = value; }
        public InputManager InputManager { get => inputManager; set => inputManager = value; }
        public LevelManager LevelManager { get => levelManager; }
        public LevelManagerServer LevelManagerServer { get => levelManagerServer; set => levelManagerServer = value; }
        public PlayerManager PlayerManager { get => playerManager; set => playerManager = value; }
        public PlayerManagerServer PlayerManagerServer { get => playerManagerServer; set => playerManagerServer = value; }
        public SystemItemManager SystemItemManager { get => systemItemManager; set => systemItemManager = value; }
        public MessageLogClient MessageLogClient { get => messageLogClient; set => messageLogClient = value; }
        public MessageLogServer MessageLogServer { get => messageLogServer; set => messageLogServer = value; }
        public ObjectPooler ObjectPooler { get => objectPooler; set => objectPooler = value; }
        public SystemDataFactory SystemDataFactory { get => systemDataFactory; set => systemDataFactory = value; }
        public NewGameManager NewGameManager { get => newGameManager; set => newGameManager = value; }
        public LoadGameManager LoadGameManager { get => loadGameManager; set => loadGameManager = value; }
        public CurrencyConverter CurrencyConverter { get => currencyConverter; set => currencyConverter = value; }
        public ChatCommandManager ChatCommandManager { get => chatCommandManager; set => chatCommandManager = value; }
        public TimeOfDayManagerClient TimeOfDayManagerClient { get => timeOfDayManagerClient; set => timeOfDayManagerClient = value; }
        public TimeOfDayManagerServer TimeOfDayManagerServer { get => timeOfDayManagerServer; set => timeOfDayManagerServer = value; }
        public TradeServiceClient TradeServiceClient { get => tradeServiceClient; set => tradeServiceClient = value; }
        public TradeServiceServer TradeServiceServer { get => tradeServiceServer; set => tradeServiceServer = value; }
        public WeatherManagerClient WeatherManagerClient { get => weatherManagerClient; set => weatherManagerClient = value; }
        public WeatherManagerServer WeatherManagerServer { get => weatherManagerServer; set => weatherManagerServer = value; }
        public DialogManagerClient DialogManagerClient { get => dialogManagerClient; set => dialogManagerClient = value; }
        public DialogManagerServer DialogManagerServer { get => dialogManagerServer; set => dialogManagerServer = value; }
        public ClassChangeManagerClient ClassChangeManager { get => classChangeManagerClient; set => classChangeManagerClient = value; }
        public ClassChangeManagerServer ClassChangeManagerServer { get => classChangeManagerServer; set => classChangeManagerServer = value; }
        public ContextMenuService ContextMenuService { get => contextMenuService; set => contextMenuService = value; }
        public FactionChangeManagerClient FactionChangeManagerClient { get => factionChangeManagerClient; set => factionChangeManagerClient = value; }
        public FactionChangeManagerServer FactionChangeManagerServer { get => factionChangeManagerServer; set => factionChangeManagerServer = value; }
        public GuildmasterManagerClient GuildmasterManagerClient { get => guildmasterManagerClient; set => guildmasterManagerClient = value; }
        public GuildmasterManagerServer GuildmasterManagerServer { get => guildmasterManagerServer; set => guildmasterManagerServer = value; }
        public InspectCharacterService InspectCharacterService { get => inspectCharacterService; set => inspectCharacterService = value; }
        public SpecializationChangeManagerClient SpecializationChangeManagerClient { get => specializationChangeManagerClient; set => specializationChangeManagerClient = value; }
        public SpecializationChangeManagerServer SpecializationChangeManagerServer { get => specializationChangeManagerServer; set => specializationChangeManagerServer = value; }
        public MusicPlayerManager MusicPlayerManager { get => musicPlayerManager; set => musicPlayerManager = value; }
        public MailboxManagerClient MailboxManagerClient { get => mailboxManagerClient; set => mailboxManagerClient = value; }
        public MailboxManagerServer MailboxManagerServer { get => mailboxManagerServer; set => mailboxManagerServer = value; }
        public NameChangeManagerClient NameChangeManagerClient { get => nameChangeManagerClient; set => nameChangeManagerClient = value; }
        public NameChangeManagerServer NameChangeManagerServer { get => nameChangeManagerServer; set => nameChangeManagerServer = value; }
        public SkillTrainerManagerClient SkillTrainerManagerClient { get => skillTrainerManagerClient; set => skillTrainerManagerClient = value; }
        public SkillTrainerManagerServer SkillTrainerManagerServer { get => skillTrainerManagerServer; set => skillTrainerManagerServer = value; }
        public UnitSpawnManager UnitSpawnManager { get => unitSpawnManager; set => unitSpawnManager = value; }
        public VendorManagerClient VendorManagerClient { get => vendorManagerClient; set => vendorManagerClient = value; }
        public VendorManagerServer VendorManagerServer { get => vendorManagerServer; set => vendorManagerServer = value; }
        public CharacterAppearanceManagerClient CharacterAppearanceManagerClient { get => characterAppearanceManagerClient; set => characterAppearanceManagerClient = value; }
        public CharacterAppearanceManagerServer CharacterAppearanceManagerServer { get => characterAppearanceManagerServer; set => characterAppearanceManagerServer = value; }
        public NetworkManagerClient NetworkManagerClient { get => networkManagerClient; set => networkManagerClient = value; }
        public NetworkManagerServer NetworkManagerServer { get => networkManagerServer; set => networkManagerServer = value; }
        public CharacterManager CharacterManager { get => characterManager; set => characterManager = value; }
        public GameMode GameMode { get => gameMode; }
        public QuestGiverManagerClient QuestGiverManagerClient { get => questGiverManagerClient; set => questGiverManagerClient = value; }
        public QuestGiverManagerServer QuestGiverManagerServer { get => questGiverManagerServer; set => questGiverManagerServer = value; }
        public CharacterGroupServiceClient CharacterGroupServiceClient { get => characterGroupServiceClient; set => characterGroupServiceClient = value; }
        public CharacterGroupServiceServer CharacterGroupServiceServer { get => characterGroupServiceServer; set => characterGroupServiceServer = value; }
        public GuildServiceClient GuildServiceClient { get => guildServiceClient; set => guildServiceClient = value; }
        public GuildServiceServer GuildServiceServer { get => guildServiceServer; set => guildServiceServer = value; }
        //public LocalGameServerClient LocalGameServerClient { get => localGameServerClient; set => localGameServerClient = value; }
        public FriendServiceClient FriendServiceClient { get => friendServiceClient; set => friendServiceClient = value; }
        public FriendServiceServer FriendServiceServer { get => friendServiceServer; set => friendServiceServer = value; }
        public ServerDataService ServerDataService { get => serverDataService; set => serverDataService = value; }

        private void Awake() {
            Init();
        }

        private void Start() {
            //Debug.Log("SystemGameManager.Start()");

            // due to "intended" but not officially documented behavior, audio updates will be overwritten if called in Awake() so they must be called in Start()
            // https://fogbugz.unity3d.com/default.asp?1197165_nik4gg1io942ae13#bugevent_1071843210

            audioManager.Configure(this);

            // first turn off the UI
            UIManager.PerformSetupActivities();

            // then launch level manager to start loading the game
            LevelManager.PerformSetupActivities();

        }

        private void Init() {
            //Debug.Log("SystemGameManager.Init()");
            SetupPermanentObjects();

            // we are going to handle the initialization of all system managers here so we can control the start order and it isn't random
            // things are initialized here instead of in their declarations to prevent them from being initialized in the Unity Editor and restrict to play mode
            // initialize event manager first because everything else uses it
            systemEventManager = new SystemEventManager();

            // system data factory next for access to data resources
            SystemDataFactory.Configure(this);

            // configuration manager next because it will need access to resources from the factory
            systemConfigurationManager.Configure(this);

            // then everything else that relies on system configuration and data resources
            objectPooler.Configure(this);

            // configure services and managers
            serverDataService.Configure(this);
            userAccountService.Configure(this);
            playerCharacterService.Configure(this);
            auctionService.Configure(this);
            mailService.Configure(this);
            authenticationService.Configure(this);
            controlsManager.Configure(this);
            windowManager.Configure(this);
            cameraManager.Configure(this);
            //audioManager.Configure(this);
            petPreviewManager.Configure(this);
            unitPreviewManager.Configure(this);
            characterPanelManager.Configure(this);
            characterCreatorManager.Configure(this);
            systemAbilityController.Configure(this);
            castTargettingManager.Configure(this);
            inputManager.Configure(this);
            levelManager.Configure(this);
            levelManagerServer.Configure(this);
            playerManager.Configure(this);
            playerManagerServer.Configure(this);
            systemItemManager.Configure(this);
            systemAchievementManager.Configure(this);
            messageLogClient.Configure(this);
            messageLogServer.Configure(this);
            newGameManager.Configure(this);
            loadGameManager.Configure(this);
            saveManager.Configure(this);
            KeyBindManager.Configure(this);
            systemEnvironmentManager.Configure(this);
            craftingManager.Configure(this);
            interactionManager.Configure(this);
            lootManager.Configure(this);
            systemPlayableDirectorManager.Configure(this);
            uIManager.Configure(this);
            currencyConverter.Configure(this);
            chatCommandManager.Configure(this);
            timeOfDayManagerServer.Configure(this);
            timeOfDayManagerClient.Configure(this);
            tradeServiceClient.Configure(this);
            tradeServiceServer.Configure(this);
            weatherManagerClient.Configure(this);
            weatherManagerServer.Configure(this);
            dialogManagerClient.Configure(this);
            dialogManagerServer.Configure(this);
            classChangeManagerClient.Configure(this);
            classChangeManagerServer.Configure(this);
            contextMenuService.Configure(this);
            factionChangeManagerClient.Configure(this);
            factionChangeManagerServer.Configure(this);
            guildmasterManagerClient.Configure(this);
            guildmasterManagerServer.Configure(this);
            inspectCharacterService.Configure(this);
            specializationChangeManagerClient.Configure(this);
            specializationChangeManagerServer.Configure(this);
            musicPlayerManager.Configure(this);
            auctionManagerClient.Configure(this);
            auctionManagerServer.Configure(this);
            mailboxManagerClient.Configure(this);
            mailboxManagerServer.Configure(this);
            nameChangeManagerClient.Configure(this);
            nameChangeManagerServer.Configure(this);
            skillTrainerManagerClient.Configure(this);
            skillTrainerManagerServer.Configure(this);
            questGiverManagerClient.Configure(this);
            questGiverManagerServer.Configure(this);
            unitSpawnManager.Configure(this);
            vendorManagerClient.Configure(this);
            vendorManagerServer.Configure(this);
            characterAppearanceManagerClient.Configure(this);
            characterAppearanceManagerServer.Configure(this);
            networkManagerClient.Configure(this);
            networkManagerServer.Configure(this);
            characterManager.Configure(this);
            characterGroupServiceClient.Configure(this);
            characterGroupServiceServer.Configure(this);
            guildServiceClient.Configure(this);
            guildServiceServer.Configure(this);
            //localGameServerClient.Configure(this);
            friendServiceClient.Configure(this);
            friendServiceServer.Configure(this);
        }

        private void Update() {
            timeOfDayManagerServer.Tick();
            controlsManager.Update();
        }

        private void SetupPermanentObjects() {
            //Debug.Log("SystemGameManager.SetupPermanentObjects()");
            DontDestroyOnLoad(this.gameObject);
            GameObject umaDCS = GameObject.Find("UMA_GLIB");
            if (umaDCS == null) {
                umaDCS = GameObject.Find("UMA_DCS");
                if (umaDCS != null) {
                    Debug.LogWarning("SystemGameManager.SetupPermanentObjects(): UMA_DCS is deprecated.  Please replace the UMA_DCS prefab with the UMA_GLIB prefab");
                }
            }
            if (umaDCS == null) {
                //Debug.Log("SystemGameManager.SetupPermanentObjects(): Neither UMA_GLIB nor UMA_DCS prefab could be found in the scene. UMA will be unavailable");
            } else {
                DontDestroyOnLoad(umaDCS);
            }
        }

        public int GetSpawnCount() {
            spawnCount++;
            return spawnCount;
        }

        private void OnApplicationQuit() {
            //Debug.Log("SystemGameManager.OnApplicationQuit()");
            isShuttingDown = true;
        }

        public void SetGameMode(GameMode gameMode) {
            //Debug.Log($"SystemGameManager.SetGameMode({gameMode})");

            this.gameMode = gameMode;
            // set physics simulation off for network mode
            /*
            if (gameMode == GameMode.Network) {
                Physics.simulationMode = SimulationMode.Script;
            } else {
                Physics.simulationMode = SimulationMode.FixedUpdate;
            }
            */
            //networkManagerServer.OnSetGameMode(gameMode);
        }

        /// <summary>
        /// configure all classes of type AutoConfiguredMonoBehavior in the scene
        /// </summary>
        public void AutoConfigureMonoBehaviours(Scene scene) {
            //Debug.Log($"SystemGameManager.AutoConfigureMonoBehaviours()");

            foreach (AutoConfiguredMonoBehaviour autoConfiguredMonoBehaviour in GameObject.FindObjectsByType<AutoConfiguredMonoBehaviour>(FindObjectsSortMode.None)) {

                if (autoConfiguredMonoBehaviour.gameObject.scene == scene) {
                    autoConfiguredMonoBehaviour.AutoConfigure(this);
                } else {
                    //Debug.Log($"SystemGameManager.AutoConfigureMonoBehaviours(): {autoConfiguredMonoBehaviour.gameObject.name} not in scene");
                }
            }
        }

        public void RequestExitGame() {
            if (gameMode == GameMode.Network) {
                disconnectingNetworkForShutdown = true;
                networkManagerClient.RequestLogout();
                return;
            }
            ExitGame();
        }

        public void ExitGame() {
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public enum GameMode { Local, Network }

}