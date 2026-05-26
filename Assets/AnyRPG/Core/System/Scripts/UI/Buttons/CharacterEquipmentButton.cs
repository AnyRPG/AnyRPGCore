using UnityEngine;

namespace AnyRPG {
    public class CharacterEquipmentButton : CharacterEquipmentButtonBase, IMoveableOwner, IContextMenuTarget {

        protected CharacterPanel characterPanel = null;

        // game manager references
        private ContextMenuService contextMenuService = null;

        public IMoveable Moveable { get => equippedEquipment; }
        public CharacterPanel CharacterPanel { get => characterPanel; set => characterPanel = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            contextMenuService = systemGameManager.ContextMenuService;
        }

        protected override void HandleLeftClick() {
            //Debug.Log("CharacterEquipmentButton.HandleLeftClick()");

            base.HandleLeftClick();

            // pick up equipment case
            if (handScript.MoveableOwner == null && equippedEquipment != null) {
                handScript.TakeMoveable(this);
                characterPanel.SelectedButton = this;
                icon.color = Color.gray;
                return;
            }

            // same item case
            if (handScript.MoveableOwner != null && handScript.MoveableOwner.Moveable == equippedEquipment) {
                handScript.CancelMove();
                return;
            }

            // did not come from inventory case
            if (playerManagerClient.UnitController.CharacterInventoryManager.FromSlot != null && (playerManagerClient.UnitController.CharacterInventoryManager.FromSlot.BagPanel is InventoryPanel == false)) {
                handScript.CancelMove();
                return;
            }

            // different item case
            if (handScript.MoveableOwner.Moveable is InstantiatedEquipment) {
                InstantiatedEquipment tmp = (InstantiatedEquipment)handScript.MoveableOwner.Moveable;
                if (equipmentSlotProfile.EquipmentSlotTypeList.Contains(tmp.Equipment.EquipmentSlotType)) {
                    playerManagerClient.UnitController.CharacterEquipmentManager.RequestEquipToSlot(tmp, equipmentSlotProfile);
                    handScript.CompleteMove();
                    uIManager.RefreshTooltip(tmp);
                }
            }
        }

        public override void Accept() {
            base.Accept();
            if (equippedEquipment != null) {
                playerManagerClient.UnitController.CharacterEquipmentManager.Unequip(equipmentSlotProfile);
                playerManagerClient.UnitController.UnitModelController.RebuildModelAppearance();
                ShowContextInfo();
            }
        }

        public override string GetDescription() {
            string returnString = base.GetDescription();
            if (returnString == string.Empty) {
                returnString = "Drag equipment here to equip it";
            }
            return returnString;
        }

        public void CancelHandscriptMove() {
            //Debug.Log("CharacterEquipmentButton.CancelHandscriptMove()");

            UpdateVisual(playerManagerClient.UnitController);
        }

        protected override void HandleRightClick() {
            base.HandleRightClick();
            if (equippedEquipment != null) {
                contextMenuService.ShowContextMenu(this, Input.mousePosition);
            }
        }

        public void SetupContextMenu(ContextMenuPanel contextMenuPanel) {
            if (equippedEquipment != null) {
                contextMenuPanel.EnableUnequipButton(true);
            }
        }

        public void PerformContextMenuAction(string actionName) {
            playerManagerClient.UnitController.CharacterEquipmentManager.RequestUnequip(equippedEquipment);
        }
    }

}