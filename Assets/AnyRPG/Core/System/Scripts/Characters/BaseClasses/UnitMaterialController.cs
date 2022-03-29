using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {

    public class UnitMaterialController : ConfiguredClass {

        // unit controller of controlling unit
        private UnitController unitController;

        private float changeDuration = 2f;

        public Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        public Material[] temporaryMaterials = null;

        private Material temporaryMaterial = null;

        Renderer[] meshRenderers = null;

        public UnitMaterialController(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public void SetupMaterialArrays() {
            //Debug.Log(unitController.gameObject.name + ".UnitMaterialController.SetupMaterialArrays()");

            // reset original materials
            originalMaterials = new Dictionary<Renderer, Material[]>();

            meshRenderers = unitController.GetComponentsInChildren<MeshRenderer>();

            List<Renderer> tempList = new List<Renderer>();
            if (meshRenderers == null || meshRenderers.Length == 0) {
                //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew(): Unable to find mesh renderer in target.");
            } else {
                //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew(): Found " + meshRenderers.Length + " Mesh Renderers");
                foreach (Renderer renderer in meshRenderers) {
                    if (renderer.gameObject.layer != LayerMask.NameToLayer("SpellEffects")) {
                        tempList.Add(renderer);
                    }
                }
            }
            Renderer[] skinnedMeshRenderers = unitController.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0) {
                //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew(): Unable to find skinned mesh renderer in target.");
            } else {
                //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew(): Found " + skinnedMeshRenderers.Length + " Skinned Mesh Renderers");
                foreach (Renderer renderer in skinnedMeshRenderers) {
                    if (renderer.gameObject.layer != LayerMask.NameToLayer("SpellEffects")) {
                        tempList.Add(renderer);
                    }
                }
            }
            meshRenderers = tempList.ToArray();


            foreach (Renderer renderer in meshRenderers) {
                if (renderer.gameObject.layer == LayerMask.NameToLayer("SpellEffects")) {
                    continue;
                }
                originalMaterials.Add(renderer, renderer.materials);
            }
        }

        public void Initialize(float changeDuration, Material temporaryMaterial) {
            this.changeDuration = changeDuration;
            this.temporaryMaterial = temporaryMaterial;

            PerformMaterialChange();
        }

        public void ActivateStealth() {
            //Debug.Log(unitController.gameObject.name + ".UnitMaterialController.ActivateStealth()");

            if (meshRenderers == null) {
                //Debug.Log(gameObject.name + ".MaterialChangeController.PerformMaterialChange(): meshRender is null.  This shouldn't happen because we checked before instantiating this!");
                return;
            }
            foreach (Renderer renderer in meshRenderers) {
                //Debug.Log("MaterialChangeController.PerformMaterialChange(): material length: " + originalMaterials[renderer].Length);
                temporaryMaterials = new Material[originalMaterials[renderer].Length];
                //Debug.Log("MaterialChangeController.PerformMaterialChange(): temporary materials length: " + temporaryMaterials.Length);
                for (int i = 0; i < originalMaterials[renderer].Length; i++) {
                    //temporaryMaterials[i] = originalMaterials[renderer][i];
                    temporaryMaterials[i] = new Material(originalMaterials[renderer][i]);
                    //enable emission and set the emission texture to none in case this item already had some type of glow mask or effect
                    //Debug.Log("Interactable.Update(): flashingmaterial: " + temporaryMaterial.name + "; emission enabled? " + temporaryMaterial.IsKeywordEnabled("_EMISSION"));
                    //Debug.Log(gameObject.name + ".Interactable.PerformMaterialChange(): enabling emission");
                    ToFadeMode(temporaryMaterials[i]);
                    Color materialColor = temporaryMaterials[i].color;
                    //temporaryMaterials[i].color = new Color32(temporaryMaterials[i].color.r, temporaryMaterials[i].color.g, temporaryMaterials[i].color.b, 100);
                    materialColor.a = 0.25f;
                    temporaryMaterials[i].color = materialColor;
                }
                renderer.materials = temporaryMaterials;
            }
        }

        public void DeactivateStealth() {
            //Debug.Log(unitController.gameObject.name + ".UnitMaterialController.DeactivateStealth()");

            RevertMaterialChange();
        }

        public void ToFadeMode(Material material) {
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        public void PerformMaterialChange() {
            //Debug.Log(gameObject.name + ".MaterialChangeController.PerformMaterialChange()");

            if (meshRenderers == null) {
                //Debug.Log("MaterialChangeController.PerformMaterialChange(): meshRender is null.  This shouldn't happen because we checked before instantiating this!");
                return;
            }
            foreach (Renderer renderer in meshRenderers) {
                if (renderer.gameObject.layer == LayerMask.NameToLayer("SpellEffects")) {
                    continue;
                }
                originalMaterials.Add(renderer, renderer.materials);
                //Debug.Log("MaterialChangeController.PerformMaterialChange(): material length: " + originalMaterials[renderer].Length);
                temporaryMaterials = new Material[originalMaterials[renderer].Length];
                //Debug.Log("MaterialChangeController.PerformMaterialChange(): temporary materials length: " + temporaryMaterials.Length);
                for (int i = 0; i < originalMaterials[renderer].Length; i++) {
                    //temporaryMaterials[i] = originalMaterials[renderer][i];
                    temporaryMaterials[i] = temporaryMaterial;
                }
                renderer.materials = temporaryMaterials;
            }

            //Debug.Log(gameObject.name + ".MaterialChangeController.PerformMaterialChange(): Invoke RevertMaterialChange in duration: " + changeDuration);
            //Invoke("RevertMaterialChange", changeDuration);
        }

        public void OnDisable() {
            //Debug.Log(gameObject.name + ".MaterialChangeController.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
        }

        public void RevertMaterialChange() {
            //Debug.Log(gameObject.name + ".MaterialChangeController.RevertMaterialChange()");

            if (meshRenderers == null) {
                //Debug.Log("meshRender is null.  This shouldn't happen because we checked before instantiating this!");
                return;
            }

            foreach (Renderer renderer in meshRenderers) {
                if (renderer != null && renderer.gameObject.layer == LayerMask.NameToLayer("SpellEffects")) {
                    continue;
                }
                if (renderer != null && originalMaterials.ContainsKey(renderer)) {
                    renderer.materials = originalMaterials[renderer];
                }
            }

        }
    }

}