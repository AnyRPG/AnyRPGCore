using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class CharacterUnit : InteractableOptionComponent {

        public event System.Action<UnitController> OnDespawn = delegate { };

        protected float despawnDelay = 20f;

        private float hitBoxSize = 1.5f;

        private Coroutine despawnCoroutine = null;

        private BaseCharacter baseCharacter = null;
        private UnitController unitController = null;

        public override string DisplayName { get => (BaseCharacter != null ? BaseCharacter.CharacterName : interactableOptionProps.GetInteractionPanelTitle()); }
        public override int PriorityValue { get => -1; }
        public BaseCharacter BaseCharacter {
            get => baseCharacter;
        }

        protected float DespawnDelay { get => despawnDelay; set => despawnDelay = value; }

        public float HitBoxSize { get => hitBoxSize; set => hitBoxSize = value; }

        public CharacterUnit(UnitController unitController, InteractableOptionProps interactableOptionProps, SystemGameManager systemGameManager) : base(unitController, interactableOptionProps, systemGameManager) {
            this.unitController = unitController;
            if (interactable.Collider != null) {
                hitBoxSize = interactable.Collider.bounds.extents.y * 1.5f;
            }
        }

        public void SetBaseCharacter(BaseCharacter baseCharacter) {
            //Debug.Log(interactable.gameObject.name + ".CharacterUnit.SetBaseCharacter: " + baseCharacter.gameObject.name);
            this.baseCharacter = baseCharacter;

        }

        public void SetCharacterStatsCapabilities() {
            // there are some properties that can come from buffs that we want to store on the unitController to avoid expensive lookups every frame
            // if this is a player those may have been saved in buffs from a loaded game before the actual unit spawn, so set them now
            if (baseCharacter.CharacterStats.HasFlight() == true) {
                baseCharacter.UnitController.CanFlyOverride = true;
            }
            if (baseCharacter.CharacterStats.HasGlide() == true) {
                baseCharacter.UnitController.CanGlideOverride = true;
            }
        }

        public static CharacterUnit GetCharacterUnit(Interactable searchInteractable) {
            if (searchInteractable == null) {
                //Debug.Log("CharacterUnit.GetCharacterUnit: searchInteractable is null");
                return null;
            }
            return searchInteractable.CharacterUnit;
        }

        public void EnableCollider() {
            if (interactable.Collider != null) {
                interactable.Collider.enabled = true;
            }
        }

        public void DisableCollider() {
            if (interactable.Collider != null) {
                interactable.Collider.enabled = false;
            }
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
            if (ProcessFactionValue(factionValue) == true && baseCharacter.CharacterStats.IsAlive == true) {
                //Debug.Log(source.name + " can interact with us!");
                return true;
            }
            //Debug.Log($"{gameObject.name}.CharacterUnit.CanInteract: " + source.name + " was unable to interact with (attack) us!");
            return false;
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(interactable.gameObject.name + ".CharacterUnit.Interact(" + source.DisplayName + ")");

            float relationValue = interactable.PerformFactionCheck(playerManager.MyCharacter);
            if (CanInteract(false, false, relationValue)) {
                base.Interact(source, optionIndex);

                // attempt to put the caster in combat so it can unsheath bows, wands, etc
                source.BaseCharacter.CharacterCombat.Attack(baseCharacter, true);

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
            if (baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.Player) {
                return true;
            }
            return base.HasMainMapIcon();
        }

        public override Color GetMiniMapIconColor() {
            if (baseCharacter != null && baseCharacter != playerManager.MyCharacter) {
                return Faction.GetFactionColor(playerManager, playerManager.MyCharacter, baseCharacter);
            }

            return base.GetMiniMapIconColor();
        }

        public void Despawn(float despawnDelay = 0f, bool addSystemDefaultTime = true, bool forceDespawn = false) {
            //Debug.Log($"{BaseCharacter.gameObject.name}.CharacterUnit.Despawn({despawnDelay}, {addSystemDefaultTime}, {forceDespawn})");

            //gameObject.SetActive(false);
            // TEST ADDING A MANDATORY DELAY
            if (despawnCoroutine == null && interactable.gameObject.activeSelf == true && interactable.isActiveAndEnabled) {
                //Debug.Log(BaseCharacter.gameObject.name + ".CharacterUnit.Despawn(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + ") starting despawn coroutine");
                despawnCoroutine = interactable.StartCoroutine(PerformDespawnDelay(despawnDelay, addSystemDefaultTime, forceDespawn));
            } else {
                //Debug.Log(BaseCharacter.gameObject.name + ".CharacterUnit.Despawn(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + ") despawncoroutine was not null");
            }
        }

        public void CancelDespawnDelay() {
            //Debug.Log(BaseCharacter.gameObject.name + ".CharacterUnit.CancelDespawnDelay()");
            if (despawnCoroutine != null) {
                interactable.StopCoroutine(despawnCoroutine);
                despawnCoroutine = null;
            }
        }

        public IEnumerator PerformDespawnDelay(float despawnDelay, bool addSystemDefaultTime = true, bool forceDespawn = false) {
            //Debug.Log(BaseCharacter.gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + ") " + this.despawnDelay);

            if (forceDespawn == false) {
                // add all possible delays together
                float extraTime = 0f;
                if (addSystemDefaultTime) {
                    extraTime = systemConfigurationManager.DefaultDespawnTimer;
                }
                float totalDelay = despawnDelay + this.despawnDelay + extraTime;
                while (totalDelay > 0f) {
                    yield return null;
                    totalDelay -= Time.deltaTime;
                }
            }

            if ((baseCharacter.CharacterStats.IsAlive == false && baseCharacter.CharacterStats.IsReviving == false) || forceDespawn == true) {
                //Debug.Log(BaseCharacter.gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + "): despawning");
                // this character could have been ressed while waiting to despawn.  don't let it despawn if that happened unless forceDesapwn is true (such as at the end of a patrol)
                // we are going to send this ondespawn call now to allow another unit to respawn from a spawn node without a long wait during events that require rapid mob spawning
                OnDespawn(baseCharacter.UnitController);
                baseCharacter.UnitController.Despawn();
            } else {
                //Debug.Log(BaseCharacter.gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + "): unit is alive or reviving !! NOT DESPAWNING");
            }
            despawnCoroutine = null;
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
            return (BaseCharacter.CharacterStats.IsAlive == true ? 1 : 0);
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }


    }

}