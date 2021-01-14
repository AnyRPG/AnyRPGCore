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

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}