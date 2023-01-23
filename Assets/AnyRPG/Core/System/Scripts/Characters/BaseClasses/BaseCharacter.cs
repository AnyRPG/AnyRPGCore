using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BaseCharacter : ConfiguredMonoBehaviour, IAbilityCaster, ICapabilityConsumer {

        //public event System.Action<string> OnNameChange = delegate { };
        //public event System.Action<string> OnTitleChange = delegate { };
        //public event System.Action<Faction> OnFactionChange = delegate { };
        //public event System.Action<CharacterClass, CharacterClass> OnClassChange = delegate { };
        //public event System.Action<CharacterRace, CharacterRace> OnRaceChange = delegate { };
        //public event System.Action<ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        //public event System.Action<UnitType, UnitType> OnUnitTypeChange = delegate { };

        [Tooltip("The name of the unit profile used to configure this character")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
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

        // components
        private CharacterCombat characterCombat = null;
        private CharacterAbilityManager characterAbilityManager = null;
        private CharacterSkillManager characterSkillManager = null;
        private CharacterPetManager characterPetManager = null;
        private CharacterFactionManager characterFactionManager = null;
        private CharacterEquipmentManager characterEquipmentManager = null;
        private CharacterStats characterStats = null;
        private CharacterCurrencyManager characterCurrencyManager = null;
        private CharacterRecipeManager characterRecipeManager = null;
        private CharacterInventoryManager characterInventoryManager = null;

        // logic for processing capabilities lives here
        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        // components
        private UnitController unitController = null;

        // track state
        private bool characterInitialized = false;
        private bool eventSubscriptionsInitialized = false;

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public CharacterStats CharacterStats { get => characterStats; }
        public CharacterCombat CharacterCombat { get => characterCombat; }
        public UnitController UnitController { get => unitController; }
        public CharacterAbilityManager CharacterAbilityManager { get => characterAbilityManager; }
        public IAbilityManager AbilityManager { get => characterAbilityManager; }
        public CharacterSkillManager CharacterSkillManager { get => characterSkillManager; }
        public CharacterFactionManager CharacterFactionManager { get => characterFactionManager; set => characterFactionManager = value; }
        public CharacterEquipmentManager CharacterEquipmentManager { get => characterEquipmentManager; set => characterEquipmentManager = value; }
        public CharacterInventoryManager CharacterInventoryManager { get => characterInventoryManager; }


        public string CharacterName { get => characterName; }

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

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log(gameObject.name + ".BaseCharacter.Configure()");

            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

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
            capabilityConsumerProcessor = new CapabilityConsumerProcessor(this, systemGameManager);
            characterStats = new CharacterStats(this, systemGameManager);
            characterEquipmentManager = new CharacterEquipmentManager(this, systemGameManager);
            characterFactionManager = new CharacterFactionManager(this);
            characterPetManager = new CharacterPetManager(this, systemGameManager);
            characterCombat = new CharacterCombat(this, systemGameManager);
            characterSkillManager = new CharacterSkillManager(this, systemGameManager);
            characterCurrencyManager = new CharacterCurrencyManager(this, systemGameManager);
            characterRecipeManager = new CharacterRecipeManager(this, systemGameManager);
            characterAbilityManager = new CharacterAbilityManager(this, systemGameManager);
            characterInventoryManager = new CharacterInventoryManager(this, systemGameManager);
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
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
        }

        // currently this is only used for load game panel and loading game, so it's always a player
        public void ApplyCapabilityConsumerSnapshot(CapabilityConsumerSnapshot newCapabilityConsumerSnapshot) {
            //Debug.Log(gameObject.name + ".ApplyCapabilityConsumerSnapshot()");

            // get initial snapshot
            //CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
            // testing - create snapshot without system abilities so they will be learned in the snapshot below
            CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(systemGameManager);

            // there is no need to perform notifications since the level is not loaded and the player isn't physically spawned yet
            // do not let unit profile load provider equipment.  player equipment was decided by the new game panel or saved equipment if loaded from save file
            SetUnitProfile(newCapabilityConsumerSnapshot.UnitProfile, false, -1, false);

            SetCharacterRace(newCapabilityConsumerSnapshot.CharacterRace, false, false);
            SetCharacterFaction(newCapabilityConsumerSnapshot.Faction, false, false);
            SetCharacterClass(newCapabilityConsumerSnapshot.CharacterClass, false, false);
            SetClassSpecialization(newCapabilityConsumerSnapshot.ClassSpecialization, false, false);

            UpdateStatProviderList();
            CapabilityConsumerProcessor.UpdateCapabilityProviderList();

            // get updated snapshot
            CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);

            ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot);

            // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
            CharacterStats.SetLevel(CharacterStats.Level);
        }

        public void SetUnitController(UnitController unitController) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitController(" + unitController.gameObject.name + ")");
            this.unitController = unitController;
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
            if (newClassSpecialization != classSpecialization) {
                characterStats.ClearStatusEffects();
                characterPetManager.DespawnAllPets();
                SetClassSpecialization(newClassSpecialization);
            }
        }

        public void ChangeCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newCharacterClass != null && newCharacterClass != characterClass) {
                characterStats.ClearStatusEffects();
                characterPetManager.DespawnAllPets();
                ChangeClassSpecialization(null);
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

        public void SetUnitProfile(string unitProfileName, bool notify = true, int unitLevel = -1, bool loadProviderEquipment = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitProfile(" + unitProfileName + ")");

            SetUnitProfile(systemDataFactory.GetResource<UnitProfile>(unitProfileName), notify, unitLevel, loadProviderEquipment);
        }

        public void SetUnitProfile (UnitProfile unitProfile, bool notify = true, int unitLevel = -1, bool loadProviderEquipment = true, bool processEquipmentRestrictions = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitProfile(" + (unitProfile == null ? "null" : unitProfile.DisplayName) + ", " + notify + ", " + unitLevel + ", " + loadProviderEquipment + ", " + processEquipmentRestrictions + ")");

            // get a snapshot of the current state
            CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);

            // set the new unit profile
            this.unitProfile = unitProfile;

            // get a snapshot of the new state
            CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);

            if (notify) {
                ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot, processEquipmentRestrictions);
            }

            SetUnitProfileProperties(notify, unitLevel, loadProviderEquipment, processEquipmentRestrictions);

            // Trying to spawn dead relies on reading properties set in the previous method
            characterStats.TrySpawnDead();
        }

        /// <summary>
        /// This will retrieve a unit profile from the system unit profile manager
        /// </summary>
        private void SetUnitProfileProperties(bool notify = true, int unitLevel = -1, bool loadProviderEquipment = true, bool processEquipmentRestrictions = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitProfileProperties(" + notify + ", " + unitLevel + ", " + loadProviderEquipment + ")");

            if (unitProfile != null) {
                if (unitProfile.CharacterName != null && unitProfile.CharacterName != string.Empty) {
                    SetCharacterName(unitProfile.CharacterName, notify);
                }
                if (unitProfile.Title != null && unitProfile.Title != string.Empty) {
                    SetCharacterTitle(unitProfile.Title, notify);
                }
                if (unitProfile.UnitType != null) {
                    SetUnitType(unitProfile.UnitType, notify, false, processEquipmentRestrictions);
                }
                if (unitProfile.CharacterRace != null) {
                    SetCharacterRace(unitProfile.CharacterRace, notify, false, processEquipmentRestrictions);
                }
                if (unitProfile.CharacterClass != null) {
                    SetCharacterClass(unitProfile.CharacterClass, notify, false, processEquipmentRestrictions);
                }
                if (unitProfile.ClassSpecialization != null) {
                    SetClassSpecialization(unitProfile.ClassSpecialization, notify, false, processEquipmentRestrictions);
                }
                if (unitProfile.Faction != null) {
                    SetCharacterFaction(unitProfile.Faction, notify, false, processEquipmentRestrictions);
                }
                if (unitProfile.DefaultToughness != null) {
                    SetUnitToughness(unitProfile.DefaultToughness);
                }
                spawnDead = unitProfile.SpawnDead;

            }

            if (notify) {
                capabilityConsumerProcessor.UpdateCapabilityProviderList();

                UpdateStatProviderList();

                if (characterStats != null) {
                    // cause stats to be recalculated
                    int newLevel = characterStats.Level;
                    if (unitLevel != -1) {
                        newLevel = unitLevel;
                    }
                    characterStats.SetLevel(newLevel);
                }

                // this must be called after setting the level in case the character has gear that is higher than level 1
                characterEquipmentManager.LoadDefaultEquipment(loadProviderEquipment);

            }

            // now that equipment has had a chance to be equipped, give character combat a chance to add default unit profile hit effects
            // in case no weapons were equipped
            characterCombat.AddUnitProfileHitEffects();
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

            characterCombat?.Update();

            // do this after combat so regen ticks can use the proper combat state
            characterStats?.Update();
        }

        public void UpdateStatProviderList() {
            statProviders = new List<IStatProvider>();
            statProviders.Add(systemConfigurationManager);
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
            characterInventoryManager.PerformSetupActivities();
        }

        public void SetUnitToughness(UnitToughness newUnitToughness, bool resetLevel = false) {
            //Debug.Log(gameObject.name + ": BaseCharacter.SetUnitToughness(" + (newUnitToughness == null ? "null" : newUnitToughness.DisplayName) + ")");
            unitToughness = newUnitToughness;
            if (resetLevel) {
                characterStats.SetLevel(characterStats.Level);
            }
        }

        public void SetCharacterName(string newName, bool notify = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharactername(" + newName + ")");
            if (newName != null && newName != string.Empty) {
                characterName = newName;
                //OnNameChange(newName);
                if (unitController != null && notify == true) {
                    UnitController.UnitEventController.NotifyOnNameChange(newName);
                }
            }
        }

        public void SetCharacterTitle(string newTitle, bool notify = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");
            if (newTitle != null) {
                title = newTitle;
                //OnTitleChange(newTitle);
                if (unitController != null && notify == true) {
                    unitController.UnitEventController.NotifyOnTitleChange(newTitle);
                }
            }
        }

        public void SetCharacterFaction(Faction newFaction, bool notify = true, bool resetStats = true, bool processEquipmentRestrictions = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");

            CapabilityConsumerSnapshot oldSnapshot = null;

            if (newFaction != null) {
                if (notify) {
                    // get a snapshot of the current state
                    oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                }
                Faction oldFaction = faction;
                faction = newFaction;
                if (notify) {

                    capabilityConsumerProcessor.UpdateCapabilityProviderList();

                    // get a snapshot of the new state
                    CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);

                    UpdateStatProviderList();

                    // update capabilities based on the difference between old and new snapshots
                    ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot, processEquipmentRestrictions);

                    if (unitController != null) {
                        unitController.UnitEventController.NotifyOnFactionChange(newFaction, oldFaction);
                    }
                }
                characterFactionManager.SetReputation(newFaction);
            }

            // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
            if (resetStats == true && characterStats != null) {
                characterStats.SetLevel(characterStats.Level);
            }
        }

        public void SetClassSpecialization(ClassSpecialization newClassSpecialization, bool notify = true, bool resetStats = true, bool processEquipmentRestrictions = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newCharacterClassName + ")");

            CapabilityConsumerSnapshot oldSnapshot = null;

            //if (newClassSpecialization != null) {
                if (notify) {
                    // get a snapshot of the current state
                    oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                }
                ClassSpecialization oldClassSpecialization = classSpecialization;
                classSpecialization = newClassSpecialization;

                if (notify) {

                    capabilityConsumerProcessor.UpdateCapabilityProviderList();

                    // get a snapshot of the new state
                    CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);

                    UpdateStatProviderList();

                    // update capabilities based on the difference between old and new snapshots
                    ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot, processEquipmentRestrictions);

                    if (unitController != null) {
                        unitController.UnitEventController.NotifyOnSpecializationChange(newClassSpecialization, oldClassSpecialization);
                    }
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true && characterStats != null) {
                    characterStats.SetLevel(characterStats.Level);
                }
            //}
        }

        public void SetCharacterClass(CharacterClass newCharacterClass, bool notify = true, bool resetStats = true, bool processEquipmentRestrictions = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterClass(" + (newCharacterClass != null ? newCharacterClass.DisplayName : "null") + ", " + notify + ", " + resetStats + ", " + processEquipmentRestrictions + ")");

            CapabilityConsumerSnapshot oldSnapshot = null;

            if (newCharacterClass != null) {
                if (notify) {
                    // get a snapshot of the current state
                    oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                }
                CharacterClass oldCharacterClass = characterClass;
                characterClass = newCharacterClass;
                if (notify) { 
                    capabilityConsumerProcessor.UpdateCapabilityProviderList();

                    // get a snapshot of the new state
                    CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);

                    UpdateStatProviderList();

                    // update capabilities based on the difference between old and new snapshots
                    ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot, processEquipmentRestrictions);

                    if (unitController != null) {
                        unitController.UnitEventController.NotifyOnClassChange(newCharacterClass, oldCharacterClass);
                    }
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true && characterStats != null) {
                    characterStats.SetLevel(characterStats.Level);
                }
            }
        }

        public void SetCharacterRace(CharacterRace newCharacterRace, bool notify = true, bool resetStats = true, bool processEquipmentRestrictions = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterClass(" + (newCharacterClass != null ? newCharacterClass.DisplayName : "null") + ", " + notify + ")");

            CapabilityConsumerSnapshot oldSnapshot = null;

            if (newCharacterRace != null) {

                if (notify) {
                    // get a snapshot of the current state
                    oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                }

                CharacterRace oldCharacterRace = characterRace;
                characterRace = newCharacterRace;

                if (notify) {
                    capabilityConsumerProcessor.UpdateCapabilityProviderList();

                    // get a snapshot of the new state
                    CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);

                    UpdateStatProviderList();

                    if (characterStats != null) {
                        characterStats.HandleUpdateStatProviders();
                    }
                    // give equipment manager time to remove equipment that this class cannot equip and ability manager time to apply class traits
                    //OnRaceChange(newCharacterRace, oldCharacterRace);

                    // update capabilities based on the difference between old and new snapshots
                    ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot, processEquipmentRestrictions);

                    if (unitController != null) {
                        unitController.UnitEventController.NotifyOnRaceChange(newCharacterRace, oldCharacterRace);
                    }
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true && characterStats != null) {
                    characterStats.SetLevel(characterStats.Level);
                }
            }
        }

        public void SetUnitType(UnitType newUnitType, bool notify = true, bool resetStats = true, bool processEquipmentRestrictions = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitType(" + (newUnitType != null ? newUnitType.DisplayName : "null") + ", " + notify + ")");

            CapabilityConsumerSnapshot oldSnapshot = null;

            if (newUnitType != null) {
                if (notify) {
                    // get a snapshot of the current state
                    oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                }
                UnitType oldUnitType = unitType;
                unitType = newUnitType;
                if (notify) {

                    capabilityConsumerProcessor.UpdateCapabilityProviderList();

                    // get a snapshot of the new state
                    CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);

                    UpdateStatProviderList();

                    // update capabilities based on the difference between old and new snapshots
                    ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot, processEquipmentRestrictions);

                    if (unitController != null) {
                        unitController.UnitEventController.NotifyOnUnitTypeChange(newUnitType, oldUnitType);
                    }
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true && characterStats != null) {
                    characterStats.SetLevel(characterStats.Level);
                }
            }
        }

        public void ProcessCapabilityConsumerChange(CapabilityConsumerSnapshot oldSnapshot, CapabilityConsumerSnapshot newSnapshot, bool processEquipmentRestrictions = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.ProcessCapabilityConsumerChange()");
            if (processEquipmentRestrictions == true) {
                characterEquipmentManager.HandleCapabilityConsumerChange();
            }
            characterAbilityManager.HandleCapabilityProviderChange(oldSnapshot, newSnapshot);
            characterPetManager.ProcessCapabilityProviderChange(newSnapshot);
        }

        public void HandleCharacterUnitSpawn() {
            //Debug.Log(gameObject.name + ".BaseCharacter.HandleCharacterUnitSpawn()");
            // no longer necessary - moved to UnitModel -> UnitModelController
            //characterEquipmentManager.HandleCharacterUnitSpawn();
            characterStats.HandleCharacterUnitSpawn();
        }

        public void HandleCharacterUnitDespawn() {
            //Debug.Log(gameObject.name + ".BaseCharacter.HandleCharacterUnitSpawn()");
            // no longer necessary - moved to UnitModel -> UnitModelController
            //characterEquipmentManager.HandleCharacterUnitDespawn();

            // There are multiple situations where a baseCharacter could receive this event after they already despawned.
            // This is because the event that is invoked will not update its list until the loop is complete.
            // Things like pets being despawned or player units being despawned as part of the level unload
            // will result in the object being returned to the pool before the invoke gets to it
            // so it is necessary to check if this character is already uninitialized
            if (characterInitialized == false) {
                return;
            }
            characterStats.HandleCharacterUnitDespawn();
            //characterStats.ProcessLevelUnload();
            characterAbilityManager.HandleCharacterUnitDespawn();
            characterCombat.HandleCharacterUnitDespawn();
            characterPetManager.HandleCharacterUnitDespawn();
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".BaseCharacter.HandleLevelUnload(): instanceID: " + gameObject.GetInstanceID());

            // testing - do nothing here and let the unit call this so that ordering is correct
            //ProcessLevelUnload()
        }

        // moved all this to HandleCharacterUnitDespawn so player characters can call it too, since they don't receive a LevelUnload event due to already being despawned
        /*
    public void ProcessLevelUnload() {
        // There are multiple situations where a baseCharacter could receive this event after they already despawned.
        // This is because the event that is invoked will not update its list until the loop is complete.
        // Things like pets being despawned or player units being despawned as part of the level unload
        // will result in the object being returned to the pool before the invoke gets to it
        // so it is necessary to check if this character is already uninitialized
        if (characterInitialized == false) {
            return;
        }
        characterStats.ProcessLevelUnload();
        characterAbilityManager.ProcessLevelUnload();
        characterCombat.ProcessLevelUnload();
        characterPetManager.ProcessLevelUnload();
        //unitController?.Despawn();
    }
        */

        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            characterStats.ProcessLevelLoad();
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

        public void OnSendObjectToPool() {
            //Debug.Log(gameObject.name + ".BaseCharacter.OnSendObjectToPool()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            StopAllCoroutines();
            CleanupEventSubscriptions();
            ResetSettings();
        }

        private void ResetSettings() {
            //Debug.Log(gameObject.name + ".BaseCharacter.ResetSettings()");
            characterName = string.Empty;
            title = string.Empty;
            faction = null;
            unitType = null;
            characterRace = null;
            characterClass = null;
            classSpecialization = null;
            spawnDead = false;
            unitToughness = null;

            unitProfile = null;

            statProviders = new List<IStatProvider>();

            characterCombat = null;
            characterAbilityManager = null;
            characterSkillManager = null;
            characterPetManager = null;
            characterFactionManager = null;
            characterEquipmentManager = null;
            characterStats = null;
            characterCurrencyManager = null;
            characterRecipeManager = null;

            capabilityConsumerProcessor = null;

            unitController = null;

            characterInitialized = false;
            eventSubscriptionsInitialized = false;
        }

    }

}