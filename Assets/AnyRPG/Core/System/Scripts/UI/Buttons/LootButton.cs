using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootButton : TransparencyButton {

        [Header("Loot")]

        [SerializeField]
        protected Image lootBackGroundImage;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI title = null;

        protected LootUI lootWindow = null;

        public TextMeshProUGUI MyTitle { get => title; }

        protected LootDrop lootDrop = null;

        public Image Icon { get => icon; }
        public LootDrop LootDrop {
            get => lootDrop;
            set {
                lootDrop = value;
                Icon.sprite = lootDrop.Icon;
                lootDrop.SetBackgroundImage(lootBackGroundImage);
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log(gameObject.name + ".LootButton.Configure(): " + GetInstanceID());
            base.Configure(systemGameManager);
        }

        public void SetLootUI(LootUI lootUI) {
            lootWindow = lootUI;
        }

        public bool TakeLoot() {
            //Debug.Log("LootButton.TakeLoot()");
            if (LootDrop == null) {
                return false;
            }

            bool result = LootDrop.TakeLoot();
            if (result) {
                LootDrop.AfterLoot();

                gameObject.SetActive(false);
                lootWindow.TakeLoot(LootDrop);
                uIManager.HideToolTip();
                return true;
            }
            return false;
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            //Debug.Log("LootButton.OnPointerEnter(): " + GetInstanceID());
            base.OnPointerEnter(eventData);
            if (LootDrop == null) {
                return;
            }

            uIManager.ShowToolTip(transform.position, LootDrop);
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            uIManager.HideToolTip();
        }

        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);
            if (LootDrop == null) {
                return;
            }

            // loot the item
            TakeLoot();
        }

    }

}