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

        [Header("Class Specialization")]

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

        private List<AbilityEffect> realTraitList = new List<AbilityEffect>();

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
        public List<AbilityEffect> TraitList { get => realTraitList; set => realTraitList = value; }
        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public List<PowerEnhancerNode> PowerEnhancerStats { get => powerEnhancerStats; set => powerEnhancerStats = value; }

        
        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            realAbilityList = new List<BaseAbility>();
            if (abilityNames != null) {
                foreach (string baseAbilityName in abilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        realAbilityList.Add(baseAbility);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            realTraitList = new List<AbilityEffect>();
            if (traitNames != null) {
                foreach (string traitName in traitNames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(traitName);
                    if (abilityEffect != null) {
                        realTraitList.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect : " + traitName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find weapon Skill : " + weaponSkillName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }

    }

}