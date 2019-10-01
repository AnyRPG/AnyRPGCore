using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharacterUnit : InteractableOption, ICharacterUnit, INamePlateUnit {

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    public event System.Action OnInitializeNamePlate = delegate { };
    public event Action<INamePlateUnit> NamePlateNeedsRemoval = delegate { };
    public event Action<int, int> HealthBarNeedsUpdate = delegate { };
    public event System.Action<GameObject> OnDespawn = delegate { };

    //[SerializeField]
    private BaseCharacter baseCharacter = null;

    [SerializeField]
    protected float despawnDelay;

    private NamePlateController namePlate;

    private NavMeshAgent agent;

    protected Rigidbody rigidBody;

    private CharacterMotor characterMotor;

    private CharacterAnimator characterAnimator;

    /// <summary>
    /// a string that represents the location of the transform in the heirarchy that we will attach the portrait camera to when this character is displayed in a unit frame
    /// </summary>
    [SerializeField]
    private string unitFrameTarget = string.Empty;

    [SerializeField]
    private Vector3 unitFrameCameraLookOffset;

    [SerializeField]
    private Vector3 unitFrameCameraPositionOffset;

    [SerializeField]
    private string playerPreviewTarget = string.Empty;

    [SerializeField]
    private Vector3 playerPreviewInitialOffset;


    public BaseCharacter MyCharacter {
        get => baseCharacter;
        set {
            baseCharacter = value;
            InitializeNamePlate();
        }
    }

    public string MyFactionName { get => MyCharacter.MyFactionName; }
    public NamePlateController MyNamePlate { get => namePlate; set => namePlate = value; }
    public NavMeshAgent MyAgent { get => agent; set => agent = value; }
    public Rigidbody MyRigidBody { get => rigidBody; set => rigidBody = value; }
    public CharacterMotor MyCharacterMotor { get => characterMotor; set => characterMotor = value; }
    public CharacterAnimator MyCharacterAnimator { get => characterAnimator; set => characterAnimator = value; }
    public string MyDisplayName { get => (MyCharacter != null ? MyCharacter.MyCharacterName : interactionPanelTitle); }
    public string MyUnitFrameTarget { get => unitFrameTarget; }
    public string MyPlayerPreviewTarget { get => playerPreviewTarget; }
    public Vector3 MyPlayerPreviewInitialOffset { get => playerPreviewInitialOffset; }
    public Vector3 MyUnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
    public Vector3 MyUnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }
    protected float MyDespawnDelay { get => despawnDelay; set => despawnDelay = value; }
    public BaseCharacter MyBaseCharacter { get => MyCharacter; }

    public bool HasHealth() {
        //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
        return true;
    }

    public void HandleNamePlateNeedsRemoval(CharacterStats _characterStats) {
        //Debug.Log(gameObject.name + ".CharacterUnit.HandleNamePlateNeedsRemoval()");
        if (gameObject != null && _characterStats != null) {
            //Debug.Log(gameObject.name + ".CharacterUnit.HandleNamePlateNeedsRemoval(" + _characterStats + ")");
            NamePlateNeedsRemoval(this as INamePlateUnit);
        }
        //baseCharacter.MyCharacterStats.OnHealthChanged -= HealthBarNeedsUpdate;
    }

    public void HandleHealthBarNeedsUpdate(int currentHealth, int maxHealth) {
        //Debug.Log(gameObject.name + ".CharacterUnit.HandleHealthBarNeedsUpdate(" + currentHealth + ", " + maxHealth + ")");
        HealthBarNeedsUpdate(currentHealth, maxHealth);
    }

    protected override void Awake() {
        //Debug.Log(gameObject.name + ".CharacterUnit.Awake() about to get references to all local components");
        base.Awake();
        // already handled in base.awake
        //GetComponentReferences();
    }

    protected override void Start() {
        //Debug.Log(gameObject.name + ": running Start()");
        base.Start();
        InitializeNamePlate();
        CreateEventReferences();
    }

    public void CreateEventReferences() {
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
            baseCharacter.MyCharacterStats.OnDie += HandleNamePlateNeedsRemoval;
            baseCharacter.MyCharacterStats.OnHealthChanged += HandleHealthBarNeedsUpdate;
            baseCharacter.MyCharacterStats.OnReviveComplete += InitializeNamePlate;
        } else {
            //Debug.Log(gameObject.name + ".CharacterUnit.Start(): baseCharacter is null");
        }
        eventReferencesInitialized = true;
    }

    public override void CleanupEventReferences() {
        //Debug.Log("CharacterUnit.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        base.CleanupEventReferences();

        if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
            baseCharacter.MyCharacterStats.OnDie -= HandleNamePlateNeedsRemoval;
            baseCharacter.MyCharacterStats.OnHealthChanged -= HandleHealthBarNeedsUpdate;
            baseCharacter.MyCharacterStats.OnReviveComplete -= InitializeNamePlate;
        }
        eventReferencesInitialized = false;
    }

    private void OnEnable() {
        //Debug.Log(gameObject.name + ".CharacterUnit.OnEnable()");
        InitializeNamePlate();
        CreateEventReferences();
    }

    public override void OnDisable() {
        //Debug.Log(gameObject.name + ".CharacterUnit.OnDisable()");
        base.OnDisable();
        CleanupEventReferences();
        if (NamePlateManager.MyInstance != null) {
            NamePlateManager.MyInstance.RemoveNamePlate(this as INamePlateUnit);
        }
    }

    public override void GetComponentReferences() {
        //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences()");
        if (componentReferencesInitialized) {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): already initialized. exiting!");
            return;
        }
        base.GetComponentReferences();
        agent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();
        characterMotor = GetComponent<CharacterMotor>();
        characterAnimator = GetComponent<CharacterAnimator>();
        if (baseCharacter == null) {
            baseCharacter = GetComponent<BaseCharacter>();
            if (baseCharacter == null) {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): baseCharacter was null and is still null");
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): baseCharacter was null but is now initialized to: " + baseCharacter.MyCharacterName);
            }
        }
    }

    public void InitializeNamePlate() {
        //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate()");
        if (baseCharacter != null && startHasRun) {
            NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(this);
            if (_namePlate != null) {
                namePlate = _namePlate;
            }
            OnInitializeNamePlate();
        } else {
            //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate(): Character is null or start has not been run yet. exiting.");
            return;
        }
    }

    /// <summary>
    /// The default interaction on any character is to be attacked.  Return true if the relationship is less than 0.
    /// </summary>
    /// <param name="targetCharacter"></param>
    /// <returns></returns>
    public override bool CanInteract(CharacterUnit targetCharacter) {
        //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract(" + targetCharacter.MyName + ")");
        if (targetCharacter == null) {
            //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract(): source is null!!");
            // we must have moused over a healthbar before the player spawned
            return false;
        }
        if (Faction.RelationWith(targetCharacter.MyCharacter, MyBaseCharacter) <= -1 && baseCharacter.MyCharacterStats.IsAlive == true) {
            //Debug.Log(source.name + " can interact with us!");
            return true;
        }
        //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract: " + source.name + " was unable to interact with (attack) us!");
        return false;
    }

    public override bool Interact(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".CharacterUnit.Interact(" + source.name + ")");
        if (CanInteract(source)) {
            source.MyCharacter.MyCharacterCombat.Attack(baseCharacter);
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            return true;
        }
        //return true;
        return false;
    }

    public override void StopInteract() {
        //Debug.Log(gameObject.name + ".CharacterUnit.StopInteract()");
        base.StopInteract();
    }

    public override bool HasMiniMapText() {
        return true;
    }

    public override bool SetMiniMapText(Text text) {
        //Debug.Log(gameObject.name + ".CharacterUnit.SetMiniMapText()");
        if (!base.SetMiniMapText(text)) {
            text.text = "";
            text.color = new Color32(0, 0, 0, 0);
            return false;
        }
        text.text = "o";
        text.fontSize = 50;
        if (baseCharacter != null && baseCharacter.MyFactionName != null && baseCharacter.MyFactionName != string.Empty) {
            text.color = Faction.GetFactionColor(PlayerManager.MyInstance.MyCharacter, baseCharacter);
        }
        return true;
    }

    public void Despawn(float despawnDelay = 0f) {
        //Debug.Log(gameObject.name + ".CharacterUnit.Despawn(" + despawnDelay + ")");
        //gameObject.SetActive(false);
        // TEST ADDING A MANDATORY DELAY
        Destroy(gameObject, Mathf.Clamp(despawnDelay + this.despawnDelay, 0.1f, Mathf.Infinity));
        OnDespawn(gameObject);
    }

    public override string GetDescription() {
        //Debug.Log(gameObject.name + ".CharacterUnit.GetDescription()");
        if (interactionPanelTitle == null || interactionPanelTitle == string.Empty) {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetDescription(): returning " + MyDisplayName);
            return MyDisplayName;
        } else {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetDescription(): returning " + interactionPanelTitle);
            return interactionPanelTitle;
        }
    }

    // TESTING, USE CANINTERACT TO ALLOW ATTACK, BUT GETVALIDOPTIONCOUNT TO SUPPRESS WINDOW
    // MORE TESTING, CHARACTER UNIT ALIVE IS ALWAYS VALID AND CURRENT TO ALLOW ATTACKS
    public override int GetValidOptionCount() {
        return (MyCharacter.MyCharacterStats.IsAlive == true ? 1 : 0);
    }

    public override int GetCurrentOptionCount() {
        return GetValidOptionCount();
    }

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }

}
