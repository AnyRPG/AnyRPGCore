using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class CharacterUnit : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public event System.Action<UnitController> OnDespawn = delegate { };

        protected float despawnDelay = 20f;

        private float hitBoxSize = 1.5f;

        private Coroutine despawnCoroutine = null;

        private BaseCharacter baseCharacter = null;

        public override string DisplayName { get => (BaseCharacter != null ? BaseCharacter.CharacterName : interactableOptionProps.InteractionPanelTitle); }
        public BaseCharacter BaseCharacter {
            get => baseCharacter;
        }

        protected float MyDespawnDelay { get => despawnDelay; set => despawnDelay = value; }

        public float HitBoxSize { get => hitBoxSize; set => hitBoxSize = value; }

        public CharacterUnit(Interactable interactable, InteractableOptionProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            if (interactable.Collider != null) {
                hitBoxSize = interactable.Collider.bounds.extents.y * 1.5f;
            }
        }

        public void SetBaseCharacter(BaseCharacter baseCharacter) {
            //Debug.Log(interactable.gameObject.name + ".CharacterUnit.SetBaseCharacter: " + baseCharacter.gameObject.name);
            this.baseCharacter = baseCharacter;
        }

        public static CharacterUnit GetCharacterUnit(Interactable searchInteractable) {
            if (searchInteractable == null) {
                Debug.Log("CharacterUnit.GetCharacterUnit: searchInteractable is null");
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

        /// <summary>
        /// The default interaction on any character is to be attacked.  Return true if the relationship is less than 0.
        /// </summary>
        /// <param name="targetCharacter"></param>
        /// <returns></returns>
        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false) {
            if (Faction.RelationWith(PlayerManager.MyInstance.MyCharacter, BaseCharacter) <= -1 && baseCharacter.CharacterStats.IsAlive == true) {
                //Debug.Log(source.name + " can interact with us!");
                return true;
            }
            //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract: " + source.name + " was unable to interact with (attack) us!");
            return false;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(interactable.gameObject.name + ".CharacterUnit.Interact(" + source.DisplayName + ")");
            if (CanInteract()) {
                base.Interact(source);

                //source.MyCharacter.MyCharacterCombat.Attack(baseCharacter);
                source.BaseCharacter.CharacterCombat.Attack(baseCharacter);
                PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
                return true;
            }
            //return true;
            return false;
        }

        public override void StopInteract() {
            //Debug.Log(gameObject.name + ".CharacterUnit.StopInteract()");
            base.StopInteract();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log(gameObject.name + ".CharacterUnit.SetMiniMapText()");
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.fontSize = 50;
            if (baseCharacter != null && baseCharacter.Faction != null) {
                text.color = Faction.GetFactionColor(PlayerManager.MyInstance.MyCharacter, baseCharacter);
            }
            return true;
        }

        public void Despawn(float despawnDelay = 0f, bool addSystemDefaultTime = true, bool forceDespawn = false) {
            //Debug.Log(gameObject.name + ".CharacterUnit.Despawn(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + ")");
            //gameObject.SetActive(false);
            // TEST ADDING A MANDATORY DELAY
            if (despawnCoroutine == null && interactable.gameObject.activeSelf == true && interactable.isActiveAndEnabled) {
                despawnCoroutine = interactable.StartCoroutine(PerformDespawnDelay(despawnDelay, addSystemDefaultTime, forceDespawn));
            }
        }

        public IEnumerator PerformDespawnDelay(float despawnDelay, bool addSystemDefaultTime = true, bool forceDespawn = false) {
            //Debug.Log(gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + ")");

            if (forceDespawn == false) {
                // add all possible delays together
                float extraTime = 0f;
                if (addSystemDefaultTime) {
                    extraTime = SystemConfigurationManager.MyInstance.MyDefaultDespawnTimer;
                }
                float totalDelay = despawnDelay + this.despawnDelay + extraTime;
                while (totalDelay > 0f) {
                    yield return null;
                    totalDelay -= Time.deltaTime;
                }
            }

            if (baseCharacter.CharacterStats.IsAlive == false || forceDespawn == true) {
                //Debug.Log(gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + "): despawning");
                // this character could have been ressed while waiting to despawn.  don't let it despawn if that happened unless forceDesapwn is true (such as at the end of a patrol)
                // we are going to send this ondespawn call now to allow another unit to respawn from a spawn node without a long wait during events that require rapid mob spawning
                OnDespawn(baseCharacter.UnitController);
                UnityEngine.Object.Destroy(baseCharacter.UnitController.gameObject);
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + "): unit is alive!! NOT DESPAWNING");
            }
        }
        /*
        public override string GetDescription() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetDescription()");
            if (interactionPanelTitle == null || interactionPanelTitle == string.Empty) {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetDescription(): returning " + MyDisplayName);
                return DisplayName;
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetDescription(): returning " + interactionPanelTitle);
                return interactionPanelTitle;
            }
        }
        */

        // CHARACTER UNIT ALIVE IS ALWAYS VALID AND CURRENT TO ALLOW ATTACKS
        public override int GetValidOptionCount() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetValidOptionCount()");
            return (BaseCharacter.CharacterStats.IsAlive == true ? 1 : 0);
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

        public override void CallMiniMapStatusUpdateHandler() {
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


    }

}