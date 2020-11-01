using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BaseCharacter : MonoBehaviour, IAbilityCaster {

        public event System.Action<string> OnNameChange = delegate { };
        public event System.Action<string> OnTitleChange = delegate { };
        public event System.Action<Faction> OnFactionChange = delegate { };
        public event System.Action<CharacterClass, CharacterClass> OnClassChange = delegate { };
        public event System.Action<ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        public event System.Action<UnitType, UnitType> OnUnitTypeChange = delegate { };

        [Tooltip("The name of the unit profile used to configure this character")]
        [SerializeField]
        private string unitProfileName;

        // properties that come from the unit profile
        private string characterName = string.Empty;
        private string title = string.Empty;
        private Faction faction;
        private UnitType unitType;
        private CharacterClass characterClass;
        private ClassSpecialization classSpecialization;
        private bool spawnDead = false;
        private UnitToughness unitToughness;

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

        // components
        private UnitController unitController = null;
        private CharacterUnit characterUnit = null;

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
        public CharacterUnit CharacterUnit { get => characterUnit; set => characterUnit = value; }
        public CharacterFactionManager CharacterFactionManager { get => characterFactionManager; set => characterFactionManager = value; }
        public CharacterEquipmentManager CharacterEquipmentManager { get => characterEquipmentManager; set => characterEquipmentManager = value; }

        public string CharacterName { get => characterName; }
        public string MyName { get => CharacterName; }

        public Faction Faction {

            get {
                if (UnitController != null && UnitController.UnderControl) {
                    //Debug.Log(gameObject.name + ".MyFactionName: return master unit faction name");
                    return UnitController.MasterUnit.Faction;
                }
                return faction;
            }
            set => faction = value;
        }

        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public string MyUnitProfileName { get => unitProfileName; set => unitProfileName = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public CharacterPetManager MyCharacterPetManager { get => characterPetManager; set => characterPetManager = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public bool MySpawnDead { get => spawnDead; set => spawnDead = value; }
        public string Title { get => title; set => title = value; }
        public List<IStatProvider> StatProviders { get => statProviders; set => statProviders = value; }
        public UnitToughness UnitToughness { get => unitToughness; set => unitToughness = value; }
        public CharacterRecipeManager CharacterRecipeManager { get => characterRecipeManager; set => characterRecipeManager = value; }
        public CharacterCurrencyManager CharacterCurrencyManager { get => characterCurrencyManager; set => characterCurrencyManager = value; }

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

            // react to level load and unload events
            CreateEventSubscriptions();

            // find out if this character is on a unit
            GetComponentReferences();

            // get reference to any hard coded unit profile
            SetupScriptableObjects();

            // setup the objects that handle different character mechanics
            CreateCharacterComponents();

            InitCharacterComponents();

            SetUnitProfileProperties();

            characterInitialized = true;
        }

        public void CreateCharacterComponents() {

            // get character components ready for intitalization by allowing them to construct needed internal objects and references back to the character
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
            characterEquipmentManager.Init();

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

            if (CharacterUnit == null && unitController != null) {
                CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(unitController.Interactable);
                if (_characterUnit != null) {
                    CharacterUnit = _characterUnit;
                }
            }

        }


        public void JoinFaction(Faction newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newFaction != null && newFaction != faction) {
                SetCharacterFaction(newFaction);
                characterAbilityManager.LearnFactionAbilities(newFaction);
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

        public void SetUnitProfile(string unitProfileName) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitProfile(" + unitProfileName + ")");

            unitProfile = null;
            this.unitProfileName = unitProfileName;
            GetUnitProfileReference();
            SetUnitProfileProperties();
        }

        /// <summary>
        /// This will retrieve a unit profile from the system unit profile manager
        /// </summary>
        private void GetUnitProfileReference() {
            if (SystemUnitProfileManager.MyInstance == null) {
                Debug.LogError(gameObject.name + ".GetUnitProfileReference(): SystemUnitProfileManager not found.  Is the GameManager in the scene?");
                return;
            }
            if (unitProfileName != null && unitProfileName != string.Empty) {
                UnitProfile tmpUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(unitProfileName);
                if (tmpUnitProfile != null) {
                    unitProfile = tmpUnitProfile;
                } else {
                    Debug.LogError(gameObject.name + ".GetUnitProfileReference(): Unit Profile " + unitProfileName + " could not be found.  Check Inspector.");
                }
            }
        }

        /// <summary>
        /// This will retrieve a unit profile from the system unit profile manager
        /// </summary>
        private void SetUnitProfileProperties() {
            if (unitProfile != null) {
                if (unitProfile.CharacterName != null && unitProfile.CharacterName != string.Empty) {
                    SetCharacterName(unitProfile.CharacterName);
                }
                if (unitProfile.Title != null && unitProfile.Title != string.Empty) {
                    SetCharacterTitle(unitProfile.Title);
                }
                if (unitProfile.CharacterClass != null) {
                    SetCharacterClass(unitProfile.CharacterClass, true, false);
                }
                if (unitProfile.ClassSpecialization != null) {
                    SetClassSpecialization(unitProfile.ClassSpecialization, true, false);
                }
                if (unitProfile.Faction != null) {
                    SetCharacterFaction(unitProfile.Faction);
                }
                if (unitProfile.UnitType != null) {
                    SetUnitType(unitProfile.UnitType, true, false);
                }
                if (unitProfile.DefaultToughness != null) {
                    SetUnitToughness(unitProfile.DefaultToughness);
                }
                spawnDead = unitProfile.SpawnDead;
            }

            UpdateStatProviderList();

            if (characterStats != null) {
                characterStats.HandleUpdateStatProviders();

                // cause stats to be recalculated
                characterStats.SetLevel(characterStats.Level);
            }

        }

        public void Update() {
            if (!characterInitialized) {
                // this is probably a player unit
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
            if (characterClass != null) {
                statProviders.Add(characterClass);
            }
            if (classSpecialization != null) {
                statProviders.Add(classSpecialization);
            }
        }

        public void Initialize(string characterName, int characterLevel = 1) {
            Debug.Log(gameObject.name + ": BaseCharacter.Initialize()");
            this.characterName = characterName;
            characterStats.SetLevel(characterLevel);
        }

        public void SetUnitToughness(UnitToughness newUnitToughness) {
            unitToughness = newUnitToughness;
        }

        public void SetCharacterName(string newName) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharactername(" + newName + ")");
            if (newName != null && newName != string.Empty) {
                characterName = newName;
                OnNameChange(newName);
                if (unitController != null) {
                    unitController.NamePlateController.HandleNameChange();
                }
            }
        }

        public void SetCharacterFaction(Faction newFaction) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");
            if (newFaction != null) {
                faction = newFaction;
                OnFactionChange(newFaction);
            }
            characterFactionManager.SetReputation(newFaction);
        }

        public void SetCharacterTitle(string newTitle) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");
            if (newTitle != null) {
                title = newTitle;
                OnTitleChange(newTitle);
                if (unitController != null) {
                    unitController.NamePlateController.HandleNameChange();
                }
            }
        }

        public void SetClassSpecialization(ClassSpecialization newClassSpecialization, bool notify = true, bool resetStats = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newCharacterClassName + ")");
            if (newClassSpecialization != null) {
                ClassSpecialization oldClassSpecialization = classSpecialization;
                classSpecialization = newClassSpecialization;
                UpdateStatProviderList();
                if (characterStats != null) {
                    characterStats.HandleUpdateStatProviders();
                }

                // resets character stats because classes and specializations can get bonuses
                if (resetStats == true) {
                    characterStats.SetLevel(characterStats.Level);
                }

                if (notify == true) {
                    OnSpecializationChange(newClassSpecialization, oldClassSpecialization);
                    characterAbilityManager.HandleSpecializationChange(newClassSpecialization, oldClassSpecialization);
                }
            }
        }

        public void SetCharacterClass(CharacterClass newCharacterClass, bool notify = true, bool resetStats = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterClass(" + (newCharacterClass != null ? newCharacterClass.MyName : "null") + ", " + notify + ")");
            if (newCharacterClass != null) {
                CharacterClass oldCharacterClass = characterClass;
                characterClass = newCharacterClass;
                UpdateStatProviderList();
                if (characterStats != null) {
                    characterStats.HandleUpdateStatProviders();
                }
                if (notify) {
                    // give equipment manager time to remove equipment that this class cannot equip and ability manager time to apply class traits
                    OnClassChange(newCharacterClass, oldCharacterClass);
                    characterEquipmentManager.HandleClassChange(newCharacterClass, oldCharacterClass);
                    characterAbilityManager.HandleClassChange(newCharacterClass, oldCharacterClass);
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true && characterStats != null) {
                    characterStats.SetLevel(characterStats.Level);
                }
            }
        }

        public void SetUnitType(UnitType newUnitType, bool notify = true, bool resetStats = true) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterClass(" + (newCharacterClass != null ? newCharacterClass.MyName : "null") + ", " + notify + ")");
            if (newUnitType != null) {
                UnitType oldUnitType = unitType;
                unitType = newUnitType;
                UpdateStatProviderList();
                //characterStats. SetCharacterClass(newUnitType);
                if (notify) {
                    // give equipment manager time to remove equipment that this class cannot equip and ability manager time to apply class traits
                    OnUnitTypeChange(newUnitType, oldUnitType);
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true) {
                    characterStats.SetLevel(characterStats.Level);
                }
            }
        }

        public void DespawnImmediate() {
            //Debug.Log(gameObject.name + ".AICharacter.DespawnImmediate()");
            if (characterUnit != null) {
                characterUnit.Despawn(0, false, true);
            }
        }


        public void Despawn() {
            //Debug.Log(gameObject.name + ".AICharacter.Despawn()");
            if (characterUnit != null) {
                characterUnit.Despawn();
            }
        }

        public void TryToDespawn() {
            //Debug.Log(gameObject.name + ".AICharacter.TryToDespawn()");
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
            GetUnitProfileReference();
        }

        public void OnDestroy() {
            StopAllCoroutines();
            CleanupEventSubscriptions();
        }

    }

}