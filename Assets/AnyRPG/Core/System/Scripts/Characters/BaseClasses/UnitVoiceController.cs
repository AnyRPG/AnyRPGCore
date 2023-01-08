using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {

    public class UnitVoiceController : ConfiguredClass {

        // unit controller of controlling unit
        private UnitController unitController;
        private UnitComponentController unitComponentController = null;

        public UnitVoiceController(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            unitComponentController = unitController.UnitComponentController;
            Configure(systemGameManager);
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            unitController.UnitEventController.OnStartInteract += HandleStartInteract;
            unitController.UnitEventController.OnStopInteract += HandleStopInteract;
            unitController.UnitEventController.OnAggroTarget += HandleAggroTarget;
            unitController.UnitEventController.OnAttack += HandleAttack;
            unitController.UnitEventController.OnTakeDamage += HandleTakeDamage;
            unitController.UnitEventController.OnTakeFallDamage += HandleTakeFallDamage;
            unitController.UnitEventController.OnKillTarget += HandleKillTarget;
            unitController.UnitEventController.OnBeforeDie += HandleBeforeDie;
            unitController.UnitEventController.OnJump += HandleJump;
        }

        public void ResetSettings() {
            unitController.UnitEventController.OnStartInteract -= HandleStartInteract;
            unitController.UnitEventController.OnStopInteract -= HandleStopInteract;
            unitController.UnitEventController.OnAggroTarget -= HandleAggroTarget;
            unitController.UnitEventController.OnAttack -= HandleAttack;
            unitController.UnitEventController.OnTakeDamage -= HandleTakeDamage;
            unitController.UnitEventController.OnTakeFallDamage -= HandleTakeFallDamage;
            unitController.UnitEventController.OnKillTarget -= HandleKillTarget;
            unitController.UnitEventController.OnBeforeDie -= HandleBeforeDie;
            unitController.UnitEventController.OnJump -= HandleJump;
        }


        public void HandleStartInteract(InteractableOptionComponent interactableOptionComponent) {
            if (unitController.UnitProfile == null) {
                return;
            }

            if (interactableOptionComponent.PlayInteractionSound() == true) {
                unitComponentController.PlayVoiceSound(interactableOptionComponent.GetInteractionSound(unitController.UnitProfile.VoiceProps));
            }
        }

        public void HandleStopInteract(InteractableOptionComponent interactableOptionComponent) {
            if (unitController.UnitProfile == null) {
                return;
            }
            if (interactableOptionComponent.GetType() == typeof(VendorComponent)) {
                unitComponentController.PlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomStopVendorInteract);
            } else {
                if (interactableOptionComponent.PlayInteractionSound() == true) {
                    unitComponentController.PlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomStopInteract);
                }
            }
        }

        public void HandleAggroTarget() {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitComponentController.PlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomAggro);
        }

        public void HandleAttack() {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitComponentController.PlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomAttack);
        }

        public void HandleTakeDamage() {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitComponentController.PlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomDamage);
        }

        public void HandleTakeFallDamage() {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitComponentController.PlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomFallDamage);
        }

        public void HandleKillTarget() {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitComponentController.PlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomVictory);
        }

        public void HandleBeforeDie(CharacterStats characterStats) {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitComponentController.PlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomDeath);
        }

        public void HandleJump() {
            if (unitController.UnitProfile == null) {
                return;
            }
            unitComponentController.PlayVoiceSound(unitController.UnitProfile.VoiceProps.RandomJump);
        }

    }

}