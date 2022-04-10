using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Weapon Type Config Template", menuName = "AnyRPG/System/WeaponTypeConfigTemplate")]
    [System.Serializable]
    public class WeaponTypeConfigTemplate : ScriptableObject {

        [Header("Weapon Type")]

        [Tooltip("The name of the weapon skill to assign")]
        [SerializeField]
        private string weaponType = string.Empty;

        [Tooltip("Link to the template content package to install for this weapon type")]
        [SerializeField]
        private ScriptableContentTemplate weaponSkillContentTemplate = null;

        [Tooltip("A list of config templates for each weapon slot type")]
        [SerializeField]
        private List<WeaponSlotConfig> weaponSlotConfigs = new List<WeaponSlotConfig>();

        public List<WeaponSlotConfig> WeaponSlotConfigs { get => weaponSlotConfigs; set => weaponSlotConfigs = value; }
        public ScriptableContentTemplate WeaponSkillContentTemplate { get => weaponSkillContentTemplate; set => weaponSkillContentTemplate = value; }
        public string WeaponType { get => weaponType; set => weaponType = value; }
    }

    [System.Serializable]
    public class WeaponSlotConfig {

        public WeaponSlotType weaponSlotType = WeaponSlotType.MainHandOnly;

        public List<HoldableObjectAttachment> holdableObjectList = new List<HoldableObjectAttachment>();
    }


    public enum WeaponSlotType { MainHandOnly, OffHandOnly, AnyHand, TwoHand }

}