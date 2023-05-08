using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {

    public class UnitMaterialController : ObjectMaterialController {

        // unit controller of controlling unit
        private UnitController unitController = null;

        // global to prevent garbage collection every time stealth is activated
        public Material[] stealthMaterials = null;

        // global to prevent garbage collection every time non stealth material change is activated
        public Material[] temporaryMaterials = null;

        private Material temporaryMaterial = null;

        public UnitMaterialController(UnitController unitController, SystemGameManager systemGameManager) : base(unitController, systemGameManager) {
            this.unitController = unitController;
        }

        public void ActivateStealth() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMaterialController.ActivateStealth()");

            if (meshRenderers == null) {
                //Debug.Log($"{gameObject.name}.MaterialChangeController.PerformMaterialChange(): meshRender is null.  This shouldn't happen because we checked before instantiating this!");
                return;
            }
            foreach (Renderer renderer in meshRenderers) {
                stealthMaterials = new Material[originalMaterials[renderer].Length];
                for (int i = 0; i < originalMaterials[renderer].Length; i++) {
                    stealthMaterials[i] = new Material(originalMaterials[renderer][i]);
                    ToFadeMode(stealthMaterials[i]);
                    Color materialColor = stealthMaterials[i].color;
                    materialColor.a = 0.25f;
                    stealthMaterials[i].color = materialColor;
                }
                renderer.materials = stealthMaterials;
            }
        }

        public void DeactivateStealth() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMaterialController.DeactivateStealth()");

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

        public void ApplyTemporaryMaterialChange(Material temporaryMaterial) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMaterialController.PerformMaterialChange()");

            this.temporaryMaterial = temporaryMaterial;

            PerformTemporaryMaterialChange();
        }

        public void PerformTemporaryMaterialChange() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMaterialController.PerformTemporaryMaterialChange()");

            if (meshRenderers == null) {
                //Debug.Log("UnitMaterialController.PerformTemporaryMaterialChange(): meshRender is null.  This shouldn't happen because we checked before instantiating this!");
                return;
            }
            foreach (Renderer renderer in meshRenderers) {
                temporaryMaterials = new Material[originalMaterials[renderer].Length];
                for (int i = 0; i < originalMaterials[renderer].Length; i++) {
                    temporaryMaterials[i] = temporaryMaterial;
                }
                renderer.materials = temporaryMaterials;
            }

        }

        public void RevertTemporaryMaterialChange() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMaterialController.RevertMaterialChange()");

            temporaryMaterial = null;
            RevertMaterialChange();
        }

        private void RevertMaterialChange() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMaterialController.RevertMaterialChange()");

            if (meshRenderers == null) {
                //Debug.Log("meshRender is null.  This shouldn't happen because we checked before instantiating this!");
                return;
            }

            foreach (Renderer renderer in meshRenderers) {
                if (renderer != null && originalMaterials.ContainsKey(renderer)) {
                    renderer.materials = originalMaterials[renderer];
                }
            }

        }

    }

}