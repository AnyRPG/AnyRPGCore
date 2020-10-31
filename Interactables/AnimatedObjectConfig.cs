using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Animated Object Config", menuName = "AnyRPG/Interactable/AnimatedObjectConfig")]
    [System.Serializable]
    public class AnimatedObjectConfig : InteractableOptionConfig {

        [SerializeField]
        private AnimatedObjectProps interactableOptionProps = new AnimatedObjectProps();

        public AnimatedObjectProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }
    }

}