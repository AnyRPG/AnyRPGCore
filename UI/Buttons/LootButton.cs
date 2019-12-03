using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootButton : TransparencyButton, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text title;

        private LootUI lootWindow;

        public Image MyIcon { get => icon; }
        public Text MyTitle { get => title; }

        public Item MyLoot { get; set; }

        private void Awake() {
            lootWindow = GetComponentInParent<LootUI>();
        }

        public void OnPointerClick(PointerEventData eventData) {
            // loot the item
            TakeLoot();
        }

        public bool TakeLoot() {
            //Debug.Log("LootButton.TakeLoot()");
            bool result = InventoryManager.MyInstance.AddItem(MyLoot);
            if (result) {
                //Debug.Log("LootButton.TakeLoot(): added item to inventory");
                //Debug.Log("LootUI.TakeAllLoot(): Loot drop type is: " + MyLoot.GetType() + " and name is " + MyLoot.MyName);
                if ((MyLoot as CurrencyItem) is CurrencyItem) {
                    //Debug.Log("LootUI.TakeAllLoot(): item is currency: " + MyLoot.MyName);
                    (MyLoot as CurrencyItem).Use();
                } else if ((MyLoot as QuestStartItem) is QuestStartItem) {
                    //Debug.Log("LootUI.TakeAllLoot(): item is questStartItem: " + MyLoot.MyName);
                    (MyLoot as QuestStartItem).Use();
                } else {
                    //Debug.Log("LootUI.TakeAllLoot(): item is normal item");
                }

                gameObject.SetActive(false);
                lootWindow.TakeLoot(MyLoot);
                UIManager.MyInstance.HideToolTip();
                return true;
            }
            return false;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            UIManager.MyInstance.ShowToolTip(transform.position, MyLoot);
        }

        public void OnPointerExit(PointerEventData eventData) {
            UIManager.MyInstance.HideToolTip();
        }
    }

}