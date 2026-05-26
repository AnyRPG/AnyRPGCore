using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Credits Category", menuName = "AnyRPG/CreditsCategory")]
    [System.Serializable]
    public class CreditsCategory : DescribableResource {

        [Header("Credits")]

        [Tooltip("The category these will be grouped under on the credits screen")]
        [SerializeField]
        private string categoryName = string.Empty;

        [Tooltip("this skill is considered to be in use by an unarmed character if set to true")]
        [SerializeField]
        private List<CreditsNode> creditsNodes = new List<CreditsNode>();

        public List<CreditsNode> CreditsNodes { get => creditsNodes; set => creditsNodes = value; }
        public string CategoryName { get => categoryName; set => categoryName = value; }
    }

}