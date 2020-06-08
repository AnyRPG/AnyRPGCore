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
        private string equipmentSlotProfileName = string.Empty;

        private EquipmentSlotProfile equipmentSlotProfile = null;

        /// <summary>
        /// A reference to the equipment that sits on this slot
        /// </summary>
        private Equipment equippedEquipment = null;

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private Image backGroundImage = null;

        private Image emptySlotImage = null;

        private Color emptyBackGroundColor;

        private Color fullBackGroundColor;

        private bool LocalComponentsGotten = false;

        public Color MyEmptyBackGroundColor { get => emptyBackGroundColor; set => emptyBackGroundColor = value; }
        public Color MyFullBackGroundColor { get => fullBackGroundColor; set => fullBackGroundColor = value; }
        public Sprite MyIcon { get => icon.sprite; set => icon.sprite = value; }
        public Image MyEmptySlotImage { get => emptySlotImage; set => emptySlotImage = value; }

        public string MyDisplayName {
            get {
                if (equippedEquipment != null) {
                    return equippedEquipment.MyDisplayName;
                } else {
                    return "Empty Equipment Slot";
                }
            }
        }

        public string MyEquipmentSlotProfileName { get => equipmentSlotProfileName; set => equipmentSlotProfileName = value; }
        public EquipmentSlotProfile MyEquipmentSlotProfile { get => equipmentSlotProfile; }

        private void Awake() {
            GetLocalComponents();
            GetSystemResourceReferences();
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
                    if (equipmentSlotProfile.MyEquipmentSlotTypeList.Contains(tmp.MyEquipmentSlotType)) {
                        PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.Unequip(equipmentSlotProfile);
                        //if (tmp.equipSlot == equipmentSlot) {

                        //tmp.Use();

                        // equip to this slot
                        PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.Equip(tmp, equipmentSlotProfile);
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

        public void GetSystemResourceReferences() {
            if (equipmentSlotProfileName != null && equipmentSlotProfileName != string.Empty) {
                //Debug.Log("CharacterButton.GetLocalComponents(): equipmentslotprofileName is not empty");
                equipmentSlotProfile = SystemEquipmentSlotProfileManager.MyInstance.GetResource(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    //Debug.Log("CharacterButton.GetLocalComponents(): equipmentslotprofile is NULL!!!");
                }
            }
        }

        public void UpdateVisual(bool resetDisplay = true) {
            //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual()");

            GetLocalComponents();
            GetSystemResourceReferences();
            Equipment tmpEquipment = equippedEquipment;
            if (equipmentSlotProfile != null && PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.CurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): equipmentslotprofile was not null and player has quipment in this slot");
                equippedEquipment = PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile];
            } else {
                /*
                if (equipmentSlotProfile == null) {
                    //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): equipmentslotprofile was null");
                }
                if (!PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                    //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): player had no equipment in this slot: " + equipmentSlotProfile + "; " + equipmentSlotProfile.GetInstanceID() + "; equipmentCount: " + PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment.Count);
                }*/
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
            // cyan
            return string.Format("<color=#00FFFF>Empty Equipment Slot</color>\n{0}\n{1}", equipmentSlotProfile.MyDisplayName, GetSummary());
        }

        public string GetSummary() {
            if (equippedEquipment != null) {
                return equippedEquipment.GetSummary();
            }
            return "Drag equipment here to equip it";
        }

    }

}