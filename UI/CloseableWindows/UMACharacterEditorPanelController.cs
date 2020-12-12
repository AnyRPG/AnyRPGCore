using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.Examples;
using UMA.CharacterSystem;
using UMA.CharacterSystem.Examples;

namespace AnyRPG {

    public class UMACharacterEditorPanelController : CharacterAppearancePanel {

        #region Singleton
        private static UMACharacterEditorPanelController instance;

        public static UMACharacterEditorPanelController MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<UMACharacterEditorPanelController>();
                }

                return instance;
            }
        }

        #endregion

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        protected CanvasGroup canvasGroup = null;

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public void HidePanel() {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public void ShowPanel() {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public void HandleTargetReady() {
            //Debug.Log("NewGameCharacterPanelController.TargetReadyCallback()");

            CloseOptionsAreas();
            OpenAppearanceOptionsArea();
            InitializeSexButtons();
        }


    }

}