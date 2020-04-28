using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionRewardButton : RewardButton {

        /// <summary>
        /// UPdates the visual representation of the describablebutton
        /// </summary>
        public override void UpdateVisual() {
            //Debug.Log("RewardButton.UpdateVisual()");
            base.UpdateVisual();
            stackSize.text = (describable as FactionNode).reputationAmount.ToString();
        }

    }

}