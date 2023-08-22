using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    public class SystemConfigurationManager : ConfiguredMonoBehaviour, IStatProvider, ICapabilityProvider {

        [Header("GAME CONFIGURATION")]

        [SerializeField]
        private string gameName;

        [SerializeField]
        private string gameVersion;

        [SerializeField]
        private List<string> loadResourcesFolders = new List<string>();

        [SerializeField]
        private bool allowOfflinePlay = true;

        [SerializeField]
        private bool allowOnlinePlay = false;

        [Header("Scenes")]

        [Tooltip("The name of the scene that loads the game manager into memory, and then proceeds to the main menu")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(SceneNode))]
        private string initializationScene = "Core Game";

        private SceneNode initializationSceneNode = null;

        [Tooltip("The name of the main menu scene")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(SceneNode))]
        private string mainMenuScene = "Main Menu";

        // reference to the main menu scene node
        private SceneNode mainMenuSceneNode = null;

        [Tooltip("When a new game is started, the character will initially spawn in this scene if no scene is provided by their faction")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(SceneNode))]
        private string defaultStartingZone = string.Empty;

        [Header("NEW GAME OPTIONS")]

        [Tooltip("The default name for the character creator, and the default to start with if no character creator is used.")]
        [SerializeField]
        private string defaultPlayerName = "New Player";

        [Tooltip("If false, launch straight into a game with no character configuration.")]
        [SerializeField]
        private bool useNewGameWindow = true;

        [Tooltip("If true, the player name can be modified in the new game window.")]
        [SerializeField]
        private bool editPlayerName = true;

        [Tooltip("Default Player Name = The player name will be set to the default defined above. Unit Profile = The player name will be changed based on the unit profile selected.")]
        [SerializeField]
        private PlayerNameSource playerNameSource = PlayerNameSource.DefaultPlayerName;

        [Tooltip("When the new game window is used, what method will be used to select a character.  DefaultCharacter = only the default character will be available. CharacterList = The faction will control the list. RaceAndGender = A Male or Female character can be chosen after the race is selected.")]
        [SerializeField]
        private CharacterSelectionType characterSelectionType = CharacterSelectionType.DefaultCharacter;

        [Tooltip("If the new game window is used, show the appearance tab.")]
        [SerializeField]
        private bool newGameAppearance = true;

        [Tooltip("If the new game window is used, show the faction tab.")]
        [SerializeField]
        private bool newGameFaction = true;

        [Tooltip("If the new game window is used, show the race tab.")]
        [SerializeField]
        private bool newGameRace = true;

        [Tooltip("If the new game window is used, show the class tab.")]
        [SerializeField]
        private bool newGameClass = true;

        [Tooltip("If the new game window is used, show the specialiation tab")]
        [SerializeField]
        private bool newGameSpecialization = true;

        [Tooltip("The name of the audio profile to play when the new game window is active")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string newGameAudio = string.Empty;

        private AudioProfile newGameAudioProfile = null;

        [Tooltip("If the character creator is not used, this unit will be the default player unit.")]
        [FormerlySerializedAs("defaultPlayerUnitProfileName")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private string defaultPlayerUnitProfile = string.Empty;

        [Tooltip("A list of unit profiles that should always be available if the character selection type is 'Character List' or no faction is available or the current faction is configured to show default profiles.")]
        [FormerlySerializedAs("characterCreatorProfileNames")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private List<string> defaultUnitProfiles = new List<string>();

        // reference to the default profile
        private UnitProfile defaultPlayerUnitProfileRef = null;

        // reference to the default profile
        private List<UnitProfile> defaultUnitProfileList = new List<UnitProfile>();

        [Header("Inventory")]

        /*
        [Tooltip("if false, default backpack goes in bank")]
        [SerializeField]
        private bool equipDefaultBackPack = true;
        */

        [Tooltip("The number of inventory slots a character has with no extra bags equipped")]
        [SerializeField]
        private int defaultInventorySlots = 20;

        [Tooltip("The number of bank slots a character has with no extra bags equipped")]
        [SerializeField]
        private int defaultBankSlots = 48;

        [Tooltip("The maximum number of bags a character can have equipped")]
        [SerializeField]
        private int maxInventoryBags = 5;

        [Tooltip("The maximum number of bags a character can have equipped in their bank")]
        [SerializeField]
        private int maxBankBags = 8;

        [Tooltip("If this field is not null, the player will have this item equipped as their backpack when starting a new game.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Bag))]
        private string defaultBackpackItem = "Backpack";

        [Tooltip("Default items that will be in the player bank when a new player is created")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        private List<string> defaultBankContents = new List<string>();

        /*
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Bag))]
        private string defaultBankBagItem = "Bank";
        */

        [Header("CONTROLLER")]

        /*
        [Tooltip("If true, allow clicking on the ground with the mouse to move to a location")]
        [SerializeField]
        private bool allowClickToMove = false;
        */

        [Tooltip("The controller configuration set on first game load")]
        [SerializeField]
        private DefaultControllerConfiguration defaultControllerConfiguration;

        [Tooltip("The maximum turn speed in degrees per second.")]
        [SerializeField]
        private float maxTurnSpeed = 360f;

        [Tooltip("The default character walk speed in meters per second.")]
        [SerializeField]
        private float walkSpeed = 1f;

        [Tooltip("The default character run speed in meters per second.")]
        [SerializeField]
        private float runSpeed = 7f;

        [Tooltip("The default character swim speed in meters per second.")]
        [SerializeField]
        private float swimSpeed = 2f;

        [Tooltip("The default character fly speed in meters per second.")]
        [SerializeField]
        private float flySpeed = 20f;

        [Tooltip("The default character glide speed in meters per second.")]
        [SerializeField]
        private float glideSpeed = 5f;

        [Tooltip("The speed the character will fall while gliding in meters per second.")]
        [SerializeField]
        private float glideFallSpeed = 2f;

        [Tooltip("If true, the player will take damage when falling from heights.")]
        [SerializeField]
        private bool useFallDamage = false;

        [Tooltip("If fall damage is used, the amount of damage per meter fallen the player will take.")]
        [SerializeField]
        private float fallDamagePerMeter = 2f;

        [Tooltip("If fall damage is used, the minimum distance the player must fall before damage is taken.")]
        [SerializeField]
        private float fallDamageMinDistance = 10f;

        [Tooltip("When not mounted, disable native movement input to allow a third party controller (such as Invector) to move the character")]
        [SerializeField]
        private bool useThirdPartyMovementControl = false;

        [Tooltip("If a third party movement controller is used, disable this to prevent movement lock in combat.")]
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
        [ResourceSelector(resourceType = typeof(AnimationProfile))]
        [SerializeField]
        private string systemAnimationProfileName = string.Empty;

        /*
                [Tooltip("This profile should contain references to all the default animations that are on the default animation controller so the system knows which animations to replace when overriding them.")]
                [SerializeField]
                */
        private AnimationProfile systemAnimationProfile;

        [Tooltip("If true, movement animations will be sped up or slowed down to match the actual speed (in m/s) the character is moving at.  This will reduce foot sliding, but may result in more jerky looking movement.")]
        [SerializeField]
        private bool syncMovementAnimationSpeed;


        [Header("CHARACTER ANIMATION CONFIGURATION")]

        [Tooltip("This profile will override the system animations included in the engine when no other unit or weapon specific animations are in use.")]
        [ResourceSelector(resourceType = typeof(AnimationProfile))]
        [SerializeField]
        private string defaultAnimationProfileName = string.Empty;

        /*
                [FormerlySerializedAs("defaultAttackAnimationProfile")]
                [SerializeField]
                */
        private AnimationProfile defaultAnimationProfile;

        [SerializeField]
        private RuntimeAnimatorController defaultAnimatorController;

        [Header("Level Values")]

        [Tooltip("The character cannot level up past this level.")]
        [SerializeField]
        private int maxLevel = 50;

        [Tooltip("Every level, the amount of experience you need for the next level is increased by this amount.")]
        [SerializeField]
        private int xpRequiredPerLevel = 100;

        [Header("Currency")]

        [Tooltip("When showing the total currency the player has in the vendor window, this currency will be used")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(CurrencyGroup))]
        private string currencyGroupName = string.Empty;

        private CurrencyGroup defaultCurrencyGroup;

        [Tooltip("When selling an item to a vendor, the offered amount will be the regular purchase amount multiplied by this number.")]
        [SerializeField]
        private float vendorPriceMultiplier = 0.25f;


        [Header("Currency Scaling")]

        [Tooltip("If automatic currency is enabled for a lootable character, this currency will be dropped.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Currency))]
        private string killCurrencyName = string.Empty;

        private Currency killCurrency = null;

        [Tooltip("If automatic currency is enabled for a lootable character, this currency amount will be multiplied by the character level.")]
        [SerializeField]
        private int killCurrencyAmountPerLevel = 1;

        [Tooltip("If automatic currency is enabled for a quest, this currency will be rewarded.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Currency))]
        private string questCurrencyName = string.Empty;

        private Currency questCurrency;

        [Tooltip("If automatic currency is enabled for a quest, this currency amount will be multiplied by the quest level.")]
        [SerializeField]
        private int questCurrencyAmountPerLevel = 1;


        [Header("Quest Experience Scaling")]

        [Tooltip("A flat experience amount to add to all quests that does not scale with level.")]
        [SerializeField]
        private int baseQuestXP = 0;

        [Tooltip("A flat experience amount to add to all quests that does scale with level.")]
        [SerializeField]
        private int questXPPerLevel = 100;

        [Tooltip("If true, the experience per level will be multiplied by (1 / level).")]
        [SerializeField]
        private bool useQuestXPLevelMultiplierDemoninator = true;

        [Tooltip("If the above option is true, and this value is more than 0, the experience per level will be multiplied by (1 / level).")]
        [SerializeField]
        private int questXPMultiplierLevelCap = 5;


        [Header("Kills")]

        [Tooltip("A flat experience amount to add to all kills that does not scale with level.")]
        [SerializeField]
        private int baseKillXP = 0;

        [Tooltip("A flat experience amount to add to all kills that does scale with level.")]
        [SerializeField]
        private int killXPPerLevel = 100;

        [Tooltip("If true, the experience per level will be multiplied by (1 / level).")]
        [SerializeField]
        private bool useKillXPLevelMultiplierDemoninator = true;

        [Tooltip("If the above option is true, and this value is more than 0, the experience per level will be multiplied by (1 / level).")]
        [SerializeField]
        private int killXPMultiplierLevelCap = 10;

        [Tooltip("The default amount of time before a unit despawns after killed and looted.")]
        [SerializeField]
        private float defaultDespawnTimer = 0f;


        [Header("DPS Scaling")]

        [Tooltip("Weapons with Dynamic Level set to true will get this amount of DPS per level.")]
        [SerializeField]
        private float weaponDPSBudgetPerLevel = 2.5f;

        [Header("Primary Stats and Scaling")]

        [Tooltip("A Per level stat budget that will be applied to all stats, in addition to their individual budgets.")]
        [SerializeField]
        private float statBudgetPerLevel = 0f;

        private List<StatScalingNode> statScalingNodes = new List<StatScalingNode>();

        [Header("Power Resources and Capabilities")]

        [Tooltip("Power Resources used by all characters.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(PowerResource))]
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        [Tooltip("Capabilities that apply to all units")]
        [SerializeField]
        private CapabilityProps capabilities = new CapabilityProps();

        [Header("Layer")]

        [Tooltip("Character units will automatically be set to this layer so they can respond to AOE / looting and other things that filter by this layer.")]
        [SerializeField]
        private string defaultCharacterUnitLayer = "CharacterUnit";

        [Tooltip("Player units will automatically be set to this layer.")]
        [SerializeField]
        private string defaultPlayerUnitLayer = "Player";

        [Tooltip("Character units will automatically be set to this layer so they can respond to AOE / looting and other things that filter by this layer.")]
        [SerializeField]
        private string defaultPreviewUnitLayer = "UnitPreview";

        [Header("SYSTEM ABILITIES")]

        [Tooltip("The ability effect to cast on a player when they level up.")]
        [FormerlySerializedAs("levelUpAbilityName")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private string levelUpEffectName = string.Empty;

        private AbilityEffect levelUpEffect = null;

        [Tooltip("The ability effect to cast on a player when they die.")]
        [FormerlySerializedAs("deathAbilityName")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private string deathEffectName = string.Empty;

        private AbilityEffect deathEffect = null;

        [Tooltip("The ability effect to cast on any character when it has loot that can be collected.")]
        [FormerlySerializedAs("lootSparkleAbilityName")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private string lootSparkleEffectName = string.Empty;

        private AbilityEffect lootSparkleEffect = null;

        [Header("SYSTEM AUDIO")]

        [Tooltip("This audio clip will play whenever a weapon (or unarmed) attack misses.")]
        [SerializeField]
        private AudioClip weaponMissAudioClip = null;

        [Tooltip("This audio clip will play whenever buying from or selling to a vendor.")]
        [SerializeField]
        private AudioClip vendorAudioClip = null;

        [Tooltip("This audio profile will play whenever buying from or selling to a vendor.  If this value is set, it will override the audio clip above.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string vendorAudioProfileName = string.Empty;

        private AudioProfile vendorAudioProfile = null;

        [Tooltip("The maximum distance at which chat in dialogs above characters will also appear in the player chat log.  Prevents distant conversations from spamming logs.")]
        [SerializeField]
        private float maxChatTextDistance = 25f;

        [Header("UI")]

        [Tooltip("The material that will be used to display the cast target on the ground when casting ground targeted spells.")]
        [SerializeField]
        private SystemUIConfiguration UI = new SystemUIConfiguration();

        [Header("INTERACTABLE CONFIGURATION")]

        [Tooltip("The material that will be used to display the cast target on the ground when casting ground targeted spells.")]
        [SerializeField]
        private Material defaultCastTargetCircle;

        [FormerlySerializedAs("focusProjectorColorMap")]
        [SerializeField]
        private List<ProjectorColorMapNode> highlightCircleColorMap = new List<ProjectorColorMapNode>();

        [Tooltip("An image to use on a nameplate if there is more than 1 valid interactable option.")]
        [SerializeField]
        private Sprite multipleInteractionNamePlateImage = null;

        [Tooltip("An image to use on a nameplate if only crafting is available, but more than 1 craft skill can be shown.")]
        [SerializeField]
        private Sprite multipleCraftNamePlateImage = null;

        [Tooltip("An image to use beside a bank option in the interaction UI window.")]
        [SerializeField]
        private Sprite bankInteractionPanelImage = null;

        [Tooltip("An image to for a bank option on a nameplate.")]
        [SerializeField]
        private Sprite bankNamePlateImage = null;

        [Tooltip("An image to use beside a quest giver option in the interaction UI window.")]
        [SerializeField]
        private Sprite questGiverInteractionPanelImage = null;

        [Tooltip("An image to for a quest giver option on a nameplate.")]
        [SerializeField]
        private Sprite questGiverNamePlateImage = null;

        [Tooltip("An image to use beside a dialog option in the interaction UI window.")]
        [SerializeField]
        private Sprite dialogInteractionPanelImage = null;

        [Tooltip("An image to for a dialog option on a nameplate.")]
        [SerializeField]
        private Sprite dialogNamePlateImage = null;

        [Tooltip("An image to use beside a name change option in the interaction UI window.")]
        [SerializeField]
        private Sprite nameChangeInteractionPanelImage = null;

        [Tooltip("An image to for a name change option on a nameplate.")]
        [SerializeField]
        private Sprite nameChangeNamePlateImage = null;

        [Tooltip("An image to use beside a cutscene option in the interaction UI window.")]
        [SerializeField]
        private Sprite cutSceneInteractionPanelImage = null;

        [Tooltip("An image to for a cutscene option on a nameplate.")]
        [SerializeField]
        private Sprite cutSceneNamePlateImage = null;

        [Tooltip("An image to use beside a lootable character option in the interaction UI window.")]
        [SerializeField]
        private Sprite lootableCharacterInteractionPanelImage = null;

        [Tooltip("An image to for a lootable character option on a nameplate.")]
        [SerializeField]
        private Sprite lootableCharacterNamePlateImage = null;

        [Tooltip("An image to use beside a character creator option in the interaction UI window.")]
        [SerializeField]
        private Sprite characterCreatorInteractionPanelImage = null;

        [Tooltip("An image to for a character creator option on a nameplate.")]
        [SerializeField]
        private Sprite characterCreatorNamePlateImage = null;

        [Tooltip("An image to use beside a unit spawn controller option in the interaction UI window.")]
        [SerializeField]
        private Sprite unitSpawnControllerInteractionPanelImage = null;

        [Tooltip("An image to for a unit spawn controller option on a nameplate.")]
        [SerializeField]
        private Sprite unitSpawnControllerNamePlateImage = null;

        [Tooltip("An image to use beside a faction change option in the interaction UI window.")]
        [SerializeField]
        private Sprite factionChangeInteractionPanelImage = null;

        [Tooltip("An image to for a faction change option on a nameplate.")]
        [SerializeField]
        private Sprite factionChangeNamePlateImage = null;

        [Tooltip("An image to use beside a class change option in the interaction UI window.")]
        [SerializeField]
        private Sprite classChangeInteractionPanelImage = null;

        [Tooltip("An image to for a class change option on a nameplate.")]
        [SerializeField]
        private Sprite classChangeNamePlateImage = null;

        [Tooltip("An image to use beside a vendor option in the interaction UI window.")]
        [SerializeField]
        private Sprite vendorInteractionPanelImage = null;

        [Tooltip("An image to for a vendor option on a nameplate.")]
        [SerializeField]
        private Sprite vendorNamePlateImage = null;

        [Tooltip("An image to use beside a portal option in the interaction UI window.")]
        [SerializeField]
        private Sprite portalInteractionPanelImage = null;

        [Tooltip("An image to for a portal option on a nameplate.")]
        [SerializeField]
        private Sprite portalNamePlateImage = null;

        [Tooltip("An image to use beside a skill trainer option in the interaction UI window.")]
        [SerializeField]
        private Sprite skillTrainerInteractionPanelImage = null;

        [Tooltip("An image to for a skill trainer option on a nameplate.")]
        [SerializeField]
        private Sprite skillTrainerNamePlateImage = null;

        [Tooltip("An image to use beside a music player option in the interaction UI window.")]
        [SerializeField]
        private Sprite musicPlayerInteractionPanelImage = null;

        [Tooltip("An image to for a music player option on a nameplate.")]
        [SerializeField]
        private Sprite musicPlayerNamePlateImage = null;

        [Header("Quest Configuration")]

        [SerializeField]
        [Tooltip("The maximum number of quests in the quest log.")]
        private int questLogSize = 25;

        protected bool eventSubscriptionsInitialized = false;

        [Header("TIME OF DAY")]

        [Tooltip("This value is a multiplier. For every 1 seconds of real-world time that passes, x seconds of in-game time will pass.")]
        [SerializeField]
        private float timeOfDaySpeed = 1f;

        [Tooltip("The number of degrees around the X axis that the sun will be rotated compared to world space. 0 is pointing directly North, 90 is pointing straight down (directly over the player head), 180 is pointing directly South.")]
        [SerializeField]
        private float defaultSunAngle = 30f;

        [Tooltip("A color gradient to use for the sun color.  The ends represent midnight, and the center is noon.  The alpha value controls sun light intensity and skybox blending.")]
        [SerializeField]
        private Gradient defaultSunGradient;

        [Header("NETWORK")]

        [Tooltip("This is the address of the API server that the dedicated game server will connect to for login and character save / load operations. Do not use a trailing slash.")]
        [SerializeField]
        private string apiServerAddress = "https://account.anyrpg.org";

        [Tooltip("This is the address (and optionally port) of the dedicated authoritative game server that all clients connect to.")]
        [SerializeField]
        private string gameServerAddress = "localhost";

        [Tooltip("If the server and client both do not have this version, the client will be told to upgrade.")]
        [SerializeField]
        private string clientVersion = "1";

        [Tooltip("This is the address of a web page where the latest client can be downloaded.")]
        [SerializeField]
        private string clientDownloadUrl = "https://www.anyrpg.org/downloads";


        // game manager references
        SystemDataFactory systemDataFactory = null;

        public AbilityEffect LootSparkleEffect { get => lootSparkleEffect; set => lootSparkleEffect = value; }
        public AbilityEffect LevelUpEffect { get => levelUpEffect; set => levelUpEffect = value; }
        public Sprite QuestGiverInteractionPanelImage { get => questGiverInteractionPanelImage; set => questGiverInteractionPanelImage = value; }
        public Sprite QuestGiverNamePlateImage { get => questGiverNamePlateImage; set => questGiverNamePlateImage = value; }
        public Sprite DialogInteractionPanelImage { get => dialogInteractionPanelImage; set => dialogInteractionPanelImage = value; }
        public Sprite DialogNamePlateImage { get => dialogNamePlateImage; set => dialogNamePlateImage = value; }
        public Sprite NameChangeInteractionPanelImage { get => nameChangeInteractionPanelImage; set => nameChangeInteractionPanelImage = value; }
        public Sprite NameChangeNamePlateImage { get => nameChangeNamePlateImage; set => nameChangeNamePlateImage = value; }
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
        public Sprite UnitSpawnControllerInteractionPanelImage { get => unitSpawnControllerInteractionPanelImage; set => unitSpawnControllerInteractionPanelImage = value; }
        public Sprite UnitSpawnControllerNamePlateImage { get => unitSpawnControllerNamePlateImage; set => unitSpawnControllerNamePlateImage = value; }
        public Sprite MusicPlayerInteractionPanelImage { get => musicPlayerInteractionPanelImage; set => musicPlayerInteractionPanelImage = value; }
        public Sprite MusicPlayerNamePlateImage { get => musicPlayerNamePlateImage; set => musicPlayerNamePlateImage = value; }
        public RuntimeAnimatorController DefaultAnimatorController { get => defaultAnimatorController; set => defaultAnimatorController = value; }
        public AnimationProfile DefaultAnimationProfile { get => defaultAnimationProfile; set => defaultAnimationProfile = value; }
        public Material DefaultCastingLightProjector { get => defaultCastTargetCircle; set => defaultCastTargetCircle = value; }
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
        public List<StatScalingNode> PrimaryStats { get => statScalingNodes; set => statScalingNodes = value; }
        public List<string> PowerResources { get => powerResources; set => powerResources = value; }
        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public string KillCurrencyName { get => killCurrencyName; set => killCurrencyName = value; }
        public int KillCurrencyAmountPerLevel { get => killCurrencyAmountPerLevel; set => killCurrencyAmountPerLevel = value; }
        public string QuestCurrencyName { get => questCurrencyName; set => questCurrencyName = value; }
        public int QuestCurrencyAmountPerLevel { get => questCurrencyAmountPerLevel; set => questCurrencyAmountPerLevel = value; }
        public Currency KillCurrency { get => killCurrency; set => killCurrency = value; }
        public Currency QuestCurrency { get => questCurrency; set => questCurrency = value; }
        public AudioClip VendorAudioClip {
            get {
                if (vendorAudioProfile != null) {
                    return vendorAudioProfile.AudioClip;
                }
                return vendorAudioClip;
            }
            set => vendorAudioClip = value;
        }
        public float MaxChatTextDistance { get => maxChatTextDistance; set => maxChatTextDistance = value; }
        public bool UseNewGameWindow { get => useNewGameWindow; set => useNewGameWindow = value; }
        public bool NewGameAppearance { get => newGameAppearance; set => newGameAppearance = value; }
        public bool NewGameClass { get => newGameClass; set => newGameClass = value; }
        public bool NewGameFaction { get => newGameFaction; set => newGameFaction = value; }
        public bool NewGameRace { get => newGameRace; set => newGameRace = value; }
        public bool NewGameSpecialization { get => newGameSpecialization; set => newGameSpecialization = value; }
        public AudioProfile NewGameAudioProfile { get => newGameAudioProfile; set => newGameAudioProfile = value; }
        public string DefaultPlayerName { get => defaultPlayerName; set => defaultPlayerName = value; }
        public string DefaultPlayerUnitProfileName { get => defaultPlayerUnitProfile; set => defaultPlayerUnitProfile = value; }
        public UnitProfile DefaultPlayerUnitProfile { get => defaultPlayerUnitProfileRef; set => defaultPlayerUnitProfileRef = value; }
        public string DefaultUnitProfileName {
            get {
                if (defaultUnitProfiles.Count > 0) {
                    return defaultUnitProfiles[0];
                }
                return null;
            }
        }
        public UnitProfile DefaultUnitProfile {
            get {
                if (defaultUnitProfileList != null && defaultUnitProfileList.Count > 0) {
                    return defaultUnitProfileList[0];
                }
                return null;
            }
        }
        public List<UnitProfile> DefaultUnitProfileList { get => defaultUnitProfileList; set => defaultUnitProfileList = value; }

        public string DefaultStartingZone { get => defaultStartingZone; set => defaultStartingZone = value; }
        public SceneNode InitializationSceneNode { get => initializationSceneNode; set => initializationSceneNode = value; }
        public SceneNode MainMenuSceneNode { get => mainMenuSceneNode; set => mainMenuSceneNode = value; }


        //public bool EquipDefaultBackPack { get => equipDefaultBackPack; set => equipDefaultBackPack = value; }
        public string DefaultPlayerUnitLayer { get => defaultPlayerUnitLayer; set => defaultPlayerUnitLayer = value; }
        public GameObject ThirdPartyCamera { get => thirdPartyCamera; set => thirdPartyCamera = value; }
        public string DefaultBackpackItem { get => defaultBackpackItem; set => defaultBackpackItem = value; }
        //public string DefaultBankBagItem { get => defaultBankBagItem; set => defaultBankBagItem = value; }
        public string MainMenuScene { get => mainMenuScene; set => mainMenuScene = value; }
        public string InitializationScene { get => initializationScene; set => initializationScene = value; }
        //public MiniMapFallBackMode MiniMapFallBackMode { get => miniMapFallBackMode; set => miniMapFallBackMode = value; }
        public List<string> DefaultUnitProfileNames { get => defaultUnitProfiles; set => defaultUnitProfiles = value; }
        public bool SyncMovementAnimationSpeed { get => syncMovementAnimationSpeed; set => syncMovementAnimationSpeed = value; }
        public int QuestLogSize { get => questLogSize; set => questLogSize = value; }
        public float MaxTurnSpeed { get => maxTurnSpeed; }
        public float WalkSpeed { get => walkSpeed; }
        public float RunSpeed { get => runSpeed; }
        public float SwimSpeed { get => swimSpeed; }
        public float FlySpeed { get => flySpeed; }
        public float GlideSpeed { get => glideSpeed; }
        public float GlideFallSpeed { get => glideFallSpeed; }
        public bool UseFallDamage { get => useFallDamage; set => useFallDamage = value; }
        public float FallDamagePerMeter { get => fallDamagePerMeter; set => fallDamagePerMeter = value; }
        public float FallDamageMinDistance { get => fallDamageMinDistance; set => fallDamageMinDistance = value; }
        public DefaultControllerConfiguration DefaultControllerConfiguration { get => defaultControllerConfiguration; set => defaultControllerConfiguration = value; }
        public int DefaultInventorySlots { get => defaultInventorySlots; set => defaultInventorySlots = value; }
        public int DefaultBankSlots { get => defaultBankSlots; set => defaultBankSlots = value; }
        public int MaxInventoryBags { get => maxInventoryBags; set => maxInventoryBags = value; }
        public int MaxBankBags { get => maxBankBags; set => maxBankBags = value; }
        public List<string> DefaultBankContents { get => defaultBankContents; set => defaultBankContents = value; }
        public string NewGameAudio { get => newGameAudio; set => newGameAudio = value; }
        public string LevelUpEffectName { get => levelUpEffectName; set => levelUpEffectName = value; }
        public string DeathEffectName { get => deathEffectName; set => deathEffectName = value; }
        public string LootSparkleEffectName { get => lootSparkleEffectName; set => lootSparkleEffectName = value; }
        public string CurrencyGroupName { get => currencyGroupName; set => currencyGroupName = value; }
        public CapabilityProps Capabilities { get => capabilities; }
        public float TimeOfDaySpeed { get => timeOfDaySpeed; set => timeOfDaySpeed = value; }
        public float DefaultSunAngle { get => defaultSunAngle; set => defaultSunAngle = value; }
        public Gradient DefaultSunGradient { get => defaultSunGradient; set => defaultSunGradient = value; }
        public AudioClip WeaponMissAudioClip { get => weaponMissAudioClip; set => weaponMissAudioClip = value; }
        public SystemUIConfiguration UIConfiguration { get => UI; set => UI = value; }
        public CharacterSelectionType CharacterSelectionType { get => characterSelectionType; set => characterSelectionType = value; }
        public string DefaultPreviewUnitLayer { get => defaultPreviewUnitLayer; set => defaultPreviewUnitLayer = value; }
        public bool EditPlayerName { get => editPlayerName; set => editPlayerName = value; }
        public PlayerNameSource PlayerNameSource { get => playerNameSource; set => playerNameSource = value; }
        public string ApiServerAddress { get => apiServerAddress; set => apiServerAddress = value; }
        public bool AllowOfflinePlay { get => allowOfflinePlay; set => allowOfflinePlay = value; }
        public bool AllowOnlinePlay { get => allowOnlinePlay; set => allowOnlinePlay = value; }
        public string ClientDownloadUrl { get => clientDownloadUrl; set => clientDownloadUrl = value; }
        public string ClientVersion { get => clientVersion; set => clientVersion = value; }
        public string GameServerAddress { get => gameServerAddress; set => gameServerAddress = value; }

        //public bool AllowClickToMove { get => allowClickToMove; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemDataFactory = systemGameManager.SystemDataFactory;

            SetupScriptableObjects();
        }

        public CapabilityProps GetFilteredCapabilities(ICapabilityConsumer capabilityConsumer, bool returnAll = true) {
            return capabilities;
        }

        // verify that system abilities are available through the factory
        public void SetupScriptableObjects() {

            if (systemAnimationProfileName != null && systemAnimationProfileName != string.Empty) {
                AnimationProfile animationProfile = systemDataFactory.GetResource<AnimationProfile>(systemAnimationProfileName);
                if (animationProfile == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): animation profile " + systemAnimationProfileName + " could not be found in factory.  CHECK INSPECTOR");
                    return;
                } else {
                    systemAnimationProfile = animationProfile;
                }
            }

            if (defaultAnimationProfileName != null && defaultAnimationProfileName != string.Empty) {
                AnimationProfile animationProfile = systemDataFactory.GetResource<AnimationProfile>(defaultAnimationProfileName);
                if (animationProfile == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): animation profile " + defaultAnimationProfileName + " could not be found in factory.  CHECK INSPECTOR");
                    return;
                } else {
                    defaultAnimationProfile = animationProfile;
                }
            }


            if (levelUpEffectName != null && levelUpEffectName != string.Empty) {
                AbilityEffect testAbility = systemDataFactory.GetResource<AbilityEffect>(levelUpEffectName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): " + levelUpEffectName + " could not be found in factory.  CHECK INSPECTOR");
                    return;
                } else {
                    levelUpEffect = testAbility;
                }
            }

            if (deathEffectName != null && deathEffectName != string.Empty) {
                AbilityEffect testAbility = systemDataFactory.GetResource<AbilityEffect>(deathEffectName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): " + deathEffectName + " could not be found in factory.  CHECK INSPECTOR");
                    return;
                } else {
                    deathEffect = testAbility;
                }
            }
            if (lootSparkleEffectName != null && lootSparkleEffectName != string.Empty) {
                AbilityEffect testAbility = systemDataFactory.GetResource<AbilityEffect>(lootSparkleEffectName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): " + lootSparkleEffectName + " could not be found in factory.  CHECK INSPECTOR");
                    return;
                } else {
                    lootSparkleEffect = testAbility;
                }
            }
            if (currencyGroupName != null && currencyGroupName != string.Empty) {
                CurrencyGroup tmpCurrencyGroup = systemDataFactory.GetResource<CurrencyGroup>(currencyGroupName);
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
                    PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(powerResourcename);
                    if (tmpPowerResource != null) {
                        powerResourceList.Add(tmpPowerResource);
                    } else {
                        Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + ". CHECK INSPECTOR");
                    }
                }
            }

            if (KillCurrencyName != null && KillCurrencyName != string.Empty) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(KillCurrencyName);
                if (tmpCurrency != null) {
                    killCurrency = tmpCurrency;
                    //currencyNode.MyAmount = gainCurrencyAmount;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find currency : " + KillCurrencyName + ".  CHECK INSPECTOR");
                }
            }

            if (questCurrencyName != null && questCurrencyName != string.Empty) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(questCurrencyName);
                if (tmpCurrency != null) {
                    questCurrency = tmpCurrency;
                    //currencyNode.MyAmount = gainCurrencyAmount;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find currency : " + questCurrencyName + ".  CHECK INSPECTOR");
                }
            }

            /*
            foreach (StatScalingNode statScalingNode in primaryStats) {
                statScalingNode.SetupScriptableObjects(systemDataFactory);
            }
            */
            List<CharacterStat> characterStats = systemDataFactory.GetResourceList<CharacterStat>();
            foreach (CharacterStat characterStat in characterStats) {
                if (characterStat.GlobalStat == true) {
                    StatScalingNode statScalingNode = new StatScalingNode();
                    statScalingNode.StatName = characterStat.ResourceName;
                    statScalingNode.BudgetPerLevel = characterStat.BudgetPerLevel;
                    statScalingNode.PrimaryToSecondaryConversion = characterStat.PrimaryToSecondaryConversion;
                    statScalingNode.PrimaryToResourceConversion = characterStat.PrimaryToResourceConversion;
                    statScalingNode.Regen = characterStat.Regen;
                    statScalingNodes.Add(statScalingNode);
                }
            }

            capabilities.SetupScriptableObjects(systemDataFactory);

            if (vendorAudioProfileName != null && vendorAudioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = systemDataFactory.GetResource<AudioProfile>(vendorAudioProfileName);
                if (tmpAudioProfile != null) {
                    vendorAudioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find audio profile : " + vendorAudioProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }

            if (newGameAudio != null && newGameAudio != string.Empty) {
                AudioProfile tmpAudioProfile = systemDataFactory.GetResource<AudioProfile>(newGameAudio);
                if (tmpAudioProfile != null) {
                    newGameAudioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find audio profile : " + newGameAudio + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }

            // get default player unit profile
            if (defaultPlayerUnitProfile != null && defaultPlayerUnitProfile != string.Empty) {
                UnitProfile tmpUnitProfile = systemDataFactory.GetResource<UnitProfile>(defaultPlayerUnitProfile);
                if (tmpUnitProfile != null) {
                    defaultPlayerUnitProfileRef = tmpUnitProfile;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): could not find unit profile " + defaultPlayerUnitProfile + ".  Check Inspector");
                }
            } else {
                if (characterSelectionType == CharacterSelectionType.DefaultCharacter) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Character Selection Type is set to Default Player, but the Default Player Unit Profile is empty.  Please set it in the inspector");
                }
            }

            // get default player unit profile
            if (defaultUnitProfiles != null) {
                foreach (string characterCreatorProfileName in defaultUnitProfiles) {
                    if (characterCreatorProfileName != null && characterCreatorProfileName != string.Empty) {
                        UnitProfile tmpUnitProfile = systemDataFactory.GetResource<UnitProfile>(characterCreatorProfileName);
                        if (tmpUnitProfile != null) {
                            defaultUnitProfileList.Add(tmpUnitProfile);
                        } else {
                            Debug.LogError($"SystemConfigurationManager.SetupScriptableObjects(): could not find unit profile {characterCreatorProfileName}.  Check Inspector");
                        }
                    } else {
                        Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): defaultPlayerUnitProfileName field is required, but not value was set.  Check Inspector");
                    }

                }
            }


            if (initializationScene != null && initializationScene != string.Empty) {
                SceneNode tmpSceneNode = systemDataFactory.GetResource<SceneNode>(initializationScene);
                if (tmpSceneNode != null) {
                    initializationSceneNode = tmpSceneNode;
                }
                // it shouldn't be required to have this scene node
                /* else {
                    Debug.LogError("LevelManager.SetupScriptableObjects: could not find scene node " + initializationScene + ". Check inspector.");
                }*/
            }

            if (mainMenuScene != null && mainMenuScene != string.Empty) {
                SceneNode tmpSceneNode = systemDataFactory.GetResource<SceneNode>(mainMenuScene);
                if (tmpSceneNode != null) {
                    mainMenuSceneNode = tmpSceneNode;
                }/* else {
                    Debug.LogError("LevelManager.SetupScriptableObjects: could not find scene node " + mainMenuScene + ". Check inspector.");
                }*/
            }




        }

    }

    public enum DefaultControllerConfiguration { MouseAndKeyboard, GamePad }

    public enum CharacterSelectionType { DefaultCharacter, CharacterList, RaceAndGender }

    public enum PlayerNameSource { DefaultPlayerName, UnitProfile }

}