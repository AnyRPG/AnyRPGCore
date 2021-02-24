using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace AnyRPG {
    public class SystemConfigurationManager : MonoBehaviour, IStatProvider, ICapabilityProvider {

        #region Singleton
        private static SystemConfigurationManager instance;

        public static SystemConfigurationManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemConfigurationManager>();
                }

                return instance;
            }
        }
        #endregion

        [Header("GAME CONFIGURATION")]

        [SerializeField]
        private string gameName;

        [SerializeField]
        private string gameVersion;

        [SerializeField]
        private List<string> loadResourcesFolders = new List<string>();

        [Header("Scenes")]

        [Tooltip("The name of the scene that loads the game manager into memory, and then proceeds to the main menu")]
        [SerializeField]
        private string initializationScene = "Core Game";

        private SceneNode initializationSceneNode = null;

        [Tooltip("The name of the main menu scene")]
        [SerializeField]
        private string mainMenuScene = "Main Menu";

        // reference to the main menu scene node
        private SceneNode mainMenuSceneNode = null;

        [Tooltip("When a new game is started, the character will initially spawn in this scene if no scene is provided by their faction")]
        [SerializeField]
        private string defaultStartingZone = string.Empty;

        [Header("NEW GAME OPTIONS")]

        [Tooltip("The default name for the character creator, and the default to start with if no character creator is used.")]
        [SerializeField]
        private string defaultPlayerName = "New Player";

        [Tooltip("If false, launch straight into a game with no character configuration")]
        [SerializeField]
        private bool useNewGameWindow = true;

        [Tooltip("If the new game window is used, show the appearance tab")]
        [SerializeField]
        private bool newGameAppearance = true;

        [Tooltip("If the appearance tab is used, show the UMA version of the character customizer")]
        [SerializeField]
        private bool newGameUMAAppearance = true;

        [Tooltip("If the new game window is used, show the class tab")]
        [SerializeField]
        private bool newGameClass = true;

        [Tooltip("If the new game window is used, show the faction tab")]
        [SerializeField]
        private bool newGameFaction = true;

        [Tooltip("If the new game window is used, show the specialiation tab")]
        [SerializeField]
        private bool newGameSpecialization = true;

        [Tooltip("The name of the audio profile to play when the new game window is active")]
        [SerializeField]
        private string newGameAudio = string.Empty;

        private AudioProfile newGameAudioProfile = null;

        [Tooltip("If the character creator is not used, this unit will be the default player unit. Usually a non UMA mecanim Unit or pre-configured UMA unit.")]
        [SerializeField]
        private string defaultPlayerUnitProfileName = string.Empty;

        [Tooltip("If true, the default profiles will always be shown, in addition to any allowed by faction (if used)")]
        [SerializeField]
        private bool alwaysShowDefaultProfiles = true;

        [Tooltip("The options available when the character creator is used")]
        [SerializeField]
        private List<string> characterCreatorProfileNames = new List<string>();

        // reference to the default profile
        private UnitProfile defaultPlayerUnitProfile = null;

        // reference to the default profile
        private List<UnitProfile> characterCreatorProfiles = new List<UnitProfile>();

        [Header("In Game Character Creator")]

        [Tooltip("If true, when the character creator is used in-game, the character will be forced to use the first character creator profile, rather than their current model")]
        [SerializeField]
        private bool useFirstCreatorProfile = false;

        [Header("Inventory")]

        [Tooltip("if false, default backpack goes in bank")]
        [SerializeField]
        private bool equipDefaultBackPack = true;

        [SerializeField]
        private string defaultBackpackItem = "Backpack";

        [SerializeField]
        private string defaultBankBagItem = "Bank";

        [Header("CONTROLLER")]

        [Tooltip("When not mounted, disable native movement input to allow a third party controller (such as Invector) to move the character")]
        [SerializeField]
        private bool useThirdPartyMovementControl = false;

        [Tooltip("If a third party movement controller is used, disable this to prevent movement lock in combat")]
        [SerializeField]
        private bool allowAutoAttack = true;

        [Header("CAMERA")]

        [Tooltip("Use a third party camera (such as Invector) to follow the character.  Built-in camera will still be used for menus and cutscenes.")]
        [SerializeField]
        private bool useThirdPartyCameraControl = false;

        [Tooltip("A reference to the third party camera prefab to be used")]
        [SerializeField]
        private GameObject thirdPartyCamera = null;

       

        [Header("ANIMATION")]

        [Tooltip("This profile should contain references to all the default animations that are on the default animation controller so the system knows which animations to replace when overriding them.")]
        [SerializeField]
        private AnimationProfile systemAnimationProfile;

        [Header("CHARACTER ANIMATION CONFIGURATION")]

        [FormerlySerializedAs("defaultAttackAnimationProfile")]
        [SerializeField]
        private AnimationProfile defaultAnimationProfile;

        [SerializeField]
        private RuntimeAnimatorController defaultAnimatorController;

        [Header("Level Values")]

        [Tooltip("The character cannot level up past this level")]
        [SerializeField]
        private int maxLevel = 50;

        [Tooltip("Every level, the amount of experience you need for the next level is increased by this amount")]
        [SerializeField]
        private int xpRequiredPerLevel = 100;

        [Header("Currency")]

        [Tooltip("When showing the total currency the player has in the vendor window, this currency will be used")]
        [SerializeField]
        private string currencyGroupName = string.Empty;

        private CurrencyGroup defaultCurrencyGroup;

        [Tooltip("When selling an item to a vendor, the offered amount will be the regular purchase amount multiplied by this number.")]
        [SerializeField]
        private float vendorPriceMultiplier = 0.25f;


        [Header("Currency Scaling")]

        [Tooltip("If automatic currency is enabled for a lootable character, this currency will be dropped")]
        [SerializeField]
        private string killCurrencyName = string.Empty;

        private Currency killCurrency = null;

        [Tooltip("If automatic currency is enabled for a lootable character, this currency amount will be multiplied by the character level")]
        [SerializeField]
        private int killCurrencyAmountPerLevel = 1;

        [Tooltip("If automatic currency is enabled for a quest, this currency will be rewarded")]
        [SerializeField]
        private string questCurrencyName = string.Empty;

        private Currency questCurrency;

        [Tooltip("If automatic currency is enabled for a quest, this currency amount will be multiplied by the quest level")]
        [SerializeField]
        private int questCurrencyAmountPerLevel = 1;

        [Header("Quest Experience Scaling")]

        [Tooltip("A flat experience amount to add to all quests that does not scale with level")]
        [SerializeField]
        private int baseQuestXP = 0;

        [Tooltip("A flat experience amount to add to all quests that does not scale with level")]
        [SerializeField]
        private int questXPPerLevel = 100;

        [Tooltip("If true, the experience per level will be multiplied by (1 / level)")]
        [SerializeField]
        private bool useQuestXPLevelMultiplierDemoninator = true;

        [Tooltip("If the above option is true, and this value is more than 0, the experience per level will be multiplied by (1 / level)")]
        [SerializeField]
        private int questXPMultiplierLevelCap = 5;

        [Header("Kill Experience Scaling")]

        [Tooltip("A flat experience amount to add to all quests that does not scale with level")]
        [SerializeField]
        private int baseKillXP = 0;

        [Tooltip("A flat experience amount to add to all quests that does not scale with level")]
        [SerializeField]
        private int killXPPerLevel = 100;

        [Tooltip("If true, the experience per level will be multiplied by (1 / level)")]
        [SerializeField]
        private bool useKillXPLevelMultiplierDemoninator = true;

        [Tooltip("If the above option is true, and this value is more than 0, the experience per level will be multiplied by (1 / level)")]
        [SerializeField]
        private int killXPMultiplierLevelCap = 10;


        [Header("DPS Scaling")]

        [SerializeField]
        private float weaponDPSBudgetPerLevel = 2.5f;

        [Header("Primary Stats and Scaling")]

        [Tooltip("A Per level stat budget that will be applied to all stats, in addition to their individual budgets")]
        [SerializeField]
        private float statBudgetPerLevel = 0f;

        [Tooltip("Default stats that all characters will use, and their budgets per level")]
        [FormerlySerializedAs("statScaling")]
        [SerializeField]
        private List<StatScalingNode> primaryStats = new List<StatScalingNode>();

        [Header("Power Resources")]

        [Tooltip("Power Resources used by all characters.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        [Header("Capabilities")]

        [Tooltip("Capabilities that apply to all units")]
        [SerializeField]
        private CapabilityProps capabilities = new CapabilityProps();

        [Header("Layer")]

        [Tooltip("character units will automatically be set to this layer so they can respond to AOE / looting and other things that filter by this layer.")]
        [SerializeField]
        private string defaultCharacterUnitLayer = "CharacterUnit";

        [Tooltip("player units will automatically be set to this layer.")]
        [SerializeField]
        private string defaultPlayerUnitLayer = "Player";

        [Header("SYSTEM ABILITIES")]

        [Tooltip("The ability effect to cast on a player when they level up")]
        [FormerlySerializedAs("levelUpAbilityName")]
        [SerializeField]
        private string levelUpEffectName = string.Empty;

        private AbilityEffect levelUpEffect = null;

        [Tooltip("The ability effect to cast on a player when they die")]
        [FormerlySerializedAs("deathAbilityName")]
        [SerializeField]
        private string deathEffectName = string.Empty;

        private AbilityEffect deathEffect = null;

        [Tooltip("The ability effect to cast on any character when it has loot that can be collected")]
        [FormerlySerializedAs("lootSparkleAbilityName")]
        [SerializeField]
        private string lootSparkleEffectName = string.Empty;

        private AbilityEffect lootSparkleEffect = null;

        [Header("SYSTEM AUDIO")]

        [Tooltip("This audio will play whenever buying from or selling to a vendor")]
        [SerializeField]
        private string vendorAudioProfileName = string.Empty;

        [Tooltip("The maximum distance at which chat in dialogs above characters will also appear in the player chat log.  Prevents distant conversations from spamming logs.")]
        [SerializeField]
        private float maxChatTextDistance = 25f;

        private AudioProfile vendorAudioProfile = null;

        [Header("UI")]

        [SerializeField]
        private Material defaultCastTargetCircle;

        [FormerlySerializedAs("focusProjectorColorMap")]
        [SerializeField]
        private List<ProjectorColorMapNode> highlightCircleColorMap = new List<ProjectorColorMapNode>();

        [Tooltip("default UI color for static elements that have no additional transparency applied to them")]
        [SerializeField]
        private Color defaultUIColor;

        [Tooltip("defaultUIColor with full opacity for button frames")]
        [SerializeField]
        private Color defaultUISolidColor;

        [SerializeField]
        private Color defaultUIFillColor;

        [SerializeField]
        private Sprite defaultUIPanelFrame;

        [Tooltip("The faction icon to show on the load game screen when the player has no faction.")]
        [SerializeField]
        private Sprite defaultFactionIcon;

        [Header("SYSTEM BAR")]

        [SerializeField]
        private Sprite systemBarMainMenu;

        [SerializeField]
        private Sprite systemBarAbilityBook;

        [SerializeField]
        private Sprite systemBarCharacter;

        [SerializeField]
        private Sprite systemBarQuestLog;

        [SerializeField]
        private Sprite systemBarMap;

        [Header("INTERACTABLE CONFIGURATION")]

        [SerializeField]
        private Material temporaryMaterial = null;

        // an image to use if there is more than 1 valid interactable option
        [SerializeField]
        private Sprite multipleInteractionNamePlateImage = null;

        // a separate image if only crafting is available, but more than 1 craft skill can be shown
        [SerializeField]
        private Sprite multipleCraftNamePlateImage = null;

        [SerializeField]
        private Sprite bankInteractionPanelImage = null;

        [SerializeField]
        private Sprite bankNamePlateImage = null;

        [SerializeField]
        private Sprite questGiverInteractionPanelImage = null;

        [SerializeField]
        private Sprite questGiverNamePlateImage = null;

        [SerializeField]
        private Sprite dialogInteractionPanelImage = null;

        [SerializeField]
        private Sprite dialogNamePlateImage = null;

        [SerializeField]
        private Sprite nameChangeInteractionPanelImage = null;

        [SerializeField]
        private Sprite nameChangeNamePlateImage = null;

        [SerializeField]
        private Sprite cutSceneInteractionPanelImage = null;

        [SerializeField]
        private Sprite cutSceneNamePlateImage = null;

        [SerializeField]
        private Sprite lootableCharacterInteractionPanelImage = null;

        [SerializeField]
        private Sprite lootableCharacterNamePlateImage = null;

        [SerializeField]
        private Sprite characterCreatorInteractionPanelImage = null;

        [SerializeField]
        private Sprite characterCreatorNamePlateImage = null;

        [SerializeField]
        private Sprite unitSpawnControllerInteractionPanelImage = null;

        [SerializeField]
        private Sprite unitSpawnControllerNamePlateImage = null;

        [SerializeField]
        private Sprite factionChangeInteractionPanelImage = null;

        [SerializeField]
        private Sprite factionChangeNamePlateImage = null;

        [SerializeField]
        private Sprite classChangeInteractionPanelImage = null;

        [SerializeField]
        private Sprite classChangeNamePlateImage = null;

        [SerializeField]
        private Sprite vendorInteractionPanelImage = null;

        [SerializeField]
        private Sprite vendorNamePlateImage = null;

        [SerializeField]
        private Sprite portalInteractionPanelImage = null;

        [SerializeField]
        private Sprite portalNamePlateImage = null;

        [SerializeField]
        private Sprite skillTrainerInteractionPanelImage = null;

        [SerializeField]
        private Sprite skillTrainerNamePlateImage = null;

        [SerializeField]
        private Sprite musicPlayerInteractionPanelImage = null;

        [SerializeField]
        private Sprite musicPlayerNamePlateImage = null;

        // the default amount of time before a unit despawns after killed and looted
        [SerializeField]
        private float defaultDespawnTimer = 0f;

        protected bool eventSubscriptionsInitialized = false;

        public AbilityEffect LootSparkleEffect { get => lootSparkleEffect; set => lootSparkleEffect = value; }
        public Material TemporaryMaterial { get => temporaryMaterial; set => temporaryMaterial = value; }
        public AbilityEffect LevelUpEffect { get => levelUpEffect; set => levelUpEffect = value; }
        public Sprite QuestGiverInteractionPanelImage { get => questGiverInteractionPanelImage; set => questGiverInteractionPanelImage = value; }
        public Sprite QuestGiverNamePlateImage { get => questGiverNamePlateImage; set => questGiverNamePlateImage = value; }
        public Sprite DialogInteractionPanelImage { get => dialogInteractionPanelImage; set => dialogInteractionPanelImage = value; }
        public Sprite DialogNamePlateImage { get => dialogNamePlateImage; set => dialogNamePlateImage = value; }
        public Sprite NameChangeInteractionPanelImage { get => nameChangeInteractionPanelImage; set => nameChangeInteractionPanelImage = value; }
        public Sprite MyNameChangeNamePlateImage { get => nameChangeNamePlateImage; set => nameChangeNamePlateImage = value; }
        public Sprite CutSceneInteractionPanelImage { get => cutSceneInteractionPanelImage; set => cutSceneInteractionPanelImage = value; }
        public Sprite CutSceneNamePlateImage { get => cutSceneNamePlateImage; set => cutSceneNamePlateImage = value; }
        public Sprite LootableCharacterInteractionPanelImage { get => lootableCharacterInteractionPanelImage; set => lootableCharacterInteractionPanelImage = value; }
        public Sprite LootableCharacterNamePlateImage { get => lootableCharacterNamePlateImage; set => lootableCharacterNamePlateImage = value; }
        public Sprite CharacterCreatorInteractionPanelImage { get => characterCreatorInteractionPanelImage; set => characterCreatorInteractionPanelImage = value; }
        public Sprite CharacterCreatorNamePlateImage { get => characterCreatorNamePlateImage; set => characterCreatorNamePlateImage = value; }
        public Sprite FactionChangeInteractionPanelImage { get => factionChangeInteractionPanelImage; set => factionChangeInteractionPanelImage = value; }
        public Sprite FactionChangeNamePlateImage { get => factionChangeNamePlateImage; set => factionChangeNamePlateImage = value; }
        public Sprite ClassChangeInteractionPanelImage { get => classChangeInteractionPanelImage; set => classChangeInteractionPanelImage = value; }
        public Sprite ClassChangeNamePlateImage { get => classChangeNamePlateImage; set => classChangeNamePlateImage = value; }
        public Sprite PortalInteractionPanelImage { get => portalInteractionPanelImage; set => portalInteractionPanelImage = value; }
        public Sprite PortalNamePlateImage { get => portalNamePlateImage; set => portalNamePlateImage = value; }
        public Sprite SkillTrainerInteractionPanelImage { get => skillTrainerInteractionPanelImage; set => skillTrainerInteractionPanelImage = value; }
        public Sprite SkillTrainerNamePlateImage { get => skillTrainerNamePlateImage; set => skillTrainerNamePlateImage = value; }
        public AbilityEffect DeathEffect { get => deathEffect; set => deathEffect = value; }
        public Sprite MultipleInteractionNamePlateImage { get => multipleInteractionNamePlateImage; set => multipleInteractionNamePlateImage = value; }
        public float DefaultDespawnTimer { get => defaultDespawnTimer; set => defaultDespawnTimer = value; }
        public Sprite BankInteractionPanelImage { get => bankInteractionPanelImage; set => bankInteractionPanelImage = value; }
        public Sprite BankNamePlateImage { get => bankNamePlateImage; set => bankNamePlateImage = value; }
        public Sprite VendorInteractionPanelImage { get => vendorInteractionPanelImage; set => vendorInteractionPanelImage = value; }
        public Sprite VendorNamePlateImage { get => vendorNamePlateImage; set => vendorNamePlateImage = value; }
        public Sprite MultipleCraftNamePlateImage { get => multipleCraftNamePlateImage; set => multipleCraftNamePlateImage = value; }
        public string GameName { get => gameName; set => gameName = value; }
        public string GameVersion { get => gameVersion; set => gameVersion = value; }
        public Sprite SystemBarMainMenu { get => systemBarMainMenu; set => systemBarMainMenu = value; }
        public Sprite SystemBarAbilityBook { get => systemBarAbilityBook; set => systemBarAbilityBook = value; }
        public Sprite SystemBarCharacter { get => systemBarCharacter; set => systemBarCharacter = value; }
        public Sprite SystemBarQuestLog { get => systemBarQuestLog; set => systemBarQuestLog = value; }
        public Sprite SystemBarMap { get => systemBarMap; set => systemBarMap = value; }
        public Sprite UnitSpawnControllerInteractionPanelImage { get => unitSpawnControllerInteractionPanelImage; set => unitSpawnControllerInteractionPanelImage = value; }
        public Sprite UnitSpawnControllerNamePlateImage { get => unitSpawnControllerNamePlateImage; set => unitSpawnControllerNamePlateImage = value; }
        public Sprite MusicPlayerInteractionPanelImage { get => musicPlayerInteractionPanelImage; set => musicPlayerInteractionPanelImage = value; }
        public Sprite MusicPlayerNamePlateImage { get => musicPlayerNamePlateImage; set => musicPlayerNamePlateImage = value; }
        public RuntimeAnimatorController DefaultAnimatorController { get => defaultAnimatorController; set => defaultAnimatorController = value; }
        public AnimationProfile DefaultAnimationProfile { get => defaultAnimationProfile; set => defaultAnimationProfile = value; }
        public Material DefaultCastingLightProjector { get => defaultCastTargetCircle; set => defaultCastTargetCircle = value; }
        public Color DefaultUIColor { get => defaultUIColor; set => defaultUIColor = value; }
        public Color DefaultUIFillColor { get => defaultUIFillColor; set => defaultUIFillColor = value; }
        public Color DefaultUISolidColor { get => defaultUISolidColor; set => defaultUISolidColor = value; }
        public List<string> LoadResourcesFolders { get => loadResourcesFolders; set => loadResourcesFolders = value; }
        public int MaxLevel { get => maxLevel; set => maxLevel = value; }
        public float StatBudgetPerLevel { get => statBudgetPerLevel; set => statBudgetPerLevel = value; }
        public CurrencyGroup DefaultCurrencyGroup { get => defaultCurrencyGroup; set => defaultCurrencyGroup = value; }
        public float VendorPriceMultiplier { get => vendorPriceMultiplier; set => vendorPriceMultiplier = value; }
        public float WeaponDPSBudgetPerLevel { get => weaponDPSBudgetPerLevel; set => weaponDPSBudgetPerLevel = value; }
        public string DefaultCharacterUnitLayer { get => defaultCharacterUnitLayer; set => defaultCharacterUnitLayer = value; }
        public AnimationProfile SystemAnimationProfile { get => systemAnimationProfile; set => systemAnimationProfile = value; }
        public List<ProjectorColorMapNode> FocusProjectorColorMap { get => highlightCircleColorMap; set => highlightCircleColorMap = value; }
        public bool UseThirdPartyMovementControl { get => useThirdPartyMovementControl; set => useThirdPartyMovementControl = value; }
        public bool UseThirdPartyCameraControl { get => useThirdPartyCameraControl; set => useThirdPartyCameraControl = value; }
        public bool AllowAutoAttack { get => allowAutoAttack; set => allowAutoAttack = value; }
        public int XpRequiredPerLevel { get => xpRequiredPerLevel; set => xpRequiredPerLevel = value; }
        public int BaseQuestXP { get => baseQuestXP; set => baseQuestXP = value; }
        public int QuestXPPerLevel { get => questXPPerLevel; set => questXPPerLevel = value; }
        public bool UseQuestXPLevelMultiplierDemoninator { get => useQuestXPLevelMultiplierDemoninator; set => useQuestXPLevelMultiplierDemoninator = value; }
        public int QuestXPMultiplierLevelCap { get => questXPMultiplierLevelCap; set => questXPMultiplierLevelCap = value; }
        public int BaseKillXP { get => baseKillXP; set => baseKillXP = value; }
        public int KillXPPerLevel { get => killXPPerLevel; set => killXPPerLevel = value; }
        public bool UseKillXPLevelMultiplierDemoninator { get => useKillXPLevelMultiplierDemoninator; set => useKillXPLevelMultiplierDemoninator = value; }
        public int KillXPMultiplierLevelCap { get => killXPMultiplierLevelCap; set => killXPMultiplierLevelCap = value; }
        public Sprite DefaultFactionIcon { get => defaultFactionIcon; set => defaultFactionIcon = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public List<string> PowerResources { get => powerResources; set => powerResources = value; }
        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public string KillCurrencyName { get => killCurrencyName; set => killCurrencyName = value; }
        public int KillCurrencyAmountPerLevel { get => killCurrencyAmountPerLevel; set => killCurrencyAmountPerLevel = value; }
        public string QuestCurrencyName { get => questCurrencyName; set => questCurrencyName = value; }
        public int QuestCurrencyAmountPerLevel { get => questCurrencyAmountPerLevel; set => questCurrencyAmountPerLevel = value; }
        public Currency KillCurrency { get => killCurrency; set => killCurrency = value; }
        public Currency QuestCurrency { get => questCurrency; set => questCurrency = value; }
        public AudioProfile VendorAudioProfile { get => vendorAudioProfile; set => vendorAudioProfile = value; }
        public float MaxChatTextDistance { get => maxChatTextDistance; set => maxChatTextDistance = value; }
        public bool UseNewGameWindow { get => useNewGameWindow; set => useNewGameWindow = value; }
        public bool NewGameAppearance { get => newGameAppearance; set => newGameAppearance = value; }
        public bool NewGameClass { get => newGameClass; set => newGameClass = value; }
        public bool NewGameFaction { get => newGameFaction; set => newGameFaction = value; }
        public bool NewGameSpecialization { get => newGameSpecialization; set => newGameSpecialization = value; }
        public AudioProfile NewGameAudioProfile { get => newGameAudioProfile; set => newGameAudioProfile = value; }
        public string DefaultPlayerName { get => defaultPlayerName; set => defaultPlayerName = value; }
        public string DefaultPlayerUnitProfileName { get => defaultPlayerUnitProfileName; set => defaultPlayerUnitProfileName = value; }
        public UnitProfile DefaultPlayerUnitProfile { get => defaultPlayerUnitProfile; set => defaultPlayerUnitProfile = value; }
        public string CharacterCreatorUnitProfileName {
            get {
                if (characterCreatorProfileNames.Count > 0) {
                    return characterCreatorProfileNames[0];
                }
                return null;
            }
        }
        public List<UnitProfile> CharacterCreatorProfiles { get => characterCreatorProfiles; set => characterCreatorProfiles = value; }
        public string DefaultStartingZone { get => defaultStartingZone; set => defaultStartingZone = value; }
        public SceneNode InitializationSceneNode { get => initializationSceneNode; set => initializationSceneNode = value; }
        public SceneNode MainMenuSceneNode { get => mainMenuSceneNode; set => mainMenuSceneNode = value; }

        public UnitProfile CharacterCreatorUnitProfile {
            get {
                if (characterCreatorProfiles != null && characterCreatorProfiles.Count > 0) {
                    return characterCreatorProfiles[0];
                }
                return null;
            }
        }

        public bool NewGameUMAAppearance { get => newGameUMAAppearance; set => newGameUMAAppearance = value; }
        public bool EquipDefaultBackPack { get => equipDefaultBackPack; set => equipDefaultBackPack = value; }
        public string DefaultPlayerUnitLayer { get => defaultPlayerUnitLayer; set => defaultPlayerUnitLayer = value; }
        public GameObject ThirdPartyCamera { get => thirdPartyCamera; set => thirdPartyCamera = value; }
        public string DefaultBackpackItem { get => defaultBackpackItem; set => defaultBackpackItem = value; }
        public string DefaultBankBagItem { get => defaultBankBagItem; set => defaultBankBagItem = value; }
        public bool AlwaysShowDefaultProfiles { get => alwaysShowDefaultProfiles; set => alwaysShowDefaultProfiles = value; }
        public string MainMenuScene { get => mainMenuScene; set => mainMenuScene = value; }
        public string InitializationScene { get => initializationScene; set => initializationScene = value; }
        public bool UseFirstCreatorProfile { get => useFirstCreatorProfile; set => useFirstCreatorProfile = value; }

        public CapabilityProps GetFilteredCapabilities(ICapabilityConsumer capabilityConsumer, bool returnAll = true) {
            return capabilities;
        }

        private void Start() {
            //Debug.Log("PlayerManager.Start()");
            CreateEventSubscriptions();
            //VerifySystemAbilities();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
        }

        public void PerformRequiredPropertyChecks() {
            if (defaultPlayerUnitProfileName == null || defaultPlayerUnitProfileName == string.Empty) {
                Debug.LogError("PlayerManager.Awake(): the default player unit profile name is null.  Please set it in the inspector");
            }
        }


        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        // verify that system abilities are available through the factory
        public void SetupScriptableObjects() {

            if (levelUpEffectName != null && levelUpEffectName != string.Empty) {
                AbilityEffect testAbility = SystemAbilityEffectManager.MyInstance.GetResource(levelUpEffectName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): " + levelUpEffectName + " could not be found in factory.  CHECK INSPECTOR");
                    return;
                } else {
                    levelUpEffect = testAbility;
                }
            }

            if (deathEffectName != null && deathEffectName != string.Empty) {
                AbilityEffect testAbility = SystemAbilityEffectManager.MyInstance.GetResource(deathEffectName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): " + deathEffectName + " could not be found in factory.  CHECK INSPECTOR");
                    return;
                } else {
                    deathEffect = testAbility;
                }
            }
            if (lootSparkleEffectName != null && lootSparkleEffectName != string.Empty) {
                AbilityEffect testAbility = SystemAbilityEffectManager.MyInstance.GetResource(lootSparkleEffectName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): " + lootSparkleEffectName + " could not be found in factory.  CHECK INSPECTOR");
                    return;
                } else {
                    lootSparkleEffect = testAbility;
                }
            }
            if (currencyGroupName != null && currencyGroupName != string.Empty) {
                CurrencyGroup tmpCurrencyGroup = SystemCurrencyGroupManager.MyInstance.GetResource(currencyGroupName);
                if (tmpCurrencyGroup == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): " + currencyGroupName + " could not be found in factory.  CHECK INSPECTOR");
                    return;
                } else {
                    defaultCurrencyGroup = tmpCurrencyGroup;
                }
            }

            if (defaultAnimationProfile == null) {
                Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): no default animation profile set.  CHECK INSPECTOR");
                return;
            }

            powerResourceList = new List<PowerResource>();
            if (powerResources != null) {
                foreach (string powerResourcename in powerResources) {
                    PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(powerResourcename);
                    if (tmpPowerResource != null) {
                        powerResourceList.Add(tmpPowerResource);
                    } else {
                        Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + ". CHECK INSPECTOR");
                    }
                }
            }

            if (KillCurrencyName != null && KillCurrencyName != string.Empty) {
                Currency tmpCurrency = SystemCurrencyManager.MyInstance.GetResource(KillCurrencyName);
                if (tmpCurrency != null) {
                    killCurrency = tmpCurrency;
                    //currencyNode.MyAmount = gainCurrencyAmount;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find currency : " + KillCurrencyName + ".  CHECK INSPECTOR");
                }
            }

            if (questCurrencyName != null && questCurrencyName != string.Empty) {
                Currency tmpCurrency = SystemCurrencyManager.MyInstance.GetResource(questCurrencyName);
                if (tmpCurrency != null) {
                    questCurrency = tmpCurrency;
                    //currencyNode.MyAmount = gainCurrencyAmount;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find currency : " + questCurrencyName + ".  CHECK INSPECTOR");
                }
            }

            foreach (StatScalingNode statScalingNode in primaryStats) {
                statScalingNode.SetupScriptableObjects();
            }

            capabilities.SetupScriptableObjects();

            if (vendorAudioProfileName != null && vendorAudioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = SystemAudioProfileManager.MyInstance.GetResource(vendorAudioProfileName);
                if (tmpAudioProfile != null) {
                    vendorAudioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find audio profile : " + vendorAudioProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }

            if (newGameAudio != null && newGameAudio != string.Empty) {
                AudioProfile tmpAudioProfile = SystemAudioProfileManager.MyInstance.GetResource(newGameAudio);
                if (tmpAudioProfile != null) {
                    newGameAudioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find audio profile : " + newGameAudio + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }

            // get default player unit profile
            if (defaultPlayerUnitProfileName != null && defaultPlayerUnitProfileName != string.Empty) {
                UnitProfile tmpUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(defaultPlayerUnitProfileName);
                if (tmpUnitProfile != null) {
                    defaultPlayerUnitProfile = tmpUnitProfile;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): could not find unit profile " + defaultPlayerUnitProfileName + ".  Check Inspector");
                }
            } else {
                Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): defaultPlayerUnitProfileName field is required, but not value was set.  Check Inspector");
            }

            // get default player unit profile
            if (characterCreatorProfileNames != null) {
                foreach (string characterCreatorProfileName in characterCreatorProfileNames) {
                    if (characterCreatorProfileName != null && characterCreatorProfileName != string.Empty) {
                        UnitProfile tmpUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(characterCreatorProfileName);
                        if (tmpUnitProfile != null) {
                            characterCreatorProfiles.Add(tmpUnitProfile);
                        } else {
                            Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): could not find unit profile " + characterCreatorProfileName + ".  Check Inspector");
                        }
                    } else {
                        Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): defaultPlayerUnitProfileName field is required, but not value was set.  Check Inspector");
                    }

                }
            }


            if (initializationScene != null && initializationScene != string.Empty) {
                SceneNode tmpSceneNode = SystemSceneNodeManager.MyInstance.GetResource(initializationScene);
                if (tmpSceneNode != null) {
                    initializationSceneNode = tmpSceneNode;
                }
                // it shouldn't be required to have this scene node
                /* else {
                    Debug.LogError("LevelManager.SetupScriptableObjects: could not find scene node " + initializationScene + ". Check inspector.");
                }*/
            }

            if (mainMenuScene != null && mainMenuScene != string.Empty) {
                SceneNode tmpSceneNode = SystemSceneNodeManager.MyInstance.GetResource(mainMenuScene);
                if (tmpSceneNode != null) {
                    mainMenuSceneNode = tmpSceneNode;
                }/* else {
                    Debug.LogError("LevelManager.SetupScriptableObjects: could not find scene node " + mainMenuScene + ". Check inspector.");
                }*/
            }




        }

    }

}