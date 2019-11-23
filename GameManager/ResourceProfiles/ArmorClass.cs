using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Armor Class", menuName = "AnyRPG/ArmorClass")]
    [System.Serializable]
    public class ArmorClass : DescribableResource {

        [SerializeField]
        private float armorPerLevel;

        public float MyArmorPerLevel { get => armorPerLevel; set => armorPerLevel = value; }
    }

}