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
    public class ClassSpecialization : DescribableResource {

        [SerializeField]
        private List<string> weaponSkillList = new List<string>();

        private List<WeaponSkill> realWeaponSkillList = new List<WeaponSkill>();

        [SerializeField]
        private List<string> abilityNames = new List<string>();

        /*
        [SerializeField]
        private List<string> abilityList = new List<string>();
        */

        private List<BaseAbility> realAbilityList = new List<BaseAbility>();

        [SerializeField]
        private List<string> traitNames = new List<string>();

        //[SerializeField]
        //private List<string> traitList = new List<string>();

        private List<AbilityEffect> realTraitList = new List<AbilityEffect>();

        /*
        [SerializeField]
        private List<string> armorClassList = new List<string>();
        */

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

        public List<BaseAbility> MyAbilityList { get => realAbilityList; set => realAbilityList = value; }
        //public List<string> MyArmorClassList { get => armorClassList; set => armorClassList = value; }
        public int MyStaminaPerLevel { get => staminaPerLevel; set => staminaPerLevel = value; }
        public int MyIntellectPerLevel { get => intellectPerLevel; set => intellectPerLevel = value; }
        public int MyStrengthPerLevel { get => strengthPerLevel; set => strengthPerLevel = value; }
        public int MyAgilityPerLevel { get => agilityPerLevel; set => agilityPerLevel = value; }
        public List<WeaponSkill> MyWeaponSkillList { get => realWeaponSkillList; set => realWeaponSkillList = value; }
        public List<PowerEnhancerNode> MyPowerEnhancerStats { get => powerEnhancerStats; set => powerEnhancerStats = value; }
        public List<AbilityEffect> MyTraitList { get => realTraitList; set => realTraitList = value; }

        
        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            realAbilityList = new List<BaseAbility>();
            if (abilityNames != null) {
                foreach (string baseAbilityName in abilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        realAbilityList.Add(baseAbility);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect : " + traitName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            realWeaponSkillList = new List<WeaponSkill>();
            if (weaponSkillList != null) {
                foreach (string weaponSkillName in weaponSkillList) {
                    WeaponSkill weaponSkill = SystemWeaponSkillManager.MyInstance.GetResource(weaponSkillName);
                    if (weaponSkill != null) {
                        realWeaponSkillList.Add(weaponSkill);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find weapon Skill : " + weaponSkillName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }

    }

}