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

        /*
        public override void Configure(SystemGameManager systemGameManager) {
            Debug.Log("SystemSoundPanelController.Configure() instanceID: " + GetInstanceID());
            base.Configure(systemGameManager);
        }
        */

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
            //Debug.Log("SystemSoundPanelController.LoadVolumeSliderValues()");
            masterSlider.value = PlayerPrefs.GetFloat(audioManager.MasterVolume);
            musicSlider.value = PlayerPrefs.GetFloat(audioManager.MusicVolume);
            ambientSlider.value = PlayerPrefs.GetFloat(audioManager.AmbientVolume);
            effectsSlider.value = PlayerPrefs.GetFloat(audioManager.EffectsVolume);
            uiSlider.value = PlayerPrefs.GetFloat(audioManager.UiVolume);
            voiceSlider.value = PlayerPrefs.GetFloat(audioManager.VoiceVolume);
        }

        public void ResetDefaultVolume() {
            audioManager.ResetDefaultVolume();
            LoadVolumeSliderValues();
        }


        public void MasterSlider() {
            if (configureCount == 0) {
                return;
            }
            //Debug.Log("SystemSoundPanelController.MasterSlider() instanceID: " + GetInstanceID());
            if (audioManager == null) {
                Debug.Log("SystemSoundPanelController.MasterSlider() audiomanager is null");
            }
            if (masterSlider == null) {
                Debug.Log("SystemSoundPanelController.MasterSlider() masterslider is null");
            }
            audioManager.SetMasterVolume(masterSlider.value);
        }

        public void MusicSlider() {
            if (configureCount == 0) {
                return;
            }
            audioManager.SetMusicVolume(musicSlider.value);
        }

        public void AmbientSlider() {
            if (configureCount == 0) {
                return;
            }
            audioManager.SetAmbientVolume(ambientSlider.value);
        }

        public void EffectsSlider() {
            if (configureCount == 0) {
                return;
            }
            audioManager.SetEffectsVolume(effectsSlider.value);
        }

        public void UISlider() {
            if (configureCount == 0) {
                return;
            }
            audioManager.SetUIVolume(uiSlider.value);
        }

        public void VoiceSlider() {
            if (configureCount == 0) {
                return;
            }
            audioManager.SetVoiceVolume(voiceSlider.value);
        }

    }

}