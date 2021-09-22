using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootButton : TransparencyButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

        [Header("Loot")]

        [SerializeField]
        protected Image lootBackGroundImage;

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI title = null;

        private LootUI lootWindow = null;

        public TextMeshProUGUI MyTitle { get => title; }

        private LootDrop lootDrop = null;

        public Image MyIcon { get => icon; }
        public LootDrop LootDrop {
            get => lootDrop;
            set {
                lootDrop = value;
                MyIcon.sprite = lootDrop.Icon;
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

        public void OnPointerClick(PointerEventData eventData) {
            if (LootDrop == null) {
                return;
            }

            // loot the item
            TakeLoot();
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

        public void OnPointerEnter(PointerEventData eventData) {
            //Debug.Log("LootButton.OnPointerEnter(): " + GetInstanceID());
            if (LootDrop == null) {
                return;
            }

            uIManager.ShowToolTip(transform.position, LootDrop);
        }

        public void OnPointerExit(PointerEventData eventData) {
            uIManager.HideToolTip();
        }
    }

}