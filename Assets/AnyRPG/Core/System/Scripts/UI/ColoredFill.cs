using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ColoredFill : ColoredUIElement {

        public override void SetImageColor() {
            // intentionally hide base
            if (systemConfigurationManager != null && coloredImage != null) {
                coloredImage.color = systemConfigurationManager.UIConfiguration.DefaultUIFillColor;
            }
        }
    }
}

