using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CastBarController : DraggableWindow {

        [SerializeField]
        private GameObject castBackground = null;

        [SerializeField]
        private Image castSlider = null;

        [SerializeField]
        private TextMeshProUGUI castText = null;

        [SerializeField]
        private Image castIcon = null;

        private UnitNamePlateController unitNamePlateController = null;

        private float originalCastSliderWidth = 0f;

        //private Transform followTransform = null;

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

        public void SetTarget(UnitNamePlateController unitNamePlateController) {
            //Debug.Log(gameObject.name + ".CastBarController.SetTarget(" + target.name + ")");
            InitializeController();
            this.unitNamePlateController = unitNamePlateController;
            TargetInitialization();
        }

        public void ClearTarget() {
            //Debug.Log(gameObject.name + ".CastBarController.ClearTarget()");
            if (unitNamePlateController != null
                && unitNamePlateController.UnitController != null
                && unitNamePlateController.UnitController.BaseCharacter != null
                && unitNamePlateController.UnitController.BaseCharacter.CharacterAbilityManager != null) {
                unitNamePlateController.UnitController.BaseCharacter.CharacterAbilityManager.OnCastTimeChanged -= OnCastTimeChanged;
                unitNamePlateController.UnitController.BaseCharacter.CharacterAbilityManager.OnCastStop -= OnCastStop;
            }
            unitNamePlateController = null;
            targetInitialized = false;
            DisableCastBar();
        }

        private void InitializeCallbacks() {
            //Debug.Log(gameObject.name + ".CastBarController.InitializeCallbacks()");

            if (unitNamePlateController != null
                && unitNamePlateController.UnitController != null
                && unitNamePlateController.UnitController.BaseCharacter != null
                && unitNamePlateController.UnitController.BaseCharacter.CharacterAbilityManager != null) {
                unitNamePlateController.UnitController.BaseCharacter.CharacterAbilityManager.OnCastTimeChanged += OnCastTimeChanged;
                unitNamePlateController.UnitController.BaseCharacter.CharacterAbilityManager.OnCastStop += OnCastStop;
            }

        }

        void OnCastStop(BaseCharacter source) {
            //Debug.Log(gameObject.name + ".CastBarController.OnCastStop();");
            DisableCastBar();
        }

        public void OnCastTimeChanged(IAbilityCaster abilityCaster, IAbility ability, float currentPercent) {
            //Debug.Log(gameObject.name + ".CastBarController.OnCastTimeChanged(" + currentTime + ") : total casting time: " + ability.MyAbilityCastingTime);

            if (currentPercent <= 1f) {
                // first set text because bar width is based on text size
                castText.text = ability.DisplayName + " ( " + (currentPercent * ability.GetAbilityCastingTime(abilityCaster)).ToString("F1") + "s / " + ability.GetAbilityCastingTime(abilityCaster).ToString("F1") + "s )";

                // then get width of container that expands to the text
                originalCastSliderWidth = castBackground.GetComponent<RectTransform>().rect.width;
                //Debug.Log(gameObject.name + ".CastBarController.OnCastTimeChanged(): cast slider width: " + originalCastSliderWidth);

                this.gameObject.SetActive(true);
                //float castPercent = (float)currentPercent / ability.GetAbilityCastingTime(abilityCaster);

                // code for an actual image, not currently used
                //playerCastSlider.fillAmount = castPercent;

                // code for the default image
                castSlider.GetComponent<LayoutElement>().preferredWidth = currentPercent * originalCastSliderWidth;
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