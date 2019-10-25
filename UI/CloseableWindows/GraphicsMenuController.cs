using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace AnyRPG {
public class GraphicsMenuController : WindowContentController {

    public override event Action<ICloseableWindowContents> OnOpenWindow;

    [SerializeField]
    private Dropdown resolutionDropDown;

    [SerializeField]
    private Dropdown qualityDropDown;

    private Resolution[] resolutions;

    private void Start() {
        //Debug.Log("KeyBindMenuController.Start()");
        InitializeSettings();
    }

    private void InitializeSettings() {
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

        // set correct quality on quality dropdown
        qualityDropDown.value = QualitySettings.GetQualityLevel();
    }

    public void SetQuality (int qualityIndex) {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullScreen(bool isFullScreen) {
        Screen.fullScreen = isFullScreen;
    }

    public void SetResolution (int resolutionIndex) {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

}

}