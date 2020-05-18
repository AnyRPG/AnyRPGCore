using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AnyRPG {
    public class UnitAudioController : MonoBehaviour {

        [Tooltip("A reference to the unit audio emitter.  This is optional as it will be found automatically if left blank.")]
        [SerializeField]
        private UnitAudioEmitter unitAudioEmitter = null;

        private void Awake() {
            if (unitAudioEmitter == null) {
                unitAudioEmitter = GetComponentInChildren<UnitAudioEmitter>();
                Debug.Log(gameObject.name + "UnitAudioController.Awake(): UnitAudioEmitter was not set.  Searching children.");
                if (unitAudioEmitter == null) {
                    Debug.LogError(gameObject.name + "UnitAudioController.Awake(): Could not find UnitAudioEmitter in children.  Check object.");
                }
            }
        }

        public void PlayCast(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            //Debug.Log(gameObject.name + "UnitAudio.PlayEffect(" + audioClip.name + ")");

            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayCast(audioClip);
            }
        }

        public void PlayEffect(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            //Debug.Log(gameObject.name + "UnitAudio.PlayEffect(" + audioClip.name + ")");

            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayEffect(audioClip);
            }
        }

        public void PlayVoice(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayVoice(audioClip);
            }
        }

        public void StopCast() {
            //Debug.Log(gameObject.name + "UnitAudio.StopCast()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopCast();
            }
        }

        public void StopEffect() {
            //Debug.Log(gameObject.name + "UnitAudio.StopEffect()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopEffect();
            }
        }

        public void StopVoice() {
            //Debug.Log(gameObject.name + "UnitAudio.StopVoice()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopVoice();
            }
        }

    }

}
