using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class MapManager : ConfiguredMonoBehaviour {

        public event System.Action OnInitializeMap = delegate { };

        Texture2D mapTexture = null;
        RenderTexture renderTexture = null;
        private bool sceneTextureFound = false;

        // saved render of map
        private const string mapTextureFolderBase = "Assets/Games/";
        private string mapTextureFolder = string.Empty;

        // fallback render of map
        private float cameraSize = 0f;


        // game manager references
        private LevelManagerClient levelManagerClient = null;
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

            levelManagerClient = systemGameManager.LevelManagerClient;
            cameraManager = systemGameManager.CameraManager;
        }

        public void ProcessLevelLoad() {
            //Debug.Log("MapManager.ProcessLevelLoad()");

            //Debug.Log("MapManager.ProcessLevelLoad(): creating Texture2D with size : " + (int)levelManager.SceneBounds.size.x + ", " + (int)levelManager.SceneBounds.size.z);
            UpdateCameraSize();

            if (cameraSize == 0f) {
                // something went wrong and there is nothing in the level
                return;
            }

            //mapTexture = new Texture2D((int)levelManager.SceneBounds.size.x, (int)levelManager.SceneBounds.size.z);
            string textureFilePath = mapTextureFolder + GetScreenshotFilename();
            if (System.IO.File.Exists(textureFilePath)) {
                mapTexture = new Texture2D((int)cameraSize, (int)cameraSize);
                sceneTextureFound = true;
                byte[] fileData = System.IO.File.ReadAllBytes(textureFilePath);
                mapTexture.LoadImage(fileData);
                OnInitializeMap();
            } else {
                sceneTextureFound = true;
                //UpdateCameraSize();
                UpdateCameraPosition();
                StartCoroutine(WaitForRender());
            }
        }

        /*
        public void RenderMapFromCamera() {
            //Debug.Log("MapManager.RenderMapFromCamera()");
            renderTexture = new RenderTexture((int)levelManager.SceneBounds.size.x, (int)levelManager.SceneBounds.size.z, 16, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            cameraManager.MainMapCamera.Render();

            mapTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            mapTexture.Apply();
        }
        */

        /// <summary>
        /// wait until end of frame and then render image of map
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitForRender() {
            //Debug.Log("MapManager.WaitForRender()");

            yield return new WaitForEndOfFrame();
            //Debug.Log("MapManager.WaitForRender(): rendering now");
            //yield return null;
            //Debug.Log("MapManager.WaitForRender(): rendering now with scene size : " + (int)levelManager.SceneBounds.size.x + ", " + (int)levelManager.SceneBounds.size.z);

            //RenderMapFromCamera();
            //renderTexture = new RenderTexture((int)levelManager.SceneBounds.size.x, (int)levelManager.SceneBounds.size.z, 16, RenderTextureFormat.ARGB32);
            mapTexture = new Texture2D((int)cameraSize * systemConfigurationManager.UIConfiguration.AutoPixelsPerMeter, (int)cameraSize * systemConfigurationManager.UIConfiguration.AutoPixelsPerMeter);
            renderTexture = new RenderTexture((int)cameraSize * systemConfigurationManager.UIConfiguration.AutoPixelsPerMeter, (int)cameraSize * systemConfigurationManager.UIConfiguration.AutoPixelsPerMeter, 16, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            cameraManager.MainMapCamera.targetTexture = renderTexture;


            float originalLodBias = QualitySettings.lodBias;
            int originalMaximumLod = QualitySettings.maximumLODLevel;
            
            // Temporarily set LOD settings to force LOD 0 for all cameras
            QualitySettings.lodBias = 100f; // A high value to force the highest LOD at any distance
            QualitySettings.maximumLODLevel = 0; // The highest detail LOD

            cameraManager.MainMapCamera.Render();

            // Restore original LOD settings
            QualitySettings.lodBias = originalLodBias;
            QualitySettings.maximumLODLevel = originalMaximumLod;


            //yield return new WaitForEndOfFrame();

            RenderTexture.active = renderTexture;
            //Debug.Log("MapManager.WaitForRender(): rendering texture is of size : " + renderTexture.width + ", " + renderTexture.height);
            mapTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            //mapTexture.LoadRawTextureData(mapTexture.GetRawTextureData());
            mapTexture.Apply();
            OnInitializeMap();
        }

        private void UpdateCameraSize() {
            //Debug.Log("MainMapController.UpdateCameraSize()");
            //float newCameraSize = cameraSizeDefault;
            cameraSize = Mathf.Max(levelManagerClient.SceneBounds.extents.x, levelManagerClient.SceneBounds.extents.z);
            cameraManager.MainMapCamera.orthographicSize = cameraSize;
        }

        private void UpdateCameraPosition() {
            //Debug.Log("MainMapController.UpdateCameraPosition()");
            Vector3 wantedPosition = new Vector3(levelManagerClient.SceneBounds.center.x, levelManagerClient.SceneBounds.center.y + levelManagerClient.SceneBounds.extents.y + 1f, levelManagerClient.SceneBounds.center.z);
            //Debug.Log("MainMapController.UpdateCameraPosition() wantedposition: " + wantedPosition);
            Vector3 wantedLookPosition = new Vector3(levelManagerClient.SceneBounds.center.x, levelManagerClient.SceneBounds.center.y, levelManagerClient.SceneBounds.center.z);
            //Debug.Log("MainMapController.UpdateCameraPosition() wantedLookPosition: " + wantedLookPosition);
            cameraManager.MainMapCamera.transform.position = wantedPosition;
            cameraManager.MainMapCamera.transform.LookAt(wantedLookPosition);
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