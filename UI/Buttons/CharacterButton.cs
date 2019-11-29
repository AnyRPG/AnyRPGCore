using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDescribable {

        /// <summary>
        /// The equipment slot associated with this button.  Only items that match this slot can be equiped here.
        /// </summary>
        [SerializeField]
        private string equipmentSlotProfileName;

        private EquipmentSlotProfile equipmentSlotProfile = null;

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

        private bool LocalComponentsGotten = false;

        public Color MyEmptyBackGroundColor { get => emptyBackGroundColor; set => emptyBackGroundColor = value; }
        public Color MyFullBackGroundColor { get => fullBackGroundColor; set => fullBackGroundColor = value; }
        public Sprite MyIcon { get => icon.sprite; set => icon.sprite = value; }
        public Image MyEmptySlotImage { get => emptySlotImage; set => emptySlotImage = value; }

        public string MyName {
            get {
                if (equippedEquipment != null) {
                    return equippedEquipment.MyName;
                } else {
                    return "Empty Equipment Slot";
                }
            }
        }

        public string MyEquipmentSlotProfileName { get => equipmentSlotProfileName; set => equipmentSlotProfileName = value; }
        public EquipmentSlotProfile MyEquipmentSlotProfile { get => equipmentSlotProfile; set => equipmentSlotProfile = value; }

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
            //Debug.Log("CharacterButton.GetLocalComponents()");
            if (equipmentSlotProfileName != null && equipmentSlotProfileName != string.Empty) {
                //Debug.Log("CharacterButton.GetLocalComponents(): equipmentslotprofileName is not empty");
                equipmentSlotProfile = SystemEquipmentSlotProfileManager.MyInstance.GetResource(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    //Debug.Log("CharacterButton.GetLocalComponents(): equipmentslotprofile is NULL!!!");
                }
            }
            LocalComponentsGotten = true;
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (HandScript.MyInstance.MyMoveable is Equipment) {
                    Equipment tmp = (Equipment)HandScript.MyInstance.MyMoveable;
                    if (equipmentSlotProfile.MyEquipmentSlotTypeList.Contains(tmp.MyEquipmentSlotType)) {
                        PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.Unequip(equipmentSlotProfile);
                        //if (tmp.equipSlot == equipmentSlot) {

                        //tmp.Use();

                        // equip to this slot
                        PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.Equip(tmp, equipmentSlotProfile);
                        // call remove
                        tmp.Remove();

                        //EquipEquipment(tmp);
                        HandScript.MyInstance.Drop();

                        UIManager.MyInstance.RefreshTooltip(tmp);
                    }
                } else if (HandScript.MyInstance.MyMoveable == null && equippedEquipment != null) {
                    HandScript.MyInstance.TakeMoveable(equippedEquipment);
                    CharacterPanel.MyInstance.MySelectedButton = this;
                    icon.color = Color.gray;
                }
            }
        }

        /*
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
                if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager != null) {
                    PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.Equip(newEquipment);
                    this.equippedEquipment = newEquipment;
                }
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
        */

        public void UpdateVisual(bool resetDisplay = true) {
            Debug.Log(gameObject.name + "CharacterButton.UpdateVisual()");

            GetLocalComponents();
            Equipment tmpEquipment = equippedEquipment;
            if (equipmentSlotProfile != null && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): equipmentslotprofile was not null and player has quipment in this slot");
                equippedEquipment = PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment[equipmentSlotProfile];
            } else {
                if (equipmentSlotProfile == null) {
                    Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): equipmentslotprofile was null");
                }
                if (!PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                    Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): player had no equipment in this slot");
                }
                equippedEquipment = null;
            }

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
            /*
            if (resetDisplay) {
                CharacterPanel.MyInstance.ResetDisplay();
            }
            */
        }

        /*
        public void SetEquipment(Equipment newEquipment) {
            //Debug.Log("CharacterButton.SetEquipment(" + (newEquipment == null ? "null" : newEquipment.MyName) + ")");
            this.equippedEquipment = newEquipment;
            this.equippedEquipment.MyCharacterButton = this;
            UpdateVisual();
        }
        */
        /*
        public void DequipEquipment(int slotIndex = -1) {
            //Debug.Log("attempting to unequip the item in slot " + equipmentSlot.ToString());
            equippedEquipment.MyCharacterButton = null;
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager != null) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.Unequip(equipmentSlot, slotIndex);
            }
            ClearButton();
        }

        public void ClearButton(bool resetDisplay = true) {
            //Debug.Log("CharacterButton.ClearButton()");
            equippedEquipment = null;
            UpdateVisual(resetDisplay);
        }
        */


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
            return string.Format("<color=cyan>Empty Equipment Slot</color>\n{0}\n{1}", equipmentSlotProfile.MyName, GetSummary());
        }

        public string GetSummary() {
            if (equippedEquipment != null) {
                return equippedEquipment.GetSummary();
            }
            return "Drag equipment here to equip it";
        }

    }

}