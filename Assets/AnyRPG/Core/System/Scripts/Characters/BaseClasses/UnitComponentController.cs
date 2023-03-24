using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AnyRPG {
    public class UnitComponentController : ConfiguredMonoBehaviour {

        [Tooltip("Drag an object in the heirarchy here and the nameplate will show at its transform location")]
        [SerializeField]
        private Transform namePlateTransform = null;

        [SerializeField]
        private AggroRange aggroRangeController = null;

        [SerializeField]
        private InteractableRange interactableRange = null;

        [Tooltip("A reference to the unit audio emitter.  This is optional as it will be found automatically if left blank.")]
        [SerializeField]
        private UnitAudioEmitter unitAudioEmitter = null;

        [Tooltip("A reference to the highlight circle")]
        [SerializeField]
        private HighlightController highlightController = null;

        private Vector3 initialNamePlatePosition = Vector3.zero;
        private bool gotInitialNamePlatePosition = false;

        public Transform NamePlateTransform { get => namePlateTransform; set => namePlateTransform = value; }
        public AggroRange AggroRangeController { get => aggroRangeController; set => aggroRangeController = value; }
        public InteractableRange InteractableRange { get => interactableRange; set => interactableRange = value; }
        public HighlightController HighlightController { get => highlightController; set => highlightController = value; }
        public Vector3 InitialNamePlatePosition { get => initialNamePlatePosition; set => initialNamePlatePosition = value; }
        public bool GotInitialNamePlatePosition { get => gotInitialNamePlatePosition; set => gotInitialNamePlatePosition = value; }

        //public UnitAudioEmitter UnitAudioEmitter { get => unitAudioEmitter; set => unitAudioEmitter = value; }

        public override void Configure(SystemGameManager systemGameManager) {

            base.Configure(systemGameManager);
            interactableRange.Configure(systemGameManager);
            if (highlightController != null) {
                highlightController.Configure(systemGameManager);
            }

            if (unitAudioEmitter == null) {
                unitAudioEmitter = GetComponentInChildren<UnitAudioEmitter>();
                Debug.Log(gameObject.name + "UnitAudioController.OnEnable(): UnitAudioEmitter was not set.  Searching children.");
                if (unitAudioEmitter == null) {
                    Debug.LogError(gameObject.name + "UnitAudioController.OnEnable(): Could not find UnitAudioEmitter in children.  Check object.");
                }
            }
        }

        public bool MovementSoundIsPlaying(bool ignoreOneShots = true) {
            if (unitAudioEmitter == null) {
                return false;
            }
            return unitAudioEmitter.MovementIsPlaying();
        }

        public void PlayCastSound(AudioClip audioClip) {
            PlayCastSound(audioClip, false);
        }

        public void PlayCastSound(AudioClip audioClip, bool loop) {
            //Debug.Log($"{gameObject.name}.UnitComponentController.PlayCastSound(" + (audioClip == null ? "null" : audioClip.name) + ")");
            if (audioClip == null) {
                return;
            }

            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayCast(audioClip, loop);
            }
        }

        public void PlayEffectSound(AudioClip audioClip) {
            PlayEffectSound(audioClip, false);
        }

        public void PlayEffectSound(AudioClip audioClip, bool loop) {
            //Debug.Log($"{gameObject.name}.UnitComponentController.PlayEffectSound(" + (audioClip == null ? "null" : audioClip.name) + ")");
            if (audioClip == null) {
                return;
            }

            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayEffect(audioClip, loop);
            }
        }

        public void PlayVoiceSound(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayVoice(audioClip);
            }
        }

        public void PlayMovementSound(AudioClip audioClip, bool loop) {
            //Debug.Log($"{gameObject.name}UnitAudio.PlayMovement()");
            if (audioClip == null) {
                return;
            }
            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayMovement(audioClip, loop);
            }
        }

        public void StopCastSound() {
            //Debug.Log($"{gameObject.name}UnitAudio.StopCast()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopCast();
            }
        }

        public void StopEffectSound() {
            //Debug.Log($"{gameObject.name}UnitAudio.StopEffect()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopEffect();
            }
        }

        public void StopVoiceSound() {
            //Debug.Log($"{gameObject.name}UnitAudio.StopVoice()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopVoice();
            }
        }

        public void StopMovementSound(bool stopLoopsOnly = true) {
            //Debug.Log($"{gameObject.name}.UnitComponentController.StopMovementSound()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopMovement(stopLoopsOnly);
            }
        }

    }

}
