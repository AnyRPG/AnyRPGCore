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

        [Header("Character Stat")]

        [Tooltip("If true, all characters will receive this stat, using the budget and conversions below.")]
        [SerializeField]
        private bool globalStat = true;

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

        public string StatName { get => DisplayName; }
        public float BudgetPerLevel { get => budgetPerLevel; set => budgetPerLevel = value; }
        public List<PrimaryToSecondaryStatNode> PrimaryToSecondaryConversion { get => primaryToSecondaryConversion; set => primaryToSecondaryConversion = value; }
        public List<CharacterStatToResourceNode> PrimaryToResourceConversion { get => primaryToResourceConversion; set => primaryToResourceConversion = value; }
        public List<PowerResourceRegenProperty> Regen { get => regen; set => regen = value; }
        public bool GlobalStat { get => globalStat; set => globalStat = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            foreach (CharacterStatToResourceNode characterStatToResourceNode in primaryToResourceConversion) {
                characterStatToResourceNode.SetupScriptableObjects(systemDataFactory);
            }
        }

    }


}