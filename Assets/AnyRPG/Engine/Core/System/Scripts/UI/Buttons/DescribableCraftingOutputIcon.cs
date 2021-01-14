using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class DescribableCraftingOutputIcon : DescribableIcon {
        public override void UpdateVisual() {
            //Debug.Log("DescribableCraftingOutputIcon.UpdateVisual()");
            base.UpdateVisual();

            if (count > 1) {
                stackSize.text = count.ToString();
            } else {
                stackSize.text = "";
            }

        }

    }

}