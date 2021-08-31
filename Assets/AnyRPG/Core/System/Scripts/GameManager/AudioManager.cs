using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AnyRPG {
    public class AudioManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private AudioMixer audioMixer = null;

        private float defaultMasterVolume = 1.0f;
        private float defaultMusicVolume = 1.0f;
        private float defaultAmbientVolume = 1.0f;
        private float defaultEffectsVolume = 1.0f;
        private float defaultUIVolume = 1.0f;
        private float defaultVoiceVolume = 1.0f;

        private string masterVolume = "MasterVolume";
        private string musicVolume = "MusicVolume";
        private string effectsVolume = "EffectsVolume";
        private string uiVolume = "UIVolume";
        private string voiceVolume = "VoiceVolume";
        private string ambientVolume = "AmbientVolume";

        [SerializeField]
        private AudioSource musicAudioSource = null;

        [SerializeField]
        private AudioSource effectsAudioSource = null;

        [SerializeField]
        private AudioSource ambientAudioSource = null;

        [SerializeField]
        private AudioSource uiAudioSource = null;

        [SerializeField]
        private AudioSource voiceAudioSource = null;

        [SerializeField]
        private AudioClip uiClickSound = null;

        private bool musicPaused = false;
        private bool ambientPaused = false;

        public string MasterVolume { get => masterVolume; }
        public string MusicVolume { get => musicVolume; }
        public string EffectsVolume { get => effectsVolume; }
        public string AmbientVolume { get => ambientVolume; }
        public string UiVolume { get => uiVolume; set => uiVolume = value; }
        public string VoiceVolume { get => voiceVolume; set => voiceVolume = value; }
        public AudioClip UIClickSound { get => uiClickSound; set => uiClickSound = value; }
        public AudioSource MusicAudioSource { get => musicAudioSource; set => musicAudioSource = value; }
        public AudioSource EffectsAudioSource { get => effectsAudioSource; set => effectsAudioSource = value; }
        public AudioSource AmbientAudioSource { get => ambientAudioSource; set => ambientAudioSource = value; }
        public AudioSource UiAudioSource { get => uiAudioSource; set => uiAudioSource = value; }
        public AudioSource VoiceAudioSource { get => voiceAudioSource; set => voiceAudioSource = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("AudioManager.Configure()");
            base.Configure(systemGameManager);

            InitializeVolume();
        }

        private void InitializeVolume() {
            //Debug.Log("AudioManager.InitializeVolume()");
            SetDefaultVolume();
        }

        private void SetDefaultVolume() {
            //Debug.Log("AudioManager.SetDefaultVolume()");
            if (PlayerPrefs.HasKey(MasterVolume) == true) {
                SetVolume(MasterVolume, PlayerPrefs.GetFloat(MasterVolume));
            } else {
                SetMasterVolume(defaultMasterVolume);
            }
            if (PlayerPrefs.HasKey(MusicVolume) == true) {
                SetVolume(MusicVolume, PlayerPrefs.GetFloat(MusicVolume));
            } else {
                SetMusicVolume(defaultMusicVolume);
            }
            if (PlayerPrefs.HasKey(AmbientVolume) == true) {
                SetVolume(AmbientVolume, PlayerPrefs.GetFloat(AmbientVolume));
            } else {
                SetAmbientVolume(defaultAmbientVolume);
            }
            if (PlayerPrefs.HasKey(EffectsVolume) == true) {
                SetVolume(EffectsVolume, PlayerPrefs.GetFloat(EffectsVolume));
            } else {
                SetEffectsVolume(defaultEffectsVolume);
            }
            if (PlayerPrefs.HasKey(UiVolume) == true) {
                SetVolume(UiVolume, PlayerPrefs.GetFloat(UiVolume));
            } else {
                SetUIVolume(defaultUIVolume);
            }
            if (PlayerPrefs.HasKey(VoiceVolume) == true) {
                SetVolume(VoiceVolume, PlayerPrefs.GetFloat(VoiceVolume));
            } else {
                SetVoiceVolume(defaultVoiceVolume);
            }
        }

        private void SetVolume(string volumeType, float volume) {
            //Debug.Log("AudioManager.SetVolume(" + volumeType + ", " + volume + "): setting audioMixer float");
            audioMixer.SetFloat(volumeType, GetLogVolume(volume));
        }

        private float GetLogVolume(float volume) {
            //Debug.Log("AudioManager.GetLogVolume(" + volume + ")");
            float logVolume = -80.0f;
            float alternateLogVolume = -80f;
            if (volume > 0) {
                logVolume = Mathf.Log10(volume) * 20f;
                alternateLogVolume = (1 - Mathf.Sqrt(volume)) * -80f;
            }
            //Debug.Log("AudioManager.GetLogVolume(" + volume + "): returning: " + logVolume + "; alternateVolume: " + alternateLogVolume);
            return logVolume;
        }

        public void SetMasterVolume(float volume) {
            //Debug.Log("AudioManager.SetMasterVolume(" + volume + ")");
            PlayerPrefs.SetFloat(MasterVolume, volume);
            audioMixer.SetFloat(MasterVolume, GetLogVolume(volume));
        }

        public void SetMusicVolume(float volume) {
            //Debug.Log("AudioManager.SetMusicVolume(" + volume + ")");
            PlayerPrefs.SetFloat(MusicVolume, volume);
            audioMixer.SetFloat(MusicVolume, GetLogVolume(volume));
        }

        public void SetAmbientVolume(float volume) {
            //Debug.Log("AudioManager.SetAmbientVolume(" + volume + ")");
            PlayerPrefs.SetFloat(AmbientVolume, volume);
            audioMixer.SetFloat(AmbientVolume, GetLogVolume(volume));
        }

        public void SetEffectsVolume(float volume) {
            //Debug.Log("AudioManager.SetEffectsVolume(" + volume + ")");
            PlayerPrefs.SetFloat(EffectsVolume, volume);
            audioMixer.SetFloat(EffectsVolume, GetLogVolume(volume));
        }

        public void SetUIVolume(float volume) {
            //Debug.Log("AudioManager.SetEffectsVolume(" + volume + ")");
            PlayerPrefs.SetFloat(UiVolume, volume);
            audioMixer.SetFloat(UiVolume, GetLogVolume(volume));
        }

        public void SetVoiceVolume(float volume) {
            //Debug.Log("AudioManager.SetEffectsVolume(" + volume + ")");
            PlayerPrefs.SetFloat(VoiceVolume, volume);
            audioMixer.SetFloat(VoiceVolume, GetLogVolume(volume));
        }

        public float GetVolume(string volumeType) {
            //Debug.Log("AudioManager.GetVolume()");
            float returnValue = 0f;
            if (!audioMixer.GetFloat(volumeType, out returnValue)) {
                //Debug.Log("could not get float!");
            }
            //Debug.Log("AudioManager.GetVolume(): returnValue: " + returnValue);
            return returnValue;
        }

        public void PlayAmbient(AudioClip audioClip) {
            if (ambientAudioSource.clip == audioClip && ambientPaused == true) {
                UnPauseAmbient();
                return;
            }
            ambientPaused = false;
            ambientAudioSource.clip = audioClip;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
        }

        public void PlayMusic(AudioClip audioClip) {
            //Debug.Log("AudioManager.PlayMusic()");
            if (musicAudioSource.clip == audioClip && musicPaused == true) {
                UnPauseMusic();
                return;
            }
            musicPaused = false;
            musicAudioSource.clip = audioClip;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }

        public void PlayEffect(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            effectsAudioSource.PlayOneShot(audioClip);
        }

        public void PlayUI(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            uiAudioSource.PlayOneShot(audioClip);
        }

        public void PlayVoice(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            voiceAudioSource.PlayOneShot(audioClip);
        }

        public void PlayUIHoverSound() {
            if (uiClickSound != null) {
                PlayUI(uiClickSound);
            }
        }

        public void PlayUIClickSound() {
            //Debug.Log("AudioManager.PlayUIClickSound()");
            if (uiClickSound != null) {
                //Debug.Log("AudioManager.PlayUIClickSound(): click sound is not null");
                PlayUI(uiClickSound);
            } else {
                //Debug.Log("AudioManager.PlayUIClickSound(): click sound is null");
            }
        }

        public void StopAmbient() {
            ambientAudioSource.Stop();
            ambientPaused = false;
        }

        public void StopMusic() {
            musicAudioSource.Stop();
            musicPaused = false;
        }

        public void PauseMusic() {
            if (musicPaused == false) {
                musicAudioSource.Pause();
                musicPaused = true;
            } else {
                UnPauseMusic();
            }
        }

        public void PauseAmbient() {
            if (ambientPaused == false) {
                ambientAudioSource.Pause();
                ambientPaused = true;
            } else {
                UnPauseAmbient();
            }
        }

        private void UnPauseMusic() {
            musicAudioSource.UnPause();
            musicPaused = false;
        }

        private void UnPauseAmbient() {
            ambientAudioSource.UnPause();
            ambientPaused = false;
        }

        public void StopEffects() {
            effectsAudioSource.Stop();
        }
        public void StopUI() {
            uiAudioSource.Stop();
        }
        public void StopVoice() {
            voiceAudioSource.Stop();
        }

    }

    public enum AudioType { Music, Ambient, Effect }

}