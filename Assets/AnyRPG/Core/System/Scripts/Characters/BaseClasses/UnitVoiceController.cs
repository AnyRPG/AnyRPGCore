using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AnyRPG {

    public class UnitVoiceController : ConfiguredClass {

        // unit controller of controlling unit
        private UnitController unitController;
        //private UnitComponentController unitComponentController = null;

        public UnitVoiceController(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            //unitComponentController = unitController.UnitComponentController;
            Configure(systemGameManager);
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //unitController.UnitEventController.OnStartInteractWithOption += HandleStartInteractWithOption;
            unitController.InteractableEventController.OnInteractionWithOptionStarted += HandleStartInteractWithOption;
            unitController.UnitEventController.OnStopInteractWithOption += HandleStopInteractWithOption;
            unitController.UnitEventController.OnStartInteract += HandleStartInteract;
            unitController.UnitEventController.OnStopInteract += HandleStopInteract;
            unitController.UnitEventController.OnAggroTarget += HandleAggroTarget;
            unitController.UnitEventController.OnAttack += HandleAttack;
            unitController.UnitEventController.OnTakeDamage += HandleTakeDamage;
            unitController.UnitEventController.OnKillTarget += HandleKillTarget;
            unitController.UnitEventController.OnBeforeDie += HandleBeforeDie;
            unitController.UnitEventController.OnJump += HandleJump;

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) {
                unitController.UnitEventController.OnTakeFallDamage += HandleTakeFallDamage;
            }
        }

        public void ResetSettings() {
            //unitController.UnitEventController.OnStartInteractWithOption -= HandleStartInteractWithOption;
            unitController.InteractableEventController.OnInteractionWithOptionStarted -= HandleStartInteractWithOption;
            unitController.UnitEventController.OnStopInteractWithOption -= HandleStopInteractWithOption;
            unitController.UnitEventController.OnAggroTarget -= HandleAggroTarget;
            unitController.UnitEventController.OnAttack -= HandleAttack;
            unitController.UnitEventController.OnTakeDamage -= HandleTakeDamage;
            unitController.UnitEventController.OnKillTarget -= HandleKillTarget;
            unitController.UnitEventController.OnBeforeDie -= HandleBeforeDie;
            unitController.UnitEventController.OnJump -= HandleJump;

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) {
                unitController.UnitEventController.OnTakeFallDamage += HandleTakeFallDamage;
            }
        }

        public void HandleStartInteract() {
            if (unitController.UnitProfile == null) {
                return;
            }

            unitController.InteractableEventController.NotifyOnPlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomStartInteract);
        }

        public void HandleStopInteract() {
            if (unitController.UnitProfile == null) {
                return;
            }

            unitController.InteractableEventController.NotifyOnPlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomStopInteract);
        }


        public void HandleStartInteractWithOption(UnitController sourceUnitController, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            //Debug.Log($"{unitController.gameObject.name}.UnitVoiceController.HandleStartInteractWithOption({sourceUnitController.gameObject.name}, {interactableOptionComponent.Interactable.DisplayName}, {componentIndex}, {choiceIndex})");
            if (unitController.UnitProfile == null) {
                return;
            }

            if (interactableOptionComponent.PlayInteractionSound() == true) {
                AudioClip audioClip = interactableOptionComponent.GetInteractionSound(unitController.UnitProfile.VoiceProps);
                if (audioClip != null) {
                    unitController.InteractableEventController.NotifyOnPlayVoiceSound(audioClip);
                } else {
                    Debug.LogWarning($"{unitController.gameObject.name}.UnitVoiceController.HandleStartInteractWithOption: No audio clip found for {interactableOptionComponent.Interactable.DisplayName} option {componentIndex} choice {choiceIndex}");
                }
            }
        }

        public void HandleStopInteractWithOption(InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log($"{unitController.gameObject.name}.UnitVoiceController.HandleStopInteractWithOption({interactableOptionComponent.Interactable.DisplayName})");

            if (unitController.UnitProfile == null) {
                return;
            }
            if (interactableOptionComponent.GetType() == typeof(VendorComponent)) {
                unitController.InteractableEventController.NotifyOnPlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomStopVendorInteract);
            } else {
                if (interactableOptionComponent.PlayInteractionSound() == true) {
                    unitController.InteractableEventController.NotifyOnPlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomStopInteract);
                }
            }
        }

        public void HandleAggroTarget() {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitController.InteractableEventController.NotifyOnPlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomAggro);
        }

        public void HandleAttack() {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitController.InteractableEventController.NotifyOnPlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomAttack);
        }

        public void HandleTakeDamage(IAbilityCaster caster, UnitController controller, int amount, CombatTextType type, CombatMagnitude magnitude, string abilityName, AbilityEffectContext context) {
            //Debug.Log($"{unitController.gameObject.name}.UnitVoiceController.HandleTakeDamage({caster.transform.name}, {controller.gameObject.name}, {amount}, {type}, {magnitude}, {abilityName})");

            if (unitController.UnitProfile == null) {
                return;
            }
            AudioClip audioClip = unitController.UnitProfile.VoiceProps.RandomDamage;
            if (audioClip == null) {
                //Debug.LogWarning($"{unitController.gameObject.name}.UnitVoiceController.HandleTakeDamage: No audio clip found for damage");
                return;
            }
            unitController.InteractableEventController.NotifyOnPlayVoiceSound(audioClip);
        }

        public void HandleTakeFallDamage(int damageAmount) {
            //Debug.Log($"{unitController.gameObject.name}.UnitVoiceController.HandleTakeFallDamage({damageAmount})");

            if (unitController.UnitProfile == null) {
                return;
            }
            unitController.InteractableEventController.NotifyOnPlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomFallDamage);
        }

        public void HandleKillTarget() {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitController.InteractableEventController.NotifyOnPlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomVictory);
        }

        public void HandleBeforeDie(UnitController targetUnitController) {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitController.InteractableEventController.NotifyOnPlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomDeath);
        }

        public void HandleJump() {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitController.InteractableEventController.NotifyOnPlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomJump);
        }

    }

}