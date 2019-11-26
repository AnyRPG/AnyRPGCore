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

        [SerializeField]
        protected string characterClassName;

        [SerializeField]
        protected CharacterStats characterStats = null;

        [SerializeField]
        protected CharacterCombat characterCombat = null;

        [SerializeField]
        protected CharacterAbilityManager characterAbilityManager = null;

        [SerializeField]
        protected CharacterSkillManager characterSkillManager = null;

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
        public string MyFactionName {
            get {
                if (MyCharacterController != null && MyCharacterController.MyUnderControl) {
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
        public string MyCharacterClassName { get => characterClassName; set => characterClassName = value; }
        public string MyUnitProfileName { get => unitProfileName; set => unitProfileName = value; }
        public UnitProfile MyUnitProfile { get => unitProfile; set => unitProfile = value; }

        protected virtual void Awake() {
            //Debug.Log(gameObject.name + ": BaseCharacter.Awake()");
        }

        public virtual void GetComponentReferences() {

            characterStats = GetComponent<CharacterStats>();
            characterCombat = GetComponent<CharacterCombat>();
            characterController = GetComponent<BaseController>();
            characterAbilityManager = GetComponent<CharacterAbilityManager>();
            characterSkillManager = GetComponent<CharacterSkillManager>();

            CharacterUnit _characterUnit = GetComponent<CharacterUnit>();
            if (_characterUnit != null) {
                MyCharacterUnit = _characterUnit;
            }
            AnimatedUnit _animatedUnit = GetComponent<AnimatedUnit>();
            if (_animatedUnit != null) {
                MyAnimatedUnit = _animatedUnit;
            }
            characterFactionManager = GetComponent<CharacterFactionManager>();
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

        public virtual void SetCharacterFaction(string newFaction) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");
            if (newFaction != null && newFaction != string.Empty) {
                factionName = newFaction;
            }
        }

        public virtual void SetCharacterClass(string newCharacterClassName) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newCharacterClassName + ")");
            if (newCharacterClassName != null && newCharacterClassName != string.Empty) {
                string oldCharacterClassName = characterClassName;
                characterClassName = newCharacterClassName;
                characterStats.SetLevel(characterStats.MyLevel);
                CharacterClass oldCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(oldCharacterClassName);
                CharacterClass newCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(newCharacterClassName);
                OnClassChange(newCharacterClass, oldCharacterClass);
            }
        }

    }

}