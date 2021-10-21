using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class WindowContentController : ConfiguredMonoBehaviour, ICloseableWindowContents {

        public virtual event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private Image backGroundImage;

        [SerializeField]
        private List<ColoredUIElement> coloredUIElements = new List<ColoredUIElement>();

        public Image BackGroundImage { get => backGroundImage; set => backGroundImage = value; }

        protected RectTransform rectTransform;

        protected CloseableWindow closeableWindow = null;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected AudioManager audioManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log(gameObject.name + ".WindowContentController.Init()");
            base.Configure(systemGameManager);
            if (backGroundImage == null) {
                backGroundImage = GetComponent<Image>();
            }
            rectTransform = GetComponent<RectTransform>();
            foreach (ColoredUIElement coloredUIElement in coloredUIElements) {
                coloredUIElement.Configure(systemGameManager);
            }
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            audioManager = systemGameManager.AudioManager;
        }

        public virtual void SetWindow(CloseableWindow closeableWindow) {
            this.closeableWindow = closeableWindow;
        }

        public virtual void Close() {
            if (closeableWindow != null) {
                closeableWindow.CloseWindow();
            }
        }

        protected virtual void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        protected virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
        }

        protected virtual void OnDestroy() {
            //Debug.Log("WindowContentController.OnDestroy()");
            CleanupEventSubscriptions();
        }

        public virtual void OnHoverSound() {
            audioManager.PlayUIHoverSound();
        }

        public virtual void OnClickSound() {
            audioManager.PlayUIClickSound();
        }

        public virtual void RecieveClosedWindowNotification() {
            OnCloseWindow(this);
        }

        public virtual void ReceiveOpenWindowNotification() {
            //Debug.Log(gameObject.name + "WindowContentController.ReceiveOpenWindowNotification()");
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