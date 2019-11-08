using AnyRPG;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CastBarController : DraggableWindow {

        [SerializeField]
        private GameObject castBackground;

        [SerializeField]
        private Image castSlider;

        [SerializeField]
        private Text castText;

        [SerializeField]
        private Image castIcon;

        [SerializeField]
        private GameObject followGameObject;

        private float originalCastSliderWidth;

        private Transform followTransform;

        private bool controllerInitialized = false;
        private bool targetInitialized = false;

        void Start() {
            InitializeController();
            if (!targetInitialized) {
                DisableCastBar();
            }
        }

        public void DisableCastBar() {
            if (uiLocked == false && neverDraggable != true) {
                //Debug.Log(gameObject.name + ".CastBarController.InitializeController(): ui is unlocked and neverdraggable is not set to true.  returning to avoid deactivating cast bar");
                return;
            }
            this.gameObject.SetActive(false);
        }

        public void InitializeController() {
            //Debug.Log(gameObject.name + ".CastBarController.InitializeController()");
            if (controllerInitialized) {
                return;
            }
            // try cast timer background?
            //originalCastSliderWidth = castBackground.GetComponent<LayoutElement>().preferredWidth;
            originalCastSliderWidth = castBackground.GetComponent<RectTransform>().rect.width;
            //originalCastSliderWidth = castSlider.GetComponent<LayoutElement>().preferredWidth;
            controllerInitialized = true;
            DisableCastBar();
        }

        private void TargetInitialization() {
            //Debug.Log(gameObject.name + ".CastBarController.TargetInitialization()");
            InitializeCallbacks();
            //this.gameObject.SetActive(true);
        }

        public void SetTarget(GameObject target) {
            //Debug.Log(gameObject.name + ".CastBarController.SetTarget(" + target.name + ")");
            InitializeController();
            followGameObject = target;
            TargetInitialization();
        }

        public void ClearTarget() {
            //Debug.Log(gameObject.name + ".CastBarController.ClearTarget()");
            if (followGameObject != null) {
                CharacterAbilityManager _characterAbilityManager = followGameObject.GetComponent<CharacterAbilityManager>();
                if (_characterAbilityManager != null) {
                    _characterAbilityManager.OnCastTimeChanged -= OnCastTimeChanged;
                    _characterAbilityManager.OnCastStop -= OnCastStop;
                } else {
                    //Debug.Log(gameObject.name + ".CastBarController.ClearTarget(): characterAbilityManager is null");
                }
            } else {
                //Debug.Log(gameObject.name + ".CastBarController.ClearTarget(): followgameobject was null");
            }
            followGameObject = null;
            targetInitialized = false;
            DisableCastBar();
        }

        private void InitializeCallbacks() {
            //Debug.Log(gameObject.name + ".CastBarController.InitializeCallbacks()");

            BaseCharacter baseCharacter = followGameObject.GetComponent<CharacterUnit>().MyCharacter;
            if (baseCharacter.MyCharacterStats == null) {
                //Debug.Log("CastBarController: baseCharacter does not have CharacterStats");
                return;
            }
            //Debug.Log("Charcter name is " + baseCharacter.MyCharacterName);

            if (baseCharacter.MyCharacterAbilityManager == null) {
                // selected a vendor or questgiver that we don't want to be attackable
                //Debug.Log("CastBarController: baseCharacter does not have CharacterAbilityManager");
            } else {
                baseCharacter.MyCharacterAbilityManager.OnCastTimeChanged += OnCastTimeChanged;
                baseCharacter.MyCharacterAbilityManager.OnCastStop += OnCastStop;
            }

        }

        void OnCastStop(BaseCharacter source) {
            //Debug.Log(gameObject.name + ".CastBarController.OnCastStop();");
            DisableCastBar();
        }

        public void OnCastTimeChanged(IAbility ability, float currentTime) {
            //Debug.Log(gameObject.name + ".CastBarController.OnCastTimeChanged(" + currentTime + ") : total casting time: " + ability.MyAbilityCastingTime);

            if (currentTime <= ability.MyAbilityCastingTime) {
                // first set text because bar width is based on text size
                castText.text = ability.MyName + " ( " + currentTime.ToString("F1") + "s / " + ability.MyAbilityCastingTime.ToString("F1") + "s )";

                // then get width of container that expands to the text
                originalCastSliderWidth = castBackground.GetComponent<RectTransform>().rect.width;
                //Debug.Log(gameObject.name + ".CastBarController.OnCastTimeChanged(): cast slider width: " + originalCastSliderWidth);

                this.gameObject.SetActive(true);
                float castPercent = (float)currentTime / ability.MyAbilityCastingTime;

                // code for an actual image, not currently used
                //playerCastSlider.fillAmount = castPercent;

                // code for the default image
                castSlider.GetComponent<LayoutElement>().preferredWidth = castPercent * originalCastSliderWidth;
                if (castIcon.sprite != ability.MyIcon) {
                    castIcon.sprite = null;
                    castIcon.sprite = ability.MyIcon;
                }
            }
        }

        public override void OnDisable() {
            base.OnDisable();
            //Debug.Log(gameObject.name + ".CastBarController.OnDisable()");
        }

    }

}