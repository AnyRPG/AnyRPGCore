using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterButton : NavigableElement, IDescribable {

        [Header("Character Button")]

        /// <summary>
        /// The equipment slot associated with this button.  Only items that match this slot can be equiped here.
        /// </summary>
        [SerializeField]
        protected string equipmentSlotProfileName = string.Empty;

        protected EquipmentSlotProfile equipmentSlotProfile = null;

        /// <summary>
        /// A reference to the equipment that sits on this slot
        /// </summary>
        protected Equipment equippedEquipment = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected Image backGroundImage = null;

        protected Image emptySlotImage = null;

        protected Color emptyBackGroundColor;

        protected Color fullBackGroundColor;

        protected CharacterPanel characterPanel = null;

        // game manager references
        protected PlayerManager playerManager = null;
        protected UIManager uIManager = null;
        protected HandScript handScript = null;
        protected SystemDataFactory systemDataFactory = null;

        public Color EmptyBackGroundColor { get => emptyBackGroundColor; set => emptyBackGroundColor = value; }
        public Color FullBackGroundColor { get => fullBackGroundColor; set => fullBackGroundColor = value; }
        public Sprite Icon { get => icon.sprite; set => icon.sprite = value; }
        public Image EmptySlotImage { get => emptySlotImage; set => emptySlotImage = value; }

        public string DisplayName {
            get {
                if (equippedEquipment != null) {
                    return equippedEquipment.DisplayName;
                } else {
                    return "Empty Equipment Slot";
                }
            }
        }

        public string EquipmentSlotProfileName { get => equipmentSlotProfileName; set => equipmentSlotProfileName = value; }
        public EquipmentSlotProfile EquipmentSlotProfile { get => equipmentSlotProfile; }
        public CharacterPanel CharacterPanel { get => characterPanel; set => characterPanel = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            handScript = uIManager.HandScript;
            systemDataFactory = systemGameManager.SystemDataFactory;

            GetLocalComponents();
            GetSystemResourceReferences();

        }

        private void GetLocalComponents() {
            if (emptySlotImage == null) {
                emptySlotImage = GetComponent<Image>();
            }
        }

        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (handScript.Moveable is Equipment) {
                    Equipment tmp = (Equipment)handScript.Moveable;
                    if (equipmentSlotProfile.EquipmentSlotTypeList.Contains(tmp.EquipmentSlotType)) {
                        playerManager.MyCharacter.CharacterEquipmentManager.Unequip(equipmentSlotProfile);
                        //if (tmp.equipSlot == equipmentSlot) {

                        //tmp.Use();

                        // equip to this slot
                        playerManager.MyCharacter.CharacterEquipmentManager.Equip(tmp, equipmentSlotProfile);
                        // call remove
                        tmp.Remove();

                        //EquipEquipment(tmp);
                        handScript.Drop();

                        uIManager.RefreshTooltip(tmp);
                    }
                } else if (handScript.Moveable == null && equippedEquipment != null) {
                    handScript.TakeMoveable(equippedEquipment);
                    characterPanel.SelectedButton = this;
                    icon.color = Color.gray;
                }
            }
        }

        public void GetSystemResourceReferences() {
            if (equipmentSlotProfileName != null && equipmentSlotProfileName != string.Empty) {
                //Debug.Log("CharacterButton.GetLocalComponents(): equipmentslotprofileName is not empty");
                equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    //Debug.Log("CharacterButton.GetLocalComponents(): equipmentslotprofile is NULL!!!");
                }
            }
        }

        public void UpdateVisual(bool resetDisplay = true) {
            //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual()");

            Equipment tmpEquipment = equippedEquipment;
            if (equipmentSlotProfile != null && playerManager.MyCharacter != null && playerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): equipmentslotprofile was not null and player has quipment in this slot");
                equippedEquipment = playerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile];
            } else {
                /*
                if (equipmentSlotProfile == null) {
                    //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): equipmentslotprofile was null");
                }
                if (!playerManager.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                    //Debug.Log(gameObject.name + "CharacterButton.UpdateVisual(): player had no equipment in this slot: " + equipmentSlotProfile + "; " + equipmentSlotProfile.GetInstanceID() + "; equipmentCount: " + playerManager.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment.Count);
                }*/
                equippedEquipment = null;
            }

            if (equippedEquipment == null) {
                if (emptyBackGroundColor != null) {
                    backGroundImage.color = emptyBackGroundColor;
                }
                emptySlotImage.sprite = null;
                if (EquipmentSlotProfile != null && EquipmentSlotProfile.Icon != null) {
                    emptySlotImage.sprite = EquipmentSlotProfile.Icon;
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

                uIManager.SetItemBackground(equippedEquipment, emptySlotImage, fullBackGroundColor);
            }

            if (playerManager.PlayerUnitSpawned == false) {
                // prevent unnecessary actions when player is not spawned
                return;
            }
            if (uIManager.characterPanelWindow.IsOpen == false) {
                // prevent unnecessary actions when window is not open
                return;
            }
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);
            uIManager.ShowToolTip(transform.position, this);
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            uIManager.HideToolTip();
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

        public virtual void CheckMouse() {
            if (UIManager.MouseInRect(icon.rectTransform)) {
                uIManager.HideToolTip();
            }
        }

        public virtual void OnDisable() {
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CheckMouse();
        }

        private void ShowContextInfo() {
            if (equippedEquipment != null) {
                uIManager.ShowGamepadTooltip(characterPanel.RectTransform, transform, this, "Sell Price: ");
                owner.SetControllerHints("Unequip", "", "", "");
            } else {
                uIManager.ShowGamepadTooltip(characterPanel.RectTransform, transform, this, "");
                owner.HideControllerHints();
            }
        }

        public override void Select() {
            //Debug.Log("SlotScript.Select()");
            base.Select();

            ShowContextInfo();
        }

        public override void DeSelect() {
            //Debug.Log("SlotScript.DeSelect()");
            base.DeSelect();
            if (owner != null) {
                owner.HideControllerHints();
            }
            uIManager.HideToolTip();
        }

        public override void Accept() {
            base.Accept();
            if (equippedEquipment != null) {
                playerManager.MyCharacter.CharacterEquipmentManager.Unequip(equipmentSlotProfile);
                ShowContextInfo();
            }
        }



    }

}