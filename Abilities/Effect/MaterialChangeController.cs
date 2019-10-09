using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MaterialChangeController : MonoBehaviour  {

    private float changeDuration = 2f;

    public Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

	public Material[] temporaryMaterials = null;

    private Material temporaryMaterial = null;

    Renderer[] meshRenderers = null;

    public void Initialize(float changeDuration, Material temporaryMaterial) {
        this.changeDuration = changeDuration;
        this.temporaryMaterial = temporaryMaterial;
        meshRenderers = GetComponentsInChildren<MeshRenderer>();

        if (meshRenderers == null || meshRenderers.Length == 0) {
            //Debug.Log("MaterialChangeController.Initialize(): Unable to find mesh renderer in target.");
            meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (meshRenderers == null || meshRenderers.Length == 0) {
                //Debug.Log("MaterialChangeController.Initialize(): Unable to find skinned mesh renderer in target.");
                return;
            } else {
                //Debug.Log("MaterialChangeController.Initialize(): Found " + meshRenderers.Length + " Skinned Mesh Renderers");
            }
        } else {
            //Debug.Log("MaterialChangeController.Initialize(): Found " + meshRenderers.Length + " Mesh Renderers");
        }

        PerformMaterialChange();
    }

    public void PerformMaterialChange() {
        //Debug.Log("MaterialChangeController.PerformMaterialChange()");

        if (meshRenderers == null) {
            //Debug.Log("MaterialChangeController.PerformMaterialChange(): meshRender is null.  This shouldn't happen because we checked before instantiating this!");
            return;
        }
        foreach (Renderer renderer in meshRenderers) {
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

        Invoke ("RevertMaterialChange", changeDuration);
	}

	public void RevertMaterialChange () {

		if (meshRenderers == null) {
            //Debug.Log("meshRender is null.  This shouldn't happen because we checked before instantiating this!");
            return;
        }

        foreach (Renderer renderer in meshRenderers) {
            renderer.materials = originalMaterials[renderer];
        }

        Destroy(this);
    }
}
