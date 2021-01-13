using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Name Change Config", menuName = "AnyRPG/Interactable/NameChangeConfig")]
    public class NameChangeConfig : InteractableOptionConfig {

        [SerializeField]
        private NameChangeProps interactableOptionProps = new NameChangeProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}