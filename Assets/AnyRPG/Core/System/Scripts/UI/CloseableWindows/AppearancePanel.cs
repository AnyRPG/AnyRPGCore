using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMA.CharacterSystem;

namespace AnyRPG {

    public class AppearancePanel : WindowContentController {

        //public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        protected CanvasGroup canvasGroup = null;

        /*
        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("AppearancePanel.OnCloseWindow()");

            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }
        */

        /*
        public override void ProcessOpenWindowNotification() {
            //Debug.Log("AppearancePanel.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            //uINavigationControllers[0].FocusCurrentButton();
        }
        */

        public virtual void HidePanel() {
            //Debug.Log("AppearancePanel.HidePanel()");
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public virtual void ShowPanel() {
            //Debug.Log("AppearancePanel.ShowPanel()");
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            SetupOptions();
        }

        public virtual void SetupOptions() {
            //Debug.Log("AppearancePanel.SetupOptions()");
        }


    }

}