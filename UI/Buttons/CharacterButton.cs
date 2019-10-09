using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDescribable {

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

    [SerializeField]
    private Image backGroundImage;

    private Image emptySlotImage;

    private Color emptyBackGroundColor;

    private Color fullBackGroundColor;

    public Color MyEmptyBackGroundColor { get => emptyBackGroundColor; set => emptyBackGroundColor = value; }
    public Color MyFullBackGroundColor { get => fullBackGroundColor; set => fullBackGroundColor = value; }
    public string MyName {
        get {
            if (equippedEquipment != null) {
                return equippedEquipment.MyName;
            } else {
                return "Empty Equipment Slot";
            }
        }
    }

    public Sprite MyIcon { get => icon.sprite; set => icon.sprite = value; }

    private bool LocalComponentsGotten = false;

    private void Awake() {
        GetLocalComponents();
    }

    private void Start() {
        GetLocalComponents();
    }

    private void GetLocalComponents() {
        if (LocalComponentsGotten == true) {
            return;
        }
        if (emptySlotImage == null) {
            emptySlotImage = GetComponent<Image>();
        }

        LocalComponentsGotten = true;
    }

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
        //Debug.Log("CharacterButton.EquipEquipment(" + (newEquipment == null ? "null" : newEquipment.MyName) + ", " + partialEquip + ")");
        if (partialEquip) {
            SetEquipment(newEquipment);
            return;
        }

        SlotScript oldSlot = newEquipment.MySlot;

        //remove from the inventory
        newEquipment.Remove();

        if (newEquipment != equippedEquipment) {
            EquipmentManager.MyInstance.Equip(newEquipment);
            this.equippedEquipment = newEquipment;
        }
        this.equippedEquipment.MyCharacterButton = this;
        //HandScript.MyInstance.DeleteItem();
        if (HandScript.MyInstance.MyMoveable == (newEquipment as IMoveable)) {
            //Debug.Log("dropping moveable from handscript");
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
        UpdateVisual();
    }

    public void UpdateVisual(bool resetDisplay = true) {
        //Debug.Log("CharacterButton.UpdateVisual()");

        GetLocalComponents();

        if (equippedEquipment != null) {
            if (fullBackGroundColor != null) {
                backGroundImage.color = fullBackGroundColor;
            }
            emptySlotImage.color = new Color32(0, 0, 0, 0);
            icon.enabled = true;
            icon.color = Color.white;
            icon.sprite = equippedEquipment.MyIcon;
        } else {
            if (emptyBackGroundColor != null) {
                backGroundImage.color = emptyBackGroundColor;
            }
            emptySlotImage.color = Color.white;
            icon.color = new Color32(0, 0, 0, 0);
            icon.sprite = null;
            icon.enabled = false;
        }

        if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
            // prevent unnecessary actions when player is not spawned
            return;
        }
        if (PopupWindowManager.MyInstance.characterPanelWindow.IsOpen == false) {
            // prevent unnecessary actions when window is not open
            return;
        }
        if (resetDisplay) {
            CharacterPanel.MyInstance.ResetDisplay();
        }
    }

    public void SetEquipment(Equipment newEquipment) {
        //Debug.Log("CharacterButton.SetEquipment(" + (newEquipment == null ? "null" : newEquipment.MyName) + ")");
        this.equippedEquipment = newEquipment;
        this.equippedEquipment.MyCharacterButton = this;
        UpdateVisual();
    }

    public void DequipEquipment(int slotIndex = -1) {
        //Debug.Log("attempting to unequip the item in slot " + equipmentSlot.ToString());
        equippedEquipment.MyCharacterButton = null;
        EquipmentManager.MyInstance.Unequip(equipmentSlot, slotIndex);
        ClearButton();
    }

    public void ClearButton(bool resetDisplay = true) {
        //Debug.Log("CharacterButton.ClearButton()");
        equippedEquipment = null;
        UpdateVisual(resetDisplay);
    }


    public void OnPointerEnter(PointerEventData eventData) {
        UIManager.MyInstance.ShowToolTip(transform.position, this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.MyInstance.HideToolTip();
    }

    public string GetDescription() {
        if (equippedEquipment != null) {
            return equippedEquipment.GetDescription();
        }
        return string.Format("<color=cyan>Empty Equipment Slot</color>\n{0}", GetSummary());
    }

    public string GetSummary() {
        if (equippedEquipment != null) {
            return equippedEquipment.GetSummary();
        }
        return "Drag equipment here to equip it";
    }

}
