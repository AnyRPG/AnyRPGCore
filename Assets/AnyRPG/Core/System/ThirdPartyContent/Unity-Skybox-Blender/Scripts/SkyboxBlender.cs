using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AnyRPG {

    [ExecuteInEditMode]
    public class SkyboxBlender : MonoBehaviour {

        [SerializeField] public enum BlendMode { Linear, Smoothstep, Maximum, Add, Substract, Multiply }
        [SerializeField] public enum ProbeResolution { _16, _32, _64, _128, _256, _512, _1024, _2048 }

        //[Header("Input Skyboxes")]
        [SerializeField] public Material skyBox1;
        [SerializeField] public Material skyBox2;

        //[Header("Blended Skybox")]
        [SerializeField] public Material blendedSkybox;
        [SerializeField] [Range(0, 8)] public float exposure = 1;
        [SerializeField] [Range(0, 360)] public float rotation = 0;
        [SerializeField] public Color tint = Color.white;
        [SerializeField] [Range(0, 1)] public float invertColors = 0;
        [SerializeField] public BlendMode blendMode = BlendMode.Linear;
        [SerializeField] [Range(0, 1)] public float blend = 0;

        [SerializeField] public bool bindOnStart = true;
        [SerializeField] public bool updateLightingOnStart = true;
        [SerializeField] public bool updateLightingEveryFrame = true;
        [SerializeField] public bool updateReflectionsOnStart = true;
        [SerializeField] public bool updateReflectionsEveryFrame = true;

        [SerializeField] public ProbeResolution reflectionResolution = ProbeResolution._128;

        private ReflectionProbe probeComponent = null;
        private GameObject probeGameObject = null;
        private Cubemap blendedCubemap = null;
        private int renderId = -1;

        #region MonoBehaviour Functions

        // Use this for initialization
        void Start() {

            if (bindOnStart)
                BindTextures();

            //Update the material parameters
            UpdateBlendedMaterialParameters();

            if (updateLightingOnStart)
                UpdateLighting();

            if (updateReflectionsOnStart)
                UpdateReflections();
        }

        // Update is called once per frame
        void Update() {

            //Update material parameters
            UpdateBlendedMaterialParameters();

            //Update lighting
            if (updateLightingEveryFrame)
                UpdateLighting();

            //Update reflections
            if (updateReflectionsEveryFrame)
                UpdateReflections();
        }

        /*
        private void OnValidate()
        {
            if (!updateInEditMode)
                return;

            Update();
        }
        */

        #endregion

        /// <summary>
        /// Get the probe resolution value
        /// </summary>
        int GetProbeResolution(ProbeResolution probeResolution) {
            switch (probeResolution) {
                case ProbeResolution._16:
                    return 16;
                case ProbeResolution._32:
                    return 32;
                case ProbeResolution._64:
                    return 64;
                case ProbeResolution._128:
                    return 128;
                case ProbeResolution._256:
                    return 256;
                case ProbeResolution._512:
                    return 512;
                case ProbeResolution._1024:
                    return 1024;
                case ProbeResolution._2048:
                    return 2048;
                default:
                    return 128;
            }
        }

        /// <summary>
        /// Create a reflection probe gameobject and setup the cubemap for environment reflections
        /// </summary>
        void CreateReflectionProbe() {
            //Search for the reflection probe object
            probeGameObject = GameObject.Find("Skybox Blender Reflection Probe");

            if (!probeGameObject) {
                //Create the gameobject if its not here
                probeGameObject = new GameObject("Skybox Blender Reflection Probe");
                probeGameObject.transform.parent = gameObject.transform;
                // Use a location such that the new Reflection Probe will not interfere with other Reflection Probes in the scene.
                probeGameObject.transform.position = new Vector3(0, -1000, 0);
            }

            probeComponent = probeGameObject.GetComponent<ReflectionProbe>();

            if (probeComponent) {
                DestroyImmediate(probeComponent);
            }

            // Create a Reflection Probe that only contains the Skybox. The Update function controls the Reflection Probe refresh.
            probeComponent = probeGameObject.AddComponent<ReflectionProbe>() as ReflectionProbe;

        }

        /// <summary>
        /// Update the reflection probe and cubemap
        /// </summary>
        public void UpdateReflectionProbe() {
            //if (!probeGameObject || !probeComponent)
            CreateReflectionProbe();

            probeComponent.resolution = GetProbeResolution(reflectionResolution);
            probeComponent.size = new Vector3(1, 1, 1);
            probeComponent.cullingMask = 0;
            probeComponent.clearFlags = ReflectionProbeClearFlags.Skybox;
            probeComponent.mode = ReflectionProbeMode.Realtime;
            probeComponent.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            probeComponent.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;

            // A cubemap is used as a default specular reflection.
            blendedCubemap = new Cubemap(probeComponent.resolution, probeComponent.hdr ? TextureFormat.RGBAHalf : TextureFormat.RGBA32, true);

            //Set the render reflection mode to Custom
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflection = blendedCubemap;
        }

        /// <summary>
        /// Update the scene environment lighting
        /// </summary>
        public void UpdateLighting() {
            DynamicGI.UpdateEnvironment();
        }

        /// <summary>
        /// Update the scene environment reflections
        /// </summary>
        public void UpdateReflections() {
            if (!probeGameObject || !probeComponent)
                UpdateReflectionProbe();

            // The Update function refreshes the Reflection Probe and copies the result to the default specular reflection Cubemap.

            // The texture associated with the real-time Reflection Probe is a render target and RenderSettings.customReflection is a Cubemap. We have to check the support if copying from render targets to Textures is supported.
            if ((SystemInfo.copyTextureSupport & CopyTextureSupport.RTToTexture) != 0) {
                // Wait until previous RenderProbe is finished before we refresh the Reflection Probe again.
                // renderId is a token used to figure out when the refresh of a Reflection Probe is finished. The refresh of a Reflection Probe can take mutiple frames when time-slicing is used.
                if (renderId == -1 || probeComponent.IsFinishedRendering(renderId)) {
                    if (probeComponent.IsFinishedRendering(renderId)) {
                        //Debug.Log("probeComponent.texture.width = " + probeComponent.texture.width + " blendedCubemap.width = "+ blendedCubemap.width);
                        //Debug.Log("probeComponent.texture.height = " + probeComponent.texture.height + " blendedCubemap.height = " + blendedCubemap.height);
                        //Debug.Log("probeComponent.resolution = " + probeComponent.resolution);
                        // After the previous RenderProbe is finished, we copy the probe's texture to the cubemap and set it as a custom reflection in RenderSettings.
                        if (probeComponent.texture.width == blendedCubemap.width && probeComponent.texture.height == blendedCubemap.height) {
                            Graphics.CopyTexture(probeComponent.texture, blendedCubemap as Texture);
                            //Debug.Log("Copying");
                        }

                        RenderSettings.customReflection = blendedCubemap;
                    }

                    renderId = probeComponent.RenderProbe();
                }
            }
        }

        /// <summary>
        /// Get the BlendMode index from the enumeration
        /// </summary>
        int GetBlendModeIndex(BlendMode blendMode) {
            switch (blendMode) {
                case BlendMode.Linear:
                    return 0;
                case BlendMode.Smoothstep:
                    return 5;
                case BlendMode.Maximum:
                    return 1;
                case BlendMode.Add:
                    return 2;
                case BlendMode.Substract:
                    return 3;
                case BlendMode.Multiply:
                    return 4;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Bind the input skyboxes textures to the blended skybox
        /// </summary>
        public void BindTextures() {
            blendedSkybox.SetTexture("_FrontTex_1", skyBox1.GetTexture("_FrontTex"));
            blendedSkybox.SetTexture("_BackTex_1", skyBox1.GetTexture("_BackTex"));
            blendedSkybox.SetTexture("_LeftTex_1", skyBox1.GetTexture("_LeftTex"));
            blendedSkybox.SetTexture("_RightTex_1", skyBox1.GetTexture("_RightTex"));
            blendedSkybox.SetTexture("_UpTex_1", skyBox1.GetTexture("_UpTex"));
            blendedSkybox.SetTexture("_DownTex_1", skyBox1.GetTexture("_DownTex"));

            blendedSkybox.SetTexture("_FrontTex_2", skyBox2.GetTexture("_FrontTex"));
            blendedSkybox.SetTexture("_BackTex_2", skyBox2.GetTexture("_BackTex"));
            blendedSkybox.SetTexture("_LeftTex_2", skyBox2.GetTexture("_LeftTex"));
            blendedSkybox.SetTexture("_RightTex_2", skyBox2.GetTexture("_RightTex"));
            blendedSkybox.SetTexture("_UpTex_2", skyBox2.GetTexture("_UpTex"));
            blendedSkybox.SetTexture("_DownTex_2", skyBox2.GetTexture("_DownTex"));
        }

        /// <summary>
        /// Update the material parameters
        /// </summary>
        void UpdateBlendedMaterialParameters() {
            blendedSkybox.SetColor("_Tint", tint);
            blendedSkybox.SetFloat("_Exposure", exposure);
            blendedSkybox.SetFloat("_Rotation", rotation);
            blendedSkybox.SetFloat("_Blend", blend);
            blendedSkybox.SetInt("_BlendMode", GetBlendModeIndex(blendMode));
            blendedSkybox.SetFloat("_InvertColors", invertColors);

        }

    }

}