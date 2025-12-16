using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterEquipmentButton : CharacterEquipmentButtonBase {

        protected CharacterPanel characterPanel = null;
        
        public CharacterPanel CharacterPanel { get => characterPanel; set => characterPanel = value; }

        protected override void HandleLeftClick() {
            base.HandleLeftClick();
            if (handScript.Moveable is InstantiatedEquipment) {
                InstantiatedEquipment tmp = (InstantiatedEquipment)handScript.Moveable;
                if (equipmentSlotProfile.EquipmentSlotTypeList.Contains(tmp.Equipment.EquipmentSlotType)) {
                    playerManager.UnitController.CharacterEquipmentManager.RequestEquipToSlot(tmp, equipmentSlotProfile);
                    handScript.Drop();
                    uIManager.RefreshTooltip(tmp);
                }
            } else if (handScript.Moveable == null && equippedEquipment != null) {
                handScript.TakeMoveable(equippedEquipment);
                characterPanel.SelectedButton = this;
                icon.color = Color.gray;
            }
        }

        public override void Accept() {
            base.Accept();
            if (equippedEquipment != null) {
                playerManager.UnitController.CharacterEquipmentManager.Unequip(equipmentSlotProfile);
                playerManager.UnitController.UnitModelController.RebuildModelAppearance();
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


    }

}