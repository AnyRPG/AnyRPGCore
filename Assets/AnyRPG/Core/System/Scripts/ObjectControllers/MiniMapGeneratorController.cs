using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        private float ySize;
        private float cameraSize;

        /*
         * Creates the file(s) necessary to pre-generate a minimap for the _active_ scene.  Files will be saved
         * with a naming convention that indicates the scene that they were saved from.
         */
        public void EnableCamera() {
            mapCamera.enabled = true;

            if (snapDelay < 0.1f) {
                snapDelay = 0.1f;
                Debug.LogWarning("Snap delay cannot be lower than 0.1 seconds, setting it back to 0.1 seconds.");
            }
        }

        public void GetSceneBounds() {

            Bounds sceneBounds = LevelManager.GetSceneBounds();
            mapCamera.orthographic = true;

            cameraSize = System.Math.Max(sceneBounds.size.x, sceneBounds.size.z);

            // orthographic camera size is a half extent so the camera diamater needs to be turned into a radius
            mapCamera.orthographicSize = cameraSize / 2f;

            float yMin = sceneBounds.min.y;
            float yMax = sceneBounds.max.y;
            float xSize = Mathf.Abs(sceneBounds.max.x - sceneBounds.min.x);
            ySize = Mathf.Abs(sceneBounds.max.y - sceneBounds.min.y);
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
        }

        public void CreateFolder() {

            // Set the far clip to reach all the way to the Y bottom of the map so that is captured
            mapCamera.farClipPlane = ySize + 1f;
            Debug.Log("Setting far clip plane to " + mapCamera.farClipPlane);
            //yield return new WaitForSeconds(snapDelay);

            if (!System.IO.Directory.Exists(minimapTextureFolder)) {
                System.IO.Directory.CreateDirectory(minimapTextureFolder);
            }
        }

        public void CreateMinimapTextures(string screenshotFileName) {

            //string screenshotFilename = EditorSceneManager.GetActiveScene().name + "_minimap.png";

            Debug.Log("Taking screenshot...");
            //yield return TakeAndSaveSnapshot(camera, screenshotFilename, (int)cameraSize * superSize, (int)cameraSize * superSize);
            TakeAndSaveSnapshot(screenshotFileName, (int)cameraSize * pixelsPerMeter, (int)cameraSize * pixelsPerMeter);

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

            Debug.Log("Minimap generation complete!  Output at " + minimapTextureFolder + "/" + screenshotFileName);
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