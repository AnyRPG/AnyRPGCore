using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Name Change Config", menuName = "AnyRPG/Interactable/NameChangeConfig")]
    public class NameChangeConfig : InteractableOptionConfig {

        [SerializeField]
        private NameChangeProps interactableOptionProps = new NameChangeProps();

        public NameChangeProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }
    }

}