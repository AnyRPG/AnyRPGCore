using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SpeedTreeOrthographicLOD : MonoBehaviour {
    // Adjust this value to force a specific LOD level.
    // 0 = highest detail, 1 = billboard (lowest detail).
    [Range(0, 1)]
    public float forcedLOD = 0.0f;

    // This is the name of the shader property that controls SpeedTree LOD.
    // Different SpeedTree versions might use different property names.
    private const string lodProperty = "unity_LODFade";

    void OnPreRender() {
        // Set the global shader property before the camera renders.
        // The SpeedTree shader will pick this up for all instances.
        Shader.SetGlobalFloat(lodProperty, forcedLOD);
    }

    void OnPostRender() {
        // Restore the default LOD value to avoid affecting other cameras.
        // Setting it to -1 tells the shader to use its default calculation.
        Shader.SetGlobalFloat(lodProperty, -1.0f);
    }
}
