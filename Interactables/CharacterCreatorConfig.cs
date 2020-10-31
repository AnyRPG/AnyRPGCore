using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Character Creator Config", menuName = "AnyRPG/Interactable/CharacterCreatorConfig")]
    public class CharacterCreatorConfig : InteractableOptionConfig {

        [SerializeField]
        private CharacterCreatorProps interactableOptionProps = new CharacterCreatorProps();

        public CharacterCreatorProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }
    }

}