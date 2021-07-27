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

        protected bool eventSubscriptionsInitialized = false;

        public virtual void Init() {
            //Debug.Log(gameObject.name + ".WindowContentController.Init()");
            if (backGroundImage == null) {
                backGroundImage = GetComponent<Image>();
            }
            rectTransform = GetComponent<RectTransform>();
            CreateEventSubscriptions();
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
            AudioManager.Instance.PlayUIHoverSound();
        }

        public virtual void OnClickSound() {
            AudioManager.Instance.PlayUIClickSound();
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