using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class XPBarController : DraggableWindow {

        [SerializeField]
        private Image xpSlider = null;

        //[SerializeField]
        //private GameObject xpBarBackGround = null;

        [SerializeField]
        private TextMeshProUGUI xpText = null;

        private float originalXPSliderWidth = 0f;

        protected bool eventSubscriptionsInitialized = false;

        private void Start() {
            //Debug.Log("XPBarController.Start()");
            CreateEventSubscriptions();
        }

        public void CreateEventSubscriptions() {
            //Debug.Log("XPBarController.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnXPGained += UpdateXP;
            SystemEventManager.MyInstance.OnLevelChanged += UpdateXPBar;
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
                HandlePlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log("XPBarController.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnXPGained -= UpdateXP;
                SystemEventManager.MyInstance.OnLevelChanged -= UpdateXPBar;
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            }
            eventSubscriptionsInitialized = false;
        }

        private void OnDestroy() {
            // this gameobject will be enabled and disabled multiple times during the game and doesn't need to reset its references every time
            CleanupEventSubscriptions();
        }

        public void HandlePlayerUnitSpawn() {
            //Debug.Log("XPBarController.HandlePlayerUnitSpawn()");
            if (originalXPSliderWidth == 0f) {
                originalXPSliderWidth = xpSlider.GetComponent<LayoutElement>().preferredWidth;
                //originalXPSliderWidth = xpBarBackGround.GetComponent<RectTransform>().rect.width;
                //Debug.Log("XPBarController.HandlePlayerUnitSpawn(): originalXPSliderWidth was 0, now: " + originalXPSliderWidth);
            }
            UpdateXPBar(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel);
        }

        public void UpdateXP() {
            //Debug.Log("XPBarController.UpdateXP()");
            UpdateXPBar(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel);
        }

        public void UpdateXPBar(int _Level) {
            if (!PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                return;
            }
            //Debug.Log("XPBarController.UpdateXPBar(" + _Level + ")");
            int currentXP = PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyCurrentXP;
            int neededXP = LevelEquations.GetXPNeededForLevel(_Level);
            float xpPercent = (float)currentXP / (float)neededXP;

            // code for an actual image, not currently used
            //playerCastSlider.fillAmount = castPercent;

            // code for the default image
            xpSlider.GetComponent<LayoutElement>().preferredWidth = xpPercent * originalXPSliderWidth;

            xpText.text = currentXP + " / " + neededXP + " (" + ((int)(xpPercent * 100)).ToString() + "%)";

        }

    }

}
