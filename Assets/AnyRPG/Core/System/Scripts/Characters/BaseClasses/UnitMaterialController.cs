using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
                    if (unitController.IsOwner == true) {
                        materialColor.a = 0.25f;
                    } else {
                        materialColor.a = 0f;
                    }
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
            // 1. Surface Type: 0 = Opaque, 1 = Transparent
            material.SetFloat("_Surface", 1f);

            // 2. Blend Mode: 0 = Alpha, 1 = Premultiply, 2 = Additive, 3 = Multiply
            material.SetFloat("_Blend", 0f);

            // 3. Manual override of blend factors & Z-write
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);

            // 4. URP Shader Keywords
            material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            material.DisableKeyword("_BLENDMODE_PREMULTIPLY");
            material.DisableKeyword("_BLENDMODE_ADD");
            material.DisableKeyword("_BLENDMODE_MULTIPLY");
            material.EnableKeyword("_BLENDMODE_ALPHA");

            // 5. Render Queue (optional—URP often handles this automatically)
            material.renderQueue = (int)RenderQueue.Transparent;
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

        public void ProcessSetModelReady() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMaterialController.ProcessSetModelReady()");

            if (unitController.IsStealth) {
                DeactivateStealth();
                PopulateOriginalMaterials();
                ActivateStealth();
            } else {
                PopulateOriginalMaterials();
            }
        }
    }

}