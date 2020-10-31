using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SkillTrainer : InteractableOption {

        [SerializeField]
        private SkillTrainerProps skillTrainerProps = new SkillTrainerProps();

        [SerializeField]
        private List<string> skillNames = new List<string>();

        public override InteractableOptionProps InteractableOptionProps { get => skillTrainerProps; }
    }

}