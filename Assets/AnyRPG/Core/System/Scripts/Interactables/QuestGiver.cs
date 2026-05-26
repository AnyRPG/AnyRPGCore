using UnityEngine;

namespace AnyRPG {
    public class QuestGiver : InteractableOption {

        [SerializeField]
        private QuestGiverProps questGiverProps = new QuestGiverProps();

        public override InteractableOptionProps InteractableOptionProps { get => questGiverProps; }
    }

}