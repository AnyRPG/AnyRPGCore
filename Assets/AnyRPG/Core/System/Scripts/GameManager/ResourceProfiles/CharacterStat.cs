using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Character Stat", menuName = "AnyRPG/CharacterStat")]
    [System.Serializable]
    public class CharacterStat : DescribableResource {

        // nothing here yet
    }

    [System.Serializable]
    public class StatScalingNode {

        [Tooltip("The stat")]
        [SerializeField]
        private string statName = string.Empty;

        [Tooltip("The amount of this stat that a character will receive for every level.")]
        [SerializeField]
        private float budgetPerLevel = 0;

        [Tooltip("Convert primary stats to secondary stats")]
        [SerializeField]
        private List<PrimaryToSecondaryStatNode> primaryToSecondaryConversion = new List<PrimaryToSecondaryStatNode>();

        [Tooltip("Convert primary stats to resources")]
        [SerializeField]
        private List<CharacterStatToResourceNode> primaryToResourceConversion = new List<CharacterStatToResourceNode>();

        public string StatName { get => statName; set => statName = value; }
        public float BudgetPerLevel { get => budgetPerLevel; set => budgetPerLevel = value; }
        public List<PrimaryToSecondaryStatNode> PrimaryToSecondaryConversion { get => primaryToSecondaryConversion; set => primaryToSecondaryConversion = value; }
        public List<CharacterStatToResourceNode> PrimaryToResourceConversion { get => primaryToResourceConversion; set => primaryToResourceConversion = value; }

        public void SetupScriptableObjects(SystemDataFactory systemDataFactory) {

            foreach (CharacterStatToResourceNode characterStatToResourceNode in primaryToResourceConversion) {
                characterStatToResourceNode.SetupScriptableObjects(systemDataFactory);
            }

        }
    }


}