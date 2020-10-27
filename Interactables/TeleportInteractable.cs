using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class TeleportInteractable : PortalInteractable {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private TeleportProps teleportProps = new TeleportProps();

        private TeleportConfig teleportConfig = null;

        [Header("Teleport")]

        [Tooltip("When interacted with, the player will cast this ability. Only applies if Portal Type is Ability.")]
        [SerializeField]
        private string abilityName = string.Empty;

        private BaseAbility ability = null;

        public BaseAbility BaseAbility { get => ability; }

        public TeleportInteractable(Interactable interactable, TeleportConfig interactableOptionConfig) : base(interactable, interactableOptionConfig) {
            this.teleportConfig = interactableOptionConfig;
        }


        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source);

            source.BaseCharacter.CharacterAbilityManager.BeginAbility(ability);
            return true;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (abilityName != null && abilityName != string.Empty) {
                BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName);
                if (baseAbility != null) {
                    ability = baseAbility;
                } else {
                    Debug.LogError(gameObject.name + ".PortalInteractable.SetupScriptableObjects(): COULD NOT FIND ABILITY " + abilityName + " while initializing " + gameObject.name);
                }
            }
        }


    }
}