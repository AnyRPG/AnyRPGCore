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
    public class BillboardWizard : ScriptableWizard {

        // Will be a subfolder of Application.dataPath and should start with "/"
        public string parentFolder = "/Screenshots/";
        public string fileName = string.Empty;

        private const string wizardTitle = "Billboard Wizard";
        public Texture2D frontTexture;
        public Texture2D backTexture;
        public Texture2D leftTexture;
        public Texture2D rightTexture;

        private static BillboardWizard openWizard = null;

        [MenuItem("Tools/AnyRPG/Wizard/Billboard Wizard")]
        static void CreateWizard() {
            if (openWizard == null) {
                openWizard = ScriptableWizard.DisplayWizard<BillboardWizard>(wizardTitle, "Create");
            } else {
                openWizard.Focus();
            }
        }

        private void OnDisable() {
            openWizard = null;
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar(wizardTitle, "Creating folders...", 0.3f);

            // Setup folder locations
            string filePath = GetFolder();

            // create missing folders
            WizardUtilities.CreateFolderIfNotExists(filePath);

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar(wizardTitle, "Making Billboard...", 0.6f);

            BakeBillboardAsset();

            EditorUtility.DisplayProgressBar(wizardTitle, "Refreshing Asset Database...", 0.9f);

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(wizardTitle, wizardTitle + " Complete! The screenshot image can be found at " + filePath, "OK");

        }

        public void BakeBillboardAsset() {
            if (!IsValidInput()) {
                Debug.LogError("All textures and a billboard material must be assigned.");
                return;
            }

            // 1. Create a texture atlas
            int width = frontTexture.width;
            int height = frontTexture.height;
            Texture2D textureAtlas = CreateTextureAtlas(width, height);

            // 2. Create the billboard material
            Material newMaterial = CreateBillboardMaterial(textureAtlas);

            // 3. Create the BillboardAsset
            BillboardAsset newBillboardAsset = new BillboardAsset();

            // Assign the material to the billboard asset
            newBillboardAsset.material = newMaterial;

            // Set the geometry for a simple quad
            SetGeometry(newBillboardAsset, width, height);

            // Set the image texture coordinates
            SetTextureCoordinates(newBillboardAsset, width, height);

            // 4. Save the assets
            SaveAssets(textureAtlas, newMaterial, newBillboardAsset);
        }
        /*
        private Material CreateBillboardMaterial(Texture2D atlas) {
            // Create a new material using the specified URP shader
            Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            // Set the texture atlas as the base map (main texture)
            newMaterial.SetTexture("_BaseMap", atlas);

            // Set the material to transparent mode
            newMaterial.SetFloat("_Surface", 1.0f); // 1 = Transparent, 0 = Opaque
            newMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            newMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            newMaterial.SetInt("_ZWrite", 0);
            newMaterial.DisableKeyword("_ALPHATEST_ON");
            newMaterial.EnableKeyword("_ALPHABLEND_ON");
            newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            newMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            return newMaterial;
        }
        */

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

        private bool IsValidInput() {
            return frontTexture != null && backTexture != null && leftTexture != null && rightTexture != null;
        }

        private Texture2D CreateTextureAtlas(int width, int height) {
            // Atlas is 2x2 for the remaining 4 textures
            Texture2D atlas = new Texture2D(width * 2, height * 2, TextureFormat.RGBA32, false);
            Color[] clearColors = new Color[atlas.width * atlas.height];
            for (int i = 0; i < clearColors.Length; i++) {
                clearColors[i] = Color.clear;
            }
            atlas.SetPixels(clearColors);

            // Arrange the textures in a logical 2x2 grid.
            // Example: front and back on the top row, left and right on the bottom.
            atlas.SetPixels(0, height, width, height, rightTexture.GetPixels());    // Top-left (0,1)
            atlas.SetPixels(width, height, width, height, frontTexture.GetPixels());     // Top-right (1,1)
            atlas.SetPixels(0, 0, width, height, leftTexture.GetPixels());      // Bottom-left (0,0)
            atlas.SetPixels(width, 0, width, height, backTexture.GetPixels());     // Bottom-right (1,0)

            atlas.Apply();

            return atlas;
        }



        private void SetGeometry(BillboardAsset billboardAsset, int width, int height) {
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
            // Inverse dimensions based on the new 2x2 grid
            float invWidth = 1f / (width * 2);
            float invHeight = 1f / (height * 2);

            // Array to hold the texture coordinates for the 4 images
            Vector4[] imageTexCoords = new Vector4[4];

            // Coordinates are (U, V, width, height) relative to the 2x2 atlas
            // Front view is now at (0,1)
            imageTexCoords[0] = new Vector4(invWidth * 0, invHeight * height, invWidth * width, invHeight * height);

            // Back view is now at (1,1)
            imageTexCoords[1] = new Vector4(invWidth * width, invHeight * height, invWidth * width, invHeight * height);

            // Left view is now at (0,0)
            imageTexCoords[2] = new Vector4(invWidth * 0, invHeight * 0, invWidth * width, invHeight * height);

            // Right view is now at (1,0)
            imageTexCoords[3] = new Vector4(invWidth * width, invHeight * 0, invWidth * width, invHeight * height);

            billboardAsset.SetImageTexCoords(imageTexCoords);
        }



        private void SaveAssets(Texture2D atlas, Material material, BillboardAsset billboardAsset) {

            // The desired output path for the billboard asset
            string assetPath = $"Assets{parentFolder}{fileName}Billboard.asset";

            // Save the texture atlas to a PNG file
            string atlasPath = Path.ChangeExtension(assetPath, "png");

            byte[] bytes = atlas.EncodeToPNG();
            File.WriteAllBytes(atlasPath, bytes);
            AssetDatabase.Refresh();

            // Load the texture back as a Texture2D asset
            Texture2D atlasAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
            material.mainTexture = atlasAsset;

            // Save the new material
            string materialPath = Path.ChangeExtension(assetPath, "mat");
            AssetDatabase.CreateAsset(material, materialPath);

            // Save the BillboardAsset
            AssetDatabase.CreateAsset(billboardAsset, assetPath);
            AssetDatabase.Refresh();

            Debug.Log("Billboard asset baked and saved to: " + assetPath);
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
            helpString = "Creates a billboard asset from a series of images";
            /*
            errorString = Validate();
            isValid = (errorString == null || errorString == "");
            */
            SetFileName();
            isValid = true;
        }

        void SetFileName() {
            if (frontTexture != null && fileName == string.Empty) {
                fileName = frontTexture.name.Substring(0,frontTexture.name.Length - 5);
            }
        }


        string GetFolder() {
            return Application.dataPath + parentFolder;
        }

        public static void DisplayProgressBar(string title, string info, float progress) {
            EditorUtility.DisplayProgressBar(title, info, progress);
        }


    }

}