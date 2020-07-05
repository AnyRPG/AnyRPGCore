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
    public class CharacterClass : DescribableResource, IStatProvider {

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

        [Header("Stats and Scaling")]

        [Tooltip("Stats available to this character class, in addition to the stats defined at the system level that all character use")]
        [FormerlySerializedAs("statScaling")]
        [SerializeField]
        private List<StatScalingNode> primaryStats = new List<StatScalingNode>();

        [Header("Power Resources")]

        [Tooltip("Power Resources used by this class.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        public List<BaseAbility> AbilityList { get => abilityList; set => abilityList = value; }
        public List<string> ArmorClassList { get => armorClassList; set => armorClassList = value; }
        public List<WeaponSkill> WeaponSkillList { get => weaponSkillList; set => weaponSkillList = value; }
        public List<AbilityEffect> TraitList { get => traitList; set => traitList = value; }
        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            abilityList = new List<BaseAbility>();
            if (abilityNames != null) {
                foreach (string baseAbilityName in abilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        abilityList.Add(baseAbility);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect : " + traitName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find weapon Skill : " + weaponSkillName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            foreach (StatScalingNode statScalingNode in primaryStats) {
                statScalingNode.SetupScriptableObjects();
            }

        }

    }


    [System.Serializable]
    public class CharacterStatToResourceNode {

        [Tooltip("The name of the resource that will receive points from the stat")]
        [SerializeField]
        private string resourceName = string.Empty;

        private PowerResource powerResource = null;

        [Tooltip("The amount of the resource to be gained per point of the stat")]
        [SerializeField]
        private float resourcePerPoint = 0;

        public float ResourcePerPoint { get => resourcePerPoint; set => resourcePerPoint = value; }
        public string ResourceName { get => resourceName; set => resourceName = value; }
        public PowerResource PowerResource { get => powerResource; set => powerResource = value; }

        public void SetupScriptableObjects() {

            if (resourceName != null && resourceName != string.Empty) {
                PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(resourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find power resource : " + resourceName + " while inititalizing statresourceNode.  CHECK INSPECTOR");
                }
            }

        }

    }

    [System.Serializable]
    public class StatToResourceNode {

        [Tooltip("Resource that will have its maximum amount increased by the stat")]
        [SerializeField]
        private string powerResourceName = string.Empty;

        private PowerResource powerResource = null;

        [Tooltip("List of stats that will contribute to this resource")]
        [SerializeField]
        private List<CharacterStatToResourceNode> statConversion = new List<CharacterStatToResourceNode>();

        public PowerResource PowerResource { get => powerResource; set => powerResource = value; }
        public List<CharacterStatToResourceNode> StatConversion { get => statConversion; set => statConversion = value; }

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