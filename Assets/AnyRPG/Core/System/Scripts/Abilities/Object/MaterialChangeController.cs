using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {

    // currently unused, replaced by UnitMaterialController
    public class MaterialChangeController : MonoBehaviour {

        private float changeDuration = 2f;

        public Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        public Material[] temporaryMaterials = null;

        private Material temporaryMaterial = null;

        Renderer[] meshRenderers = null;

        public void Initialize(float changeDuration, Material temporaryMaterial) {
            this.changeDuration = changeDuration;
            this.temporaryMaterial = temporaryMaterial;

            meshRenderers = GetComponentsInChildren<MeshRenderer>();

            List<Renderer> tempList = new List<Renderer>();
            if (meshRenderers == null || meshRenderers.Length == 0) {
                //Debug.Log($"{gameObject.name}.Interactable.InitializeMaterialsNew(): Unable to find mesh renderer in target.");
            } else {
                //Debug.Log($"{gameObject.name}.Interactable.InitializeMaterialsNew(): Found " + meshRenderers.Length + " Mesh Renderers");
                foreach (Renderer renderer in meshRenderers) {
                    if (renderer.gameObject.layer != LayerMask.NameToLayer("SpellEffects")) {
                        tempList.Add(renderer);
                    }
                }
            }
            Renderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0) {
                //Debug.Log($"{gameObject.name}.Interactable.InitializeMaterialsNew(): Unable to find skinned mesh renderer in target.");
            } else {
                //Debug.Log($"{gameObject.name}.Interactable.InitializeMaterialsNew(): Found " + skinnedMeshRenderers.Length + " Skinned Mesh Renderers");
                foreach (Renderer renderer in skinnedMeshRenderers) {
                    if (renderer.gameObject.layer != LayerMask.NameToLayer("SpellEffects")) {
                        tempList.Add(renderer);
                    }
                }
            }
            meshRenderers = tempList.ToArray();

            PerformMaterialChange();
        }

        public void PerformMaterialChange() {
            //Debug.Log($"{gameObject.name}.MaterialChangeController.PerformMaterialChange()");

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

            //Debug.Log($"{gameObject.name}.MaterialChangeController.PerformMaterialChange(): Invoke RevertMaterialChange in duration: " + changeDuration);
            Invoke("RevertMaterialChange", changeDuration);
        }

        public void RevertMaterialChange() {
            //Debug.Log($"{gameObject.name}.MaterialChangeController.RevertMaterialChange()");

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

            Destroy(this);
        }
    }

}