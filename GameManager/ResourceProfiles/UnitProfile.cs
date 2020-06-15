using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Unit Profile", menuName = "AnyRPG/UnitProfile")]
    [System.Serializable]
    public class UnitProfile : DescribableResource {

        [Header("Unit")]

        [Tooltip("The physical game object to spawn for this unit")]
        [SerializeField]
        private GameObject unitPrefab = null;

        [Tooltip("Mark this if true is the unit is an UMA unit")]
        [SerializeField]
        private bool isUMAUnit = false;

        [Tooltip("If true, this unit can be charmed and made into a pet")]
        [SerializeField]
        private bool isPet = false;

        [Tooltip("If this is set, when the unit spawns, it will use this toughness")]
        [SerializeField]
        private string defaultToughness = string.Empty;

        protected UnitToughness unitToughness = null;

        [Header("Abilities")]

        [Tooltip("When no weapons are equippped to learn auto-attack abilities from, this auto-attack ability will be used")]
        [SerializeField]
        private string defaultAutoAttackAbilityName = string.Empty;

        private BaseAbility defaultAutoAttackAbility = null;

        [Tooltip("Abilities this unit will know")]
        [SerializeField]
        private List<string> learnedAbilityNames = new List<string>();

        private List<BaseAbility> learnedAbilities = new List<BaseAbility>();

        [Header("Capabilities")]

        [Tooltip("Weapon skills known by this class")]
        [FormerlySerializedAs("weaponSkillList")]
        [SerializeField]
        private List<string> weaponSkills = new List<string>();

        // reference to the actual weapon skills
        private List<WeaponSkill> weaponSkillList = new List<WeaponSkill>();

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

        [Header("Movement")]

        [Tooltip("If true, the movement sounds are played on footstep hit instead of in a continuous track.")]
        [SerializeField]
        private bool playOnFootstep = false;

        [Tooltip("These profiles will be played when the unit is in motion.  If footsteps are used, the next sound on the list will be played on every footstep.")]
        [SerializeField]
        private List<string> movementAudioProfileNames = new List<string>();

        private List<AudioProfile> movementAudioProfiles = new List<AudioProfile>();

        public GameObject MyUnitPrefab { get => unitPrefab; set => unitPrefab = value; }
        public UnitToughness MyDefaultToughness { get => unitToughness; set => unitToughness = value; }
        public BaseAbility MyDefaultAutoAttackAbility { get => defaultAutoAttackAbility; set => defaultAutoAttackAbility = value; }
        public bool MyIsUMAUnit { get => isUMAUnit; set => isUMAUnit = value; }
        public bool MyIsPet { get => isPet; set => isPet = value; }
        public List<BaseAbility> MyLearnedAbilities { get => learnedAbilities; set => learnedAbilities = value; }
        public bool PlayOnFootstep { get => playOnFootstep; set => playOnFootstep = value; }
        public List<AudioProfile> MovementAudioProfiles { get => movementAudioProfiles; set => movementAudioProfiles = value; }
        public List<WeaponSkill> WeaponSkillList { get => weaponSkillList; set => weaponSkillList = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }

        /// <summary>
        /// Return the maximum value for a power resource
        /// </summary>
        /// <param name="powerResource"></param>
        /// <param name="characterStats"></param>
        /// <returns></returns>
        public float GetResourceMaximum(PowerResource powerResource, CharacterStats characterStats) {

            float returnValue = powerResource.MaximumAmount;

            foreach (StatScalingNode statScalingNode in primaryStats) {
                if (characterStats.PrimaryStats.ContainsKey(statScalingNode.StatName)) {
                    foreach (CharacterStatToResourceNode characterStatToResourceNode in statScalingNode.PrimaryToResourceConversion) {
                        if (characterStatToResourceNode.PowerResource == powerResource) {
                            returnValue += (characterStatToResourceNode.ResourcePerPoint * characterStats.PrimaryStats[statScalingNode.StatName].CurrentValue);
                        }
                    }
                }
            }
            return returnValue;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            defaultAutoAttackAbility = null;
            if (defaultAutoAttackAbilityName != null && defaultAutoAttackAbilityName != string.Empty) {
                defaultAutoAttackAbility = SystemAbilityManager.MyInstance.GetResource(defaultAutoAttackAbilityName);
            }/* else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + defaultAutoAttackAbilityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
            }*/

            if (unitToughness == null && defaultToughness != null && defaultToughness != string.Empty) {
                UnitToughness tmpToughness = SystemUnitToughnessManager.MyInstance.GetResource(defaultToughness);
                if (tmpToughness != null) {
                    unitToughness = tmpToughness;
                } else {
                    Debug.LogError("Unit Toughness: " + defaultToughness + " not found while initializing Unit Profiles.  Check Inspector!");
                }
            }

            learnedAbilities = new List<BaseAbility>();
            if (learnedAbilityNames != null) {
                foreach (string baseAbilityName in learnedAbilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        learnedAbilities.Add(baseAbility);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (movementAudioProfileNames != null) {
                foreach (string movementAudioProfileName in movementAudioProfileNames) {
                    AudioProfile tmpAudioProfile = SystemAudioProfileManager.MyInstance.GetResource(movementAudioProfileName);
                    if (tmpAudioProfile != null) {
                        movementAudioProfiles.Add(tmpAudioProfile);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find audio profile : " + movementAudioProfileName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find weapon Skill : " + weaponSkillName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            foreach (StatScalingNode statScalingNode in primaryStats) {
                statScalingNode.SetupScriptableObjects();
            }


        }
    }

}