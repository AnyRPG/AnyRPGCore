using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AnyRPG {
    public class ComponentController : ConfiguredMonoBehaviour {

        [Tooltip("Drag an object in the heirarchy here and the nameplate will show at its transform location")]
        [SerializeField]
        protected Transform namePlateTransform = null;

        [SerializeField]
        protected InteractableRange interactableRange = null;

        [Tooltip("A reference to the unit audio emitter.  This is optional as it will be found automatically if left blank.")]
        [SerializeField]
        private UnitAudioEmitter unitAudioEmitter = null;

        [Tooltip("A reference to the highlight circle")]
        [SerializeField]
        private HighlightController highlightController = null;

        [SerializeField]
        private AggroRange aggroRangeController = null;

        private Interactable interactable = null;
        private UnitController unitController = null;

        private Vector3 initialNamePlatePosition = Vector3.zero;
        private bool gotInitialNamePlatePosition = false;
        private Vector3 nameplateVector = Vector3.zero;

        // game manager references
        protected NetworkManagerServer networkManagerServer = null;
        protected LevelManagerClient levelManagerClient = null;

        public Vector3 NameplateVector { get => nameplateVector; }

        public override void Configure(SystemGameManager systemGameManager) {

            base.Configure(systemGameManager);
            interactableRange.Configure(systemGameManager);
            if (highlightController != null) {
                highlightController.Configure(systemGameManager);
            }

            if (gotInitialNamePlatePosition == false) {
                initialNamePlatePosition = namePlateTransform.localPosition;
                //Debug.Log($"{gameObject.name}.ComponentController.Configure(): initialNamePlatePosition: {initialNamePlatePosition} GetInstanceId: {GetInstanceID()}");
                gotInitialNamePlatePosition = true;
            }

            if (unitAudioEmitter == null) {
                unitAudioEmitter = GetComponentInChildren<UnitAudioEmitter>();
                //Debug.Log($"{gameObject.name}.UnitAudioController.OnEnable(): UnitAudioEmitter was not set.  Searching children.");
                if (unitAudioEmitter == null) {
                    Debug.LogError(gameObject.name + "UnitAudioController.OnEnable(): Could not find UnitAudioEmitter in children.  Check object.");
                }
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            networkManagerServer = systemGameManager.NetworkManagerServer;
            levelManagerClient = systemGameManager.LevelManagerClient;

        }

        public Transform GetNameplateTransform() {
            return namePlateTransform;
        }

        public void SetUnitController(UnitController unitController) {
            //Debug.Log($"ComponentController.SetUnitController({unitController.gameObject.name})");
            this.unitController = unitController;

            if (highlightController != null) {
                highlightController.SetUnitController(unitController);
            }
            if (aggroRangeController != null) {
                aggroRangeController.SetUnitController(unitController);
            }
            unitController.UnitEventController.OnCharacterConfigured += HandleCharacterConfigured;
        }

        public void SetInteractable(Interactable interactable) {
            this.interactable = interactable;
            interactableRange.SetInteractable(interactable);
            namePlateTransform.localPosition = initialNamePlatePosition;
            SetNameplateVector();
            //Debug.Log($"ComponentController.SetInteractable({interactable.gameObject.name}) nameplateVector: {nameplateVector} instanceId: {GetInstanceID()}");

            interactable.InteractableEventController.OnPlayCastSound += HandlePlayCastSound;
            interactable.InteractableEventController.OnStopCastSound += HandleStopCastSound;
            interactable.InteractableEventController.OnPlayEffectSound += HandlePlayEffectSound;
            interactable.InteractableEventController.OnStopEffectSound += HandleStopEffectSound;
            interactable.InteractableEventController.OnPlayVoiceSound += HandlePlayVoiceSound;
            interactable.InteractableEventController.OnStopVoiceSound += HandleStopVoiceSound;
            interactable.InteractableEventController.OnPlayMovementSound += HandlePlayMovementSound;
            interactable.InteractableEventController.OnStopMovementSound += HandleStopMovementSound;
            interactable.InteractableEventController.OnSetNameplatePosition += HandleSetNameplatePosition;
            interactable.OnInteractableResetSettings += HandleInteractableResetSettings;
            interactable.InteractableEventController.OnEnableInteractableRange += HandleEnableInteractableRange;
        }

        private void HandleInteractableResetSettings() {
            interactable.InteractableEventController.OnPlayCastSound -= HandlePlayCastSound;
            interactable.InteractableEventController.OnStopCastSound -= HandleStopCastSound;
            interactable.InteractableEventController.OnPlayEffectSound -= HandlePlayEffectSound;
            interactable.InteractableEventController.OnStopEffectSound -= HandleStopEffectSound;
            interactable.InteractableEventController.OnPlayVoiceSound -= HandlePlayVoiceSound;
            interactable.InteractableEventController.OnStopVoiceSound -= HandleStopVoiceSound;
            interactable.InteractableEventController.OnPlayMovementSound -= HandlePlayMovementSound;
            interactable.InteractableEventController.OnStopMovementSound -= HandleStopMovementSound;
            interactable.InteractableEventController.OnSetNameplatePosition -= HandleSetNameplatePosition;
            interactable.OnInteractableResetSettings -= HandleInteractableResetSettings;
            interactable.InteractableEventController.OnEnableInteractableRange -= HandleEnableInteractableRange;
            if (unitController != null) {
                unitController.UnitEventController.OnCharacterConfigured -= HandleCharacterConfigured;
            }
            unitController = null;
            interactableRange.ResetSettings();
            if (highlightController != null) {
                highlightController.ResetSettings();
            }
        }

        private void HandleCharacterConfigured() {
            interactableRange.AdjustCollider();
        }

        private void HandleEnableInteractableRange() {
            interactableRange.EnableCollider();
        }

        private void HandleSetNameplatePosition(Vector3 overridePosition) {
            //Debug.Log($"{interactable.gameObject.name}.ComponentController.HandleSetNameplatePosition({overridePosition})");

            namePlateTransform.localPosition = overridePosition;
            SetNameplateVector();
            if (unitController != null) {
                unitController.SetNameplateVector();
            }
        }

        private void SetNameplateVector() {
            nameplateVector = namePlateTransform.position - interactable.transform.position;
        }

        public bool MovementSoundIsPlaying(bool ignoreOneShots = true) {
            if (unitAudioEmitter == null) {
                return false;
            }
            return unitAudioEmitter.MovementIsPlaying();
        }

        /*
        public void PlayCastSound(AudioClip audioClip) {
            PlayCastSound(audioClip, false);
        }
        */

        public void HandlePlayCastSound(AudioClip audioClip, bool loop) {
            //Debug.Log($"{gameObject.name}.UnitComponentController.PlayCastSound(" + (audioClip == null ? "null" : audioClip.name) + ")");
            if (audioClip == null) {
                return;
            }

            if (unitAudioEmitter != null && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false)) {
                unitAudioEmitter.PlayCast(audioClip, loop);
            }
        }

        /*
        public void PlayEffectSound(AudioClip audioClip) {
            PlayEffectSound(audioClip, false);
        }
        */

        public void HandlePlayEffectSound(AudioClip audioClip, bool loop) {
            //Debug.Log($"{gameObject.name}.UnitComponentController.PlayEffectSound(" + (audioClip == null ? "null" : audioClip.name) + ")");
            if (audioClip == null) {
                return;
            }

            if (unitAudioEmitter != null && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false)) {
                unitAudioEmitter.PlayEffect(audioClip, loop);
            }
        }

        public void HandlePlayVoiceSound(AudioClip audioClip) {
            //Debug.Log($"{gameObject.name}.UnitComponentController.PlayVoiceSound({(audioClip == null ? "null" : audioClip.name)})");

            if (audioClip == null) {
                return;
            }
            if (unitAudioEmitter != null && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false)) {
                unitAudioEmitter.PlayVoice(audioClip);
            }
        }

        public void HandlePlayMovementSound(AudioClip audioClip, bool loop) {
            //Debug.Log($"{gameObject.name}UnitAudio.PlayMovement()");
            if (audioClip == null) {
                return;
            }
            if (unitAudioEmitter != null && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false)) {
                unitAudioEmitter.PlayMovement(audioClip, loop);
            }
        }

        public void HandleStopCastSound() {
            //Debug.Log($"{gameObject.name}UnitAudio.StopCast()");
            if (unitAudioEmitter != null && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false)) {
                unitAudioEmitter.StopCast();
            }
        }

        public void HandleStopEffectSound() {
            //Debug.Log($"{gameObject.name}UnitAudio.StopEffect()");
            if (unitAudioEmitter != null && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false)) {
                unitAudioEmitter.StopEffect();
            }
        }

        public void HandleStopVoiceSound() {
            //Debug.Log($"{gameObject.name}UnitAudio.StopVoice()");
            if (unitAudioEmitter != null && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false)) {
                unitAudioEmitter.StopVoice();
            }
        }

        public void HandleStopMovementSound(bool stopLoopsOnly = true) {
            //Debug.Log($"{gameObject.name}.UnitComponentController.StopMovementSound()");
            if (unitAudioEmitter != null && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false)) {
                unitAudioEmitter.StopMovement(stopLoopsOnly);
            }
        }


    }

}
