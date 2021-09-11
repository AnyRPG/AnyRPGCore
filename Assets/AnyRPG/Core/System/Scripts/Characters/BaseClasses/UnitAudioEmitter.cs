using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AnyRPG {
    public class UnitAudioEmitter : MonoBehaviour {

        [Tooltip("Play interruptable casting sounds through this audio source")]
        [SerializeField]
        private AudioSource castSource = null;

        [Tooltip("Play uninterruptable effects through this audio source")]
        [SerializeField]
        private AudioSource effectSource = null;

        [Tooltip("Play voice tracks through this audio source")]
        [SerializeField]
        private AudioSource voiceSource = null;

        [Tooltip("Play movement sounds like footsteps or engines through this audio source")]
        [SerializeField]
        private AudioSource movementSource = null;

        private bool movementIsLoop = false;


        public void PlayCast(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.PlayCast(" + audioClip.name + ")");

            if (castSource != null) {
                castSource.PlayOneShot(audioClip);
            }
        }

        public void PlayEffect(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.PlayEffect(" + audioClip.name + ")");

            if (effectSource != null) {
                effectSource.PlayOneShot(audioClip);
            }
        }

        public void PlayVoice(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.PlayVoice(" + audioClip.name + ")");
            if (voiceSource != null) {
                voiceSource.PlayOneShot(audioClip);
            }
        }

        public void PlayMovement(AudioClip audioClip, bool loop) {
            if (audioClip == null) {
                return;
            }
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.PlayMovement(" + audioClip.name + ")");
            if (movementSource != null) {
                if (loop) {
                    movementIsLoop = true;
                    movementSource.loop = true;
                    movementSource.clip = audioClip;
                    movementSource.Play();
                } else {
                    movementSource.PlayOneShot(audioClip);
                }
            }
        }

        public void StopCast() {
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.StopCast()");
            if (castSource != null) {
                castSource.Stop();
            }
        }

        public void StopEffect() {
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.StopEffect()");
            if (effectSource != null) {
                effectSource.Stop();
            }
        }

        public void StopVoice() {
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.StopVoice()");
            if (voiceSource != null) {
                voiceSource.Stop();
            }
        }

        public void StopMovement(bool stopLoopsOnly = true) {
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.StopVoice()");
            if (movementSource != null
                && (stopLoopsOnly == false || movementIsLoop == true)) {
                movementSource.Stop();
                movementIsLoop = false;
            }
        }

        public bool MovementIsPlaying(bool ignoreOneShots = true) {
            if (movementSource == null) {
                return false;
            }
            if (movementSource.isPlaying == true
                && (ignoreOneShots == false || movementIsLoop == true)) {
                return true;
            }

            return false;
        }

    }

}
