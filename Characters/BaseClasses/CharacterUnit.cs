using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterUnit : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public event System.Action<UnitController> OnDespawn = delegate { };

        //[SerializeField]
        protected float despawnDelay = 20f;

        private Collider capsuleCollider;

        private float hitBoxSize = 1.5f;

        private Coroutine despawnCoroutine = null;

        private bool startHasRun = false;

        private BaseCharacter baseCharacter = null;

        public BaseCharacter BaseCharacter {
            get => baseCharacter;
            set {
                baseCharacter = value;
            }
        }

        protected float MyDespawnDelay { get => despawnDelay; set => despawnDelay = value; }

        public Collider MyCapsuleCollider { get => capsuleCollider; set => capsuleCollider = value; }
        public float HitBoxSize { get => hitBoxSize; set => hitBoxSize = value; }

        public CharacterUnit(Interactable interactable) : base(interactable) {
        }

        public void EnableCollider() {
            if (capsuleCollider != null) {
                capsuleCollider.enabled = true;
            }
        }

        public void DisableCollider() {
            if (capsuleCollider != null) {
                capsuleCollider.enabled = false;
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

        protected override void Start() {
            //Debug.Log(gameObject.name + ".CharacterUnit.Start()");
            base.Start();
            SetDefaultLayer();

            startHasRun = true;

            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.Player) {
                // this code is a quick way to set speed on third party controllers when the player spawns
                if (BaseCharacter.CharacterStats != null) {
                    EventParamProperties eventParam = new EventParamProperties();
                    eventParam.simpleParams.FloatParam = BaseCharacter.CharacterStats.RunSpeed;
                    SystemEventManager.TriggerEvent("OnSetRunSpeed", eventParam);

                    eventParam.simpleParams.FloatParam = BaseCharacter.CharacterStats.SprintSpeed;
                    SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);

                }
                if (SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl) {
                    KeyBindManager.MyInstance.SendKeyBindEvents();
                }

            }
        }

        public static bool IsInLayerMask(int layer, LayerMask layermask) {
            return layermask == (layermask | (1 << layer));
        }

        protected virtual void SetDefaultLayer() {
            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.Player) {
                // players should stay on player unit layer
                return;
            }
            if (SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer != null && SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer != string.Empty) {
                int defaultLayer = LayerMask.NameToLayer(SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer);
                int finalmask = (1 << defaultLayer) | (1 << UnitPreviewManager.MyInstance.PreviewLayer) | (1 << PetPreviewManager.MyInstance.PreviewLayer);
                if (!IsInLayerMask(gameObject.layer, finalmask)) {
                    //if (gameObject.layer != defaultLayer) {
                    gameObject.layer = defaultLayer;
                    Debug.Log(gameObject.name + ".CharacterUnit.SetDefaultLayer(): object was not set to correct layer: " + SystemConfigurationManager.MyInstance.MyDefaultCharacterUnitLayer + ". Setting automatically");
                }
            }
        }
        
        public override void OnDisable() {
            //Debug.Log(gameObject.name + ".CharacterUnit.OnDisable()");
            base.OnDisable();
            if (despawnCoroutine != null) {
                interactable.StopCoroutine(despawnCoroutine);
            }
        }

        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences()");
            if (componentReferencesInitialized) {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): already initialized. exiting!");
                return;
            }
            base.GetComponentReferences();

            capsuleCollider = GetComponent<Collider>();
            if (capsuleCollider != null) {
                hitBoxSize = capsuleCollider.bounds.extents.y * 1.5f;
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): found collider");
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.GetComponentReferences(): DID NOT FIND collider");
            }
        }


        /// <summary>
        /// The default interaction on any character is to be attacked.  Return true if the relationship is less than 0.
        /// </summary>
        /// <param name="targetCharacter"></param>
        /// <returns></returns>
        public override bool CanInteract() {
            if (Faction.RelationWith(PlayerManager.MyInstance.MyCharacter, BaseCharacter) <= -1 && baseCharacter.CharacterStats.IsAlive == true) {
                //Debug.Log(source.name + " can interact with us!");
                return true;
            }
            //Debug.Log(gameObject.name + ".CharacterUnit.CanInteract: " + source.name + " was unable to interact with (attack) us!");
            return false;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".CharacterUnit.Interact(" + source.name + ")");
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
            if (despawnCoroutine == null && gameObject.activeSelf == true && isActiveAndEnabled) {
                despawnCoroutine = StartCoroutine(PerformDespawnDelay(despawnDelay, addSystemDefaultTime, forceDespawn));
            }
        }

        public IEnumerator PerformDespawnDelay(float despawnDelay, bool addSystemDefaultTime = true, bool forceDespawn = false) {
            //Debug.Log(gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + ")");
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

            if (baseCharacter.CharacterStats.IsAlive == false || forceDespawn == true) {
                //Debug.Log(gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + "): despawning");
                // this character could have been ressed while waiting to despawn.  don't let it despawn if that happened unless forceDesapwn is true (such as at the end of a patrol)
                // we are going to send this ondespawn call now to allow another unit to respawn from a spawn node without a long wait during events that require rapid mob spawning
                OnDespawn(baseCharacter.UnitController);
                Destroy(baseCharacter.UnitController.gameObject);

            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.PerformDespawnDelay(" + despawnDelay + ", " + addSystemDefaultTime + ", " + forceDespawn + "): unit is alive!! NOT DESPAWNING");
            }
        }

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

        // CHARACTER UNIT ALIVE IS ALWAYS VALID AND CURRENT TO ALLOW ATTACKS
        public override int GetValidOptionCount() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetValidOptionCount()");
            return (BaseCharacter.CharacterStats.IsAlive == true ? 1 : 0);
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".CharacterUnit.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


    }

}