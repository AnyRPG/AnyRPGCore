using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(CharacterFactionManager))]
    public abstract class BaseCharacter : MonoBehaviour, ICharacter {
        [SerializeField]
        protected string characterName;

        [SerializeField]
        protected string factionName;

        [SerializeField]
        protected ICharacterStats characterStats = null;

        [SerializeField]
        protected ICharacterCombat characterCombat = null;

        [SerializeField]
        protected ICharacterAbilityManager characterAbilityManager = null;

        [SerializeField]
        protected ICharacterSkillManager characterSkillManager = null;

        [SerializeField]
        protected ICharacterController characterController = null;

        protected CharacterFactionManager characterFactionManager = null;

        protected CharacterEquipmentManager characterEquipmentManager = null;

        protected CharacterUnit characterUnit = null;
        protected AnimatedUnit animatedUnit = null;

        public ICharacterStats MyCharacterStats { get => characterStats; }
        public ICharacterCombat MyCharacterCombat { get => characterCombat; }
        public ICharacterController MyCharacterController { get => characterController; }
        public ICharacterAbilityManager MyCharacterAbilityManager { get => characterAbilityManager; }
        public ICharacterSkillManager MyCharacterSkillManager { get => characterSkillManager; }
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
        public CharacterUnit MyCharacterUnit { get => characterUnit; set => characterUnit = value; }
        public AnimatedUnit MyAnimatedUnit { get => animatedUnit; set => animatedUnit = value; }
        public CharacterFactionManager MyCharacterFactionManager { get => characterFactionManager; set => characterFactionManager = value; }
        public CharacterEquipmentManager MyCharacterEquipmentManager { get => characterEquipmentManager; set => characterEquipmentManager = value; }

        protected virtual void Awake() {
            //Debug.Log(gameObject.name + ": BaseCharacter.Awake()");
            CharacterUnit _characterUnit = GetComponent<CharacterUnit>();
            if (_characterUnit != null) {
                MyCharacterUnit = _characterUnit;
            }
            AnimatedUnit _animatedUnit = GetComponent<AnimatedUnit>();
            if (_animatedUnit != null) {
                MyAnimatedUnit = _animatedUnit;
            }
            characterSkillManager = GetComponent<CharacterSkillManager>();
            characterFactionManager = GetComponent<CharacterFactionManager>();
            characterEquipmentManager = GetComponent<CharacterEquipmentManager>();
            characterAbilityManager = GetComponent<CharacterAbilityManager>();

        }

        protected virtual void Start() {
            //Debug.Log(gameObject.name + ": BaseCharacter.Start()");
            if (characterStats != null) {
                characterStats.CreateEventSubscriptions();
                characterStats.OrchestratorSetLevel();
            }
            if (characterAbilityManager != null) {
                characterAbilityManager.CreateEventSubscriptions();
            }

            if (characterEquipmentManager != null) {
                characterEquipmentManager.LoadDefaultEquipment();
            } else {
                //Debug.Log(gameObject.name + ": BaseCharacter.Start(): characterEquipmentManager is null");
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
            }
        }

        public virtual void SetCharacterFaction(string newFaction) {
            //Debug.Log(gameObject.name + ".BaseCharacter.SetCharacterFaction(" + newFaction + ")");
            if (newFaction != null && newFaction != string.Empty) {
                factionName = newFaction;
            }
        }

    }

}