using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Class Specialization", menuName = "AnyRPG/CharacterClassSpecialization")]
    [System.Serializable]
    public class ClassSpecialization : DescribableResource, IStatProvider {

        //[Header("Class Specialization")]

        [Header("NewGame")]

        [Tooltip("If true, this faction is available for Players to choose on the new game menu")]
        [SerializeField]
        private bool newGameOption = false;

        [Header("Start Equipment")]

        [Tooltip("The names of the equipment that will be worn by this class when a new game is started")]
        [SerializeField]
        private List<string> equipmentNames = new List<string>();

        private List<Equipment> equipmentList = new List<Equipment>();

        [Header("Character Classes")]

        [Tooltip("The list of class names that have access to this specialization")]
        [SerializeField]
        private List<string> classNames = new List<string>();

        private List<CharacterClass> characterClasses = new List<CharacterClass>();


        [Header("Capabilities")]

        [Tooltip("Weapon skills known by this specialization")]
        [FormerlySerializedAs("weaponSkillList")]
        [SerializeField]
        private List<string> weaponSkillNames = new List<string>();

        private List<WeaponSkill> weaponSkillList = new List<WeaponSkill>();

        [Tooltip("Abilities this specialization will know")]
        [SerializeField]
        private List<string> abilityNames = new List<string>();

        private List<BaseAbility> realAbilityList = new List<BaseAbility>();

        [SerializeField]
        private List<string> traitNames = new List<string>();

        private List<StatusEffect> realTraitList = new List<StatusEffect>();

        [Header("Stats and Scaling")]

        [Tooltip("Stats available to this unit, in addition to the stats defined at the system level that all character use")]
        [FormerlySerializedAs("statScaling")]
        [SerializeField]
        private List<StatScalingNode> primaryStats = new List<StatScalingNode>();

        [Header("Power Resources")]

        [Tooltip("Power Resources used by this unit.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        [SerializeField]
        private List<PowerEnhancerNode> powerEnhancerStats = new List<PowerEnhancerNode>();

        public List<BaseAbility> AbilityList { get => realAbilityList; set => realAbilityList = value; }
        public List<WeaponSkill> WeaponSkillList { get => weaponSkillList; set => weaponSkillList = value; }
        public List<StatusEffect> TraitList { get => realTraitList; set => realTraitList = value; }
        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public List<PowerEnhancerNode> PowerEnhancerStats { get => powerEnhancerStats; set => powerEnhancerStats = value; }
        public bool NewGameOption { get => newGameOption; set => newGameOption = value; }
        public List<CharacterClass> CharacterClasses { get => characterClasses; set => characterClasses = value; }
        public List<Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (equipmentNames != null) {
                foreach (string equipmentName in equipmentNames) {
                    Equipment tmpEquipment = null;
                    tmpEquipment = SystemItemManager.MyInstance.GetResource(equipmentName) as Equipment;
                    if (tmpEquipment != null) {
                        equipmentList.Add(tmpEquipment);
                    } else {
                        Debug.LogError("CharacterClass.SetupScriptableObjects(): Could not find equipment : " + equipmentName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (classNames != null) {
                foreach (string className in classNames) {
                    CharacterClass tmpClass = SystemCharacterClassManager.MyInstance.GetResource(className);
                    if (tmpClass != null) {
                        characterClasses.Add(tmpClass);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + className + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            realAbilityList = new List<BaseAbility>();
            if (abilityNames != null) {
                foreach (string baseAbilityName in abilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        realAbilityList.Add(baseAbility);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            realTraitList = new List<StatusEffect>();
            if (traitNames != null) {
                foreach (string traitName in traitNames) {
                    StatusEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(traitName) as StatusEffect;
                    if (abilityEffect != null) {
                        realTraitList.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect : " + traitName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            weaponSkillList = new List<WeaponSkill>();
            if (weaponSkillNames != null) {
                foreach (string weaponSkillName in weaponSkillNames) {
                    WeaponSkill weaponSkill = SystemWeaponSkillManager.MyInstance.GetResource(weaponSkillName);
                    if (weaponSkill != null) {
                        weaponSkillList.Add(weaponSkill);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find weapon Skill : " + weaponSkillName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }

    }

}