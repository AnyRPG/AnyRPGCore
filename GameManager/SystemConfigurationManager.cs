using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UMA;
using UMA.CharacterSystem;

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

    // THESE SHOULD BE MOVED TO SOME TYPE OF GAMECONFIG MANAGER
    [SerializeField]
    private DirectAbility levelUpAbility;

    [SerializeField]
    private DirectAbility lootSparkleAbility;

    [SerializeField]
    private DirectAbility doWhiteDamageAbility;

    [SerializeField]
    private DirectAbility takeDamageAbility;

    [SerializeField]
    private Material temporaryMaterial = null;

    [SerializeField]
    private AudioClip defaultHitSoundEffect;

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
    private Sprite portalInteractionPanelImage;

    [SerializeField]
    private Sprite portalNamePlateImage;

    [SerializeField]
    private Sprite skillTrainerInteractionPanelImage;

    [SerializeField]
    private Sprite skillTrainerNamePlateImage;

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
