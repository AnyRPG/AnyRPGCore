using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace AnyRPG {
    public class MapTextLayerController : MiniMapIndicatorLayerController {

        [SerializeField]
        private TextMeshProUGUI text = null;

        public override void Setup(InteractableOptionComponent interactableOptionComponent) {
            base.Setup(interactableOptionComponent);

            ConfigureDisplay();
        }

        public override void ConfigureDisplay() {
            //Debug.Log($"{interactableOptionComponent.Interactable.DisplayName}.{interactableOptionComponent.GetType().Name}.MiniMapTextLayerController.ConfigureDisplay()  InstanceID: {GetInstanceID()}");

            interactableOptionComponent.SetMiniMapText(text);

            if (text.text == "") {
                isActive = false;
            } else {
                isActive = true;
            }
        }
    }

}