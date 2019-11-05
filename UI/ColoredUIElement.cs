using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ColoredUIElement : MonoBehaviour {

        //[SerializeField]
        protected Image coloredImage;

        // Start is called before the first frame update
        protected virtual void Start() {
            if (coloredImage == null) {
                coloredImage = GetComponent<Image>();
            }

            SetImageColor();
        }

        public virtual void SetImageColor() {
            if (SystemConfigurationManager.MyInstance != null && coloredImage != null) {
                coloredImage.color = SystemConfigurationManager.MyInstance.MyDefaultUIColor;
            }
        }

    }

}
