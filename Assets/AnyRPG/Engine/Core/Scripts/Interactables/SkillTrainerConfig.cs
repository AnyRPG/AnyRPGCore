using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Skill Trainer Config", menuName = "AnyRPG/Interactable/SkillTrainerConfig")]
    public class SkillTrainerConfig : InteractableOptionConfig {

        [SerializeField]
        private SkillTrainerProps interactableOptionProps = new SkillTrainerProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}