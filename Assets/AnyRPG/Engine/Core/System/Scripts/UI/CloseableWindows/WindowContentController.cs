using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class WindowContentController : MonoBehaviour, ICloseableWindowContents {

        public virtual event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private Image backGroundImage;

        public Image MyBackGroundImage { get => backGroundImage; set => backGroundImage = value; }

        protected RectTransform rectTransform;

        public virtual void Awake() {
            if (backGroundImage == null) {
                backGroundImage = GetComponent<Image>();
            }
            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void OnHoverSound() {
            AudioManager.MyInstance.PlayUIHoverSound();
        }

        public virtual void OnClickSound() {
            AudioManager.MyInstance.PlayUIClickSound();
        }

        public virtual void RecieveClosedWindowNotification() {
            OnCloseWindow(this);
        }

        public virtual void ReceiveOpenWindowNotification() {
            //Debug.Log("WindowContentController.OnOpenWindow()");
        }

        public void SetBackGroundColor(Color color) {
            //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor()");
            if (backGroundImage != null) {
                //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor(): background image is not null, setting color");
                backGroundImage.color = color;
            } else {
                //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor(): background image IS NULL!");
            }
        }
    }

}