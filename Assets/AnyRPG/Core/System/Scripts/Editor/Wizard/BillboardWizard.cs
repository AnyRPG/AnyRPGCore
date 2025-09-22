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
using static UnityEngine.GraphicsBuffer;

namespace AnyRPG {
    public class BillboardWizard : ScriptableWizard {

        // Will be a subfolder of Application.dataPath and should start with "/"
        public string parentFolder = "Screenshots";
        public string fileName = string.Empty;

        private const string wizardTitle = "Billboard Wizard";
        private const string indicatorFrame = "Assets/AnyRPG/Core/System/Images/UI/Window/Frame2px.png";
        private Texture frameTexture = null;

        [Header("Size")]
        public bool showSizeIndicator = true;
        public int width = 256;
        public int height = 256;
        private const int borderWidth = 2;

        [Header("Light")]
        public bool useLight = true;
        public Color lightColor = new Color32(100, 100, 100, 255);
        public float lightIntensity = 2f;

        private static BillboardWizard openWizard = null;

        [MenuItem("Tools/AnyRPG/Wizard/Billboard Wizard")]
        static void CreateWizard() {
            if (openWizard == null) {
                openWizard = ScriptableWizard.DisplayWizard<BillboardWizard>(wizardTitle, "Create");
            } else {
                openWizard.Focus();
            }
        }

        private void OnEnable() {
            SetSelection();
            //Selection.selectionChanged += SetSelection;
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
            //Selection.selectionChanged -= SetSelection;
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
            OnWizardUpdate();
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar(wizardTitle, "Creating folders...", 0.1f);

            // Setup folder locations
            string filePath = GetFolder();

            // create missing folders
            WizardUtilities.CreateFolderIfNotExists(filePath);

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar(wizardTitle, "Taking Screenshot...", 0.2f);

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
            EditorUtility.DisplayDialog(wizardTitle, $"{wizardTitle} Complete! The billboard components can be found at {filePath}", "OK");

        }

        public void TakeAndSaveSnapshotNew(string folderName) {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage == null) {
                Debug.LogError("Please open a prefab in the editor first.");
                return;
            }

            Collider collider = prefabStage.prefabContentsRoot.GetComponentInChildren<Collider>();
            float billboardHeight = 10f; // default height if no collider found
            if (collider != null) {
                billboardHeight = collider.bounds.size.y;
            }

            string prefabAssetPath = prefabStage.assetPath;
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);

            if (prefabAsset == null) {
                Debug.LogError($"Could not find prefab asset at path: {prefabAssetPath}");
                return;
            }

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
            string originalScenePath = GetOriginalScenePath();

            // 3. Create a temporary, empty scene
            EditorUtility.DisplayProgressBar(wizardTitle, "Loading Temporary Scene...", 0.3f);
            LoadTempScene();


            // 4. Clone the prefab root into the temporary scene
            EditorUtility.DisplayProgressBar(wizardTitle, "Cloning prefab into temporary Scene...", 0.4f);
            GameObject tempPrefabInstance = GameObject.Instantiate(prefabAsset);

            // 5. Create a temporary camera for rendering
            EditorUtility.DisplayProgressBar(wizardTitle, "Create Temporary Camera...", 0.5f);
            Camera tempCamera = CreateTempCamera(camPosition, camRotation, camFieldOfView, camIsOrthographic, camOrthographicSize);

            // 6. Render to a RenderTexture
            RenderTexture renderTexture = new RenderTexture(captureWidth, captureHeight, 24, RenderTextureFormat.ARGB32);
            tempCamera.targetTexture = renderTexture;

            RotateToFront(tempCamera, tempPrefabInstance.transform);
            List<Texture2D> capturedImages = new List<Texture2D>();
            for (int i = 0; i < 8; i++) {
                EditorUtility.DisplayProgressBar(wizardTitle, $"Rendering Image {i}...", (5f+((i+1)/4f)));
                capturedImages.Add(RenderOneImage(tempCamera, captureWidth, captureHeight, i, folderName));
                tempCamera.transform.RotateAround(tempPrefabInstance.transform.position, Vector3.up, -45f);
            }

            // 8. Cleanup screenshot objects
            tempCamera.targetTexture = null;
            RenderTexture.active = null;
            GameObject.DestroyImmediate(renderTexture);
            GameObject.DestroyImmediate(tempCamera.gameObject);
            GameObject.DestroyImmediate(tempPrefabInstance);

            // 9. Bake billboard asset
            EditorUtility.DisplayProgressBar(wizardTitle, "Baking Billboard Assets...", 0.8f);
            BakeBillboardAsset(capturedImages, billboardHeight);

            // Restore the original scene
            EditorUtility.DisplayProgressBar(wizardTitle, "Loading Original scene...", 0.85f);
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);

            // 10. Re-open the prefab editor
            EditorUtility.DisplayProgressBar(wizardTitle, "Re-opening original prefab...", 0.85f);
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath));

            Debug.Log($"Billboard components saved to {folderName}");
        }

        public void BakeBillboardAsset(List<Texture2D> capturedImages, float billboardHeight) {
            //Debug.Log($"BillboardWizard.BakeBillboardAsset({billboardHeight})");

            // 1. Create a texture atlas
            Texture2D textureAtlas = CreateTextureAtlas(width, height, capturedImages);

            // 2. Create the billboard material
            Material newMaterial = CreateBillboardMaterial(textureAtlas);

            // 3. Create the BillboardAsset
            BillboardAsset newBillboardAsset = new BillboardAsset();

            // Assign the material to the billboard asset
            newBillboardAsset.material = newMaterial;

            // Set the geometry for a simple quad
            SetGeometry(newBillboardAsset, billboardHeight, billboardHeight);

            // Set the image texture coordinates
            SetTextureCoordinates(newBillboardAsset, width, height);

            // 4. Save the assets
            SaveAssets(textureAtlas, newMaterial, newBillboardAsset);
        }

        private void SaveAssets(Texture2D atlas, Material material, BillboardAsset billboardAsset) {

            // The desired output path for the billboard asset
            string assetPath = $"Assets/{parentFolder}/{fileName}Billboard.asset";

            // Save the texture atlas to a PNG file
            string atlasPath = Path.ChangeExtension(assetPath, "png");

            byte[] bytes = atlas.EncodeToPNG();
            File.WriteAllBytes(atlasPath, bytes);
            AssetDatabase.Refresh();

            // Get the TextureImporter for the saved asset
            TextureImporter importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
            if (importer != null) {
                // Set the desired serialized properties
                importer.alphaIsTransparency = true;
                // Trigger the re-import
                AssetDatabase.ImportAsset(atlasPath);
                //Debug.Log($"Re-imported texture withAlpha is Transparency: {assetPath}");
            } else {
                Debug.LogError($"Could not get TextureImporter for asset at path: {atlasPath}");
            }

            // Load the texture back as a Texture2D asset
            Texture2D atlasAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
            material.mainTexture = atlasAsset;

            // Save the new material
            string materialPath = Path.ChangeExtension(assetPath, "mat");
            AssetDatabase.CreateAsset(material, materialPath);

            // Save the BillboardAsset
            AssetDatabase.CreateAsset(billboardAsset, assetPath);
            AssetDatabase.Refresh();

            //Debug.Log("Billboard asset baked and saved to: " + assetPath);
        }

        private void SetGeometry(BillboardAsset billboardAsset, float width, float height) {
            // Vertices for a simple quad
            Vector2[] vertices = {
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(1f, 0f), new Vector2(1f, 1f)
        };
            billboardAsset.SetVertices(vertices);

            // Indices for the triangles
            ushort[] indices = { 0, 1, 2, 1, 3, 2 };
            billboardAsset.SetIndices(indices);

            billboardAsset.width = width;
            billboardAsset.height = height;
            billboardAsset.bottom = 0f;
        }

        private void SetTextureCoordinates(BillboardAsset billboardAsset, int width, int height) {
            float invAtlasWidth = 1f / (width * 4);
            float invAtlasHeight = 1f / (height * 2);

            Vector4[] imageTexCoords = new Vector4[8];

            // Standard 8-directional mapping starting from Positive X (Right) and moving clockwise.
            // Match this order with the image positions in your atlas.

            // BillboardAsset expects:
            // Index 0: Right (90 degrees clockwise from Front)
            // Index 1: RightFront (45 degrees clockwise)
            // Index 2: Front (0 degrees)
            // Index 3: LeftFront (45 degrees counter-clockwise)
            // ...and so on.

            // Map according to the new atlas layout from Step 1:
            // Atlas Bottom Row: Front (0), LeftFront (1), Left (2), LeftBack (3)
            // Atlas Top Row:    Back (4),  RightBack (5), Right (6), RightFront (7)

            // Billboard view 0: Corresponds to Right (capturedImages[6]) at (2,1)
            imageTexCoords[0] = new Vector4(invAtlasWidth * (width * 2), invAtlasHeight * height, invAtlasWidth * width, invAtlasHeight * height);

            // Billboard view 1: Corresponds to RightFront (capturedImages[7]) at (3,1)
            imageTexCoords[1] = new Vector4(invAtlasWidth * (width * 3), invAtlasHeight * height, invAtlasWidth * width, invAtlasHeight * height);

            // Billboard view 2: Corresponds to Front (capturedImages[0]) at (0,0)
            imageTexCoords[2] = new Vector4(invAtlasWidth * 0, invAtlasHeight * 0, invAtlasWidth * width, invAtlasHeight * height);

            // Billboard view 3: Corresponds to LeftFront (capturedImages[1]) at (1,0)
            imageTexCoords[3] = new Vector4(invAtlasWidth * width, invAtlasHeight * 0, invAtlasWidth * width, invAtlasHeight * height);

            // Billboard view 4: Corresponds to Left (capturedImages[2]) at (2,0)
            imageTexCoords[4] = new Vector4(invAtlasWidth * (width * 2), invAtlasHeight * 0, invAtlasWidth * width, invAtlasHeight * height);

            // Billboard view 5: Corresponds to LeftBack (capturedImages[3]) at (3,0)
            imageTexCoords[5] = new Vector4(invAtlasWidth * (width * 3), invAtlasHeight * 0, invAtlasWidth * width, invAtlasHeight * height);

            // Billboard view 6: Corresponds to Back (capturedImages[4]) at (0,1)
            imageTexCoords[6] = new Vector4(invAtlasWidth * 0, invAtlasHeight * height, invAtlasWidth * width, invAtlasHeight * height);

            // Billboard view 7: Corresponds to RightBack (capturedImages[5]) at (1,1)
            imageTexCoords[7] = new Vector4(invAtlasWidth * width, invAtlasHeight * height, invAtlasWidth * width, invAtlasHeight * height);

            billboardAsset.SetImageTexCoords(imageTexCoords);
        }

        private Material CreateBillboardMaterial(Texture2D atlas) {
            // Create a new material using a URP-compatible billboard shader
            // You must create this shader using Shader Graph as explained below.
            Shader billboardShader = Shader.Find("Universal Render Pipeline/Nature/SpeedTree7 Billboard");
            if (billboardShader == null) {
                Debug.LogError("Custom URP billboard shader not found. Please create it using Shader Graph or assign it manually.");
                return null;
            }

            Material newMaterial = new Material(billboardShader);
            newMaterial.SetTexture("_MainTex", atlas);

            // Set other properties as needed by your shader graph
            newMaterial.SetColor("_Color", Color.white);
            newMaterial.SetFloat("_Surface", 1.0f); // 1 = Transparent, 0 = Opaque

            // It's also important to tell the BillboardRenderer that this is a billboarding material.
            // The shader properties and names below must match your Shader Graph setup exactly.
            newMaterial.SetFloat("_CullMode", (float)UnityEngine.Rendering.CullMode.Off);

            return newMaterial;
        }

        private Texture2D CreateTextureAtlas(int width, int height, List<Texture2D> capturedImages) {
            // Atlas is 4x2 for the 8 textures
            Texture2D atlas = new Texture2D(width * 4, height * 2, TextureFormat.RGBA32, false);
            Color[] clearColors = new Color[atlas.width * atlas.height];
            for (int i = 0; i < clearColors.Length; i++) {
                clearColors[i] = Color.clear;
            }
            atlas.SetPixels(clearColors);

            // Arrange the textures in a 4x2 grid, respecting the enum order.
            // Bottom Row: [0] Front, [1] LeftFront, [2] Left, [3] LeftBack
            // Top Row:    [4] Back,  [5] RightBack, [6] Right, [7] RightFront

            // Bottom row (Images 0-3 from capturedImages)
            atlas.SetPixels(width * 0, 0, width, height, capturedImages[0].GetPixels()); // 0: Front
            atlas.SetPixels(width * 1, 0, width, height, capturedImages[1].GetPixels()); // 1: LeftFront
            atlas.SetPixels(width * 2, 0, width, height, capturedImages[2].GetPixels()); // 2: Left
            atlas.SetPixels(width * 3, 0, width, height, capturedImages[3].GetPixels()); // 3: LeftBack

            // Top row (Images 4-7 from capturedImages)
            atlas.SetPixels(width * 0, height, width, height, capturedImages[4].GetPixels()); // 4: Back
            atlas.SetPixels(width * 1, height, width, height, capturedImages[5].GetPixels()); // 5: RightBack
            atlas.SetPixels(width * 2, height, width, height, capturedImages[6].GetPixels()); // 6: Right
            atlas.SetPixels(width * 3, height, width, height, capturedImages[7].GetPixels()); // 7: RightFront

            atlas.Apply();

            return atlas;
        }


        private Camera CreateTempCamera(Vector3 camPosition, Quaternion camRotation, float camFieldOfView, bool camIsOrthographic, float camOrthographicSize) {
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
            tempCamera.backgroundColor = new Color32(0, 0, 0, 0);

            // Disable Post-Processing for the temporary camera
            if (tempCamera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData)) {
                cameraData.renderPostProcessing = false;
            }
            return tempCamera;
        }

        private void LoadTempScene() {
            Scene tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SetActiveScene(tempScene);
        }

        private string GetOriginalScenePath() {
            Scene originalScene = EditorSceneManager.GetActiveScene();
            string originalScenePath = string.Empty;
            if (originalScene != null) {
                originalScenePath = originalScene.path;
                //Debug.Log($"Original scene is {originalScene.name} at path {originalScene.path}");
            }
            return originalScenePath;
        }

        public void RotateToFront(Camera tempCamera, Transform targetTransform) {
            if (targetTransform == null) {
                Debug.LogWarning("Target object not assigned. Cannot rotate camera.");
                return;
            }

            // Capture the camera's starting height.
            float initialY = tempCamera.transform.position.y;

            // Determine the desired direction from the camera to the target.
            // Use the opposite of the target's forward vector to point *towards* the front.
            Vector3 desiredDirection = targetTransform.forward;
            desiredDirection.y = 0; // Ignore vertical component for a horizontal rotation.
            desiredDirection.Normalize();

            // Get the horizontal distance between the camera and the target.
            Vector3 cameraPositionHorizontal = new Vector3(tempCamera.transform.position.x, 0, tempCamera.transform.position.z);
            Vector3 targetPositionHorizontal = new Vector3(targetTransform.position.x, 0, targetTransform.position.z);
            float distance = Vector3.Distance(cameraPositionHorizontal, targetPositionHorizontal);

            // Calculate the camera's new position based on the desired horizontal direction and distance.
            Vector3 newPosition = targetTransform.TransformPoint(desiredDirection * distance);
            newPosition.y = initialY; // Set the y-position to the initial height.

            // Apply the new position.
            tempCamera.transform.position = newPosition;

            // Make the camera look at the target, but keep its own y-axis unchanged to maintain the same vertical angle.
            Vector3 lookAtPoint = targetTransform.position;
            lookAtPoint.y = initialY;
            tempCamera.transform.LookAt(lookAtPoint);
        }


        /*
        public void RotateToFront(Camera tempCamera, Transform targetTransform) {
            if (targetTransform == null) {
                Debug.LogWarning("Target object not assigned. Cannot rotate camera.");
                return;
            }

            // Determine the desired direction from the target to the camera.
            // This will be the opposite of the target's forward direction.
            Vector3 desiredDirection = -targetTransform.forward;

            // Get the distance between the camera and the target.
            float distance = Vector3.Distance(tempCamera.transform.position, targetTransform.position);

            // Calculate the camera's new position based on the desired direction and distance.
            Vector3 newPosition = targetTransform.position + desiredDirection * distance;

            // Set the camera's position.
            tempCamera.transform.position = newPosition;

            // Make the camera look at the target. This ensures perfect alignment.
            tempCamera.transform.LookAt(targetTransform.position);
        }
        */

        /*
        public void RotateToFront(Camera tempCamera, Transform targetTransform) {
            if (targetTransform == null) {
                Debug.LogWarning("Target object not assigned. Cannot rotate camera.");
                return;
            }

            // Calculate the vector from the camera to the target.
            Vector3 currentDirection = tempCamera.transform.position - targetTransform.position;

            // Determine the target position based on the target's positive Z-axis.
            Vector3 targetDirection = targetTransform.forward * currentDirection.magnitude;

            // Calculate the angle between the current direction and the target direction.
            float angle = Vector3.SignedAngle(currentDirection, targetDirection, Vector3.up);

            // Use RotateAround to perform the rotation around the target's position.
            // We rotate around the global Y-axis to stay on the same horizontal plane.
            tempCamera.transform.RotateAround(targetTransform.position, Vector3.up, angle);
        }
        */

        private Texture2D RenderOneImage(Camera tempCamera, int captureWidth, int captureHeight, int imageCount, string folderName) {

            tempCamera.Render();

            // 7. Save the image to a transparent PNG
            RenderTexture.active = tempCamera.targetTexture;
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

            //string prefabName = Path.GetFileNameWithoutExtension(prefabStage.assetPath);
            string screenshotFilename = GetFinalFileName(folderName, Enum.GetName(typeof(BillboardDirection), imageCount));
            byte[] screenshotData = screenShot.EncodeToPNG();
            //Debug.Log($"Capturing screenshot to file {screenshotFilename}. width: {width}/{captureWidth} Height: {height}/{captureHeight} sourceX: {sourceX} sourceY: {sourceY}");

            File.WriteAllBytes(screenshotFilename, screenshotData);

            GameObject.DestroyImmediate(screenShot);
            
            ForceReimportWithSettings(screenshotFilename);

            AssetDatabase.Refresh();

            Texture2D importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(GetAssetPathFromFilePath(screenshotFilename));

            return importedTexture;
        }

        private string GetAssetPathFromFilePath(string filePath) {
            string projectAssetsPath = Application.dataPath;
            if (!filePath.StartsWith(projectAssetsPath)) {
                Debug.LogError($"Asset path {filePath} is not inside the project's Assets folder.");
                return null;
            }
            return filePath.Substring(projectAssetsPath.Length - "Assets".Length);
        }

        private void ForceReimportWithSettings(string absoluteFilePath) {

            string assetPath = GetAssetPathFromFilePath(absoluteFilePath);
            //Debug.Log(assetPath);

            // Tell Unity to rescan its asset database and discover the newly created file
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            // Get the TextureImporter for the saved asset
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null) {
                // Set the desired serialized properties
                importer.isReadable = true;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                // Trigger the re-import
                AssetDatabase.ImportAsset(assetPath);
                //Debug.Log($"Re-imported texture with Read/Write and Alpha is Transparency: {assetPath}");
            } else {
                Debug.LogError($"Could not get TextureImporter for asset at path: {assetPath}");
            }
        }

        public string GetFinalFileName(string folderName, string appendString) {
            if (fileName == string.Empty) {
                return $"{folderName}/{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.png";
            }
            if (System.IO.File.Exists(folderName + "/" + fileName + ".png")) {
                return $"{folderName}/{fileName}{appendString}_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.png";
            } else {
                return $"{folderName}/{fileName}{appendString}.png";
            }
        }

        void OnWizardUpdate() {
            helpString = "Creates a screenshot image of the currently loaded scene";
            errorString = Validate();
            SetFileName();
            isValid = (errorString == null || errorString == "");
        }

        void SetFileName() {
            if (fileName == string.Empty) {
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage == null) {
                    return;
                }
                fileName = prefabStage.prefabContentsRoot.name;
            }
        }

        string GetFolder() {
            return $"{Application.dataPath}/{parentFolder}";
        }

        string Validate() {
            if (parentFolder == null || parentFolder.Trim() == "") {
                return "Parent Folder name must not be empty";
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

    public enum BillboardDirection { Front = 0, LeftFront = 1, Left = 2, LeftBack = 3, Back = 4, RightBack = 5, Right = 6, RightFront = 7 }

}