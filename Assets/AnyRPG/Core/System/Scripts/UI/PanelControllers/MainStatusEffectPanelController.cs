using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MainStatusEffectPanelController : StatusEffectPanelController {

        protected StatusEffectWindowPanel statusEffectWindowPanel = null;

        public void SetStatusEffectWindowPanel(StatusEffectWindowPanel statusEffectWindowPanel) {
            this.statusEffectWindowPanel = statusEffectWindowPanel;
        }

        public override StatusEffectNodeScript ClearStatusEffectNode(StatusEffectNode statusEffectNode) {
            //Debug.Log($"{gameObject.name}.MainStatusEffectPanelController.ClearStatusEffectNode()");
            StatusEffectNodeScript returnValue = base.ClearStatusEffectNode(statusEffectNode);

            if (returnValue != null && statusEffectWindowPanel != null) {
                statusEffectWindowPanel.RemoveStatusNode(returnValue);
            }

            return returnValue;
        }


        public override StatusEffectNodeScript SpawnStatusNode(StatusEffectNode statusEffectNode) {
            //Debug.Log($"{gameObject.name}.MainStatusEffectPanelController.SpawnStatusNode()");

            StatusEffectNodeScript returnValue = base.SpawnStatusNode(statusEffectNode);

            if (returnValue != null && statusEffectWindowPanel != null) {
                statusEffectWindowPanel.AddStatusNode(returnValue);
            }

            return returnValue;
        }
      
    }

}