using AnyRPG;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
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
        private DirectAbility levelUpAbility;

        [SerializeField]
        private DirectAbility deathAbility;

        [SerializeField]
        private DirectAbility lootSparkleAbility;

        [SerializeField]
        private DirectAbility doWhiteDamageAbility;

        [SerializeField]
        private DirectAbility takeDamageAbility;

        [SerializeField]
        private AudioClip defaultHitSoundEffect;

        [Header("INTERACTABLE CONFIGURATION")]

        [SerializeField]
        private Material temporaryMaterial = null;

        // an image to use if there is more than 1 valid interactable option
        [SerializeField]
        private Sprite multipleInteractionNamePlateImage;

        // a separate image if only crafting is available, but more than 1 craft skill can be shown
        [SerializeField]
        private Sprite multipleCraftNamePlateImage;

        [SerializeField]
        private Sprite bankInteractionPanelImage;

        [SerializeField]
        private Sprite bankNamePlateImage;

        [SerializeField]
        private Sprite questGiverInteractionPanelImage;

        [SerializeField]
        private Sprite questGiverNamePlateImage;

        [SerializeField]
        private Sprite dialogInteractionPanelImage;

        [SerializeField]
        private Sprite dialogNamePlateImage;

        [SerializeField]
        private Sprite nameChangeInteractionPanelImage;

        [SerializeField]
        private Sprite nameChangeNamePlateImage;

        [SerializeField]
        private Sprite cutSceneInteractionPanelImage;

        [SerializeField]
        private Sprite cutSceneNamePlateImage;

        [SerializeField]
        private Sprite lootableCharacterInteractionPanelImage;

        [SerializeField]
        private Sprite lootableCharacterNamePlateImage;

        [SerializeField]
        private Sprite characterCreatorInteractionPanelImage;

        [SerializeField]
        private Sprite characterCreatorNamePlateImage;

        [SerializeField]
        private Sprite factionChangeInteractionPanelImage;

        [SerializeField]
        private Sprite factionChangeNamePlateImage;

        [SerializeField]
        private Sprite vendorInteractionPanelImage;

        [SerializeField]
        private Sprite vendorNamePlateImage;

        [SerializeField]
        private Sprite portalInteractionPanelImage;

        [SerializeField]
        private Sprite portalNamePlateImage;

        [SerializeField]
        private Sprite skillTrainerInteractionPanelImage;

        [SerializeField]
        private Sprite skillTrainerNamePlateImage;

        // the default amount of time before a unit despawns after killed and looted
        [SerializeField]
        private float defaultDespawnTimer;

        protected bool startHasRun = false;
        protected bool eventReferencesInitialized = false;

        public DirectAbility MyLootSparkleAbility { get => lootSparkleAbility; set => lootSparkleAbility = value; }
        public Material MyTemporaryMaterial { get => temporaryMaterial; set => temporaryMaterial = value; }
        public DirectAbility MyLevelUpAbility { get => levelUpAbility; set => levelUpAbility = value; }
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
        public Sprite MyPortalInteractionPanelImage { get => portalInteractionPanelImage; set => portalInteractionPanelImage = value; }
        public Sprite MyPortalNamePlateImage { get => portalNamePlateImage; set => portalNamePlateImage = value; }
        public Sprite MySkillTrainerInteractionPanelImage { get => skillTrainerInteractionPanelImage; set => skillTrainerInteractionPanelImage = value; }
        public Sprite MySkillTrainerNamePlateImage { get => skillTrainerNamePlateImage; set => skillTrainerNamePlateImage = value; }
        public AudioClip MyDefaultHitSoundEffect { get => defaultHitSoundEffect; set => defaultHitSoundEffect = value; }
        public DirectAbility MyDoWhiteDamageAbility { get => doWhiteDamageAbility; set => doWhiteDamageAbility = value; }
        public DirectAbility MyTakeDamageAbility { get => takeDamageAbility; set => takeDamageAbility = value; }
        public DirectAbility MyDeathAbility { get => deathAbility; set => deathAbility = value; }
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

        private void Awake() {
            //Debug.Log("PlayerManager.Awake()");
        }

        private void Start() {
            //Debug.Log("PlayerManager.Start()");
            startHasRun = true;
            CreateEventReferences();
        }

        private void CreateEventReferences() {
            //Debug.Log("PlayerManager.CreateEventReferences()");
            if (eventReferencesInitialized || !startHasRun) {
                return;
            }
            eventReferencesInitialized = true;
        }

        private void CleanupEventReferences() {
            //Debug.Log("PlayerManager.CleanupEventReferences()");
            if (!eventReferencesInitialized) {
                return;
            }
            eventReferencesInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventReferences();
        }

    }

}