using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class SceneUtilityService : ConfiguredClass {

        // dictionary of scene file names to scene nodes for quick lookup at runtime
        private Dictionary<string, SceneNode> sceneDictionary = new Dictionary<string, SceneNode>();

        public Dictionary<string, SceneNode> SceneDictionary { get => sceneDictionary; set => sceneDictionary = value; }

        public void PerformSetupActivities() {
            // initialize the scene dictionary
            foreach (SceneNode sceneNode in systemDataFactory.GetResourceList<SceneNode>()) {
                if (sceneNode.SceneFile != null && sceneNode.SceneFile != string.Empty) {
                    //Debug.Log($"LevelManager.InitializeLevelManager(): adding scene {sceneNode.SceneFile} to scene dictionary from {sceneNode.ResourceName}");
                    sceneDictionary.Add(sceneNode.SceneFile, sceneNode);
                }
            }
        }

        public static Bounds GetSceneBounds() {
            Renderer[] renderers;
            TerrainCollider[] terrainColliders;
            Bounds sceneBounds = new Bounds();

            // only grab mesh renderers because skinned mesh renderers get strange angles when their bones are rotated
            renderers = GameObject.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            terrainColliders = GameObject.FindObjectsByType<TerrainCollider>(FindObjectsSortMode.None);

            // add bounds of renderers in case there are structures higher or lower than terrain bounds
            if (renderers.Length != 0) {
                for (int i = 0; i < renderers.Length; i++) {
                    if (renderers[i].enabled == true && renderers[i].gameObject.layer == LayerMask.NameToLayer("Default")) {
                        sceneBounds.Encapsulate(renderers[i].bounds);
                        //Debug.Log("MainMapController.SetSceneBounds(). Encapsulating gameobject: " + renderers[i].gameObject.name + " with bounds " + renderers[i].bounds);
                    }
                }
            }

            // add bounds of terrain colliders to get 'main' bounds
            if (terrainColliders.Length != 0) {
                for (int i = 0; i < terrainColliders.Length; i++) {
                    if (terrainColliders[i].enabled == true) {
                        sceneBounds.Encapsulate(terrainColliders[i].bounds);
                        //Debug.Log("MiniMapGeneratorController.GetSceneBounds(). Encapsulating terrain bounds: " + terrainColliders[i].bounds);
                    }
                }
            }

            return sceneBounds;
        }

        public static bool DoesSceneHaveNavMesh(Scene loadedScene) {
            // 1. Get all root objects in the specific scene to avoid cross-scene contamination
            GameObject[] rootObjects = loadedScene.GetRootGameObjects();

            foreach (GameObject root in rootObjects) {
                // 2. Search for the NavMeshSurface component in this hierarchy
                NavMeshSurface surface = root.GetComponentInChildren<NavMeshSurface>(true);

                // 3. Ensure the surface actually has a baked data asset assigned
                if (surface != null && surface.navMeshData != null) {
                    //Debug.Log($"[Server] NavMesh detected in scene: {loadedScene.name}");
                    return true;
                }
            }

            return false;
        }

        public SceneNode GetSceneNodeBySceneName(string sceneName) {
            if (sceneDictionary.ContainsKey(sceneName)) {
                return sceneDictionary[sceneName];
            }
            return null;
        }

        public SceneNode GetSceneNodeBySceneOrResourceName(string sceneName) {
            if (sceneDictionary.ContainsKey(sceneName)) {
                return sceneDictionary[sceneName];
            }
            return systemDataFactory.GetResource<SceneNode>(sceneName);
        }

    }

}