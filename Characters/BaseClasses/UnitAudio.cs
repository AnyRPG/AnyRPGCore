using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AnyRPG {
    public class UnitAudio : MonoBehaviour {

        [SerializeField]
        private AudioSource effectSource = null;

        [SerializeField]
        private AudioSource voiceSource = null;

        //public AudioSource MyEffectSource { get => effectSource; set => effectSource = value; }
        //public AudioSource MyVoiceSource { get => voiceSource; set => voiceSource = value; }

        public void PlayEffect(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            if (effectSource != null) {
                effectSource.PlayOneShot(audioClip);
            }
        }

        public void PlayVoice(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            if (voiceSource != null) {
                voiceSource.PlayOneShot(audioClip);
            }
        }

        public void StopEffect() {
            if (effectSource != null) {
                effectSource.Stop();
            }
        }

        public void StopVoice() {
            if (voiceSource != null) {
                voiceSource.Stop();
            }
        }

    }

}
