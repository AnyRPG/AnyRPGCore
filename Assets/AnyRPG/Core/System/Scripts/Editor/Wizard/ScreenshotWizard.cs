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

        // the used file path name for the game
        //private string fileSystemGameName = string.Empty;

        // user modified variables
        //public string gameName = "";
        //public int pixelsPerMeter = 10;
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

        [MenuItem("Tools/AnyRPG/Wizard/Screenshot Wizard")]
        static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ScreenshotWizard>(wizardTitle, "Create");
        }

        private void OnEnable() {
            SetSelection();
            Selection.selectionChanged += SetSelection;
        }

        private void OnDisable() {
            Selection.selectionChanged -= SetSelection;
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

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(wizardTitle, wizardTitle +" Complete! The screenshot image can be found at " + filePath, "OK");

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
            screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.ARGB32, false);
            screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            screenShot.Apply();

            string screenshotFilename = GetFinalFileName(folderName);
            byte[] screenshotData = screenShot.EncodeToPNG();
            Debug.Log("Capturing screenshot to file " + screenshotFilename + ". width: " + captureWidth + " Height: " + captureHeight);

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
            isValid = (errorString == null || errorString == "");
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

        private void ShowError(string message) {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }

        public static void DisplayProgressBar(string title, string info, float progress) {
            EditorUtility.DisplayProgressBar(title, info, progress);
        }

    }

}
