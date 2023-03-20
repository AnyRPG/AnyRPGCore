using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class DescribableCraftingIcon : DescribableIcon {

        [Header("Crafting")]

        [SerializeField]
        protected Image backgroundImage = null;

        protected Item item = null;

        public void SetItem(Item item, int count) {
            this.item = item;
            SetDescribable(item, count);
        }

        public override void UpdateVisual() {
            //Debug.Log("DescribableCraftingOutputIcon.UpdateVisual()");
            base.UpdateVisual();

            SetBackGroundColor();
        }

        public void SetBackGroundColor() {
            uIManager.SetItemBackground(item, backgroundImage, new Color32(0, 0, 0, 255));

            //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor()");
        }

        public override void Select() {
            base.Select();

            uIManager.ShowGamepadTooltip(toolTipTransform, transform, describable, "");
        }

        public override void DeSelect() {
            base.DeSelect();

            uIManager.HideToolTip();
        }

    }

}