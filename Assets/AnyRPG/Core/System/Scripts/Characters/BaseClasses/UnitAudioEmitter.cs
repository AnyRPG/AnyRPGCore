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

        [Tooltip("Play looping movement sounds like footsteps or engines through this audio source")]
        [SerializeField]
        private AudioSource movementSource = null;

        [Tooltip("Play non looping movement sounds like footsteps or engine revs through this audio source")]
        [SerializeField]
        private AudioSource movementHitSource = null;

        public void PlayCast(AudioClip audioClip) {
            PlayCast(audioClip, false);
        }

        public void PlayCast(AudioClip audioClip, bool loop) {
            if (audioClip == null) {
                return;
            }
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.PlayCast(" + audioClip.name + ")");

            if (castSource != null) {
                if (loop == true) {
                    castSource.loop = true;
                    castSource.clip = audioClip;
                    castSource.Play();
                } else {
                    castSource.PlayOneShot(audioClip);
                }
            }
        }

        public void PlayEffect(AudioClip audioClip) {
            PlayEffect(audioClip, false);
        }

        public void PlayEffect(AudioClip audioClip, bool loop) {
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.PlayEffect(" + (audioClip == null ? "null" : audioClip.name) + ", " + loop + ")");
            if (audioClip == null) {
                return;
            }
            if (effectSource != null) {
                if (loop == true) {
                    effectSource.loop = true;
                    effectSource.clip = audioClip;
                    effectSource.Play();
                } else {
                    effectSource.PlayOneShot(audioClip);
                }
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

            if (loop) {
                if (movementSource != null) {
                    movementSource.loop = true;
                    movementSource.clip = audioClip;
                    movementSource.Play();
                }
            } else {
                if (movementHitSource != null) {
                    movementHitSource.PlayOneShot(audioClip);
                }
            }
        }

        public void StopCast() {
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.StopCast()");
            if (castSource != null) {
                castSource.Stop();
                castSource.loop = false;
            }
        }

        public void StopEffect() {
            //Debug.Log(gameObject.name + ".UnitAudioEmitter.StopEffect()");
            if (effectSource != null) {
                effectSource.Stop();
                effectSource.loop = false;
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
            if (movementSource != null) { 
                movementSource.Stop();
            }
            if (movementHitSource != null && stopLoopsOnly == false) {
                movementHitSource.Stop();
            }
        }

        public bool MovementIsPlaying(bool ignoreOneShots = true) {
            if (movementSource == null && movementHitSource == null) {
                return false;
            }
            if (movementSource.isPlaying == true) {
                return true;
            }
            if (movementHitSource.isPlaying == true && ignoreOneShots == false) {
                return true;
            }

            return false;
        }

    }

}
