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
            SystemEventManager.StartListening("OnXPGained", HandleXPGained);
            SystemGameManager.Instance.EventManager.OnLevelChanged += UpdateXPBar;
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            if (PlayerManager.Instance.PlayerUnitSpawned == true) {
                ProcessPlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log("XPBarController.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemGameManager.Instance.EventManager != null) {
                SystemEventManager.StopListening("OnXPGained", HandleXPGained);
                SystemGameManager.Instance.EventManager.OnLevelChanged -= UpdateXPBar;
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            }
            eventSubscriptionsInitialized = false;
        }

        public void HandleXPGained(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            UpdateXP();
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        private void OnDestroy() {
            // this gameobject will be enabled and disabled multiple times during the game and doesn't need to reset its references every time
            CleanupEventSubscriptions();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("XPBarController.HandlePlayerUnitSpawn()");
            if (originalXPSliderWidth == 0f) {
                originalXPSliderWidth = xpSlider.GetComponent<LayoutElement>().preferredWidth;
                //originalXPSliderWidth = xpBarBackGround.GetComponent<RectTransform>().rect.width;
                //Debug.Log("XPBarController.HandlePlayerUnitSpawn(): originalXPSliderWidth was 0, now: " + originalXPSliderWidth);
            }
            UpdateXPBar(PlayerManager.Instance.MyCharacter.CharacterStats.Level);
        }

        public void UpdateXP() {
            //Debug.Log("XPBarController.UpdateXP()");
            UpdateXPBar(PlayerManager.Instance.MyCharacter.CharacterStats.Level);
        }

        public void UpdateXPBar(int _Level) {
            if (!PlayerManager.Instance.PlayerUnitSpawned) {
                return;
            }
            //Debug.Log("XPBarController.UpdateXPBar(" + _Level + ")");
            int currentXP = PlayerManager.Instance.MyCharacter.CharacterStats.CurrentXP;
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
