using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class GatheringNode : LootableNode {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        // gathering nodes are special.  The image is based on what ability it supports
        public override Sprite MyIcon {
            get {
                return (MyAbility.MyIcon != null ? MyAbility.MyIcon : base.MyIcon);
            }
        }

        public override Sprite MyNamePlateImage {
            get {
                return (MyAbility.MyIcon != null ? MyAbility.MyIcon : base.MyNamePlateImage);
            }
        }
        public override string MyInteractionPanelTitle { get => (MyAbility != null ? MyAbility.MyName : base.MyInteractionPanelTitle); }

        /// <summary>
        /// The ability to cast in order to mine this node
        /// </summary>
        [SerializeField]
        private string abilityName = string.Empty;

        private GatherAbility realAbility = null;

        public GatherAbility MyAbility { get => realAbility; }

        protected override void Awake() {
            //Debug.Log(gameObject.name + ".GatheringNode.Awake();");
            base.Awake();
            if (abilityName != null && abilityName != string.Empty) {
                realAbility = SystemAbilityManager.MyInstance.GetResource(abilityName) as GatherAbility;
            }
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".GatheringNode.Interact(" + source.name + ")");
            if (lootTableNames == null) {
                //Debug.Log(gameObject.name + ".GatheringNode.Interact(" + source.name + "): lootTable was null!");
                return true;
            }
            // base.Interact() will drop loot automatically so we will intentionally not call it because the loot drop in this class is activated by the gatherability
            /*
            int lootCount = 0;
            base.Interact(source);

            foreach (LootTable lootTable in lootTables) {
                if (lootTable.MyDroppedItems.Count > 0) {
                    lootCount += lootTable.MyDroppedItems.Count;
                }
            }
            */
            //if (lootCount > 0) {
            if (lootDropped == true) {
                PickUp();
            } else {
                source.GetComponent<CharacterUnit>().MyCharacter.CharacterAbilityManager.BeginAbility(MyAbility, gameObject);
            }
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            return true;
            //return PickUp();
        }

        public override void DropLoot() {
            base.DropLoot();
            PickUp();
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".GatheringNode.GetCurrentOptionCount()");
            return (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(MyAbility) == true && interactable.MySpawnReference != null ? 1 : 0);
        }

        /*
        public override void HandlePrerequisiteUpdates() {
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override bool CanInteract(CharacterUnit source) {
            bool returnValue = base.CanInteract(source);
            if (returnValue == false) {
                return false;
            }
            return (GetCurrentOptionCount() == 0 ? false : true);
        }
        */
    }

}