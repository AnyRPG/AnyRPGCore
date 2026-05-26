using UnityEngine;

namespace AnyRPG {
    
    public class ObjectAudioController : MonoBehaviour {

        public event System.Action<AudioClip> OnPlayAudioClip = delegate { };
        public event System.Action<AudioClip> OnPlayOneShot = delegate { };
        public event System.Action OnStopAudio = delegate { };
        public event System.Action OnPauseAudio = delegate { };
        public event System.Action OnUnPauseAudio = delegate { };

        private bool serverModeActive = false;

        [SerializeField]
        private AudioSource audioSource = null;

        public AudioSource AudioSource { get => audioSource; set => audioSource = value; }
        
        public void SetServerModeActive(bool serverModeActive) {
            this.serverModeActive = serverModeActive;
        }

        public void PlayAudioClip(AudioClip audioClip) {
            if (audioSource != null && audioClip != null) {
                audioSource.clip = audioClip;
                if (serverModeActive == false) {
                    audioSource.Play();
                }
                OnPlayAudioClip(audioClip);
            }
        }

        public void PlayOneShot(AudioClip audioClip) {
            if (audioSource != null && audioClip != null) {
                if (serverModeActive == false) {
                    audioSource.PlayOneShot(audioClip);
                }
                OnPlayOneShot(audioClip);
            }
        }

        public void StopAudio() {
            if (audioSource != null && serverModeActive == false) {
                audioSource.Stop();
            }
            OnStopAudio();
        }
        public void PauseAudio() {
            if (audioSource != null && serverModeActive == false) {
                audioSource.Pause();
            }
            OnPauseAudio();
        }
        public void UnPauseAudio() {
            if (audioSource != null && serverModeActive == false) {
                audioSource.UnPause();
            }
            OnUnPauseAudio();
        }

    }

}
