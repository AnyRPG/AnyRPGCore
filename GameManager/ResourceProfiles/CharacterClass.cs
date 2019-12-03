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

        [SerializeField]
        private List<string> weaponSkillList = new List<string>();

        [SerializeField]
        private List<string> abilityList = new List<string>();

        [SerializeField]
        private List<string> traitList = new List<string>();

        [SerializeField]
        private List<string> armorClassList = new List<string>();

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

        public List<string> MyAbilityList { get => abilityList; set => abilityList = value; }
        public List<string> MyArmorClassList { get => armorClassList; set => armorClassList = value; }
        public int MyStaminaPerLevel { get => staminaPerLevel; set => staminaPerLevel = value; }
        public int MyIntellectPerLevel { get => intellectPerLevel; set => intellectPerLevel = value; }
        public int MyStrengthPerLevel { get => strengthPerLevel; set => strengthPerLevel = value; }
        public int MyAgilityPerLevel { get => agilityPerLevel; set => agilityPerLevel = value; }
        public List<string> MyWeaponSkillList { get => weaponSkillList; set => weaponSkillList = value; }
        public List<PowerEnhancerNode> MyPowerEnhancerStats { get => powerEnhancerStats; set => powerEnhancerStats = value; }
        public List<string> MyTraitList { get => traitList; set => traitList = value; }
    }

}