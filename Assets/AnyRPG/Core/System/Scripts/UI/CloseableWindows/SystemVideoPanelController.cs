using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemVideoPanelController : WindowContentController {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        [Header("Video Panel")]

        [SerializeField]
        private TMP_Dropdown graphicsQualityDropdown = null;

        [SerializeField]
        private TMP_Dropdown resolutionDropDown = null;

        /*
        [SerializeField]
        private TMP_Text graphicsDescription = null;
        */

        private Resolution[] resolutions;
        private string[] graphicsQualities;

        [Header("VIDEO SETTINGS")]
        public OnOffTextButton fullScreenButton;
        public OnOffTextButton vSyncButton;
        //public OnOffTextButton ambientOcclusionButton;
        //public OnOffTextButton motionBlurButton;
        //public OnOffTextButton cameraEffectsButton;
        public TextOptionHighlightArea shadowQualityArea;
        public TextOptionHighlightArea textureQualityArea;
        //public TextOptionHighlightArea graphicsQualityArea;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //fullScreenButton.Configure(systemGameManager);
            //vSyncButton.Configure(systemGameManager);
            //shadowQualityArea.Configure(systemGameManager);
            //textureQualityArea.Configure(systemGameManager);
            //graphicsQualityArea.Configure(systemGameManager);
            SetPlayerPrefsDefaults();
            InitializeSettings();
        }

        /*
        public void OnEnable() {
            InitializeSettings();
        }
        */

        private void SetPlayerPrefsDefaults() {

            if (!PlayerPrefs.HasKey("GraphicsQualityIndex")) {
                //Debug.Log("MainSettingsMenuController.SetPlayerPrefsDefaults() graphicsQuality is: " + QualitySettings.GetQualityLevel());
                PlayerPrefs.SetInt("GraphicsQualityIndex", QualitySettings.GetQualityLevel());
            }

            if (!PlayerPrefs.HasKey("FullScreen")) {
                PlayerPrefs.SetInt("FullScreen", (Screen.fullScreen == true ? 1 : 0));
            }

            if (!PlayerPrefs.HasKey("VSyncValue")) {
                PlayerPrefs.SetInt("VSyncValue", QualitySettings.vSyncCount);
            }

            if (!PlayerPrefs.HasKey("Textures")) {
                PlayerPrefs.SetInt("Textures", 2);
            }

            if (!PlayerPrefs.HasKey("Shadows")) {
                PlayerPrefs.SetInt("Shadows", 2);
            }

        }

        private void InitializeSettings() {

            CheckScreenResolution();

            // set graphics quality to saved value, and then update the dropdown
            SetGraphicsQuality(PlayerPrefs.GetInt("GraphicsQualityIndex"));
            CheckGraphicsQuality();

            CheckFullScreen();
            CheckVSync();

            // check and set advanced settings which may override main quality setting
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

            graphicsQualities = QualitySettings.names;

            graphicsQualityDropdown.ClearOptions();

            List<string> options = new List<string>();

            for (int i = 0; i < graphicsQualities.Length; i++) {
                options.Add(graphicsQualities[i]);
            }
            graphicsQualityDropdown.AddOptions(options);
            //graphicsQualityDropdown.value = currentResolutionIndex;
            graphicsQualityDropdown.value = PlayerPrefs.GetInt("GraphicsQualityIndex");
            graphicsQualityDropdown.RefreshShownValue();

            UpdateGraphicsDescription();
        }

        public void SetGraphicsQuality(int qualityIndex) {
            //Debug.Log("SystemVideoPanelController.SetGraphicsQuality(" + qualityIndex + ")");
            PlayerPrefs.SetInt("GraphicsQualityIndex", qualityIndex);
            QualitySettings.SetQualityLevel(qualityIndex, true);
            //CheckGraphicsQuality();
            UpdateGraphicsDescription();
        }

        public void UpdateGraphicsDescription() {

            //QualitySettings.
        }

        private void CheckFullScreen() {
            if (PlayerPrefs.GetInt("FullScreen") == 1) {
                fullScreenButton.SetOn();
            } else if (PlayerPrefs.GetInt("FullScreen") == 0) {
                fullScreenButton.SetOff();
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
            //Debug.Log("SystemVideoPanelController.CheckVSync()");
            if (PlayerPrefs.GetInt("VSyncValue") == 0) {
                vSyncButton.SetOff();
                QualitySettings.vSyncCount = 0;
            } else if (PlayerPrefs.GetInt("VSyncValue") == 1) {
                vSyncButton.SetOn();
                QualitySettings.vSyncCount = 1;
            }
        }

        public void ToggleVSync() {
            //Debug.Log("SystemVideoPanelController.ToggleVSync()");
            if (QualitySettings.vSyncCount == 0) {
                PlayerPrefs.SetInt("VSyncValue", 1);
            } else if (QualitySettings.vSyncCount == 1) {
                PlayerPrefs.SetInt("VSyncValue", 0);
            }
            CheckVSync();
        }


        public void CheckTextureQuality() {
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