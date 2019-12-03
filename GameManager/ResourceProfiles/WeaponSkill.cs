using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Weapon Skill", menuName = "AnyRPG/WeaponSkill")]
    [System.Serializable]
    public class WeaponSkill : DescribableResource {

        // this skill is considered to be in use by an unarmed character if set to true
        [SerializeField]
        private bool defaultWeaponSkill;

        public bool MyDefaultWeaponSkill { get => defaultWeaponSkill; set => defaultWeaponSkill = value; }
    }

}