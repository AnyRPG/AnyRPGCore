using UnityEngine;

namespace AnyRPG {
    
    public class ObjectAudioController : MonoBehaviour {

        public event System.Action<AudioClip> OnPlayAudioClip = delegate { };
        public event System.Action<AudioClip> OnPlayOneShot = delegate { };
        public event System.Action OnStopAudio = delegate { };
        public event System.Action OnPauseAudio = delegate { };
        public event System.Action OnUnPauseAudio = delegate { };

        private bool serverModeActive = false;

        // when set, decides at play time whether this device should hear audio (see SetMuteCheck)
        private System.Func<bool> isMuted = null;

        [SerializeField]
        private AudioSource audioSource = null;

        public AudioSource AudioSource { get => audioSource; set => audioSource = value; }
        
        public void SetServerModeActive(bool serverModeActive) {
            this.serverModeActive = serverModeActive;
        }

        /// <summary>
        /// Decide muting at play time instead of once at configure time. A host's own client can connect after
        /// objects are configured, so a configure-time "server mode" flag would mute a host permanently.
        /// </summary>
        public void SetMuteCheck(System.Func<bool> isMuted) {
            this.isMuted = isMuted;
        }

        private bool Muted { get => isMuted != null ? isMuted() : serverModeActive; }

        public void PlayAudioClip(AudioClip audioClip) {
            if (audioSource != null && audioClip != null) {
                audioSource.clip = audioClip;
                if (Muted == false) {
                    audioSource.Play();
                }
                OnPlayAudioClip(audioClip);
            }
        }

        public void PlayOneShot(AudioClip audioClip) {
            if (audioSource != null && audioClip != null) {
                if (Muted == false) {
                    audioSource.PlayOneShot(audioClip);
                }
                OnPlayOneShot(audioClip);
            }
        }

        public void StopAudio() {
            if (audioSource != null && Muted == false) {
                audioSource.Stop();
            }
            OnStopAudio();
        }
        public void PauseAudio() {
            if (audioSource != null && Muted == false) {
                audioSource.Pause();
            }
            OnPauseAudio();
        }
        public void UnPauseAudio() {
            if (audioSource != null && Muted == false) {
                audioSource.UnPause();
            }
            OnUnPauseAudio();
        }

    }

}
