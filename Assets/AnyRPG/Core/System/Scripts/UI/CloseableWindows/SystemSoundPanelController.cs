using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemSoundPanelController : WindowContentController {

        [Header("Sound Panel")]

        [SerializeField]
        public Slider masterSlider;

        [SerializeField]
        public Slider musicSlider;

        [SerializeField]
        public Slider effectsSlider;

        [SerializeField]
        public Slider ambientSlider;

        [SerializeField]
        public Slider uiSlider;

        [SerializeField]
        public Slider voiceSlider;

        public override void Configure(SystemGameManager systemGameManager) {
            Debug.Log("SystemSoundPanelController.Configure()");
            base.Configure(systemGameManager);
        }

        public void Start() {
            //Debug.Log("SoundMenuController.Start()");
            /*
            float rawValue = audioManager.GetVolume(audioManager.MasterVolume);
            float adjustedValue = Mathf.Pow(10, (rawValue / 20));
            //Debug.Log("SoundMenuController.Start() adjusted value: " + adjustedValue);
            masterVolumeSlider.value = adjustedValue;
            */

            LoadVolumeSliderValues();

        }

        private void LoadVolumeSliderValues() {
            Debug.Log("SystemSoundPanelController.LoadVolumeSliderValues()");
            masterSlider.value = PlayerPrefs.GetFloat(audioManager.MasterVolume);
            musicSlider.value = PlayerPrefs.GetFloat(audioManager.MusicVolume);
            ambientSlider.value = PlayerPrefs.GetFloat(audioManager.AmbientVolume);
            effectsSlider.value = PlayerPrefs.GetFloat(audioManager.EffectsVolume);
            uiSlider.value = PlayerPrefs.GetFloat(audioManager.UiVolume);
            voiceSlider.value = PlayerPrefs.GetFloat(audioManager.VoiceVolume);
        }


        public void MasterSlider() {
            Debug.Log("SystemSoundPanelController.MasterSlider()");
            if (audioManager == null) {
                Debug.Log("SystemSoundPanelController.MasterSlider() audiomanager is null");
            }
            if (masterSlider == null) {
                Debug.Log("SystemSoundPanelController.MasterSlider() masterslider is null");
            }
            audioManager.SetMasterVolume(masterSlider.value);
        }

        public void MusicSlider() {
            audioManager.SetMusicVolume(musicSlider.value);
        }

        public void AmbientSlider() {
            audioManager.SetAmbientVolume(ambientSlider.value);
        }

        public void EffectsSlider() {
            audioManager.SetEffectsVolume(effectsSlider.value);
        }

        public void UISlider() {
            audioManager.SetUIVolume(uiSlider.value);
        }

        public void VoiceSlider() {
            audioManager.SetVoiceVolume(voiceSlider.value);
        }

    }

}