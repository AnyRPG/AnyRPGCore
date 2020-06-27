using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;

namespace AnyRPG {
    public class SystemConfigurationManager : MonoBehaviour {

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


        [SerializeField]
        private float vendorPriceMultiplier = 0.25f;

        [Header("CONTROLLER")]

        [SerializeField]
        private bool useThirdPartyMovementControl = false;

        [Tooltip("If a third party movement controller is used, disable this to prevent movement lock in combat")]
        [SerializeField]
        private bool allowAutoAttack = true;

        [SerializeField]
        private bool useThirdPartyCameraControl = false;

        [Header("UI")]

        [SerializeField]
        private Material defaultCastingLightProjector;

        [SerializeField]
        private List<ProjectorColorMapNode> focusProjectorColorMap = new List<ProjectorColorMapNode>();

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

        [SerializeField]
        private CurrencyGroup defaultCurrencyGroup;

        [Tooltip("The faction icon to show on the load game screen when the player has no faction.")]
        [SerializeField]
        private Sprite defaultFactionIcon;

        [Header("ANIMATION")]

        // this should contain references to all the default animations that are on the default animation controller for mapping overrides
        [SerializeField]
        private AnimationProfile systemAnimationProfile;


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

        [Header("Layer")]

        [Tooltip("character units will automatically be set to this layer so they can respond to AOE / looting and other things that filter by this layer.")]
        [SerializeField]
        private string defaultCharacterUnitLayer = string.Empty;

        [Header("SYSTEM ABILITIES")]

        [SerializeField]
        private string levelUpAbilityName = string.Empty;

        private BaseAbility levelUpAbility = null;

        [SerializeField]
        private string deathAbilityName = string.Empty;

        private BaseAbility deathAbility = null;

        [SerializeField]
        private string lootSparkleAbilityName = string.Empty;

        private BaseAbility lootSparkleAbility = null;

        [Header("SYSTEM AUDIO")]

        [Tooltip("This audio will play whenever buying from or selling to a vendor")]
        [SerializeField]
        private string vendorAudioProfileName = string.Empty;

        private AudioProfile vendorAudioProfile;

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

        public BaseAbility LootSparkleAbility { get => lootSparkleAbility; set => lootSparkleAbility = value; }
        public Material MyTemporaryMaterial { get => temporaryMaterial; set => temporaryMaterial = value; }
        public BaseAbility MyLevelUpAbility { get => levelUpAbility; set => levelUpAbility = value; }
        public Sprite MyQuestGiverInteractionPanelImage { get => questGiverInteractionPanelImage; set => questGiverInteractionPanelImage = value; }
        public Sprite MyQuestGiverNamePlateImage { get => questGiverNamePlateImage; set => questGiverNamePlateImage = value; }
        public Sprite MyDialogInteractionPanelImage { get => dialogInteractionPanelImage; set => dialogInteractionPanelImage = value; }
        public Sprite MyDialogNamePlateImage { get => dialogNamePlateImage; set => dialogNamePlateImage = value; }
        public Sprite MyNameChangeInteractionPanelImage { get => nameChangeInteractionPanelImage; set => nameChangeInteractionPanelImage = value; }
        public Sprite MyNameChangeNamePlateImage { get => nameChangeNamePlateImage; set => nameChangeNamePlateImage = value; }
        public Sprite MyCutSceneInteractionPanelImage { get => cutSceneInteractionPanelImage; set => cutSceneInteractionPanelImage = value; }
        public Sprite MyCutSceneNamePlateImage { get => cutSceneNamePlateImage; set => cutSceneNamePlateImage = value; }
        public Sprite MyLootableCharacterInteractionPanelImage { get => lootableCharacterInteractionPanelImage; set => lootableCharacterInteractionPanelImage = value; }
        public Sprite MyLootableCharacterNamePlateImage { get => lootableCharacterNamePlateImage; set => lootableCharacterNamePlateImage = value; }
        public Sprite MyCharacterCreatorInteractionPanelImage { get => characterCreatorInteractionPanelImage; set => characterCreatorInteractionPanelImage = value; }
        public Sprite MyCharacterCreatorNamePlateImage { get => characterCreatorNamePlateImage; set => characterCreatorNamePlateImage = value; }
        public Sprite MyFactionChangeInteractionPanelImage { get => factionChangeInteractionPanelImage; set => factionChangeInteractionPanelImage = value; }
        public Sprite MyFactionChangeNamePlateImage { get => factionChangeNamePlateImage; set => factionChangeNamePlateImage = value; }
        public Sprite MyClassChangeInteractionPanelImage { get => classChangeInteractionPanelImage; set => classChangeInteractionPanelImage = value; }
        public Sprite MyClassChangeNamePlateImage { get => classChangeNamePlateImage; set => classChangeNamePlateImage = value; }
        public Sprite MyPortalInteractionPanelImage { get => portalInteractionPanelImage; set => portalInteractionPanelImage = value; }
        public Sprite MyPortalNamePlateImage { get => portalNamePlateImage; set => portalNamePlateImage = value; }
        public Sprite MySkillTrainerInteractionPanelImage { get => skillTrainerInteractionPanelImage; set => skillTrainerInteractionPanelImage = value; }
        public Sprite MySkillTrainerNamePlateImage { get => skillTrainerNamePlateImage; set => skillTrainerNamePlateImage = value; }
        public BaseAbility DeathAbility { get => deathAbility; set => deathAbility = value; }
        public Sprite MyMultipleInteractionNamePlateImage { get => multipleInteractionNamePlateImage; set => multipleInteractionNamePlateImage = value; }
        public float MyDefaultDespawnTimer { get => defaultDespawnTimer; set => defaultDespawnTimer = value; }
        public Sprite MyBankInteractionPanelImage { get => bankInteractionPanelImage; set => bankInteractionPanelImage = value; }
        public Sprite MyBankNamePlateImage { get => bankNamePlateImage; set => bankNamePlateImage = value; }
        public Sprite MyVendorInteractionPanelImage { get => vendorInteractionPanelImage; set => vendorInteractionPanelImage = value; }
        public Sprite MyVendorNamePlateImage { get => vendorNamePlateImage; set => vendorNamePlateImage = value; }
        public Sprite MyMultipleCraftNamePlateImage { get => multipleCraftNamePlateImage; set => multipleCraftNamePlateImage = value; }
        public string MyGameName { get => gameName; set => gameName = value; }
        public string MyGameVersion { get => gameVersion; set => gameVersion = value; }
        public Sprite MySystemBarMainMenu { get => systemBarMainMenu; set => systemBarMainMenu = value; }
        public Sprite MySystemBarAbilityBook { get => systemBarAbilityBook; set => systemBarAbilityBook = value; }
        public Sprite MySystemBarCharacter { get => systemBarCharacter; set => systemBarCharacter = value; }
        public Sprite MySystemBarQuestLog { get => systemBarQuestLog; set => systemBarQuestLog = value; }
        public Sprite MySystemBarMap { get => systemBarMap; set => systemBarMap = value; }
        public Sprite MyUnitSpawnControllerInteractionPanelImage { get => unitSpawnControllerInteractionPanelImage; set => unitSpawnControllerInteractionPanelImage = value; }
        public Sprite MyUnitSpawnControllerNamePlateImage { get => unitSpawnControllerNamePlateImage; set => unitSpawnControllerNamePlateImage = value; }
        public Sprite MyMusicPlayerInteractionPanelImage { get => musicPlayerInteractionPanelImage; set => musicPlayerInteractionPanelImage = value; }
        public Sprite MyMusicPlayerNamePlateImage { get => musicPlayerNamePlateImage; set => musicPlayerNamePlateImage = value; }
        public RuntimeAnimatorController MyDefaultAnimatorController { get => defaultAnimatorController; set => defaultAnimatorController = value; }
        public AnimationProfile MyDefaultAnimationProfile { get => defaultAnimationProfile; set => defaultAnimationProfile = value; }
        public Material MyDefaultCastingLightProjector { get => defaultCastingLightProjector; set => defaultCastingLightProjector = value; }
        public Color MyDefaultUIColor { get => defaultUIColor; set => defaultUIColor = value; }
        public Color MyDefaultUIFillColor { get => defaultUIFillColor; set => defaultUIFillColor = value; }
        public Color MyDefaultUISolidColor { get => defaultUISolidColor; set => defaultUISolidColor = value; }
        public List<string> MyLoadResourcesFolders { get => loadResourcesFolders; set => loadResourcesFolders = value; }
        public int MyMaxLevel { get => maxLevel; set => maxLevel = value; }
        public float MyStatBudgetPerLevel { get => statBudgetPerLevel; set => statBudgetPerLevel = value; }
        public CurrencyGroup MyDefaultCurrencyGroup { get => defaultCurrencyGroup; set => defaultCurrencyGroup = value; }
        public float MyVendorPriceMultiplier { get => vendorPriceMultiplier; set => vendorPriceMultiplier = value; }
        public float MyWeaponDPSBudgetPerLevel { get => weaponDPSBudgetPerLevel; set => weaponDPSBudgetPerLevel = value; }
        public string MyDefaultCharacterUnitLayer { get => defaultCharacterUnitLayer; set => defaultCharacterUnitLayer = value; }
        public AnimationProfile MySystemAnimationProfile { get => systemAnimationProfile; set => systemAnimationProfile = value; }
        public List<ProjectorColorMapNode> MyFocusProjectorColorMap { get => focusProjectorColorMap; set => focusProjectorColorMap = value; }
        public bool MyUseThirdPartyMovementControl { get => useThirdPartyMovementControl; set => useThirdPartyMovementControl = value; }
        public bool MyUseThirdPartyCameraControl { get => useThirdPartyCameraControl; set => useThirdPartyCameraControl = value; }
        public bool MyAllowAutoAttack { get => allowAutoAttack; set => allowAutoAttack = value; }
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

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        // verify that system abilities are available through the factory
        public void SetupScriptableObjects() {

            BaseAbility testAbility = null;
            if (levelUpAbilityName != null && levelUpAbilityName != string.Empty) {
                testAbility = SystemAbilityManager.MyInstance.GetResource(levelUpAbilityName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): " + levelUpAbilityName + " COULD NOT BE FOUND IN FACTORY.  CHECK INSPECTOR");
                    return;
                } else {
                    levelUpAbility = testAbility;
                }
            }

            if (deathAbilityName != null && deathAbilityName != string.Empty) {
                testAbility = SystemAbilityManager.MyInstance.GetResource(deathAbilityName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): " + deathAbilityName + " COULD NOT BE FOUND IN FACTORY.  CHECK INSPECTOR");
                    return;
                } else {
                    deathAbility = testAbility;
                }
            }
            if (lootSparkleAbilityName != null && lootSparkleAbilityName != string.Empty) {
                testAbility = SystemAbilityManager.MyInstance.GetResource(lootSparkleAbilityName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): " + lootSparkleAbilityName + " COULD NOT BE FOUND IN FACTORY.  CHECK INSPECTOR");
                    return;
                } else {
                    lootSparkleAbility = testAbility;
                }
            }
            if (defaultCurrencyGroup == null) {
                Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): NO DEFAULT CURRENCY GROUP SET.  CHECK INSPECTOR");
                return;
            }
            
            if (defaultAnimationProfile == null) {
                Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): NO DEFAULT ANIMATION PROFILE SET.  CHECK INSPECTOR");
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

            if (vendorAudioProfileName != null && vendorAudioProfileName != string.Empty) {
                AudioProfile tmpAudioProfile = SystemAudioProfileManager.MyInstance.GetResource(vendorAudioProfileName);
                if (tmpAudioProfile != null) {
                    vendorAudioProfile = tmpAudioProfile;
                } else {
                    Debug.LogError("SystemConfigurationManager.SetupScriptableObjects(): Could not find audio profile : " + vendorAudioProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }


        }

    }

}