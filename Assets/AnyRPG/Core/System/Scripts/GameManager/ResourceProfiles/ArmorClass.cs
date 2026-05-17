using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Armor Class", menuName = "AnyRPG/ArmorClass")]
    [System.Serializable]
    public class ArmorClass : DescribableResource {

        [Header("Armor")]

        [SerializeField]
        private float armorPerLevel;

        public float ArmorPerLevel { get => armorPerLevel; set => armorPerLevel = value; }
    }

}