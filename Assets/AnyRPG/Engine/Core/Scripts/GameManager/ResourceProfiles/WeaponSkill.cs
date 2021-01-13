using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Weapon Skill", menuName = "AnyRPG/WeaponSkill")]
    public class WeaponSkill : DescribableResource {

        [SerializeField]
        private WeaponSkillProps weaponSkillProps = new WeaponSkillProps();

        public WeaponSkillProps WeaponSkillProps { get => weaponSkillProps; set => weaponSkillProps = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            weaponSkillProps.SetupScriptableObjects();
        }
    }

}