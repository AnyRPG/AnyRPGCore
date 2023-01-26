﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AnyRPG.EditorTools {

    public class MissingMaterialReferencesFinder : MonoBehaviour {

        [MenuItem("Tools/AnyRPG/Find Missing References In Materials", false, 52)]
        public static void FindMissingReferencesInMaterials() {
            showInitialProgressBar("Materials");
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            string[] objs = allAssetPaths
                       .Where(isProjectAsset)
                       .ToArray();

            bool wasCancelled = false;
            int missingCount = findMissingMaterialReferences(objs, () => { wasCancelled = false; }, () => { wasCancelled = true; });
            showFinishDialog(wasCancelled, missingCount, allAssetPaths.Length);
        }

        private static bool isProjectAsset(string path) {
#if UNITY_EDITOR_OSX
        return !path.StartsWith("/");
#else
            return path.Substring(1, 2) != ":/";
#endif
        }

        private static int findMissingMaterialReferences(string[] paths, Action onFinished, Action onCanceled, float initialProgress = 0f, float progressWeight = 1f) {
            int missingCount = 0;
            int materialCount = 0;
            bool wasCancelled = false;
            for (int i = 0; i < paths.Length; i++) {
                Material material = AssetDatabase.LoadAssetAtPath(paths[i], typeof(Material)) as Material;
                if (material == null || !material) {
                    continue;
                }
                materialCount++;

                if (wasCancelled || EditorUtility.DisplayCancelableProgressBar("Searching missing references in assets.",
                                                                               $"{paths[i]}",
                                                                               initialProgress + ((i / (float)paths.Length) * progressWeight))) {
                    onCanceled.Invoke();
                    return missingCount;
                }

                missingCount += findMissingMaterialReferences(paths[i], material);
            }

            Debug.Log($"Found {materialCount} Materials");

            onFinished.Invoke();
            return missingCount;
        }

        private static int findMissingMaterialReferences(string assetPath, Material material) {
            int missingCount = 0;

            var serializedObject = new SerializedObject(material);
            var serializedProperty = serializedObject.GetIterator();

            while (serializedProperty.NextVisible(true)) {
                if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference) {
                    if (serializedProperty.objectReferenceValue == null
                     && serializedProperty.objectReferenceInstanceIDValue != 0) {
                        ShowError(assetPath, ObjectNames.NicifyVariableName(serializedProperty.name));
                        missingCount++;
                    }
                }
            }

            return missingCount;
        }



        private static void showInitialProgressBar(string searchContext, bool clearConsole = true) {
            if (clearConsole) {
                Debug.ClearDeveloperConsole();
            }
            EditorUtility.DisplayProgressBar("Missing References Finder", $"Preparing search in {searchContext}", 0f);
        }

        private static void showFinishDialog(bool wasCancelled, int missingCount, int totalCount) {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Missing References Finder",
                                        wasCancelled ?
                                            $"Process cancelled.\n{missingCount} missing references were found.\nSearched {totalCount} assets.\n Current results are shown as errors in the console." :
                                            $"Finished finding missing references.\n{missingCount} missing references were found.\nSearched {totalCount} assets.\n Results are shown as errors in the console.",
                                        "Ok");
        }

        private const string err = "Missing Ref in: [{0}] Property: {1}";


        private static void ShowError(string objectPath, string property) {
            Debug.LogError(string.Format(err, objectPath, property));
        }

        private static string FullPath(GameObject go) {
            var parent = go.transform.parent;
            return parent == null ? go.name : FullPath(parent.gameObject) + "/" + go.name;
        }
    }
}

