using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {

    public class ObjectMaterialController : ConfiguredClass {

        // interactable of owning object
        protected Interactable interactable = null;

        protected Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        protected Renderer[] meshRenderers = null;

        public Renderer[] MeshRenderers { get => meshRenderers; }

        public ObjectMaterialController(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        public void PopulateOriginalMaterials() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMaterialController.SetupMaterialArrays()");

            // reset original materials
            originalMaterials = new Dictionary<Renderer, Material[]>();

            meshRenderers = interactable.GetComponentsInChildren<MeshRenderer>();

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
            Renderer[] skinnedMeshRenderers = interactable.GetComponentsInChildren<SkinnedMeshRenderer>();
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

            foreach (Renderer renderer in meshRenderers) {
                originalMaterials.Add(renderer, renderer.materials);
            }
        }

    }

}