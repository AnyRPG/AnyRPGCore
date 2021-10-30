using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class TransparencyButton : NavigableElement {

        [SerializeField]
        protected Image backGroundImage;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            GetComponentReferences();
            SetBackGroundTransparency();
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("TransparencyButton.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPagedButtonsTransparencyUpdate", HandlePagedButtonsTransparencyUpdate);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("TransparencyButton.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnPagedButtonsTransparencyUpdate", HandlePagedButtonsTransparencyUpdate);
            eventSubscriptionsInitialized = false;
        }

        public void HandlePagedButtonsTransparencyUpdate(string eventName, EventParamProperties eventParamProperties) {
            SetBackGroundTransparency();
        }

        public void OnDestroy() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        protected virtual void GetComponentReferences() {
            //Debug.Log("TransparencyButton.GetComponentReferences()");
            if (backGroundImage == null) {
                backGroundImage = GetComponent<Image>();
                //Debug.Log("TransparencyButton.GetComponentReferences()");
            } else {
                //Debug.Log("TransparencyButton.GetComponentReferences(): bg image set");
            }
        }

        public void SetBackGroundTransparency() {
            //Debug.Log("TransparencyButton.SetBackGroundTransparency()");
            int opacityLevel = (int)(PlayerPrefs.GetFloat("PagedButtonsOpacity") * 255f);
            //Debug.Log("TransparencyButton.GetComponentReferences(): got opacity: " + opacityLevel);
            backGroundImage.color = new Color32(0, 0, 0, (byte)opacityLevel);
        }

        public virtual void CheckMouse() {
            if (UIManager.MouseInRect(transform as RectTransform)) {
                uIManager.HideToolTip();
            }
        }
    }

}