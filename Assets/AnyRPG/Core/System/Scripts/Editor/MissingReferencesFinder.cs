using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG.Editor {

    public class MissingReferencesFinder : MonoBehaviour {
        private class ObjectData {
            public float ExpectedProgress;
            public GameObject GameObject;
        }

        [MenuItem("Tools/AnyRPG/Find Missing References In GameObjects/In current scene", false, 50)]
        public static void FindMissingReferencesInCurrentScene() {
            var scene = SceneManager.GetActiveScene();
            showInitialProgressBar(scene.path);

            var rootObjects = scene.GetRootGameObjects();

            /*var queue = new Queue<ObjectData>();
            foreach (var rootObject in rootObjects) {
                queue.Enqueue(new ObjectData{ExpectedProgress = 1/(float)rootObjects.Length, GameObject = rootObject});
            }*/

            var wasCancelled = false;
            var count = findMissingReferencesInScene(scene, 1, () => { wasCancelled = false; }, () => { wasCancelled = true; });
            showFinishDialog(wasCancelled, count);
        }

        [MenuItem("Tools/AnyRPG/Find Missing References In GameObjects/In current prefab", false, 51)]
        public static void FindMissingReferencesInCurrentPrefab() {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            showInitialProgressBar(prefabStage.assetPath);

            var count = findMissingReferences(prefabStage.assetPath, prefabStage.prefabContentsRoot, true);
            showFinishDialog(false, count);
        }

        [MenuItem("Tools/AnyRPG/Find Missing References In GameObjects/In current prefab", true, 51)]
        public static bool FindMissingReferencesInCurrentPrefabValidate() => PrefabStageUtility.GetCurrentPrefabStage() != null;

        [MenuItem("Tools/AnyRPG/Find Missing References In GameObjects/In all scenes in build", false, 52)]
        public static void FindMissingReferencesInAllScenesInBuild() {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToList();

            var count = 0;
            var wasCancelled = true;
            foreach (var scene in scenes) {
                Scene openScene;
                try {
                    openScene = EditorSceneManager.OpenScene(scene.path);
                } catch (Exception ex) {
                    Debug.LogError($"Could not open scene at path \"{scene.path}\". This scene was added to the build, and it's possible that it has been deleted: Error: {ex.Message}");
                    continue;
                }

                count += findMissingReferencesInScene(openScene, 1 / (float)scenes.Count(), () => { wasCancelled = false; }, () => { wasCancelled = true; });
                if (wasCancelled) break;
            }
            showFinishDialog(wasCancelled, count);
        }

        /*[MenuItem("Tools/Find Missing References/In all scenes in project", false, 52)]
        public static void FindMissingReferencesInAllScenes() {
            var scenes = EditorBuildSettings.scenes;

            var finished = true;
            foreach (var scene in scenes) {
                var s = EditorSceneManager.OpenScene(scene.path);
                finished = findMissingReferencesInScene(s, 1 /(float)scenes.Count());
                if (!finished) break;
            }
            showFinishDialog(!finished);
        }*/

        [MenuItem("Tools/AnyRPG/Find Missing References In GameObjects/In assets", false, 52)]
        public static void FindMissingReferencesInAssets() {
            showInitialProgressBar("all assets");
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            var objs = allAssetPaths
                       .Where(isProjectAsset)
                       .ToArray();

            var wasCancelled = false;
            var count = findMissingReferences("Project", objs, () => { wasCancelled = false; }, () => { wasCancelled = true; });
            showFinishDialog(wasCancelled, count);
        }

        [MenuItem("Tools/AnyRPG/Find Missing References In GameObjects/Everywhere", false, 53)]
        public static void FindMissingReferencesEverywhere() {
            var scenes = EditorBuildSettings.scenes;
            var progressWeight = 1 / (float)(scenes.Length + 1);

            var count = 0;
            var wasCancelled = true;
            var currentProgress = 0f;
            foreach (var scene in scenes) {
                Scene openScene;
                try {
                    openScene = EditorSceneManager.OpenScene(scene.path);
                } catch (Exception ex) {
                    Debug.LogError($"Could not open scene at path \"{scene.path}\". This scene was added to the build, and it's possible that it has been deleted: Error: {ex.Message}");
                    continue;
                }
                count += findMissingReferencesInScene(openScene, progressWeight, () => { wasCancelled = false; }, () => { wasCancelled = true; }, currentProgress);
                currentProgress += progressWeight;
                if (wasCancelled) break;
            }

            if (!wasCancelled) {
                var allAssetPaths = AssetDatabase.GetAllAssetPaths();
                var objs = allAssetPaths
                           .Where(isProjectAsset)
                           .ToArray();

                count += findMissingReferences("Project", objs, () => { wasCancelled = false; }, () => { wasCancelled = true; }, currentProgress, progressWeight);
            }

            showFinishDialog(wasCancelled, count);
        }

        private static bool isProjectAsset(string path) {
#if UNITY_EDITOR_OSX
        return !path.StartsWith("/");
#else
            return path.Substring(1, 2) != ":/";
#endif
        }

        private static int findMissingReferences(string context, string[] paths, Action onFinished, Action onCanceled, float initialProgress = 0f, float progressWeight = 1f) {
            var count = 0;
            var wasCancelled = false;
            for (var i = 0; i < paths.Length; i++) {
                var obj = AssetDatabase.LoadAssetAtPath(paths[i], typeof(GameObject)) as GameObject;
                if (obj == null || !obj) continue;

                if (wasCancelled || EditorUtility.DisplayCancelableProgressBar("Searching missing references in assets.",
                                                                               $"{paths[i]}",
                                                                               initialProgress + ((i / (float)paths.Length) * progressWeight))) {
                    onCanceled.Invoke();
                    return count;
                }

                count += findMissingReferences(context, obj);
            }

            onFinished.Invoke();
            return count;
        }

        private static int findMissingReferences(string context, GameObject go, bool findInChildren = false) {
            var count = 0;
            var components = go.GetComponents<Component>();

            for (var j = 0; j < components.Length; j++) {
                var c = components[j];
                if (!c) {
                    Debug.LogError($"Missing Component in GameObject: {FullPath(go)}", go);
                    count++;
                    continue;
                }

                var so = new SerializedObject(c);
                var sp = so.GetIterator();

                while (sp.NextVisible(true)) {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference) {
                        if (sp.objectReferenceValue == null
                         && sp.objectReferenceInstanceIDValue != 0) {
                            ShowError(context, go, c.GetType().Name, ObjectNames.NicifyVariableName(sp.name));
                            count++;
                        }
                    }
                }
            }

            if (findInChildren) {
                foreach (Transform child in go.transform) {
                    count += findMissingReferences(context, child.gameObject, true);
                }
            }

            return count;
        }

        private static int findMissingReferencesInScene(Scene scene, float progressWeightByScene, Action onFinished, Action onCanceled, float currentProgress = 0f) {
            var rootObjects = scene.GetRootGameObjects();

            var queue = new Queue<ObjectData>();
            foreach (var rootObject in rootObjects) {
                queue.Enqueue(new ObjectData { ExpectedProgress = progressWeightByScene / (float)rootObjects.Length, GameObject = rootObject });
            }

            var count = findMissingReferences(scene.path, queue,
                                              onFinished,
                                              onCanceled,
                                              true, currentProgress);
            return count;
        }

        private static int findMissingReferences(string context, Queue<ObjectData> queue, Action onFinished, Action onCanceled, bool findInChildren = false, float currentProgress = 0f) {
            var count = 0;
            while (queue.Any()) {
                var data = queue.Dequeue();
                var go = data.GameObject;
                var components = go.GetComponents<Component>();

                float progressEachComponent;
                if (findInChildren) {
                    progressEachComponent = (data.ExpectedProgress) / (float)(components.Length + go.transform.childCount);
                } else {
                    progressEachComponent = data.ExpectedProgress / (float)components.Length;
                }

                for (var j = 0; j < components.Length; j++) {
                    currentProgress += progressEachComponent;
                    if (EditorUtility.DisplayCancelableProgressBar($"Searching missing references in {context}",
                                                                   go.name,
                                                                   currentProgress)) {
                        onCanceled.Invoke();
                        return count;
                    }

                    var c = components[j];
                    if (!c) {
                        Debug.LogError($"Missing Component in GameObject: \"{FullPath(go)}\"", go);
                        count++;
                        continue;
                    }

                    using (var so = new SerializedObject(c)) {
                        using (var sp = so.GetIterator()) {
                            while (sp.NextVisible(true)) {
                                if (sp.propertyType == SerializedPropertyType.ObjectReference) {
                                    if (sp.objectReferenceValue == null
                                     && sp.objectReferenceInstanceIDValue != 0) {
                                        ShowError(context, go, c.GetType().Name, ObjectNames.NicifyVariableName(sp.name));
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                }

                if (findInChildren) {
                    foreach (Transform child in go.transform) {
                        if (child.gameObject == go) continue;
                        queue.Enqueue(new ObjectData { ExpectedProgress = progressEachComponent, GameObject = child.gameObject });
                    }
                }
            }

            onFinished.Invoke();
            return count;
        }

        private static void showInitialProgressBar(string searchContext, bool clearConsole = true) {
            if (clearConsole) {
                Debug.ClearDeveloperConsole();
            }
            EditorUtility.DisplayProgressBar("Missing References Finder", $"Preparing search in {searchContext}", 0f);
        }

        private static void showFinishDialog(bool wasCancelled, int count) {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Missing References Finder",
                                        wasCancelled ?
                                            $"Process cancelled.\n{count} missing references were found.\n Current results are shown as errors in the console." :
                                            $"Finished finding missing references.\n{count} missing references were found.\n Results are shown as errors in the console.",
                                        "Ok");
        }

        private const string err = "Missing Ref in: [{3}]{0}. Component: {1}, Property: {2}";

        private static void ShowError(string context, GameObject go, string c, string property) {
            Debug.LogError(string.Format(err, FullPath(go), c, property, context), go);
        }

        private static string FullPath(GameObject go) {
            var parent = go.transform.parent;
            return parent == null ? go.name : FullPath(parent.gameObject) + "/" + go.name;
        }
    }

}
