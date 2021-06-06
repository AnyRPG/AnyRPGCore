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
            float rawValue = AudioManager.MyInstance.GetVolume(AudioManager.MyInstance.MasterVolume);
            float adjustedValue = Mathf.Pow(10, (rawValue / 20));
            //Debug.Log("SoundMenuController.Start() adjusted value: " + adjustedValue);
            masterVolumeSlider.value = adjustedValue;
        }

        public void SetMasterVolume(float volume) {
            //AudioManager.MyInstance.SetVolume(AudioManager.MyInstance.MyMasterVolume, volume);
        }

        public void SetMusicVolume(float volume) {
            //AudioManager.MyInstance.SetVolume(AudioManager.MyInstance.MyMusicVolume, volume);
        }

        public void SetEffectsVolume(float volume) {
            //AudioManager.MyInstance.SetVolume(AudioManager.MyInstance.MyEffectsVolume, volume);
        }

        public void SetAmbientVolume(float volume) {
            //AudioManager.MyInstance.SetVolume(AudioManager.MyInstance.MyAmbientVolume, volume);
        }

    }

}