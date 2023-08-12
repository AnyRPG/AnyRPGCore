using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class CharacterUnit : InteractableOptionComponent {

        private float hitBoxSize = 1.5f;

        private UnitController unitController = null;

        public override string DisplayName { get => unitController.BaseCharacter.CharacterName; }
        public override int PriorityValue { get => -1; }

        public float HitBoxSize { get => hitBoxSize; set => hitBoxSize = value; }
        public UnitController UnitController { get => unitController; }

        public CharacterUnit(UnitController unitController, InteractableOptionProps interactableOptionProps, SystemGameManager systemGameManager) : base(unitController, interactableOptionProps, systemGameManager) {
            this.unitController = unitController;
            if (interactable.Collider != null) {
                hitBoxSize = interactable.Collider.bounds.extents.y * 1.5f;
            }
        }

        public void SetCharacterStatsCapabilities() {
            // there are some properties that can come from buffs that we want to store on the unitController to avoid expensive lookups every frame
            // if this is a player those may have been saved in buffs from a loaded game before the actual unit spawn, so set them now
            if (unitController.CharacterStats.HasFlight() == true) {
                unitController.CanFlyOverride = true;
            }
            if (unitController.CharacterStats.HasGlide() == true) {
                unitController.CanGlideOverride = true;
            }
        }

        public static CharacterUnit GetCharacterUnit(Interactable searchInteractable) {
            if (searchInteractable == null) {
                //Debug.Log("CharacterUnit.GetCharacterUnit: searchInteractable is null");
                return null;
            }
            return searchInteractable.CharacterUnit;
        }

        public void HandleReviveComplete() {

            // give chance to update minimap and put character indicator back on it
            HandlePrerequisiteUpdates();
        }

        public void HandleDie(CharacterStats _characterStats) {
            // give a chance to blank out minimap indicator
            // when the engine is upgraded to support multiplayer, this may need to be revisited.
            // some logic to still show minimap icons for dead players in your group so you can find and res them could be necessary
            HandlePrerequisiteUpdates();
        }

        public override bool ProcessFactionValue(float factionValue) {
            return (factionValue <= -1f ? true : false);
        }

        /// <summary>
        /// The default interaction on any character is to be attacked.  Return true if the relationship is less than 0.
        /// </summary>
        /// <param name="targetCharacter"></param>
        /// <returns></returns>
        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false, float factionValue = 0f, bool processNonCombatCheck = true) {
            if (ProcessFactionValue(factionValue) == true && unitController.CharacterStats.IsAlive == true) {
                //Debug.Log(source.name + " can interact with us!");
                return true;
            }
            //Debug.Log($"{gameObject.name}.CharacterUnit.CanInteract: " + source.name + " was unable to interact with (attack) us!");
            return false;
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(interactable.gameObject.name + ".CharacterUnit.Interact(" + source.DisplayName + ")");

            float relationValue = interactable.PerformFactionCheck(playerManager.UnitController);
            if (CanInteract(false, false, relationValue)) {
                base.Interact(source, optionIndex);

                // attempt to put the caster in combat so it can unsheath bows, wands, etc
                source.UnitController.CharacterCombat.Attack(unitController, true);

                uIManager.interactionWindow.CloseWindow();
                return true;
            }
            //return true;
            return false;
        }

        public override void StopInteract() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.StopInteract()");
            base.StopInteract();
        }

        public override bool CanShowMiniMapIcon() {
            if (unitController.UnitControllerMode == UnitControllerMode.Mount) {
                return false;
            }
            
            if (interactable.CombatOnly) {
                return true;
            }
            return base.CanShowMiniMapIcon();
        }

        public override Sprite GetMiniMapIcon() {
            if (interactable.CombatOnly) {
                return systemConfigurationManager.UIConfiguration.PlayerMiniMapIcon;
            }

            return systemConfigurationManager.UIConfiguration.CharacterMiniMapIcon;
        }

        public override bool HasMiniMapIcon() {
            return true;
        }

        public override bool HasMainMapIcon() {
            //Debug.Log($"{baseCharacter.gameObject.name}.CharacterUnit.HasMainMapIcon()");
            if (unitController.UnitControllerMode == UnitControllerMode.Player) {
                return true;
            }
            return base.HasMainMapIcon();
        }

        public override Color GetMiniMapIconColor() {
            if (unitController != playerManager.UnitController) {
                return Faction.GetFactionColor(playerManager, playerManager.UnitController, unitController);
            }

            return base.GetMiniMapIconColor();
        }

        /*
        public override string GetDescription() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.GetDescription()");
            if (interactionPanelTitle == null || interactionPanelTitle == string.Empty) {
                //Debug.Log($"{gameObject.name}.CharacterUnit.GetDescription(): returning " + MyDisplayName);
                return DisplayName;
            } else {
                //Debug.Log($"{gameObject.name}.CharacterUnit.GetDescription(): returning " + interactionPanelTitle);
                return interactionPanelTitle;
            }
        }
        */

        // CHARACTER UNIT ALIVE IS ALWAYS VALID AND CURRENT TO ALLOW ATTACKS
        public override int GetValidOptionCount() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.GetValidOptionCount()");
            return (unitController.CharacterStats.IsAlive == true ? 1 : 0);
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }


    }

}