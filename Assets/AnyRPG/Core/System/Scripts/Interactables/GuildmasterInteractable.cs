using System;
using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class GuildmasterInteractable : InteractableOption {

        [SerializeField]
        private GuildmasterProps guildmasterProps = new GuildmasterProps();

        public override InteractableOptionProps InteractableOptionProps { get => guildmasterProps; }
    }

}