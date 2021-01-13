using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Credits Category", menuName = "AnyRPG/CreditsCategory")]
    [System.Serializable]
    public class CreditsCategory : DescribableResource {

        [Header("Credits")]

        [Tooltip("this skill is considered to be in use by an unarmed character if set to true")]
        [SerializeField]
        private List<CreditsNode> creditsNodes = new List<CreditsNode>();

        public List<CreditsNode> MyCreditsNodes { get => creditsNodes; set => creditsNodes = value; }
    }

}