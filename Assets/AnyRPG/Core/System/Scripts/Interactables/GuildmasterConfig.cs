using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Guild Master Config", menuName = "AnyRPG/Interactable/GuildmasterConfig")]
    public class GuildmasterConfig : InteractableOptionConfig {

        [SerializeField]
        private GuildmasterProps interactableOptionProps = new GuildmasterProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}