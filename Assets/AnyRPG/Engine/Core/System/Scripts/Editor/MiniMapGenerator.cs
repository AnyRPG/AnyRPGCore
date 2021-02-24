using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;


namespace AnyRPG
{
    public class MiniMapGenerator : MonoBehaviour
    {
        [SerializeField]
        public float snapDelay = 2f;

        [SerializeField]
        public int superSize = 10;

        /*
         * Thin static wrapper around method to create minimapTextures and provide toolbar item.
        */
        [MenuItem("Tools/AnyRPG/Minimap/Generate Minimap")]
        public static void GenerateMinimapTextures()
        {
            if (!Application.isPlaying)
            {
                string errorMessage = "Application must be in play mode to use Minimap Generator";
                EditorUtility.DisplayDialog("MiniMap Generator", errorMessage, "OK");
                Debug.LogError(errorMessage);
                return;
            }

            if (MiniMapController.MyInstance == null)
            {
                string errorMessage = "MiniMapController not found in scene. Aborting...";
                EditorUtility.DisplayDialog("MiniMap Generator", errorMessage, "OK");
                Debug.LogError(errorMessage);
                return;
            }
            MiniMapController.MyInstance.StartCoroutine(new MiniMapGenerator().CreateMinimapTextures());
        }

        /*
         * Creates the file(s) necessary to pre-generate a minimap for the _active_ scene.  Files will be saved
         * with a naming convention that indicates the scene that they were saved from.
         */
        IEnumerator CreateMinimapTextures()
        {
            if (snapDelay < 0.1f)
            {
                snapDelay = 0.1f;
                Debug.LogWarning("Snap delay cannot be lower than 0.1 seconds, setting it back to 0.1 seconds.");
            }

            EditorUtility.DisplayProgressBar("Generating Minimap...", "Please wait", 0);

            // NOTE: Probably a better way to get this
            Bounds sceneBounds = MainMapController.MyInstance.GetSceneBounds();
            GameObject minimapGenerator = new GameObject();
            Camera camera = minimapGenerator.AddComponent<Camera>();
            camera.orthographic = true;
            camera.name = "MinimapGeneratorCamera";

            // NOTE: maybe should use an aspect ratio here based on on x/z?
            float cameraSize = System.Math.Max(sceneBounds.size.x, sceneBounds.size.z);
            camera.orthographicSize = cameraSize;

            float yMin = sceneBounds.min.y;
            float yMax = sceneBounds.max.y;
            Debug.Log("Found Y min max bounds at " + yMin + " / " + yMax);
            // Place the camera at the top of the the level right in the center
            Vector3 cameraPosition = new Vector3(sceneBounds.center.x, sceneBounds.max.y, sceneBounds.center.z);
            camera.transform.position = cameraPosition;
            Debug.Log("Pointing camera at  " + cameraPosition.x + " / " + cameraPosition.y + " / " + cameraPosition.z);
            // Look at the middle of the map
            Vector3 lookAt = new Vector3(sceneBounds.center.x, sceneBounds.center.y, sceneBounds.center.z);
            camera.transform.LookAt(lookAt);
            Debug.Log("Pointing camera at  " + lookAt.x + " / " + lookAt.y + " / " + lookAt.z);

            EditorUtility.DisplayProgressBar("Generating Minimap...", "Please wait", .25f);

            // Set the far clip to reach all the way to the Y middle of the map so that is captured
            camera.farClipPlane = sceneBounds.max.y - sceneBounds.min.y;
            Debug.Log("Setting far clip plane to " + camera.farClipPlane);
            yield return new WaitForSeconds(snapDelay);

            if (!System.IO.Directory.Exists(MiniMapController.minimapTextureFolder))
            {
                System.IO.Directory.CreateDirectory(MiniMapController.minimapTextureFolder);
            }

            string screenshotFilename = MiniMapController.MyInstance.GetScreenshotFilename();
            EditorUtility.DisplayProgressBar("Generating Minimap...", "Please wait", 0.5f);
            Debug.Log("Taking screenshot...");
            yield return TakeAndSaveSnapshot(camera, screenshotFilename, (int)cameraSize * superSize, (int)cameraSize * superSize);

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
            Debug.Log("Destroying camera object " + minimapGenerator.name);
            UnityEngine.Object.Destroy(minimapGenerator);
            EditorUtility.DisplayProgressBar("Generating Minimap...", "Complete", 1);
            EditorUtility.ClearProgressBar();

            Debug.Log("Minimap generation complete!  Output at " + MiniMapController.minimapTextureFolder + "/" + screenshotFilename);
        }

        /*
         * Renders a snapshot from the given camera and saves it to the filename provided at the resolution provided.
         * Needs to wait for a certain number of seconds (snapDelay) to give the camera time to render
         */
        IEnumerator TakeAndSaveSnapshot(Camera camera, string filename, int captureWidth, int captureHeight)
        {
            RenderTexture renderTexture;
            Rect rect = new Rect(0, 0, captureWidth, captureHeight);
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
            camera.targetTexture = renderTexture;
            camera.Render();
            yield return new WaitForSeconds(snapDelay);

            // read pixels will read from the currently active render texture so make our offscreen 
            // render texture active and then read the pixels
            RenderTexture.active = renderTexture;
            Texture2D screenShot;
            screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
            screenShot.ReadPixels(rect, 0, 0);

            // reset active camera texture and render texture
            camera.targetTexture = null;
            RenderTexture.active = null;

            // Not sure if you want to store this in /Resources or somewhere besides /Assets
            string screenshotFilename = MiniMapController.minimapTextureFolder + "/" + filename;
            byte[] screenshotData = screenShot.EncodeToPNG();
            System.IO.FileStream fStream = System.IO.File.Create(screenshotFilename);
            fStream.Write(screenshotData, 0, screenshotData.Length);
            fStream.Close();

            GameObject.Destroy(screenShot);
            GameObject.Destroy(renderTexture);
        }

    }

}