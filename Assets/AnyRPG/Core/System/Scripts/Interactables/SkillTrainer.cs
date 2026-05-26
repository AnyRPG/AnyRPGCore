using UnityEngine;

namespace AnyRPG {
    public class SkillTrainer : InteractableOption {

        [SerializeField]
        private SkillTrainerProps skillTrainerProps = new SkillTrainerProps();

        public override InteractableOptionProps InteractableOptionProps { get => skillTrainerProps; }
    }

}