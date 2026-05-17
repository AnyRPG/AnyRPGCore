using System;
using System.Collections;
using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Faction Change Config", menuName = "AnyRPG/Interactable/GuildmasterConfig")]
    public class GuildmasterConfig : InteractableOptionConfig {

        [SerializeField]
        private GuildmasterProps interactableOptionProps = new GuildmasterProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}