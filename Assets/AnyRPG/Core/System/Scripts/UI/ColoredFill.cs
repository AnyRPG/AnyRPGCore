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

