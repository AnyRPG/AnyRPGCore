using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace AnyRPG {
    public class LoadScreenManager : ConfiguredMonoBehaviour {

        [Header("Loading Screen")]

        public Slider loadBar;
        public TextMeshProUGUI finishedLoadingText;
        public Image backgroundImage = null;

        // game manager references
        private LevelManagerClient levelManagerClient = null;
        private UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            levelManagerClient.OnBeginLoadingLevel += HandleBeginLoadingLevel;
            levelManagerClient.OnSetLoadingProgress += HandleSetLoadingProgress;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            levelManagerClient = systemGameManager.LevelManagerClient;
            uIManager = systemGameManager.UIManager;
        }

        public void HandleBeginLoadingLevel(string sceneName) {
            uIManager.ActivateLoadingUI();

            if (levelManagerClient.LoadingSceneNode != null && levelManagerClient.LoadingSceneNode.LoadingScreenImage != null) {
                backgroundImage.sprite = levelManagerClient.LoadingSceneNode.LoadingScreenImage;
                backgroundImage.color = Color.white;
            } else {
                backgroundImage.sprite = null;
                backgroundImage.color = Color.black;
            }
        }

        private void HandleSetLoadingProgress(float newProgress) {
            if (loadBar != null) {
                loadBar.value = newProgress;
            }
        }

        public void HandleEndLoadingLevel() { 
        }

    }

}