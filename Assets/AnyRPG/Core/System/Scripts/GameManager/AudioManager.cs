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
        private AudioSource ambientAudioSource1 = null;

        [SerializeField]
        private AudioSource ambientAudioSource2 = null;

        [SerializeField]
        private AudioSource uiAudioSource = null;

        [SerializeField]
        private AudioSource voiceAudioSource = null;

        [SerializeField]
        private AudioClip uiClickSound = null;

        private bool musicPaused = false;
        private bool ambientPaused = false;
        private AudioSource currentAmbientAudioSource = null;
        private AudioSource secondaryAmbientAudioSource = null;
        private Coroutine fadeCoroutine = null;

        public string MasterVolume { get => masterVolume; }
        public string MusicVolume { get => musicVolume; }
        public string EffectsVolume { get => effectsVolume; }
        public string AmbientVolume { get => ambientVolume; }
        public string UiVolume { get => uiVolume; set => uiVolume = value; }
        public string VoiceVolume { get => voiceVolume; set => voiceVolume = value; }
        public AudioClip UIClickSound { get => uiClickSound; set => uiClickSound = value; }
        public AudioSource MusicAudioSource { get => musicAudioSource; set => musicAudioSource = value; }
        public AudioSource EffectsAudioSource { get => effectsAudioSource; set => effectsAudioSource = value; }
        public AudioSource AmbientAudioSource { get => currentAmbientAudioSource; set => currentAmbientAudioSource = value; }
        public AudioSource UiAudioSource { get => uiAudioSource; set => uiAudioSource = value; }
        public AudioSource VoiceAudioSource { get => voiceAudioSource; set => voiceAudioSource = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("AudioManager.Configure()");
            base.Configure(systemGameManager);

            InitializeVolume();
            currentAmbientAudioSource = ambientAudioSource1;
            secondaryAmbientAudioSource = ambientAudioSource2;
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

        public void ResetDefaultVolume() {
            SetMasterVolume(defaultMasterVolume);
            SetMusicVolume(defaultMusicVolume);
            SetAmbientVolume(defaultAmbientVolume);
            SetEffectsVolume(defaultEffectsVolume);
            SetUIVolume(defaultUIVolume);
            SetVoiceVolume(defaultVoiceVolume);
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
            Debug.Log("AudioManager.PlayAmbient(" + (audioClip == null ? "null" : audioClip.name) + ")");

            if (currentAmbientAudioSource.clip == audioClip && ambientPaused == true) {
                UnPauseAmbient();
                return;
            }
            ambientPaused = false;
            currentAmbientAudioSource.clip = audioClip;
            currentAmbientAudioSource.loop = true;
            currentAmbientAudioSource.volume = 1f;
            currentAmbientAudioSource.Play();
        }

        public void CrossFadeAmbient(AudioClip audioClip, float seconds) {
            //Debug.Log("AudioManager.CrossFadeAmbient(" + (audioClip == null ? "null" : audioClip.name) + ", " + seconds + ")");

            if (ambientPaused == false
                && currentAmbientAudioSource.clip == audioClip
                && currentAmbientAudioSource.isPlaying == true) {
                return;
            }
            ambientPaused = false;
            if (currentAmbientAudioSource == ambientAudioSource1) {
                currentAmbientAudioSource = ambientAudioSource2;
                secondaryAmbientAudioSource = ambientAudioSource1;
            } else {
                currentAmbientAudioSource = ambientAudioSource1;
                secondaryAmbientAudioSource = ambientAudioSource2;
            }
            currentAmbientAudioSource.clip = audioClip;
            currentAmbientAudioSource.loop = true;
            currentAmbientAudioSource.volume = 0f;
            if (audioClip != null) {
                currentAmbientAudioSource.Play();
            }

            if (fadeCoroutine != null) {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeAmbientAudio(seconds));
        }

        private IEnumerator FadeAmbientAudio(float seconds) {
            if (seconds == 0f) {
                // avoid division by zero
                seconds = 0.1f;
            }
            float elapsedTime = 0f;
            float secondaryDelta = secondaryAmbientAudioSource.volume;
            while (elapsedTime < seconds) {
                currentAmbientAudioSource.volume = Mathf.Clamp(elapsedTime / seconds, 0f, 1f);
                secondaryAmbientAudioSource.volume = Mathf.Clamp(secondaryDelta - (elapsedTime / seconds), 0f, 1f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            secondaryAmbientAudioSource.Stop();
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
            if (fadeCoroutine != null) {
                StopCoroutine(fadeCoroutine);
            }
            ambientAudioSource1.Stop();
            ambientAudioSource2.Stop();

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
                currentAmbientAudioSource.Pause();
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
            currentAmbientAudioSource.UnPause();
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