using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(CharacterFactionManager))]
    public abstract class BaseCharacter : MonoBehaviour {

        public event System.Action<CharacterClass, CharacterClass> OnClassChange = delegate { };

        [SerializeField]
        protected string characterName;

        [SerializeField]
        protected string factionName;

        protected Faction faction;

        [SerializeField]
        protected string characterClassName;

        protected CharacterClass characterClass;

        [SerializeField]
        protected CharacterStats characterStats = null;

        [SerializeField]
        protected CharacterCombat characterCombat = null;

        [SerializeField]
        protected CharacterAbilityManager characterAbilityManager = null;

        [SerializeField]
        protected CharacterSkillManager characterSkillManager = null;

        [SerializeField]
        protected CharacterPetManager characterPetManager = null;

        [SerializeField]
        protected BaseController characterController = null;

        // unit profile name
        [SerializeField]
        protected string unitProfileName;

        // reference to actual unit profile
        protected UnitProfile unitProfile = null;

        protected CharacterFactionManager characterFactionManager = null;

        protected CharacterEquipmentManager characterEquipmentManager = null;

        protected CharacterUnit characterUnit = null;
        protected AnimatedUnit animatedUnit = null;

        public CharacterStats MyCharacterStats { get => characterStats; }
        public CharacterCombat MyCharacterCombat { get => characterCombat; }
        public BaseController MyCharacterController { get => characterController; }
        public CharacterAbilityManager MyCharacterAbilityManager { get => characterAbilityManager; }
        public CharacterSkillManager MyCharacterSkillManager { get => characterSkillManager; }
        public CharacterUnit MyCharacterUnit { get => characterUnit; set => characterUnit = value; }
        public AnimatedUnit MyAnimatedUnit { get => animatedUnit; set => animatedUnit = value; }
        public CharacterFactionManager MyCharacterFactionManager { get => characterFactionManager; set => characterFactionManager = value; }
        public CharacterEquipmentManager MyCharacterEquipmentManager { get => characterEquipmentManager; set => characterEquipmentManager = value; }

        public string MyCharacterName { get => characterName; }
        public string MyName { get => MyCharacterName; }
        /*
        private string MyFactionName {

            get {
                if (MyCharacterController != null && MyCharacterController.MyUnderControl) {
                    //Debug.Log(gameObject.name + ".MyFactionName: return master unit faction name");
                    return MyCharacterController.MyMasterUnit.MyFactionName;
                }
                if (factionName != null && factionName != string.Empty) {
                    //Debug.Log(gameObject.name + ".MyFactionName: factionName has value: " + factionName);
                    return factionName;
                }
                return string.Empty;
            }
            set => factionName = value;
        }
        */
        public Faction MyFaction {

            get {
                if (MyCharacterController != null && MyCharacterController.MyUnderControl) {
                    //Debug.Log(gameObject.name + ".MyFactionName: return master unit faction name");
                    return MyCharacterController.MyMasterUnit.MyFaction;
                }
                return faction;
            }
            set => faction = value;
        }

        public CharacterClass MyCharacterClass { get => characterClass; set => characterClass = value; }
        public string MyUnitProfileName { get => unitProfileName; set => unitProfileName = value; }
        public UnitProfile MyUnitProfile { get => unitProfile; set => unitProfile = value; }
        public CharacterPetManager MyCharacterPetManager { get => characterPetManager; set => characterPetManager = value; }
        public string MyCharacterClassName { get => characterClassName; set => characterClassName = value; }

        protected virtual void Awake() {
            //Debug.Log(gameObject.name + ": BaseCharacter.Awake()");
            GetComponentReferences();
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

            if (MyCharacterUnit == null) {
                CharacterUnit _characterUnit = GetComponent<CharacterUnit>();
                if (_characterUnit != null) {
                    MyCharacterUnit = _characterUnit;
                }
            }
            if (MyAnimatedUnit == null) {
                AnimatedUnit _animatedUnit = GetComponent<AnimatedUnit>();
                if (_animatedUnit != null) {
                    MyAnimatedUnit = _animatedUnit;
                }
            }
            characterFactionManager = GetComponent<CharacterFactionManager>();
            if (characterFactionManager == null) {
                gameObject.AddComponent<CharacterFactionManager>();
                Debug.Log(gameObject.name + ".BaseCharacter.GetComponentReferences(): CharacterFactionManager MISSING.  ADDING BUT CHECK GAMEOBJECT AND ADD MANUALLY IF POSSIBLE.");
            }
            characterEquipmentManager = GetComponent<CharacterEquipmentManager>();

        }

        public virtual void SetUnitProfile(string unitProfileName) {
            unitProfile = null;
            this.unitProfileName = unitProfileName;
            GetUnitProfile();
        }

        public virtual void GetUnitProfile() {
            if (unitProfileName != null && unitProfileName != string.Empty && unitProfile == null) {
                unitProfile = SystemUnitProfileManager.MyInstance.GetResource(unitProfileName);
            }
        }

        public virtual void OrchestratorStart() {
            //Debug.Log(gameObject.name + ": BaseCharacter.OrchestratorStart()");

            GetUnitProfile();

            OrchestratorStartCommon();
        }

        public virtual void OrchestratorStartCommon() {
            GetComponentReferences();
            if (characterStats != null) {
                characterStats.OrchestratorStart();
                characterStats.OrchestratorSetLevel();
            }
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

        protected virtual void Start() {
            //Debug.Log(gameObject.name + ": BaseCharacter.Start()");
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
            }
        }

        public virtual void SetCharacterFaction(Faction newFaction) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");
            if (newFaction != null) {
                faction = newFaction;
            }
        }

        public virtual void SetCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newCharacterClassName + ")");
            if (newCharacterClass != null) {
                CharacterClass oldCharacterClass = characterClass;
                characterClass = newCharacterClass;
                characterStats.SetLevel(characterStats.MyLevel);
                OnClassChange(newCharacterClass, oldCharacterClass);
            }
        }

        public void SetupScriptableObjects() {
            if (faction == null && factionName != null && factionName != string.Empty) {
                Faction tmpFaction = SystemFactionManager.MyInstance.GetResource(factionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }

            }
            if (characterClass == null && characterClassName != null && characterClassName != string.Empty) {
                CharacterClass tmpCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(characterClassName);
                if (tmpCharacterClass != null) {
                    characterClass = tmpCharacterClass;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }

            }

        }

    }

}