using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace AnyRPG {
    public class MiniMapImageLayerController : MiniMapIndicatorLayerController {

        [SerializeField]
        private Image image = null;

        public override void Setup(InteractableOptionComponent interactableOptionComponent) {
            base.Setup(interactableOptionComponent);

            ConfigureDisplay();
        }

        public override void ConfigureDisplay() {
            interactableOptionComponent.SetMiniMapIcon(image);

            if (image.sprite == null) {
                isActive = false;
            } else {
                isActive = true;
            }
        }
    }

}