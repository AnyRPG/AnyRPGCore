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
        public Sprite Icon { get => icon.sprite; set => icon.sprite = value; }
        public Image MyEmptySlotImage { get => emptySlotImage; set => emptySlotImage = value; }

        public string DisplayName {
            get {
                if (equippedEquipment != null) {
                    return equippedEquipment.DisplayName;
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
                if (HandScript.Instance.MyMoveable is Equipment) {
                    Equipment tmp = (Equipment)HandScript.Instance.MyMoveable;
                    if (equipmentSlotProfile.MyEquipmentSlotTypeList.Contains(tmp.EquipmentSlotType)) {
                        PlayerManager.Instance.MyCharacter.CharacterEquipmentManager.Unequip(equipmentSlotProfile);
                        //if (tmp.equipSlot == equipmentSlot) {

                        //tmp.Use();

                        // equip to this slot
                        PlayerManager.Instance.MyCharacter.CharacterEquipmentManager.Equip(tmp, equipmentSlotProfile);
                        // call remove
                        tmp.Remove();

                        //EquipEquipment(tmp);
                        HandScript.Instance.Drop();

                        UIManager.Instance.RefreshTooltip(tmp);
                    }
                } else if (HandScript.Instance.MyMoveable == null && equippedEquipment != null) {
                    HandScript.Instance.TakeMoveable(equippedEquipment);
                    CharacterPanel.Instance.MySelectedButton = this;
                    icon.color = Color.gray;
                }
            }
        }

        public void GetSystemResourceReferences() {
            if (equipmentSlotProfileName != null && equipmentSlotProfileName != string.Empty) {
                //Debug.Log("CharacterButton.GetLocalComponents(): equipmentslotprofileName is not empty");
                equipmentSlotProfile = SystemEquipmentSlotProfileManager.Instance.GetResource(equipmentSlotProfileName);
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
            if (equipmentSlotProfile != null && PlayerManager.Instance.MyCharacter.CharacterEquipmentManager.CurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): equipmentslotprofile was not null and player has quipment in this slot");
                equippedEquipment = PlayerManager.Instance.MyCharacter.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile];
            } else {
                /*
                if (equipmentSlotProfile == null) {
                    //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): equipmentslotprofile was null");
                }
                if (!PlayerManager.Instance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                    //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): player had no equipment in this slot: " + equipmentSlotProfile + "; " + equipmentSlotProfile.GetInstanceID() + "; equipmentCount: " + PlayerManager.Instance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment.Count);
                }*/
                equippedEquipment = null;
            }

            if (equippedEquipment == null) {
                if (emptyBackGroundColor != null) {
                    backGroundImage.color = emptyBackGroundColor;
                }
                emptySlotImage.sprite = null;
                if (MyEquipmentSlotProfile != null && MyEquipmentSlotProfile.Icon != null) {
                    emptySlotImage.sprite = MyEquipmentSlotProfile.Icon;
                    emptySlotImage.color = Color.white;
                } else {
                    emptySlotImage.color = new Color32(0, 0, 0, 0);
                }
                icon.color = new Color32(0, 0, 0, 0);
                icon.sprite = null;
                icon.enabled = false;
            } else {
                //emptySlotImage.color = new Color32(0, 0, 0, 0);
                icon.enabled = true;
                icon.color = Color.white;
                icon.sprite = equippedEquipment.Icon;

                UIManager.Instance.SetItemBackground(equippedEquipment, emptySlotImage, fullBackGroundColor);
            }

            if (PlayerManager.Instance.PlayerUnitSpawned == false) {
                // prevent unnecessary actions when player is not spawned
                return;
            }
            if (PopupWindowManager.Instance.characterPanelWindow.IsOpen == false) {
                // prevent unnecessary actions when window is not open
                return;
            }
            /*
            if (resetDisplay) {
                CharacterPanel.Instance.ResetDisplay();
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
            if (PlayerManager.Instance != null && PlayerManager.Instance.MyCharacter != null && PlayerManager.Instance.MyCharacter.MyCharacterEquipmentManager != null) {
                PlayerManager.Instance.MyCharacter.MyCharacterEquipmentManager.Unequip(equipmentSlot, slotIndex);
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
            UIManager.Instance.ShowToolTip(transform.position, this);
        }

        public void OnPointerExit(PointerEventData eventData) {
            UIManager.Instance.HideToolTip();
        }

        public string GetDescription() {
            if (equippedEquipment != null) {
                return equippedEquipment.GetDescription();
            }
            // cyan
            return string.Format("<color=#00FFFF>Empty Equipment Slot</color>\n{0}\n{1}", (equipmentSlotProfile?.DisplayName == null ? "" : equipmentSlotProfile?.DisplayName), GetSummary());
        }

        public string GetSummary() {
            if (equippedEquipment != null) {
                return equippedEquipment.GetSummary();
            }
            return "Drag equipment here to equip it";
        }

    }

}