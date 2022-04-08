using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ScreenshotWizard : ScriptableWizard {

        // Will be a subfolder of Application.dataPath and should start with "/"
        //private const string newGameParentFolder = "/Games/";
        public string parentFolder = "/Screenshots/";
        public string fileName = string.Empty;
        //private const string imagesFolder = "Images/Screenshot";

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
        public Color backgroundColor = Color.black;

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

                int sourceX = (SceneView.currentDrawingSceneView.camera.pixelWidth / 2) - (width / 2) - borderWidth;
                int sourceY = (SceneView.currentDrawingSceneView.camera.pixelHeight / 2) - (height / 2) - borderWidth;
                //Debug.Log("sourceX: " + sourceX + " sourceY: " + sourceY + " screenWidth: " + Screen.width + " screenHeight: " + Screen.height + " pixelHeight: " + SceneView.currentDrawingSceneView.camera.pixelHeight + " pixelWidth: " + SceneView.currentDrawingSceneView.camera.pixelWidth);
                GUILayout.BeginArea(new Rect(sourceX, sourceY, width + (borderWidth * 2), height + (borderWidth * 2)), GUIStyle.none);
                GUIStyle gUIStyle = new GUIStyle();
                gUIStyle.normal.background = (Texture2D)frameTexture;
                gUIStyle.border.left = borderWidth;
                gUIStyle.border.right = borderWidth;
                gUIStyle.border.top = borderWidth;
                gUIStyle.border.bottom = borderWidth;
                GUILayout.Box(GUIContent.none, gUIStyle, GUILayout.MinWidth(width + (borderWidth * 2)), GUILayout.MinHeight(height + (borderWidth * 2)));

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

            TakeAndSaveSnapshot(filePath);

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

        public void TakeAndSaveSnapshot(string folderName) {

            SceneView view = SceneView.lastActiveSceneView;
            Camera cam = view.camera;
            int captureWidth = (int)cam.pixelRect.width;
            int captureHeight = (int)cam.pixelRect.height;

            RenderTexture renderTexture;
            Rect rect = new Rect(0, 0, captureWidth, captureHeight);
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);

            // set camera settings
            cam.targetTexture = renderTexture;
            cam.clearFlags = cameraClearFlags;
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
                /*
                //GameObject go = new GameObject("CameraObject");
                go.transform.parent = cam.transform;
                go.transform.position = cam.transform.position;
                Debug.Log("Cam Position is " + cam.transform.position);
                go.transform.rotation = cam.transform.rotation;
                go.transform.parent = null;
                */
                //light = cam.gameObject.AddComponent<Light>();
                //light = lightObject.AddComponent<Light>();
                /*
                light.type = LightType.Directional;
                light.color = lightColor;
                light.intensity = lightIntensity;
                */
                //light.gameObject.transform.SetParent(null);
            }

            cam.Render();

            // read pixels will read from the currently active render texture so make our offscreen 
            // render texture active and then read the pixels
            RenderTexture.active = renderTexture;
            Texture2D screenShot;

            // original
            //screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.ARGB32, false);
            // modified
            screenShot = new Texture2D(width, height, TextureFormat.ARGB32, false);

            // original
            //screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            //modified
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

    }

}