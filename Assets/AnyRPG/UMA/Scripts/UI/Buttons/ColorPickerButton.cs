using System.Collections;
using UMA.CharacterSystem;
using UMA;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class ColorPickerButton : HighlightButton {

        [Header("Color Picker")]

        [SerializeField]
        private Image colorSwatch = null;

        DynamicCharacterAvatar Avatar;
        string ColorName;
        OverlayColorData ColorValue;
        bool IsRemover;

        public void Setup(DynamicCharacterAvatar avatar, string colorName, OverlayColorData colorValue, Color color) {
            IsRemover = false;
            Avatar = avatar;
            ColorName = colorName;
            ColorValue = colorValue;
            colorSwatch.color = color;
        }

        public void SetupRemover(DynamicCharacterAvatar avatar, string colorName, Color color) {
            IsRemover = true;
            Avatar = avatar;
            ColorName = colorName;
            ColorValue = new OverlayColorData(1);
            colorSwatch.color = color;
        }

        public void OnClick() {
            if (IsRemover) {
                Avatar.ClearColor(ColorName);
            } else {
                Avatar.SetColor(ColorName, ColorValue);
            }
        }
    }
}
