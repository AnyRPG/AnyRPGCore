using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// The equipment slot associated with this button.  Only items that match this slot can be equiped here.
    /// </summary>
    [SerializeField]
    private EquipmentSlot equipmentSlot;

    /// <summary>
    /// A reference to the equipment that sits on this slot
    /// </summary>
    private Equipment equippedEquipment;

    [SerializeField]
    private Image icon;

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            if (HandScript.MyInstance.MyMoveable is Equipment) {
                Equipment tmp = (Equipment)HandScript.MyInstance.MyMoveable;
                if (tmp.equipSlot == equipmentSlot) {
                    EquipEquipment(tmp);
                    UIManager.MyInstance.RefreshTooltip(tmp);
                }
            } else if (HandScript.MyInstance.MyMoveable == null && equippedEquipment != null) {
                HandScript.MyInstance.TakeMoveable(equippedEquipment);
                CharacterPanel.MyInstance.MySelectedButton = this;
                icon.color = Color.gray;
            }
        }
    }

    public void EquipEquipment(Equipment newEquipment, bool partialEquip = false) {
        if (partialEquip) {
            SetEquipment(newEquipment);
            return;
        }

        SlotScript oldSlot = newEquipment.MySlot;

        //remove from the inventory
        newEquipment.Remove();

        icon.enabled = true;
        icon.sprite = newEquipment.MyIcon;
        icon.color = Color.white;
        if (newEquipment != equippedEquipment) {
            EquipmentManager.MyInstance.Equip(newEquipment);
            this.equippedEquipment = newEquipment;
        }
        this.equippedEquipment.MyCharacterButton = this;
        //HandScript.MyInstance.DeleteItem();
        if (HandScript.MyInstance.MyMoveable == (newEquipment as IMoveable)) {
            Debug.Log("dropping moveable from handscript");
            HandScript.MyInstance.Drop();
        }

        if (equippedEquipment != null) {
            // not needed because the equipment manager currently handles the swapping
            // however, this code is actually slightly better because it would directly swap slots if the bag is full
            //newEquipment.MySlot.AddItem(equipment);
            if (oldSlot.MyItem == null) {
                UIManager.MyInstance.HideToolTip();
            } else {
                UIManager.MyInstance.RefreshTooltip(oldSlot.MyItem);
            }
        } else {
            UIManager.MyInstance.HideToolTip();
        }
    }

    public void SetEquipment(Equipment newEquipment) {
        icon.enabled = true;
        icon.sprite = newEquipment.MyIcon;
        icon.color = Color.white;
        this.equippedEquipment = newEquipment;
        this.equippedEquipment.MyCharacterButton = this;
    }



public void DequipEquipment() {
        icon.color = Color.white;
        icon.enabled = false;
        Debug.Log("attempting to unequip the item in slot " + equipmentSlot.ToString());
        equippedEquipment.MyCharacterButton = null;
        EquipmentManager.MyInstance.Unequip(equipmentSlot);
        equippedEquipment = null;
    }


    public void OnPointerEnter(PointerEventData eventData) {
        if (equippedEquipment != null) {
            UIManager.MyInstance.ShowToolTip(transform.position, equippedEquipment);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.MyInstance.HideToolTip();
    }

}
