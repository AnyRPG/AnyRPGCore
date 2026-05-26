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
        protected PlayerManagerClient playerManagerClient = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        protected override void ProcessCreateEventSubscriptions() {
            base.ProcessCreateEventSubscriptions();
            SystemEventManager.StartListening("OnXPGained", HandleXPGained);
            systemEventManager.OnLevelChanged += UpdateXPBar;
            systemEventManager.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            if (playerManagerClient.PlayerUnitSpawned == true) {
                ProcessPlayerUnitSpawn();
            }
        }

        protected override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("XPBarController.CleanupEventSubscriptions()");
            SystemEventManager.StopListening("OnXPGained", HandleXPGained);
            systemEventManager.OnLevelChanged -= UpdateXPBar;
            systemEventManager.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
        }

        public void HandleXPGained(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log($"{gameObject.name}.InanimateUnit.HandlePlayerUnitSpawn()");
            UpdateXP();
        }

        public void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
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
            UpdateXPBar(playerManagerClient.UnitController, playerManagerClient.UnitController.CharacterStats.Level);
        }

        public void UpdateXP() {
            //Debug.Log("XPBarController.UpdateXP()");
            UpdateXPBar(playerManagerClient.UnitController, playerManagerClient.UnitController.CharacterStats.Level);
        }

        public void UpdateXPBar(UnitController sourceUnitController, int _Level) {
            if (!playerManagerClient.PlayerUnitSpawned) {
                return;
            }
            //Debug.Log("XPBarController.UpdateXPBar(" + _Level + ")");
            int currentXP = playerManagerClient.UnitController.CharacterStats.CurrentXP;
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
