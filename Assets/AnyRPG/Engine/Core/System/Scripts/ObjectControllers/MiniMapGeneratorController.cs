using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;


namespace AnyRPG
{
    public class MiniMapGeneratorController : MonoBehaviour
    {
        [SerializeField]
        public float snapDelay = 2f;

        [SerializeField]
        public int pixelsPerMeter = 10;

        [SerializeField]
        public Camera mapCamera = null;

        [SerializeField]
        public string minimapTextureFolder = "Assets/Minimap/Textures";

        public Bounds GetSceneBounds() {
            Renderer[] renderers;
            TerrainCollider[] terrainColliders;
            Bounds sceneBounds = new Bounds();
            renderers = GameObject.FindObjectsOfType<Renderer>();
            terrainColliders = GameObject.FindObjectsOfType<TerrainCollider>();

            Debug.Log("Found " + renderers.Length + " renderers");
            Debug.Log("Found " + terrainColliders.Length + " terrain colliders");

            // add bounds of renderers in case there are structures higher or lower than terrain bounds
            if (renderers.Length != 0) {
                for (int i = 0; i < renderers.Length; i++) {
                    if (renderers[i].enabled == true) {
                        sceneBounds.Encapsulate(renderers[i].bounds);
                    }
                    //Debug.Log("MainMapController.SetSceneBounds(). Encapsulating: " + renderers[i].bounds);
                }
            }

            // add bounds of terrain colliders to get 'main' bounds
            if (terrainColliders.Length != 0) {
                for (int i = 0; i < terrainColliders.Length; i++) {
                    if (terrainColliders[i].enabled == true) {
                        sceneBounds.Encapsulate(terrainColliders[i].bounds);
                    }
                    //Debug.Log("MiniMapGeneratorController.GetSceneBounds(). Encapsulating terrain bounds: " + terrainColliders[i].bounds);
                }
            }

            return sceneBounds;
        }

        
        /*
         * Creates the file(s) necessary to pre-generate a minimap for the _active_ scene.  Files will be saved
         * with a naming convention that indicates the scene that they were saved from.
         */
        public void CreateMinimapTextures()
        {
            mapCamera.enabled = true;

            if (snapDelay < 0.1f)
            {
                snapDelay = 0.1f;
                Debug.LogWarning("Snap delay cannot be lower than 0.1 seconds, setting it back to 0.1 seconds.");
            }

            EditorUtility.DisplayProgressBar("Generating Minimap...", "Please wait", 0.5f);

            Bounds sceneBounds = GetSceneBounds();
            mapCamera.orthographic = true;

            float cameraSize = System.Math.Max(sceneBounds.size.x, sceneBounds.size.z);

            // orthographic camera size is a half extent so the camera diamater needs to be turned into a radius
            mapCamera.orthographicSize = cameraSize / 2f;

            float yMin = sceneBounds.min.y;
            float yMax = sceneBounds.max.y;
            float xSize = Mathf.Abs(sceneBounds.max.x - sceneBounds.min.x);
            float ySize = Mathf.Abs(sceneBounds.max.y - sceneBounds.min.y);
            float zSize = Mathf.Abs(sceneBounds.max.z - sceneBounds.min.z);
            Debug.Log("Determined scene bounds to be X: " + xSize + "(" + sceneBounds.max.x + "/" + sceneBounds.min.x + ") Y: " + ySize + "(" + yMax + "/" + yMin + ") Z: " + zSize + "(" + sceneBounds.max.z + "/" + sceneBounds.min.z + ")");
            // Place the camera at the top of the the level right in the center
            Vector3 cameraPosition = new Vector3(sceneBounds.center.x, sceneBounds.max.y + 1f, sceneBounds.center.z);
            mapCamera.transform.position = cameraPosition;
            Debug.Log("Moving camera to  " + cameraPosition.x + " / " + cameraPosition.y + " / " + cameraPosition.z);
            // Look at the middle of the map
            Vector3 lookAt = new Vector3(sceneBounds.center.x, sceneBounds.center.y, sceneBounds.center.z);
            mapCamera.transform.LookAt(lookAt);
            Debug.Log("Pointing camera at  " + lookAt.x + " / " + lookAt.y + " / " + lookAt.z);

            EditorUtility.DisplayProgressBar("Generating Minimap...", "Please wait", 0.6f);

            // Set the far clip to reach all the way to the Y bottom of the map so that is captured
            mapCamera.farClipPlane = ySize + 1f;
            Debug.Log("Setting far clip plane to " + mapCamera.farClipPlane);
            //yield return new WaitForSeconds(snapDelay);

            if (!System.IO.Directory.Exists(minimapTextureFolder))
            {
                System.IO.Directory.CreateDirectory(minimapTextureFolder);
            }
                
            //string screenshotFilename = EditorSceneManager.GetActiveScene().name + "_minimap.png";
            string screenshotFilename = EditorSceneManager.GetActiveScene().name + ".png";

            EditorUtility.DisplayProgressBar("Generating Minimap...", "Please wait", 0.7f);
            Debug.Log("Taking screenshot...");
            //yield return TakeAndSaveSnapshot(camera, screenshotFilename, (int)cameraSize * superSize, (int)cameraSize * superSize);
            TakeAndSaveSnapshot(screenshotFilename, (int)cameraSize * pixelsPerMeter, (int)cameraSize * pixelsPerMeter);

            /*  NOTE: Multiple y-levels code below
            // Loop over all the Y "levels" and grab snapshots of all of them
            for (float y = yMin; y < yMax; y += yInterval) {
                // Place the camera at the top of the the level right in the center
                camera.transform.position = new Vector3(sceneBounds.center.x, y, sceneBounds.center.z);
                // Look at the middle of the map
                camera.transform.LookAt(new Vector3(sceneBounds.center.x, sceneBounds.center.y, sceneBounds.center.z));
                camera.farClipPlane = farClipPlane;
                yield return new WaitForSeconds(snapDelay);

                // Take the snapshot
                string screenshotFilename = sceneName + "_" + y + "_minimap.png";
                yield return TakeAndSaveSnapshot(camera, screenshotFilename, (int)cameraSize * superSize, (int)cameraSize * superSize);
            }
            */
            //Debug.Log("Destroying camera object " + minimapGenerator.name);
            //UnityEngine.Object.Destroy(minimapGenerator);
            EditorUtility.DisplayProgressBar("Generating Minimap...", "Complete", 1);
            EditorUtility.ClearProgressBar();

            Debug.Log("Minimap generation complete!  Output at " + minimapTextureFolder + "/" + screenshotFilename);
            mapCamera.enabled = false;
        }

        /*
         * Renders a snapshot from the given camera and saves it to the filename provided at the resolution provided.
         * Needs to wait for a certain number of seconds (snapDelay) to give the camera time to render
         */
        public void TakeAndSaveSnapshot(string filename, int captureWidth, int captureHeight)
        {
            Debug.Log("Capturing screenshot to file " + filename + ". width: " + captureWidth + " Height: " + captureHeight);
            RenderTexture renderTexture;
            Rect rect = new Rect(0, 0, captureWidth, captureHeight);
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
            mapCamera.targetTexture = renderTexture;
            mapCamera.Render();
            //yield return new WaitForSeconds(snapDelay);

            // read pixels will read from the currently active render texture so make our offscreen 
            // render texture active and then read the pixels
            RenderTexture.active = renderTexture;
            Texture2D screenShot;
            screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
            screenShot.ReadPixels(rect, 0, 0);

            // reset active camera texture and render texture
            mapCamera.targetTexture = null;
            RenderTexture.active = null;

            // Not sure if you want to store this in /Resources or somewhere besides /Assets
            string screenshotFilename = minimapTextureFolder + "/" + filename;
            byte[] screenshotData = screenShot.EncodeToPNG();
            Debug.Log("Saving screenshot to " + screenshotFilename);
            System.IO.FileStream fStream = System.IO.File.Create(screenshotFilename);
            fStream.Write(screenshotData, 0, screenshotData.Length);
            fStream.Close();

            if (Application.isPlaying) {
                GameObject.Destroy(screenShot);
                GameObject.Destroy(renderTexture);
            } else {
                // DestroyImmediate must be used in the editor
                GameObject.DestroyImmediate(screenShot);
                GameObject.DestroyImmediate(renderTexture);
            }
        }

    }

}