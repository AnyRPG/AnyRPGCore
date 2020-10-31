using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Faction Change Config", menuName = "AnyRPG/Interactable/ClassChangeConfig")]
    public class ClassChangeConfig : InteractableOptionConfig {

        [SerializeField]
        private ClassChangeProps interactableOptionProps = new ClassChangeProps();

        [Tooltip("the class that this interactable option offers")]
        [SerializeField]
        private string className = string.Empty;

        public ClassChangeProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }
    }

}