using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace AnyRPG {
    public class SoundMenuController : WindowContentController {

        [SerializeField]
        private Slider masterVolumeSlider = null;

        //[SerializeField]
        //private Slider musicVolumeSlider = null;

        //[SerializeField]
        //private Slider effectsVolumeSlider = null;

        //[SerializeField]
        //private Slider ambientVolumeSlider = null;

        private void Start() {
            //Debug.Log("SoundMenuController.Start()");
            float rawValue = AudioManager.Instance.GetVolume(AudioManager.Instance.MasterVolume);
            float adjustedValue = Mathf.Pow(10, (rawValue / 20));
            //Debug.Log("SoundMenuController.Start() adjusted value: " + adjustedValue);
            masterVolumeSlider.value = adjustedValue;
        }

        public void SetMasterVolume(float volume) {
            //AudioManager.Instance.SetVolume(AudioManager.Instance.MyMasterVolume, volume);
        }

        public void SetMusicVolume(float volume) {
            //AudioManager.Instance.SetVolume(AudioManager.Instance.MyMusicVolume, volume);
        }

        public void SetEffectsVolume(float volume) {
            //AudioManager.Instance.SetVolume(AudioManager.Instance.MyEffectsVolume, volume);
        }

        public void SetAmbientVolume(float volume) {
            //AudioManager.Instance.SetVolume(AudioManager.Instance.MyAmbientVolume, volume);
        }

    }

}