using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Character Class", menuName = "AnyRPG/CharacterClass")]
    [System.Serializable]
    public class CharacterClass : DescribableResource {

        [Header("Abilities and Traits")]

        [Tooltip("Abilities available to this class")]
        [SerializeField]
        private List<string> abilityNames = new List<string>();

        // reference to the actual ability
        private List<BaseAbility> abilityList = new List<BaseAbility>();

        [Tooltip("Traits are status effects which are automatically active at all times if the level requirement is met.")]
        [SerializeField]
        private List<string> traitNames = new List<string>();

        private List<AbilityEffect> traitList = new List<AbilityEffect>();

        [Header("Capabilities")]

        [Tooltip("Armor classes that can be equipped by this class")]
        [SerializeField]
        private List<string> armorClassList = new List<string>();

        [Tooltip("Weapon skills known by this class")]
        [FormerlySerializedAs("weaponSkillList")]
        [SerializeField]
        private List<string> weaponSkills = new List<string>();

        // reference to the actual weapon skills
        private List<WeaponSkill> weaponSkillList = new List<WeaponSkill>();

        [Header("Stat Amounts")]

        [SerializeField]
        private List<PowerEnhancerNode> powerEnhancerStats = new List<PowerEnhancerNode>();

        [SerializeField]
        private int staminaPerLevel;

        [SerializeField]
        private int intellectPerLevel;

        [SerializeField]
        private int strengthPerLevel;

        [SerializeField]
        private int agilityPerLevel;

        [Header("Power Resources")]

        [Tooltip("Power Resources used by this class.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        [Header("Stat Conversion")]

        [Tooltip("Conversion amounts for translating primary stats into resources")]
        [SerializeField]
        private List<StatToResourceNode> statToResourceNodes = new List<StatToResourceNode>();

        public List<BaseAbility> MyAbilityList { get => abilityList; set => abilityList = value; }
        public List<string> MyArmorClassList { get => armorClassList; set => armorClassList = value; }
        public int MyStaminaPerLevel { get => staminaPerLevel; set => staminaPerLevel = value; }
        public int MyIntellectPerLevel { get => intellectPerLevel; set => intellectPerLevel = value; }
        public int MyStrengthPerLevel { get => strengthPerLevel; set => strengthPerLevel = value; }
        public int MyAgilityPerLevel { get => agilityPerLevel; set => agilityPerLevel = value; }
        public List<WeaponSkill> MyWeaponSkillList { get => weaponSkillList; set => weaponSkillList = value; }
        public List<PowerEnhancerNode> MyPowerEnhancerStats { get => powerEnhancerStats; set => powerEnhancerStats = value; }
        public List<AbilityEffect> MyTraitList { get => traitList; set => traitList = value; }
        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            abilityList = new List<BaseAbility>();
            if (abilityNames != null) {
                foreach (string baseAbilityName in abilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        abilityList.Add(baseAbility);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            traitList = new List<AbilityEffect>();
            if (traitNames != null) {
                foreach (string traitName in traitNames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(traitName);
                    if (abilityEffect != null) {
                        traitList.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect : " + traitName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            weaponSkillList = new List<WeaponSkill>();
            if (weaponSkills != null) {
                foreach (string weaponSkillName in weaponSkills) {
                    WeaponSkill weaponSkill = SystemWeaponSkillManager.MyInstance.GetResource(weaponSkillName);
                    if (weaponSkill != null) {
                        weaponSkillList.Add(weaponSkill);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find weapon Skill : " + weaponSkillName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            powerResourceList = new List<PowerResource>();
            if (powerResources != null) {
                foreach (string powerResourcename in powerResources) {
                    PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(powerResourcename);
                    if (tmpPowerResource != null) {
                        powerResourceList.Add(tmpPowerResource);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            foreach (StatToResourceNode statToResourceNode in statToResourceNodes) {
                statToResourceNode.SetupScriptableObjects();
            }



        }

        public float GetResourceMaximum(PowerResource powerResource, CharacterStats characterStats) {

            float returnValue = powerResource.MaximumAmount;

            foreach (StatToResourceNode statToResourceNode in statToResourceNodes) {
                if (statToResourceNode.PowerResource == powerResource) {
                    returnValue += (statToResourceNode.ResourcePerStaminaPoint * characterStats.MyStamina);
                    returnValue += (statToResourceNode.ResourcePerStrengthPoint * characterStats.MyStrength);
                    returnValue += (statToResourceNode.ResourcePerIntellectPoint * characterStats.MyIntellect);
                    returnValue += (statToResourceNode.ResourcePerAgilityPoint * characterStats.MyAgility);
                }
            }
            return returnValue;
        }

    }

    [System.Serializable]
    public class StatToResourceNode {

        [Tooltip("Resource that will have its maximum amount increased by the stat")]
        [SerializeField]
        private string powerResourceName = string.Empty;

        private PowerResource powerResource = null;

        [Tooltip("The amount of the resource to be gained per point of the stat")]
        [SerializeField]
        private float resourcePerStaminaPoint = 10f;

        [Tooltip("The amount of the resource to be gained per point of the stat")]
        [SerializeField]
        private float resourcePerStrengthPoint = 10f;

        [Tooltip("The amount of the resource to be gained per point of the stat")]
        [SerializeField]
        private float resourcePerIntellectPoint = 10f;

        [Tooltip("The amount of the resource to be gained per point of the stat")]
        [SerializeField]
        private float resourcePerAgilityPoint = 10f;

        public PowerResource PowerResource { get => powerResource; set => powerResource = value; }
        public float ResourcePerStaminaPoint { get => resourcePerStaminaPoint; set => resourcePerStaminaPoint = value; }
        public float ResourcePerStrengthPoint { get => resourcePerStrengthPoint; set => resourcePerStrengthPoint = value; }
        public float ResourcePerIntellectPoint { get => resourcePerIntellectPoint; set => resourcePerIntellectPoint = value; }
        public float ResourcePerAgilityPoint { get => resourcePerAgilityPoint; set => resourcePerAgilityPoint = value; }

        public void SetupScriptableObjects() {

            if (powerResourceName != null && powerResourceName != string.Empty) {
                PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(powerResourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find power resource : " + powerResourceName + " while inititalizing statresourceNode.  CHECK INSPECTOR");
                }
            }


        }
    }

}