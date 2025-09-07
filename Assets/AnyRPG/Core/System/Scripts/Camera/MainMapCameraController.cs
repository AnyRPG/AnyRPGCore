using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainMapCameraController : MonoBehaviour {
    // The Terrain you want to control. Assign this in the Inspector.
    public Terrain targetTerrain;

    // The distance at which trees switch to billboards (less detailed).
    public float treeBillboardDistance = 50f;

    // A factor to scale the tree render distance.
    public float treeDrawDistanceScale = 1.0f;

    private float originalTreeBillboardDistance;
    private float originalTreeDrawDistance;

    void OnPreRender() {
        if (targetTerrain != null) {
            // Store original values to restore them later
            originalTreeBillboardDistance = targetTerrain.treeBillboardDistance;
            originalTreeDrawDistance = targetTerrain.treeDistance;

            // Adjust the tree drawing distance based on the camera's orthographic size.
            // This links tree detail to the "zoom" level of your ortho camera.
            float orthoSize = GetComponent<Camera>().orthographicSize;
            targetTerrain.treeDistance = orthoSize * treeDrawDistanceScale;

            // Set the billboard distance to force trees to appear more detailed.
            // A higher number means full-detail trees are visible further away.
            targetTerrain.treeBillboardDistance = treeBillboardDistance;
        }
    }

    void OnPostRender() {
        if (targetTerrain != null) {
            // Restore the original values to avoid affecting other cameras.
            targetTerrain.treeBillboardDistance = originalTreeBillboardDistance;
            targetTerrain.treeDistance = originalTreeDrawDistance;
        }
    }
}
