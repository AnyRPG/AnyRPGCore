using System;
using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class CharacterCreatorManager : PreviewManager {

        [Header("Skybox")]

        [Tooltip("The mesh renderer on the top quad.  Used for changing the skybox.")]
        [SerializeField]
        private MeshRenderer topRenderer = null;

        [Tooltip("The mesh renderer on the bottom quad.  Used for changing the skybox.")]
        [SerializeField]
        private MeshRenderer bottomRenderer = null;

        [Tooltip("The mesh renderer on the north quad.  Used for changing the skybox.")]
        [SerializeField]
        private MeshRenderer northRenderer = null;

        [Tooltip("The mesh renderer on the south quad.  Used for changing the skybox.")]
        [SerializeField]
        private MeshRenderer southRenderer = null;

        [Tooltip("The mesh renderer on the east quad.  Used for changing the skybox.")]
        [SerializeField]
        private MeshRenderer eastRenderer = null;

        [Tooltip("The mesh renderer on the west quad.  Used for changing the skybox.")]
        [SerializeField]
        private MeshRenderer westRenderer = null;

        [Header("Platform")]

        [Tooltip("The mesh renderer on the platform the unit stands on.  Used for changing the material.")]
        [SerializeField]
        private MeshRenderer platformRenderer = null;

        [Header("Light")]

        [Tooltip("The light source used in the preview.")]
        [SerializeField]
        private Light directionalLight = null;

        private GameObject environmentPreviewPrefab = null;
        private GameObject environmentPreviewPrefabRef = null;

        // game manager references
        ObjectPooler objectPooler = null;

        public MeshRenderer PlatformRenderer { get => platformRenderer; }
        public MeshRenderer TopRenderer { get => topRenderer; }
        public MeshRenderer BottomRenderer { get => bottomRenderer; }
        public MeshRenderer NorthRenderer { get => northRenderer; }
        public MeshRenderer SouthRenderer { get => southRenderer; }
        public MeshRenderer EastRenderer { get => eastRenderer; }
        public MeshRenderer WestRenderer { get => westRenderer; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            objectPooler = systemGameManager.ObjectPooler;
        }

        public void SpawnUnit(UnitProfile unitProfile) {
            Debug.Log("CharacterCreatorManager.SpawnUnit(" + (unitProfile == null ? "null" : unitProfile.DisplayName) + ")");

            if (unitProfile == null) {
                Debug.Log("CharacterCreatorManager.HandleOpenWindow(): unitProfile is null");
                return;
            }
            this.unitProfile = unitProfile;

            SpawnUnit();
        }

        public void SetPlatformMaterial(Material material) {
            platformRenderer.material = material;
        }

        public void SetSkybox(Material topMaterial, Material bottomMaterial, Material northMaterial, Material southMaterial, Material eastMaterial, Material westMaterial) {
            topRenderer.material = topMaterial;
            bottomRenderer.material = bottomMaterial;
            northRenderer.material = northMaterial;
            southRenderer.material = southMaterial;
            eastRenderer.material = eastMaterial;
            westRenderer.material = westMaterial;
        }

        public void DisableLight() {
            directionalLight.gameObject.SetActive(false);
        }

        public void EnableLight() {
            directionalLight.gameObject.SetActive(true);
        }

        public void SpawnEnvironmentPreviewPrefab(GameObject environmentPreviewPrefab) {
            //Debug.Log("CharacterCreatorManager.SpawnEnvironmentPreviewPrefab(" + (environmentPreviewPrefab == null ? "null" : environmentPreviewPrefab.name) + ")");

            if (environmentPreviewPrefab == this.environmentPreviewPrefab) {
                // there is no change to the preview prefab, do nothing
                return;
            }

            // remove the old preview prefab
            DespawnEnvironmentPreviewPrefab();

            if (environmentPreviewPrefab == null) {
                // no prefab, nothing to do
                return;
            }

            this.environmentPreviewPrefab = environmentPreviewPrefab;

            environmentPreviewPrefabRef = objectPooler.GetPooledObject(environmentPreviewPrefab, transform);
            SetLayerRecursive(environmentPreviewPrefabRef, LayerMask.NameToLayer("UnitPreview"));
        }

        public void DespawnEnvironmentPreviewPrefab() {
            if (environmentPreviewPrefabRef == null) {
                return;
            }

            objectPooler.ReturnObjectToPool(environmentPreviewPrefabRef);
            environmentPreviewPrefabRef = null;
        }

        private void SetLayerRecursive(GameObject objectName, int newLayer) {
            // set the preview unit layer to the PlayerPreview layer so the preview camera can see it and all other cameras will ignore it

            objectName.layer = newLayer;
            foreach (Transform childTransform in objectName.gameObject.GetComponentsInChildren<Transform>(true)) {
                if (childTransform.gameObject.layer != newLayer) {
                    childTransform.gameObject.layer = newLayer;
                }
            }
        }

    }
}