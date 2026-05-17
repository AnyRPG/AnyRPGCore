using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class NavigableToggle : NavigableElement {

        [Header("Toggle")]

        [SerializeField]
        private Toggle toggle;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            if (toggle != null) {
                ColorBlock colorBlock = toggle.colors;
                colorBlock.normalColor = systemConfigurationManager.UIConfiguration.ButtonNormalColor;
                colorBlock.highlightedColor = systemConfigurationManager.UIConfiguration.ButtonHighlightedColor;
                colorBlock.pressedColor = systemConfigurationManager.UIConfiguration.ButtonPressedColor;
                colorBlock.selectedColor = systemConfigurationManager.UIConfiguration.ButtonSelectedColor;
                colorBlock.disabledColor = systemConfigurationManager.UIConfiguration.ButtonDisabledColor;
                toggle.colors = colorBlock;
                toggle.graphic.color = systemConfigurationManager.UIConfiguration.HighlightButtonColor;
                toggle.targetGraphic.color = systemConfigurationManager.UIConfiguration.HighlightButtonColor;
            }
        }

        public override void Accept() {
            toggle.isOn = !toggle.isOn;
        }

    }

}