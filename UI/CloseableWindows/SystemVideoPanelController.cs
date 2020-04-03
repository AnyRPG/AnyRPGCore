using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemVideoPanelController : WindowContentController {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        [SerializeField]
        private Dropdown resolutionDropDown = null;

        //[SerializeField]
        //private Dropdown qualityDropDown = null;

        private Resolution[] resolutions;

        [Header("VIDEO SETTINGS")]
        public OnOffTextButton fullScreenButton;
        public OnOffTextButton vSyncButton;
        //public OnOffTextButton ambientOcclusionButton;
        //public OnOffTextButton motionBlurButton;
        //public OnOffTextButton cameraEffectsButton;
        public TextOptionHighlightArea shadowQualityArea;
        public TextOptionHighlightArea textureQualityArea;
        public TextOptionHighlightArea graphicsQualityArea;

        private void Start() {
            //Debug.Log("SystemVideoPanelController.Start()");
            InitializeSettings();
        }

        public void OnEnable() {
            InitializeSettings();
        }

        private void InitializeSettings() {

            CheckScreenResolution();
            CheckGraphicsQuality();
            CheckFullScreen();
            CheckVSync();
            CheckAdvancedVideoSettings();
        }

        public void CheckScreenResolution() {

            resolutions = Screen.resolutions;

            resolutionDropDown.ClearOptions();

            List<string> options = new List<string>();

            int currentResolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++) {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height) {
                    currentResolutionIndex = i;
                }
            }
            resolutionDropDown.AddOptions(options);
            resolutionDropDown.value = currentResolutionIndex;
            resolutionDropDown.RefreshShownValue();
        }

        private void CheckGraphicsQuality() {
            if (!PlayerPrefs.HasKey("GraphicsQualityIndex")) {
                PlayerPrefs.SetInt("GraphicsQualityIndex", QualitySettings.GetQualityLevel());
            }
            if (PlayerPrefs.GetInt("GraphicsQualityIndex") == 0) {
                graphicsQualityArea.SelectButton(0);
            } else if (PlayerPrefs.GetInt("GraphicsQualityIndex") == 1) {
                graphicsQualityArea.SelectButton(1);
            } else if (PlayerPrefs.GetInt("GraphicsQualityIndex") == 2) {
                graphicsQualityArea.SelectButton(2);
            }
        }

        public void SetGraphicsQualityLow() {
            PlayerPrefs.SetInt("GraphicsQualityIndex", 0);
            SetGraphicsQuality(0);
            CheckGraphicsQuality();
        }

        public void SetGraphicsQualityMed() {
            PlayerPrefs.SetInt("GraphicsQualityIndex", 1);
            SetGraphicsQuality(1);
            CheckGraphicsQuality();
        }

        public void SetGraphicsQualityHigh() {
            PlayerPrefs.SetInt("GraphicsQualityIndex", 2);
            SetGraphicsQuality(2);
            CheckGraphicsQuality();
        }

        public void SetGraphicsQuality(int qualityIndex) {
            QualitySettings.SetQualityLevel(qualityIndex);
        }

        private void CheckFullScreen() {
            if (!PlayerPrefs.HasKey("FullScreen")) {
                PlayerPrefs.SetInt("FullScreen", (Screen.fullScreen == true ? 1 : 0));
            }
            if (PlayerPrefs.GetInt("FullScreen") == 1) {
                fullScreenButton.Select();
            } else if (PlayerPrefs.GetInt("FullScreen") == 0) {
                fullScreenButton.DeSelect();
            }
        }

        public void SetFullScreen(bool isFullScreen) {
            PlayerPrefs.SetInt("FullScreen", (isFullScreen == true ? 1 : 0));
            Screen.fullScreen = isFullScreen;
            CheckFullScreen();
        }

        public void ToggleFullScreen() {
            Screen.fullScreen = !Screen.fullScreen;
            PlayerPrefs.SetInt("FullScreen", (Screen.fullScreen == true ? 1 : 0));
            CheckFullScreen();
        }

        public void SetResolution(int resolutionIndex) {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        public void CheckVSync() {
            if (!PlayerPrefs.HasKey("VSyncValue")) {
                PlayerPrefs.SetInt("VSyncValue", QualitySettings.vSyncCount);
            }
            if (PlayerPrefs.GetInt("VSyncValue") == 0) {
                vSyncButton.DeSelect();
            } else if (PlayerPrefs.GetInt("VSyncValue") == 1) {
                vSyncButton.Select();
            }

        }

        public void ToggleVSync() {
            if (QualitySettings.vSyncCount == 0) {
                PlayerPrefs.SetInt("VSyncValue", 1);
                QualitySettings.vSyncCount = 1;
            } else if (QualitySettings.vSyncCount == 1) {
                PlayerPrefs.SetInt("VSyncValue", 0);
                QualitySettings.vSyncCount = 0;
            }
            CheckVSync();
        }


        public void CheckTextureQuality() {
            if (!PlayerPrefs.HasKey("Textures")) {
                PlayerPrefs.SetInt("Textures", 2);
            }
            if (PlayerPrefs.GetInt("Textures") == 0) {
                QualitySettings.masterTextureLimit = 2;
                textureQualityArea.SelectButton(0);
            } else if (PlayerPrefs.GetInt("Textures") == 1) {
                QualitySettings.masterTextureLimit = 1;
                textureQualityArea.SelectButton(1);
            } else if (PlayerPrefs.GetInt("Textures") == 2) {
                QualitySettings.masterTextureLimit = 0;
                textureQualityArea.SelectButton(2);
            }
        }

        public void CheckAdvancedVideoSettings() {
            CheckShadowQuality();
            CheckTextureQuality();
        }

        public void CheckShadowQuality() {
            if (!PlayerPrefs.HasKey("Shadows")) {
                PlayerPrefs.SetInt("Shadows", 2);
            }
            if (PlayerPrefs.GetInt("Shadows") == 0) {
                QualitySettings.shadowCascades = 0;
                QualitySettings.shadowDistance = 0;
                shadowQualityArea.SelectButton(0);
            } else if (PlayerPrefs.GetInt("Shadows") == 1) {
                QualitySettings.shadowCascades = 2;
                QualitySettings.shadowDistance = 75;
                shadowQualityArea.SelectButton(1);
            } else if (PlayerPrefs.GetInt("Shadows") == 2) {
                QualitySettings.shadowCascades = 4;
                QualitySettings.shadowDistance = 500;
                shadowQualityArea.SelectButton(2);
            }
        }

        public void ShadowsOff() {
            PlayerPrefs.SetInt("Shadows", 0);
            CheckShadowQuality();
        }

        public void ShadowsLow() {
            PlayerPrefs.SetInt("Shadows", 1);
            CheckShadowQuality();
        }

        public void ShadowsHigh() {
            PlayerPrefs.SetInt("Shadows", 2);
            CheckShadowQuality();
        }

        public void TexturesLow() {
            PlayerPrefs.SetInt("Textures", 0);
            QualitySettings.masterTextureLimit = 2;
            CheckTextureQuality();
        }

        public void TexturesMed() {
            PlayerPrefs.SetInt("Textures", 1);
            QualitySettings.masterTextureLimit = 1;
            CheckTextureQuality();
        }

        public void TexturesHigh() {
            PlayerPrefs.SetInt("Textures", 2);
            QualitySettings.masterTextureLimit = 0;
            CheckTextureQuality();
        }

       
    }

}