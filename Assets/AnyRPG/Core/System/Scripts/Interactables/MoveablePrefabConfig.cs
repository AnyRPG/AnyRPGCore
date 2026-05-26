using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Moveable Prefab Config", menuName = "AnyRPG/Interactable/Moveable Prefab Config")]
    public class MoveablePrefabConfig : InteractableOptionConfig {

        [SerializeField]
        private MoveablePrefabProps interactableOptionProps = new MoveablePrefabProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}