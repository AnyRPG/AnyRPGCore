using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BaseCharacter : MonoBehaviour, IAbilityCaster, ICapabilityConsumer {

        //public event System.Action<string> OnNameChange = delegate { };
        //public event System.Action<string> OnTitleChange = delegate { };
        //public event System.Action<Faction> OnFactionChange = delegate { };
        //public event System.Action<CharacterClass, CharacterClass> OnClassChange = delegate { };
        //public event System.Action<CharacterRace, CharacterRace> OnRaceChange = delegate { };
        //public event System.Action<ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        //public event System.Action<UnitType, UnitType> OnUnitTypeChange = delegate { };

        [Tooltip("The name of the unit profile used to configure this character")]
        [SerializeField]
        private string unitProfileName;

        // properties that come from the unit profile
        private string characterName = string.Empty;
        private string title = string.Empty;
        private Faction faction = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private bool spawnDead = false;
        private UnitToughness unitToughness = null;

        private UnitProfile unitProfile = null;

        // common access to properties that provide stats
        private List<IStatProvider> statProviders = new List<IStatProvider>();

        // properties
        private CharacterCombat characterCombat = null;
        private CharacterAbilityManager characterAbilityManager = null;
        private CharacterSkillManager characterSkillManager = null;
        private CharacterPetManager characterPetManager = null;
        private CharacterFactionManager characterFactionManager = null;
        private CharacterEquipmentManager characterEquipmentManager = null;
        private CharacterStats characterStats = null;
        private CharacterCurrencyManager characterCurrencyManager = null;
        private CharacterRecipeManager characterRecipeManager = null;

        // logic for processing capabilities lives here
        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        // components
        private UnitController unitController = null;

        //private bool animationEnabled = false;

        // track state
        private bool characterInitialized = false;
        private bool eventSubscriptionsInitialized = false;

        public CharacterStats CharacterStats { get => characterStats; }
        public CharacterCombat CharacterCombat { get => characterCombat; }
        public UnitController UnitController { get => unitController; }
        public CharacterAbilityManager CharacterAbilityManager { get => characterAbilityManager; }
        public IAbilityManager AbilityManager { get => characterAbilityManager; }
        public CharacterSkillManager CharacterSkillManager { get => characterSkillManager; }
        public CharacterFactionManager CharacterFactionManager { get => characterFactionManager; set => characterFactionManager = value; }
        public CharacterEquipmentManager CharacterEquipmentManager { get => characterEquipmentManager; set => characterEquipmentManager = value; }

        public string CharacterName { get => characterName; }
        public string MyName { get => CharacterName; }

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; }
        public CharacterClass CharacterClass { get => characterClass; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; }
        public Faction Faction {
            get {
                if (UnitController != null && UnitController.UnderControl) {
                    //Debug.Log(gameObject.name + ".MyFactionName: return master unit faction name");
                    return UnitController.MasterUnit.Faction;
                }
                return faction;
            }
        }

        public CharacterPetManager CharacterPetManager { get => characterPetManager; set => characterPetManager = value; }
        public bool SpawnDead { get => spawnDead; set => spawnDead = value; }
        public string Title { get => title; set => title = value; }
        public List<IStatProvider> StatProviders { get => statProviders; set => statProviders = value; }
        public UnitToughness UnitToughness { get => unitToughness; set => unitToughness = value; }
        public CharacterRecipeManager CharacterRecipeManager { get => characterRecipeManager; set => characterRecipeManager = value; }
        public CharacterCurrencyManager CharacterCurrencyManager { get => characterCurrencyManager; set => characterCurrencyManager = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }
        public string UnitProfileName { get => unitProfileName; }

        /*
        private void Awake() {
            Debug.Log(gameObject.name + ": BaseCharacter.Awake()");

            // react to level load and unload events
            CreateEventSubscriptions();

            // find out if this character is on a unit
            GetComponentReferences();

            // get reference to any hard coded unit profile
            SetupScriptableObjects();

            // setup the objects that handle different character mechanics
            CreateCharacterComponents();

        }
        */

        // baseCharacter does not initialize itself.  It is initialized by the PlayerManager (player case), or the UnitController (AI case)
        public void Init() {
            //Debug.Log(gameObject.name + ".BaseCharacter.Init()");

            // react to level load and unload events
            CreateEventSubscriptions();

            // find out if this character is on a unit
            GetComponentReferences();

            // get reference to any hard coded unit profile
            // testing : disabled for now.  let this happen later in the process
            //SetupScriptableObjects();

            // setup the objects that handle different character mechanics
            CreateCharacterComponents();

            InitCharacterComponents();

            //SetUnitProfileProperties();

            characterInitialized = true;
        }

        public void CreateCharacterComponents() {

            // get character components ready for intitalization by allowing them to construct needed internal objects and references back to the character
            capabilityConsumerProcessor = new CapabilityConsumerProcessor(this);
            characterStats = new CharacterStats(this);
            characterEquipmentManager = new CharacterEquipmentManager(this);
            characterFactionManager = new CharacterFactionManager(this);
            characterPetManager = new CharacterPetManager(this);
            characterCombat = new CharacterCombat(this);
            characterSkillManager = new CharacterSkillManager(this);
            characterCurrencyManager = new CharacterCurrencyManager(this);
            characterRecipeManager = new CharacterRecipeManager(this);
            characterAbilityManager = new CharacterAbilityManager(this);
        }

        public void InitCharacterComponents() {
            // characterStats needs a chance to set level and spawn dead
            characterStats.Init();

            // equipment manager should load default equipment
            // moved to direct call from setunitprofile since this call happens before any unit profile is set
            //characterEquipmentManager.Init();

            // learn any skills for the level
            characterSkillManager.Init();

            // learn recipes for the level for any known skills
            characterRecipeManager.Init();

            // learn abilities for the level
            characterAbilityManager.Init();
        }


        public void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
            eventSubscriptionsInitialized = true;
        }

        public void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
                SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
            }
        }

        public void ApplyCapabilityConsumerSnapshot(CapabilityConsumerSnapshot capabilityConsumerSnapshot) {

            // get initial snapshot
            CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(this);

            // there is no need to perform notifications since the level is not loaded and the player isn't physically spawned yet
            SetUnitProfile(capabilityConsumerSnapshot.UnitProfile, false);
            SetCharacterRace(capabilityConsumerSnapshot.CharacterRace, false, false);
            SetCharacterFaction(capabilityConsumerSnapshot.Faction, false, false);
            SetCharacterClass(capabilityConsumerSnapshot.CharacterClass, false, false);
            SetClassSpecialization(capabilityConsumerSnapshot.ClassSpecialization, false, false);

            UpdateStatProviderList();
            CapabilityConsumerProcessor.UpdateCapabilityProviderList();

            // get updated snapshot
            CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this);

            ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot);

            // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
            CharacterStats.SetLevel(CharacterStats.Level);
        }

        public void SetUnitController(UnitController unitController) {
            this.unitController = unitController;
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            characterStats.ProcessLevelUnload();
            characterAbilityManager.ProcessLevelUnload();
            characterCombat.ProcessLevelUnload();
        }

        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            characterStats.ProcessLevelLoad();
        }

        public virtual void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".BaseCharacter.GetComponentReferences()");

            unitController = GetComponent<UnitController>();
        }

        public void JoinFaction(Faction newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newFaction != null && newFaction != faction) {
                SetCharacterFaction(newFaction);
            }
        }

        public void ChangeClassSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newClassSpecialization != null && newClassSpecialization != classSpecialization) {
                SetClassSpecialization(newClassSpecialization);
            }
        }

        public void ChangeCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newCharacterClass != null && newCharacterClass != characterClass) {
                SetCharacterClass(newCharacterClass);
            }
        }

        /*
        public void ChangeCharacterRace(CharacterRace newCharacterRace) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newCharacterRace != null && newCharacterRace != characterRace) {
                SetCharacterRace(newCharacterRace);
            }
        }
        */

        public void SetUnitProfile(string unitProfileName, bool notify = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitProfile(" + unitProfileName + ")");

            SetUnitProfile(UnitProfile.GetUnitProfileReference(unitProfileName), notify);
        }

        public void SetUnitProfile (UnitProfile unitProfile, bool notify = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitProfile(" + (unitProfile == null ? "null" : unitProfile.DisplayName) + ")");

            // get a snapshot of the current state
            CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(this);

            // set the new unit profile
            this.unitProfile = unitProfile;

            // get a snapshot of the new state
            CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this);

            if (notify) {
                ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot);
            }

            SetUnitProfileProperties(notify);
        }

        /// <summary>
        /// This will retrieve a unit profile from the system unit profile manager
        /// </summary>
        private void SetUnitProfileProperties(bool notify = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitProfileProperties()");

            if (unitProfile != null) {
                if (unitProfile.CharacterName != null && unitProfile.CharacterName != string.Empty) {
                    SetCharacterName(unitProfile.CharacterName, notify);
                }
                if (unitProfile.Title != null && unitProfile.Title != string.Empty) {
                    SetCharacterTitle(unitProfile.Title, notify);
                }
                if (unitProfile.UnitType != null) {
                    SetUnitType(unitProfile.UnitType, notify, false);
                }
                if (unitProfile.CharacterRace != null) {
                    SetCharacterRace(unitProfile.CharacterRace, notify, false);
                }
                if (unitProfile.CharacterClass != null) {
                    SetCharacterClass(unitProfile.CharacterClass, notify, false);
                }
                if (unitProfile.ClassSpecialization != null) {
                    SetClassSpecialization(unitProfile.ClassSpecialization, notify, false);
                }
                if (unitProfile.Faction != null) {
                    SetCharacterFaction(unitProfile.Faction, notify, false);
                }
                if (unitProfile.DefaultToughness != null) {
                    SetUnitToughness(unitProfile.DefaultToughness);
                }
                spawnDead = unitProfile.SpawnDead;
            }

            if (notify) {
                capabilityConsumerProcessor.UpdateCapabilityProviderList();

                characterEquipmentManager.LoadDefaultEquipment();

                UpdateStatProviderList();

                if (characterStats != null) {
                    // cause stats to be recalculated
                    characterStats.SetLevel(characterStats.Level);
                }
            }

        }

        public void Update() {
            if (!characterInitialized) {
                return;
            }

            // no need to update if the player is not spawned or this is disabled due to player unit control
            if (unitController == null) {
                return;
            }

            // no need to update if this is a preview unit
            if (unitController.UnitControllerMode == UnitControllerMode.Preview) {
                return;
            }

            characterCombat.Update();

            // do this after combat so regen ticks can use the proper combat state
            characterStats.Update();
        }

        public void UpdateStatProviderList() {
            statProviders = new List<IStatProvider>();
            if (unitProfile != null) {
                statProviders.Add(unitProfile);
            }
            if (unitType != null) {
                statProviders.Add(unitType);
            }
            if (characterRace != null) {
                statProviders.Add(characterRace);
            }
            if (characterClass != null) {
                statProviders.Add(characterClass);
            }
            if (classSpecialization != null) {
                statProviders.Add(classSpecialization);
            }

            if (characterStats != null) {
                characterStats.HandleUpdateStatProviders();
            }
        }

        public void Initialize(string characterName, int characterLevel = 1) {
            //Debug.Log(gameObject.name + ": BaseCharacter.Initialize()");
            this.characterName = characterName;
            characterStats.SetLevel(characterLevel);
        }

        public void SetUnitToughness(UnitToughness newUnitToughness) {
            unitToughness = newUnitToughness;
        }

        public void SetCharacterName(string newName, bool notify = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharactername(" + newName + ")");
            if (newName != null && newName != string.Empty) {
                characterName = newName;
                //OnNameChange(newName);
                if (unitController != null && notify == true) {
                    UnitController.NotifyOnNameChange(newName);
                }
            }
        }

        public void SetCharacterTitle(string newTitle, bool notify = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");
            if (newTitle != null) {
                title = newTitle;
                //OnTitleChange(newTitle);
                if (unitController != null && notify == true) {
                    unitController.NotifyOnTitleChange(newTitle);
                }
            }
        }

        public void SetCharacterFaction(Faction newFaction, bool notify = true, bool resetStats = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");

            CapabilityConsumerSnapshot oldSnapshot = null;

            if (newFaction != null) {
                if (notify) {
                    // get a snapshot of the current state
                    oldSnapshot = new CapabilityConsumerSnapshot(this);
                }
                Faction oldFaction = faction;
                faction = newFaction;
                if (notify) {

                    capabilityConsumerProcessor.UpdateCapabilityProviderList();

                    // get a snapshot of the new state
                    CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this);

                    UpdateStatProviderList();

                    // update capabilities based on the difference between old and new snapshots
                    ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot);

                    if (unitController != null) {
                        unitController.NotifyOnFactionChange(newFaction, oldFaction);
                    }
                }
                characterFactionManager.SetReputation(newFaction);
            }

            // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
            if (resetStats == true && characterStats != null) {
                characterStats.SetLevel(characterStats.Level);
            }
        }

        public void SetClassSpecialization(ClassSpecialization newClassSpecialization, bool notify = true, bool resetStats = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newCharacterClassName + ")");

            CapabilityConsumerSnapshot oldSnapshot = null;

            if (newClassSpecialization != null) {
                if (notify) {
                    // get a snapshot of the current state
                    oldSnapshot = new CapabilityConsumerSnapshot(this);
                }
                ClassSpecialization oldClassSpecialization = classSpecialization;
                classSpecialization = newClassSpecialization;

                if (notify) {

                    capabilityConsumerProcessor.UpdateCapabilityProviderList();

                    // get a snapshot of the new state
                    CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this);

                    UpdateStatProviderList();

                    // update capabilities based on the difference between old and new snapshots
                    ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot);

                    if (unitController != null) {
                        unitController.NotifyOnSpecializationChange(newClassSpecialization, oldClassSpecialization);
                    }
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true && characterStats != null) {
                    characterStats.SetLevel(characterStats.Level);
                }
            }
        }

        public void SetCharacterClass(CharacterClass newCharacterClass, bool notify = true, bool resetStats = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterClass(" + (newCharacterClass != null ? newCharacterClass.MyName : "null") + ", " + notify + ")");

            CapabilityConsumerSnapshot oldSnapshot = null;

            if (newCharacterClass != null) {
                if (notify) {
                    // get a snapshot of the current state
                    oldSnapshot = new CapabilityConsumerSnapshot(this);
                }
                CharacterClass oldCharacterClass = characterClass;
                characterClass = newCharacterClass;
                if (notify) { 
                    capabilityConsumerProcessor.UpdateCapabilityProviderList();

                    // get a snapshot of the new state
                    CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this);

                    UpdateStatProviderList();

                    // update capabilities based on the difference between old and new snapshots
                    ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot);

                    if (unitController != null) {
                        unitController.NotifyOnClassChange(newCharacterClass, oldCharacterClass);
                    }
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true && characterStats != null) {
                    characterStats.SetLevel(characterStats.Level);
                }
            }
        }

        public void SetCharacterRace(CharacterRace newCharacterRace, bool notify = true, bool resetStats = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterClass(" + (newCharacterClass != null ? newCharacterClass.MyName : "null") + ", " + notify + ")");

            CapabilityConsumerSnapshot oldSnapshot = null;

            if (newCharacterRace != null) {

                if (notify) {
                    // get a snapshot of the current state
                    oldSnapshot = new CapabilityConsumerSnapshot(this);
                }

                CharacterRace oldCharacterRace = characterRace;
                characterRace = newCharacterRace;

                if (notify) {
                    capabilityConsumerProcessor.UpdateCapabilityProviderList();

                    // get a snapshot of the new state
                    CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this);

                    UpdateStatProviderList();

                    if (characterStats != null) {
                        characterStats.HandleUpdateStatProviders();
                    }
                    // give equipment manager time to remove equipment that this class cannot equip and ability manager time to apply class traits
                    //OnRaceChange(newCharacterRace, oldCharacterRace);

                    // update capabilities based on the difference between old and new snapshots
                    ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot);

                    if (unitController != null) {
                        unitController.NotifyOnRaceChange(newCharacterRace, oldCharacterRace);
                    }
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true && characterStats != null) {
                    characterStats.SetLevel(characterStats.Level);
                }
            }
        }

        public void SetUnitType(UnitType newUnitType, bool notify = true, bool resetStats = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitType(" + (newUnitType != null ? newUnitType.DisplayName : "null") + ", " + notify + ")");

            CapabilityConsumerSnapshot oldSnapshot = null;

            if (newUnitType != null) {
                if (notify) {
                    // get a snapshot of the current state
                    oldSnapshot = new CapabilityConsumerSnapshot(this);
                }
                UnitType oldUnitType = unitType;
                unitType = newUnitType;
                if (notify) {

                    capabilityConsumerProcessor.UpdateCapabilityProviderList();

                    // get a snapshot of the new state
                    CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this);

                    UpdateStatProviderList();

                    // update capabilities based on the difference between old and new snapshots
                    ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot);

                    if (unitController != null) {
                        unitController.NotifyOnUnitTypeChange(newUnitType, oldUnitType);
                    }
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true && characterStats != null) {
                    characterStats.SetLevel(characterStats.Level);
                }
            }
        }

        public void ProcessCapabilityConsumerChange(CapabilityConsumerSnapshot oldSnapshot, CapabilityConsumerSnapshot newSnapshot) {
            //Debug.Log(gameObject.name + ".BaseCharacter.ProcessCapabilityConsumerChange()");
            characterEquipmentManager.UnequipUnwearableEquipment();
            characterAbilityManager.HandleCapabilityProviderChange(oldSnapshot, newSnapshot);
        }

        public void HandleCharacterUnitSpawn() {
            //Debug.Log(gameObject.name + ".BaseCharacter.HandleCharacterUnitSpawn()");
            characterEquipmentManager.HandleCharacterUnitSpawn();
        }

        public void DespawnImmediate() {
            //Debug.Log(gameObject.name + ".BaseCharacter.DespawnImmediate()");
            if (unitController != null && unitController.CharacterUnit != null) {
                unitController.CharacterUnit.Despawn(0, false, true);
            }
        }

        public void Despawn() {
            //Debug.Log(gameObject.name + ".BaseCharacter.Despawn()");
            if (unitController != null && unitController.CharacterUnit != null) {
                unitController.CharacterUnit.Despawn();
            }
        }

        public void TryToDespawn() {
            //Debug.Log(gameObject.name + ".BaseCharacter.TryToDespawn()");
            if (unitProfile != null && unitProfile.PreventAutoDespawn == true) {
                return;
            }
            if (unitController != null && unitController.LootableCharacter != null) {
                // lootable character handles its own despawn logic
                return;
            }
            Despawn();
        }

        public void SetupScriptableObjects() {
            /*
            if (unitProfileName != null && unitProfileName != string.Empty) {
                SetUnitProfile(UnitProfile.GetUnitProfileReference(unitProfileName));
            }
            */
        }

        public void OnDestroy() {
            StopAllCoroutines();
            CleanupEventSubscriptions();
        }

    }

}