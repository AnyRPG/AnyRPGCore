using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(CharacterFactionManager))]
    public abstract class BaseCharacter : MonoBehaviour {

        public event System.Action<string> OnNameChange = delegate { };
        public event System.Action<string> OnTitleChange = delegate { };
        public event System.Action<Faction> OnFactionChange = delegate { };
        public event System.Action<CharacterClass, CharacterClass> OnClassChange = delegate { };
        public event System.Action<ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        public event System.Action<UnitType, UnitType> OnUnitTypeChange = delegate { };

        [Tooltip("The name of the unit profile used to configure this character")]
        [SerializeField]
        protected string unitProfileName;

        // properties that come from the unit profile
        protected string characterName = string.Empty;
        protected string title = string.Empty;
        protected Faction faction;
        protected UnitType unitType;
        protected CharacterClass characterClass;
        protected ClassSpecialization classSpecialization;
        protected bool spawnDead = false;

        protected UnitProfile unitProfile = null;

        // common access to properties that provide stats
        protected List<IStatProvider> statProviders = new List<IStatProvider>();

        // components
        protected CharacterCombat characterCombat = null;
        protected CharacterAbilityManager characterAbilityManager = null;
        protected CharacterSkillManager characterSkillManager = null;
        protected CharacterPetManager characterPetManager = null;
        protected BaseController characterController = null;
        protected CharacterFactionManager characterFactionManager = null;
        protected CharacterEquipmentManager characterEquipmentManager = null;
        protected CharacterStats characterStats = null;
        protected CharacterUnit characterUnit = null;
        protected AnimatedUnit animatedUnit = null;
        protected Interactable interactable = null;

        // disable certain things not needed for preview units
        protected bool previewCharacter = false;

        public CharacterStats CharacterStats { get => characterStats; }
        public CharacterCombat CharacterCombat { get => characterCombat; }
        public BaseController CharacterController { get => characterController; }
        public CharacterAbilityManager CharacterAbilityManager { get => characterAbilityManager; }
        public CharacterSkillManager CharacterSkillManager { get => characterSkillManager; }
        public CharacterUnit CharacterUnit { get => characterUnit; set => characterUnit = value; }
        public AnimatedUnit AnimatedUnit { get => animatedUnit; set => animatedUnit = value; }
        public CharacterFactionManager CharacterFactionManager { get => characterFactionManager; set => characterFactionManager = value; }
        public CharacterEquipmentManager CharacterEquipmentManager { get => characterEquipmentManager; set => characterEquipmentManager = value; }

        public string CharacterName { get => characterName; }
        public string MyName { get => CharacterName; }

        public Faction MyFaction {

            get {
                if (CharacterController != null && CharacterController.MyUnderControl) {
                    //Debug.Log(gameObject.name + ".MyFactionName: return master unit faction name");
                    return CharacterController.MyMasterUnit.MyFaction;
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
        public bool PreviewCharacter { get => previewCharacter; set => previewCharacter = value; }
        public string Title { get => title; set => title = value; }
        public List<IStatProvider> StatProviders { get => statProviders; set => statProviders = value; }

        protected virtual void Awake() {
            //Debug.Log(gameObject.name + ": BaseCharacter.Awake()");
            SetupScriptableObjects();
        }

        public virtual void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".BaseCharacter.GetComponentReferences()");

            characterStats = GetComponent<CharacterStats>();
            characterCombat = GetComponent<CharacterCombat>();
            characterController = GetComponent<BaseController>();
            characterAbilityManager = GetComponent<CharacterAbilityManager>();
            characterSkillManager = GetComponent<CharacterSkillManager>();
            characterPetManager = GetComponent<CharacterPetManager>();

            if (CharacterUnit == null) {
                CharacterUnit _characterUnit = GetComponent<CharacterUnit>();
                if (_characterUnit != null) {
                    CharacterUnit = _characterUnit;
                }
            }
            if (AnimatedUnit == null) {
                AnimatedUnit _animatedUnit = GetComponent<AnimatedUnit>();
                if (_animatedUnit != null) {
                    AnimatedUnit = _animatedUnit;
                }
            }
            characterFactionManager = GetComponent<CharacterFactionManager>();
            if (characterFactionManager == null) {
                gameObject.AddComponent<CharacterFactionManager>();
                Debug.Log(gameObject.name + ".BaseCharacter.GetComponentReferences(): CharacterFactionManager MISSING.  ADDING BUT CHECK GAMEOBJECT AND ADD MANUALLY IF POSSIBLE.");
            }
            characterEquipmentManager = GetComponent<CharacterEquipmentManager>();

        }

        public virtual void OrchestratorStart() {
            //Debug.Log(gameObject.name + ": BaseCharacter.OrchestratorStart()");
            SetUnitProfileProperties();

            OrchestratorStartCommon();
        }

        public virtual void OrchestratorStartCommon() {
            //Debug.Log(gameObject.name + ": BaseCharacter.OrchestratorStartCommon()");

            GetComponentReferences();
            if (characterStats != null) {
                characterStats.OrchestratorStart();
                characterStats.OrchestratorSetLevel();
            }
            //GetUnitProfile();
            if (characterCombat != null) {
                characterCombat.OrchestratorStart();
            }
            if (characterAbilityManager != null) {
                characterAbilityManager.OrchestratorStart();
            }

            if (characterEquipmentManager != null) {
                characterEquipmentManager.OrchestratorStart();
                characterEquipmentManager.LoadDefaultEquipment();
            } else {
                //Debug.Log(gameObject.name + ": BaseCharacter.Start(): characterEquipmentManager is null");
            }
            if (characterAbilityManager != null) {
                characterAbilityManager.LearnDefaultAutoAttackAbility();
            }
            if (characterPetManager != null) {
                characterPetManager.OrchestratorStart();
            }

        }

        public virtual void OrchestratorFinish() {
            if (characterStats != null) {
                characterStats.OrchestratorFinish();
            }
            if (characterEquipmentManager != null) {
                characterEquipmentManager.OrchestratorFinish();
            }
        }

        protected virtual void Start() {
            //Debug.Log(gameObject.name + ": BaseCharacter.Start()");
        }

        public virtual void SetUnitProfile(string unitProfileName) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetUnitProfile(" + unitProfileName + ")");

            unitProfile = null;
            this.unitProfileName = unitProfileName;
            GetUnitProfileReference();
            SetUnitProfileProperties();
        }

        /// <summary>
        /// This will retrieve a unit profile from the system unit profile manager
        /// </summary>
        protected virtual void GetUnitProfileReference() {
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
        protected virtual void SetUnitProfileProperties() {
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
                spawnDead = unitProfile.SpawnDead;
            }

            UpdateStatProviderList();

            if (characterStats != null) {
                characterStats.HandleUpdateStatProviders();

                // cause stats to be recalculated
                characterStats.SetLevel(characterStats.Level);
            }

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

        public virtual void Initialize(string characterName, int characterLevel = 1) {
            //Debug.Log(gameObject.name + ": BaseCharacter.Initialize()");
            this.characterName = characterName;
            characterStats.SetLevel(characterLevel);
        }

        public virtual void SetCharacterName(string newName) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharactername(" + newName + ")");
            if (newName != null && newName != string.Empty) {
                characterName = newName;
                OnNameChange(newName);
                if (characterUnit != null) {
                    characterUnit.HandleNameChange();
                }
            }
        }

        public virtual void SetCharacterFaction(Faction newFaction) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");
            if (newFaction != null) {
                faction = newFaction;
                OnFactionChange(newFaction);
            }
        }

        public virtual void SetCharacterTitle(string newTitle) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");
            if (newTitle != null) {
                title = newTitle;
                OnTitleChange(newTitle);
                if (characterUnit != null) {
                    characterUnit.HandleNameChange();
                }
            }
        }

        public virtual void SetClassSpecialization(ClassSpecialization newClassSpecialization, bool notify = true, bool resetStats = true) {
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
                }
            }
        }

        public virtual void SetCharacterClass(CharacterClass newCharacterClass, bool notify = true, bool resetStats = true) {
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
                }

                // now it is safe to setlevel because when we set level we will calculate stats that require the traits and equipment to be properly set for the class
                if (resetStats == true && characterStats != null) {
                    characterStats.SetLevel(characterStats.Level);
                }
            }
        }

        public virtual void SetUnitType(UnitType newUnitType, bool notify = true, bool resetStats = true) {
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


        public virtual void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetupScriptableObjects()");

            GetUnitProfileReference();
        }

    }

}