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

        // default UI color for static elements that have no additional transparency applied to them
        [SerializeField]
        private Color defaultUIColor;

        // defaultUIColor with full opacity for button frames
        [SerializeField]
        private Color defaultUISolidColor;


        [SerializeField]
        private Color defaultUIFillColor;

        [SerializeField]
        private Sprite defaultUIPanelFrame;

        [SerializeField]
        private CurrencyGroup defaultCurrencyGroup;


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

        [Header("CHARACTER CONFIGURATION")]

        [FormerlySerializedAs("defaultAttackAnimationProfile")]
        [SerializeField]
        private AnimationProfile defaultAnimationProfile;

        [SerializeField]
        private RuntimeAnimatorController defaultAnimatorController;

        [SerializeField]
        private int maxLevel = 50;

        [SerializeField]
        private float weaponDPSBudgetPerLevel = 2.5f;

        [SerializeField]
        private float statBudgetPerLevel = 0f;

        [SerializeField]
        private float staminaStatBudgetPerLevel = 10f;

        [SerializeField]
        private float agilityStatBudgetPerLevel = 5f;

        [SerializeField]
        private float strengthStatBudgetPerLevel = 5f;

        [SerializeField]
        private float intellectStatBudgetPerLevel = 5f;

        // character units will automatically be set to this layer so they can respond to AOE / looting and other things that filter by this layer.
        [SerializeField]
        private string defaultCharacterUnitLayer = string.Empty;

        [Header("CHARACTER PANEL")]

        [SerializeField]
        private Sprite characterPanelHead;

        [SerializeField]
        private Sprite characterPanelShoulders;

        [SerializeField]
        private Sprite characterPanelChest;

        [SerializeField]
        private Sprite characterPanelHands;

        [SerializeField]
        private Sprite characterPanelLegs;

        [SerializeField]
        private Sprite characterPanelFeet;

        [SerializeField]
        private Sprite characterPanelMainHand;

        [SerializeField]
        private Sprite characterPanelOffHand;

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

        [SerializeField]
        private string doWhiteDamageAbilityName = string.Empty;

        private BaseAbility doWhiteDamageAbility = null;

        [SerializeField]
        private AudioClip defaultHitSoundEffect = null;

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

        public BaseAbility MyLootSparkleAbility { get => lootSparkleAbility; set => lootSparkleAbility = value; }
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
        public AudioClip MyDefaultHitSoundEffect { get => defaultHitSoundEffect; set => defaultHitSoundEffect = value; }
        public BaseAbility MyDoWhiteDamageAbility { get => doWhiteDamageAbility; set => doWhiteDamageAbility = value; }
        public BaseAbility MyDeathAbility { get => deathAbility; set => deathAbility = value; }
        public Sprite MyMultipleInteractionNamePlateImage { get => multipleInteractionNamePlateImage; set => multipleInteractionNamePlateImage = value; }
        public float MyDefaultDespawnTimer { get => defaultDespawnTimer; set => defaultDespawnTimer = value; }
        public Sprite MyBankInteractionPanelImage { get => bankInteractionPanelImage; set => bankInteractionPanelImage = value; }
        public Sprite MyBankNamePlateImage { get => bankNamePlateImage; set => bankNamePlateImage = value; }
        public Sprite MyVendorInteractionPanelImage { get => vendorInteractionPanelImage; set => vendorInteractionPanelImage = value; }
        public Sprite MyVendorNamePlateImage { get => vendorNamePlateImage; set => vendorNamePlateImage = value; }
        public Sprite MyMultipleCraftNamePlateImage { get => multipleCraftNamePlateImage; set => multipleCraftNamePlateImage = value; }
        public string MyGameName { get => gameName; set => gameName = value; }
        public string MyGameVersion { get => gameVersion; set => gameVersion = value; }
        public Sprite MyCharacterPanelHead { get => characterPanelHead; set => characterPanelHead = value; }
        public Sprite MyCharacterPanelShoulders { get => characterPanelShoulders; set => characterPanelShoulders = value; }
        public Sprite MyCharacterPanelChest { get => characterPanelChest; set => characterPanelChest = value; }
        public Sprite MyCharacterPanelHands { get => characterPanelHands; set => characterPanelHands = value; }
        public Sprite MyCharacterPanelLegs { get => characterPanelLegs; set => characterPanelLegs = value; }
        public Sprite MyCharacterPanelFeet { get => characterPanelFeet; set => characterPanelFeet = value; }
        public Sprite MyCharacterPanelMainHand { get => characterPanelMainHand; set => characterPanelMainHand = value; }
        public Sprite MyCharacterPanelOffHand { get => characterPanelOffHand; set => characterPanelOffHand = value; }
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
        public float MyStaminaStatBudgetPerLevel { get => staminaStatBudgetPerLevel; set => staminaStatBudgetPerLevel = value; }
        public float MyAgilityStatBudgetPerLevel { get => agilityStatBudgetPerLevel; set => agilityStatBudgetPerLevel = value; }
        public float MyStrengthStatBudgetPerLevel { get => strengthStatBudgetPerLevel; set => strengthStatBudgetPerLevel = value; }
        public float MyIntellectStatBudgetPerLevel { get => intellectStatBudgetPerLevel; set => intellectStatBudgetPerLevel = value; }
        public float MyWeaponDPSBudgetPerLevel { get => weaponDPSBudgetPerLevel; set => weaponDPSBudgetPerLevel = value; }
        public string MyDefaultCharacterUnitLayer { get => defaultCharacterUnitLayer; set => defaultCharacterUnitLayer = value; }
        public AnimationProfile MySystemAnimationProfile { get => systemAnimationProfile; set => systemAnimationProfile = value; }
        public List<ProjectorColorMapNode> MyFocusProjectorColorMap { get => focusProjectorColorMap; set => focusProjectorColorMap = value; }
        public bool MyUseThirdPartyMovementControl { get => useThirdPartyMovementControl; set => useThirdPartyMovementControl = value; }
        public bool MyUseThirdPartyCameraControl { get => useThirdPartyCameraControl; set => useThirdPartyCameraControl = value; }
        public bool MyAllowAutoAttack { get => allowAutoAttack; set => allowAutoAttack = value; }

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
        public void VerifySystemAbilities() {
            BaseAbility testAbility = null;
            if (levelUpAbilityName != null && levelUpAbilityName != string.Empty) {
                testAbility = SystemAbilityManager.MyInstance.GetResource(levelUpAbilityName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.VerifySystemAbilities(): " + levelUpAbilityName + " COULD NOT BE FOUND IN FACTORY.  CHECK INSPECTOR");
                    return;
                } else {
                    levelUpAbility = testAbility;
                }
            }
            if (deathAbilityName != null && deathAbilityName != string.Empty) {
                testAbility = SystemAbilityManager.MyInstance.GetResource(deathAbilityName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.VerifySystemAbilities(): " + deathAbilityName + " COULD NOT BE FOUND IN FACTORY.  CHECK INSPECTOR");
                    return;
                } else {
                    deathAbility = testAbility;
                }
            }
            if (lootSparkleAbilityName != null && lootSparkleAbilityName != string.Empty) {
                testAbility = SystemAbilityManager.MyInstance.GetResource(lootSparkleAbilityName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.VerifySystemAbilities(): " + lootSparkleAbilityName + " COULD NOT BE FOUND IN FACTORY.  CHECK INSPECTOR");
                    return;
                } else {
                    lootSparkleAbility = testAbility;
                }
            }
            if (doWhiteDamageAbilityName != null && doWhiteDamageAbilityName != string.Empty) {
                testAbility = SystemAbilityManager.MyInstance.GetResource(doWhiteDamageAbilityName);
                if (testAbility == null) {
                    Debug.LogError("SystemConfigurationManager.VerifySystemAbilities(): " + doWhiteDamageAbilityName + " COULD NOT BE FOUND IN FACTORY.  CHECK INSPECTOR");
                    return;
                } else {
                    doWhiteDamageAbility = testAbility;
                }
            }
            if (defaultCurrencyGroup == null) {
                Debug.LogError("SystemConfigurationManager.VerifySystemAbilities(): NO DEFAULT CURRENCY GROUP SET.  CHECK INSPECTOR");
                return;
            }
            
            if (defaultAnimationProfile == null) {
                Debug.LogError("SystemConfigurationManager.VerifySystemAbilities(): NO DEFAULT ANIMATION PROFILE SET.  CHECK INSPECTOR");
                return;
            }
        }

    }

}