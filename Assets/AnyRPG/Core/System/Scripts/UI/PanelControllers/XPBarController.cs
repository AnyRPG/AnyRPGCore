using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class XPBarController : NavigableInterfaceElement {

        [Header("XP Bar")]

        [SerializeField]
        protected Image xpSlider = null;

        //[SerializeField]
        //private GameObject xpBarBackGround = null;

        [SerializeField]
        protected TextMeshProUGUI xpText = null;

        protected float originalXPSliderWidth = 0f;

        // game manager references
        protected SystemEventManager systemEventManager = null;
        protected PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
        }

        protected override void ProcessCreateEventSubscriptions() {
            base.ProcessCreateEventSubscriptions();
            SystemEventManager.StartListening("OnXPGained", HandleXPGained);
            systemEventManager.OnLevelChanged += UpdateXPBar;
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            if (playerManager.PlayerUnitSpawned == true) {
                ProcessPlayerUnitSpawn();
            }
        }

        protected override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("XPBarController.CleanupEventSubscriptions()");
            SystemEventManager.StopListening("OnXPGained", HandleXPGained);
            systemEventManager.OnLevelChanged -= UpdateXPBar;
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
        }

        public void HandleXPGained(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log($"{gameObject.name}.InanimateUnit.HandlePlayerUnitSpawn()");
            UpdateXP();
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log($"{gameObject.name}.InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("XPBarController.HandlePlayerUnitSpawn()");
            if (originalXPSliderWidth == 0f) {
                originalXPSliderWidth = xpSlider.GetComponent<LayoutElement>().preferredWidth;
                //originalXPSliderWidth = xpBarBackGround.GetComponent<RectTransform>().rect.width;
                //Debug.Log("XPBarController.HandlePlayerUnitSpawn(): originalXPSliderWidth was 0, now: " + originalXPSliderWidth);
            }
            UpdateXPBar(playerManager.MyCharacter.CharacterStats.Level);
        }

        public void UpdateXP() {
            //Debug.Log("XPBarController.UpdateXP()");
            UpdateXPBar(playerManager.MyCharacter.CharacterStats.Level);
        }

        public void UpdateXPBar(int _Level) {
            if (!playerManager.PlayerUnitSpawned) {
                return;
            }
            //Debug.Log("XPBarController.UpdateXPBar(" + _Level + ")");
            int currentXP = playerManager.MyCharacter.CharacterStats.CurrentXP;
            int neededXP = LevelEquations.GetXPNeededForLevel(_Level, systemConfigurationManager);
            float xpPercent = (float)currentXP / (float)neededXP;

            // code for an actual image, not currently used
            //playerCastSlider.fillAmount = castPercent;

            // code for the default image
            xpSlider.GetComponent<LayoutElement>().preferredWidth = xpPercent * originalXPSliderWidth;

            xpText.text = currentXP + " / " + neededXP + " (" + ((int)(xpPercent * 100)).ToString() + "%)";

        }

    }

}
