using AnyRPG;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class StatScalingNode {

        [Tooltip("The stat")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(CharacterStat))]
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

        [Tooltip("Convert primary stats to resource regen")]
        [SerializeField]
        private List<PowerResourceRegenProperty> regen = new List<PowerResourceRegenProperty>();

        public string StatName { get => statName; set => statName = value; }
        public float BudgetPerLevel { get => budgetPerLevel; set => budgetPerLevel = value; }
        public List<PrimaryToSecondaryStatNode> PrimaryToSecondaryConversion { get => primaryToSecondaryConversion; set => primaryToSecondaryConversion = value; }
        public List<CharacterStatToResourceNode> PrimaryToResourceConversion { get => primaryToResourceConversion; set => primaryToResourceConversion = value; }
        public List<PowerResourceRegenProperty> Regen { get => regen; set => regen = value; }

        public void SetupScriptableObjects(SystemDataFactory systemDataFactory) {

            foreach (CharacterStatToResourceNode characterStatToResourceNode in primaryToResourceConversion) {
                characterStatToResourceNode.SetupScriptableObjects(systemDataFactory);
            }

        }
    }

}