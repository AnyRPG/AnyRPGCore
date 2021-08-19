using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class MapManager : ConfiguredMonoBehaviour {

        Texture2D mapTexture = null;
        private bool sceneTextureFound = false;

        private const string mapTextureFolderBase = "Assets/Games/";
        private string mapTextureFolder = string.Empty;

        // game manager references
        private LevelManager levelManager = null;
        private SystemConfigurationManager systemConfigurationManager = null;
        private CameraManager cameraManager = null;

        public Texture2D MapTexture { get => mapTexture; }
        public bool SceneTextureFound { get => sceneTextureFound; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            cameraManager.MainMapCamera.enabled = false;
            mapTextureFolder = mapTextureFolderBase + systemConfigurationManager.GameName.Replace(" ", "") + "/Images/MiniMap/";
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            levelManager = systemGameManager.LevelManager;
            cameraManager = systemGameManager.CameraManager;
        }

        public void ProcessLevelLoad() {
            mapTexture = new Texture2D((int)levelManager.SceneBounds.size.x, (int)levelManager.SceneBounds.size.z);
            string textureFilePath = mapTextureFolder + GetScreenshotFilename();
            if (System.IO.File.Exists(textureFilePath)) {
                sceneTextureFound = true;
                byte[] fileData = System.IO.File.ReadAllBytes(textureFilePath);
                mapTexture.LoadImage(fileData);
            }
        }

        public void ProcessLevelUnload() {
            mapTexture = null;
            sceneTextureFound = false;
        }

        /// <summary>
        /// Return the standardized name of the minimap image file
        /// </summary>
        /// <returns></returns>
        public string GetScreenshotFilename() {
            return SceneManager.GetActiveScene().name + ".png";
        }


    }

}