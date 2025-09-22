using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class ScreenshotWizard : ScriptableWizard {

        // Will be a subfolder of Application.dataPath and should start with "/"
        public string parentFolder = "/Screenshots/";
        public string fileName = string.Empty;

        private const string wizardTitle = "Screenshot Wizard";
        private const string indicatorFrame = "Assets/AnyRPG/Core/System/Images/UI/Window/Frame2px.png";
        private Texture frameTexture = null;

        [Header("Size")]
        public bool showSizeIndicator = true;
        public int width = 256;
        public int height = 256;
        private const int borderWidth = 2;

        [Header("Camera")]

        [Tooltip("Set to Depth for images with a transparent background")]
        public CameraClearFlags cameraClearFlags = CameraClearFlags.Depth;
        public Color backgroundColor = new Color32(0, 0, 0, 0);

        [Header("Light")]
        public bool useLight = true;
        public Color lightColor = new Color32(100, 100, 100, 255);
        public float lightIntensity = 2f;

        [Header("Object")]
        public GameObject objectToScreenShot = null;

        private bool originalSceneLighting = false;

        private static ScreenshotWizard openWizard = null;

        [MenuItem("Tools/AnyRPG/Wizard/Screenshot Wizard")]
        static void CreateWizard() {
            if (openWizard == null) {
                openWizard = ScriptableWizard.DisplayWizard<ScreenshotWizard>(wizardTitle, "Create");
            } else {
                openWizard.Focus();
            }
        }

        private void OnEnable() {
            SetSelection();
            Selection.selectionChanged += SetSelection;
            SceneView.duringSceneGui += OnScene;
            frameTexture = AssetDatabase.LoadMainAssetAtPath(indicatorFrame) as Texture;
            if (PlayerPrefs.HasKey("ScreenshotWizardWidth")) {
                width = PlayerPrefs.GetInt("ScreenshotWizardWidth");
            }
            if (PlayerPrefs.HasKey("ScreenshotWizardHeight")) {
                height = PlayerPrefs.GetInt("ScreenshotWizardHeight");
            }
            if (PlayerPrefs.HasKey("ScreenshotWizardLightIntensity")) {
                lightIntensity = PlayerPrefs.GetFloat("ScreenshotWizardLightIntensity");
            }
            if (PlayerPrefs.HasKey("ScreenshotWizardLightColorR")) {
                /*
                lightColor = new Color32(PlayerPrefs.GetFloat("ScreenshotWizardLightColorR"),
                    (byte)PlayerPrefs.GetFloat("ScreenshotWizardLightColorG"),
                    (byte)PlayerPrefs.GetFloat("ScreenshotWizardLightColorB"),
                    (byte)PlayerPrefs.GetFloat("ScreenshotWizardLightColorA"));
                */
                lightColor.r = PlayerPrefs.GetFloat("ScreenshotWizardLightColorR");
                lightColor.g = PlayerPrefs.GetFloat("ScreenshotWizardLightColorG");
                lightColor.b = PlayerPrefs.GetFloat("ScreenshotWizardLightColorB");
                lightColor.a = PlayerPrefs.GetFloat("ScreenshotWizardLightColorA");
            }
        }

        private void OnDisable() {
            Selection.selectionChanged -= SetSelection;
            SceneView.duringSceneGui -= OnScene;
            openWizard = null;
        }

        private void OnScene(SceneView sceneview) {

            if (showSizeIndicator) {

                Handles.BeginGUI();

                // all dimension calculations below are scaled by EditorGUIUtility.pixelsPerPoint
                // this takes windows display scaling into account (eg, scaling everything by 150% on a 4k laptop monitor so things aren't too tiny)
                int sourceX = (int)(((SceneView.currentDrawingSceneView.camera.pixelWidth / 2f) - (width / 2f) - borderWidth) / EditorGUIUtility.pixelsPerPoint);
                int sourceY = (int)(((SceneView.currentDrawingSceneView.camera.pixelHeight / 2f) - (height / 2f) - borderWidth) / EditorGUIUtility.pixelsPerPoint);
                //Debug.Log("sourceX: " + sourceX + " sourceY: " + sourceY + " screenWidth: " + Screen.width + " screenHeight: " + Screen.height + " pixelHeight: " + SceneView.currentDrawingSceneView.camera.pixelHeight + " pixelWidth: " + SceneView.currentDrawingSceneView.camera.pixelWidth);
                GUILayout.BeginArea(new Rect(sourceX, sourceY, (width + (borderWidth * 2)) / EditorGUIUtility.pixelsPerPoint, (height + (borderWidth * 2)) / EditorGUIUtility.pixelsPerPoint), GUIStyle.none);
                GUIStyle gUIStyle = new GUIStyle();
                gUIStyle.normal.background = (Texture2D)frameTexture;
                gUIStyle.border.left = borderWidth;
                gUIStyle.border.right = borderWidth;
                gUIStyle.border.top = borderWidth;
                gUIStyle.border.bottom = borderWidth;
                GUILayout.Box(GUIContent.none, gUIStyle, GUILayout.MinWidth((width + (borderWidth * 2)) / EditorGUIUtility.pixelsPerPoint), GUILayout.MinHeight((height + (borderWidth * 2)) / EditorGUIUtility.pixelsPerPoint));

                GUILayout.EndArea();

                Handles.EndGUI();
            }
        }

        public void SetSelection() {
            objectToScreenShot = Selection.activeGameObject;
            OnWizardUpdate();
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar(wizardTitle, "Creating folders...", 0.3f);

            // Setup folder locations
            string filePath = GetFolder();

            // create missing folders
            WizardUtilities.CreateFolderIfNotExists(filePath);

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar(wizardTitle, "Taking Screenshot...", 0.6f);

            TakeAndSaveSnapshotNew(filePath);

            EditorUtility.DisplayProgressBar(wizardTitle, "Refreshing Asset Database...", 0.9f);

            AssetDatabase.Refresh();

            PlayerPrefs.SetInt("ScreenshotWizardWidth", width);
            PlayerPrefs.SetInt("ScreenshotWizardHeight", height);
            PlayerPrefs.SetFloat("ScreenshotWizardLightIntensity", lightIntensity);
            PlayerPrefs.SetFloat("ScreenshotWizardLightColorR", lightColor.r);
            PlayerPrefs.SetFloat("ScreenshotWizardLightColorG", lightColor.g);
            PlayerPrefs.SetFloat("ScreenshotWizardLightColorB", lightColor.b);
            PlayerPrefs.SetFloat("ScreenshotWizardLightColorA", lightColor.a);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(wizardTitle, wizardTitle + " Complete! The screenshot image can be found at " + filePath, "OK");

        }

        public void TakeAndSaveSnapshotNew(string folderName) {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage == null) {
                Debug.LogError("Please open a prefab in the editor first.");
                return;
            }

            string prefabAssetPath = prefabStage.assetPath;
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);

            if (prefabAsset == null) {
                Debug.LogError($"Could not find prefab asset at path: {prefabAssetPath}");
                return;
            }

            // --- Step 1: Get the SceneView camera settings from the prefab stage ---
            //SceneView prefabStageSceneView = SceneView.sceneViews.Cast<SceneView>().FirstOrDefault(view => view.rootVisualElement != null && prefabRoot.scene.path == view.rootVisualElement.scene.path);

            SceneView prefabStageSceneView = SceneView.lastActiveSceneView;

            Vector3 camPosition = Vector3.zero;
            Quaternion camRotation = Quaternion.identity;
            float camFieldOfView = 60f; // Default if SceneView not found
            bool camIsOrthographic = false;
            float camOrthographicSize = 5f;

            int captureWidth = (int)prefabStageSceneView.camera.pixelRect.width;
            int captureHeight = (int)prefabStageSceneView.camera.pixelRect.height;

            if (prefabStageSceneView != null) {
                camPosition = prefabStageSceneView.camera.transform.position;
                camRotation = prefabStageSceneView.camera.transform.rotation;
                camFieldOfView = prefabStageSceneView.camera.fieldOfView;
                camIsOrthographic = prefabStageSceneView.camera.orthographic;
                camOrthographicSize = prefabStageSceneView.camera.orthographicSize;

            } else {
                Debug.LogWarning("Could not find prefab stage SceneView. Using default camera settings.");
            }

            // 2. Save and store the current scene
            Scene originalScene = EditorSceneManager.GetActiveScene();

            // 3. Create a temporary, empty scene
            Scene tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(tempScene);

            // 4. Clone the prefab root into the temporary scene
            GameObject tempPrefabInstance = GameObject.Instantiate(prefabAsset);

            // 5. Create a temporary camera for rendering
            GameObject tempCameraObject = new GameObject("TempScreenshotCamera");
            Camera tempCamera = tempCameraObject.AddComponent<Camera>();
            if (useLight) {
                Light light = tempCameraObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = lightColor;
                light.intensity = lightIntensity;
            }

            // Apply the saved camera settings
            tempCamera.transform.position = camPosition;
            tempCamera.transform.rotation = camRotation;
            tempCamera.fieldOfView = camFieldOfView;
            tempCamera.orthographic = camIsOrthographic;
            tempCamera.orthographicSize = camOrthographicSize;

            tempCamera.enabled = false;
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = backgroundColor;

            // Disable Post-Processing for the temporary camera
            if (tempCamera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData)) {
                cameraData.renderPostProcessing = false;
            }

            // 6. Render to a RenderTexture
            RenderTexture renderTexture = new RenderTexture(captureWidth, captureHeight, 24, RenderTextureFormat.ARGB32);
            tempCamera.targetTexture = renderTexture;
            tempCamera.Render();

            // 7. Save the image to a transparent PNG
            RenderTexture.active = renderTexture;
            // original screen size
            //Texture2D screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.ARGB32, false);
            // modified screen size
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.ARGB32, false);

            // original screen size
            //screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            // modified screen size
            int sourceX = (captureWidth / 2) - (width / 2);
            int sourceY = (captureHeight / 2) - (width / 2);
            screenShot.ReadPixels(new Rect(sourceX, sourceY, width, height), 0, 0);

            screenShot.Apply();

            string prefabName = Path.GetFileNameWithoutExtension(prefabStage.assetPath);
            //string fileName = Path.Combine(folderPath, prefabName + "_transparent_snapshot.png");
            //byte[] screenshotData = screenShot.EncodeToPNG();
            string screenshotFilename = GetFinalFileName(folderName);
            byte[] screenshotData = screenShot.EncodeToPNG();
            Debug.Log($"Capturing screenshot to file {screenshotFilename}. width: {width}/{captureWidth} Height: {height}/{captureHeight} sourceX: {sourceX} sourceY: {sourceY}");

            File.WriteAllBytes(screenshotFilename, screenshotData);

            // 8. Cleanup
            tempCamera.targetTexture = null;
            RenderTexture.active = null;
            GameObject.DestroyImmediate(renderTexture);
            GameObject.DestroyImmediate(screenShot);
            GameObject.DestroyImmediate(tempCameraObject);
            GameObject.DestroyImmediate(tempPrefabInstance);

            // Restore the original scene
            EditorSceneManager.SetActiveScene(originalScene);
            EditorSceneManager.UnloadSceneAsync(tempScene);

            // 9. Re-open the prefab editor
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath));

            AssetDatabase.Refresh();

            Debug.Log($"Transparent screenshot of prefab '{prefabName}' saved to {fileName}");
        }

        public void TakeAndSaveSnapshotOriginal(string folderName) {

            SceneView view = SceneView.lastActiveSceneView;
            Camera cam = view.camera;
            int captureWidth = (int)cam.pixelRect.width;
            int captureHeight = (int)cam.pixelRect.height;

            RenderTexture renderTexture;
            Rect rect = new Rect(0, 0, captureWidth, captureHeight);
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);

            // set camera settings
            cam.targetTexture = renderTexture;
            //cam.clearFlags = cameraClearFlags;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;
            cam.renderingPath = RenderingPath.Forward;
            cam.cullingMask = ~0;

            // add light for rendering
            Light light = cam.GetComponent<Light>();
            if (light == null && useLight) {
                originalSceneLighting = view.sceneLighting;
                view.sceneLighting = false;

                Undo.RegisterFullObjectHierarchyUndo(objectToScreenShot, "LightParent");

                GameObject lightObject = new GameObject();

                Undo.RegisterCreatedObjectUndo(lightObject, "Created lightObject");

                lightObject.transform.parent = objectToScreenShot.transform;
                lightObject.transform.position = cam.transform.position;
                lightObject.transform.rotation = cam.transform.rotation;
                light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = lightColor;
                light.intensity = lightIntensity;
            }

            cam.Render();

            // read pixels will read from the currently active render texture so make our offscreen 
            // render texture active and then read the pixels
            RenderTexture.active = renderTexture;
            Texture2D screenShot;

            // original screen size
            //screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.ARGB32, false);
            // modified screen size
            screenShot = new Texture2D(width, height, TextureFormat.ARGB32, false);

            // original screen size
            //screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            //modified screen size
            int sourceX = (captureWidth / 2) - (width / 2);
            int sourceY = (captureHeight / 2) - (width / 2);
            screenShot.ReadPixels(new Rect(sourceX, sourceY, width, height), 0, 0);

            screenShot.Apply();

            string screenshotFilename = GetFinalFileName(folderName);
            byte[] screenshotData = screenShot.EncodeToPNG();
            Debug.Log("Capturing screenshot to file " + screenshotFilename + ". width: " + captureWidth + " Height: " + captureHeight + " sourceX: " + sourceX + " sourceY: " + sourceY);

            System.IO.FileStream fStream = System.IO.File.Create(screenshotFilename);
            fStream.Write(screenshotData, 0, screenshotData.Length);
            fStream.Close();

            // clean up light
            if (useLight) {
                view.sceneLighting = originalSceneLighting;
                Undo.PerformUndo();
            }


            // clean up screenshot
            if (Application.isPlaying) {
                GameObject.Destroy(screenShot);
            } else {
                // DestroyImmediate must be used in the editor
                GameObject.DestroyImmediate(screenShot);
            }
        }

        /*
        public void TakeAndSaveSnapshot(string folderName) {

            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) {
                Debug.LogError("No active SceneView found.");
                return;
            }

            // Capture settings from the SceneView camera
            Camera sceneViewCam = view.camera;
            int captureWidth = (int)sceneViewCam.pixelRect.width;
            int captureHeight = (int)sceneViewCam.pixelRect.height;

            // 1. Create a temporary camera for rendering
            GameObject tempCameraObject = new GameObject("TempScreenshotCamera");
            Camera tempCamera = tempCameraObject.AddComponent<Camera>();
            tempCamera.transform.position = sceneViewCam.transform.position;
            tempCamera.transform.rotation = sceneViewCam.transform.rotation;
            tempCamera.fieldOfView = sceneViewCam.fieldOfView;
            tempCamera.orthographic = sceneViewCam.orthographic;
            tempCamera.orthographicSize = sceneViewCam.orthographicSize;

            // Optional: Use a specific culling mask to isolate objects if needed
            // tempCamera.cullingMask = LayerMask.GetMask("YourCaptureLayer");
            tempCamera.cullingMask = ~0;

            // Set camera for transparent background
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent black
            tempCamera.renderingPath = RenderingPath.Forward;
            //tempCamera.backgroundColor = backgroundColor;

            // Ensure no post-processing interferes
            if (tempCamera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData)) {
                cameraData.renderPostProcessing = false;
            }

            RenderTexture renderTexture = new RenderTexture(captureWidth, captureHeight, 24, RenderTextureFormat.ARGB32);
            tempCamera.targetTexture = renderTexture;

            // add light for rendering
            Light light = tempCamera.GetComponent<Light>();
            if (light == null && useLight) {
                originalSceneLighting = view.sceneLighting;
                view.sceneLighting = false;

                Undo.RegisterFullObjectHierarchyUndo(objectToScreenShot, "LightParent");

                GameObject lightObject = new GameObject();

                Undo.RegisterCreatedObjectUndo(lightObject, "Created lightObject");

                lightObject.transform.parent = objectToScreenShot.transform;
                lightObject.transform.position = tempCamera.transform.position;
                lightObject.transform.rotation = tempCamera.transform.rotation;
                light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = lightColor;
                light.intensity = lightIntensity;
                //GameObject go = new GameObject("CameraObject");
                //go.transform.parent = cam.transform;
                //go.transform.position = cam.transform.position;
                //Debug.Log("Cam Position is " + cam.transform.position);
                //go.transform.rotation = cam.transform.rotation;
                //go.transform.parent = null;
                //light = cam.gameObject.AddComponent<Light>();
                //light = lightObject.AddComponent<Light>();
                //light.type = LightType.Directional;
                //light.color = lightColor;
                //light.intensity = lightIntensity;
                //light.gameObject.transform.SetParent(null);
            }

            tempCamera.Render();

            // read pixels will read from the currently active render texture so make our offscreen 
            // render texture active and then read the pixels
            RenderTexture.active = renderTexture;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.ARGB32, false);

            // original
            //screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            //modified
            int sourceX = (captureWidth / 2) - (width / 2);
            int sourceY = (captureHeight / 2) - (width / 2);
            screenShot.ReadPixels(new Rect(sourceX, sourceY, width, height), 0, 0);

            screenShot.Apply();

            string screenshotFilename = GetFinalFileName(folderName);
            byte[] screenshotData = screenShot.EncodeToPNG();
            Debug.Log($"Capturing screenshot to file {screenshotFilename}. width: {captureWidth} Height: {captureHeight} sourceX: {sourceX} sourceY: {sourceY}");

            System.IO.File.WriteAllBytes(screenshotFilename, screenshotData);
            //System.IO.FileStream fStream = System.IO.File.Create(screenshotFilename);
            //fStream.Write(screenshotData, 0, screenshotData.Length);
            //fStream.Close();

            // clean up light
            if (useLight) {
                view.sceneLighting = originalSceneLighting;
                Undo.PerformUndo();
            }

            // Cleanup textures
            tempCamera.targetTexture = null;
            RenderTexture.active = null;

            // clean up screenshot
            if (Application.isPlaying) {
                GameObject.Destroy(screenShot);
            } else {
                // DestroyImmediate must be used in the editor
                GameObject.DestroyImmediate(screenShot);
                GameObject.DestroyImmediate(renderTexture);
                GameObject.DestroyImmediate(screenShot);
                GameObject.DestroyImmediate(tempCameraObject);
            }
        }
        */

        public void TakeAndSaveSnapshot(string folderName) {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage == null) {
                Debug.LogError("No prefab stage is currently open. Please open a prefab in the editor first.");
                return;
            }

            GameObject prefabRoot = prefabStage.prefabContentsRoot;

            // Get the Scene View for the prefab stage by checking each view's title
            //SceneView prefabStageSceneView = SceneView.sceneViews.Cast<SceneView>().FirstOrDefault(view => view.titleContent.text.Contains(prefabStage.assetPath));
            //SceneView prefabStageSceneView = null;
            //foreach (SceneView view in SceneView.sceneViews) {
            //    if (view.scene == prefabStage.scene) {
            //        prefabStageSceneView = view;
            //        break;
            //    }
            //}
            SceneView prefabStageSceneView = SceneView.lastActiveSceneView;
            if (prefabStageSceneView == null) {
                Debug.LogError("No active SceneView found.");
                return;
            }

            // Capture settings from the SceneView camera
            //Camera sceneViewCam = view.camera;
            /*
            foreach (SceneView view in SceneView.sceneViews) {
                Debug.Log($"Checking SceneView with title: {view.titleContent.text} {view.name} {view.rootVisualElement.name}");
                // Check if the SceneView's camera can see the prefab root
                if (view.camera != null && IsVisible(view.camera, prefabRoot)) {
                    Debug.Log($"Found sceneview where prefabRoot is visible: {view.titleContent.text} {view.name} {view.rootVisualElement.name}");
                    prefabStageSceneView = view;
                    break;
                }
            }
            */

            if (prefabStageSceneView == null) {
                Debug.LogError("Could not find the scene view for the current prefab stage.");
                return;
            }

            Camera cam = prefabStageSceneView.camera;

            if (prefabRoot == null) {
                Debug.LogError("Could not find the root of the prefab contents.");
                return;
            }

            // Capture settings from the SceneView camera
            int captureWidth = (int)cam.pixelRect.width;
            int captureHeight = (int)cam.pixelRect.height;

            originalSceneLighting = prefabStageSceneView.sceneLighting;
            prefabStageSceneView.sceneLighting = false;

            Undo.RegisterFullObjectHierarchyUndo(objectToScreenShot, "LightParent");

            GameObject lightObject = new GameObject();

            Undo.RegisterCreatedObjectUndo(lightObject, "Created lightObject");

            lightObject.transform.parent = objectToScreenShot.transform;
            lightObject.transform.position = cam.transform.position;
            lightObject.transform.rotation = cam.transform.rotation;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = lightColor;
            light.intensity = lightIntensity;
            
            // 1. Create a temporary camera for rendering
            //GameObject tempCameraObject = new GameObject("TempScreenshotCamera");
            Camera tempCamera = lightObject.AddComponent<Camera>();
            //tempCamera.transform.position = cam.transform.position;
            //tempCamera.transform.rotation = cam.transform.rotation;
            tempCamera.fieldOfView = cam.fieldOfView;
            tempCamera.orthographic = cam.orthographic;
            tempCamera.orthographicSize = cam.orthographicSize;

            // Optional: Use a specific culling mask to isolate objects if needed
            // tempCamera.cullingMask = LayerMask.GetMask("YourCaptureLayer");

            // Set camera for transparent background
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent black

            // Ensure no post-processing interferes
            if (tempCamera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData)) {
                cameraData.renderPostProcessing = false;
            }

            // 2. Create a RenderTexture with ARGB32 format for alpha
            RenderTexture renderTexture = new RenderTexture(captureWidth, captureHeight, 24, RenderTextureFormat.ARGB32);
            tempCamera.targetTexture = renderTexture;

            // 3. Render the camera to the texture
            tempCamera.Render();

            // 4. Read pixels and save as PNG
            RenderTexture.active = renderTexture;
            Texture2D screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.ARGB32, false);
            screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            screenShot.Apply();

            byte[] screenshotData = screenShot.EncodeToPNG();
            string screenshotFilename = GetFinalFileName(folderName);
            System.IO.File.WriteAllBytes(screenshotFilename, screenshotData);

            // 5. Cleanup
            tempCamera.targetTexture = null;
            RenderTexture.active = null;
            GameObject.DestroyImmediate(renderTexture);
            GameObject.DestroyImmediate(screenShot);
            //GameObject.DestroyImmediate(lightObject);

            Debug.Log($"Screenshot saved to {screenshotFilename}");
        }


        public string GetFinalFileName(string folderName) {
            if (fileName == string.Empty) {
                return folderName + "/" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".png";
            }
            if (System.IO.File.Exists(folderName + "/" + fileName + ".png")) {
                return folderName + "/" + fileName + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".png";
            } else {
                return folderName + "/" + fileName + ".png";
            }
        }

        void OnWizardUpdate() {
            helpString = "Creates a screenshot image of the currently loaded scene";
            errorString = Validate();
            SetFileName();
            isValid = (errorString == null || errorString == "");
        }

        void SetFileName() {
            if (objectToScreenShot != null && fileName == string.Empty) {
                fileName = objectToScreenShot.name;
            }
        }

        string GetFolder() {
            return Application.dataPath + parentFolder;
        }

        string Validate() {
            if (parentFolder == null || parentFolder.Trim() == "") {
                return "Parent Folder name must not be empty";
            }
            if (objectToScreenShot == null) {
                return "A GameObject in the scene must be selected";
            }

            return null;
        }

        public static void DisplayProgressBar(string title, string info, float progress) {
            EditorUtility.DisplayProgressBar(title, info, progress);
        }

        private static bool IsVisible(Camera camera, GameObject go) {
            // Check if any part of the object's bounds is within the camera's frustum
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return false;

            foreach (Renderer r in renderers) {
                if (IsRendererVisible(camera, r)) {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRendererVisible(Camera camera, Renderer renderer) {
            // A simple check to see if the renderer's bounds are within the camera's frustum
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }

    }

}