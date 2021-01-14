using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Animated Object Config", menuName = "AnyRPG/Interactable/AnimatedObjectConfig")]
    public class AnimatedObjectConfig : InteractableOptionConfig {

        [SerializeField]
        private AnimatedObjectProps interactableOptionProps = new AnimatedObjectProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}